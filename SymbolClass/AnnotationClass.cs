using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

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
        public bool IsSingleProperty {
            get;
            set;
        }
        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// 标注文本符号
        /// </summary>
        public TextSymbolClass TextSymbol { get; set; }
        /// <summary>
        /// 获取标注XML节点，SLD的注记方式和ArcMap的标注方式不一样，这里只解析了最基本的要素，高级标注要素未进行解析
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        public XmlElement GetSymbolNode(XmlDocument xmlDoc)
        {
            XmlElement pAnnotaElment = null;
            if (this.IsSingleProperty && !string.IsNullOrEmpty(this.PropertyName))
            {
                return pAnnotaElment;
            }
            else
            {
                //创建TextSymbolizer节点
                pAnnotaElment = CommXmlHandle.CreateElement("TextSymbolizer", xmlDoc);
                //写标注字段信息
                XmlElement pLableElment = CommXmlHandle.CreateElementAndSetElemnetText("TextLabel", xmlDoc, PropertyName);
                pAnnotaElment.AppendChild(pLableElment);
                //写字体信息
                pAnnotaElment.AppendChild(TextSymbol.GetSymbolNode(xmlDoc)[0]);
                //写填充颜色
                XmlElement pTextFillElment = CommXmlHandle.CreateElement("TextFill", xmlDoc);
                XmlElement pFillElment = CommXmlHandle.CreateElementAndSetElemnetText("TextFillCssParameter", xmlDoc, TextSymbol.Color);
                CommXmlHandle.SetAttributeValue("fill", CommXmlHandle.CreateAttribute("name", pFillElment, xmlDoc));
                pTextFillElment.AppendChild(pFillElment);
                pAnnotaElment.AppendChild(pTextFillElment);
            }
            return pAnnotaElment;
        }
    }
}
