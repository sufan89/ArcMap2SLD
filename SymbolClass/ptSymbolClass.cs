using ESRI.ArcGIS.Display;
using stdole;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;

namespace ArcGIS_SLD_Converter
{
    public class ptSymbolClass
    {
        public ptSymbolClass(ISymbol pSymbol)
        {

        }
        /// <summary>
        /// 标记
        /// </summary>
        public string Label { get; set; }
        /// <summary>
        /// 字段值
        /// </summary>
        public IList<string> Fieldvalues { get; set; }
        /// <summary>
        /// 最大限制
        /// </summary>
        public double UpperLimit { get; set; }
        /// <summary>
        /// 最小限制
        /// </summary>
        public double LowerLimit { get; set; }
        public virtual IList<XmlElement> GetSymbolNode(XmlDocument xmlDoc)
        {
            return null;
        }
    }
    /// <summary>
    /// 文本符号
    /// </summary>
    public class TextSymbolClass : ptSymbolClass
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="symbol"></param>
        public TextSymbolClass(ISymbol symbol):
            base(symbol)
        {
            if (symbol != null)
            {
                ITextSymbol pTextSymbol = symbol as ITextSymbol;
                Angle = pTextSymbol.Angle;
                Color = CommStaticClass.GimmeStringForColor(pTextSymbol.Color);
                Font = pTextSymbol.Font.Name;
                Style = "normal";
                if (pTextSymbol.Font.Italic)
                {
                    Style = "italic";
                }
                Weight = "normal";
                if (pTextSymbol.Font.Bold)
                {
                    Weight = "bold";
                }
                HorizontalAlignment = pTextSymbol.HorizontalAlignment.ToString();
                RightToLeft = pTextSymbol.RightToLeft;
                Size = pTextSymbol.Size;
                Text = pTextSymbol.Text;
                VerticalAlignment = pTextSymbol.VerticalAlignment.ToString();
            }
        }
        /// <summary>
        /// 角度
        /// </summary>
        public double Angle { get;  }
        /// <summary>
        /// 颜色
        /// </summary>
        public string Color { get; }
        /// <summary>
        /// 字体
        /// </summary>
        public string Font { get; }
        /// <summary>
        /// 样式
        /// </summary>
        public string Style { get; }
        /// <summary>
        /// 粗细
        /// </summary>
        public string Weight { get;}
        /// <summary>
        /// 水平对齐
        /// </summary>
        public string HorizontalAlignment { get;}
        /// <summary>
        /// 是否对齐
        /// </summary>
        public bool RightToLeft { get; }
        /// <summary>
        ///大小 
        /// </summary>
        public double Size { get; }
        /// <summary>
        /// 文本
        /// </summary>
        public string Text { get;}
        /// <summary>
        /// 垂直对齐
        /// </summary>
        public string VerticalAlignment { get;}
    }

    #region 点符号
    /// <summary>
    /// 点符号
    /// </summary>
    public class ptMarkerSymbolClass : ptSymbolClass
    {
        public ptMarkerSymbolClass(ISymbol pSymbol)
            : base(pSymbol )
        {
            IMarkerSymbol pMarkerSymbol = pSymbol as IMarkerSymbol;
            Angle = pMarkerSymbol.Angle;
            Color = CommStaticClass.GimmeStringForColor(pMarkerSymbol.Color);
            Size = CommStaticClass.GetPiexlFromPoints(pMarkerSymbol.Size);
            XOffset = CommStaticClass.GetPiexlFromPoints(pMarkerSymbol.XOffset);
            YOffset = CommStaticClass.GetPiexlFromPoints(pMarkerSymbol.YOffset);
            if (!string.IsNullOrEmpty(Color))
            {
                Filled = pMarkerSymbol.Color.Transparency != 0;
            }
        }
        /// <summary>
        /// 角度
        /// </summary>
        public double Angle { get;  }
        /// <summary>
        /// 颜色
        /// </summary>
        public string Color { get;  }
        /// <summary>
        /// 大小
        /// </summary>
        public double Size { get;  }
        /// <summary>
        /// X偏移量
        /// </summary>
        public double XOffset { get; }
        /// <summary>
        /// Y偏移量
        /// </summary>
        public double YOffset { get; }
        /// <summary>
        /// 是否填充
        /// </summary>
        public bool Filled { get;}
        /// <summary>
        /// 获取当前符号节点
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        public override IList<XmlElement> GetSymbolNode(XmlDocument xmlDoc)
        {
            IList<XmlElement> returenData = new List<XmlElement>();
            XmlElement pSymboleElement = default(XmlElement);
            pSymboleElement = CommXmlHandle.CreateElement("PointSymbolizer", xmlDoc);
            //写偏移
            if (XOffset != 0.00 || YOffset != 0.00)
            {
                XmlElement pOffsetElement = CommXmlHandle.CreateElement("PointGeometry", xmlDoc);
                XmlElement pFunctionElment= CommXmlHandle.CreateElement("PointFunction", xmlDoc);
                CommXmlHandle.SetAttributeValue("offset", CommXmlHandle.CreateAttribute("name", pFunctionElment, xmlDoc));
                pFunctionElment.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("PropertyName", xmlDoc, "the_geom"));
                pFunctionElment.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("Fieldvalue", xmlDoc, XOffset.ToString()));
                pFunctionElment.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("Fieldvalue", xmlDoc, YOffset.ToString()));
                pOffsetElement.AppendChild(pFunctionElment);
                pSymboleElement.AppendChild(pOffsetElement);
            }
            pSymboleElement.AppendChild(CommXmlHandle.CreateElement("PointGraphic", xmlDoc));
            //返回PointSymbolizer节点
            returenData.Add(pSymboleElement);
            return returenData;
        }
    }
    /// <summary>
    /// 简单标记符号
    /// </summary>
    public class ptSimpleMarkerSymbolClass : ptMarkerSymbolClass
    {
        public ptSimpleMarkerSymbolClass(ISymbol pSymbol)
            : base(pSymbol)
        {
            ISimpleMarkerSymbol pSimpleMarkerSymbol = pSymbol as ISimpleMarkerSymbol;
            Outline = pSimpleMarkerSymbol.Outline;
            OutlineColor = CommStaticClass.GimmeStringForColor(pSimpleMarkerSymbol.OutlineColor);
            OutlineSize = CommStaticClass.GetPiexlFromPoints(pSimpleMarkerSymbol.OutlineSize);
            switch (pSimpleMarkerSymbol.Style)
            {
                case esriSimpleMarkerStyle.esriSMSCircle:
                    Style = "circle";
                    break;
                case esriSimpleMarkerStyle.esriSMSCross:
                    Style = "cross";
                    break;
                case esriSimpleMarkerStyle.esriSMSDiamond:
                    Style = "triangle";
                    break;
                case esriSimpleMarkerStyle.esriSMSSquare:
                    Style = "square";
                    break;
                case esriSimpleMarkerStyle.esriSMSX:
                    Style = "X";
                    break;
                default:
                    Style = "circle";
                    break;
            }
        }
        /// <summary>
        /// 是否有边框线
        /// </summary>
        public bool Outline { get; }
        /// <summary>
        /// 边框线颜色
        /// </summary>
        public string OutlineColor { get; }
        /// <summary>
        /// 边框线尺寸
        /// </summary>
        public double OutlineSize { get; }
        /// <summary>
        /// 样式
        /// </summary>
        public string Style { get; }
        public override IList<XmlElement> GetSymbolNode(XmlDocument xmlDoc)
        {
            //PointSymbolizer节点
            IList<XmlElement> SymbolizerElement = base.GetSymbolNode(xmlDoc);
            XmlElement graphicElement = SymbolizerElement[0].LastChild as XmlElement;
            if (graphicElement == default(XmlElement))
            {
                ptLogManager.WriteMessage(string.Format("无法从点符号化节点下获取图形节点"));
                return SymbolizerElement;
            }
            //mark节点
            XmlElement markElement =CommXmlHandle.CreateElement("Mark", xmlDoc);
            //写WellKnownName
            markElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("PointWellKnownName", xmlDoc, Style));
            //填充节点
            if (Filled)
            {
                XmlElement pFillElment = CommXmlHandle.CreateElement("PointFill", xmlDoc);
                XmlElement CssElment = CommXmlHandle.CreateElementAndSetElemnetText("PointFillCssParameter", xmlDoc, Color);
                CommXmlHandle.SetAttributeValue("fill", CommXmlHandle.CreateAttribute("name", CssElment, xmlDoc));
                pFillElment.AppendChild(CssElment);
                markElement.AppendChild(pFillElment);
            }
            //outline节点
            if (Outline)
            {
                XmlElement pOutLineElment = CommXmlHandle.CreateElement("PointStroke", xmlDoc);
                XmlElement CssColor= CommXmlHandle.CreateElementAndSetElemnetText("PointStrokeCssParameter", xmlDoc,OutlineColor);
                CommXmlHandle.SetAttributeValue("stroke", CommXmlHandle.CreateAttribute("name", CssColor, xmlDoc));
                pOutLineElment.AppendChild(CssColor);
                XmlElement CssOutlinesize= CommXmlHandle.CreateElementAndSetElemnetText("PointStrokeCssParameter", xmlDoc, OutlineSize.ToString());
                CommXmlHandle.SetAttributeValue("stroke-width", CommXmlHandle.CreateAttribute("name", CssOutlinesize, xmlDoc));
                pOutLineElment.AppendChild(CssOutlinesize);
                markElement.AppendChild(pOutLineElment);
            }
            graphicElement.AppendChild(markElement);
            //size节点
            graphicElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("PointSize", xmlDoc, Size.ToString()));
            //Rotation节点
            graphicElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("PointRotation", xmlDoc, Angle.ToString()));
            return SymbolizerElement;
        }

    }
    /// <summary>
    ///字符标记符号
    /// </summary>
    public class ptCharacterMarkerSymbolClass : ptMarkerSymbolClass
    {
        public ptCharacterMarkerSymbolClass(ISymbol pSymbol)
            : base(pSymbol)
        {
            ICharacterMarkerSymbol pCharacterMarkerSymbol = pSymbol as ICharacterMarkerSymbol;
            CharacterIndex = pCharacterMarkerSymbol.CharacterIndex;
            Font = pCharacterMarkerSymbol.Font.Name;
        }
        /// <summary>
        /// 字符集索引
        /// </summary>
        public int CharacterIndex { get; set; }
        /// <summary>
        /// 字体名称
        /// </summary>
        public string Font { get; set; }
        public override IList<XmlElement> GetSymbolNode(XmlDocument xmlDoc)
        {
            //PointSymbolizer节点
            IList<XmlElement> SymbolizerElement = base.GetSymbolNode(xmlDoc);
            XmlElement graphicElement = SymbolizerElement[0].LastChild as XmlElement;
            if (graphicElement == default(XmlElement))
            {
                ptLogManager.WriteMessage(string.Format("无法从点符号化节点下获取图形节点"));
                return SymbolizerElement;
            }
            //mark节点
            XmlElement markElement = CommXmlHandle.CreateElement("Mark", xmlDoc);
            //写WellKnownName
            markElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("PointWellKnownName", xmlDoc, string.Format("ttf://{0}#0x{1}", Font, CharacterIndex.ToString("X"))));
            //填充节点
            if (Filled)
            {
                XmlElement pFillElment = CommXmlHandle.CreateElement("PointFill", xmlDoc);
                XmlElement CssElment = CommXmlHandle.CreateElementAndSetElemnetText("PointFillCssParameter", xmlDoc, Color);
                CommXmlHandle.SetAttributeValue("fill", CommXmlHandle.CreateAttribute("name", CssElment, xmlDoc));
                pFillElment.AppendChild(CssElment);
                markElement.AppendChild(pFillElment);
            }
            //stroke节点
            XmlElement pstrokeElement = CommXmlHandle.CreateElement("PointStroke", xmlDoc);
            XmlElement CssColor = CommXmlHandle.CreateElementAndSetElemnetText("PointStrokeCssParameter", xmlDoc, Color);
            CommXmlHandle.SetAttributeValue("stroke", CommXmlHandle.CreateAttribute("name", CssColor, xmlDoc));
            pstrokeElement.AppendChild(CssColor);
            markElement.AppendChild(pstrokeElement);
            graphicElement.AppendChild(markElement);
            //size节点
            graphicElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("PointSize", xmlDoc, Size.ToString()));
            //Rotation节点
            graphicElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("PointRotation", xmlDoc, Angle.ToString()));
            return SymbolizerElement;
        }
    }
    /// <summary>
    /// 图片标记符号
    /// </summary>
    public class ptPictureMarkerSymbolClass : ptMarkerSymbolClass
    {
        public ptPictureMarkerSymbolClass(ISymbol pSymbol)
            : base(pSymbol)
        {
            IPictureMarkerSymbol pPictureMarkerSymbol = pSymbol as IPictureMarkerSymbol;
            BackgroundColor = CommStaticClass.GimmeStringForColor(pPictureMarkerSymbol.BackgroundColor);
            Picture = pPictureMarkerSymbol.Picture as IPicture;
            
        }
        /// <summary>
        /// 背景颜色
        /// </summary>
        public string BackgroundColor { get; }
        /// <summary>
        /// 图片对象
        /// </summary>
        public IPicture Picture { get;}
        public override IList<XmlElement> GetSymbolNode(XmlDocument xmlDoc)
        {
            IList<XmlElement> SymbolizerElement = base.GetSymbolNode(xmlDoc);
            XmlElement graphicElement = SymbolizerElement[0].LastChild as XmlElement;
            if (graphicElement == default(XmlElement))
            {
                ptLogManager.WriteMessage(string.Format("无法从点符号化节点下获取图形节点"));
                return SymbolizerElement;
            }
            //先将图片保存到本地
            string imagefile = string.Empty;
            if (Picture != null)
            {
                string filePath = System.IO.Path.GetDirectoryName(CommXmlHandle.m_SaveFileName);
                imagefile = string.Format("{0}\\{1}.png",filePath,Guid.NewGuid().ToString());
                Image pimage = IPictureConverter.IPictureToImage(Picture);
                Graphics g = Graphics.FromImage(pimage);
                Bitmap pbitmap = new Bitmap(pimage);
                pbitmap.MakeTransparent(System.Drawing.Color.Black);
                pbitmap.Save(imagefile, System.Drawing.Imaging.ImageFormat.Png);
            }
            //PointExternalGraphic节点
            XmlElement pExterGraphic = CommXmlHandle.CreateElement("PointExternalGraphic", xmlDoc);
            XmlElement pOnlineElment = CommXmlHandle.CreateElement("PointOnlineResource", xmlDoc);
            CommXmlHandle.SetAttributeValue("http://www.w3.org/1999/xlink", CommXmlHandle.CreateAttribute("xmlns:xlink", pOnlineElment, xmlDoc));
            CommXmlHandle.SetAttributeValue("simple", CommXmlHandle.CreateAttribute("xlink:type", pOnlineElment, xmlDoc));
            CommXmlHandle.SetAttributeValue(string.Format("file:\\{0}", imagefile.Replace('/', '\\')), CommXmlHandle.CreateAttribute("xlink:href", pOnlineElment, xmlDoc));
            pExterGraphic.AppendChild(pOnlineElment);
            pExterGraphic.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("PointFormat", xmlDoc, "image/png"));
            graphicElement.AppendChild(pExterGraphic);
            //size节点
            graphicElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("PointSize", xmlDoc, Size.ToString()));
            //Rotation节点
            graphicElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("PointRotation", xmlDoc, Angle.ToString()));
            return SymbolizerElement;
        }
    }
    /// <summary>
    /// 箭头标记符号
    /// </summary>
    public class ptArrowMarkerSymbolClass : ptMarkerSymbolClass
    {
        public ptArrowMarkerSymbolClass(ISymbol pSymbol)
            : base(pSymbol)
        {
            IArrowMarkerSymbol pArrowMarkerSymbol = pSymbol as IArrowMarkerSymbol;
            Style = pArrowMarkerSymbol.Style.ToString();
            Width = CommStaticClass.GetPiexlFromPoints(pArrowMarkerSymbol.Width);
            Length = CommStaticClass.GetPiexlFromPoints(pArrowMarkerSymbol.Length);
        }
        /// <summary>
        /// 样式
        /// </summary>
        public string Style { get; }
        /// <summary>
        /// 宽度
        /// </summary>
        public double Width { get;}
        /// <summary>
        /// 长度
        /// </summary>
        public double Length { get;}
        public override IList<XmlElement> GetSymbolNode(XmlDocument xmlDoc)
        {
            //PointSymbolizer节点
            IList<XmlElement> SymbolizerElement = base.GetSymbolNode(xmlDoc);
            XmlElement graphicElement = SymbolizerElement[0].LastChild as XmlElement;
            if (graphicElement == default(XmlElement))
            {
                ptLogManager.WriteMessage(string.Format("无法从点符号化节点下获取图形节点"));
                return SymbolizerElement;
            }
            //mark节点
            XmlElement markElement = CommXmlHandle.CreateElement("Mark", xmlDoc);
            //写WellKnownName
            markElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("PointWellKnownName", xmlDoc, "arrow"));
            //填充节点
            if (Filled)
            {
                XmlElement pFillElment = CommXmlHandle.CreateElement("PointFill", xmlDoc);
                XmlElement CssElment = CommXmlHandle.CreateElementAndSetElemnetText("PointFillCssParameter", xmlDoc, Color);
                CommXmlHandle.SetAttributeValue("fill", CommXmlHandle.CreateAttribute("name", CssElment, xmlDoc));
                pFillElment.AppendChild(CssElment);
                markElement.AppendChild(pFillElment);
            }
            XmlElement pstrokeNode = CommXmlHandle.CreateElement("PointStroke", xmlDoc);
            XmlElement CssOutlinesize = CommXmlHandle.CreateElementAndSetElemnetText("PointStrokeCssParameter", xmlDoc, Width.ToString());
            CommXmlHandle.SetAttributeValue("stroke-width", CommXmlHandle.CreateAttribute("name", CssOutlinesize, xmlDoc));
            pstrokeNode.AppendChild(CssOutlinesize);
            markElement.AppendChild(pstrokeNode);
            graphicElement.AppendChild(markElement);
            //size节点
            graphicElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("PointSize", xmlDoc, Size.ToString()));
            //Rotation节点
            graphicElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("PointRotation", xmlDoc, Angle.ToString()));
            return SymbolizerElement;
        }
    }
    #endregion

    #region 线符号
    /// <summary>
    /// 线符号
    /// </summary>
    public class ptLineSymbolClass : ptSymbolClass
    {
        public ptLineSymbolClass(ISymbol pSymbol)
            : base(pSymbol)
        {
            ILineSymbol pLineSymbol = pSymbol as ILineSymbol;
            Color = CommStaticClass.GimmeStringForColor(pLineSymbol.Color);
            Width = CommStaticClass.GetPiexlFromPoints(pLineSymbol.Width);
            Transparency = pLineSymbol.Color.Transparency;
            if (pLineSymbol is ILineProperties)
            {
                ILineProperties pLinePro = pLineSymbol as ILineProperties;
                Offset = CommStaticClass.GetPiexlFromPoints(pLinePro.Offset);
            }
        }
        /// <summary>
        /// 颜色
        /// </summary>
        public string Color { get; set; }
        /// <summary>
        /// 透明度
        /// </summary>
        public byte Transparency { get;}
        /// <summary>
        /// 宽度
        /// </summary>
        public double Width { get;}
        /// <summary>
        /// 偏移
        /// </summary>
        public double Offset { get; }
        public override IList<XmlElement> GetSymbolNode(XmlDocument xmlDoc)
        {
            IList<XmlElement> returenData = new List<XmlElement>();
            XmlElement pSymboleElement = default(XmlElement);
            pSymboleElement = CommXmlHandle.CreateElement("LineSymbolizer", xmlDoc);
            if (Offset!=0.00)
            {
                XmlElement pGeometryElement = CommXmlHandle.CreateElement("PolygonGeometry", xmlDoc);
                XmlElement pFunciontElement = CommXmlHandle.CreateElement("PointFunction", xmlDoc);
                CommXmlHandle.SetAttributeValue("offset", CommXmlHandle.CreateAttribute("name", pFunciontElement, xmlDoc));
                pGeometryElement.AppendChild(pFunciontElement);
                XmlElement pProElement = CommXmlHandle.CreateElementAndSetElemnetText("PropertyName", xmlDoc, "geom");
                pFunciontElement.AppendChild(pProElement);
                pFunciontElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("Fieldvalue", xmlDoc, Offset.ToString()));
                pFunciontElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("Fieldvalue", xmlDoc, "0.00"));
                pSymboleElement.AppendChild(pGeometryElement);
            }
            pSymboleElement.AppendChild(CommXmlHandle.CreateElement("LineStroke", xmlDoc));
            //返回LineSymbolizer节点
            returenData.Add(pSymboleElement);
            return returenData;
        }
    }
    /// <summary>
    /// 简单线符号
    /// </summary>
    public class ptSimpleLineSymbolClass : ptLineSymbolClass
    {
        /// <summary>
        /// 简单线符号
        /// </summary>
        /// <param name="pSymbol"></param>
        public ptSimpleLineSymbolClass(ISymbol pSymbol) 
            : base(pSymbol)
        {
            ISimpleLineSymbol pSimpLineSymbol = pSymbol as ISimpleLineSymbol;
            switch (pSimpLineSymbol.Style)
            {
                case esriSimpleLineStyle.esriSLSDash:
                    publicStyle = "10.0 10.0";
                    break;
                case esriSimpleLineStyle.esriSLSDashDot:
                    publicStyle = "10.0 10.0 1.0 10.0";
                    break;
                case esriSimpleLineStyle.esriSLSDashDotDot:
                    publicStyle = "10.0 10.0 1.0 10.0 1.0 10.0";
                    break;
                case esriSimpleLineStyle.esriSLSDot:
                    publicStyle = "1.0 5.0";
                    break;
                default:
                    publicStyle = string.Empty;
                    break;

            }
            publicStyle = pSimpLineSymbol.Style.ToString();
            if (pSimpLineSymbol.Style == esriSimpleLineStyle.esriSLSNull)
            {
                base.Color = string.Empty;
            }
        }
        /// <summary>
        /// 样式类型
        /// </summary>
        public string publicStyle { get;}
        public override IList<XmlElement> GetSymbolNode(XmlDocument xmlDoc)
        {
            //LineSymbolizer节点
            IList<XmlElement> SymbolizerElement = base.GetSymbolNode(xmlDoc);
            XmlElement strokeElement = SymbolizerElement[0].LastChild as XmlElement;
            if (strokeElement == default(XmlElement))
            {
                ptLogManager.WriteMessage(string.Format("无法从线符号化节点下获取图形节点"));
                return SymbolizerElement;
            }
            if (string.IsNullOrEmpty(Color))
            {
                //颜色节点
                XmlElement pStrokeColor = CommXmlHandle.CreateElementAndSetElemnetText("LineCssParameter", xmlDoc, Color.ToString());
                CommXmlHandle.SetAttributeValue("stroke", CommXmlHandle.CreateAttribute("name", pStrokeColor, xmlDoc));
                strokeElement.AppendChild(pStrokeColor);
            }
            XmlElement pOpacityElement = CommXmlHandle.CreateElementAndSetElemnetText("LineCssParameter", xmlDoc, string.Format("{0}",Transparency / 255));
            CommXmlHandle.SetAttributeValue("stroke-opacity", CommXmlHandle.CreateAttribute("name", pOpacityElement, xmlDoc));
            strokeElement.AppendChild(pOpacityElement);
            //线宽节点
            XmlElement pStrokeWidth = default(XmlElement);
            if (publicStyle == "esriSLSSolid")
            {
                pStrokeWidth = CommXmlHandle.CreateElementAndSetElemnetText("LineCssParameter", xmlDoc, Width.ToString());
            }
            else
            {
                pStrokeWidth = CommXmlHandle.CreateElementAndSetElemnetText("LineCssParameter", xmlDoc, "1");
            }
            CommXmlHandle.SetAttributeValue("stroke-width", CommXmlHandle.CreateAttribute("name", pStrokeWidth, xmlDoc));
            strokeElement.AppendChild(pStrokeWidth);
            //写Dash节点
            if (string.IsNullOrEmpty(publicStyle))
            {
                XmlElement dashElment = CommXmlHandle.CreateElementAndSetElemnetText("LineCssParameter", xmlDoc, publicStyle);
                CommXmlHandle.SetAttributeValue("stroke-dasharray", CommXmlHandle.CreateAttribute("name", dashElment, xmlDoc));
                strokeElement.AppendChild(dashElment);
            }
            return SymbolizerElement;
        }
    }
    /// <summary>
    /// 制图线符号
    /// </summary>
    public class ptCartographicLineSymbol : ptLineSymbolClass
    {
        /// <summary>
        /// 制图线符号
        /// </summary>
        /// <param name="pSymbol"></param>
        public ptCartographicLineSymbol(ISymbol pSymbol) 
            : base(pSymbol)
        {
            ICartographicLineSymbol pCartographicLineSymbol = pSymbol as ICartographicLineSymbol;
            switch (pCartographicLineSymbol.Join)
            {
                case esriLineJoinStyle.esriLJSBevel:
                    Join = "bevel";
                    break;
                case esriLineJoinStyle.esriLJSMitre:
                    Join = "mitre";
                    break;
                case esriLineJoinStyle.esriLJSRound:
                    Join = "round";
                    break;
            }
            switch (pCartographicLineSymbol.Cap)
            {
                case esriLineCapStyle.esriLCSButt:
                    Cap = "butt";
                    break;
                case esriLineCapStyle.esriLCSRound:
                    Cap = "round";
                    break;
                case esriLineCapStyle.esriLCSSquare:
                    Cap = "square";
                    break;
            }
            MiterLimit = pCartographicLineSymbol.MiterLimit;
            DashArray = new List<double>();
            if (pCartographicLineSymbol is ILineProperties)
            {
                ILineProperties lineProperties = pCartographicLineSymbol as ILineProperties;
                double markLen = 0;
                double gapLen = 0;
                bool filp = lineProperties.Flip;

                if (lineProperties.Template is ITemplate)
                {
                    ITemplate template = lineProperties.Template;
                    double interval = template.Interval;
                    for (int templateIdx = 0; templateIdx <= template.PatternElementCount - 1; templateIdx++)
                    {
                        template.GetPatternElement(templateIdx, out markLen, out gapLen);
                        DashArray.Add(markLen * interval);
                        DashArray.Add(gapLen * interval);
                    }
                }
                //装饰线
                //忽略装饰线多图层的情况，这里SLD貌似不支持
                ILineDecoration pLineDecoration = lineProperties.LineDecoration;
                if (pLineDecoration != null)
                {
                    if (pLineDecoration.ElementCount > 0)
                    {
                        ISimpleLineDecorationElement pElement = pLineDecoration.Element[0] as ISimpleLineDecorationElement;
                        SimpleLineDecoration = new ptSimpleLineDecorationClass(pElement);
                    }
                }
            }
            if (DashArray.Count != 0)
            {
                foreach (var key in DashArray)
                {
                    Dash = string.Format("{0} {1}", Dash, key);
                }
                Dash = Dash.TrimStart();
            }
        }
        public string Join { get;}
        public double MiterLimit { get;}
        public string Cap { get; }
        public IList<double> DashArray { get;}
        private string Dash = string.Empty;
        /// <summary>
        /// 装饰线
        /// </summary>
        public ptSimpleLineDecorationClass SimpleLineDecoration { get; }
        /// <summary>
        /// 获取符号XML节点
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        public override IList<XmlElement> GetSymbolNode(XmlDocument xmlDoc)
        {
            //LineSymbolizer节点
            IList<XmlElement> SymbolizerElement = base.GetSymbolNode(xmlDoc);
            XmlElement strokeElement = SymbolizerElement[0].LastChild as XmlElement;
            if (strokeElement == default(XmlElement))
            {
                ptLogManager.WriteMessage(string.Format("无法从线符号化节点下获取图形节点"));
                return SymbolizerElement;
            }
            if (!string.IsNullOrEmpty(Color))
            {
                //颜色节点
                XmlElement pStrokeColor = CommXmlHandle.CreateElementAndSetElemnetText("LineCssParameter", xmlDoc, Color.ToString());
                CommXmlHandle.SetAttributeValue("stroke", CommXmlHandle.CreateAttribute("name", pStrokeColor, xmlDoc));
                strokeElement.AppendChild(pStrokeColor);
            }
            //透明度节点
            XmlElement pOpacityElement = CommXmlHandle.CreateElementAndSetElemnetText("LineCssParameter", xmlDoc, string.Format("{0}", Transparency / 255));
            CommXmlHandle.SetAttributeValue("stroke-opacity", CommXmlHandle.CreateAttribute("name", pOpacityElement, xmlDoc));
            strokeElement.AppendChild(pOpacityElement);
            //linecap节点
            XmlElement pCapElement= CommXmlHandle.CreateElementAndSetElemnetText("LineCssParameter", xmlDoc, Cap);
            CommXmlHandle.SetAttributeValue("stroke-linecap", CommXmlHandle.CreateAttribute("name", pCapElement, xmlDoc));
            strokeElement.AppendChild(pCapElement);
            //linejoin节点
            XmlElement pJoinElment = CommXmlHandle.CreateElementAndSetElemnetText("LineCssParameter", xmlDoc, Join);
            CommXmlHandle.SetAttributeValue("stroke-linejoin", CommXmlHandle.CreateAttribute("name", pJoinElment, xmlDoc));
            strokeElement.AppendChild(pJoinElment);
            //线宽节点
            XmlElement pStrokeWidth = CommXmlHandle.CreateElementAndSetElemnetText("LineCssParameter", xmlDoc, Width.ToString());
            CommXmlHandle.SetAttributeValue("stroke-width", CommXmlHandle.CreateAttribute("name", pStrokeWidth, xmlDoc));
            strokeElement.AppendChild(pStrokeWidth);
            //写Dash节点
            if (!string.IsNullOrEmpty(Dash))
            {
                XmlElement dashElment = CommXmlHandle.CreateElementAndSetElemnetText("LineCssParameter", xmlDoc, Dash);
                CommXmlHandle.SetAttributeValue("stroke-dasharray", CommXmlHandle.CreateAttribute("name", dashElment, xmlDoc));
                strokeElement.AppendChild(dashElment);
            }
            //写装饰线符号
            if (SimpleLineDecoration != null)
            {
                IList<XmlElement> pLineDecoration = SimpleLineDecoration.GetSymbolNode(xmlDoc);
                foreach(XmlElement pElement in pLineDecoration)
                {
                    SymbolizerElement.Add(pElement);
                }
            }
            return SymbolizerElement;
        }
    }
    /// <summary>
    /// 混列线符号
    /// </summary>
    public class ptHashLineSymbolClass : ptLineSymbolClass
    {
        /// <summary>
        /// 混列线符号
        /// </summary>
        /// <param name="pSymbol"></param>
        public ptHashLineSymbolClass(ISymbol pSymbol) 
            : base(pSymbol)
        {
            IHashLineSymbol pHashLineSymbol = pSymbol as IHashLineSymbol;
            Angle = pHashLineSymbol.Angle;
            ILineSymbol pLineSymbol = pHashLineSymbol.HashSymbol;
            if (pLineSymbol is ICartographicLineSymbol)
            {
                HashSymbol = new ptCartographicLineSymbol(pLineSymbol as ISymbol);
            }
            else if (pLineSymbol is IMarkerLineSymbol)
            {
                HashSymbol = new ptMarkerLineSymbolClass(pLineSymbol as ISymbol);
            }
            else if (pLineSymbol is ISimpleLineSymbol)
            {
                HashSymbol = new ptSimpleLineSymbolClass(pLineSymbol as ISymbol);
            }
            else if (pLineSymbol is IPictureLineSymbol)
            {
                HashSymbol = new ptPictureLineSymbolClass(pLineSymbol as ISymbol);
            }
            else if (pLineSymbol is IMultiLayerLineSymbol)
            {
                HashSymbol = new ptMultilayerLineSymbolClass(pLineSymbol as ISymbol);
            }

            if (pHashLineSymbol is ILineProperties)
            {
                ILineProperties lineProperties = pHashLineSymbol as ILineProperties;
                double markLen = 0;
                double gapLen = 0;
                bool filp = lineProperties.Flip;
                DashArray = new List<double>();
                if (lineProperties.Template is ITemplate)
                {
                    ITemplate template = lineProperties.Template;
                    double interval = template.Interval;
                    for (int templateIdx = 0; templateIdx <= template.PatternElementCount - 1; templateIdx++)
                    {
                        template.GetPatternElement(templateIdx, out markLen, out gapLen);
                        DashArray.Add(markLen * interval);
                        DashArray.Add(gapLen * interval);
                    }
                }
                if (DashArray.Count != 0)
                {
                    foreach (var key in DashArray)
                    {
                        Dash = string.Format("{0} {1}", Dash, key);
                    }
                    Dash = Dash.TrimStart();
                }
                //装饰线
                //由于SLD装饰线只有两头，所以默认值读第一个，其他的忽略
                ILineDecoration pLineDecoration = lineProperties.LineDecoration;
                if (pLineDecoration.ElementCount > 0)
                {
                    ISimpleLineDecorationElement pElement = pLineDecoration.Element[0] as ISimpleLineDecorationElement;
                    SimpleLineDecoration = new ptSimpleLineDecorationClass(pElement);
                }
            }
            if (pHashLineSymbol is ICartographicLineSymbol)
            {
                ICartographicLineSymbol pCartographicLine = pHashLineSymbol as ICartographicLineSymbol;
                switch (pCartographicLine.Join)
                {
                    case esriLineJoinStyle.esriLJSBevel:
                        Join = "bevel";
                        break;
                    case esriLineJoinStyle.esriLJSMitre:
                        Join = "mitre";
                        break;
                    case esriLineJoinStyle.esriLJSRound:
                        Join = "round";
                        break;
                }
                switch (pCartographicLine.Cap)
                {
                    case esriLineCapStyle.esriLCSButt:
                        Cap = "butt";
                        break;
                    case esriLineCapStyle.esriLCSRound:
                        Cap = "round";
                        break;
                    case esriLineCapStyle.esriLCSSquare:
                        Cap = "square";
                        break;
                }
            }
        }
        public IList<double> DashArray { get; }
        /// <summary>
        /// 装饰线
        /// </summary>
        public ptSimpleLineDecorationClass SimpleLineDecoration { get; }
        private string Dash = string.Empty;
        public string Cap { get; }
        public string Join { get; }
        /// <summary>
        /// 角度
        /// </summary>
        public double Angle { get; }
        /// <summary>
        /// 混列线符号
        /// </summary>
        public ptLineSymbolClass HashSymbol { get;}
        public override IList<XmlElement> GetSymbolNode(XmlDocument xmlDoc)
        {
            //LineSymbolizer节点
            IList<XmlElement> SymbolizerElement = base.GetSymbolNode(xmlDoc);
            XmlElement strokeElement = SymbolizerElement[0].LastChild as XmlElement;
            if (strokeElement == default(XmlElement))
            {
                ptLogManager.WriteMessage(string.Format("无法从线符号化节点下获取图形节点"));
                return SymbolizerElement;
            }
            if (!string.IsNullOrEmpty(Color))
            {
                //颜色节点
                XmlElement pStrokeColor = CommXmlHandle.CreateElementAndSetElemnetText("LineCssParameter", xmlDoc, Color.ToString());
                CommXmlHandle.SetAttributeValue("stroke", CommXmlHandle.CreateAttribute("name", pStrokeColor, xmlDoc));
                strokeElement.AppendChild(pStrokeColor);
            }
            //透明度节点
            XmlElement pOpacityElement = CommXmlHandle.CreateElementAndSetElemnetText("LineCssParameter", xmlDoc, string.Format("{0}", Transparency / 255));
            CommXmlHandle.SetAttributeValue("stroke-opacity", CommXmlHandle.CreateAttribute("name", pOpacityElement, xmlDoc));
            strokeElement.AppendChild(pOpacityElement);
            if (!string.IsNullOrEmpty(Cap))
            {
                //linecap节点
                XmlElement pCapElement = CommXmlHandle.CreateElementAndSetElemnetText("LineCssParameter", xmlDoc, Cap);
                CommXmlHandle.SetAttributeValue("stroke-linecap", CommXmlHandle.CreateAttribute("name", pCapElement, xmlDoc));
                strokeElement.AppendChild(pCapElement);
            }
            if (!string.IsNullOrEmpty(Join))
            {
                //linejoin节点
                XmlElement pJoinElment = CommXmlHandle.CreateElementAndSetElemnetText("LineCssParameter", xmlDoc, Join);
                CommXmlHandle.SetAttributeValue("stroke-linejoin", CommXmlHandle.CreateAttribute("name", pJoinElment, xmlDoc));
                strokeElement.AppendChild(pJoinElment);
            }
            //线宽节点
            XmlElement pStrokeWidth = CommXmlHandle.CreateElementAndSetElemnetText("LineCssParameter", xmlDoc, Width.ToString());
            CommXmlHandle.SetAttributeValue("stroke-width", CommXmlHandle.CreateAttribute("name", pStrokeWidth, xmlDoc));
            strokeElement.AppendChild(pStrokeWidth);
            //写Dash节点
            if (!string.IsNullOrEmpty(Dash))
            {
                XmlElement dashElment = CommXmlHandle.CreateElementAndSetElemnetText("LineCssParameter", xmlDoc, Dash);
                CommXmlHandle.SetAttributeValue("stroke-dasharray", CommXmlHandle.CreateAttribute("name", dashElment, xmlDoc));
                strokeElement.AppendChild(dashElment);
            }
            //写装饰线符号
            if (SimpleLineDecoration != null)
            {
                IList<XmlElement> pLineDecoration = SimpleLineDecoration.GetSymbolNode(xmlDoc);
                foreach (XmlElement pElement in pLineDecoration)
                {
                    SymbolizerElement.Add(pElement);
                }
            }
            return SymbolizerElement;
        }
    }
    /// <summary>
    /// 标记线符号
    /// </summary>
    public class ptMarkerLineSymbolClass : ptLineSymbolClass
    {
        /// <summary>
        /// 标记线符号
        /// </summary>
        /// <param name="pSymbol"></param>
        public ptMarkerLineSymbolClass(ISymbol pSymbol) 
            : base(pSymbol)
        {
            IMarkerLineSymbol pMarkerLineSymbol = pSymbol as IMarkerLineSymbol;
            IMarkerSymbol pMarkerSymbol = pMarkerLineSymbol.MarkerSymbol;
            if (pMarkerSymbol is ISimpleMarkerSymbol)
            {
                MarkSymbol = new ptSimpleMarkerSymbolClass(pMarkerSymbol as ISymbol);
            }
            else if (pMarkerSymbol is ICharacterMarkerSymbol)
            {
                MarkSymbol = new ptCharacterMarkerSymbolClass(pMarkerSymbol as ISymbol);
            }
            else if (pMarkerSymbol is IPictureMarkerSymbol)
            {
                MarkSymbol = new ptPictureMarkerSymbolClass(pMarkerSymbol as ISymbol);
            }
            else if (pMarkerSymbol is IArrowMarkerSymbol)
            {
                MarkSymbol = new ptArrowMarkerSymbolClass(pMarkerSymbol as ISymbol);
            }
            else if (pMarkerSymbol is IMultiLayerMarkerSymbol)
            {
                MarkSymbol = new ptMultilayerMarkerSymbolClass(pMarkerSymbol as ISymbol);
            }

            if (pMarkerLineSymbol is ILineProperties)
            {
                ILineProperties lineProperties = pMarkerLineSymbol as ILineProperties;
                double markLen = 0;
                double gapLen = 0;
                bool filp = lineProperties.Flip;
                DashArray = new List<double>();
                if (lineProperties.Template is ITemplate)
                {
                    ITemplate template = lineProperties.Template;
                    double interval = template.Interval;
                    for (int templateIdx = 0; templateIdx <= template.PatternElementCount - 1; templateIdx++)
                    {
                        template.GetPatternElement(templateIdx, out markLen, out gapLen);
                        DashArray.Add(markLen * interval);
                        DashArray.Add(gapLen * interval);
                    }
                }
                if (DashArray.Count != 0)
                {
                    foreach (var key in DashArray)
                    {
                        Dash = string.Format("{0} {1}", Dash, key);
                    }
                    Dash = Dash.TrimStart();
                }
                //装饰线
                //由于SLD装饰线只有两头，所以默认值读第一个，其他的忽略
                ILineDecoration pLineDecoration = lineProperties.LineDecoration;
                if (pLineDecoration!=null&&pLineDecoration.ElementCount > 0)
                {
                    ISimpleLineDecorationElement pElement = pLineDecoration.Element[0] as ISimpleLineDecorationElement;
                    SimpleLineDecoration = new ptSimpleLineDecorationClass(pElement);
                }
            }
            if (pMarkerLineSymbol is ICartographicLineSymbol)
            {
                ICartographicLineSymbol pCartographicLine = pMarkerLineSymbol as ICartographicLineSymbol;
                switch (pCartographicLine.Join)
                {
                    case esriLineJoinStyle.esriLJSBevel:
                        Join = "bevel";
                        break;
                    case esriLineJoinStyle.esriLJSMitre:
                        Join = "mitre";
                        break;
                    case esriLineJoinStyle.esriLJSRound:
                        Join = "round";
                        break;
                }
                switch (pCartographicLine.Cap)
                {
                    case esriLineCapStyle.esriLCSButt:
                        Cap = "butt";
                        break;
                    case esriLineCapStyle.esriLCSRound:
                        Cap = "round";
                        break;
                    case esriLineCapStyle.esriLCSSquare:
                        Cap = "square";
                        break;
                }
            }
        }
        public IList<double> DashArray { get; }
        public string Cap { get; }
        public string Join { get; }
        private string Dash = string.Empty;
        /// <summary>
        /// 装饰线
        /// </summary>
        public ptSimpleLineDecorationClass SimpleLineDecoration { get; }
        /// <summary>
        /// 标记符号
        /// </summary>
        public ptMarkerSymbolClass MarkSymbol { get;}
        public override IList<XmlElement> GetSymbolNode(XmlDocument xmlDoc)
        {
            //LineSymbolizer节点
            IList<XmlElement> SymbolizerElement = base.GetSymbolNode(xmlDoc);
            XmlElement strokeElement = SymbolizerElement[0].LastChild as XmlElement;
            if (strokeElement == default(XmlElement))
            {
                ptLogManager.WriteMessage(string.Format("无法从线符号化节点下获取图形节点"));
                return SymbolizerElement;
            }
            //写点标记信息
            if (MarkSymbol != null)
            {
                XmlElement pGraphicStrokeElement = CommXmlHandle.CreateElement("LineGraphicStroke",xmlDoc);
                pGraphicStrokeElement.AppendChild(MarkSymbol.GetSymbolNode(xmlDoc)[0].LastChild.Clone());
                strokeElement.AppendChild(pGraphicStrokeElement);
            }
            if (!string.IsNullOrEmpty(Color))
            {
                //颜色节点
                XmlElement pStrokeColor = CommXmlHandle.CreateElementAndSetElemnetText("LineCssParameter", xmlDoc, Color.ToString());
                CommXmlHandle.SetAttributeValue("stroke", CommXmlHandle.CreateAttribute("name", pStrokeColor, xmlDoc));
                strokeElement.AppendChild(pStrokeColor);
            }
            //透明度节点
            XmlElement pOpacityElement = CommXmlHandle.CreateElementAndSetElemnetText("LineCssParameter", xmlDoc, string.Format("{0}", Transparency / 255));
            CommXmlHandle.SetAttributeValue("stroke-opacity", CommXmlHandle.CreateAttribute("name", pOpacityElement, xmlDoc));
            strokeElement.AppendChild(pOpacityElement);
            if (!string.IsNullOrEmpty(Cap))
            {
                //linecap节点
                XmlElement pCapElement = CommXmlHandle.CreateElementAndSetElemnetText("LineCssParameter", xmlDoc, Cap);
                CommXmlHandle.SetAttributeValue("stroke-linecap", CommXmlHandle.CreateAttribute("name", pCapElement, xmlDoc));
                strokeElement.AppendChild(pCapElement);
            }
            if (!string.IsNullOrEmpty(Join))
            {
                //linejoin节点
                XmlElement pJoinElment = CommXmlHandle.CreateElementAndSetElemnetText("LineCssParameter", xmlDoc, Join);
                CommXmlHandle.SetAttributeValue("stroke-linejoin", CommXmlHandle.CreateAttribute("name", pJoinElment, xmlDoc));
                strokeElement.AppendChild(pJoinElment);
            }
            //线宽节点
            XmlElement pStrokeWidth = CommXmlHandle.CreateElementAndSetElemnetText("LineCssParameter", xmlDoc, Width.ToString());
            CommXmlHandle.SetAttributeValue("stroke-width", CommXmlHandle.CreateAttribute("name", pStrokeWidth, xmlDoc));
            strokeElement.AppendChild(pStrokeWidth);
            //写Dash节点
            if (!string.IsNullOrEmpty(Dash))
            {
                XmlElement dashElment = CommXmlHandle.CreateElementAndSetElemnetText("LineCssParameter", xmlDoc, Dash);
                CommXmlHandle.SetAttributeValue("stroke-dasharray", CommXmlHandle.CreateAttribute("name", dashElment, xmlDoc));
                strokeElement.AppendChild(dashElment);
            }
            //写装饰线符号
            if (SimpleLineDecoration != null)
            {
                IList<XmlElement> pLineDecoration = SimpleLineDecoration.GetSymbolNode(xmlDoc);
                foreach (XmlElement pElement in pLineDecoration)
                {
                    SymbolizerElement.Add(pElement);
                }
            }
 
            return SymbolizerElement;
        }
    }
    /// <summary>
    /// 图片线符号
    /// </summary>
    public class ptPictureLineSymbolClass : ptLineSymbolClass
    {
        /// <summary>
        /// 图片线符号
        /// </summary>
        /// <param name="pSymbol"></param>
        public ptPictureLineSymbolClass(ISymbol pSymbol) 
            : base(pSymbol)
        {
            IPictureLineSymbol pPictureLineSymbol = pSymbol as IPictureLineSymbol;
            BackgroundColor = CommStaticClass.GimmeStringForColor(pPictureLineSymbol.BackgroundColor);
            BackgroundTransparency = pPictureLineSymbol.BackgroundColor.Transparency;
            Picture = pPictureLineSymbol.Picture as IPicture;
            Rotate = pPictureLineSymbol.Rotate;
            XScale = pPictureLineSymbol.XScale;
            YScale = pPictureLineSymbol.YScale;
        }
        /// <summary>
        /// 背景颜色
        /// </summary>
        public string BackgroundColor { get;}
        /// <summary>
        /// 背景透明度
        /// </summary>
        public byte BackgroundTransparency { get;}
        /// <summary>
        /// 图片
        /// </summary>
        public IPicture Picture { get;}
        /// <summary>
        /// 是否旋转
        /// </summary>
        public bool Rotate { get;}
        /// <summary>
        /// X比例
        /// </summary>
        public double XScale { get;}
        /// <summary>
        /// Y比例
        /// </summary>
        public double YScale { get;}
        public override IList<XmlElement> GetSymbolNode(XmlDocument xmlDoc)
        {
            //LineSymbolizer节点
            IList<XmlElement> SymbolizerElement = base.GetSymbolNode(xmlDoc);
            XmlElement strokeElement = SymbolizerElement[0].LastChild as XmlElement;
            if (strokeElement == default(XmlElement))
            {
                ptLogManager.WriteMessage(string.Format("无法从线符号化节点下获取图形节点"));
                return SymbolizerElement;
            }
            //先将图片保存到本地
            string imagefile = string.Empty;
            if (Picture != null)
            {
                string filePath = System.IO.Path.GetDirectoryName(CommXmlHandle.m_SaveFileName);
                imagefile = string.Format("{0}\\{1}.png", filePath, Guid.NewGuid().ToString());
                Image pimage = IPictureConverter.IPictureToImage(Picture);
                Graphics g = Graphics.FromImage(pimage);
                Bitmap pbitmap = new Bitmap(pimage);
                pbitmap.MakeTransparent(System.Drawing.Color.Black);
                pbitmap.Save(imagefile, System.Drawing.Imaging.ImageFormat.Png);
            }
            //新建GraphicStroke节点
            XmlElement pGraphicStrokeElement = CommXmlHandle.CreateElement("LineGraphicStroke", xmlDoc);
            strokeElement.AppendChild(pGraphicStrokeElement);
            //Graphic节点
            XmlElement pgraphicElement = CommXmlHandle.CreateElement("PointGraphic", xmlDoc);
            pGraphicStrokeElement.AppendChild(pgraphicElement);
            //ExternalGraphic节点
            XmlElement pExtrernalElement= CommXmlHandle.CreateElement("PointExternalGraphic", xmlDoc);
            pgraphicElement.AppendChild(pExtrernalElement);
            //OnlineResource节点
            XmlElement pOnlineResourceElement = CommXmlHandle.CreateElement("PointOnlineResource", xmlDoc);
            CommXmlHandle.SetAttributeValue("http://www.w3.org/1999/xlink", CommXmlHandle.CreateAttribute("xmlns:xlink", pOnlineResourceElement, xmlDoc));
            CommXmlHandle.SetAttributeValue("simple", CommXmlHandle.CreateAttribute("xlink:type", pOnlineResourceElement, xmlDoc));
            CommXmlHandle.SetAttributeValue(imagefile, CommXmlHandle.CreateAttribute("xlink:href", pOnlineResourceElement, xmlDoc));
            //Format节点
            XmlElement pFormatElement = CommXmlHandle.CreateElementAndSetElemnetText("PointFormat", xmlDoc, "image/png");
            pExtrernalElement.AppendChild(pOnlineResourceElement);
            pExtrernalElement.AppendChild(pFormatElement);
            //size节点
            XmlElement pSizeElement = CommXmlHandle.CreateElementAndSetElemnetText("PointSize", xmlDoc, this.Width.ToString());
            pgraphicElement.AppendChild(pSizeElement);
            return SymbolizerElement;
        }
    }
    #endregion

    #region 面符号
    /// <summary>
    /// 面符号
    /// </summary>
    public class ptFillSymbolClass : ptSymbolClass
    {
        public ptFillSymbolClass(ISymbol pSymbol) 
            : base(pSymbol)
        {
            IFillSymbol pFillSymbol = pSymbol as IFillSymbol;
            Color = CommStaticClass.GimmeStringForColor(pFillSymbol.Color);
            Transparency = pFillSymbol.Color.Transparency;
            ILineSymbol pOutLineSymbol = pFillSymbol.Outline;
            ptLineSymbolClass tempSymbol = null;
            if (pOutLineSymbol is ISimpleLineSymbol)
            {
                tempSymbol = new ptSimpleLineSymbolClass(pOutLineSymbol as ISymbol);
            }
            else if (pOutLineSymbol is ICartographicLineSymbol)
            {
                tempSymbol = new ptCartographicLineSymbol(pOutLineSymbol as ISymbol);
            }
            else if (pOutLineSymbol is IHashLineSymbol)
            {
                tempSymbol = new ptHashLineSymbolClass(pOutLineSymbol as ISymbol);

            }
            else if (pOutLineSymbol is IMarkerLineSymbol)
            {
                tempSymbol = new ptMarkerLineSymbolClass(pOutLineSymbol as ISymbol);
            }
            else if (pOutLineSymbol is IPictureLineSymbol)
            {
                tempSymbol = new ptPictureLineSymbolClass(pOutLineSymbol as ISymbol);
            }
            else if (pOutLineSymbol is IMultiLayerLineSymbol)
            {
                tempSymbol = new ptMultilayerLineSymbolClass(pOutLineSymbol as ISymbol);
            }
            OutlineSymbol = tempSymbol;
        }
        /// <summary>
        /// 颜色
        /// </summary>
        public string Color { get; set; }
        /// <summary>
        /// 透明度
        /// </summary>
        public byte Transparency { get;}
        /// <summary>
        /// 轮廓符号
        /// </summary>
        public ptLineSymbolClass OutlineSymbol { get;}
        /// <summary>
        /// 获取符号SLD节点(SLD暂时不支持外轮廓线多图层表达，这里只取第一层)
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        public override IList<XmlElement> GetSymbolNode(XmlDocument xmlDoc)
        {
            IList<XmlElement> returenData = new List<XmlElement>();
            XmlElement pSymboleElement = default(XmlElement);
            pSymboleElement = CommXmlHandle.CreateElement("PolygonSymbolizer", xmlDoc);
            //返回PolygonSymbolizer节点
            returenData.Add(pSymboleElement);
            return returenData;
        }
    }
    /// <summary>
    /// 简单填充符号
    /// </summary>
    public class ptSimpleFillSymbolClass : ptFillSymbolClass
    {
        public ptSimpleFillSymbolClass(ISymbol pSymbol) 
            : base(pSymbol)
        {
            ISimpleFillSymbol pFillSymbol = pSymbol as ISimpleFillSymbol;
            Style = pFillSymbol.Style.ToString();
            if (pFillSymbol.Style == esriSimpleFillStyle.esriSFSHollow)
            {
                Color = "";
            }
        }
        /// <summary>
        /// 样式
        /// </summary>
        public string Style { get; set; }
        /// <summary>
        /// 获取符号SLD节点
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        public override IList<XmlElement> GetSymbolNode(XmlDocument xmlDoc)
        {
            IList<XmlElement> returenData = new List<XmlElement>();
            XmlElement pSymbolizerElement = base.GetSymbolNode(xmlDoc)[0] as XmlElement;
            if (pSymbolizerElement == null)
            {
                ptLogManager.WriteMessage("无法获取面符号节点");
                return null;
            }
            //填充节点
            XmlElement pFillElement = CommXmlHandle.CreateElement("Fill", xmlDoc);
            pSymbolizerElement.AppendChild(pFillElement);
            if (!string.IsNullOrEmpty(Color))
            {
                XmlElement pFillColorEl = CommXmlHandle.CreateElementAndSetElemnetText("PolyCssParameter", xmlDoc,Color);
                CommXmlHandle.SetAttributeValue("fill", CommXmlHandle.CreateAttribute("name", pFillColorEl, xmlDoc));
                pFillElement.AppendChild(pFillColorEl);
            }
            //外轮廓线节点
            XmlElement pLineEl = OutlineSymbol.GetSymbolNode(xmlDoc)[0].LastChild as XmlElement;
            pSymbolizerElement.AppendChild(pLineEl);
            returenData.Add(pSymbolizerElement);
            return returenData; 

        }
    }
    /// <summary>
    /// 标记填充符号
    /// </summary>
    public class ptMarkerFillSymbolClass : ptFillSymbolClass
    {
        public ptMarkerFillSymbolClass(ISymbol pSymbol) 
            : base(pSymbol)
        {
            IMarkerFillSymbol pMarkerFillSymbol = pSymbol as IMarkerFillSymbol;
            IFillProperties pFillProperties = pMarkerFillSymbol as IFillProperties;
            GridAngle = pMarkerFillSymbol.GridAngle;
            IMarkerSymbol pMarkerSymbol = pMarkerFillSymbol.MarkerSymbol;
            ptMarkerSymbolClass pSymbolClass = null ;
            if (pMarkerSymbol is ISimpleMarkerSymbol)
            {
                pSymbolClass = new ptSimpleMarkerSymbolClass(pMarkerSymbol as ISymbol);
            }
            else if (pMarkerSymbol is ICharacterMarkerSymbol)
            {
                pSymbolClass = new ptCharacterMarkerSymbolClass(pMarkerSymbol as ISymbol);
            }
            else if (pMarkerSymbol is IPictureMarkerSymbol)
            {
                pSymbolClass = new ptPictureMarkerSymbolClass(pMarkerSymbol as ISymbol);
            }
            else if (pMarkerSymbol is IArrowMarkerSymbol)
            {
                pSymbolClass = new ptArrowMarkerSymbolClass(pMarkerSymbol as ISymbol);
            }
            else if (pMarkerSymbol is IMultiLayerMarkerSymbol)
            {
                pSymbolClass = new ptMultilayerMarkerSymbolClass(pMarkerSymbol as ISymbol);
            }
            MarkerSymbol = pSymbolClass;
            if (pFillProperties != null)
            {
                XOffset = CommStaticClass.GetPiexlFromPoints( pFillProperties.XOffset);
                YOffset = CommStaticClass.GetPiexlFromPoints(pFillProperties.YOffset);
                XSeparation = CommStaticClass.GetPiexlFromPoints(pFillProperties.XSeparation);
                YSeparation = CommStaticClass.GetPiexlFromPoints(pFillProperties.YSeparation);
            }
        }
        /// <summary>
        /// 网格角度
        /// </summary>
        public double GridAngle { get;}
        /// <summary>
        /// 标记符号
        /// </summary>
        public ptMarkerSymbolClass MarkerSymbol { get; }
        /// <summary>
        /// x轴偏移量
        /// </summary>
        public double XOffset { get; }
        /// <summary>
        /// y轴偏移量
        /// </summary>
        public double YOffset { get; }
        /// <summary>
        /// x轴间隔
        /// </summary>
        public double XSeparation { get; }
        /// <summary>
        /// y轴间隔
        /// </summary>
        public double YSeparation { get; }
        public override IList<XmlElement> GetSymbolNode(XmlDocument xmlDoc)
        {
            IList<XmlElement> returenData = new List<XmlElement>();
            XmlElement pSymbolizerElement = base.GetSymbolNode(xmlDoc)[0] as XmlElement;
            if (pSymbolizerElement == null)
            {
                ptLogManager.WriteMessage("无法获取面符号节点");
                return null;
            }
            //获取点符号
            if (MarkerSymbol != null)
            {
                XmlElement pGraphFillEl = CommXmlHandle.CreateElement("PolygonGraphicFill", xmlDoc);
                pSymbolizerElement.AppendChild(pGraphFillEl);
                XmlElement pPointEl = MarkerSymbol.GetSymbolNode(xmlDoc)[0].LastChild as XmlElement;
                pGraphFillEl.AppendChild(pPointEl);
            }
            //填充节点
            XmlElement pFillElement = CommXmlHandle.CreateElement("Fill", xmlDoc);
            pSymbolizerElement.AppendChild(pFillElement);
            if (!string.IsNullOrEmpty(Color))
            {
                XmlElement pFillColorEl = CommXmlHandle.CreateElementAndSetElemnetText("PolyCssParameter", xmlDoc, Color);
                CommXmlHandle.SetAttributeValue("fill", CommXmlHandle.CreateAttribute("name", pFillColorEl, xmlDoc));
                pFillElement.AppendChild(pFillColorEl);
            }
            if (!string.IsNullOrEmpty(Transparency.ToString()))
            {
                XmlElement pTransEl = CommXmlHandle.CreateElementAndSetElemnetText("PolyCssParameter", xmlDoc, Transparency.ToString());
                CommXmlHandle.SetAttributeValue("fill-opacity", CommXmlHandle.CreateAttribute("name", pTransEl, xmlDoc));
                pFillElement.AppendChild(pTransEl);
            }

            //外轮廓线节点
            XmlElement pLineEl = OutlineSymbol.GetSymbolNode(xmlDoc)[0].LastChild as XmlElement;
            pSymbolizerElement.AppendChild(pLineEl);

            returenData.Add(pSymbolizerElement);
            return returenData;
        }

    }
    /// <summary>
    /// 线填充符号
    /// </summary>
    public class ptLineFillSymbolClass : ptFillSymbolClass
    {
        public ptLineFillSymbolClass(ISymbol pSymbol)
            : base(pSymbol)
        {
            ILineFillSymbol pLineFillSymbol = pSymbol as ILineFillSymbol;
            Angle = pLineFillSymbol.Angle;
            Offset = CommStaticClass.GetPiexlFromPoints(pLineFillSymbol.Offset);
            Separation = CommStaticClass.GetPiexlFromPoints(pLineFillSymbol.Separation);
            ILineSymbol pLineSymbol = pLineFillSymbol.LineSymbol;
            ptLineSymbolClass pSymbolClass = null;
            if (pLineSymbol is ICartographicLineSymbol)
            {
                pSymbolClass = new ptCartographicLineSymbol(pLineSymbol as ISymbol);
            }
            else if (pLineSymbol is IMarkerLineSymbol)
            {
                pSymbolClass = new ptMarkerLineSymbolClass(pLineSymbol as ISymbol);
            }
            else if (pLineSymbol is IHashLineSymbol)
            {
                pSymbolClass = new ptHashLineSymbolClass(pLineSymbol as ISymbol);
            }
            else if (pLineSymbol is ISimpleLineSymbol)
            {
                pSymbolClass =new ptSimpleLineSymbolClass(pLineSymbol as ISymbol);
            }
            else if (pLineSymbol is IPictureLineSymbol)
            {
                pSymbolClass = new ptPictureLineSymbolClass(pLineSymbol as ISymbol);
            }
            else if (pLineSymbol is IMultiLayerLineSymbol)
            {
                pSymbolClass = new ptMultilayerLineSymbolClass(pLineSymbol as ISymbol);
            }
            LineSymbol = pSymbolClass;
        }
        /// <summary>
        /// 角度
        /// </summary>
        public double Angle { get;}
        /// <summary>
        /// 偏移
        /// </summary>
        public double Offset { get;}
        /// <summary>
        /// 间隔
        /// </summary>
        public double Separation { get;}
        /// <summary>
        /// 填充线符号
        /// </summary>
        public ptLineSymbolClass LineSymbol { get;}
        /// <summary>
        /// 先填充符号，SLD中填充无法设定填充线符号，只能设定填充线的颜色，角度以及线的宽度
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        public override IList<XmlElement> GetSymbolNode(XmlDocument xmlDoc)
        {
            IList<XmlElement> returenData = new List<XmlElement>();
            XmlElement pSymbolizerElement = base.GetSymbolNode(xmlDoc)[0] as XmlElement;
            if (pSymbolizerElement == null)
            {
                ptLogManager.WriteMessage("无法获取面符号节点");
                return null;
            }
            //Fill节点
            XmlElement pFillElment = CommXmlHandle.CreateElement("Fill", xmlDoc);
            //GraphicFill节点
            XmlElement pGraphFillElment = CommXmlHandle.CreateElement("PolygonGraphicFill", xmlDoc);
            pFillElment.AppendChild(pGraphFillElment);
            //Graphic节点
            XmlElement pGraphElment = CommXmlHandle.CreateElement("PolygonGraphic", xmlDoc);
            pGraphFillElment.AppendChild(pGraphElment);
            //Mark节点
            XmlElement pMarkElment = CommXmlHandle.CreateElement("PolygonMark", xmlDoc);
            pGraphElment.AppendChild(pMarkElment);
            //wellknownName节点
            XmlElement pWellElment = CommXmlHandle.CreateElement("PolygonWellKnownName", xmlDoc);
            if (Angle == 0.00 || Angle == 180.00 || Angle == 360.00 || Angle == -180.00 || Angle == -360.00)//水平线
            {
                CommXmlHandle.SetElementText("shape://horline", pWellElment);
            }
            else if (Angle == 90.00 || Angle == 270.00 || Angle == -90.00 || Angle == -270.00)//竖线填充
            {
                CommXmlHandle.SetElementText("shape://vertline", pWellElment);
            }
            else if ((Angle > 0.0 && Angle < 90.0) || (Angle > 180.0 && Angle < 270.0) || (Angle > -180.0 && Angle < -90.0) || (Angle > -360.0 && Angle < -270.0))
            {
                CommXmlHandle.SetElementText("shape://slash", pWellElment);
            }
            else if ((Angle > 90.0 && Angle < 180.0) || (Angle > 270.0 && Angle < 360.0) || (Angle < 0.0 && Angle > -90.0) || (Angle < -180.0 && Angle > -270.0))
            {
                CommXmlHandle.SetElementText("shape://backslash", pWellElment);
            }
            pGraphElment.AppendChild(pWellElment);
            //写线宽和颜色，SLD不支持线样式
            XmlElement pStrokeElment = CommXmlHandle.CreateElement("PolygonStroke", xmlDoc);
            pGraphElment.AppendChild(pStrokeElment);
            //线颜色
            XmlElement pStrokeColorElment = CommXmlHandle.CreateElementAndSetElemnetText("PolyCssParameter", xmlDoc, LineSymbol.Color);
            CommXmlHandle.SetAttributeValue("stroke", CommXmlHandle.CreateAttribute("name", pStrokeColorElment, xmlDoc));
            pStrokeElment.AppendChild(pStrokeColorElment);
            //线宽度
            XmlElement pLineWidthElment = CommXmlHandle.CreateElementAndSetElemnetText("PolyCssParameter", xmlDoc, LineSymbol.Width.ToString());
            CommXmlHandle.SetAttributeValue("stroke-width", CommXmlHandle.CreateAttribute("name", pLineWidthElment, xmlDoc));
            pStrokeElment.AppendChild(pLineWidthElment);
            //写Size节点
            XmlElement pSizeElment = CommXmlHandle.CreateElementAndSetElemnetText("PolygonSize", xmlDoc, CommStaticClass.CommaToPoint(Separation+5));
            pGraphElment.AppendChild(pSizeElment);
            pSymbolizerElement.AppendChild(pFillElment);
            //外轮廓线节点
            XmlElement pLineEl = OutlineSymbol.GetSymbolNode(xmlDoc)[0].LastChild as XmlElement;
            pSymbolizerElement.AppendChild(pLineEl);

            returenData.Add(pSymbolizerElement);
            return returenData;
        }
    }
    /// <summary>
    /// 点密度填充符号(ArcGIS10.2没有点密度填充符号)
    /// </summary>
    public class ptDotDensityFillSymbolClass : ptFillSymbolClass
    {
        public ptDotDensityFillSymbolClass(ISymbol pSymbol) 
            : base(pSymbol)
        {
            IDotDensityFillSymbol pDotDensityFillSymol = pSymbol as IDotDensityFillSymbol;
            BackgroundColor =CommStaticClass.GimmeStringForColor(pDotDensityFillSymol.BackgroundColor);
            BackgroundTransparency = pDotDensityFillSymol.BackgroundColor.Transparency;
            ISymbolArray pSymbolArray = pDotDensityFillSymol as ISymbolArray;
            SymbolCount = pSymbolArray.SymbolCount;
            DotSize = pDotDensityFillSymol.DotSize;
            DotSpacing = pDotDensityFillSymol.DotSpacing;
            FixedPlacement = pDotDensityFillSymol.FixedPlacement;
            SymbolList = new List<ptSymbolClass>();
            for (int i = 0; i <= pSymbolArray.SymbolCount - 1; i++)
            {
                ptSymbolClass pSymbolClass = null;
                ISymbol pTempSymbol = pSymbolArray.get_Symbol(i);
                if (pTempSymbol is ISimpleMarkerSymbol)
                {
                    pSymbolClass = new ptSimpleMarkerSymbolClass(pTempSymbol);
                }
                else if (pTempSymbol is ICharacterMarkerSymbol)
                {
                    pSymbolClass = new ptCharacterMarkerSymbolClass(pTempSymbol);
                }
                else if (pTempSymbol is IPictureMarkerSymbol)
                {
                    pSymbolClass = new ptPictureMarkerSymbolClass(pTempSymbol);
                }
                else if (pTempSymbol is IArrowMarkerSymbol)
                {
                    pSymbolClass = new ptArrowMarkerSymbolClass(pTempSymbol);
                }
                else if (pTempSymbol is IMultiLayerMarkerSymbol)
                {
                    pSymbolClass = new ptMultilayerMarkerSymbolClass(pTempSymbol);
                }
                if (pSymbolClass != null)
                {
                    SymbolList.Add(pSymbolClass);
                }
            }
        }
        /// <summary>
        /// 背景颜色
        /// </summary>
        public string BackgroundColor { get;}
        /// <summary>
        /// 背景透明度
        /// </summary>
        public byte BackgroundTransparency { get;}
        /// <summary>
        /// 点数量
        /// </summary>
        public int DotCount { get;}
        /// <summary>
        /// 点大小
        /// </summary>
        public double DotSize { get;}
        /// <summary>
        /// 点间隔
        /// </summary>
        public double DotSpacing { get;}
        /// <summary>
        /// 固定位置
        /// </summary>
        public bool FixedPlacement { get;}
        /// <summary>
        /// 符号列表
        /// </summary>
        public IList<ptSymbolClass> SymbolList { get;}
        /// <summary>
        /// 符号数量
        /// </summary>
        public int SymbolCount { get;}
        public override IList<XmlElement> GetSymbolNode(XmlDocument xmlDoc)
        {
            return base.GetSymbolNode(xmlDoc);
        }
    }
    /// <summary>
    /// 图片填充符号
    /// </summary>
    public class ptPictureFillSymbolClass : ptFillSymbolClass
    {
        public ptPictureFillSymbolClass(ISymbol pSymbol) 
            : base(pSymbol)
        {
            IPictureFillSymbol pPictureFillSymbol = pSymbol as IPictureFillSymbol;
            Angle = pPictureFillSymbol.Angle;
            BackgroundColor = CommStaticClass.GimmeStringForColor(pPictureFillSymbol.BackgroundColor);
            BackgroundTransparency = pPictureFillSymbol.BackgroundColor.Transparency;
            Picture = pPictureFillSymbol.Picture;
            XScale = pPictureFillSymbol.XScale;
            YScale = pPictureFillSymbol.YScale;
        }
        /// <summary>
        /// 角度
        /// </summary>
        public double Angle { get;}
        /// <summary>
        /// 背景颜色
        /// </summary>
        public string BackgroundColor { get;}
        /// <summary>
        /// 背景透明度
        /// </summary>
        public byte BackgroundTransparency { get;}
        /// <summary>
        /// 图片
        /// </summary>
        public IPictureDisp Picture { get;}
        /// <summary>
        /// X比例
        /// </summary>
        public double XScale { get;}
        /// <summary>
        /// Y比例
        /// </summary>
        public double YScale { get;}
        /// <summary>
        /// 获取SLD节点
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        public override IList<XmlElement> GetSymbolNode(XmlDocument xmlDoc)
        {
            IList<XmlElement> returenData = new List<XmlElement>();
            XmlElement pSymbolizerElement = base.GetSymbolNode(xmlDoc)[0] as XmlElement;
            if (pSymbolizerElement == null)
            {
                ptLogManager.WriteMessage("无法获取面符号节点");
                return null;
            }
            //先将图片保存到本地
            string imagefile = string.Empty;
            if (Picture != null)
            {
                string filePath = System.IO.Path.GetDirectoryName(CommXmlHandle.m_SaveFileName);
                imagefile = string.Format("{0}\\{1}.png", filePath, Guid.NewGuid().ToString());
                Image pimage = IPictureConverter.IPictureDispToImage(Picture);
                Graphics g = Graphics.FromImage(pimage);
                Bitmap pbitmap = new Bitmap(pimage);
                pbitmap.MakeTransparent(System.Drawing.Color.Black);
                pbitmap.Save(imagefile, System.Drawing.Imaging.ImageFormat.Png);
            }
            //Fill节点
            XmlElement pFillElment = CommXmlHandle.CreateElement("Fill", xmlDoc);
            //GraphicFill节点
            XmlElement pGraphFillElment = CommXmlHandle.CreateElement("PolygonGraphicFill", xmlDoc);
            pFillElment.AppendChild(pGraphFillElment);
            //Graphic节点
            XmlElement pGraphElment = CommXmlHandle.CreateElement("PolygonGraphic", xmlDoc);
            pGraphFillElment.AppendChild(pGraphElment);
            //ExternalGraphic节点 
            XmlElement pexternalGraphicElment = CommXmlHandle.CreateElement("PointExternalGraphic", xmlDoc);
            pGraphElment.AppendChild(pexternalGraphicElment);
            //OnlineResource节点
            XmlElement pOnlineElment = CommXmlHandle.CreateElement("PointOnlineResource", xmlDoc);
            CommXmlHandle.SetAttributeValue("http://www.w3.org/1999/xlink", CommXmlHandle.CreateAttribute("xmlns:xlink", pOnlineElment, xmlDoc));
            CommXmlHandle.SetAttributeValue("simple", CommXmlHandle.CreateAttribute("xlink:type", pOnlineElment, xmlDoc));
            CommXmlHandle.SetAttributeValue(imagefile, CommXmlHandle.CreateAttribute("xlink:href", pOnlineElment, xmlDoc));
            pexternalGraphicElment.AppendChild(pOnlineElment);
            //Format节点
            XmlElement pFormatElment = CommXmlHandle.CreateElementAndSetElemnetText("PointFormat", xmlDoc, "image/png");
            pexternalGraphicElment.AppendChild(pFormatElment);
            //写Size节点
            XmlElement pSizeElment = CommXmlHandle.CreateElementAndSetElemnetText("PolygonSize", xmlDoc, CommStaticClass.CommaToPoint(Picture.Height>Picture.Width?Picture.Width:Picture.Height));
            pGraphElment.AppendChild(pSizeElment);
            pSymbolizerElement.AppendChild(pFillElment);
            //外轮廓线节点
            XmlElement pLineEl = OutlineSymbol.GetSymbolNode(xmlDoc)[0].LastChild as XmlElement;
            pSymbolizerElement.AppendChild(pLineEl);

            returenData.Add(pSymbolizerElement);
            return returenData;
        }
    }
    /// <summary>
    /// 渐变填充符号(貌似SLD不支持渐变符号，这里直接用简单填充符号，取颜色的第一个颜色)
    /// </summary>
    public class ptGradientFillSymbolClass : ptFillSymbolClass
    {
        public ptGradientFillSymbolClass(ISymbol pSymbol) 
            : base(pSymbol)
        {
            IGradientFillSymbol pGradientFillSymbol = pSymbol as IGradientFillSymbol;
            Colors = CommStaticClass.GimmeArrayListForColorRamp(pGradientFillSymbol.ColorRamp);
            GradientAngle = pGradientFillSymbol.GradientAngle;
            GradientPercentage = pGradientFillSymbol.GradientPercentage;
            IntervallCount = pGradientFillSymbol.IntervalCount;
            Style = pGradientFillSymbol.Style.ToString();
        }
        /// <summary>
        /// 颜色列表
        /// </summary>
        public IList<string> Colors { get; }
        /// <summary>
        /// 角度
        /// </summary>
        public double GradientAngle { get; }
        /// <summary>
        /// 百分比
        /// </summary>
        public double GradientPercentage { get; }
        /// <summary>
        /// 间隔带
        /// </summary>
        public int IntervallCount { get; }
        /// <summary>
        /// 样式
        /// </summary>
        public string Style { get; }
        /// <summary>
        /// 获取SLD节点
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        public override IList<XmlElement> GetSymbolNode(XmlDocument xmlDoc)
        {
            IList<XmlElement> returenData = new List<XmlElement>();
            XmlElement pSymbolizerElement = base.GetSymbolNode(xmlDoc)[0] as XmlElement;
            if (pSymbolizerElement == null)
            {
                ptLogManager.WriteMessage("无法获取面符号节点");
                return null;
            }
            //填充节点
            XmlElement pFillElement = CommXmlHandle.CreateElement("Fill", xmlDoc);
            pSymbolizerElement.AppendChild(pFillElement);
            //外轮廓线节点
            XmlElement pLineEl = OutlineSymbol.GetSymbolNode(xmlDoc)[0].LastChild as XmlElement;
            pSymbolizerElement.AppendChild(pLineEl);
            returenData.Add(pSymbolizerElement);
            return returenData;
        }
    }
    #endregion

    #region 图表符号(SLD暂时不支持图表类型)
    /// <summary>
    /// 条形图符号
    /// </summary>
    public class ptBarChartSymbolClass : ptSymbolClass
    {
        public ptBarChartSymbolClass(ISymbol pSymbol) 
            : base(pSymbol)
        {
            IBarChartSymbol pBarChartSymbol = pSymbol as IBarChartSymbol;
            ShowAxes = pBarChartSymbol.ShowAxes;
            Spacing = pBarChartSymbol.Spacing;
            VerticalBars = pBarChartSymbol.VerticalBars;
            Width = pBarChartSymbol.Width;
            ptLineSymbolClass pLineSymboClass = null;
            ILineSymbol pLineSymbol = pBarChartSymbol.Axes;
            if (pLineSymbol is ICartographicLineSymbol)
            {
                pLineSymboClass = new ptCartographicLineSymbol(pLineSymbol as ISymbol);
            }
            else if (pLineSymbol is IMarkerLineSymbol)
            {
                pLineSymboClass = new ptMarkerLineSymbolClass(pLineSymbol as ISymbol);
            }
            else if (pLineSymbol is IHashLineSymbol)
            {
                pLineSymboClass = new ptHashLineSymbolClass(pLineSymbol as ISymbol);
            }
            else if (pLineSymbol is ISimpleLineSymbol)
            {
                pLineSymboClass = new ptSimpleLineSymbolClass(pLineSymbol as ISymbol);
            }
            else if (pLineSymbol is IPictureLineSymbol)
            {
                pLineSymboClass = new ptPictureLineSymbolClass(pLineSymbol as ISymbol);
            }
            else if (pLineSymbol is IMultiLayerLineSymbol)
            {
                pLineSymboClass = new ptMultilayerLineSymbolClass(pLineSymbol as ISymbol);
            }
            if (pLineSymboClass != null)
            {
                AxesLineSymbol = pLineSymboClass;
            }
        }
        /// <summary>
        /// 是否显示轴
        /// </summary>
        public bool ShowAxes { get; }
        /// <summary>
        /// 间隔
        /// </summary>
        public double Spacing { get; }
        /// <summary>
        /// 显示方向
        /// </summary>
        public bool VerticalBars { get; }
        /// <summary>
        /// 宽度
        /// </summary>
        public double Width { get; }
        /// <summary>
        /// 坐标轴线符号
        /// </summary>
        public ptLineSymbolClass AxesLineSymbol { get; }
    }
    /// <summary>
    /// 饼状图符号
    /// </summary>
    public class ptPieChartSymbolClass : ptSymbolClass
    {
        public ptPieChartSymbolClass(ISymbol pSymbol) 
            : base(pSymbol)
        {
            IPieChartSymbol pPieChartSymbol = pSymbol as IPieChartSymbol;
            Clockwise = pPieChartSymbol.Clockwise;
            UseOutline = pPieChartSymbol.UseOutline;
            ILineSymbol pLineSymbol = pPieChartSymbol.Outline;
            if (pLineSymbol is ICartographicLineSymbol)
            {
                OutLineSymbol = new ptCartographicLineSymbol(pLineSymbol as ISymbol);
            }
            else if (pLineSymbol is IMarkerLineSymbol)
            {
                OutLineSymbol = new ptMarkerLineSymbolClass(pLineSymbol as ISymbol);
            }
            else if (pLineSymbol is IHashLineSymbol)
            {
                OutLineSymbol = new ptHashLineSymbolClass(pLineSymbol as ISymbol);
            }
            else if (pLineSymbol is ISimpleLineSymbol)
            {
                OutLineSymbol = new ptSimpleLineSymbolClass(pLineSymbol as ISymbol);
            }
            else if (pLineSymbol is IPictureLineSymbol)
            {
                OutLineSymbol = new ptPictureLineSymbolClass(pLineSymbol as ISymbol);
            }
            else if (pLineSymbol is IMultiLayerLineSymbol)
            {
                OutLineSymbol = new ptMultilayerLineSymbolClass(pLineSymbol as ISymbol);
            }
        }
        /// <summary>
        /// 方向
        /// </summary>
        public bool Clockwise { get; }
        /// <summary>
        /// 轮廓
        /// </summary>
        public bool UseOutline { get; }
        /// <summary>
        /// 轮廓线符号
        /// </summary>
        public ptLineSymbolClass OutLineSymbol { get; }
    }
    /// <summary>
    /// 堆柱状符号
    /// </summary>
    public class ptStackedChartSymbolClass : ptSymbolClass
    {
        public ptStackedChartSymbolClass(ISymbol pSymbol) 
            : base(pSymbol)
        {
            IStackedChartSymbol pStackedChartSymbol = pSymbol as IStackedChartSymbol;
            Fixed = pStackedChartSymbol.Fixed;
            UseOutline = pStackedChartSymbol.UseOutline;
            VerticalBar = pStackedChartSymbol.VerticalBar;
            Width = pStackedChartSymbol.Width;
            ILineSymbol pOutLineSymbol = pStackedChartSymbol.Outline;
            if (pOutLineSymbol is ICartographicLineSymbol)
            {
                OutLineSymbol = new ptCartographicLineSymbol(pOutLineSymbol as ISymbol);
            }
            else if (pOutLineSymbol is IMarkerLineSymbol)
            {
                OutLineSymbol = new ptMarkerLineSymbolClass(pOutLineSymbol as ISymbol);
            }
            else if (pOutLineSymbol is IHashLineSymbol)
            {
                OutLineSymbol = new ptHashLineSymbolClass(pOutLineSymbol as ISymbol);
            }
            else if (pOutLineSymbol is ISimpleLineSymbol)
            {
                OutLineSymbol = new ptSimpleLineSymbolClass(pOutLineSymbol as ISymbol);
            }
            else if (pOutLineSymbol is IPictureLineSymbol)
            {
                OutLineSymbol = new ptPictureLineSymbolClass(pOutLineSymbol as ISymbol);
            }
            else if (pOutLineSymbol is IMultiLayerLineSymbol)
            {
                OutLineSymbol = new ptMultilayerLineSymbolClass(pOutLineSymbol as ISymbol);
            }
        }
        /// <summary>
        /// 确定的
        /// </summary>
        public bool Fixed { get; }
        /// <summary>
        /// 是否有轮廓
        /// </summary>
        public bool UseOutline { get; }
        /// <summary>
        /// 方向
        /// </summary>
        public bool VerticalBar { get; }
        /// <summary>
        /// 宽度
        /// </summary>
        public double Width { get; }
        /// <summary>
        /// 轮廓线符号
        /// </summary>
        public ptLineSymbolClass OutLineSymbol { get; }
    }
    #endregion

    #region 多图层符号
    /// <summary>
    ///多图层标记符号
    /// </summary>
    public class ptMultilayerMarkerSymbolClass : ptMarkerSymbolClass
    {
        public ptMultilayerMarkerSymbolClass(ISymbol pSymbol) 
            : base(pSymbol)
        {
            IMultiLayerMarkerSymbol pMultilayerMarkerSymbol = pSymbol as IMultiLayerMarkerSymbol;
            LayerCount = pMultilayerMarkerSymbol.LayerCount;
            MultiMarkerLayers = new List<ptSymbolClass>();
            for (int i = 0; i <= pMultilayerMarkerSymbol.LayerCount - 1; i++)
            {
                IMarkerSymbol pMarkerSymbol = pMultilayerMarkerSymbol.Layer[i];
                ptSymbolClass tempSymbol = null;
                if (pMarkerSymbol is ISimpleMarkerSymbol)
                {
                    tempSymbol = new ptSimpleMarkerSymbolClass(pMarkerSymbol as ISymbol);
                }
                else if (pMarkerSymbol is ICartographicMarkerSymbol)
                {
                    ICartographicMarkerSymbol ICMS = pMarkerSymbol as ICartographicMarkerSymbol;
                    if (ICMS is ICharacterMarkerSymbol)
                    {
                        tempSymbol = new ptCharacterMarkerSymbolClass(pMarkerSymbol as ISymbol);

                    }
                    else if (ICMS is IPictureMarkerSymbol)
                    {
                        tempSymbol = new ptPictureMarkerSymbolClass(pMarkerSymbol as ISymbol);

                    }
                }
                else if (pMarkerSymbol is IArrowMarkerSymbol)
                {
                    tempSymbol = new ptArrowMarkerSymbolClass(pMarkerSymbol as ISymbol);

                }
                else if (pMarkerSymbol is IMultiLayerMarkerSymbol)
                {
                    tempSymbol = new ptMultilayerMarkerSymbolClass(pMarkerSymbol as ISymbol);

                }
                MultiMarkerLayers.Add(tempSymbol);
            }
        }
        /// <summary>
        /// 符号列表
        /// </summary>
        public IList<ptSymbolClass> MultiMarkerLayers { get; }
        /// <summary>
        /// 图层数量
        /// </summary>
        public int LayerCount { get; }
        public override IList<XmlElement> GetSymbolNode(XmlDocument xmlDoc)
        {
            IList<XmlElement> returnData = new List<XmlElement>();
            foreach (ptSymbolClass symbol in MultiMarkerLayers)
            {
                returnData.Add(symbol.GetSymbolNode(xmlDoc)[0]);
            }
            return returnData;
        }
    }
    /// <summary>
    /// 多图层线符号
    /// </summary>
    public class ptMultilayerLineSymbolClass : ptLineSymbolClass
    {
        public ptMultilayerLineSymbolClass(ISymbol pSymbol) 
            : base(pSymbol)
        {
            IMultiLayerLineSymbol pMultiLayerLineSymbol = pSymbol as IMultiLayerLineSymbol;
            LayerCount = pMultiLayerLineSymbol.LayerCount;
            MultiLineSymbol = new List<ptLineSymbolClass>();
            for (int i = 0; i <= pMultiLayerLineSymbol.LayerCount - 1; i++)
            {
                ILineSymbol pLineSymbol = pMultiLayerLineSymbol.get_Layer(i);
                ptLineSymbolClass tempSymbol = null;
                if (pLineSymbol is ISimpleLineSymbol)
                {
                    tempSymbol = new ptSimpleLineSymbolClass(pLineSymbol as ISymbol);
                }
                else if (pLineSymbol is ICartographicLineSymbol)
                {
                    ICartographicLineSymbol ICLS = pLineSymbol as ICartographicLineSymbol;
                    if (ICLS is IHashLineSymbol)
                    {
                        tempSymbol = new ptHashLineSymbolClass(pLineSymbol as ISymbol);
                    }
                    else if (ICLS is IMarkerLineSymbol)
                    {
                        tempSymbol = new ptMarkerLineSymbolClass(pLineSymbol as ISymbol);
                    }
                    else
                    {
                        tempSymbol = new ptCartographicLineSymbol(pLineSymbol as ISymbol);
                    }
                }
                else if(pLineSymbol is IPictureLineSymbol)
                {
                     tempSymbol = new ptPictureLineSymbolClass(pLineSymbol as ISymbol);
                }
                else if (pLineSymbol is IMultiLayerLineSymbol)
                {
                    tempSymbol = new ptMultilayerLineSymbolClass(pLineSymbol as ISymbol);
                }
                MultiLineSymbol.Add(tempSymbol);
            }
        }
        /// <summary>
        /// 符号列表
        /// </summary>
        public IList<ptLineSymbolClass> MultiLineSymbol { get; }
        /// <summary>
        /// 图层数量
        /// </summary>
        public int LayerCount { get; }
        public override IList<XmlElement> GetSymbolNode(XmlDocument xmlDoc)
        {
            IList<XmlElement> returnData = new List<XmlElement>();
            foreach (ptSymbolClass symbol in MultiLineSymbol)
            {
                returnData.Add(symbol.GetSymbolNode(xmlDoc)[0]);
            }
            return returnData;
        }
    }
    /// <summary>
    /// 多图层填充符号
    /// </summary>
    public class ptMultilayerFillSymbolClass : ptFillSymbolClass
    {
        public ptMultilayerFillSymbolClass(ISymbol pSymbol) 
            : base(pSymbol)
        {
            IMultiLayerFillSymbol pMultiLayerFillSymbol = pSymbol as IMultiLayerFillSymbol;
            LayerCount = pMultiLayerFillSymbol.LayerCount;
            MultiFillSymbol = new List<ptFillSymbolClass>();
            for (int i = 0; i <= LayerCount - 1; i++)
            {
                ptFillSymbolClass pPtSymbolClass=null;
                IFillSymbol pFillSymbol = pMultiLayerFillSymbol.get_Layer(i);
                if (pFillSymbol is ISimpleFillSymbol)
                {
                    pPtSymbolClass = new ptSimpleFillSymbolClass(pFillSymbol as ISymbol);
                }
                else if (pFillSymbol is IMarkerFillSymbol)
                {
                    pPtSymbolClass = new ptMarkerFillSymbolClass(pFillSymbol as ISymbol);
                }
                else if (pFillSymbol is ILineFillSymbol)
                {
                    pPtSymbolClass = new ptLineFillSymbolClass(pFillSymbol as ISymbol);
                }
                else if (pFillSymbol is IPictureFillSymbol)
                {
                    pPtSymbolClass = new ptPictureFillSymbolClass(pFillSymbol as ISymbol);
                }
                else if (pFillSymbol is IDotDensityFillSymbol)
                {
                    pPtSymbolClass = new ptDotDensityFillSymbolClass(pFillSymbol as ISymbol);
                }
                else if (pFillSymbol is IGradientFillSymbol)
                {
                    pPtSymbolClass = new ptGradientFillSymbolClass(pFillSymbol as ISymbol);
                }
                else if (pFillSymbol is IMultiLayerFillSymbol)
                {
                    pPtSymbolClass = new ptMultilayerFillSymbolClass(pFillSymbol as ISymbol);
                }
                if (pPtSymbolClass != null)
                {
                    MultiFillSymbol.Add(pPtSymbolClass);
                }
            }
        }
        /// <summary>
        /// 符号列表
        /// </summary>
        public IList<ptFillSymbolClass> MultiFillSymbol { get; }
        /// <summary>
        /// 图层数量
        /// </summary>
        public int LayerCount { get; }
        public override IList<XmlElement> GetSymbolNode(XmlDocument xmlDoc)
        {
            IList<XmlElement> returnData = new List<XmlElement>();
            foreach (ptSymbolClass symbol in MultiFillSymbol)
            {
                returnData.Add(symbol.GetSymbolNode(xmlDoc)[0]);
            }
            return returnData;
        }
    }
    #endregion
    /// <summary>
    /// 装饰线符号
    /// </summary>
    public class ptSimpleLineDecorationClass 
    {
        public ptSimpleLineDecorationClass(ISimpleLineDecorationElement pSymbol)
        {
            if (pSymbol != null)
            {
                FlipAll = pSymbol.FlipAll;
                FilipFirst = pSymbol.FlipFirst;
                IMarkerSymbol pMarkSymbol = pSymbol.MarkerSymbol;
                if (pMarkSymbol is ISimpleMarkerSymbol)
                {
                    MarkerSymbol = new ptSimpleMarkerSymbolClass(pMarkSymbol as ISymbol);
                }
                else if (pMarkSymbol is ICharacterMarkerSymbol)
                {
                    MarkerSymbol = new ptCharacterMarkerSymbolClass(pMarkSymbol as ISymbol);
                }
                else if (pMarkSymbol is IPictureMarkerSymbol)
                {
                    MarkerSymbol = new ptPictureMarkerSymbolClass(pMarkSymbol as ISymbol);
                }
                else if (pMarkSymbol is IArrowMarkerSymbol)
                {
                    MarkerSymbol = new ptArrowMarkerSymbolClass(pMarkSymbol as ISymbol);
                }
                else if (pMarkSymbol is IMultiLayerMarkerSymbol)
                {
                    MarkerSymbol = new ptMultilayerMarkerSymbolClass(pMarkSymbol as ISymbol);
                }
                PositionCount = pSymbol.PositionCount;
            }
            if (PositionCount != 0)
            {
                ElementPosit = new List<double>();
                for (int i = 0; i < PositionCount; i++)
                {
                    ElementPosit.Add(pSymbol.Position[i]);
                }
            }

        }
        /// <summary>
        /// 反转所有点
        /// </summary>
        private bool FlipAll { get; }
        /// <summary>
        /// 反转第一个点
        /// </summary>
        private bool FilipFirst { get; }
        /// <summary>
        /// 点符号
        /// </summary>
        private ptMarkerSymbolClass MarkerSymbol { get; }
        /// <summary>
        /// 点符号个数
        /// </summary>
        private int PositionCount { get; }
        /// <summary>
        /// 点符号位置集合
        /// </summary>
        private IList<double> ElementPosit { get; }
        /// <summary>
        /// 获取装饰线的SLD节点
        /// </summary>
        /// <param name="xmlDoc"></param>
        /// <returns></returns>
        public IList<XmlElement> GetSymbolNode(XmlDocument xmlDoc)
        {
            IList<XmlElement> returnData = new List<XmlElement>();
            //目前SLD文件只支持线的头和尾的装饰符号，暂时不支持线中间的装饰符号
            for (int j = 0; j < PositionCount; j++)
            {
                XmlElement pPointSymbolEle = MarkerSymbol.GetSymbolNode(xmlDoc)[0];
                //Start Symbol
                if (ElementPosit[j] == 0.00)
                {
                    //添加Geometry节点
                    XmlElement pGeometryElement = CommXmlHandle.CreateElement("PointGeometry", xmlDoc);
                    XmlElement pFuncionElement = CommXmlHandle.CreateElement("PointFunction", xmlDoc);
                    CommXmlHandle.SetAttributeValue("startPoint", CommXmlHandle.CreateAttribute("name", pFuncionElement, xmlDoc));
                    XmlElement pPropertyNameElement = CommXmlHandle.CreateElementAndSetElemnetText("PropertyName", xmlDoc, "geom");
                    pFuncionElement.AppendChild(pPropertyNameElement);
                    pGeometryElement.AppendChild(pFuncionElement);
                    pPointSymbolEle.AppendChild(pGeometryElement);
                    //添加Rotation节点
                    XmlElement pRotationElement = CommXmlHandle.CreateElement("PointRotation", xmlDoc);
                    XmlElement pAddElement = CommXmlHandle.CreateElement("PointAdd", xmlDoc);
                    pRotationElement.AppendChild(pAddElement);
                    XmlElement pFuntionElment = CommXmlHandle.CreateElement("PointFunction", xmlDoc);
                    CommXmlHandle.SetAttributeValue("startAngle", CommXmlHandle.CreateAttribute("name", pFuntionElment, xmlDoc));
                    XmlElement pPropertyElement = CommXmlHandle.CreateElementAndSetElemnetText("PropertyName", xmlDoc, "geom");
                    pFuntionElment.AppendChild(pPropertyElement);
                    XmlElement pLiteralElemetn= CommXmlHandle.CreateElementAndSetElemnetText("Fieldvalue", xmlDoc, "-180");
                    pFuntionElment.AppendChild(pLiteralElemetn);
                    pAddElement.AppendChild(pFuntionElment);
                    pPointSymbolEle.AppendChild(pRotationElement);
                    returnData.Add(pPointSymbolEle);
                }
                //StopSymbol
                else if (ElementPosit[j] == 1.00)
                {
                    //添加Geometry节点
                    XmlElement pGeometryElement = CommXmlHandle.CreateElement("PointGeometry", xmlDoc);
                    XmlElement pFuncionElement = CommXmlHandle.CreateElement("PointFunction", xmlDoc);
                    CommXmlHandle.SetAttributeValue("endPoint", CommXmlHandle.CreateAttribute("name", pFuncionElement, xmlDoc));
                    XmlElement pPropertyNameElement = CommXmlHandle.CreateElementAndSetElemnetText("PropertyName", xmlDoc, "geom");
                    pFuncionElement.AppendChild(pPropertyNameElement);
                    pGeometryElement.AppendChild(pFuncionElement);
                    pPointSymbolEle.AppendChild(pGeometryElement);
                    //添加Rotation节点
                    XmlElement pRotationElement = CommXmlHandle.CreateElement("PointRotation", xmlDoc);
                    XmlElement pAddElement = CommXmlHandle.CreateElement("PointAdd", xmlDoc);
                    pRotationElement.AppendChild(pAddElement);
                    XmlElement pFuntionElment = CommXmlHandle.CreateElement("PointFunction", xmlDoc);
                    CommXmlHandle.SetAttributeValue("endAngle", CommXmlHandle.CreateAttribute("name", pFuntionElment, xmlDoc));
                    XmlElement pPropertyElement = CommXmlHandle.CreateElementAndSetElemnetText("PropertyName", xmlDoc, "geom");
                    pFuntionElment.AppendChild(pPropertyElement);
                    pAddElement.AppendChild(pFuntionElment);
                    pPointSymbolEle.AppendChild(pRotationElement);
                    returnData.Add(pPointSymbolEle);
                }
            }
            return returnData;
        }

    }
}
