﻿using Microsoft.Vbe.Interop;
using PowerVBA.Core.Interface;
using Microsoft.Office.Interop.PowerPoint;
using Microsoft.Office.Core;
using System.Collections;
using PowerVBA.Core.Wrap;
using PowerVBA.Core.Wrap.WrapBase;
using PowerVBA.Core.Connector;
using System;

namespace PowerVBA.V2010.WrapClass

{
    [Wrapped(typeof(_VBComponent))]
    public class VBComponentWrapping : VBComponentWrappingBase
    {
        public VBComponent VBComponent { get; }
        public VBComponentWrapping(VBComponent VBComponent)
        {
            this.VBComponent = VBComponent;
        }

        public override PPTVersion ClassVersion => PPTVersion.PPT2010;

        public void Export(string FileName)
        {
            VBComponent.Export(FileName);
        }

        public Window DesignerWindow()
        {
            return VBComponent.DesignerWindow();
        }

        public void Activate()
        {
            VBComponent.Activate();
        }

        public bool Saved => VBComponent.Saved;
        public string Name { set { VBComponent.Name = value; } get { return VBComponent.Name; } }
        public dynamic Designer => VBComponent.Designer;
        public CodeModule CodeModule => VBComponent.CodeModule;
        public vbext_ComponentType Type => VBComponent.Type;
        public VBE VBE => VBComponent.VBE;
        public VBComponents Collection => VBComponent.Collection;
        public bool HasOpenDesigner => VBComponent.HasOpenDesigner;
        public Properties Properties => VBComponent.Properties;
        public string DesignerID => VBComponent.DesignerID;

        public override string CompName => Name;
    }
}
