using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGIS_SLD_Converter
{
    //注记
    public class AnnotationClass
    {
        public AnnotationClass(IFeatureLayer pFeatureLayer)
        {
            if (pFeatureLayer != null)
            {
                if (pFeatureLayer is IGeoFeatureLayer)
                {
                    IGeoFeatureLayer objGFL = pFeatureLayer as IGeoFeatureLayer;
                    IAnnotateLayerPropertiesCollection annoPropsColl = objGFL.AnnotationProperties;
                    if (objGFL.DisplayAnnotation && annoPropsColl.Count > 0)
                    {
                        IAnnotateLayerProperties annoLayerProps = null;
                        ESRI.ArcGIS.Carto.IElementCollection null_ESRIArcGISCartoIElementCollection = null;
                        ESRI.ArcGIS.Carto.IElementCollection null_ESRIArcGISCartoIElementCollection2 = null;
                        annoPropsColl.QueryItem(0, out annoLayerProps, out null_ESRIArcGISCartoIElementCollection, out null_ESRIArcGISCartoIElementCollection2);
                        if (annoLayerProps is ILabelEngineLayerProperties && annoLayerProps.DisplayAnnotation)
                        {
                            ILabelEngineLayerProperties labelProps = annoLayerProps as ILabelEngineLayerProperties;
                            if (annoLayerProps.WhereClause == "" && labelProps.IsExpressionSimple)
                            {
                                IsSingleProperty = true;
                                PropertyName = labelProps.Expression.Replace("[", "").Replace("]", "");
                                TextSymbol = new TextSymbolClass(labelProps.Symbol as ISymbol);
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 是否是单个属性
        /// </summary>
        public bool IsSingleProperty { get; set; }
        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// 标注文本符号
        /// </summary>
        public TextSymbolClass TextSymbol { get; set; }
    }
}
