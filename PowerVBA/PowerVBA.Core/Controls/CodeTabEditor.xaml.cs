﻿using PowerVBA.Core.AvalonEdit;
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

namespace PowerVBA.Core.Controls
{
    /// <summary>
    /// CodeTabEditor.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CodeTabEditor : UserControl
    {
        public CodeTabEditor()
        {
            InitializeComponent();
        }

        public CodeEditor CodeEditor { get => VBAEditor; set => VBAEditor = value; }
    }
}