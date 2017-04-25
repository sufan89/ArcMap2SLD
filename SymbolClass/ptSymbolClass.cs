using ESRI.ArcGIS.Display;
using stdole;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public double Angle { get; set; }
        /// <summary>
        /// 颜色
        /// </summary>
        public string Color { get; set; }
        /// <summary>
        /// 字体
        /// </summary>
        public string Font { get; set; }
        /// <summary>
        /// 样式
        /// </summary>
        public string Style { get; set; }
        /// <summary>
        /// 粗细
        /// </summary>
        public string Weight { get; set; }
        /// <summary>
        /// 水平对齐
        /// </summary>
        public string HorizontalAlignment { get; set; }
        /// <summary>
        /// 是否对齐
        /// </summary>
        public bool RightToLeft { get; set; }
        /// <summary>
        ///大小 
        /// </summary>
        public double Size { get; set; }
        /// <summary>
        /// 文本
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// 垂直对齐
        /// </summary>
        public string VerticalAlignment { get; set; }
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
            Size = pMarkerSymbol.Size;
            XOffset = pMarkerSymbol.XOffset;
            YOffset = pMarkerSymbol.YOffset;
            Filled = pMarkerSymbol.Color.Transparency != 0;
        }
        /// <summary>
        /// 角度
        /// </summary>
        public double Angle { get; set; }
        /// <summary>
        /// 颜色
        /// </summary>
        public string Color { get; set; }
        /// <summary>
        /// 大小
        /// </summary>
        public double Size { get; set; }
        /// <summary>
        /// X偏移量
        /// </summary>
        public double XOffset { get; set; }
        /// <summary>
        /// Y偏移量
        /// </summary>
        public double YOffset { get; set; }
        /// <summary>
        /// 是否填充
        /// </summary>
        public bool Filled { get; set; }
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
            OutlineSize = pSimpleMarkerSymbol.OutlineSize;
            Style = pSimpleMarkerSymbol.Style.ToString();
        }
        /// <summary>
        /// 是否有边框线
        /// </summary>
        public bool Outline { get; set; }
        /// <summary>
        /// 边框线颜色
        /// </summary>
        public string OutlineColor;
        /// <summary>
        /// 边框线尺寸
        /// </summary>
        public double OutlineSize;
        /// <summary>
        /// 样式
        /// </summary>
        public string Style;

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
        public string BackgroundColor { get; set; }
        /// <summary>
        /// 图片对象
        /// </summary>
        public IPicture Picture { get; set; }
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
            Width = pArrowMarkerSymbol.Width;
            Length = pArrowMarkerSymbol.Length;
        }
        /// <summary>
        /// 样式
        /// </summary>
        public string Style { get; set; }
        /// <summary>
        /// 宽度
        /// </summary>
        public double Width { get; set; }
        /// <summary>
        /// 长度
        /// </summary>
        public double Length { get; set; }
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
            Width = pLineSymbol.Width;
            Transparency = pLineSymbol.Color.Transparency; 
        }
        /// <summary>
        /// 颜色
        /// </summary>
        public string Color { get; set; }
        /// <summary>
        /// 透明度
        /// </summary>
        public byte Transparency { get; set; }
        /// <summary>
        /// 宽度
        /// </summary>
        public double Width { get; set; }
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
            publicStyle = pSimpLineSymbol.Style.ToString();
            if (pSimpLineSymbol.Style == esriSimpleLineStyle.esriSLSNull)
            {
                Color = string.Empty;
            }
        }
        /// <summary>
        /// 样式类型
        /// </summary>
        public string publicStyle { get; set; }
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
            Join = pCartographicLineSymbol.Join.ToString();
            MiterLimit = pCartographicLineSymbol.MiterLimit;
            Cap = pCartographicLineSymbol.Cap.ToString();
            DashArray = new List<double>();
            if (pCartographicLineSymbol is ILineProperties)
            {
                ILineProperties lineProperties = pCartographicLineSymbol as ILineProperties;
                double markLen = 0;
                double gapLen = 0;
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
            }
        }
        public string Join { get; set; }
        public double MiterLimit { get; set; }
        public string Cap { get; set; }
        public IList<double> DashArray { get; set; }
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

        }
        /// <summary>
        /// 角度
        /// </summary>
        public double Angle { get; set; }
        /// <summary>
        /// 混列线符号
        /// </summary>
        public ptSymbolClass HashSymbol { get; set; }
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
        }
        /// <summary>
        /// 标记符号
        /// </summary>
        public ptSymbolClass MarkSymbol { get; set; }
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
        public string BackgroundColor { get; set; }
        /// <summary>
        /// 背景透明度
        /// </summary>
        public byte BackgroundTransparency { get; set; }
        /// <summary>
        /// 图片
        /// </summary>
        public IPicture Picture { get; set; }
        /// <summary>
        /// 是否旋转
        /// </summary>
        public bool Rotate { get; set; }
        /// <summary>
        /// X比例
        /// </summary>
        public double XScale { get; set; }
        /// <summary>
        /// Y比例
        /// </summary>
        public double YScale { get; set; }
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
        public byte Transparency { get; set; }
        /// <summary>
        /// 轮廓符号
        /// </summary>
        public ptLineSymbolClass OutlineSymbol { get; set; }
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
        }
        /// <summary>
        /// 网格角度
        /// </summary>
        public double GridAngle { get; set; }
        /// <summary>
        /// 标记符号
        /// </summary>
        public ptMarkerSymbolClass MarkerSymbol { get; set; }
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
            Offset = pLineFillSymbol.Offset;
            Separation = pLineFillSymbol.Separation;
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
        public double Angle { get; set; }
        /// <summary>
        /// 偏移
        /// </summary>
        public double Offset { get; set; }
        /// <summary>
        /// 间隔
        /// </summary>
        public double Separation { get; set; }
        /// <summary>
        /// 填充线符号
        /// </summary>
        public ptLineSymbolClass LineSymbol { get; set; }
    }
    /// <summary>
    /// 点密度填充符号
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
        public string BackgroundColor { get; set; }
        /// <summary>
        /// 背景透明度
        /// </summary>
        public byte BackgroundTransparency { get; set; }
        /// <summary>
        /// 点数量
        /// </summary>
        public int DotCount { get; set; }
        /// <summary>
        /// 点大小
        /// </summary>
        public double DotSize { get; set; }
        /// <summary>
        /// 点间隔
        /// </summary>
        public double DotSpacing { get; set; }
        /// <summary>
        /// 固定位置
        /// </summary>
        public bool FixedPlacement { get; set; }
        /// <summary>
        /// 符号列表
        /// </summary>
        public IList<ptSymbolClass> SymbolList { get; set; }
        /// <summary>
        /// 符号数量
        /// </summary>
        public int SymbolCount { get; set; }
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
        public double Angle { get; set; }
        /// <summary>
        /// 背景颜色
        /// </summary>
        public string BackgroundColor { get; set; }
        /// <summary>
        /// 背景透明度
        /// </summary>
        public byte BackgroundTransparency { get; set; }
        /// <summary>
        /// 图片
        /// </summary>
        public IPictureDisp Picture { get; set; }
        /// <summary>
        /// X比例
        /// </summary>
        public double XScale { get; set; }
        /// <summary>
        /// Y比例
        /// </summary>
        public double YScale { get; set; }
    }
    /// <summary>
    /// 渐变填充符号
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
        public IList<string> Colors;
        /// <summary>
        /// 角度
        /// </summary>
        public double GradientAngle;
        /// <summary>
        /// 百分比
        /// </summary>
        public double GradientPercentage;
        /// <summary>
        /// 间隔带
        /// </summary>
        public int IntervallCount;
        /// <summary>
        /// 样式
        /// </summary>
        public string Style;
    }
    #endregion

    #region 图表符号
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
        public bool ShowAxes { get; set; }
        /// <summary>
        /// 间隔
        /// </summary>
        public double Spacing{ get; set; }
        /// <summary>
        /// 显示方向
        /// </summary>
        public bool VerticalBars { get; set; }
        /// <summary>
        /// 宽度
        /// </summary>
        public double Width { get; set; }
        /// <summary>
        /// 坐标轴线符号
        /// </summary>
        public ptLineSymbolClass AxesLineSymbol { get; set; }
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
        public bool Clockwise { get; set; }
        /// <summary>
        /// 轮廓
        /// </summary>
        public bool UseOutline { get; set; }
        /// <summary>
        /// 轮廓线符号
        /// </summary>
        public ptLineSymbolClass OutLineSymbol { get; set; }
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
        public bool Fixed { get; set; }
        /// <summary>
        /// 是否有轮廓
        /// </summary>
        public bool UseOutline { get; set; }
        /// <summary>
        /// 方向
        /// </summary>
        public bool VerticalBar { get; set; }
        /// <summary>
        /// 宽度
        /// </summary>
        public double Width { get; set; }
        /// <summary>
        /// 轮廓线符号
        /// </summary>
        public ptLineSymbolClass OutLineSymbol { get; set; }
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
        public IList<ptSymbolClass> MultiMarkerLayers { get; set; }
        /// <summary>
        /// 图层数量
        /// </summary>
        public int LayerCount { get; set; }
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
                    tempSymbol = new ptCartographicLineSymbol(pLineSymbol as ISymbol);
                }
                else if (pLineSymbol is IHashLineSymbol)
                {
                    tempSymbol = new ptHashLineSymbolClass(pLineSymbol as ISymbol);

                }
                else if (pLineSymbol is IMarkerLineSymbol)
                {
                    tempSymbol = new ptMarkerLineSymbolClass(pLineSymbol as ISymbol);

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
        public IList<ptLineSymbolClass> MultiLineSymbol { get; set; }
        /// <summary>
        /// 图层数量
        /// </summary>
        public int LayerCount { get; set; }
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
        public IList<ptFillSymbolClass> MultiFillSymbol { get; set; }
        /// <summary>
        /// 图层数量
        /// </summary>
        public int LayerCount { get; set; }
    }
    #endregion
}
