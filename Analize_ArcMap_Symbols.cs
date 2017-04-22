using System;
using System.Drawing;
using System.Diagnostics;
using System.Data;
using System.Collections;
using System.Windows.Forms;
using ESRI.ArcGIS.Framework;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using stdole;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using System.IO;
using Microsoft.VisualBasic.CompilerServices;
using System.Collections.Generic;

namespace ArcGIS_SLD_Converter
{
    public class Analize_ArcMap_Symbols
    {
        #region 全局变量
        /// <summary>
        /// 地图文档
        /// </summary>
        private IMxDocument m_ObjDoc;
        /// <summary>
        /// 当前运行的ArcMap程序中的地图对象
        /// </summary>
        private IMap m_ObjMap;
        /// <summary>
        /// 主窗体
        /// </summary>
        private Motherform frmMotherform;
        /// <summary>
        /// 工程对象
        /// </summary>
        internal ProjectClass m_StrProject;

        private ArrayList m_al1;
        private ArrayList m_al2;
        private ArrayList m_al3;
        /// <summary>
        /// 所有分类字段值
        /// </summary>
        private ArrayList m_alClassifiedFields;
        /// <summary>
        /// SLD文件路径
        /// </summary>
        private string m_cFilename;
        #endregion

        #region 枚举类型
        /// <summary>
        /// 图层要素类型
        /// </summary>
        internal enum FeatureClass
        {
            /// <summary>
            /// 点
            /// </summary>
			PointFeature = 0,
            /// <summary>
            /// 线
            /// </summary>
			LineFeature = 1,
            /// <summary>
            /// 面
            /// </summary>
			PolygonFeature = 2
        }
        /// <summary>
        /// 标记符号类型
        /// </summary>
        internal enum MarkerStructs
        {
            StructSimpleMarkerSymbol,
            StructCharacterMarkerSymbol,
            StructPictureMarkerSymbol,
            StructArrowMarkerSymbol,
            StructMultilayerMarkerSymbol
        }
        /// <summary>
        /// 线符号类型
        /// </summary>
        internal enum LineStructs
        {
            StructSimpleLineSymbol = 0,
            StructMarkerLineSymbol = 1,
            StructHashLineSymbol = 2,
            StructPictureLineSymbol = 3,
            StructMultilayerLineSymbol = 4,
            StructCartographicLineSymbol = 5
        }
        #endregion

        #region 符号和渲染方式结构体

        /// <summary>
        /// 分析的图层信息
        /// </summary>
        internal struct StructProject
        {
            public ArrayList LayerList;
            public int LayerCount;
        }
        /// <summary>
        /// 唯一值渲染方式
        /// </summary>
		internal struct StructUniqueValueRenderer
        {
            public FeatureClass FeatureCls;
            public string LayerName;
            public string DatasetName;
            public int ValueCount;
            public ArrayList SymbolList;
            public int FieldCount;
            public ArrayList FieldNames;
            public string StylePath;
            public StructAnnotation Annotation;
        }
        internal struct StructClassBreaksRenderer
        {
            public FeatureClass FeatureCls;
            public string LayerName;
            public string DatasetName;
            public int BreakCount;
            public string FieldName;
            public string NormFieldName;
            public ArrayList SymbolList;
            public StructAnnotation Annotation;
        }
        internal struct StructSimpleRenderer
        {
            public FeatureClass FeatureCls;
            public string LayerName;
            public string DatasetName;
            public ArrayList SymbolList;
            public StructAnnotation Annotation;
        }

        internal struct StructSimpleMarkerSymbol
        {
            public double Angle;
            public bool Filled;
            public string Color;
            public bool Outline;
            public string OutlineColor;
            public double OutlineSize;
            public double Size;
            public string Style;
            public double XOffset;
            public double YOffset;
            public string Label;
            public ArrayList Fieldvalues;
            public double UpperLimit;
            public double LowerLimit;
        }
        internal struct StructCharacterMarkerSymbol
        {
            public double Angle;
            public int CharacterIndex;
            public string Color;
            public string Font;
            public double Size;
            public double XOffset;
            public double YOffset;
            public string Label;
            public ArrayList Fieldvalues;
            public double UpperLimit;
            public double LowerLimit;
        }
        internal struct StructPictureMarkerSymbol
        {
            public double Angle;
            public string BackgroundColor;
            public string Color;
            public IPicture Picture;
            public double Size;
            public double XOffset;
            public double YOffset;
            public string Label;
            public ArrayList Fieldvalues;
            public double UpperLimit;
            public double LowerLimit;
        }
        internal struct StructArrowMarkerSymbol
        {
            public double Angle;
            public string Color;
            public double Length;
            public double Size;
            public string Style;
            public double Width;
            public double XOffset;
            public double YOffset;
            public string Label;
            public ArrayList Fieldvalues;
            public double UpperLimit;
            public double LowerLimit;
        }
        internal struct StructSimpleLineSymbol
        {
            public string Color;
            public byte Transparency;
            public string publicStyle;
            public double Width;
            public string Label;
            public ArrayList Fieldvalues;
            public double UpperLimit;
            public double LowerLimit;
        }
        internal struct StructCartographicLineSymbol
        {
            public string Color;
            public byte Transparency;
            public double Width;
            public string Join;
            public double MiterLimit;
            public string Cap;
            public ArrayList DashArray;
            public string Label;
            public ArrayList Fieldvalues;
            public double UpperLimit;
            public double LowerLimit;
        }
        internal struct StructHashLineSymbol
        {
            public double Angle;
            public string Color;
            public byte Transparency;
            public double Width;
            public LineStructs kindOfLineStruct;
            public StructSimpleLineSymbol HashSymbol_SimpleLine;
            public StructCartographicLineSymbol HashSymbol_CartographicLine;
            public StructMarkerLineSymbol HashSymbol_MarkerLine;
            public StructPictureLineSymbol HashSymbol_PictureLine;
            public StructMultilayerLineSymbol HashSymbol_MultiLayerLines;
            public string Label;
            public ArrayList Fieldvalues;
            public double UpperLimit;
            public double LowerLimit;
        }
        internal struct StructMarkerLineSymbol
        {
            public string Color;
            public byte Transparency;
            public double Width;
            public MarkerStructs kindOfMarkerStruct;
            public StructSimpleMarkerSymbol MarkerSymbol_SimpleMarker;
            public StructCharacterMarkerSymbol MarkerSymbol_CharacterMarker;
            public StructPictureMarkerSymbol MarkerSymbol_PictureMarker;
            public StructArrowMarkerSymbol MarkerSymbol_ArrowMarker;
            public StructMultilayerMarkerSymbol MarkerSymbol_MultilayerMarker;
            public string Label;
            public ArrayList Fieldvalues;
            public double UpperLimit;
            public double LowerLimit;
        }
        internal struct StructPictureLineSymbol
        {
            public string BackgroundColor;
            public byte BackgroundTransparency;
            public string Color;
            public byte Transparency;
            public double Offset;
            public IPicture Picture;
            public bool Rotate;
            public double Width;
            public double XScale;
            public double YScale;
            public string Label;
            public ArrayList Fieldvalues;
            public double UpperLimit;
            public double LowerLimit;
        }
        internal struct StructSimpleFillSymbol
        {
            public string Color;
            public string Style;
            public byte Transparency;
            public LineStructs kindOfOutlineStruct;
            public StructSimpleLineSymbol Outline_SimpleLine;
            public StructCartographicLineSymbol Outline_CartographicLine;
            public StructMarkerLineSymbol Outline_MarkerLine;
            public StructHashLineSymbol Outline_HashLine;
            public StructPictureLineSymbol Outline_PictureLine;
            public StructMultilayerLineSymbol Outline_MultiLayerLines;
            public string Label;
            public ArrayList Fieldvalues;
            public double UpperLimit;
            public double LowerLimit;
        }
        internal struct StructMarkerFillSymbol
        {
            public string Color;
            public byte Transparency;
            public double GridAngle;
            public MarkerStructs kindOfMarkerStruct;
            public StructSimpleMarkerSymbol MarkerSymbol_SimpleMarker;
            public StructCharacterMarkerSymbol MarkerSymbol_CharacterMarker;
            public StructPictureMarkerSymbol MarkerSymbol_PictureMarker;
            public StructArrowMarkerSymbol MarkerSymbol_ArrowMarker;
            public StructMultilayerMarkerSymbol MarkerSymbol_MultilayerMarker;
            public LineStructs kindOfOutlineStruct;
            public StructSimpleLineSymbol Outline_SimpleLine;
            public StructCartographicLineSymbol Outline_CartographicLine;
            public StructMarkerLineSymbol Outline_MarkerLine;
            public StructHashLineSymbol Outline_HashLine;
            public StructPictureLineSymbol Outline_PictureLine;
            public StructMultilayerLineSymbol Outline_MultiLayerLines;
            public string Label;
            public ArrayList Fieldvalues;
            public double UpperLimit;
            public double LowerLimit;
        }
        internal struct StructLineFillSymbol
        {
            public double Angle;
            public string Color;
            public byte Transparency;
            public double Offset;
            public double Separation;
            public LineStructs kindOfLineStruct;
            public StructSimpleLineSymbol LineSymbol_SimpleLine;
            public StructCartographicLineSymbol LineSymbol_CartographicLine;
            public StructMarkerLineSymbol LineSymbol_MarkerLine;
            public StructHashLineSymbol LineSymbol_HashLine;
            public StructPictureLineSymbol LineSymbol_PictureLine;
            public StructMultilayerLineSymbol LineSymbol_MultiLayerLines;
            public LineStructs kindOfOutlineStruct;
            public StructSimpleLineSymbol Outline_SimpleLine;
            public StructCartographicLineSymbol Outline_CartographicLine;
            public StructMarkerLineSymbol Outline_MarkerLine;
            public StructHashLineSymbol Outline_HashLine;
            public StructPictureLineSymbol Outline_PictureLine;
            public StructMultilayerLineSymbol Outline_MultiLayerLines;
            public string Label;
            public ArrayList Fieldvalues;
            public double UpperLimit;
            public double LowerLimit;
        }
        internal struct StructDotDensityFillSymbol
        {
            public string BackgroundColor;
            public byte BackgroundTransparency;
            public string Color;
            public byte Transparency;
            public int DotCount;
            public double DotSize;
            public double DotSpacing;
            public bool FixedPlacement;
            public ArrayList SymbolList;
            public int SymbolCount;
            public LineStructs kindOfOutlineStruct;
            public StructSimpleLineSymbol Outline_SimpleLine;
            public StructCartographicLineSymbol Outline_CartographicLine;
            public StructMarkerLineSymbol Outline_MarkerLine;
            public StructHashLineSymbol Outline_HashLine;
            public StructPictureLineSymbol Outline_PictureLine;
            public StructMultilayerLineSymbol Outline_MultiLayerLines;
            public string Label;
            public ArrayList Fieldvalues;
            public double UpperLimit;
            public double LowerLimit;
        }
        internal struct StructPictureFillSymbol
        {
            public double Angle;
            public string BackgroundColor;
            public byte BackgroundTransparency;
            public string Color;
            public byte Transparency;
            public IPictureDisp Picture;
            public double XScale;
            public double YScale;
            public LineStructs kindOfOutlineStruct;
            public StructSimpleLineSymbol Outline_SimpleLine;
            public StructCartographicLineSymbol Outline_CartographicLine;
            public StructMarkerLineSymbol Outline_MarkerLine;
            public StructHashLineSymbol Outline_HashLine;
            public StructPictureLineSymbol Outline_PictureLine;
            public StructMultilayerLineSymbol Outline_MultiLayerLines;
            public string Label;
            public ArrayList Fieldvalues;
            public double UpperLimit;
            public double LowerLimit;
        }
        internal struct StructGradientFillSymbol
        {
            public string Color;
            public byte Transparency;
            public ArrayList Colors;
            public double GradientAngle;
            public double GradientPercentage;
            public int IntervallCount;
            public string Style;
            public LineStructs kindOfOutlineStruct;
            public StructSimpleLineSymbol Outline_SimpleLine;
            public StructCartographicLineSymbol Outline_CartographicLine;
            public StructMarkerLineSymbol Outline_MarkerLine;
            public StructHashLineSymbol Outline_HashLine;
            public StructPictureLineSymbol Outline_PictureLine;
            public StructMultilayerLineSymbol Outline_MultiLayerLines;
            public string Label;
            public ArrayList Fieldvalues;
            public double UpperLimit;
            public double LowerLimit;
        }
        internal struct StructBarChartSymbol
        {
            public bool ShowAxes;
            public double Spacing;
            public bool VerticalBars;
            public double Width;
            public LineStructs kindOfAxeslineStruct;
            public StructSimpleLineSymbol Axes_SimpleLine;
            public StructCartographicLineSymbol Axes_CartographicLine;
            public StructMarkerLineSymbol Axes_MarkerLine;
            public StructHashLineSymbol Axes_HashLine;
            public StructPictureLineSymbol Axes_PictureLine;
            public StructMultilayerLineSymbol Axes_MultiLayerLines;
            public string Label;
            public ArrayList Fieldvalues;
            public double UpperLimit;
            public double LowerLimit;
        }
        internal struct StructPieChartSymbol
        {
            public bool Clockwise;
            public bool UseOutline;
            public LineStructs kindOfOutlineStruct;
            public StructSimpleLineSymbol Outline_SimpleLine;
            public StructCartographicLineSymbol Outline_CartographicLine;
            public StructMarkerLineSymbol Outline_MarkerLine;
            public StructHashLineSymbol Outline_HashLine;
            public StructPictureLineSymbol Outline_PictureLine;
            public StructMultilayerLineSymbol Outline_MultiLayerLines;
            public string Label;
            public ArrayList Fieldvalues;
            public double UpperLimit;
            public double LowerLimit;
        }
        internal struct StructStackedChartSymbol
        {
            public bool Fixed;
            public bool UseOutline;
            public bool VerticalBar;
            public double Width;
            public LineStructs kindOfOutlineStruct;
            public StructSimpleLineSymbol Outline_SimpleLine;
            public StructCartographicLineSymbol Outline_CartographicLine;
            public StructMarkerLineSymbol Outline_MarkerLine;
            public StructHashLineSymbol Outline_HashLine;
            public StructPictureLineSymbol Outline_PictureLine;
            public StructMultilayerLineSymbol Outline_MultiLayerLines;
            public string Label;
            public ArrayList Fieldvalues;
            public double UpperLimit;
            public double LowerLimit;
        }
        internal struct StructTextSymbol
        {
            public double Angle;
            public string Color;
            public string Font;
            public string Style;
            public string Weight;
            public string HorizontalAlignment;
            public bool RightToLeft;
            public double Size;
            public string Text;
            public string VerticalAlignment;
            public string Label;
            public ArrayList Fieldvalues;
            public double UpperLimit;
            public double LowerLimit;
        }
        internal struct StructMultilayerMarkerSymbol
        {
            public ArrayList MultiMarkerLayers;
            public int LayerCount;
            public string Label;
            public ArrayList Fieldvalues;
            public double UpperLimit;
            public double LowerLimit;
        }
        internal struct StructMultilayerLineSymbol
        {
            public ArrayList MultiLineLayers;
            public int LayerCount;
            public string Label;
            public ArrayList Fieldvalues;
            public double UpperLimit;
            public double LowerLimit;
        }
        internal struct StructMultilayerFillSymbol
        {
            public ArrayList MultiFillLayers;
            public int LayerCount;
            public string Label;
            public ArrayList Fieldvalues;
            public double UpperLimit;
            public double LowerLimit;
        }
        internal struct StructAnnotation
        {
            public bool IsSingleProperty;
            public string PropertyName;
            public StructTextSymbol TextSymbol;
        }
        #endregion


        #region 主要处理函数 
        /// <summary>
        /// 初始化函数
        /// </summary>
        /// <param name="value">主窗体</param>
        /// <param name="Filename">保存文件路径</param>
        public Analize_ArcMap_Symbols(Motherform value, string Filename,IMxDocument mxDoc)
        {
            m_cFilename = Filename;
            frmMotherform = value;
            m_ObjDoc = mxDoc;
            CentralProcessingFunc();
        }

        #endregion

        #region 属性信息

        /// <summary>
        /// 获取项目信息
        /// </summary>
        public ProjectClass GetProjectData
        {
            get
            {
                return m_StrProject;
            }
        }
        #endregion

        #region 符号解析
        /// <summary>
        /// 简单渲染方式
        /// </summary>
        /// <param name="Renderer">渲染方式</param>
        /// <param name="Layer">图层</param>
        /// <returns></returns>
        private StructSimpleRenderer StoreStructSimpleRenderer(ISimpleRenderer Renderer, IFeatureLayer Layer)
        {
            StructSimpleRenderer strRenderer = new StructSimpleRenderer();
            ISymbol objFstOrderSymbol = Renderer.Symbol;
            IDataset objDataset = Layer.FeatureClass as IDataset;
            strRenderer.SymbolList = new ArrayList();
            try
            {
                strRenderer.LayerName = Layer.Name;
                strRenderer.DatasetName = objDataset.Name;
                strRenderer.Annotation = GetAnnotation(Layer);
                //注记
                if (objFstOrderSymbol is ITextSymbol)
                {
                    StructTextSymbol strTS = new StructTextSymbol();
                    ITextSymbol objSymbol = objFstOrderSymbol as ITextSymbol;
                    strTS = StoreText(objSymbol);
                    strTS.Label = Renderer.Label;
                    strRenderer.SymbolList.Add(strTS);

                }
                #region 点符号
                if (objFstOrderSymbol is IMarkerSymbol)
                {
                    strRenderer.FeatureCls = FeatureClass.PointFeature;
                    IMarkerSymbol objSymbol = objFstOrderSymbol as IMarkerSymbol;
                    switch (MarkerSymbolScan(objSymbol))
                    {
                        case "ISimpleMarkerSymbol":
                            ISimpleMarkerSymbol SMS = objSymbol as ISimpleMarkerSymbol;
                            StructSimpleMarkerSymbol strSMS = new StructSimpleMarkerSymbol();
                            strSMS = StoreSimpleMarker(SMS);
                            strSMS.Label = Renderer.Label;
                            strRenderer.SymbolList.Add(strSMS);
                            break;
                        case "ICharacterMarkerSymbol":
                            ICharacterMarkerSymbol CMS = objSymbol as ICharacterMarkerSymbol;
                            StructCharacterMarkerSymbol strCMS = new StructCharacterMarkerSymbol();
                            strCMS = StoreCharacterMarker(CMS);
                            strCMS.Label = Renderer.Label;
                            strRenderer.SymbolList.Add(strCMS);
                            break;
                        case "IPictureMarkerSymbol":
                            IPictureMarkerSymbol PMS = objSymbol as IPictureMarkerSymbol;
                            StructPictureMarkerSymbol strPMS = new StructPictureMarkerSymbol();
                            strPMS = StorePictureMarker(PMS);
                            strPMS.Label = Renderer.Label;
                            strRenderer.SymbolList.Add(strPMS);
                            break;
                        case "IArrowMarkerSymbol":
                            IArrowMarkerSymbol AMS = objSymbol as IArrowMarkerSymbol;
                            StructArrowMarkerSymbol strAMS = new StructArrowMarkerSymbol();
                            strAMS = StoreArrowMarker(AMS);
                            strAMS.Label = Renderer.Label;
                            strRenderer.SymbolList.Add(strAMS);
                            break;
                        case "IMultiLayerMarkerSymbol":
                            IMultiLayerMarkerSymbol MLMS = objSymbol as IMultiLayerMarkerSymbol;
                            StructMultilayerMarkerSymbol strMLMS = new StructMultilayerMarkerSymbol();
                            strMLMS = StoreMultiLayerMarker(MLMS);
                            strMLMS.Label = Renderer.Label;
                            strRenderer.SymbolList.Add(strMLMS);
                            break;
                        case "false":
                            InfoMsg("未知点符号", "StoreStructLayer");
                            break;
                    }
                }
                #endregion

                #region 线符号
                if (objFstOrderSymbol is ILineSymbol)
                {
                    strRenderer.FeatureCls = FeatureClass.LineFeature;
                    ILineSymbol objSymbol = objFstOrderSymbol as ILineSymbol;
                    switch (LineSymbolScan(objSymbol))
                    {
                        case "ICartographicLineSymbol":
                            ICartographicLineSymbol CLS = objSymbol as ICartographicLineSymbol;
                            StructCartographicLineSymbol strCLS = new StructCartographicLineSymbol();
                            strCLS = StoreCartographicLine(CLS);
                            strCLS.Label = Renderer.Label;
                            strRenderer.SymbolList.Add(strCLS);
                            break;
                        case "IHashLineSymbol":
                            IHashLineSymbol HLS = objSymbol as IHashLineSymbol;
                            StructHashLineSymbol strHLS = new StructHashLineSymbol();
                            strHLS = StoreHashLine(HLS);
                            strHLS.Label = Renderer.Label;
                            strRenderer.SymbolList.Add(strHLS);
                            break;
                        case "IMarkerLineSymbol":
                            IMarkerLineSymbol MLS = objSymbol as IMarkerLineSymbol;
                            StructMarkerLineSymbol strMLS = new StructMarkerLineSymbol();
                            strMLS = StoreMarkerLine(MLS);
                            strMLS.Label = Renderer.Label;
                            strRenderer.SymbolList.Add(strMLS);
                            break;
                        case "ISimpleLineSymbol":
                            ISimpleLineSymbol SLS = objSymbol as ISimpleLineSymbol;
                            StructSimpleLineSymbol strSLS = new StructSimpleLineSymbol();
                            strSLS = StoreSimpleLine(SLS);
                            strSLS.Label = Renderer.Label;
                            strRenderer.SymbolList.Add(strSLS);
                            break;
                        case "IPictureLineSymbol":
                            IPictureLineSymbol PLS = objSymbol as IPictureLineSymbol;
                            StructPictureLineSymbol strPLS = new StructPictureLineSymbol();
                            strPLS = StorePictureLine(PLS);
                            strPLS.Label = Renderer.Label;
                            strRenderer.SymbolList.Add(strPLS);
                            break;
                        case "IMultiLayerLineSymbol":
                            IMultiLayerLineSymbol MLLS = objSymbol as IMultiLayerLineSymbol;
                            StructMultilayerLineSymbol strMLLS = new StructMultilayerLineSymbol();
                            strMLLS = StoreMultilayerLines(MLLS);
                            strMLLS.Label = Renderer.Label;
                            strRenderer.SymbolList.Add(strMLLS);
                            break;
                        case "false":
                            InfoMsg("未知线符号", "StoreStructLayer");
                            break;
                    }

                }
                #endregion

                #region 面符号
                if (objFstOrderSymbol is IFillSymbol)
                {
                    strRenderer.FeatureCls = FeatureClass.PolygonFeature;
                    IFillSymbol objSymbol = objFstOrderSymbol as IFillSymbol;
                    switch (FillSymbolScan(objSymbol))
                    {
                        case "ISimpleFillSymbol":
                            ISimpleFillSymbol SFS = objSymbol as ISimpleFillSymbol;
                            StructSimpleFillSymbol strSFS = new StructSimpleFillSymbol();
                            strSFS = StoreSimpleFill(SFS);
                            strSFS.Label = Renderer.Label;
                            strRenderer.SymbolList.Add(strSFS);
                            break;
                        case "IMarkerFillSymbol":
                            IMarkerFillSymbol MFS = objSymbol as IMarkerFillSymbol;
                            StructMarkerFillSymbol strMFS = new StructMarkerFillSymbol();
                            strMFS = StoreMarkerFill(MFS);
                            strMFS.Label = Renderer.Label;
                            strRenderer.SymbolList.Add(strMFS);
                            break;
                        case "ILineFillSymbol":
                            ILineFillSymbol LFS = objSymbol as ILineFillSymbol;
                            StructLineFillSymbol strLFS = new StructLineFillSymbol();
                            strLFS = StoreLineFill(LFS);
                            strLFS.Label = Renderer.Label;
                            strRenderer.SymbolList.Add(strLFS);
                            break;
                        case "IDotDensityFillSymbol":
                            IDotDensityFillSymbol DFS = objSymbol as IDotDensityFillSymbol;
                            StructDotDensityFillSymbol strDFS = new StructDotDensityFillSymbol();
                            strDFS = StoreDotDensityFill(DFS);
                            strDFS.Label = Renderer.Label;
                            strRenderer.SymbolList.Add(strDFS);
                            break;
                        case "IPictureFillSymbol":
                            IPictureFillSymbol PFS = objSymbol as IPictureFillSymbol;
                            StructPictureFillSymbol strPFS = new StructPictureFillSymbol();
                            strPFS = StorePictureFill(PFS);
                            strPFS.Label = Renderer.Label;
                            strRenderer.SymbolList.Add(strPFS);
                            break;
                        case "IGradientFillSymbol":
                            IGradientFillSymbol GFS = objSymbol as IGradientFillSymbol;
                            StructGradientFillSymbol strGFS = new StructGradientFillSymbol();
                            strGFS = StoreGradientFill(GFS);
                            strGFS.Label = Renderer.Label;
                            strRenderer.SymbolList.Add(strGFS);
                            break;
                        case "IMultiLayerFillSymbol":
                            IMultiLayerFillSymbol MLFS = objSymbol as IMultiLayerFillSymbol;
                            StructMultilayerFillSymbol strMLFS = new StructMultilayerFillSymbol();
                            strMLFS = StoreMultiLayerFill(MLFS);
                            strMLFS.Label = Renderer.Label;
                            strRenderer.SymbolList.Add(strMLFS);
                            break;
                        case "false":
                            InfoMsg("未知面符号", "StoreStructLayer");
                            break;
                    }
                }
                #endregion

                #region 统计符号

                if (objFstOrderSymbol is I3DChartSymbol)
                {
                    I3DChartSymbol objSymbol = objFstOrderSymbol as I3DChartSymbol;
                    switch (IIIDChartSymbolScan(objSymbol))
                    {
                        case "IBarChartSymbol":
                            IBarChartSymbol BCS = objSymbol as IBarChartSymbol;
                            StructBarChartSymbol strBCS = new StructBarChartSymbol();
                            strBCS = StoreBarChart(BCS);
                            strBCS.Label = Renderer.Label;
                            strRenderer.SymbolList.Add(strBCS);
                            break;
                        case "IPieChartSymbol":
                            IPieChartSymbol PCS = objSymbol as IPieChartSymbol;
                            StructPieChartSymbol strPCS = new StructPieChartSymbol();
                            strPCS = StorePieChart(PCS);
                            strPCS.Label = Renderer.Label;
                            strRenderer.SymbolList.Add(strPCS);
                            break;
                        case "IStackedChartSymbol":
                            IStackedChartSymbol SCS = objSymbol as IStackedChartSymbol;
                            StructStackedChartSymbol strSCS = new StructStackedChartSymbol();
                            strSCS = StoreStackedChart(SCS);
                            strSCS.Label = Renderer.Label;
                            strRenderer.SymbolList.Add(strSCS);
                            break;
                        case "false":
                            InfoMsg("未知统计符号", "StoreStructLayer");
                            break;
                    }
                }
                #endregion
                return strRenderer;
            }
            catch (Exception ex)
            {

                ErrorMsg("解析简单渲染出错", ex.Message, ex.StackTrace, "StoreStructSimpleRenderer");
                return strRenderer;
            }
        }
        /// <summary>
        /// 数值分类渲染
        /// </summary>
        /// <param name="Renderer">渲染方式</param>
        /// <param name="Layer">图层</param>
        /// <returns></returns>
        private StructClassBreaksRenderer StoreStructCBRenderer(IClassBreaksRenderer Renderer, IFeatureLayer Layer)
        {
            StructClassBreaksRenderer strRenderer = new StructClassBreaksRenderer(); 
            strRenderer.SymbolList = new ArrayList();
            int iNumberOfSymbols = Renderer.BreakCount;
            strRenderer.BreakCount = Renderer.BreakCount;
            ISymbol objFstOrderSymbol = default(ISymbol); 
            IDataset objDataset = Layer.FeatureClass as IDataset;
            bool bIsJoined = false; //是否是连接表
            try
            {
                strRenderer.LayerName = Layer.Name;
                strRenderer.DatasetName = objDataset.Name;
                strRenderer.Annotation = GetAnnotation(Layer);
                ITable pTable = default(ITable);
                IDisplayTable pDisplayTable = default(IDisplayTable);
                pDisplayTable = Layer as IDisplayTable;
                pTable = pDisplayTable.DisplayTable;
                if (pTable is IRelQueryTable) 
                {
                    bIsJoined = true;
                }
                //非连接表
                if (bIsJoined == false) 
                {
                    //SDE数据库
                    //if (frmMotherform.chkArcIMS.Checked == true)
                    //{
                    //    strRenderer.FieldName = objDataset.Name + "." + Renderer.Field;
                    //    strRenderer.NormFieldName = objDataset.Name + "." + Renderer.NormField;
                    //}
                    //else//Shape数据
                    //{
                        strRenderer.FieldName = Renderer.Field;
                        strRenderer.NormFieldName = Renderer.NormField;
                    //}
                }
                for (int j = 0; j <= iNumberOfSymbols - 1; j++)
                {
                    frmMotherform.CHLabelSmall("解析符号 " + (j + 1).ToString() + " 中 " + iNumberOfSymbols.ToString()); 
                    objFstOrderSymbol = Renderer.Symbol[j];
                    IClassBreaksUIProperties objClassBreaksProp = Renderer as IClassBreaksUIProperties;
                    double cLowerLimit = objClassBreaksProp.LowBreak[j];
                    double cUpperLimit = Renderer.Break[j];
                    //文本符号
                    if (objFstOrderSymbol is ITextSymbol)
                    {
                        StructTextSymbol strTS = new StructTextSymbol();
                        ITextSymbol objSymbol = objFstOrderSymbol as ITextSymbol;
                        strTS = StoreText(objSymbol);
                        strTS.Label = Renderer.Label[j];
                        strTS.LowerLimit = cLowerLimit;
                        strTS.UpperLimit = cUpperLimit;
                        strRenderer.SymbolList.Add(strTS);

                    }
                    #region 解析点符号
                    if (objFstOrderSymbol is IMarkerSymbol)
                    {
                        strRenderer.FeatureCls = FeatureClass.PointFeature;
                        IMarkerSymbol objSymbol = default(IMarkerSymbol);
                        objSymbol = objFstOrderSymbol as IMarkerSymbol;
                        switch (MarkerSymbolScan(objSymbol))
                        {
                            case "ISimpleMarkerSymbol":
                                ISimpleMarkerSymbol SMS = objSymbol as ISimpleMarkerSymbol;
                                StructSimpleMarkerSymbol strSMS = new StructSimpleMarkerSymbol();
                                strSMS = StoreSimpleMarker(SMS);
                                strSMS.Label = Renderer.Label[j];
                                strSMS.LowerLimit = cLowerLimit;
                                strSMS.UpperLimit = cUpperLimit;
                                strRenderer.SymbolList.Add(strSMS);
                                break;
                            case "ICharacterMarkerSymbol":
                                ICharacterMarkerSymbol CMS = objSymbol as ICharacterMarkerSymbol;
                                StructCharacterMarkerSymbol strCMS = new StructCharacterMarkerSymbol();
                                strCMS = StoreCharacterMarker(CMS);
                                strCMS.Label = Renderer.Label[j];
                                strCMS.LowerLimit = cLowerLimit;
                                strCMS.UpperLimit = cUpperLimit;
                                strRenderer.SymbolList.Add(strCMS);
                                break;
                            case "IPictureMarkerSymbol":
                                IPictureMarkerSymbol PMS = objSymbol as IPictureMarkerSymbol;
                                StructPictureMarkerSymbol strPMS = new StructPictureMarkerSymbol();
                                strPMS = StorePictureMarker(PMS);
                                strPMS.Label = Renderer.Label[j];
                                strPMS.LowerLimit = cLowerLimit;
                                strPMS.UpperLimit = cUpperLimit;
                                strRenderer.SymbolList.Add(strPMS);
                                break;
                            case "IArrowMarkerSymbol":
                                IArrowMarkerSymbol AMS = objSymbol as IArrowMarkerSymbol;
                                StructArrowMarkerSymbol strAMS = new StructArrowMarkerSymbol();
                                strAMS = StoreArrowMarker(AMS);
                                strAMS.Label = Renderer.Label[j];
                                strAMS.LowerLimit = cLowerLimit;
                                strAMS.UpperLimit = cUpperLimit;
                                strRenderer.SymbolList.Add(strAMS);
                                break;
                            case "IMultiLayerMarkerSymbol":
                                IMultiLayerMarkerSymbol MLMS = objSymbol as IMultiLayerMarkerSymbol;
                                StructMultilayerMarkerSymbol strMLMS = new StructMultilayerMarkerSymbol();
                                strMLMS = StoreMultiLayerMarker(MLMS);
                                strMLMS.Label = Renderer.Label[j];
                                strMLMS.LowerLimit = cLowerLimit;
                                strMLMS.UpperLimit = cUpperLimit;
                                strRenderer.SymbolList.Add(strMLMS);
                                break;
                            case "false":
                                InfoMsg("未知标记符号", "StoreStructLayer");
                                break;
                        }
                    }
                    #endregion
                    #region  解析线符号
                    if (objFstOrderSymbol is ILineSymbol)
                    {
                        strRenderer.FeatureCls = FeatureClass.LineFeature;
                        ILineSymbol objSymbol = objFstOrderSymbol as ILineSymbol;
                        switch (LineSymbolScan(objSymbol))
                        {
                            case "ICartographicLineSymbol":
                                ICartographicLineSymbol CLS = objSymbol as ICartographicLineSymbol;
                                StructCartographicLineSymbol strCLS = new StructCartographicLineSymbol();
                                strCLS = StoreCartographicLine(CLS);
                                strCLS.Label = Renderer.Label[j];
                                strCLS.LowerLimit = cLowerLimit;
                                strCLS.UpperLimit = cUpperLimit;
                                strRenderer.SymbolList.Add(strCLS);
                                break;
                            case "IHashLineSymbol":
                                IHashLineSymbol HLS = objSymbol as IHashLineSymbol;
                                StructHashLineSymbol strHLS = new StructHashLineSymbol();
                                strHLS = StoreHashLine(HLS);
                                strHLS.Label = Renderer.Label[j];
                                strHLS.LowerLimit = cLowerLimit;
                                strHLS.UpperLimit = cUpperLimit;
                                strRenderer.SymbolList.Add(strHLS);
                                break;
                            case "IMarkerLineSymbol":
                                IMarkerLineSymbol MLS = objSymbol as IMarkerLineSymbol;
                                StructMarkerLineSymbol strMLS = new StructMarkerLineSymbol();
                                strMLS = StoreMarkerLine(MLS);
                                strMLS.Label = Renderer.Label[j];
                                strMLS.LowerLimit = cLowerLimit;
                                strMLS.UpperLimit = cUpperLimit;
                                strRenderer.SymbolList.Add(strMLS);
                                break;
                            case "ISimpleLineSymbol":
                                ISimpleLineSymbol SLS = objSymbol as ISimpleLineSymbol;
                                StructSimpleLineSymbol strSLS = new StructSimpleLineSymbol();
                                strSLS = StoreSimpleLine(SLS);
                                strSLS.Label = Renderer.Label[j];
                                strSLS.LowerLimit = cLowerLimit;
                                strSLS.UpperLimit = cUpperLimit;
                                strRenderer.SymbolList.Add(strSLS);
                                break;
                            case "IPictureLineSymbol":
                                IPictureLineSymbol PLS = objSymbol as IPictureLineSymbol;
                                StructPictureLineSymbol strPLS = new StructPictureLineSymbol();
                                strPLS = StorePictureLine(PLS);
                                strPLS.Label = Renderer.Label[j];
                                strPLS.LowerLimit = cLowerLimit;
                                strPLS.UpperLimit = cUpperLimit;
                                strRenderer.SymbolList.Add(strPLS);
                                break;
                            case "IMultiLayerLineSymbol":
                                IMultiLayerLineSymbol MLLS = objSymbol as IMultiLayerLineSymbol;
                                StructMultilayerLineSymbol strMLLS = new StructMultilayerLineSymbol();
                                strMLLS = StoreMultilayerLines(MLLS);
                                strMLLS.Label = Renderer.Label[j];
                                strMLLS.LowerLimit = cLowerLimit;
                                strMLLS.UpperLimit = cUpperLimit;
                                strRenderer.SymbolList.Add(strMLLS);
                                break;
                            case "false":
                                InfoMsg("未知线符号", "StoreStructLayer");
                                break;
                        }
                    }
                    #endregion

                    #region 解析面符号
                    if (objFstOrderSymbol is IFillSymbol)
                    {
                        strRenderer.FeatureCls = FeatureClass.PolygonFeature;
                        IFillSymbol objSymbol = objFstOrderSymbol as IFillSymbol;
                        switch (FillSymbolScan(objSymbol))
                        {
                            case "ISimpleFillSymbol":
                                ISimpleFillSymbol SFS = objSymbol as ISimpleFillSymbol;
                                StructSimpleFillSymbol strSFS = new StructSimpleFillSymbol();
                                strSFS = StoreSimpleFill(SFS);
                                strSFS.Label = Renderer.Label[j];
                                strSFS.LowerLimit = cLowerLimit;
                                strSFS.UpperLimit = cUpperLimit;
                                strRenderer.SymbolList.Add(strSFS);
                                break;
                            case "IMarkerFillSymbol":
                                IMarkerFillSymbol MFS = objSymbol as IMarkerFillSymbol;
                                StructMarkerFillSymbol strMFS = new StructMarkerFillSymbol();
                                strMFS = StoreMarkerFill(MFS);
                                strMFS.Label = Renderer.Label[j];
                                strMFS.LowerLimit = cLowerLimit;
                                strMFS.UpperLimit = cUpperLimit;
                                strRenderer.SymbolList.Add(strMFS);
                                break;
                            case "ILineFillSymbol":
                                ILineFillSymbol LFS = objSymbol as ILineFillSymbol;
                                StructLineFillSymbol strLFS = new StructLineFillSymbol();
                                strLFS = StoreLineFill(LFS);
                                strLFS.Label = Renderer.Label[j];
                                strLFS.LowerLimit = cLowerLimit;
                                strLFS.UpperLimit = cUpperLimit;
                                strRenderer.SymbolList.Add(strLFS);
                                break;
                            case "IDotDensityFillSymbol":
                                IDotDensityFillSymbol DFS = objSymbol as IDotDensityFillSymbol;
                                StructDotDensityFillSymbol strDFS = new StructDotDensityFillSymbol();
                                strDFS = StoreDotDensityFill(DFS);
                                strDFS.Label = Renderer.Label[j];
                                strDFS.LowerLimit = cLowerLimit;
                                strDFS.UpperLimit = cUpperLimit;
                                strRenderer.SymbolList.Add(strDFS);
                                break;
                            case "IPictureFillSymbol":
                                IPictureFillSymbol PFS = objSymbol as IPictureFillSymbol;
                                StructPictureFillSymbol strPFS = new StructPictureFillSymbol();
                                strPFS = StorePictureFill(PFS);
                                strPFS.Label = Renderer.Label[j];
                                strPFS.LowerLimit = cLowerLimit;
                                strPFS.UpperLimit = cUpperLimit;
                                strRenderer.SymbolList.Add(strPFS);
                                break;
                            case "IGradientFillSymbol":
                                IGradientFillSymbol GFS = objSymbol as IGradientFillSymbol;
                                StructGradientFillSymbol strGFS = new StructGradientFillSymbol();
                                strGFS = StoreGradientFill(GFS);
                                strGFS.Label = Renderer.Label[j];
                                strGFS.LowerLimit = cLowerLimit;
                                strGFS.UpperLimit = cUpperLimit;
                                strRenderer.SymbolList.Add(strGFS);
                                break;
                            case "IMultiLayerFillSymbol":
                                IMultiLayerFillSymbol MLFS = objSymbol as IMultiLayerFillSymbol;
                                StructMultilayerFillSymbol strMLFS = new StructMultilayerFillSymbol();
                                strMLFS = StoreMultiLayerFill(MLFS);
                                strMLFS.Label = Renderer.Label[j];
                                strMLFS.LowerLimit = cLowerLimit;
                                strMLFS.UpperLimit = cUpperLimit;
                                strRenderer.SymbolList.Add(strMLFS);
                                break;
                            case "false":
                                InfoMsg("未知填充符号", "StoreStructLayer");
                                break;
                        }
                    }
                    #endregion

                    #region 3D 符号
                    if (objFstOrderSymbol is I3DChartSymbol)
                    {
                        I3DChartSymbol objSymbol = objFstOrderSymbol as I3DChartSymbol;
                        switch (IIIDChartSymbolScan(objSymbol))
                        {
                            case "IBarChartSymbol":
                                IBarChartSymbol BCS = objSymbol as IBarChartSymbol;
                                StructBarChartSymbol strBCS = new StructBarChartSymbol();
                                strBCS = StoreBarChart(BCS);
                                strBCS.Label = Renderer.Label[j];
                                strBCS.LowerLimit = cLowerLimit;
                                strBCS.UpperLimit = cUpperLimit;
                                strRenderer.SymbolList.Add(strBCS);
                                break;
                            case "IPieChartSymbol":
                                IPieChartSymbol PCS = objSymbol as IPieChartSymbol;
                                StructPieChartSymbol strPCS = new StructPieChartSymbol();
                                strPCS = StorePieChart(PCS);
                                strPCS.Label = Renderer.Label[j];
                                strPCS.LowerLimit = cLowerLimit;
                                strPCS.UpperLimit = cUpperLimit;
                                strRenderer.SymbolList.Add(strPCS);
                                break;
                            case "IStackedChartSymbol":
                                IStackedChartSymbol SCS = objSymbol as IStackedChartSymbol;
                                StructStackedChartSymbol strSCS = new StructStackedChartSymbol();
                                strSCS = StoreStackedChart(SCS);
                                strSCS.Label = Renderer.Label[j];
                                strSCS.LowerLimit = cLowerLimit;
                                strSCS.UpperLimit = cUpperLimit;
                                strRenderer.SymbolList.Add(strSCS);
                                break;
                            case "false":
                                InfoMsg("未知统计符号", "StoreStructLayer");
                                break;
                        }
                    }
                    #endregion
                }
                return strRenderer;
            }
            catch (Exception ex)
            {

                ErrorMsg("解析分类符号出错", ex.Message, ex.StackTrace, "StoreStructCBRenderer");
                return strRenderer;
            }
        }
        /// <summary>
        /// 分析唯一值渲染方式
        /// </summary>
        /// <param name="Renderer">唯一值渲染方式</param>
        /// <param name="Layer">分析目标图层</param>
        /// <returns></returns>
        private StructUniqueValueRenderer StoreStructUVRenderer(IUniqueValueRenderer Renderer, IFeatureLayer Layer)
        {
            StructUniqueValueRenderer strRenderer = new StructUniqueValueRenderer();
            //符号数量
            int iNumberOfSymbols = Renderer.ValueCount;

            ISymbol objFstOrderSymbol = default(ISymbol);
            //所有字段名称
            ArrayList alFieldNames = new ArrayList();

            bool bNoSepFieldVal = false;
            ITable objTable = Layer.FeatureClass as ITable; 
            IDataset objDataset = objTable as IDataset; 

            strRenderer.SymbolList = new ArrayList();

            m_alClassifiedFields = new ArrayList(); 

            bool bIsJoined = false;
            try
            {
                strRenderer.ValueCount = iNumberOfSymbols;
                strRenderer.LayerName = Layer.Name;
                strRenderer.DatasetName = objDataset.Name;
                //获取图层注记信息
                strRenderer.Annotation = GetAnnotation(Layer);

                #region 分析渲染的属性字段信息
                //字段数量
                int iFieldCount = Renderer.FieldCount;

                if (iFieldCount > 1) 
                {
                    bNoSepFieldVal = true; 
                }

                IDisplayTable pDisplayTable = Layer as IDisplayTable;
                ITable pTable = pDisplayTable.DisplayTable;
                //是否是关系表
                if (pTable is IRelQueryTable) 
                {
                    bIsJoined = true;
                }
                //唯一值字段有多个
                if (bNoSepFieldVal) 
                {
                    //数据源为SHAPE文件
                    if (objDataset.Workspace.Type == esriWorkspaceType.esriFileSystemWorkspace)
                    {
                        for (int i = 1; i <= iFieldCount; i++)
                        {
                            alFieldNames.Add(Renderer.Field[i - 1]); 
                        }

                        GimmeUniqueValuesFromShape(objTable, alFieldNames);
                     
                        strRenderer.FieldNames = alFieldNames;
                    }
                    //数据源为其他
                    else
                    {
                        for (int i = 1; i <= iFieldCount; i++)
                        {
                            alFieldNames.Add(Renderer.Field[i - 1]); 
                             //属性表有连接表                                        
                            if (pTable is IRelQueryTable)
                            {
                               
                                IRelQueryTable pRelQueryTable = default(IRelQueryTable);
                                ITable pDestTable = default(ITable);
                                IDataset pDataSet = default(IDataset);
                             
                                ArrayList alJoinedTableNames = new ArrayList();
                                while (pTable is IRelQueryTable)
                                {
                                    pRelQueryTable = pTable as IRelQueryTable;
                                    pDestTable = pRelQueryTable.DestinationTable;
                                    pDataSet = pDestTable as IDataset;
                                 
                                    pTable = pRelQueryTable.SourceTable;
                                    alJoinedTableNames.Add(pDataSet.Name);
                                }
                                GimmeUniqeValuesForFieldname(objTable, Convert.ToString(Renderer.Field[i - 1]), alJoinedTableNames); 
                                pTable = pDisplayTable.DisplayTable; 
                            }
                            //属性表没有连接表
                            else 
                            {
                                GimmeUniqeValuesForFieldname(objTable, Renderer.Field[i - 1]); 
                            }
                        }
                        strRenderer.FieldNames = alFieldNames;
                    }
                }
                //唯一值字段只有一个
                else
                {
                    alFieldNames.Add(Renderer.Field[iFieldCount - 1]);
                    strRenderer.FieldNames = alFieldNames;
                }
                 //是否存在连接表
                if (bIsJoined == false) 
                {
                    int idummy;
                    //数据源为SDE
                    //if (frmMotherform.chkArcIMS.Checked == true)
                    //{
                    //    for (int i = 0; i <= strRenderer.FieldNames.Count - 1; i++)
                    //    {
                    //        strRenderer.FieldNames[i] = objDataset.Name + "." + Convert.ToString(strRenderer.FieldNames[i]);
                    //    }
                    //}
                }
                #endregion
                #region 分析符号信息
                for (int j = 0; j <= iNumberOfSymbols - 1; j++)
                {
                    frmMotherform.CHLabelSmall("Symbol " + (j + 1).ToString() + " von " + iNumberOfSymbols.ToString()); 

                    objFstOrderSymbol = Renderer.Symbol[Renderer.get_Value(j)];

                    #region 分析文本符号
                    if (objFstOrderSymbol is ITextSymbol)
                    {
                        StructTextSymbol strTS = new StructTextSymbol();
                        ITextSymbol objSymbol = default(ITextSymbol);
                        objSymbol = objFstOrderSymbol as ITextSymbol;
                        strTS = StoreText(objSymbol);
                   
                        strTS.Label = Renderer.Label[Renderer.get_Value(j)];
                        strTS.Fieldvalues = getUVFieldValues(Renderer, j);

                        strRenderer.SymbolList.Add(strTS);
                    }
                    #endregion

                    #region 分析点符号
                    if (objFstOrderSymbol is IMarkerSymbol)
                    {
                        strRenderer.FeatureCls = FeatureClass.PointFeature;
                        IMarkerSymbol objSymbol = objFstOrderSymbol as IMarkerSymbol;
                        switch (MarkerSymbolScan(objSymbol))
                        {
                            case "ISimpleMarkerSymbol"://简单点符号
                                ISimpleMarkerSymbol SMS = objSymbol as ISimpleMarkerSymbol;
                                StructSimpleMarkerSymbol strSMS = StoreSimpleMarker(SMS);
                                strSMS.Label = Renderer.Label[Renderer.get_Value(j)];
                                strSMS.Fieldvalues = getUVFieldValues(Renderer, j);
                                strRenderer.SymbolList.Add(strSMS);
                                break;

                            case "ICharacterMarkerSymbol"://字符集点符号
                                ICharacterMarkerSymbol CMS = objSymbol as ICharacterMarkerSymbol;
                                StructCharacterMarkerSymbol strCMS = StoreCharacterMarker(CMS);
                                strCMS.Label = Renderer.Label[Renderer.get_Value(j)];
                                strCMS.Fieldvalues = getUVFieldValues(Renderer, j);
                                strRenderer.SymbolList.Add(strCMS);
                                break;

                            case "IPictureMarkerSymbol"://图片点符号
                                IPictureMarkerSymbol PMS = objSymbol as IPictureMarkerSymbol;
                                StructPictureMarkerSymbol strPMS = StorePictureMarker(PMS);
                                strPMS.Label = Renderer.Label[Renderer.get_Value(j)];
                                strPMS.Fieldvalues = getUVFieldValues(Renderer, j);
                                strRenderer.SymbolList.Add(strPMS);
                                break;

                            case "IArrowMarkerSymbol"://箭头点符号
                                IArrowMarkerSymbol AMS = objSymbol as IArrowMarkerSymbol;
                                StructArrowMarkerSymbol strAMS = StoreArrowMarker(AMS);
                                strAMS.Label = Renderer.Label[Renderer.get_Value(j)];
                                strAMS.Fieldvalues = getUVFieldValues(Renderer, j);
                                strRenderer.SymbolList.Add(strAMS);
                                break;

                            case "IMultiLayerMarkerSymbol"://多图层点符号
                                IMultiLayerMarkerSymbol MLMS = objSymbol as IMultiLayerMarkerSymbol;
                                StructMultilayerMarkerSymbol strMLMS = StoreMultiLayerMarker(MLMS);
                                strMLMS.Label = Renderer.Label[Renderer.get_Value(j)];
                                strMLMS.Fieldvalues = getUVFieldValues(Renderer, j);
                                strRenderer.SymbolList.Add(strMLMS);
                                break;

                            case "false":
                                InfoMsg("未知符号信息", "StoreStructLayer");
                                break;
                        }
                    }
                    #endregion

                    #region 分析线符号
                    if (objFstOrderSymbol is ILineSymbol)
                    {
                        strRenderer.FeatureCls = FeatureClass.LineFeature;
                        ILineSymbol objSymbol  = objFstOrderSymbol as ILineSymbol;
                        switch (LineSymbolScan(objSymbol))
                        {
                            case "ICartographicLineSymbol"://制图线符号
                                ICartographicLineSymbol CLS = objSymbol as ICartographicLineSymbol;
                                StructCartographicLineSymbol strCLS = StoreCartographicLine(CLS);
                                strCLS.Label = Renderer.Label[Renderer.get_Value(j)];
                                strCLS.Fieldvalues = getUVFieldValues(Renderer, j);
                                strRenderer.SymbolList.Add(strCLS);
                                break;

                            case "IHashLineSymbol"://哈希线符号
                                IHashLineSymbol HLS = objSymbol as IHashLineSymbol;
                                StructHashLineSymbol strHLS = new StructHashLineSymbol();
                                strHLS = StoreHashLine(HLS);
                                strHLS.Label = Renderer.Label[Renderer.get_Value(j)];
                                strHLS.Fieldvalues = getUVFieldValues(Renderer, j);
                                strRenderer.SymbolList.Add(strHLS);
                                break;

                            case "IMarkerLineSymbol"://标记线符号
                                IMarkerLineSymbol MLS = objSymbol as IMarkerLineSymbol;
                                StructMarkerLineSymbol strMLS = new StructMarkerLineSymbol();
                                strMLS = StoreMarkerLine(MLS);
                                strMLS.Label = Renderer.Label[Renderer.get_Value(j)];
                                strMLS.Fieldvalues = getUVFieldValues(Renderer, j);
                                strRenderer.SymbolList.Add(strMLS);
                                break;

                            case "ISimpleLineSymbol"://简单线符号
                                ISimpleLineSymbol SLS = objSymbol as ISimpleLineSymbol;
                                StructSimpleLineSymbol strSLS = new StructSimpleLineSymbol();
                                strSLS = StoreSimpleLine(SLS);
                                strSLS.Label = Renderer.Label[Renderer.get_Value(j)];
                                strSLS.Fieldvalues = getUVFieldValues(Renderer, j);
                                strRenderer.SymbolList.Add(strSLS);
                                break;

                            case "IPictureLineSymbol"://图片线符号
                                IPictureLineSymbol PLS = objSymbol as IPictureLineSymbol;
                                StructPictureLineSymbol strPLS = new StructPictureLineSymbol();
                                strPLS = StorePictureLine(PLS);
                                strPLS.Label = Renderer.Label[Renderer.get_Value(j)];
                                strPLS.Fieldvalues = getUVFieldValues(Renderer, j);
                                strRenderer.SymbolList.Add(strPLS);
                                break;

                            case "IMultiLayerLineSymbol"://多图层线符号
                                IMultiLayerLineSymbol MLLS = objSymbol as IMultiLayerLineSymbol;
                                StructMultilayerLineSymbol strMLLS = new StructMultilayerLineSymbol();
                                strMLLS = StoreMultilayerLines(MLLS);
                                strMLLS.Label = Renderer.Label[Renderer.get_Value(j)];
                                strMLLS.Fieldvalues = getUVFieldValues(Renderer, j);
                                strRenderer.SymbolList.Add(strMLLS);
                                break;
                            case "false":
                                InfoMsg("未知符号信息", "StoreStructLayer");
                                break;
                        }

                    }
                    #endregion

                    #region 分析面符号
                    if (objFstOrderSymbol is IFillSymbol)
                    {
                        strRenderer.FeatureCls = FeatureClass.PolygonFeature;
                        IFillSymbol objSymbol = objFstOrderSymbol as IFillSymbol;
                        switch (FillSymbolScan(objSymbol))
                        {
                            case "ISimpleFillSymbol"://简单面符号
                                ISimpleFillSymbol SFS = objSymbol as ISimpleFillSymbol;
                                StructSimpleFillSymbol strSFS = new StructSimpleFillSymbol();
                                strSFS = StoreSimpleFill(SFS);
                                strSFS.Label = Renderer.Label[Renderer.get_Value(j)];
                                strSFS.Fieldvalues = getUVFieldValues(Renderer, j);
                                strRenderer.SymbolList.Add(strSFS);
                                break;
                            case "IMarkerFillSymbol"://点填充面符号
                                IMarkerFillSymbol MFS = objSymbol as IMarkerFillSymbol;
                                StructMarkerFillSymbol strMFS = new StructMarkerFillSymbol();
                                strMFS = StoreMarkerFill(MFS);
                                strMFS.Label = Renderer.Label[Renderer.get_Value(j)];
                                strMFS.Fieldvalues = getUVFieldValues(Renderer, j);
                                strRenderer.SymbolList.Add(strMFS);
                                break;
                            case "ILineFillSymbol"://线填充面符号
                                ILineFillSymbol LFS = objSymbol as ILineFillSymbol;
                                StructLineFillSymbol strLFS = new StructLineFillSymbol();
                                strLFS = StoreLineFill(LFS);
                                strLFS.Label = Renderer.Label[Renderer.get_Value(j)];
                                strLFS.Fieldvalues = getUVFieldValues(Renderer, j);
                                strRenderer.SymbolList.Add(strLFS);
                                break;
                            case "IDotDensityFillSymbol"://点密度面符号
                                IDotDensityFillSymbol DFS = objSymbol as IDotDensityFillSymbol;
                                StructDotDensityFillSymbol strDFS = new StructDotDensityFillSymbol();
                                strDFS = StoreDotDensityFill(DFS);
                                strDFS.Label = Renderer.Label[Renderer.get_Value(j)];
                                strDFS.Fieldvalues = getUVFieldValues(Renderer, j);
                                strRenderer.SymbolList.Add(strDFS);
                                break;
                            case "IPictureFillSymbol"://图片填充符号
                                IPictureFillSymbol PFS = objSymbol as IPictureFillSymbol;
                                StructPictureFillSymbol strPFS = new StructPictureFillSymbol();
                                strPFS = StorePictureFill(PFS);
                                strPFS.Label = Renderer.Label[Renderer.get_Value(j)];
                                strPFS.Fieldvalues = getUVFieldValues(Renderer, j);
                                strRenderer.SymbolList.Add(strPFS);
                                break;
                            case "IGradientFillSymbol"://梯度填充符号
                                IGradientFillSymbol GFS = objSymbol as IGradientFillSymbol;
                                StructGradientFillSymbol strGFS = new StructGradientFillSymbol();
                                strGFS = StoreGradientFill(GFS);
                                strGFS.Label = Renderer.Label[Renderer.get_Value(j)];
                                strGFS.Fieldvalues = getUVFieldValues(Renderer, j);
                                strRenderer.SymbolList.Add(strGFS);
                                break;
                            case "IMultiLayerFillSymbol"://多图层填充符号
                                IMultiLayerFillSymbol MLFS = objSymbol as IMultiLayerFillSymbol;
                                StructMultilayerFillSymbol strMLFS = new StructMultilayerFillSymbol();
                                strMLFS = StoreMultiLayerFill(MLFS);
                                strMLFS.Label = Renderer.Label[Renderer.get_Value(j)];
                                strMLFS.Fieldvalues = getUVFieldValues(Renderer, j);
                                strRenderer.SymbolList.Add(strMLFS);
                                break;
                            case "false":
                                InfoMsg("未知填充符号", "StoreStructLayer");
                                break;
                        }
                    }
                    #endregion

                    #region 分析统计符号
                    if (objFstOrderSymbol is I3DChartSymbol)
                    {
                        I3DChartSymbol objSymbol = objFstOrderSymbol as I3DChartSymbol;
                        switch (IIIDChartSymbolScan(objSymbol))
                        {
                            case "IBarChartSymbol"://条形图统计符号
                                IBarChartSymbol BCS = objSymbol as IBarChartSymbol;
                                StructBarChartSymbol strBCS = new StructBarChartSymbol();
                                strBCS = StoreBarChart(BCS);
                                strBCS.Label = Renderer.Label[Renderer.get_Value(j)];
                                strBCS.Fieldvalues = getUVFieldValues(Renderer, j);
                                strRenderer.SymbolList.Add(strBCS);
                                break;
                            case "IPieChartSymbol"://饼状统计符号
                                IPieChartSymbol PCS = objSymbol as IPieChartSymbol;
                                StructPieChartSymbol strPCS = new StructPieChartSymbol();
                                strPCS = StorePieChart(PCS);
                                strPCS.Label = Renderer.Label[Renderer.get_Value(j)];
                                strPCS.Fieldvalues = getUVFieldValues(Renderer, j);
                                strRenderer.SymbolList.Add(strPCS);
                                break;

                            case "IStackedChartSymbol"://表格统计符号
                                IStackedChartSymbol SCS = objSymbol as IStackedChartSymbol;
                                StructStackedChartSymbol strSCS = new StructStackedChartSymbol();
                                strSCS = StoreStackedChart(SCS);
                                strSCS.Label = Renderer.Label[Renderer.get_Value(j)];
                                strSCS.Fieldvalues = getUVFieldValues(Renderer, j);
                                strRenderer.SymbolList.Add(strSCS);
                                break;

                            case "false":
                                InfoMsg("未知符号信息", "StoreStructLayer");
                                break;
                        }
                    }
                    #endregion
                }
                #endregion

                return strRenderer;
            }
            catch (Exception ex)
            {
                ErrorMsg("分析唯一值符号出错", ex.Message, ex.StackTrace, "StoreStructUVRenderer");
                return strRenderer;
            }
        }
        /// <summary>
        /// 获取唯一值渲染方式中指定序号中符号对应的值
        /// </summary>
        /// <param name="Renderer"></param>
        /// <param name="Index"></param>
        /// <returns></returns>
        private ArrayList getUVFieldValues(IUniqueValueRenderer Renderer, int Index)
        {
            ArrayList Fieldvalues = default(ArrayList);
            int iFieldCount = 0; 
            int Index2 = 0;
            iFieldCount = Renderer.FieldCount;
            if (iFieldCount > 0)
            {
                bool bNoSepFieldVal; 
                string Label;
                string Label2 = "";
                ISymbol objSymbol = default(ISymbol);
                int iNumberOfSymbols = Renderer.ValueCount; 
                bNoSepFieldVal = false;
                if (iFieldCount > 1) 
                {
                    bNoSepFieldVal = true; 
                }

                Label = Renderer.Label[Renderer.Value[Index]];
                if (bNoSepFieldVal == false)
                {
                    Fieldvalues = new ArrayList();
                    Fieldvalues.Add(Renderer.Value[Index]);
                    Index2 = Index + 1;
                    while (Index2 < iNumberOfSymbols)
                    {
                        objSymbol = Renderer.Symbol[Renderer.Value[Index2]];
                        Label2 = Renderer.Label[Renderer.Value[Index2]];
                        if (objSymbol == null && Label == Label2)
                        {
                            Fieldvalues.Add(Renderer.Value[Index2]);
                        }
                        else
                        {
                            break;
                        }
                        Index2++;
                    }
                }
                else
                {
                    Fieldvalues = GimmeSeperateFieldValues(Renderer.Value[Index], Renderer.FieldDelimiter);
                }
            }
            return Fieldvalues;
        }
        /// <summary>
        /// 解析简单标记符号
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private StructSimpleMarkerSymbol StoreSimpleMarker(ISimpleMarkerSymbol symbol)
        {
            StructSimpleMarkerSymbol StructStorage = new StructSimpleMarkerSymbol();
            StructStorage.Angle = symbol.Angle;
            StructStorage.Filled = symbol.Color.Transparency != 0;
            StructStorage.Color = GimmeStringForColor(symbol.Color);
            StructStorage.Outline = symbol.Outline;
            StructStorage.OutlineColor = GimmeStringForColor(symbol.OutlineColor);
            StructStorage.OutlineSize = symbol.OutlineSize;
            StructStorage.Size = symbol.Size;
            StructStorage.Style = symbol.Style.ToString();
            StructStorage.XOffset = symbol.XOffset;
            StructStorage.YOffset = symbol.YOffset;
            return StructStorage;
        }
        /// <summary>
        /// 解析字符集符号
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private StructCharacterMarkerSymbol StoreCharacterMarker(ICharacterMarkerSymbol symbol)
        {
            StructCharacterMarkerSymbol StructStorage = new StructCharacterMarkerSymbol();
            StructStorage.Angle = symbol.Angle;
            StructStorage.CharacterIndex = symbol.CharacterIndex;
            StructStorage.Color = GimmeStringForColor(symbol.Color);
            StructStorage.Font = symbol.Font.Name;
            StructStorage.Size = symbol.Size;
            StructStorage.XOffset = symbol.XOffset;
            StructStorage.YOffset = symbol.YOffset;
            return StructStorage;
        }
        /// <summary>
        /// 解析图片点符号
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private StructPictureMarkerSymbol StorePictureMarker(IPictureMarkerSymbol symbol)
        {
            StructPictureMarkerSymbol StructStorage = new StructPictureMarkerSymbol();
            StructStorage.Angle = symbol.Angle;
            StructStorage.BackgroundColor = GimmeStringForColor(symbol.BackgroundColor);
            StructStorage.Color = GimmeStringForColor(symbol.Color);
            StructStorage.Picture = symbol.Picture as IPicture;
            StructStorage.Size = symbol.Size;
            StructStorage.XOffset = symbol.XOffset;
            StructStorage.YOffset = symbol.YOffset;
            return StructStorage;
        }
        /// <summary>
        /// 箭头点符号
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private StructArrowMarkerSymbol StoreArrowMarker(IArrowMarkerSymbol symbol)
        {
            StructArrowMarkerSymbol StructStorage = new StructArrowMarkerSymbol();
            StructStorage.Angle = symbol.Angle;
            StructStorage.Color = GimmeStringForColor(symbol.Color);
            StructStorage.Length = symbol.Length;
            StructStorage.Size = symbol.Size;
            StructStorage.Style = symbol.Style.ToString();
            StructStorage.Width = symbol.Width;
            StructStorage.XOffset = symbol.XOffset;
            StructStorage.YOffset = symbol.YOffset;
            return StructStorage;
        }
        /// <summary>
        /// 解析多图层点符号
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private StructMultilayerMarkerSymbol StoreMultiLayerMarker(IMultiLayerMarkerSymbol symbol)
        {
            StructMultilayerMarkerSymbol StructStorage = new StructMultilayerMarkerSymbol();
            StructStorage.MultiMarkerLayers = new ArrayList();
            StructStorage.LayerCount = symbol.LayerCount;
            for (int i = 0; i <= symbol.LayerCount - 1; i++) 
            {
                switch (MarkerSymbolScan(symbol.Layer[i])) 
                {
                    case "ISimpleMarkerSymbol":
                        ISimpleMarkerSymbol SMS = default(ISimpleMarkerSymbol);
                        SMS = symbol.get_Layer(i) as ISimpleMarkerSymbol;
                        StructStorage.MultiMarkerLayers.Add(StoreSimpleMarker(SMS)); 
                        break;
                    case "ICharacterMarkerSymbol":
                        ICharacterMarkerSymbol CMS = default(ICharacterMarkerSymbol);
                        CMS = symbol.get_Layer(i) as ICharacterMarkerSymbol;
                        StructStorage.MultiMarkerLayers.Add(StoreCharacterMarker(CMS)); 
                        break;
                    case "IPictureMarkerSymbol":
                        IPictureMarkerSymbol PMS = default(IPictureMarkerSymbol);
                        PMS = symbol.get_Layer(i) as IPictureMarkerSymbol;
                        StructStorage.MultiMarkerLayers.Add(StorePictureMarker(PMS)); 
                        break;
                    case "IArrowMarkerSymbol":
                        IArrowMarkerSymbol AMS = default(IArrowMarkerSymbol);
                        AMS = symbol.get_Layer(i) as IArrowMarkerSymbol;
                        StructStorage.MultiMarkerLayers.Add(StoreArrowMarker(AMS)); 
                        break;
                    case "false":
                        InfoMsg("分析多图层点符号出错", "StoreMultiLayerMarker");
                        break;
                }
            }
            return StructStorage;
        }
        /// <summary>
        /// 解析简单线符号
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private StructSimpleLineSymbol StoreSimpleLine(ISimpleLineSymbol symbol)
        {
            StructSimpleLineSymbol StructStorage = new StructSimpleLineSymbol();
            if (symbol.Style == esriSimpleLineStyle.esriSLSNull)
            {
                StructStorage.Color = "";
            }
            else
            {
                StructStorage.Color = GimmeStringForColor(symbol.Color);
            }
            StructStorage.Transparency = symbol.Color.Transparency;
            StructStorage.publicStyle = symbol.Style.ToString();
            StructStorage.Width = symbol.Width;
            return StructStorage;
        }
        /// <summary>
        /// 解析制图线符号
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private StructCartographicLineSymbol StoreCartographicLine(ICartographicLineSymbol symbol)
        {
            StructCartographicLineSymbol StructStorage = new StructCartographicLineSymbol();
            StructStorage.Color = GimmeStringForColor(symbol.Color);
            StructStorage.Transparency = symbol.Color.Transparency;
            StructStorage.Width = symbol.Width;
            StructStorage.Join = symbol.Join.ToString();
            StructStorage.MiterLimit = symbol.MiterLimit;
            StructStorage.Cap = symbol.Cap.ToString();
            StructStorage.DashArray = new ArrayList();
            if (symbol is ILineProperties)
            {
                ILineProperties lineProperties = symbol as ILineProperties;
                double markLen = 0;
                double gapLen = 0;
                if (lineProperties.Template is ITemplate)
                {
                    ITemplate template = lineProperties.Template;
                    double interval = template.Interval;
                    for (int templateIdx = 0; templateIdx <= template.PatternElementCount - 1; templateIdx++)
                    {
                        template.GetPatternElement(templateIdx, out markLen, out gapLen);
                        StructStorage.DashArray.Add(markLen * interval);
                        StructStorage.DashArray.Add(gapLen * interval);
                    }
                }
            }
            return StructStorage;
        }
        /// <summary>
        /// 解析哈希线符号
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private StructHashLineSymbol StoreHashLine(IHashLineSymbol symbol)
        {
            StructHashLineSymbol StructStorage = new StructHashLineSymbol();
            StructStorage.Angle = symbol.Angle;
            StructStorage.Color = GimmeStringForColor(symbol.Color);
            StructStorage.Transparency = symbol.Color.Transparency;
            switch (LineSymbolScan(symbol.HashSymbol)) 
            {
                case "ICartographicLineSymbol":
                    ICartographicLineSymbol CLS = symbol.HashSymbol as ICartographicLineSymbol;
                    StructStorage.HashSymbol_CartographicLine = StoreCartographicLine(CLS);
                    StructStorage.kindOfLineStruct = LineStructs.StructCartographicLineSymbol;
                    break;
                case "IMarkerLineSymbol":
                    IMarkerLineSymbol MLS = symbol.HashSymbol as IMarkerLineSymbol;
                    StructStorage.HashSymbol_MarkerLine = StoreMarkerLine(MLS);
                    StructStorage.kindOfLineStruct = LineStructs.StructMarkerLineSymbol;
                    break;
                case "ISimpleLineSymbol":
                    ISimpleLineSymbol SLS = symbol.HashSymbol as ISimpleLineSymbol;
                    StructStorage.HashSymbol_SimpleLine = StoreSimpleLine(SLS);
                    StructStorage.kindOfLineStruct = LineStructs.StructSimpleLineSymbol;
                    break;
                case "IPictureLineSymbol":
                    IPictureLineSymbol PLS = symbol.HashSymbol as IPictureLineSymbol;
                    StructStorage.HashSymbol_PictureLine = StorePictureLine(PLS);
                    StructStorage.kindOfLineStruct = LineStructs.StructPictureLineSymbol;
                    break;
                case "IMultiLayerLineSymbol":
                    IMultiLayerLineSymbol MLLS = symbol.HashSymbol as IMultiLayerLineSymbol;
                    StructStorage.HashSymbol_MultiLayerLines = StoreMultilayerLines(MLLS);
                    StructStorage.kindOfLineStruct = LineStructs.StructMultilayerLineSymbol;
                    break;
                case "false":
                    InfoMsg("未知符号类型", "StoreHashLine");
                    break;
            }
            return StructStorage;
        }
        /// <summary>
        /// 解析标记线符号
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private StructMarkerLineSymbol StoreMarkerLine(IMarkerLineSymbol symbol)
        {
            StructMarkerLineSymbol StructStorage = new StructMarkerLineSymbol();
            StructStorage.Color = GimmeStringForColor(symbol.Color);
            StructStorage.Transparency = symbol.Color.Transparency;
            StructStorage.Width = symbol.Width;
            switch (MarkerSymbolScan(symbol.MarkerSymbol))
            {
                case "ISimpleMarkerSymbol":
                    ISimpleMarkerSymbol SMS = symbol.MarkerSymbol as ISimpleMarkerSymbol;
                    StructStorage.MarkerSymbol_SimpleMarker = StoreSimpleMarker(SMS);
                    StructStorage.kindOfMarkerStruct = MarkerStructs.StructSimpleMarkerSymbol;
                    break;
                case "ICharacterMarkerSymbol":
                    ICharacterMarkerSymbol CMS = symbol.MarkerSymbol as ICharacterMarkerSymbol;
                    StructStorage.MarkerSymbol_CharacterMarker = StoreCharacterMarker(CMS);
                    StructStorage.kindOfMarkerStruct = MarkerStructs.StructCharacterMarkerSymbol;
                    break;
                case "IPictureMarkerSymbol":
                    IPictureMarkerSymbol PMS = symbol.MarkerSymbol as IPictureMarkerSymbol;
                    StructStorage.MarkerSymbol_PictureMarker = StorePictureMarker(PMS);
                    StructStorage.kindOfMarkerStruct = MarkerStructs.StructPictureMarkerSymbol;
                    break;
                case "IArrowMarkerSymbol":
                    IArrowMarkerSymbol AMS = symbol.MarkerSymbol as IArrowMarkerSymbol;
                    StructStorage.MarkerSymbol_ArrowMarker = StoreArrowMarker(AMS);
                    StructStorage.kindOfMarkerStruct = MarkerStructs.StructArrowMarkerSymbol;
                    break;
                case "IMultiLayerMarkerSymbol":
                    IMultiLayerMarkerSymbol MLMS = symbol.MarkerSymbol as IMultiLayerMarkerSymbol;
                    StructStorage.MarkerSymbol_MultilayerMarker = StoreMultiLayerMarker(MLMS);
                    StructStorage.kindOfMarkerStruct = MarkerStructs.StructMultilayerMarkerSymbol;
                    break;
                case "false":
                    InfoMsg("未知符号类型", "StoreMarkerLine");
                    break;
            }
            return StructStorage;
        }
        /// <summary>
        /// 解析图片线符号
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private StructPictureLineSymbol StorePictureLine(IPictureLineSymbol symbol)
        {
            StructPictureLineSymbol StructStorage = new StructPictureLineSymbol();
            StructStorage.BackgroundColor = GimmeStringForColor(symbol.BackgroundColor);
            StructStorage.BackgroundTransparency = symbol.BackgroundColor.Transparency;
            StructStorage.Color = GimmeStringForColor(symbol.Color);
            StructStorage.Transparency = symbol.Color.Transparency;
            StructStorage.Offset = symbol.Offset;
            StructStorage.Picture = symbol.Picture as IPicture;
            StructStorage.Rotate = symbol.Rotate;
            StructStorage.Width = symbol.Width;
            StructStorage.XScale = symbol.XScale;
            StructStorage.YScale = symbol.YScale;
            return StructStorage;
        }
        /// <summary>
        /// 解析多图层线符号
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private StructMultilayerLineSymbol StoreMultilayerLines(IMultiLayerLineSymbol symbol)
        {
            StructMultilayerLineSymbol StructStorage = new StructMultilayerLineSymbol();
            StructStorage.MultiLineLayers = new ArrayList();
            StructStorage.LayerCount = symbol.LayerCount;
            for (int i = 0; i <= symbol.LayerCount - 1; i++)
            {
                switch (LineSymbolScan(symbol.Layer[i]))
                {
                    case "ICartographicLineSymbol":
                        ICartographicLineSymbol CLS = symbol.get_Layer(i) as ICartographicLineSymbol;
                        StructStorage.MultiLineLayers.Add(StoreCartographicLine(CLS));
                        break;
                    case "IHashLineSymbol":
                        IHashLineSymbol HLS = symbol.get_Layer(i) as IHashLineSymbol;
                        StructStorage.MultiLineLayers.Add(StoreHashLine(HLS));
                        break;
                    case "IMarkerLineSymbol":
                        IMarkerLineSymbol MLS = symbol.get_Layer(i) as IMarkerLineSymbol;
                        StructStorage.MultiLineLayers.Add(StoreMarkerLine(MLS));
                        break;
                    case "ISimpleLineSymbol":
                        ISimpleLineSymbol SLS = symbol.get_Layer(i) as ISimpleLineSymbol;
                        StructStorage.MultiLineLayers.Add(StoreSimpleLine(SLS));
                        break;
                    case "IPictureLineSymbol":
                        IPictureLineSymbol PLS = symbol.get_Layer(i) as IPictureLineSymbol;
                        StructStorage.MultiLineLayers.Add(StorePictureLine(PLS));
                        break;
                    case "IMultiLayerLineSymbol":
                        IMultiLayerLineSymbol MLLS = symbol.get_Layer(i) as IMultiLayerLineSymbol;
                        StructStorage.MultiLineLayers.Add(StoreMultilayerLines(MLLS));
                        break;
                    case "false":
                        InfoMsg("未知符号类型", "StoreMultilayerLines");
                        break;
                }
            }
            return StructStorage;
        }
        /// <summary>
        /// 解析简单面符号
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private StructSimpleFillSymbol StoreSimpleFill(ISimpleFillSymbol symbol)
        {
            StructSimpleFillSymbol StructStorage = new StructSimpleFillSymbol();
            if (symbol.Style == esriSimpleFillStyle.esriSFSHollow)
            {
                StructStorage.Color = "";
            }
            else
            {
                StructStorage.Color = GimmeStringForColor(symbol.Color);
            }
            StructStorage.Style = symbol.Style.ToString();
            StructStorage.Transparency = symbol.Color.Transparency;
            //边框符号
            switch (LineSymbolScan(symbol.Outline)) 
            {
                case "ICartographicLineSymbol":
                    ICartographicLineSymbol CLS = default(ICartographicLineSymbol);
                    CLS = symbol.Outline as ICartographicLineSymbol;
                    StructStorage.Outline_CartographicLine = StoreCartographicLine(CLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructCartographicLineSymbol;
                    break;
                case "IMarkerLineSymbol":
                    IMarkerLineSymbol MLS = default(IMarkerLineSymbol);
                    MLS = symbol.Outline as IMarkerLineSymbol;
                    StructStorage.Outline_MarkerLine = StoreMarkerLine(MLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructMarkerLineSymbol;
                    break;
                case "IHashLineSymbol":
                    IHashLineSymbol HLS = default(IHashLineSymbol);
                    HLS = symbol.Outline as IHashLineSymbol;
                    StructStorage.Outline_HashLine = StoreHashLine(HLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructHashLineSymbol;
                    break;
                case "ISimpleLineSymbol":
                    ISimpleLineSymbol SLS = default(ISimpleLineSymbol);
                    SLS = symbol.Outline as ISimpleLineSymbol;
                    StructStorage.Outline_SimpleLine = StoreSimpleLine(SLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructSimpleLineSymbol;
                    break;
                case "IPictureLineSymbol":
                    IPictureLineSymbol PLS = default(IPictureLineSymbol);
                    PLS = symbol.Outline as IPictureLineSymbol;
                    StructStorage.Outline_PictureLine = StorePictureLine(PLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructPictureLineSymbol;
                    break;
                case "IMultiLayerLineSymbol":
                    IMultiLayerLineSymbol MLLS = default(IMultiLayerLineSymbol);
                    MLLS = symbol.Outline as IMultiLayerLineSymbol;
                    StructStorage.Outline_MultiLayerLines = StoreMultilayerLines(MLLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructMultilayerLineSymbol;
                    break;
                case "false":
                    InfoMsg("无法获取线符号类型", "StoreSimpleFill");
                    break;
            }
            return StructStorage;
        }
        /// <summary>
        /// 解析点填充面符号
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private StructMarkerFillSymbol StoreMarkerFill(IMarkerFillSymbol symbol)
        {
            StructMarkerFillSymbol StructStorage = new StructMarkerFillSymbol();
            StructStorage.Color = GimmeStringForColor(symbol.Color);
            StructStorage.Transparency = symbol.Color.Transparency;
            StructStorage.GridAngle = symbol.GridAngle;
            switch (MarkerSymbolScan(symbol.MarkerSymbol))
            {
                case "ISimpleMarkerSymbol":
                    ISimpleMarkerSymbol SMS = symbol.MarkerSymbol as ISimpleMarkerSymbol;
                    StructStorage.MarkerSymbol_SimpleMarker = StoreSimpleMarker(SMS);
                    StructStorage.kindOfMarkerStruct = MarkerStructs.StructSimpleMarkerSymbol;
                    break;
                case "ICharacterMarkerSymbol":
                    ICharacterMarkerSymbol CMS = symbol.MarkerSymbol as ICharacterMarkerSymbol;
                    StructStorage.MarkerSymbol_CharacterMarker = StoreCharacterMarker(CMS);
                    StructStorage.kindOfMarkerStruct = MarkerStructs.StructCharacterMarkerSymbol;
                    break;
                case "IPictureMarkerSymbol":
                    IPictureMarkerSymbol PMS = symbol.MarkerSymbol as IPictureMarkerSymbol;
                    StructStorage.MarkerSymbol_PictureMarker = StorePictureMarker(PMS);
                    StructStorage.kindOfMarkerStruct = MarkerStructs.StructPictureMarkerSymbol;
                    break;
                case "IArrowMarkerSymbol":
                    IArrowMarkerSymbol AMS = symbol.MarkerSymbol as IArrowMarkerSymbol;
                    StructStorage.MarkerSymbol_ArrowMarker = StoreArrowMarker(AMS);
                    StructStorage.kindOfMarkerStruct = MarkerStructs.StructArrowMarkerSymbol;
                    break;
                case "IMultiLayerMarkerSymbol":
                    IMultiLayerMarkerSymbol MLMS = symbol.MarkerSymbol as IMultiLayerMarkerSymbol;
                    StructStorage.MarkerSymbol_MultilayerMarker = StoreMultiLayerMarker(MLMS);
                    StructStorage.kindOfMarkerStruct = MarkerStructs.StructMultilayerMarkerSymbol;
                    break;
                case "false":
                    InfoMsg("无法获取点符号", "StoreMarkerFill");
                    break;
            }
            //边框
            switch (LineSymbolScan(symbol.Outline))
            {
                case "ICartographicLineSymbol":
                    ICartographicLineSymbol CLS = symbol.Outline as ICartographicLineSymbol;
                    StructStorage.Outline_CartographicLine = StoreCartographicLine(CLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructCartographicLineSymbol;
                    break;
                case "IMarkerLineSymbol":
                    IMarkerLineSymbol MLS = symbol.Outline as IMarkerLineSymbol;
                    StructStorage.Outline_MarkerLine = StoreMarkerLine(MLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructMarkerLineSymbol;
                    break;
                case "IHashLineSymbol":
                    IHashLineSymbol HLS = symbol.Outline as IHashLineSymbol;
                    StructStorage.Outline_HashLine = StoreHashLine(HLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructHashLineSymbol;
                    break;
                case "ISimpleLineSymbol":
                    ISimpleLineSymbol SLS = symbol.Outline as ISimpleLineSymbol;
                    StructStorage.Outline_SimpleLine = StoreSimpleLine(SLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructSimpleLineSymbol;
                    break;
                case "IPictureLineSymbol":
                    IPictureLineSymbol PLS = symbol.Outline as IPictureLineSymbol;
                    StructStorage.Outline_PictureLine = StorePictureLine(PLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructPictureLineSymbol;
                    break;
                case "IMultiLayerLineSymbol":
                    IMultiLayerLineSymbol MLLS = symbol.Outline as IMultiLayerLineSymbol;
                    StructStorage.Outline_MultiLayerLines = StoreMultilayerLines(MLLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructMultilayerLineSymbol;
                    break;
                case "false":
                    InfoMsg("无法获取边框类型", "StoreMarkerFill");
                    break;
            }
            return StructStorage;
        }
        /// <summary>
        /// 解析线填充符号
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private StructLineFillSymbol StoreLineFill(ILineFillSymbol symbol)
        {
            StructLineFillSymbol StructStorage = new StructLineFillSymbol();
            StructStorage.Angle = symbol.Angle;
            StructStorage.Color = GimmeStringForColor(symbol.Color);
            StructStorage.Transparency = symbol.Color.Transparency;
            StructStorage.Offset = symbol.Offset;
            StructStorage.Separation = symbol.Separation;
            //填充线
            switch (LineSymbolScan(symbol.LineSymbol))
            {
                case "ICartographicLineSymbol":
                    ICartographicLineSymbol CLS = symbol.LineSymbol as ICartographicLineSymbol;
                    StructStorage.LineSymbol_CartographicLine = StoreCartographicLine(CLS);
                    StructStorage.kindOfLineStruct = LineStructs.StructCartographicLineSymbol;
                    break;
                case "IMarkerLineSymbol":
                    IMarkerLineSymbol MLS = symbol.LineSymbol as IMarkerLineSymbol;
                    StructStorage.LineSymbol_MarkerLine = StoreMarkerLine(MLS);
                    StructStorage.kindOfLineStruct = LineStructs.StructMarkerLineSymbol;
                    break;
                case "IHashLineSymbol":
                    IHashLineSymbol HLS = symbol.LineSymbol as IHashLineSymbol;
                    StructStorage.LineSymbol_HashLine = StoreHashLine(HLS);
                    StructStorage.kindOfLineStruct = LineStructs.StructHashLineSymbol;
                    break;
                case "ISimpleLineSymbol":
                    ISimpleLineSymbol SLS = symbol.LineSymbol as ISimpleLineSymbol;
                    StructStorage.LineSymbol_SimpleLine = StoreSimpleLine(SLS);
                    StructStorage.kindOfLineStruct = LineStructs.StructSimpleLineSymbol;
                    break;
                case "IPictureLineSymbol":
                    IPictureLineSymbol PLS = symbol.LineSymbol as IPictureLineSymbol;
                    StructStorage.LineSymbol_PictureLine = StorePictureLine(PLS);
                    StructStorage.kindOfLineStruct = LineStructs.StructPictureLineSymbol;
                    break;
                case "IMultiLayerLineSymbol":
                    IMultiLayerLineSymbol MLLS = symbol.LineSymbol as IMultiLayerLineSymbol;
                    StructStorage.LineSymbol_MultiLayerLines = StoreMultilayerLines(MLLS);
                    StructStorage.kindOfLineStruct = LineStructs.StructMultilayerLineSymbol;
                    break;
                case "false":
                    InfoMsg("未知线类型", "StoreLineFill");
                    break;
            }
            //边框线符号
            switch (LineSymbolScan(symbol.Outline))
            {
                case "ICartographicLineSymbol":
                    ICartographicLineSymbol CLS = symbol.Outline as ICartographicLineSymbol;
                    StructStorage.Outline_CartographicLine = StoreCartographicLine(CLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructCartographicLineSymbol;
                    break;
                case "IMarkerLineSymbol":
                    IMarkerLineSymbol MLS = symbol.Outline as IMarkerLineSymbol;
                    StructStorage.Outline_MarkerLine = StoreMarkerLine(MLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructMarkerLineSymbol;
                    break;
                case "IHashLineSymbol":
                    IHashLineSymbol HLS = symbol.Outline as IHashLineSymbol;
                    StructStorage.Outline_HashLine = StoreHashLine(HLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructHashLineSymbol;
                    break;
                case "ISimpleLineSymbol":
                    ISimpleLineSymbol SLS = symbol.Outline as ISimpleLineSymbol;
                    StructStorage.Outline_SimpleLine = StoreSimpleLine(SLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructSimpleLineSymbol;
                    break;
                case "IPictureLineSymbol":
                    IPictureLineSymbol PLS = symbol.Outline as IPictureLineSymbol;
                    StructStorage.Outline_PictureLine = StorePictureLine(PLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructPictureLineSymbol;
                    break;
                case "IMultiLayerLineSymbol":
                    IMultiLayerLineSymbol MLLS = symbol.Outline as IMultiLayerLineSymbol;
                    StructStorage.Outline_MultiLayerLines = StoreMultilayerLines(MLLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructMultilayerLineSymbol;
                    break;
                case "false":
                    InfoMsg("未知线类型", "StoreLineFill");
                    break;
            }
            return StructStorage;
        }
        /// <summary>
        /// 解析点密度填充符号
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private StructDotDensityFillSymbol StoreDotDensityFill(IDotDensityFillSymbol symbol)
        {
            StructDotDensityFillSymbol StructStorage = new StructDotDensityFillSymbol();
            ISymbolArray objSymbolArray = symbol as ISymbolArray;
            StructStorage.SymbolList = new ArrayList();
            StructStorage.BackgroundColor = GimmeStringForColor(symbol.BackgroundColor);
            StructStorage.BackgroundTransparency = symbol.BackgroundColor.Transparency;
            StructStorage.Color = GimmeStringForColor(symbol.Color);
            StructStorage.Transparency = symbol.Color.Transparency;
            StructStorage.FixedPlacement = symbol.FixedPlacement;
            StructStorage.DotSpacing = symbol.DotSpacing;
            StructStorage.SymbolCount = objSymbolArray.SymbolCount;
            for (int i = 0; i <= objSymbolArray.SymbolCount - 1; i++)
            {
                if (objSymbolArray.Symbol[i] is IMarkerSymbol) 
                {
                    IMarkerSymbol MS = objSymbolArray.Symbol[i] as IMarkerSymbol;
                    switch (MarkerSymbolScan(MS))
                    {
                        case "ISimpleMarkerSymbol":
                            ISimpleMarkerSymbol SMS = MS as ISimpleMarkerSymbol;
                            StructStorage.SymbolList.Add(StoreSimpleMarker(SMS));
                            StructStorage.SymbolList.Add(symbol.DotCount[i]);
                            break;
                        case "ICharacterMarkerSymbol":
                            ICharacterMarkerSymbol CMS = MS as ICharacterMarkerSymbol;
                            StructStorage.SymbolList.Add(StoreCharacterMarker(CMS));
                            StructStorage.SymbolList.Add(symbol.DotCount[i]);
                            break;
                        case "IPictureMarkerSymbol":
                            IPictureMarkerSymbol PMS = MS as IPictureMarkerSymbol;
                            StructStorage.SymbolList.Add(StorePictureMarker(PMS));
                            StructStorage.SymbolList.Add(symbol.DotCount[i]);
                            break;
                        case "IArrowMarkerSymbol":
                            IArrowMarkerSymbol AMS = MS as IArrowMarkerSymbol;
                            StructStorage.SymbolList.Add(StoreArrowMarker(AMS));
                            StructStorage.SymbolList.Add(symbol.DotCount[i]);
                            break;
                        case "IMultiLayerMarkerSymbol":
                            IMultiLayerMarkerSymbol MLMS = MS as IMultiLayerMarkerSymbol;
                            StructStorage.SymbolList.Add(StoreMultiLayerMarker(MLMS));
                            StructStorage.SymbolList.Add(symbol.DotCount[i]);
                            break;
                        case "false":
                            InfoMsg("未知点符号", "StoreDotDensityFill");
                            break;
                    }
                }
            }
            //边框符号
            switch (LineSymbolScan(symbol.Outline))
            {
                case "ICartographicLineSymbol":
                    ICartographicLineSymbol CLS = symbol.Outline as ICartographicLineSymbol;
                    StructStorage.Outline_CartographicLine = StoreCartographicLine(CLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructCartographicLineSymbol;
                    break;
                case "IMarkerLineSymbol":
                    IMarkerLineSymbol MLS = symbol.Outline as IMarkerLineSymbol;
                    StructStorage.Outline_MarkerLine = StoreMarkerLine(MLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructMarkerLineSymbol;
                    break;
                case "IHashLineSymbol":
                    IHashLineSymbol HLS = symbol.Outline as IHashLineSymbol;
                    StructStorage.Outline_HashLine = StoreHashLine(HLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructHashLineSymbol;
                    break;
                case "ISimpleLineSymbol":
                    ISimpleLineSymbol SLS = symbol.Outline as ISimpleLineSymbol;
                    StructStorage.Outline_SimpleLine = StoreSimpleLine(SLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructSimpleLineSymbol;
                    break;
                case "IPictureLineSymbol":
                    IPictureLineSymbol PLS = symbol.Outline as IPictureLineSymbol;
                    StructStorage.Outline_PictureLine = StorePictureLine(PLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructPictureLineSymbol;
                    break;
                case "IMultiLayerLineSymbol":
                    IMultiLayerLineSymbol MLLS = symbol.Outline as IMultiLayerLineSymbol;
                    StructStorage.Outline_MultiLayerLines = StoreMultilayerLines(MLLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructMultilayerLineSymbol;
                    break;
                case "false":
                    InfoMsg("未知线符号", "StoreLineFill");
                    break;
            }

            return StructStorage;
        }
        /// <summary>
        /// 解析图片填充符号
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private StructPictureFillSymbol StorePictureFill(IPictureFillSymbol symbol)
        {
            StructPictureFillSymbol StructStorage = new StructPictureFillSymbol();
            StructStorage.Angle = symbol.Angle;
            StructStorage.BackgroundColor = GimmeStringForColor(symbol.BackgroundColor);
            StructStorage.BackgroundTransparency = symbol.BackgroundColor.Transparency;
            StructStorage.Color = GimmeStringForColor(symbol.Color);
            StructStorage.Transparency = symbol.Color.Transparency;
            StructStorage.XScale = symbol.XScale;
            StructStorage.YScale = symbol.YScale;
            //边框
            switch (LineSymbolScan(symbol.Outline))
            {
                case "ICartographicLineSymbol":
                    ICartographicLineSymbol CLS = symbol.Outline as ICartographicLineSymbol;
                    StructStorage.Outline_CartographicLine = StoreCartographicLine(CLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructCartographicLineSymbol;
                    break;
                case "IMarkerLineSymbol":
                    IMarkerLineSymbol MLS = symbol.Outline as IMarkerLineSymbol;
                    StructStorage.Outline_MarkerLine = StoreMarkerLine(MLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructMarkerLineSymbol;
                    break;
                case "IHashLineSymbol":
                    IHashLineSymbol HLS = symbol.Outline as IHashLineSymbol;
                    StructStorage.Outline_HashLine = StoreHashLine(HLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructHashLineSymbol;
                    break;
                case "ISimpleLineSymbol":
                    ISimpleLineSymbol SLS = symbol.Outline as ISimpleLineSymbol;
                    StructStorage.Outline_SimpleLine = StoreSimpleLine(SLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructSimpleLineSymbol;
                    break;
                case "IPictureLineSymbol":
                    IPictureLineSymbol PLS = symbol.Outline as IPictureLineSymbol;
                    StructStorage.Outline_PictureLine = StorePictureLine(PLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructPictureLineSymbol;
                    break;
                case "IMultiLayerLineSymbol":
                    IMultiLayerLineSymbol MLLS = symbol.Outline as IMultiLayerLineSymbol;
                    StructStorage.Outline_MultiLayerLines = StoreMultilayerLines(MLLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructMultilayerLineSymbol;
                    break;
                case "false":
                    InfoMsg("未知线符号", "StorePictureFill");
                    break;
            }
            return StructStorage;
        }
        /// <summary>
        /// 解析梯度填充符号
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private StructGradientFillSymbol StoreGradientFill(IGradientFillSymbol symbol)
        {
            StructGradientFillSymbol StructStorage = new StructGradientFillSymbol();
            StructStorage.Color = GimmeStringForColor(symbol.Color);
            StructStorage.Transparency = symbol.Color.Transparency;
            StructStorage.Colors = GimmeArrayListForColorRamp(symbol.ColorRamp);
            StructStorage.GradientAngle = symbol.GradientAngle;
            StructStorage.GradientPercentage = symbol.GradientPercentage;
            StructStorage.IntervallCount = symbol.IntervalCount;
            //边框
            switch (LineSymbolScan(symbol.Outline))
            {
                case "ICartographicLineSymbol":
                    ICartographicLineSymbol CLS = symbol.Outline as ICartographicLineSymbol;
                    StructStorage.Outline_CartographicLine = StoreCartographicLine(CLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructCartographicLineSymbol;
                    break;
                case "IMarkerLineSymbol":
                    IMarkerLineSymbol MLS = symbol.Outline as IMarkerLineSymbol;
                    StructStorage.Outline_MarkerLine = StoreMarkerLine(MLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructMarkerLineSymbol;
                    break;
                case "IHashLineSymbol":
                    IHashLineSymbol HLS = symbol.Outline as IHashLineSymbol;
                    StructStorage.Outline_HashLine = StoreHashLine(HLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructHashLineSymbol;
                    break;
                case "ISimpleLineSymbol":
                    ISimpleLineSymbol SLS = symbol.Outline as ISimpleLineSymbol;
                    StructStorage.Outline_SimpleLine = StoreSimpleLine(SLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructSimpleLineSymbol;
                    break;
                case "IPictureLineSymbol":
                    IPictureLineSymbol PLS = symbol.Outline as IPictureLineSymbol;
                    StructStorage.Outline_PictureLine = StorePictureLine(PLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructPictureLineSymbol;
                    break;
                case "IMultiLayerLineSymbol":
                    IMultiLayerLineSymbol MLLS = symbol.Outline as IMultiLayerLineSymbol;
                    StructStorage.Outline_MultiLayerLines = StoreMultilayerLines(MLLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructMultilayerLineSymbol;
                    break;
                case "false":
                    InfoMsg("未知线类型", "StorePictureFill");
                    break;
            }
            return StructStorage;
        }
        /// <summary>
        /// 解析多图层填充符号
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private StructMultilayerFillSymbol StoreMultiLayerFill(IMultiLayerFillSymbol symbol)
        {
            StructMultilayerFillSymbol StructStorage = new StructMultilayerFillSymbol();
            StructStorage.LayerCount = symbol.LayerCount;
            StructStorage.MultiFillLayers = new ArrayList();
            int i = 0;
            for (i = 0; i <= symbol.LayerCount - 1; i++)
            {
                //每个图层的填充符号
                switch (FillSymbolScan(symbol.Layer[i]))
                {
                    case "ISimpleFillSymbol":
                        ISimpleFillSymbol SFS = symbol.get_Layer(i) as ISimpleFillSymbol;
                        StructStorage.MultiFillLayers.Add(StoreSimpleFill(SFS));
                        break;
                    case "IMarkerFillSymbol":
                        IMarkerFillSymbol MFS = symbol.get_Layer(i) as IMarkerFillSymbol;
                        StructStorage.MultiFillLayers.Add(StoreMarkerFill(MFS));
                        break;
                    case "ILineFillSymbol":
                        ILineFillSymbol LFS = symbol.get_Layer(i) as ILineFillSymbol;
                        StructStorage.MultiFillLayers.Add(StoreLineFill(LFS));
                        break;
                    case "IPictureFillSymbol":
                        IPictureFillSymbol PFS = symbol.get_Layer(i) as IPictureFillSymbol;
                        StructStorage.MultiFillLayers.Add(StorePictureFill(PFS));
                        break;
                    case "IDotDensityFillSymbol":
                        IDotDensityFillSymbol DFS = symbol.get_Layer(i) as IDotDensityFillSymbol;
                        StructStorage.MultiFillLayers.Add(StoreDotDensityFill(DFS));
                        break;
                    case "IGradientFillSymbol":
                        IGradientFillSymbol GFS = symbol.get_Layer(i) as IGradientFillSymbol;
                        StructStorage.MultiFillLayers.Add(StoreGradientFill(GFS));
                        break;
                    case "IMultiLayerFillSymbol":
                        IMultiLayerFillSymbol MLFS = symbol.get_Layer(i) as IMultiLayerFillSymbol;
                        //MLFS = symbol;
                        StructStorage.MultiFillLayers.Add(StoreMultiLayerFill(MLFS)); 
                        break;
                    case "false":
                        InfoMsg("未知填充符号类型", "StoreMultilayerFill");
                        break;
                }
            }
            return StructStorage;
        }
        /// <summary>
        /// 解析条形图统计符号
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private StructBarChartSymbol StoreBarChart(IBarChartSymbol symbol)
        {
            StructBarChartSymbol StructStorage = new StructBarChartSymbol();
            StructStorage.ShowAxes = symbol.ShowAxes;
            StructStorage.Spacing = symbol.Spacing;
            StructStorage.VerticalBars = symbol.VerticalBars;
            StructStorage.Width = symbol.Width;
            //坐标轴线符号
            switch (LineSymbolScan(symbol.Axes))
            {
                case "ICartographicLineSymbol":
                    ICartographicLineSymbol CLS = symbol.Axes as ICartographicLineSymbol;
                    StructStorage.Axes_CartographicLine = StoreCartographicLine(CLS);
                    StructStorage.kindOfAxeslineStruct = LineStructs.StructCartographicLineSymbol;
                    break;
                case "IMarkerLineSymbol":
                    IMarkerLineSymbol MLS = symbol.Axes as IMarkerLineSymbol;
                    StructStorage.Axes_MarkerLine = StoreMarkerLine(MLS);
                    StructStorage.kindOfAxeslineStruct = LineStructs.StructMarkerLineSymbol;
                    break;
                case "IHashLineSymbol":
                    IHashLineSymbol HLS = symbol.Axes as IHashLineSymbol;
                    StructStorage.Axes_HashLine = StoreHashLine(HLS);
                    StructStorage.kindOfAxeslineStruct = LineStructs.StructHashLineSymbol;
                    break;
                case "ISimpleLineSymbol":
                    ISimpleLineSymbol SLS = symbol.Axes as ISimpleLineSymbol;
                    StructStorage.Axes_SimpleLine = StoreSimpleLine(SLS);
                    StructStorage.kindOfAxeslineStruct = LineStructs.StructSimpleLineSymbol;
                    break;
                case "IPictureLineSymbol":
                    IPictureLineSymbol PLS = symbol.Axes as IPictureLineSymbol;
                    StructStorage.Axes_PictureLine = StorePictureLine(PLS);
                    StructStorage.kindOfAxeslineStruct = LineStructs.StructPictureLineSymbol;
                    break;
                case "IMultiLayerLineSymbol":
                    IMultiLayerLineSymbol MLLS = symbol.Axes as IMultiLayerLineSymbol;
                    StructStorage.Axes_MultiLayerLines = StoreMultilayerLines(MLLS);
                    StructStorage.kindOfAxeslineStruct = LineStructs.StructMultilayerLineSymbol;
                    break;
                case "false":
                    InfoMsg("未知线类型", "StoreBarChart");
                    break;
            }
            return StructStorage;
        }
        /// <summary>
        /// 解析饼状统计符号
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private StructPieChartSymbol StorePieChart(IPieChartSymbol symbol)
        {
            StructPieChartSymbol StructStorage = new StructPieChartSymbol();
            StructStorage.Clockwise = symbol.Clockwise;
            StructStorage.UseOutline = symbol.UseOutline;
            //边框
            switch (LineSymbolScan(symbol.Outline))
            {
                case "ICartographicLineSymbol":
                    ICartographicLineSymbol CLS = symbol.Outline as ICartographicLineSymbol;
                    StructStorage.Outline_CartographicLine = StoreCartographicLine(CLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructCartographicLineSymbol;
                    break;
                case "IMarkerLineSymbol":
                    IMarkerLineSymbol MLS = symbol.Outline as IMarkerLineSymbol;
                    StructStorage.Outline_MarkerLine = StoreMarkerLine(MLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructMarkerLineSymbol;
                    break;
                case "IHashLineSymbol":
                    IHashLineSymbol HLS = symbol.Outline as IHashLineSymbol;
                    StructStorage.Outline_HashLine = StoreHashLine(HLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructHashLineSymbol;
                    break;
                case "ISimpleLineSymbol":
                    ISimpleLineSymbol SLS = symbol.Outline as ISimpleLineSymbol;
                    StructStorage.Outline_SimpleLine = StoreSimpleLine(SLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructSimpleLineSymbol;
                    break;
                case "IPictureLineSymbol":
                    IPictureLineSymbol PLS = symbol.Outline as IPictureLineSymbol;
                    StructStorage.Outline_PictureLine = StorePictureLine(PLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructPictureLineSymbol;
                    break;
                case "IMultiLayerLineSymbol":
                    IMultiLayerLineSymbol MLLS = symbol.Outline as IMultiLayerLineSymbol;
                    StructStorage.Outline_MultiLayerLines = StoreMultilayerLines(MLLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructMultilayerLineSymbol;
                    break;
                case "false":
                    InfoMsg("未知线类型", "StorePictureFill");
                    break;
            }
            return StructStorage;
        }
        /// <summary>
        /// 解析表格统计符号
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private StructStackedChartSymbol StoreStackedChart(IStackedChartSymbol symbol)
        {
            StructStackedChartSymbol StructStorage = new StructStackedChartSymbol();
            StructStorage.Fixed = symbol.Fixed;
            StructStorage.UseOutline = symbol.UseOutline;
            StructStorage.VerticalBar = symbol.VerticalBar;
            StructStorage.Width = symbol.Width;
            //边框符号
            switch (LineSymbolScan(symbol.Outline))
            {
                case "ICartographicLineSymbol":
                    ICartographicLineSymbol CLS = symbol.Outline as ICartographicLineSymbol;
                    StructStorage.Outline_CartographicLine = StoreCartographicLine(CLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructCartographicLineSymbol;
                    break;
                case "IMarkerLineSymbol":
                    IMarkerLineSymbol MLS = symbol.Outline as IMarkerLineSymbol;
                    StructStorage.Outline_MarkerLine = StoreMarkerLine(MLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructMarkerLineSymbol;
                    break;
                case "IHashLineSymbol":
                    IHashLineSymbol HLS = symbol.Outline as IHashLineSymbol;
                    StructStorage.Outline_HashLine = StoreHashLine(HLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructHashLineSymbol;
                    break;
                case "ISimpleLineSymbol":
                    ISimpleLineSymbol SLS = symbol.Outline as ISimpleLineSymbol;
                    StructStorage.Outline_SimpleLine = StoreSimpleLine(SLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructSimpleLineSymbol;
                    break;
                case "IPictureLineSymbol":
                    IPictureLineSymbol PLS = symbol.Outline as IPictureLineSymbol;
                    StructStorage.Outline_PictureLine = StorePictureLine(PLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructPictureLineSymbol;
                    break;
                case "IMultiLayerLineSymbol":
                    IMultiLayerLineSymbol MLLS = symbol.Outline as IMultiLayerLineSymbol;
                    StructStorage.Outline_MultiLayerLines = StoreMultilayerLines(MLLS);
                    StructStorage.kindOfOutlineStruct = LineStructs.StructMultilayerLineSymbol;
                    break;
                case "false":
                    InfoMsg("未知线类型", "StorePictureFill");
                    break;
            }
            return StructStorage;
        }
        /// <summary>
        /// 获取文本符号信息（字体相关）
        /// </summary>
        /// <param name="symbol">符号</param>
        /// <returns></returns>
        private StructTextSymbol StoreText(ITextSymbol symbol)
        {
            StructTextSymbol StructStorage = new StructTextSymbol();
            StructStorage.Angle = symbol.Angle;
            StructStorage.Color = GimmeStringForColor(symbol.Color);
            StructStorage.Font = symbol.Font.Name;
            StructStorage.Style = "normal";
            if (symbol.Font.Italic)
            {
                StructStorage.Style = "italic";
            }
            StructStorage.Weight = "normal";
            if (symbol.Font.Bold)
            {
                StructStorage.Weight = "bold";
            }
            StructStorage.HorizontalAlignment = symbol.HorizontalAlignment.ToString();
            StructStorage.RightToLeft = symbol.RightToLeft;
            StructStorage.Size = symbol.Size;
            StructStorage.Text = symbol.Text;
            StructStorage.VerticalAlignment = symbol.VerticalAlignment.ToString();
            return StructStorage;
        }

        #endregion

        /// <summary>
        /// 分析符号信息主函数
        /// </summary>
        /// <returns></returns>
        private bool CentralProcessingFunc()
        {
            frmMotherform.CHLabelTop("正在分析ArcMap符号...");
            bool blnAnswer = false;
            Output_SLD objOutputSLD;

            if (GetMap() == false)//获取地图文档
            {
                MyTermination();
                return false;
            }
            if (AnalyseLayerSymbology() == false)//分析地图文档中的图层符号信息
            {
                MyTermination();
                return false;
            }

            if (string.IsNullOrEmpty(m_cFilename))
            {
                frmMotherform.CHLabelTop(string.Format("ArcMap符号分析完成"));
                blnAnswer = MessageBox.Show("请先选择SLD文件保存路径", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes;
                if (blnAnswer)
                {
                    if (File.Exists(frmMotherform.GetSLDFileFromConfigXML))
                    {
                        frmMotherform.dlgSave.InitialDirectory = frmMotherform.GetSLDFileFromConfigXML;
                    }
                    if (frmMotherform.dlgSave.ShowDialog() == DialogResult.OK)
                    {
                        frmMotherform.dlgSave.CheckFileExists = false;
                        frmMotherform.dlgSave.CheckPathExists = true;
                        frmMotherform.dlgSave.DefaultExt = "sld";
                        frmMotherform.dlgSave.Filter = "SLD-files (*.sld)|*.sld";
                        frmMotherform.dlgSave.AddExtension = true;
                        frmMotherform.dlgSave.InitialDirectory = System.IO.Path.GetDirectoryName(m_cFilename);
                        frmMotherform.dlgSave.OverwritePrompt = true;
                        frmMotherform.dlgSave.CreatePrompt = false;
                        if (frmMotherform.dlgSave.ShowDialog() == DialogResult.OK)
                        {
                            m_cFilename = frmMotherform.dlgSave.FileName;
                            frmMotherform.txtFileName.Text = m_cFilename;
                        }
                        objOutputSLD = new Output_SLD(frmMotherform, this, m_cFilename); //输出SLD文件
                    }
                    else
                    {
                        MyTermination();
                    }
                }
                else
                {
                    MyTermination();
                }
            }
            else
            {
                objOutputSLD = new Output_SLD(frmMotherform, this, m_cFilename);//输出SLD文件
            }
            frmMotherform.CHLabelBottom("");
            frmMotherform.CHLabelSmall("");
            return false;
        }

        #region 
        /// <summary>
        /// 获取地图文档信息
        /// </summary>
        /// <returns></returns>
		private bool GetMap()
        {
            frmMotherform.CHLabelTop(string.Format("获取当前的地图信息..."));
            try
            {
                if (m_ObjDoc.Maps.Count > 1)
                {
                    if (MessageBox.Show(string.Format("当前地图文档中的地图过多"), "选择一个地图", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        m_ObjMap = m_ObjDoc.FocusMap;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    m_ObjMap = m_ObjDoc.FocusMap;
                    return true;
                }
            }
            catch (Exception ex)
            {
                ErrorMsg(string.Format("获取地图文档失败!"), ex.Message, ex.StackTrace, "GetMap");
                return false;
            }
            return false;
        }
        /// <summary>
        /// 地图符号预分析
        /// </summary>
        /// <returns></returns>
		private bool AnalyseLayerSymbology()
        {
            ILayer objLayer = default(ILayer);
            int iNumberLayers = 0;
            string cLayerName = "";
            ISymbol objFstOrderSymbol;

            m_StrProject = new ProjectClass();

            iNumberLayers = m_ObjMap.LayerCount;
            try
            {
                for (int i = 0; i <= iNumberLayers - 1; i++)
                {
                    objLayer = m_ObjMap.Layer[i];
                    cLayerName = objLayer.Name;
                    if (frmMotherform.m_bAllLayers == false && objLayer.Visible == false)
                    {
                        frmMotherform.CHLabelBottom(string.Format("图层【{0}】不可见，不进行分析", cLayerName));
                    }
                    else
                    {
                        frmMotherform.CHLabelBottom(string.Format("正在分析图层【{0}】...", cLayerName));
                        SpreadLayerStructure(objLayer);
                    }
                    frmMotherform.CHLabelSmall("");
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorMsg("解析图层符号失败", ex.Message, ex.StackTrace, "AnalyseLayerSymbology");
                return false;
            }
        }
        /// <summary>
        /// 图层转换
        /// </summary>
        /// <param name="objLayer"></param>
        private void SpreadLayerStructure(ILayer objLayer)
        {
            try
            {
                //如果是图层组，则需要嵌套调用
                if (objLayer is IGroupLayer)
                {
                    int j = 0;
                    IGroupLayer objGRL = objLayer as IGroupLayer; ;
                    ICompositeLayer objCompLayer = objGRL as ICompositeLayer; ;
                    for (j = 0; j <= objCompLayer.Count - 1; j++)
                    {
                        SpreadLayerStructure(objCompLayer.Layer[j]);
                    }
                }
                //如果是要素图层，则进行分析
                else if (objLayer is IFeatureLayer)
                {
                    if (objLayer is IGeoFeatureLayer)
                    {
                        IGeoFeatureLayer objGFL = objLayer as IGeoFeatureLayer;
                        ptRenderFactory renderFac = new ptRenderFactory(objGFL.Renderer, objLayer);
                        m_StrProject.m_LayerRender.Add(objLayer.Name, renderFac.GetRenderLayer());

                        //唯一值渲染
                        if (objGFL.Renderer is IUniqueValueRenderer)
                        {
                            IUniqueValueRenderer objRenderer = objGFL.Renderer as IUniqueValueRenderer;

                            //m_StrProject.m_LayerRender.Add(objLayer.Name,StoreStructUVRenderer(objRenderer, objLayer as IFeatureLayer));
                        }
                        //简单渲染
                        if (objGFL.Renderer is ISimpleRenderer)
                        {
                            ISimpleRenderer objRenderer = objGFL.Renderer as ISimpleRenderer;
                            ISymbol pSymbol = objRenderer.Symbol;
                            string tempStr = pSymbol.GetType().Name;
                            //m_StrProject.m_LayerRender.Add(objLayer.Name,StoreStructSimpleRenderer(objRenderer, objLayer as IFeatureLayer));
                        }
                        //分类图表渲染
                        if (objGFL.Renderer is IClassBreaksRenderer)
                        {
                            IClassBreaksRenderer objRenderer = objGFL.Renderer as IClassBreaksRenderer;
                            //m_StrProject.m_LayerRender.Add(objLayer.Name,StoreStructCBRenderer(objRenderer, objLayer as IFeatureLayer));
                            
                        }
                    }

                }
                //非要素图层和其他图层不分析
                else
                {
                    InfoMsg(string.Format("图层符号类型不支持"), "SpreadLayerStructure");
                    //关闭程序
                    MyTermination();
                }
            }
            catch (Exception)
            {
                InfoMsg(string.Format("图层转换出错"), "SpreadLayerStructure");
            }
        }
        #endregion
        #region 公共方法
        /// <summary>
        /// 获取标记符号名称
        /// </summary>
        /// <param name="Symbol"></param>
        /// <returns></returns>
        private string MarkerSymbolScan(IMarkerSymbol Symbol)
        {
            string cValue = "";
            if (Symbol is ISimpleMarkerSymbol)
            {
                cValue = "ISimpleMarkerSymbol";
                return cValue;
            }
            else if (Symbol is ICartographicMarkerSymbol)
            {
                ICartographicMarkerSymbol ICMS = default(ICartographicMarkerSymbol);
                ICMS = Symbol as ICartographicMarkerSymbol;
                if (ICMS is ICharacterMarkerSymbol)
                {
                    cValue = "ICharacterMarkerSymbol";
                    return cValue;
                }
                else if (ICMS is IPictureMarkerSymbol)
                {
                    cValue = "IPictureMarkerSymbol";
                    return cValue;
                }
            }
            else if (Symbol is IArrowMarkerSymbol)
            {
                cValue = "IArrowMarkerSymbol";
                return cValue;
            }
            else if (Symbol is IMultiLayerMarkerSymbol)
            {
                cValue = "IMultiLayerMarkerSymbol";
                return cValue;
            }
            else
            {
                cValue = "false";
                return cValue;
            }
            return cValue;
        }
        /// <summary>
        /// 获取线符号接口名称
        /// </summary>
        /// <param name="Symbol"></param>
        /// <returns></returns>
		private string LineSymbolScan(ILineSymbol Symbol)
        {
            string cValue = "";
            bool bSwitch;
            bSwitch = false;
            if (Symbol is ICartographicLineSymbol)
            {
                ICartographicLineSymbol ICLS = default(ICartographicLineSymbol);
                ICLS = Symbol as ICartographicLineSymbol;
                if (ICLS is IHashLineSymbol)
                {
                    cValue = "IHashLineSymbol";
                    bSwitch = true;
                    return cValue;
                }
                else if (ICLS is IMarkerLineSymbol)
                {
                    cValue = "IMarkerLineSymbol";
                    bSwitch = true;
                    return cValue;
                }
                if (bSwitch == false)
                {
                    cValue = "ICartographicLineSymbol";
                    return cValue;
                }
                return cValue;
            }
            else if (Symbol is ISimpleLineSymbol)
            {
                cValue = "ISimpleLineSymbol";
                return cValue;
            }
            else if (Symbol is IPictureLineSymbol)
            {
                cValue = "IPictureLineSymbol";
                return cValue;
            }
            else if (Symbol is IMultiLayerLineSymbol)
            {
                cValue = "IMultiLayerLineSymbol";
                return cValue;
            }
            else
            {
                cValue = "false";
                return cValue;
            }
            return cValue;
        }
        /// <summary>
        /// 获取填充符号接口名称
        /// </summary>
        /// <param name="Symbol"></param>
        /// <returns></returns>
		private string FillSymbolScan(IFillSymbol Symbol)
        {
            string cValue = "";
            if (Symbol is ISimpleFillSymbol)
            {
                cValue = "ISimpleFillSymbol";
                return cValue;
            }
            else if (Symbol is IMarkerFillSymbol)
            {
                cValue = "IMarkerFillSymbol";
                return cValue;
            }
            else if (Symbol is ILineFillSymbol)
            {
                cValue = "ILineFillSymbol";
                return cValue;
            }
            else if (Symbol is IDotDensityFillSymbol)
            {
                cValue = "IDotDensityFillSymbol";
                return cValue;
            }
            else if (Symbol is IPictureFillSymbol)
            {
                cValue = "IPictureFillSymbol";
                return cValue;
            }
            else if (Symbol is IGradientFillSymbol)
            {
                cValue = "IGradientFillSymbol";
                return cValue;
            }
            else if (Symbol is IMultiLayerFillSymbol)
            {
                cValue = "IMultiLayerFillSymbol";
                return cValue;
            }
            else
            {
                cValue = "false";
                return cValue;
            }
        }
        /// <summary>
        /// 获取3D图标符号接口名称
        /// </summary>
        /// <param name="Symbol"></param>
        /// <returns></returns>
        private string IIIDChartSymbolScan(I3DChartSymbol Symbol)
        {
            string cValue = "";
            if (Symbol is IBarChartSymbol)
            {
                cValue = "IBarChartSymbol";
                return cValue;
            }
            else if (Symbol is IPieChartSymbol)
            {
                cValue = "IPieChartSymbol";
                return cValue;
            }
            else if (Symbol is IStackedChartSymbol)
            {
                cValue = "IStackedChartSymbol";
                return cValue;
            }
            else
            {
                cValue = "false";
                return cValue;
            }
        }
        /// <summary>
        /// 获取唯一值
        /// </summary>
        /// <param name="Table">数据表</param>
        /// <param name="FieldName">字段名称</param>
        /// <returns></returns>
        private bool GimmeUniqeValuesForFieldname(ITable Table, string FieldName)
        {
            IQueryDef pQueryDef = default(IQueryDef);
            IRow pRow = default(IRow);
            ICursor pCursor = default(ICursor);
            IFeatureWorkspace pFeatureWorkspace = default(IFeatureWorkspace);
            IDataset pDataset = default(IDataset);
            ArrayList alUniqueVal = new ArrayList();
            try
            {
                pDataset = Table as IDataset;
                pFeatureWorkspace = pDataset.Workspace as IFeatureWorkspace;
                pQueryDef = pFeatureWorkspace.CreateQueryDef();
                pQueryDef.Tables = pDataset.Name;
                pQueryDef.SubFields = "DISTINCT(" + FieldName + ")";
                pCursor = pQueryDef.Evaluate();
            
                pRow = pCursor.NextRow();
                while (!(pRow == null))
                {
                    alUniqueVal.Add(pRow.Value[0]);
                    pRow = pCursor.NextRow();
                }
                m_alClassifiedFields.Add(alUniqueVal);
                return true;
            }
            catch (Exception ex)
            {
                ErrorMsg("获取唯一值出错", ex.Message, ex.StackTrace, "GimmeUniqeValuesForFieldname");
                return false;
            }
        }
        /// <summary>
        /// 获取唯一值
        /// </summary>
        /// <param name="Table">数据表</param>
        /// <param name="FieldName">字段名称</param>
        /// <param name="JoinedTables">连接表</param>
        /// <returns></returns>
        private bool GimmeUniqeValuesForFieldname(ITable Table, string FieldName, ArrayList JoinedTables)
        {
            IQueryDef pQueryDef = default(IQueryDef);
            IRow pRow = default(IRow);
            ICursor pCursor = default(ICursor);
            IFeatureWorkspace pFeatureWorkspace = default(IFeatureWorkspace);
            IDataset pDataset = default(IDataset);
            ArrayList alUniqueVal = new ArrayList();
            try
            {
                string cMember = "";
                foreach (string tempLoopVar_cMember in JoinedTables)
                {
                    cMember = tempLoopVar_cMember;
                    cMember = "," + cMember;
                }

                pDataset = Table as IDataset;
                pFeatureWorkspace = pDataset.Workspace as IFeatureWorkspace;
                pQueryDef = pFeatureWorkspace.CreateQueryDef();
                pQueryDef.Tables = pDataset.Name + cMember;
                pQueryDef.SubFields = "DISTINCT(" + FieldName + ")";
                pCursor = pQueryDef.Evaluate();
                pRow = pCursor.NextRow();
                while (!(pRow == null))
                {
                    alUniqueVal.Add(pRow.Value[0]);
                    pRow = pCursor.NextRow();
                }
                m_alClassifiedFields.Add(alUniqueVal);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.Source + " " + ex.StackTrace);
                return false;
            }

        }
        /// <summary>
        /// 从shape文件中获取唯一值
        /// </summary>
        /// <param name="Table"></param>
        /// <param name="FieldNames"></param>
        private void GimmeUniqueValuesFromShape(ITable Table, ArrayList FieldNames)
        {
            IQueryFilter pQueryFilter = new QueryFilter();

            ICursor pCursor = default(ICursor);
            IDataStatistics pData = new DataStatistics();
            try
            {
                for (int i = 0; i <= FieldNames.Count - 1; i++)
                {
                    this.frmMotherform.CHLabelSmall("正在处理字段 " + System.Convert.ToString(i + 1) + " 中 " + System.Convert.ToString(FieldNames.Count));
                    pData.Field = FieldNames[i].ToString();
                    pQueryFilter.SubFields = FieldNames[i].ToString();
                    pCursor = Table.Search(pQueryFilter, false);
                    pData.Cursor = pCursor;
                    frmMotherform.DoEvents();
                    IEnumerator objEnum = pData.UniqueValues;
                    ArrayList al = new ArrayList();
                    objEnum.MoveNext();
                    while (!(objEnum.Current == null))
                    {
                        al.Add(objEnum.Current);
                        objEnum.MoveNext();
                    }
                    al.Sort();
                    m_alClassifiedFields.Add(al);
                }
            }
            catch (Exception ex)
            {
                ErrorMsg("获取唯一值出错", ex.Message, ex.StackTrace, "GimmeUniqueValuesFromShape");
                MyTermination();
            }
        }
        private bool SortCursorRowValues(ArrayList al)
        {
            switch (al.Count)
            {
                case 2:
                    m_al1.Add(al[0]);
                    m_al2.Add(al[1]);
                    break;
                case 3:
                    m_al1.Add(al[0]);
                    m_al2.Add(al[1]);
                    m_al3.Add(al[2]);
                    break;
            }
            return default(bool);
        }
        /// <summary>
        /// 获取唯一值渲染中多字段的某个符号的字段值列表
        /// </summary>
        /// <param name="value"></param>
        /// <param name="FieldDelimiter"></param>
        /// <returns></returns>
        private ArrayList GimmeSeperateFieldValues(string value, string FieldDelimiter)
        {
            ArrayList alSepValues = new ArrayList();
            try
            {
                switch (m_alClassifiedFields.Count)
                {
                    case 2:
                        ArrayList al1_1 = (ArrayList)(m_alClassifiedFields[0]);
                        ArrayList al2_1 = (ArrayList)(m_alClassifiedFields[1]);

                        for (int i_1 = 0; i_1 <= al1_1.Count - 1; i_1++)
                        {
                            for (int j_1 = 0; j_1 <= al2_1.Count - 1; j_1++)
                            {
                                if (((al1_1[i_1]).ToString() + FieldDelimiter + (al2_1[j_1]).ToString()) == value)
                                {
                                    alSepValues.Add(al1_1[i_1]);
                                    alSepValues.Add(al2_1[j_1]);
                                    return alSepValues;
                                }
                            }
                        }
                        break;
                    case 3:
                        ArrayList al1 = (ArrayList)(m_alClassifiedFields[0]);
                        ArrayList al2 = (ArrayList)(m_alClassifiedFields[1]);
                        ArrayList al3 = (ArrayList)(m_alClassifiedFields[2]);
                        for (int i = 0; i <= al1.Count - 1; i++)
                        {
                            for (int j = 0; j <= al2.Count - 1; j++)
                            {
                                for (int k = 0; k <= al3.Count - 1; k++)
                                {
                                    if (((al1[i]).ToString() + FieldDelimiter + (al2[j]).ToString() + FieldDelimiter + (al3[k]).ToString()) == value)
                                    {
                                        alSepValues.Add(al1[i]);
                                        alSepValues.Add(al2[j]);
                                        alSepValues.Add(al3[k]);
                                        return alSepValues;
                                    }
                                }
                            }
                        }
                        break;
                }
                return alSepValues;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return alSepValues;
            }
        }
        /// <summary>
        /// 获取符号颜色数组
        /// </summary>
        /// <param name="ColorRamp"></param>
        /// <returns></returns>
		private ArrayList GimmeArrayListForColorRamp(IColorRamp ColorRamp)
        {
            IEnumColors EColors = ColorRamp.Colors;
            ArrayList AL = default(ArrayList);
            for (int i = 0; i <= ColorRamp.Size - 1; i++)
            {
                AL.Add(GimmeStringForColor(EColors.Next()));
            }
            return AL;
        }
        /// <summary>
        /// 获取颜色字符串
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
		private string GimmeStringForColor(IColor color)
        {
            string cCol = "";
            string cRed = "";
            string cGreen = "";
            string cBlue = "";
            IRgbColor objRGB;
            if (color == null) return cCol;
            if (color.Transparency == 0)
            {
                cCol = "";
            }
            else
            {
                objRGB = new RgbColor();

                objRGB.RGB = color.RGB;
                //十进制颜色数字需要转换成16进制
                cRed = CheckDigits(objRGB.Red.ToString("X"));
                cGreen = CheckDigits(objRGB.Green.ToString("X"));
                cBlue = CheckDigits(objRGB.Blue.ToString("X"));
                cCol = "#" + cRed + cGreen + cBlue;
            }

            return cCol;
        }
        /// <summary>
        /// 16进制颜色补位
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		private string CheckDigits(string value)
        {
            string cReturn = value;
            if (cReturn.Length == 1)
            {
                cReturn = cReturn.Insert(0, "0");
            }
            return cReturn;
        }
        /// <summary>
        /// 根据图层获取标注信息符号
        /// </summary>
        /// <param name="objLayer">图层</param>
        /// <returns></returns>
		private StructAnnotation GetAnnotation(IFeatureLayer objLayer)
        {
            StructAnnotation annotation = new StructAnnotation();
            annotation.PropertyName = "";

            if (objLayer is IGeoFeatureLayer)
            {
                IGeoFeatureLayer objGFL = objLayer as IGeoFeatureLayer;
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
                            annotation.IsSingleProperty = true;
                            annotation.PropertyName = System.Convert.ToString(labelProps.Expression.Replace("[", "").Replace("]", ""));
                            annotation.TextSymbol = StoreText(labelProps.Symbol);
                        }
                    }
                }
            }
            return annotation;
        }
        /// <summary>
        /// 错误消息处理
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="ExMessage"></param>
        /// <param name="Stack"></param>
        /// <param name="FunctionName"></param>
        /// <returns></returns>
		private object ErrorMsg(string Message, string ExMessage, string Stack, string FunctionName)
        {
            MessageBox.Show(Message + "\r\n" + ExMessage + "\r\n" + Stack,FunctionName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            MyTermination();
            return null;
        }
        /// <summary>
        /// 消息处理
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="FunctionName"></param>
        /// <returns></returns>
		private object InfoMsg(string Message, string FunctionName)
        {
            MessageBox.Show(Message, FunctionName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return null;
        }
        /// <summary>
        /// 退出程序
        /// </summary>
        /// <returns></returns>
		public void MyTermination()
        {
            if (frmMotherform != null)
            {
                frmMotherform.Close();
            }
        }

        #endregion
    }
}

