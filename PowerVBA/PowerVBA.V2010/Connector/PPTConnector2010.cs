﻿using PowerVBA.Core.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerVBA.Core.Interface;
using PowerVBA.Core.Wrap.WrapBase;

namespace PowerVBA.V2010.Connector
{
    class PPTConnector2010 : PPTConnectorBase
    {
        public PPTConnector2010()
        {

        }

        public override PPTVersion Version => PPTVersion.PPT2010;

        public override bool AddClass(string name)
        {
            throw new NotImplementedException();
        }

        public override bool AddForm(string name)
        {
            throw new NotImplementedException();
        }

        public override bool AddModule(string name)
        {
            throw new NotImplementedException();
        }

        public override bool AddSlide()
        {
            throw new NotImplementedException();
        }

        public override bool AddSlide(int SlideNumber)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteClass(string name)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteForm(string name)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteModule(string name)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteSlide()
        {
            throw new NotImplementedException();
        }

        public override bool DeleteSlide(int SlideNumber)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override List<ShapeWrappingBase> Shapes()
        {
            throw new NotImplementedException();
        }
    }
}