using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGIS_SLD_Converter
{
    public class ProjectClass
    {
        public ProjectClass()
        {
            this.m_LayerRender = new Dictionary<string, ptLayer>();
        }
        public Dictionary<string, ptLayer> m_LayerRender { get; set; }
    }
}
