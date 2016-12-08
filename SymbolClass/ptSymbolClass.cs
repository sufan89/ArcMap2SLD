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
    }
    /// <summary>
    /// 简单标记符号
    /// </summary>
    public class SimpleMarkerSymbolClass : ptMarkerSymbolClass
    {
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
    public class CharacterMarkerSymbolClass : ptMarkerSymbolClass
    {
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
    public class PictureMarkerSymbolClass : ptMarkerSymbolClass
    {
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
    public class ArrowMarkerSymbolClass : ptMarkerSymbolClass
    {
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
    public class SimpleLineSymbolClass : ptLineSymbolClass
    {
        /// <summary>
        /// 样式类型
        /// </summary>
        public string publicStyle { get; set; }
    }
    /// <summary>
    /// 制图线符号
    /// </summary>
    public class CartographicLineSymbol : ptLineSymbolClass
    {
        public string Join { get; set; }
        public double MiterLimit { get; set; }
        public string Cap { get; set; }
        public ArrayList DashArray { get; set; }
    }
    /// <summary>
    /// 混列线符号
    /// </summary>
    public class HashLineSymbolClass : ptLineSymbolClass
    {
        /// <summary>
        /// 角度
        /// </summary>
        public double Angle { get; set; }
        /// <summary>
        /// 混列线符号
        /// </summary>
        public ptLineSymbolClass HashSymbol { get; set; }
    }
    /// <summary>
    /// 标记线符号
    /// </summary>
    public class MarkerLineSymbolClass : ptLineSymbolClass
    {
        /// <summary>
        /// 标记符号
        /// </summary>
        public ptMarkerSymbolClass MarkSymbol { get; set; }
    }
    /// <summary>
    /// 图片线符号
    /// </summary>
    public class PictureLineSymbolClass : ptLineSymbolClass
    {
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
    public class SimpleFillSymbolClass : ptFillSymbolClass
    {
        /// <summary>
        /// 样式
        /// </summary>
        public string Style { get; set; }
    }
    /// <summary>
    /// 标记填充符号
    /// </summary>
    public class MarkerFillSymbolClass : ptFillSymbolClass
    {
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
    public class LineFillSymbolClass : ptFillSymbolClass
    {
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
    public class DotDensityFillSymbolClass : ptFillSymbolClass
    {
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
    public class PictureFillSymbolClass : ptFillSymbolClass
    {
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
        /// <summary>
        /// X偏移
        /// </summary>
        public double OffsetX { get; set; }
        /// <summary>
        /// Y偏移
        /// </summary>
        public double OffsetY { get; set; }
        /// <summary>
        /// 间隔X
        /// </summary>
        public double SeparationX { get; set; }
        /// <summary>
        /// 间隔Y
        /// </summary>
        public double SeparationY { get; set; }

    }
    /// <summary>
    /// 渐变填充符号
    /// </summary>
    public class GradientFillSymbolClass : ptFillSymbolClass
    {
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
    public class BarChartSymbolClass : ptSymbolClass
    {
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
    public class PieChartSymbolClass : ptSymbolClass
    {
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
    public class StackedChartSymbolClass : ptSymbolClass
    {
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
    /// 多图层符号
    /// </summary>
    public class MultilayerSymbolClass : ptSymbolClass
    {
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
    ///多图层标记符号
    /// </summary>
    public class MultilayerMarkerSymbolClass : MultilayerSymbolClass
    {

    }
    /// <summary>
    /// 多图层线符号
    /// </summary>
    public class MultilayerLineSymbolClass : MultilayerSymbolClass
    {

    }
    /// <summary>
    /// 多图层填充符号
    /// </summary>
    public class MultilayerFillSymbolClass : MultilayerSymbolClass
    {

    }
    #endregion
}
