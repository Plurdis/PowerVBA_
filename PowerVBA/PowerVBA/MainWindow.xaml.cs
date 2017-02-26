﻿using ICSharpCode.AvalonEdit.Folding;
using PowerVBA.Core.CodeEdit.Folding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using PowerVBA.Core.CodeEdit.Renderer;
using ICSharpCode.AvalonEdit.Document;
using PowerVBA.Core.CodeEdit.Substitution;
using PowerVBA.Core.CodeEdit.Substitution.Base;
using System.Text.RegularExpressions;
using static PowerVBA.Global.Globals;
using PowerVBA.Core.Extension;
using PowerVBA.Core.CodeEdit.Indentation;
using PowerVBA.Global.Regex;
using PowerVBA.Core.Error;
using PowerVBA.Control.Customize;
using PowerVBA.Core.CodeEdit.Parser;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Editing;

namespace PowerVBA
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {

        #region [  전역 변수  ]

        /// <summary>
        /// 기존 코드를 치환해주는 클래스들의 목록입니다.
        /// </summary>
        List<BaseSubstitution> Substitutions = new List<BaseSubstitution>();

        FoldingManager foldingManager;
        VBAFoldingStrategy foldingStrategy;

        Thread thr;

        Stopwatch sw = new Stopwatch();

        CodeIndentation codeIndentation;

        List<CodeError> CodeErrors = new List<CodeError>();

        ErrorToolTip errToolTip;

        double LineHeight;

        #endregion

        

        #region [  초 기 화  ]


        public MainWindow()
        {
            InitializeComponent();
            

            #region [  코드 폴딩  ]

            foldingManager = FoldingManager.Install(codeEditor.TextArea);
            foldingStrategy = new VBAFoldingStrategy();

            thr = new Thread(() => {
                do
                {
                    if (sw.ElapsedMilliseconds > 500)
                    {
                        sw.Reset();
                        Dispatcher.Invoke(new Action(() => { foldingStrategy.UpdateFoldings(foldingManager, codeEditor.Document);
                        }), System.Windows.Threading.DispatcherPriority.Background);   
                    }
                    Thread.Sleep(10);
                } while (true);
            });

            thr.Start();

            codeEditor.TextChanged += delegate (object sender, EventArgs e)
            {
                sw.Restart();
            };

            #endregion

            #region [  하이라이팅, 키워드 연결  ]

            using (Stream s = new MemoryStream(PowerVBA.Properties.Resources.VBA_Highlight))
            {
                using (XmlTextReader reader = new XmlTextReader(s))
                {
                    highlightingDefintion = HighlightingLoader.Load(reader, HighlightingManager.Instance);

                    codeEditor.SyntaxHighlighting = highlightingDefintion;
                }
            }

            // TODO : 위치 변경
            Type[] typelist = GetTypesInNamespace(Assembly.Load(PowerVBA.Properties.Resources.LibPowerPoint), "Microsoft.Office.Interop.PowerPoint");

            foreach (var t in typelist)
                if (t.IsInterface || t.IsEnum)
                {
                    codeEditor.SyntaxHighlighting.MainRuleSet.Rules[1].Add(t.Name);
                }


            #endregion

            #region [  BackgroundRenderer 추가  ]

            codeEditor.TextArea.TextView.BackgroundRenderers.Add(new CurrentLineBackgroundRenderer(codeEditor));
            codeEditor.TextArea.TextView.BackgroundRenderers.Add(new ErrorLineBackgroundRenderer(codeEditor, CodeErrors));

            #endregion

            #region [  Substitution 초기화  ]


            MethodSubstitution methodSubstitution = new MethodSubstitution(codeEditor);
            VariableSubstitution variableSubstitution = new VariableSubstitution(codeEditor);


            Substitutions.Add(methodSubstitution);
            Substitutions.Add(variableSubstitution);

            #endregion

            #region [  Indentation 초기화  ]

            codeIndentation = new CodeIndentation(codeEditor);

            #endregion

            #region [  CodeParser 초기화  ]
            CodeParser codeParser = new CodeParser(codeEditor, CodeErrors);
            #endregion

            #region [  이벤트 핸들러 연결  ]

            // MainWindow
            this.Closing += WindowClosingEvent;
            


            // codeEditor
            codeEditor.TextArea.Caret.PositionChanged += (sender, e) => codeEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
            codeEditor.PreviewKeyDown += prevKeyDown;

            codeEditor.TextArea.SelectionChanged += codeSelChanged;
            codeEditor.TextArea.Caret.PositionChanged += codeCaretChanged;
            codeEditor.TextArea.TextEntering += textEditor_TextArea_TextEntering;
            codeEditor.TextArea.TextEntered += textEditor_TextArea_TextEntered;


            codeEditor.MouseMove += codeMouseMove;
            codeEditor.MouseLeave += codeMouseLeave;
            

            #endregion
            
            #region [  TextEditor 옵션/설정 변경  ]

            codeEditor.Options.InheritWordWrapIndentation = false;
            errToolTip = new ErrorToolTip();

            LineHeight = codeEditor.TextArea.TextView.DefaultLineHeight;

            #endregion


            CodeErrors.Add(new CodeError(1, ErrorType.Error, "string 값을 할당할 수 없습니다."));
            CodeErrors.Add(new CodeError(2, ErrorType.Error, "Dim A As String과 같은 형태는 사용할 수 없습니다."));
        }
        
        #endregion



        #region [  Avalon Text Editor 이벤트  ]
        int lastindex = 0;
        private void codeMouseMove(object sender, MouseEventArgs e)
        {
            bool Handled = false;

            for (int ctr = 1; ctr <= codeEditor.LineCount; ctr++){
                double lineTop = codeEditor.TextArea.TextView.GetVisualTopByDocumentLine(ctr);
                double MouseY = Mouse.GetPosition(codeEditor).Y;
                
                if (MouseY >= lineTop && MouseY < LineHeight + lineTop)
                {
                    string d = string.Join("\r\n", CodeErrors.Where((err) => err.LineNumber == ctr)
                                                             .Select((err) => err.LineNumber + " : " + err.ErrorMessage));

                    int count = CodeErrors.Where((err) => err.LineNumber == ctr).Count();

                    // 현재 라인에 오류가 있을 경우
                    if (!string.IsNullOrEmpty(d))
                    {
                        
                        if (lastindex != ctr)
                        {
                            errToolTip.IsOpen = false;
                            errToolTip.IsOpen = true;

                            errToolTip.Title = ctr + "번째 줄에서 발생한 " + count + "개의 오류";
                            errToolTip.Text = d;
                        }
                    }
                    else
                    {
                        errToolTip.IsOpen = false;
                        
                    }
                    Handled = true;
                    lastindex = ctr;
                    break;
                }
            }
            
            if (!Handled)
            {
                {
                    errToolTip.IsOpen = false;
                    lastindex = -1;
                }
            }

            this.Title = DateTime.Now.ToString() + " :: " + Mouse.GetPosition(codeEditor).Y + " :: " + lastindex;
        }


        private void codeMouseLeave(object sender, MouseEventArgs e)
        {
            errToolTip.IsOpen = false;
            lastindex = -1;
        }


        public class MyCompletionData : ICompletionData
        {
            public MyCompletionData(string d)
            {
                text = d;
            }
            public object Content
            {
                get
                {
                    return "AA";
                }
            }

            public object Description
            {
                get
                {
                    return "ABC";
                }
            }

            public ImageSource Image
            {
                get
                {
                    return null;
                }
            }

            public double Priority
            {
                get
                {
                    return 1;
                }
            }

            public string text { get; set; }
            public string Text
            {
                get
                {
                    return text;
                }
            }

            public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
            {
                textArea.Document.Replace(completionSegment, this.text);
            }
        }

        CompletionWindow completionWindow;

        void textEditor_TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == ".")
            {
                // Open code completion after the user has pressed dot:
                completionWindow = new CompletionWindow(codeEditor.TextArea);
                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                data.Add(new MyCompletionData("Item1"));
                data.Add(new MyCompletionData("Item2"));
                data.Add(new MyCompletionData("Item3"));
                completionWindow.Show();
                completionWindow.Closed += delegate {
                    completionWindow = null;
                };
            }
        }

        void textEditor_TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }




        internal string GetIndentation(int Indentation)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Indentation; i++)
            {
                sb.Append("\t");
            }

            return sb.ToString();
        }



        private void codeSelChanged(object sender, EventArgs e)
        {
            // 텍스트 선택이 바뀌었을때 발생 (단순히 커서를 움직이는 것 만으로는 발생하지 않음)
        }
        
        DocumentLine LastLine = null;
        private void codeCaretChanged(object sender, EventArgs e)
        {
            DocumentLine line = codeEditor.Document.GetLineByOffset(codeEditor.CaretOffset);

            if (LastLine != line)
            {
                //codeIndentation.Indent();
                // 선택된 라인이 바뀌었을때 (예 : 선택된 라인이 4였다가 5나 7로 바뀌었을때)
                LastLine = line;
            }
        }

        

        private void prevKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                bool Handled = false;

                foreach(BaseSubstitution substitution in Substitutions)
                {
                    substitution.Handled = false;
                    substitution.Convert();

                    if (substitution.Handled)
                    {
                        Handled = true;
                        break;
                    }
                }
                e.Handled = Handled;
            }
        }
        #endregion


        #region [  윈도우 이벤트  ]
        private void WindowClosingEvent(object sender, System.ComponentModel.CancelEventArgs e)
        {
            thr.Abort();
        }
        #endregion

    }
}
