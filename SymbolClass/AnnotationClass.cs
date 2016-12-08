using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGIS_SLD_Converter
{
    //注记
    public class AnnotationClass
    {
        public AnnotationClass()
        {

        }
        public bool IsSingleProperty { get; set; }
        public string PropertyName { get; set; }
        public TextSymbolClass TextSymbol { get; set; }
    }
}
