using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGIS_SLD_Converter
{
    /// <summary>
    /// 注记类
    /// </summary>
    public class Annotation
    {
        /// <summary>
        /// 文本符号
        /// </summary>
        public TextSymbolClass m_TextSymbol
        {
            get; set;

        }
        /// <summary>
        /// 是否是单个属性
        /// </summary>
        public bool m_IsSingleProperty
        {
            get; set;
        }
        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropertyName
        {
            get; set;
        }
    }
}
