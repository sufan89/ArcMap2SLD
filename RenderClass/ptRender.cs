using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGIS_SLD_Converter
{
    /// <summary>
    /// 渲染方式
    /// </summary>
    public class ptRender
    {
        public ptRender()
        {

        }
        /// <summary>
        /// 渲染的要素类
        /// </summary>
        public IFeatureClass m_FeatureClass{get; set;}
        /// <summary>
        /// 渲染图层名称
        /// </summary>
        public string m_LayerName{get; set;}
        /// <summary>
        /// 数据集名称
        /// </summary>
        public string m_DatasetName{get; set;}
        /// <summary>
        /// 标记
        /// </summary>
        public AnnotationClass AnnotationClass { get; set; }
        /// <summary>
        /// 符号列表
        /// </summary>
        public IList<ptSymbolClass> SymbolList { get; set; }
    }
    /// <summary>
    /// 唯一值渲染
    /// </summary>
    public class UniqueValueRendererClass : ptRender
    {
        public UniqueValueRendererClass() : base()
        {
        }
        /// <summary>
        /// 唯一值数量
        /// </summary>
        public int ValueCount { get; set;}
        /// <summary>
        /// 字段数量
        /// </summary>
        public int FieldCount { get; set; }
        public IList<string> FieldNames { get; set; }
        public string StylePath { get; set; }
    }
    /// <summary>
    /// 分类渲染
    /// </summary>
    public class ClassBreaksRendererCalss : ptRender
    {
        public ClassBreaksRendererCalss() : base()
        {
        }
        public int BreakCount { get; set; }
        public string FieldName { get; set; }
        public string NormFieldName { get; set; }
    }
    /// <summary>
    /// 简单渲染方式
    /// </summary>
    public class SimpleRendererClass : ptRender
    {
        public SimpleRendererClass() : base()
        {

        }
    }
}
