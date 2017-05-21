﻿using PowerVBA.Controls.Customize;
using PowerVBA.Controls.Tools;
using PowerVBA.Core.AvalonEdit;
using PowerVBA.Core.Connector;
using PowerVBA.V2010.Connector;
using PowerVBA.V2013.Connector;
using PowerVBA.Windows;
using PowerVBA.Windows.AddWindows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PowerVBA
{
    partial class MainWindow
    {
        #region [  홈 탭 이벤트  ]

        #region [  클립보드  ]
        private void BtnCopy_SimpleButtonClicked(object sender)
        {
            Clipboard.Clear();
            Clipboard.SetText(((CodeEditor)codeTabControl.SelectedContent).SelectedText);
        }
        private void BtnPaste_SimpleButtonClicked(object sender)
        {
            if (Clipboard.ContainsText())
            {
                string t = Clipboard.GetText();
                CodeEditor editor = ((CodeEditor)codeTabControl.SelectedContent);

                if (editor.SelectionLength != 0) editor.SelectedText = t;
                else editor.TextArea.Document.Insert(editor.CaretOffset, t);
            }
        }
        #endregion

        #region [  작업  ]
        private void BtnUndo_SimpleButtonClicked(object sender)
        {
            CodeEditor editor = ((CodeEditor)codeTabControl.SelectedItem);
            if (editor == null) return;
            if (editor.CanUndo) editor.Undo();
            btnUndo.IsEnabled = editor.CanUndo;
            btnRedo.IsEnabled = editor.CanRedo;
            editor.Focus();
        }

        private void BtnRedo_SimpleButtonClicked(object sender)
        {
            CodeEditor editor = ((CodeEditor)codeTabControl.SelectedItem);
            if (editor == null) return;
            if (editor.CanRedo) editor.Redo();
            btnUndo.IsEnabled = editor.CanUndo;
            btnRedo.IsEnabled = editor.CanRedo;
            editor.Focus();
        }

        #endregion

        #region [  슬라이드 관리  ]
        private void BtnNewSlide_SimpleButtonClicked(object sender)
        {
            int slideNumber = connector.Slide;

            connector.AddSlide(slideNumber + 1);

            SetMessage((slideNumber + 1) + "번째 슬라이드를 추가했습니다.");

        }

        private void BtnDelSlide_SimpleButtonClicked(object sender)
        {
            int SlideNumber = 0;
            if (connector.SlideCount == 0)
            {
                SetMessage("삭제할 슬라이드가 없습니다.");

                return;
            }

            SlideNumber = connector.Slide;

            if (MessageBox.Show(SlideNumber + "슬라이드를 삭제하시겠습니까?", "슬라이드 삭제 확인", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                connector.DeleteSlide();
                SetMessage((SlideNumber) + "번째 슬라이드를 삭제했습니다.");
            }
        }

        #endregion


        private void BtnHelp_SimpleButtonClicked(object sender)
        {
            HelperWindow hWdw = new HelperWindow();
            hWdw.ShowDialog();
        }

        #endregion


        #region [  삽입 탭 이벤트  ]

        private void BtnAddClass_SimpleButtonClicked(object sender)
        {
            AddFileWindow filewindow = new AddFileWindow(connector, AddFileWindow.AddFileType.Class);

            SolutionExplorer_Open(this, filewindow.ShowDialog());
        }

        private void BtnAddModule_SimpleButtonClicked(object sender)
        {
            AddFileWindow filewindow = new AddFileWindow(connector, AddFileWindow.AddFileType.Module);

            SolutionExplorer_Open(this, filewindow.ShowDialog());
        }

        private void BtnAddForm_SimpleButtonClicked(object sender)
        {
            AddFileWindow filewindow = new AddFileWindow(connector, AddFileWindow.AddFileType.Form);

            SolutionExplorer_Open(this, filewindow.ShowDialog());
        }


        private void BtnAddSub_SimpleButtonClicked(object sender)
        {
            var procWindow =
                new AddProcedureWindow(GetCodeTab(),
                                       ((TabItem)codeTabControl.SelectedItem).Header.ToString(),
                                       codeInfo, AddProcedureWindow.AddProcedureType.Sub);

            procWindow.ShowDialog();
        }

        private void BtnAddFunc_SimpleButtonClicked(object sender)
        {
            new AddProcedureWindow(GetCodeTab(), GetCodeTabName(),
                                   codeInfo, AddProcedureWindow.AddProcedureType.Function).ShowDialog();
        }
        private void BtnAddProp_SimpleButtonClicked(object sender)
        {
            new AddProcedureWindow(GetCodeTab(), GetCodeTabName(),
                                   codeInfo, AddProcedureWindow.AddProcedureType.Property).ShowDialog();

        }

        private void BtnAddMouseOverTrigger_SimpleButtonClicked(object sender)
        {
            new AddTriggerWindow(true, GetCodeTab(), codeInfo, GetCodeTabName()).ShowDialog(connector);
        }

        private void BtnAddMouseClickTrigger_SimpleButtonClicked(object sender)
        {
            new AddTriggerWindow(false, GetCodeTab(), codeInfo, GetCodeTabName()).ShowDialog(connector);
        }

        public string GetCodeTabName()
        {
            return ((TabItem)codeTabControl.SelectedItem).Header.ToString();
        }


        private void BtnAddVar_SimpleButtonClicked(object sender)
        {
            new AddVarWindow(GetCodeTab(), ((TabItem)codeTabControl.SelectedItem).Header.ToString(),
                             codeInfo, true).ShowDialog();
        }

        private void BtnAddConst_SimpleButtonClicked(object sender)
        {
            new AddVarWindow(GetCodeTab(), ((TabItem)codeTabControl.SelectedItem).Header.ToString(),
                             codeInfo, false).ShowDialog();
        }
        #endregion

        #region [  프로젝트 탭  ]

        private void PreDeclareFuncBtn_SimpleButtonClicked(object sender)
        {
            var tabItems = codeTabControl.Items.Cast<CloseableTabItem>().Where((i) => i.Header.ToString() == "미리 정의된 함수").ToList();
            if (tabItems.Count >= 1)
            {
                codeTabControl.SelectedItem = tabItems.First();
            }
            else
            {
                CloseableTabItem itm = new CloseableTabItem()
                {
                    Header = "미리 정의된 함수",
                    Content = new PreDeclareFuncManager() { Connector = connector }
                };
                codeTabControl.Items.Add(itm);
                codeTabControl.SelectedItem = itm;
            }
        }

        private void CheckError_SimpleButtonClicked(object sender)
        {
            var result = MessageBox.Show("코드 분석을 시작합니다.\r\n코드 분석은 현재 프로젝트에 있는 파일 모두를 분석해 오류를 확인합니다.\r\n" +
                            "저장되지 않은 내용은 검사되지 않으며 문법적 검사만 실행합니다.\r\n계속하시겠습니까?", "코드 분석 확인", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                var itm = connector.GetFiles();

                if (itm.Count != 0)
                {
                    ErrorWindow errWdw = new ErrorWindow(itm);

                    errWdw.ShowDialog();
                }
                else
                {
                    MessageBox.Show("파일이 없습니다!");
                }
            }
        }

        private void BtnFileSync_SimpleButtonClicked(object sender)
        {
            var itm = GetCodeTab();
            if (itm != null) itm.Save();
            SetMessage("저장되었습니다.");
        }

        private void BtnAllFileSync_SimpleButtonClicked(object sender)
        {
            var itm = GetAllCodeEditors();
            itm.ForEach(editor => editor.Save());
            SetMessage("전체 저장되었습니다.");
        }
        #endregion
    }
}