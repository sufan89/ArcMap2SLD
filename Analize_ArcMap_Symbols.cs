using System;
using System.Drawing;
using System.Diagnostics;
using System.Data;
using Microsoft.VisualBasic;
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
namespace ArcGIS_SLD_Converter
{
	public class Analize_ArcMap_Symbols
	{
		#region 全局变量
		private IMxDocument m_ObjDoc; 
		private IApplication m_ObjApp; 
		private IAppROT m_ObjAppROT; 
		private IObjectFactory m_ObjObjectCreator; 
		private IMap m_ObjMap; 
		private Motherform frmMotherform; 
		internal StructProject m_StrProject; 
		private ArrayList m_al1; 
		private ArrayList m_al2;
		private ArrayList m_al3;
		private ArrayList m_alClassifiedFields; 
		private string m_cFilename;
        #endregion

		#region Enums
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
        /// 标记类型
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

#region Datenstrukturen

        public class ptRender
        {
            
        }
        public class ptSymbol
        {
 
        }
		internal struct StructProject
		{
			public ArrayList LayerList; 
			public int LayerCount; 
		}
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
		public Analize_ArcMap_Symbols(Motherform value, string Filename)
		{
			m_cFilename = Filename;
			frmMotherform = value; 
			m_ObjApp = null;
			CentralProcessingFunc();
		}
		private void AddOneToLayerNumber()
		{
			m_StrProject.LayerCount++;
		}
		
#endregion
#region 属性信息
		
		/// <summary>
        /// 获取项目信息
        /// </summary>
        public object GetProjectData
		        {
			        get
			        {
				        return m_StrProject;
			        }
		        }
#endregion
#region Speicherfuntionen der Datenstrukturen		
		private StructSimpleRenderer StoreStructSimpleRenderer(ISimpleRenderer Renderer, IFeatureLayer Layer)
		{
			StructSimpleRenderer strRenderer = new StructSimpleRenderer();
			ISymbol objFstOrderSymbol = default(ISymbol);
			IDataset objDataset = default(IDataset);
			objDataset = Layer.FeatureClass as IDataset;
			strRenderer.SymbolList = new ArrayList();
			try
			{
				strRenderer.LayerName = Layer.Name;
				strRenderer.DatasetName = objDataset.Name;
				strRenderer.Annotation = GetAnnotation(Layer);
				objFstOrderSymbol = Renderer.Symbol; 
				if (objFstOrderSymbol is ITextSymbol)
				{
					StructTextSymbol strTS = new StructTextSymbol();
					ITextSymbol objSymbol = default(ITextSymbol);
					objSymbol = objFstOrderSymbol as ITextSymbol;
					strTS = StoreText(objSymbol);
					strTS.Label = Renderer.Label;
					strRenderer.SymbolList.Add(strTS);
					
				}
				if (objFstOrderSymbol is IMarkerSymbol)
				{
					strRenderer.FeatureCls = FeatureClass.PointFeature;
					IMarkerSymbol objSymbol = default(IMarkerSymbol);
                    objSymbol = objFstOrderSymbol as IMarkerSymbol;
					switch (MarkerSymbolScan(objSymbol))
					{
						case "ISimpleMarkerSymbol":
							ISimpleMarkerSymbol SMS = default(ISimpleMarkerSymbol);
                            SMS = objSymbol as ISimpleMarkerSymbol;
							StructSimpleMarkerSymbol strSMS = new StructSimpleMarkerSymbol();
							strSMS = StoreSimpleMarker(SMS);
							strSMS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strSMS);
							break;
							
						case "ICharacterMarkerSymbol":
							ICharacterMarkerSymbol CMS = default(ICharacterMarkerSymbol);
							CMS = objSymbol as ICharacterMarkerSymbol ;
							StructCharacterMarkerSymbol strCMS = new StructCharacterMarkerSymbol();
							strCMS = StoreCharacterMarker(CMS);
							strCMS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strCMS);
							break;
							
						case "IPictureMarkerSymbol":
							IPictureMarkerSymbol PMS = default(IPictureMarkerSymbol);
                            PMS = objSymbol as IPictureMarkerSymbol;
							StructPictureMarkerSymbol strPMS = new StructPictureMarkerSymbol();
							strPMS = StorePictureMarker(PMS);
							strPMS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strPMS);
							break;
						case "IArrowMarkerSymbol":
							IArrowMarkerSymbol AMS = default(IArrowMarkerSymbol);
                            AMS = objSymbol as IArrowMarkerSymbol;
							StructArrowMarkerSymbol strAMS = new StructArrowMarkerSymbol();
							strAMS = StoreArrowMarker(AMS);
							strAMS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strAMS);
							break;
						case "IMultiLayerMarkerSymbol":
							IMultiLayerMarkerSymbol MLMS = default(IMultiLayerMarkerSymbol);
                            MLMS = objSymbol as IMultiLayerMarkerSymbol;
							StructMultilayerMarkerSymbol strMLMS = new StructMultilayerMarkerSymbol();
							strMLMS = StoreMultiLayerMarker(MLMS);
							strMLMS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strMLMS);
							break;
						case "false":
							InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreStructLayer");
							break;
					}
				}
				if (objFstOrderSymbol is ILineSymbol)
				{
					strRenderer.FeatureCls = FeatureClass.LineFeature;
					ILineSymbol objSymbol = default(ILineSymbol);
                    objSymbol = objFstOrderSymbol as ILineSymbol;
					switch (LineSymbolScan(objSymbol))
					{
						case "ICartographicLineSymbol":
							ICartographicLineSymbol CLS = default(ICartographicLineSymbol);
                            CLS = objSymbol as ICartographicLineSymbol;
							StructCartographicLineSymbol strCLS = new StructCartographicLineSymbol();
							strCLS = StoreCartographicLine(CLS);
							strCLS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strCLS);
							break;
						case "IHashLineSymbol":
							IHashLineSymbol HLS = default(IHashLineSymbol);
                            HLS = objSymbol as IHashLineSymbol;
							StructHashLineSymbol strHLS = new StructHashLineSymbol();
							strHLS = StoreHashLine(HLS);
							strHLS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strHLS);
							break;
						case "IMarkerLineSymbol":
							IMarkerLineSymbol MLS = default(IMarkerLineSymbol);
                            MLS = objSymbol as IMarkerLineSymbol;
							StructMarkerLineSymbol strMLS = new StructMarkerLineSymbol();
							strMLS = StoreMarkerLine(MLS);
							strMLS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strMLS);
							break;
						case "ISimpleLineSymbol":
							ISimpleLineSymbol SLS = default(ISimpleLineSymbol);
                            SLS = objSymbol as ISimpleLineSymbol;
							StructSimpleLineSymbol strSLS = new StructSimpleLineSymbol();
							strSLS = StoreSimpleLine(SLS);
							strSLS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strSLS);
							break;
						case "IPictureLineSymbol":
							IPictureLineSymbol PLS = default(IPictureLineSymbol);
                            PLS = objSymbol as IPictureLineSymbol;
							StructPictureLineSymbol strPLS = new StructPictureLineSymbol();
							strPLS = StorePictureLine(PLS);
							strPLS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strPLS);
							break;
						case "IMultiLayerLineSymbol":
							IMultiLayerLineSymbol MLLS = default(IMultiLayerLineSymbol);
                            MLLS = objSymbol as IMultiLayerLineSymbol;
							StructMultilayerLineSymbol strMLLS = new StructMultilayerLineSymbol();
							strMLLS = StoreMultilayerLines(MLLS);
							strMLLS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strMLLS);
							break;
						case "false":
							InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreStructLayer");
							break;
					}
					
				}
				if (objFstOrderSymbol is IFillSymbol)
				{
					strRenderer.FeatureCls = FeatureClass.PolygonFeature;
					IFillSymbol objSymbol = default(IFillSymbol);
                    objSymbol = objFstOrderSymbol as IFillSymbol;
					switch (FillSymbolScan(objSymbol))
					{
						case "ISimpleFillSymbol":
							ISimpleFillSymbol SFS = default(ISimpleFillSymbol);
                            SFS = objSymbol as ISimpleFillSymbol;
							StructSimpleFillSymbol strSFS = new StructSimpleFillSymbol();
							strSFS = StoreSimpleFill(SFS);
							strSFS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strSFS);
							break;
						case "IMarkerFillSymbol":
							IMarkerFillSymbol MFS = default(IMarkerFillSymbol);
                            MFS = objSymbol as IMarkerFillSymbol;
							StructMarkerFillSymbol strMFS = new StructMarkerFillSymbol();
							strMFS = StoreMarkerFill(MFS);
							strMFS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strMFS);
							break;
						case "ILineFillSymbol":
							ILineFillSymbol LFS = default(ILineFillSymbol);
                            LFS = objSymbol as ILineFillSymbol;
							StructLineFillSymbol strLFS = new StructLineFillSymbol();
							strLFS = StoreLineFill(LFS);
							strLFS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strLFS);
							break;
						case "IDotDensityFillSymbol":
							IDotDensityFillSymbol DFS = default(IDotDensityFillSymbol);
                            DFS = objSymbol as IDotDensityFillSymbol;
							StructDotDensityFillSymbol strDFS = new StructDotDensityFillSymbol();
							strDFS = StoreDotDensityFill(DFS);
							strDFS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strDFS);
							break;
						case "IPictureFillSymbol":
							IPictureFillSymbol PFS = default(IPictureFillSymbol);
                            PFS = objSymbol as IPictureFillSymbol;
							StructPictureFillSymbol strPFS = new StructPictureFillSymbol();
							strPFS = StorePictureFill(PFS);
							strPFS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strPFS);
							break;
						case "IGradientFillSymbol":
							IGradientFillSymbol GFS = default(IGradientFillSymbol);
                            GFS = objSymbol as IGradientFillSymbol;
							StructGradientFillSymbol strGFS = new StructGradientFillSymbol();
							strGFS = StoreGradientFill(GFS);
							strGFS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strGFS);
							break;
						case "IMultiLayerFillSymbol":
							IMultiLayerFillSymbol MLFS = default(IMultiLayerFillSymbol);
                            MLFS = objSymbol as IMultiLayerFillSymbol;
							StructMultilayerFillSymbol strMLFS = new StructMultilayerFillSymbol();
							strMLFS = StoreMultiLayerFill(MLFS);
							strMLFS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strMLFS);
							break;
						case "false":
							InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreStructLayer");
							break;
					}
				}
				if (objFstOrderSymbol is I3DChartSymbol)
				{
					I3DChartSymbol objSymbol = default(I3DChartSymbol);
                    objSymbol = objFstOrderSymbol as I3DChartSymbol;
					switch (IIIDChartSymbolScan(objSymbol))
					{
						case "IBarChartSymbol":
							IBarChartSymbol BCS = default(IBarChartSymbol);
                            BCS = objSymbol as IBarChartSymbol;
							StructBarChartSymbol strBCS = new StructBarChartSymbol();
							strBCS = StoreBarChart(BCS);
							strBCS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strBCS);
							break;
						case "IPieChartSymbol":
							IPieChartSymbol PCS = default(IPieChartSymbol);
                            PCS = objSymbol as IPieChartSymbol;
							StructPieChartSymbol strPCS = new StructPieChartSymbol();
							strPCS = StorePieChart(PCS);
							strPCS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strPCS);
							break;
						case "IStackedChartSymbol":
							IStackedChartSymbol SCS = default(IStackedChartSymbol);
                            SCS = objSymbol as IStackedChartSymbol;
							StructStackedChartSymbol strSCS = new StructStackedChartSymbol();
							strSCS = StoreStackedChart(SCS);
							strSCS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strSCS);
							break;
						case "false":
							InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreStructLayer");
							break;
					}
				}
				return strRenderer;
			}
			catch (Exception ex)
			{
				
				ErrorMsg("Fehler beim Speichern der Symbole in den Layerstrukturen", ex.Message, ex.StackTrace, "StoreStructSimpleRenderer");
                return strRenderer;
			}
		}
		private StructClassBreaksRenderer StoreStructCBRenderer(IClassBreaksRenderer Renderer, IFeatureLayer Layer)
		{
			StructClassBreaksRenderer strRenderer = new StructClassBreaksRenderer(); //Hierin wird das eine Rendererobjekt gespeichert plus zus鋞zliche Layerinformationen
			strRenderer.SymbolList = new ArrayList();
			int iNumberOfSymbols = 0; //Anzahl aller Symbole des Rendererobjekts
			iNumberOfSymbols = Renderer.BreakCount; //Anzahl der Symbole
			strRenderer.BreakCount = Renderer.BreakCount;
			ISymbol objFstOrderSymbol = default(ISymbol); //Das gerade aktuelle Symbol des durchlaufenen Rendererobjekts
			IDataset objDataset = default(IDataset);
            objDataset = Layer.FeatureClass as IDataset;
			bool bIsJoined;
			bIsJoined = false;
			
			try
			{
				strRenderer.LayerName = Layer.Name;
				strRenderer.DatasetName = objDataset.Name;
				strRenderer.Annotation = GetAnnotation(Layer);
				
				//diese Objekte dienen lediglich der 躡erpr黤ung, ob eine andere Tabelle an die Featuretable gejoint ist
				//++++++++++++++++++++++++++++++++++++++++++
				ITable pTable = default(ITable);
				IDisplayTable pDisplayTable = default(IDisplayTable);
                pDisplayTable = Layer as IDisplayTable;
				pTable = pDisplayTable.DisplayTable;
				if (pTable is IRelQueryTable) //Dann ist eine Tabelle drangejoint
				{
					bIsJoined = true;
				}
				//++++++++++++++++++++++++++++++++++++++++++
				
				if (bIsJoined == false) //Wenn eine Tabelle drangejoint wurde, ist sowieso schon der Datasetname mit dabei
				{
					//Je nachdem, welcher Zielkartendienst gew鋒lt wurde, muss Fieldname ein anderes Format besitzen
					if (frmMotherform.chkArcIMS.Checked == true)
					{
						strRenderer.FieldName = objDataset.Name + "." + Renderer.Field;
						strRenderer.NormFieldName = objDataset.Name + "." + Renderer.NormField;
					}
					else
					{
						strRenderer.FieldName = Renderer.Field;
						strRenderer.NormFieldName = Renderer.NormField;
					}
				}
				//AB HIER BEGINNEN DIE FALLUNTERSCHEIDUNGEN DER SYMBOLE
				int j = 0;
				for (j = 0; j <= iNumberOfSymbols - 1; j++)
				{
					frmMotherform.CHLabelSmall("Symbol " + (j + 1).ToString() + " von " + iNumberOfSymbols.ToString()); //Anzeige auf dem Label
					objFstOrderSymbol = Renderer.Symbol[j]; //Die Zuweisung der jeweiligen einzelnen Symbole
					IClassBreaksUIProperties objClassBreaksProp = default(IClassBreaksUIProperties);
                    objClassBreaksProp = Renderer as IClassBreaksUIProperties;
					string cLowerLimit = "";
					string cUpperLimit = "";
					cLowerLimit = (objClassBreaksProp.LowBreak[j]).ToString(); //Die Untergrenze der aktuellen Klasse
					cUpperLimit = (Renderer.Break[j]).ToString();
					
					//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
					if (objFstOrderSymbol is ITextSymbol)
					{
						StructTextSymbol strTS = new StructTextSymbol();
						ITextSymbol objSymbol = default(ITextSymbol);
                        objSymbol = objFstOrderSymbol as ITextSymbol;
						strTS = StoreText(objSymbol);
						//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
						strTS.Label = Renderer.Label[j];
						strTS.LowerLimit = double.Parse(cLowerLimit);
						strTS.UpperLimit = double.Parse(cUpperLimit);
						strRenderer.SymbolList.Add(strTS);
						
					}
					//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx()
					if (objFstOrderSymbol is IMarkerSymbol)
					{
						strRenderer.FeatureCls = FeatureClass.PointFeature;
						IMarkerSymbol objSymbol = default(IMarkerSymbol);
                        objSymbol = objFstOrderSymbol as IMarkerSymbol;
						switch (MarkerSymbolScan(objSymbol))
						{
							case "ISimpleMarkerSymbol":
								ISimpleMarkerSymbol SMS = default(ISimpleMarkerSymbol);
                                SMS = objSymbol as ISimpleMarkerSymbol;
								StructSimpleMarkerSymbol strSMS = new StructSimpleMarkerSymbol();
								strSMS = StoreSimpleMarker(SMS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strSMS.Label = Renderer.Label[j];
								strSMS.LowerLimit = double.Parse(cLowerLimit);
								strSMS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strSMS);
								break;
								
							case "ICharacterMarkerSymbol":
								ICharacterMarkerSymbol CMS = default(ICharacterMarkerSymbol);
                                CMS = objSymbol as ICharacterMarkerSymbol;
								StructCharacterMarkerSymbol strCMS = new StructCharacterMarkerSymbol();
								strCMS = StoreCharacterMarker(CMS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strCMS.Label = Renderer.Label[j];
								strCMS.LowerLimit = double.Parse(cLowerLimit);
								strCMS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strCMS);
								break;
								
							case "IPictureMarkerSymbol":
								IPictureMarkerSymbol PMS = default(IPictureMarkerSymbol);
                                PMS = objSymbol as IPictureMarkerSymbol;
								StructPictureMarkerSymbol strPMS = new StructPictureMarkerSymbol();
								strPMS = StorePictureMarker(PMS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strPMS.Label = Renderer.Label[j];
								strPMS.LowerLimit = double.Parse(cLowerLimit);
								strPMS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strPMS);
								break;
								
							case "IArrowMarkerSymbol":
								IArrowMarkerSymbol AMS = default(IArrowMarkerSymbol);
                                AMS = objSymbol as IArrowMarkerSymbol;
								StructArrowMarkerSymbol strAMS = new StructArrowMarkerSymbol();
								strAMS = StoreArrowMarker(AMS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strAMS.Label = Renderer.Label[j];
								strAMS.LowerLimit = double.Parse(cLowerLimit);
								strAMS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strAMS);
								break;
								
							case "IMultiLayerMarkerSymbol":
								IMultiLayerMarkerSymbol MLMS = default(IMultiLayerMarkerSymbol);
                                MLMS = objSymbol as IMultiLayerMarkerSymbol;
								StructMultilayerMarkerSymbol strMLMS = new StructMultilayerMarkerSymbol();
								strMLMS = StoreMultiLayerMarker(MLMS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strMLMS.Label = Renderer.Label[j];
								strMLMS.LowerLimit = double.Parse(cLowerLimit);
								strMLMS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strMLMS);
								break;
								
							case "false":
								InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreStructLayer");
								break;
						}
					}
					//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
					if (objFstOrderSymbol is ILineSymbol)
					{
						strRenderer.FeatureCls = FeatureClass.LineFeature;
						ILineSymbol objSymbol = default(ILineSymbol);
                        objSymbol = objFstOrderSymbol as ILineSymbol;
						switch (LineSymbolScan(objSymbol))
						{
							case "ICartographicLineSymbol":
								ICartographicLineSymbol CLS = default(ICartographicLineSymbol);
                                CLS = objSymbol as ICartographicLineSymbol;
								StructCartographicLineSymbol strCLS = new StructCartographicLineSymbol();
								strCLS = StoreCartographicLine(CLS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strCLS.Label = Renderer.Label[j];
								strCLS.LowerLimit = double.Parse(cLowerLimit);
								strCLS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strCLS);
								break;
								
							case "IHashLineSymbol":
								IHashLineSymbol HLS = default(IHashLineSymbol);
                                HLS = objSymbol as IHashLineSymbol;
								StructHashLineSymbol strHLS = new StructHashLineSymbol();
								strHLS = StoreHashLine(HLS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strHLS.Label = Renderer.Label[j];
								strHLS.LowerLimit = double.Parse(cLowerLimit);
								strHLS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strHLS);
								break;
								
							case "IMarkerLineSymbol":
								IMarkerLineSymbol MLS = default(IMarkerLineSymbol);
                                MLS = objSymbol as IMarkerLineSymbol;
								StructMarkerLineSymbol strMLS = new StructMarkerLineSymbol();
								strMLS = StoreMarkerLine(MLS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strMLS.Label = Renderer.Label[j];
								strMLS.LowerLimit = double.Parse(cLowerLimit);
								strMLS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strMLS);
								break;
								
							case "ISimpleLineSymbol":
								ISimpleLineSymbol SLS = default(ISimpleLineSymbol);
                                SLS = objSymbol as ISimpleLineSymbol;
								StructSimpleLineSymbol strSLS = new StructSimpleLineSymbol();
								strSLS = StoreSimpleLine(SLS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strSLS.Label = Renderer.Label[j];
								strSLS.LowerLimit = double.Parse(cLowerLimit);
								strSLS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strSLS);
								break;
								
							case "IPictureLineSymbol":
								IPictureLineSymbol PLS = default(IPictureLineSymbol);
                                PLS = objSymbol as IPictureLineSymbol;
								StructPictureLineSymbol strPLS = new StructPictureLineSymbol();
								strPLS = StorePictureLine(PLS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strPLS.Label = Renderer.Label[j];
								strPLS.LowerLimit = double.Parse(cLowerLimit);
								strPLS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strPLS);
								break;
								
							case "IMultiLayerLineSymbol":
								IMultiLayerLineSymbol MLLS = default(IMultiLayerLineSymbol);
                                MLLS = objSymbol as IMultiLayerLineSymbol;
								StructMultilayerLineSymbol strMLLS = new StructMultilayerLineSymbol();
								strMLLS = StoreMultilayerLines(MLLS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strMLLS.Label = Renderer.Label[j];
								strMLLS.LowerLimit = double.Parse(cLowerLimit);
								strMLLS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strMLLS);
								break;
								
							case "false":
								InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreStructLayer");
								break;
						}
						
					}
					//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
					if (objFstOrderSymbol is IFillSymbol)
					{
						strRenderer.FeatureCls = FeatureClass.PolygonFeature;
						IFillSymbol objSymbol = default(IFillSymbol);
                        objSymbol = objFstOrderSymbol as IFillSymbol;
						switch (FillSymbolScan(objSymbol))
						{
							case "ISimpleFillSymbol":
								ISimpleFillSymbol SFS = default(ISimpleFillSymbol);
                                SFS = objSymbol as ISimpleFillSymbol;
								StructSimpleFillSymbol strSFS = new StructSimpleFillSymbol();
								strSFS = StoreSimpleFill(SFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strSFS.Label = Renderer.Label[j];
								strSFS.LowerLimit = double.Parse(cLowerLimit);
								strSFS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strSFS);
								break;
								
							case "IMarkerFillSymbol":
								IMarkerFillSymbol MFS = default(IMarkerFillSymbol);
                                MFS = objSymbol as IMarkerFillSymbol;
								StructMarkerFillSymbol strMFS = new StructMarkerFillSymbol();
								strMFS = StoreMarkerFill(MFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strMFS.Label = Renderer.Label[j];
								strMFS.LowerLimit = double.Parse(cLowerLimit);
								strMFS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strMFS);
								break;
								
							case "ILineFillSymbol":
								ILineFillSymbol LFS = default(ILineFillSymbol);
                                LFS = objSymbol as ILineFillSymbol;
								StructLineFillSymbol strLFS = new StructLineFillSymbol();
								strLFS = StoreLineFill(LFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strLFS.Label = Renderer.Label[j];
								strLFS.LowerLimit = double.Parse(cLowerLimit);
								strLFS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strLFS);
								break;
								
							case "IDotDensityFillSymbol":
								IDotDensityFillSymbol DFS = default(IDotDensityFillSymbol);
                                DFS = objSymbol as IDotDensityFillSymbol;
								StructDotDensityFillSymbol strDFS = new StructDotDensityFillSymbol();
								strDFS = StoreDotDensityFill(DFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strDFS.Label = Renderer.Label[j];
								strDFS.LowerLimit = double.Parse(cLowerLimit);
								strDFS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strDFS);
								break;
								
							case "IPictureFillSymbol":
								IPictureFillSymbol PFS = default(IPictureFillSymbol);
                                PFS = objSymbol as IPictureFillSymbol;
								StructPictureFillSymbol strPFS = new StructPictureFillSymbol();
								strPFS = StorePictureFill(PFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strPFS.Label = Renderer.Label[j];
								strPFS.LowerLimit = double.Parse(cLowerLimit);
								strPFS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strPFS);
								break;
								
							case "IGradientFillSymbol":
								IGradientFillSymbol GFS = default(IGradientFillSymbol);
                                GFS = objSymbol as IGradientFillSymbol;
								StructGradientFillSymbol strGFS = new StructGradientFillSymbol();
								strGFS = StoreGradientFill(GFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strGFS.Label = Renderer.Label[j];
								strGFS.LowerLimit = double.Parse(cLowerLimit);
								strGFS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strGFS);
								break;
								
							case "IMultiLayerFillSymbol":
								IMultiLayerFillSymbol MLFS = default(IMultiLayerFillSymbol);
                                MLFS = objSymbol as IMultiLayerFillSymbol;
								StructMultilayerFillSymbol strMLFS = new StructMultilayerFillSymbol();
								strMLFS = StoreMultiLayerFill(MLFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strMLFS.Label = Renderer.Label[j];
								strMLFS.LowerLimit = double.Parse(cLowerLimit);
								strMLFS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strMLFS);
								break;
								
							case "false":
								InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreStructLayer");
								break;
						}
					}
					//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
					if (objFstOrderSymbol is I3DChartSymbol)
					{
						I3DChartSymbol objSymbol = default(I3DChartSymbol);
                        objSymbol = objFstOrderSymbol as I3DChartSymbol;
						switch (IIIDChartSymbolScan(objSymbol))
						{
							case "IBarChartSymbol":
								IBarChartSymbol BCS = default(IBarChartSymbol);
                                BCS = objSymbol as IBarChartSymbol;
								StructBarChartSymbol strBCS = new StructBarChartSymbol();
								strBCS = StoreBarChart(BCS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strBCS.Label = Renderer.Label[j];
								strBCS.LowerLimit = double.Parse(cLowerLimit);
								strBCS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strBCS);
								break;
								
							case "IPieChartSymbol":
								IPieChartSymbol PCS = default(IPieChartSymbol);
                                PCS = objSymbol as IPieChartSymbol;
								StructPieChartSymbol strPCS = new StructPieChartSymbol();
								strPCS = StorePieChart(PCS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strPCS.Label = Renderer.Label[j];
								strPCS.LowerLimit = double.Parse(cLowerLimit);
								strPCS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strPCS);
								break;
								
							case "IStackedChartSymbol":
								IStackedChartSymbol SCS = default(IStackedChartSymbol);
                                SCS = objSymbol as IStackedChartSymbol;
								StructStackedChartSymbol strSCS = new StructStackedChartSymbol();
								strSCS = StoreStackedChart(SCS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strSCS.Label = Renderer.Label[j];
								strSCS.LowerLimit = double.Parse(cLowerLimit);
								strSCS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strSCS);
								break;
								
							case "false":
								InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreStructLayer");
								break;
						}
					}
					//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
				}
				return strRenderer;
			}
			catch (Exception ex)
			{
				
				ErrorMsg("Fehler beim Speichern der Symbole in den Layerstrukturen", ex.Message, ex.StackTrace, "StoreStructCBRenderer");
                return strRenderer;
			}
		}
		private StructUniqueValueRenderer StoreStructUVRenderer(IUniqueValueRenderer Renderer, IFeatureLayer Layer)
		{
			StructUniqueValueRenderer strRenderer = new StructUniqueValueRenderer(); //Hierin wird das eine Rendererobjekt gespeichert plus zus鋞zliche Layerinformationen
			int iNumberOfSymbols = 0; //Anzahl aller Symbole des Rendererobjekts
			iNumberOfSymbols = Renderer.ValueCount; //Anzahl der Symbole
			ISymbol objFstOrderSymbol = default(ISymbol); //Das gerade aktuelle Symbol des durchlaufenen Rendererobjekts
			ArrayList alFieldNames = new ArrayList(); //Die ArrayLisit mit den Feldnamen
			bool bNoSepFieldVal; //Wenn nur nach einem Feld klassifiziert wurde muss der Flag auf true gesetzt werden
			ITable objTable = default(ITable); //Der FeatureTable, der an den Layer gebunden ist
			IDataset objDataset = default(IDataset); //Der aktuelle Dataset
            objTable = Layer.FeatureClass as ITable;
            objDataset = objTable as IDataset;
			strRenderer.SymbolList = new ArrayList();
			bNoSepFieldVal = false; //!!!! Damit nicht in die aufwendige Funktion GimmeSeperateFieldValues(..) gegangen werden mu? wenn nicht n鰐ig
			m_alClassifiedFields = new ArrayList(); //enth鋖t jeweils wiederum arraylist(s) von den normalisierten Werten aus den Feldern, nach denen klassifiziert wurde
			bool bIsJoined;
			bIsJoined = false;
			
			
			try
			{
				strRenderer.ValueCount = iNumberOfSymbols;
				strRenderer.LayerName = Layer.Name;
				strRenderer.DatasetName = objDataset.Name;
				strRenderer.Annotation = GetAnnotation(Layer);
				//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
				//Hier werden die Informationen 黚er Felder nach denen klassifiziert wurde gesammelt
				int iFieldCount = 0; //Anzahl der Spalten, nach denen klassifiziert wurde
				iFieldCount = Renderer.FieldCount;
				strRenderer.FieldCount = iFieldCount;
				if (iFieldCount > 1) //!!!! Damit nicht in die aufwendige Funktion GimmeSeperateFieldValues(..) gegangen werden mu? wenn nicht n鰐ig
				{
					bNoSepFieldVal = true; //und damit nicht die aufw鋘digen UniqueValue-Listen erzeugt werden m黶sen, wenn nicht notwendig
				}
				
				//diese Objekte dienen lediglich der 躡erpr黤ung, ob eine andere Tabelle an die Featuretable gejoint ist
				//++++++++++++++++++++++++++++++++++++++++++
				ITable pTable = default(ITable);
				IDisplayTable pDisplayTable = default(IDisplayTable);
                pDisplayTable = Layer as IDisplayTable;
				pTable = pDisplayTable.DisplayTable;
				if (pTable is IRelQueryTable) //Dann ist eine Tabelle drangejoint
				{
					bIsJoined = true;
				}
				//++++++++++++++++++++++++++++++++++++++++++
				
				//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
				//Das ganze nachfolgende Konstrukt dient nur dem Zweck, Unique-Value-Listen aus den Datenbanktabellen zu erstellen,
				//um sp鋞er mit der Funktion GimmeSeperateFieldValues die separaten Spaltenwerte, die zu einem Symbol geh鰎en, zu erhalten,
				//wenn (und nur wenn) nach mehr als 1 Wert klassifiziert wurde (da esri in diesem Fall nur einen aus allen Feldwerten
				//zusammengesetzten String liefert)
				int i = 0;
				if (bNoSepFieldVal == true) //Feldnamen (Spaltennamen) werden nur dann gespeichert, wenn nach mehr als 1 Feld klassifiziert wurde. (da sehr aufw鋘dige Sache!)
				{
					
					//Wenn der aktuelle Layer aus einem Shapefile stammt, ist das weitere Vorgehen
					//unterschiedlich zu dem Fall, wenn er aus DB's stammt:
					if (objDataset.Workspace.Type == esriWorkspaceType.esriFileSystemWorkspace)
					{
						for (i = 1; i <= iFieldCount; i++)
						{
							alFieldNames.Add(Renderer.Field[i - 1]); //Die Spaltennamen werden alle abgespeichert
						}
						GimmeUniqueValuesFromShape(objTable, alFieldNames);
						//GimmeUniqueValuesFromShape(Layer, alFieldNames)
						strRenderer.FieldNames = alFieldNames;
					}
					else
					{
						for (i = 1; i <= iFieldCount; i++)
						{
							alFieldNames.Add(Renderer.Field[i - 1]); //Die Spaltennamen werden alle abgespeichert
							//wenn eine andere Tabelle an die Featuretable gejoint ist
							if (pTable is IRelQueryTable)
							{
								// ++ Get the list of joined tables
								IRelQueryTable pRelQueryTable = default(IRelQueryTable);
								ITable pDestTable = default(ITable);
								IDataset pDataSet = default(IDataset);
								//Dim cTable As String
								ArrayList alJoinedTableNames = new ArrayList();
								while (pTable is IRelQueryTable)
								{
                                    pRelQueryTable = pTable as IRelQueryTable;
									pDestTable = pRelQueryTable.DestinationTable;
                                    pDataSet = pDestTable as IDataset;
									//cTable = cTable & pDataSet.Name
									pTable = pRelQueryTable.SourceTable;
									alJoinedTableNames.Add(pDataSet.Name);
								}
								GimmeUniqeValuesForFieldname(objTable, System.Convert.ToString(Renderer.Field[i - 1]), alJoinedTableNames); //Hier wird die Funktion aufgerufen, um die Unique Values des jew. Feldes abzuspeichern
								pTable = pDisplayTable.DisplayTable; //Zur點ksetzen des pTable, damit beim n鋍hsten Durchlauf wieder abgefragt werden kann
							}
							else //Bei nichtgejointer Tabelle  nat黵lich Aufruf der Funktion ohne die gejointen Tabellennamen
							{
								GimmeUniqeValuesForFieldname(objTable, Renderer.Field[i - 1]); //Hier wird die Funktion aufgerufen, um die Unique Values des jew. Feldes abzuspeichern
							}
						}
						strRenderer.FieldNames = alFieldNames;
					}
				}
				else
				{
					alFieldNames.Add(Renderer.Field[iFieldCount - 1]);
					strRenderer.FieldNames = alFieldNames;
				}
				
				//Je nachdem, welcher Zielkartendienst gew鋒lt wurde, muss Fieldname ein anderes Format besitzen
				if (bIsJoined == false) //Wenn eine Tabelle drangejoint wurde, ist sowieso schon der Datasetname mit dabei
				{
					int idummy;
					if (frmMotherform.chkArcIMS.Checked == true)
					{
						for (i = 0; i <= strRenderer.FieldNames.Count - 1; i++)
						{
							strRenderer.FieldNames[i] = objDataset.Name + "." + System.Convert.ToString(strRenderer.FieldNames[i]);
						}
					}
				}
				
				
				//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
				
				//AB HIER BEGINNEN DIE FALLUNTERSCHEIDUNGEN DER SYMBOLE
				int j = 0;
				for (j = 0; j <= iNumberOfSymbols - 1; j++)
				{
					frmMotherform.CHLabelSmall("Symbol " + (j + 1).ToString() + " von " + iNumberOfSymbols.ToString()); //Anzeige auf dem Label
					objFstOrderSymbol = Renderer.Symbol[Renderer.get_Value(j)]; //Die Zuweisung der jeweiligen einzelnen Symbole
					//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
					if (objFstOrderSymbol is ITextSymbol)
					{
						StructTextSymbol strTS = new StructTextSymbol();
						ITextSymbol objSymbol = default(ITextSymbol);
                        objSymbol = objFstOrderSymbol as ITextSymbol;
						strTS = StoreText(objSymbol);
						//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
						strTS.Label = Renderer.Label[Renderer.get_Value(j)];
						strTS.Fieldvalues = getUVFieldValues(Renderer, j);
						strRenderer.SymbolList.Add(strTS);
					}
					//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx()
					if (objFstOrderSymbol is IMarkerSymbol)
					{
						strRenderer.FeatureCls = FeatureClass.PointFeature;
						IMarkerSymbol objSymbol = default(IMarkerSymbol);
                        objSymbol = objFstOrderSymbol as IMarkerSymbol;
						switch (MarkerSymbolScan(objSymbol))
						{
							case "ISimpleMarkerSymbol":
								ISimpleMarkerSymbol SMS = default(ISimpleMarkerSymbol);
                                SMS = objSymbol as ISimpleMarkerSymbol;
								StructSimpleMarkerSymbol strSMS = new StructSimpleMarkerSymbol();
								strSMS = StoreSimpleMarker(SMS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strSMS.Label = Renderer.Label[Renderer.get_Value(j)];
								strSMS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strSMS);
								break;
								
							case "ICharacterMarkerSymbol":
								ICharacterMarkerSymbol CMS = default(ICharacterMarkerSymbol);
                                CMS = objSymbol as ICharacterMarkerSymbol;
								StructCharacterMarkerSymbol strCMS = new StructCharacterMarkerSymbol();
								strCMS = StoreCharacterMarker(CMS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strCMS.Label = Renderer.Label[Renderer.get_Value(j)];
								strCMS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strCMS);
								break;
								
							case "IPictureMarkerSymbol":
								IPictureMarkerSymbol PMS = default(IPictureMarkerSymbol);
                                PMS = objSymbol as IPictureMarkerSymbol;
								StructPictureMarkerSymbol strPMS = new StructPictureMarkerSymbol();
								strPMS = StorePictureMarker(PMS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strPMS.Label = Renderer.Label[Renderer.get_Value(j)];
								strPMS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strPMS);
								break;
								
							case "IArrowMarkerSymbol":
								IArrowMarkerSymbol AMS = default(IArrowMarkerSymbol);
                                AMS = objSymbol as IArrowMarkerSymbol;
								StructArrowMarkerSymbol strAMS = new StructArrowMarkerSymbol();
								strAMS = StoreArrowMarker(AMS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strAMS.Label = Renderer.Label[Renderer.get_Value(j)];
								strAMS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strAMS);
								break;
								
							case "IMultiLayerMarkerSymbol":
								IMultiLayerMarkerSymbol MLMS = default(IMultiLayerMarkerSymbol);
                                MLMS = objSymbol as IMultiLayerMarkerSymbol;
								StructMultilayerMarkerSymbol strMLMS = new StructMultilayerMarkerSymbol();
								strMLMS = StoreMultiLayerMarker(MLMS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strMLMS.Label = Renderer.Label[Renderer.get_Value(j)];
								strMLMS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strMLMS);
								break;
								
							case "false":
								InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreStructLayer");
								break;
						}
					}
					//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
					if (objFstOrderSymbol is ILineSymbol)
					{
						strRenderer.FeatureCls = FeatureClass.LineFeature;
						ILineSymbol objSymbol = default(ILineSymbol);
                        objSymbol = objFstOrderSymbol as ILineSymbol;
						switch (LineSymbolScan(objSymbol))
						{
							case "ICartographicLineSymbol":
								ICartographicLineSymbol CLS = default(ICartographicLineSymbol);
                                CLS = objSymbol as ICartographicLineSymbol;
								StructCartographicLineSymbol strCLS = new StructCartographicLineSymbol();
								strCLS = StoreCartographicLine(CLS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strCLS.Label = Renderer.Label[Renderer.get_Value(j)];
								strCLS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strCLS);
								break;
								
							case "IHashLineSymbol":
								IHashLineSymbol HLS = default(IHashLineSymbol);
                                HLS = objSymbol as IHashLineSymbol;
								StructHashLineSymbol strHLS = new StructHashLineSymbol();
								strHLS = StoreHashLine(HLS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strHLS.Label = Renderer.Label[Renderer.get_Value(j)];
								strHLS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strHLS);
								break;
								
							case "IMarkerLineSymbol":
								IMarkerLineSymbol MLS = default(IMarkerLineSymbol);
                                MLS = objSymbol as IMarkerLineSymbol;
								StructMarkerLineSymbol strMLS = new StructMarkerLineSymbol();
								strMLS = StoreMarkerLine(MLS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strMLS.Label = Renderer.Label[Renderer.get_Value(j)];
								strMLS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strMLS);
								break;
								
							case "ISimpleLineSymbol":
								ISimpleLineSymbol SLS = default(ISimpleLineSymbol);
                                SLS = objSymbol as ISimpleLineSymbol;
								StructSimpleLineSymbol strSLS = new StructSimpleLineSymbol();
								strSLS = StoreSimpleLine(SLS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strSLS.Label = Renderer.Label[Renderer.get_Value(j)];
								strSLS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strSLS);
								break;
								
							case "IPictureLineSymbol":
								IPictureLineSymbol PLS = default(IPictureLineSymbol);
                                PLS = objSymbol as IPictureLineSymbol;
								StructPictureLineSymbol strPLS = new StructPictureLineSymbol();
								strPLS = StorePictureLine(PLS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strPLS.Label = Renderer.Label[Renderer.get_Value(j)];
								strPLS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strPLS);
								break;
								
							case "IMultiLayerLineSymbol":
								IMultiLayerLineSymbol MLLS = default(IMultiLayerLineSymbol);
                                MLLS = objSymbol as IMultiLayerLineSymbol;
								StructMultilayerLineSymbol strMLLS = new StructMultilayerLineSymbol();
								strMLLS = StoreMultilayerLines(MLLS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strMLLS.Label = Renderer.Label[Renderer.get_Value(j)];
								strMLLS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strMLLS);
								break;
								
							case "false":
								InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreStructLayer");
								break;
						}
						
					}
					//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
					if (objFstOrderSymbol is IFillSymbol)
					{
						strRenderer.FeatureCls = FeatureClass.PolygonFeature;
						IFillSymbol objSymbol = default(IFillSymbol);
                        objSymbol = objFstOrderSymbol as IFillSymbol;
						switch (FillSymbolScan(objSymbol))
						{
							case "ISimpleFillSymbol":
								ISimpleFillSymbol SFS = default(ISimpleFillSymbol);
                                SFS = objSymbol as ISimpleFillSymbol;
								StructSimpleFillSymbol strSFS = new StructSimpleFillSymbol();
								strSFS = StoreSimpleFill(SFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strSFS.Label = Renderer.Label[Renderer.get_Value(j)];
								strSFS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strSFS);
								break;
								
							case "IMarkerFillSymbol":
								IMarkerFillSymbol MFS = default(IMarkerFillSymbol);
                                MFS = objSymbol as IMarkerFillSymbol;
								StructMarkerFillSymbol strMFS = new StructMarkerFillSymbol();
								strMFS = StoreMarkerFill(MFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strMFS.Label = Renderer.Label[Renderer.get_Value(j)];
								strMFS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strMFS);
								break;
								
							case "ILineFillSymbol":
								ILineFillSymbol LFS = default(ILineFillSymbol);
                                LFS = objSymbol as ILineFillSymbol;
								StructLineFillSymbol strLFS = new StructLineFillSymbol();
								strLFS = StoreLineFill(LFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strLFS.Label = Renderer.Label[Renderer.get_Value(j)];
								strLFS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strLFS);
								break;
								
							case "IDotDensityFillSymbol":
								IDotDensityFillSymbol DFS = default(IDotDensityFillSymbol);
                                DFS = objSymbol as IDotDensityFillSymbol;
								StructDotDensityFillSymbol strDFS = new StructDotDensityFillSymbol();
								strDFS = StoreDotDensityFill(DFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strDFS.Label = Renderer.Label[Renderer.get_Value(j)];
								strDFS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strDFS);
								break;
								
							case "IPictureFillSymbol":
								IPictureFillSymbol PFS = default(IPictureFillSymbol);
                                PFS = objSymbol as IPictureFillSymbol;
								StructPictureFillSymbol strPFS = new StructPictureFillSymbol();
								strPFS = StorePictureFill(PFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strPFS.Label = Renderer.Label[Renderer.get_Value(j)];
								strPFS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strPFS);
								break;
								
							case "IGradientFillSymbol":
								IGradientFillSymbol GFS = default(IGradientFillSymbol);
                                GFS = objSymbol as IGradientFillSymbol;
								StructGradientFillSymbol strGFS = new StructGradientFillSymbol();
								strGFS = StoreGradientFill(GFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strGFS.Label = Renderer.Label[Renderer.get_Value(j)];
								strGFS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strGFS);
								break;
								
							case "IMultiLayerFillSymbol":
								IMultiLayerFillSymbol MLFS = default(IMultiLayerFillSymbol);
                                MLFS = objSymbol as IMultiLayerFillSymbol;
								StructMultilayerFillSymbol strMLFS = new StructMultilayerFillSymbol();
								strMLFS = StoreMultiLayerFill(MLFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strMLFS.Label = Renderer.Label[Renderer.get_Value(j)];
								strMLFS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strMLFS);
								break;
								
							case "false":
								InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreStructLayer");
								break;
						}
					}
					//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
					if (objFstOrderSymbol is I3DChartSymbol)
					{
						I3DChartSymbol objSymbol = default(I3DChartSymbol);
                        objSymbol = objFstOrderSymbol as I3DChartSymbol;
						switch (IIIDChartSymbolScan(objSymbol))
						{
							case "IBarChartSymbol":
								IBarChartSymbol BCS = default(IBarChartSymbol);
                                BCS = objSymbol as IBarChartSymbol;
								StructBarChartSymbol strBCS = new StructBarChartSymbol();
								strBCS = StoreBarChart(BCS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strBCS.Label = Renderer.Label[Renderer.get_Value(j)];
								strBCS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strBCS);
								break;
								
							case "IPieChartSymbol":
								IPieChartSymbol PCS = default(IPieChartSymbol);
                                PCS = objSymbol as IPieChartSymbol;
								StructPieChartSymbol strPCS = new StructPieChartSymbol();
								strPCS = StorePieChart(PCS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strPCS.Label = Renderer.Label[Renderer.get_Value(j)];
								strPCS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strPCS);
								break;
								
							case "IStackedChartSymbol":
								IStackedChartSymbol SCS = default(IStackedChartSymbol);
                                SCS = objSymbol as IStackedChartSymbol;
								StructStackedChartSymbol strSCS = new StructStackedChartSymbol();
								strSCS = StoreStackedChart(SCS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strSCS.Label = Renderer.Label[Renderer.get_Value(j)];
								strSCS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strSCS);
								break;
								
							case "false":
								InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreStructLayer");
								break;
						}
					}
					//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
				}
				return strRenderer;
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Speichern der Symbole in den Layerstrukturen", ex.Message, ex.StackTrace, "StoreStructUVRenderer");
                return strRenderer;
			}
		}
		private ArrayList getUVFieldValues(IUniqueValueRenderer Renderer, int Index)
		{
			//Es macht nur Sinn hier Fieldvalues einzuf黦en, wenn 黚erhaupt nach einem Feld klassifiziert wurde
			ArrayList Fieldvalues = default(ArrayList);
			int iFieldCount = 0; //Anzahl der Spalten, nach denen klassifiziert wurde
			int Index2 = 0;
			
			iFieldCount = Renderer.FieldCount;
			Fieldvalues = (ArrayList) null;
			if (iFieldCount > 0)
			{
				bool bNoSepFieldVal; //Wenn nur nach einem Feld klassifiziert wurde muss der Flag auf true gesetzt werden
				string Label;
				string Label2 = "";
				ISymbol objSymbol = default(ISymbol);
				int iNumberOfSymbols = 0; //Anzahl aller Symbole des Rendererobjekts
				iNumberOfSymbols = Renderer.ValueCount; //Anzahl der Symbole
				bNoSepFieldVal = false;
				if (iFieldCount > 1) //!!!! Damit nicht in die aufwendige Funktion GimmeSeperateFieldValues(..) gegangen werden mu? wenn nicht n鰐ig
				{
					bNoSepFieldVal = true; //und damit nicht die aufw鋘digen UniqueValue-Listen erzeugt werden m黶sen, wenn nicht notwendig
				}
				
				Label = Renderer.Label[Renderer.Value[Index]];
				if (bNoSepFieldVal == false)
				{
					Fieldvalues = new ArrayList();
					Fieldvalues.Add(Renderer.Value[Index]); //Wenn nur nach 1 Feld klassifiziert wurde, muss nicht in GimmeSeperateFieldValues gegangen werden (Zeit!!!)
					//Find grouped values with the same label (values where no symbol defined).
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
		private StructMultilayerMarkerSymbol StoreMultiLayerMarker(IMultiLayerMarkerSymbol symbol)
		{
			StructMultilayerMarkerSymbol StructStorage = new StructMultilayerMarkerSymbol();
			StructStorage.MultiMarkerLayers = new ArrayList();
			int i = 0;
			StructStorage.LayerCount = symbol.LayerCount;
			for (i = 0; i <= symbol.LayerCount - 1; i++) //Damit alle Layer erfasst werden
			{
				switch (MarkerSymbolScan(symbol.Layer[i])) //Das Multilayersymbol besteht aus mehreren Layern, die alle durchlaufen werden m黶sen
				{
					case "ISimpleMarkerSymbol":
						ISimpleMarkerSymbol SMS = default(ISimpleMarkerSymbol);
                        SMS = symbol.get_Layer(i) as ISimpleMarkerSymbol;
						StructStorage.MultiMarkerLayers.Add(StoreSimpleMarker(SMS)); //Der Layer wird der Arraylist von Multilayern hinzugef黦t
						break;
					case "ICharacterMarkerSymbol":
						ICharacterMarkerSymbol CMS = default(ICharacterMarkerSymbol);
                        CMS = symbol.get_Layer(i) as ICharacterMarkerSymbol;
						StructStorage.MultiMarkerLayers.Add(StoreCharacterMarker(CMS)); //Der Layer wird der Arraylist von Multilayern hinzugef黦t
						break;
					case "IPictureMarkerSymbol":
						IPictureMarkerSymbol PMS = default(IPictureMarkerSymbol);
                        PMS = symbol.get_Layer(i) as IPictureMarkerSymbol;
						StructStorage.MultiMarkerLayers.Add(StorePictureMarker(PMS)); //Der Layer wird der Arraylist von Multilayern hinzugef黦t
						break;
					case "IArrowMarkerSymbol":
						IArrowMarkerSymbol AMS = default(IArrowMarkerSymbol);
                        AMS = symbol.get_Layer(i) as IArrowMarkerSymbol;
						StructStorage.MultiMarkerLayers.Add(StoreArrowMarker(AMS)); //Der Layer wird der Arraylist von Multilayern hinzugef黦t
						break;
					case "false":
						InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreMultiLayerMarker");
						break;
				}
			}
			return StructStorage;
		}
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
				ILineProperties lineProperties = default(ILineProperties);
				ITemplate template = default(ITemplate);
				double markLen = 0;
				double gapLen = 0;
				double interval = 0;
				int templateIdx = 0;
                lineProperties = symbol as ILineProperties;
				if (lineProperties.Template is ITemplate)
				{
					template = lineProperties.Template;
					interval = template.Interval;
					for (templateIdx = 0; templateIdx <= template.PatternElementCount - 1; templateIdx++)
					{
                        template.GetPatternElement(templateIdx, out markLen, out gapLen);
						StructStorage.DashArray.Add(markLen * interval);
						StructStorage.DashArray.Add(gapLen * interval);
					}
				}
			}
			return StructStorage;
		}
		private StructHashLineSymbol StoreHashLine(IHashLineSymbol symbol)
		{
			StructHashLineSymbol StructStorage = new StructHashLineSymbol();
			StructStorage.Angle = symbol.Angle;
			StructStorage.Color = GimmeStringForColor(symbol.Color);
			StructStorage.Transparency = symbol.Color.Transparency;
			switch (LineSymbolScan(symbol.HashSymbol)) //symbol.HashSymbol ist ein Liniensymbol. deshalb bei case... kein IHashSymbol/StructHashLineSymbol
			{
				case "ICartographicLineSymbol":
					ICartographicLineSymbol CLS = default(ICartographicLineSymbol);
                    CLS = symbol.HashSymbol as ICartographicLineSymbol;
					StructStorage.HashSymbol_CartographicLine = StoreCartographicLine(CLS);
					StructStorage.kindOfLineStruct = LineStructs.StructCartographicLineSymbol;
					break;
				case "IMarkerLineSymbol":
					IMarkerLineSymbol MLS = default(IMarkerLineSymbol);
                    MLS = symbol.HashSymbol as IMarkerLineSymbol;
					StructStorage.HashSymbol_MarkerLine = StoreMarkerLine(MLS);
					StructStorage.kindOfLineStruct = LineStructs.StructMarkerLineSymbol;
					break;
				case "ISimpleLineSymbol":
					ISimpleLineSymbol SLS = default(ISimpleLineSymbol);
                    SLS = symbol.HashSymbol as ISimpleLineSymbol;
					StructStorage.HashSymbol_SimpleLine = StoreSimpleLine(SLS);
					StructStorage.kindOfLineStruct = LineStructs.StructSimpleLineSymbol;
					break;
				case "IPictureLineSymbol":
					IPictureLineSymbol PLS = default(IPictureLineSymbol);
                    PLS = symbol.HashSymbol as IPictureLineSymbol;
					StructStorage.HashSymbol_PictureLine = StorePictureLine(PLS);
					StructStorage.kindOfLineStruct = LineStructs.StructPictureLineSymbol;
					break;
				case "IMultiLayerLineSymbol":
					IMultiLayerLineSymbol MLLS = default(IMultiLayerLineSymbol);
                    MLLS = symbol.HashSymbol as IMultiLayerLineSymbol;
					StructStorage.HashSymbol_MultiLayerLines = StoreMultilayerLines(MLLS);
					StructStorage.kindOfLineStruct = LineStructs.StructMultilayerLineSymbol;
					break;
				case "false":
					InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreHashLine");
					break;
			}
			return StructStorage;
		}
		private StructMarkerLineSymbol StoreMarkerLine(IMarkerLineSymbol symbol)
		{
			StructMarkerLineSymbol StructStorage = new StructMarkerLineSymbol();
			StructStorage.Color = GimmeStringForColor(symbol.Color);
			StructStorage.Transparency = symbol.Color.Transparency;
			StructStorage.Width = symbol.Width;
			switch (MarkerSymbolScan(symbol.MarkerSymbol))
			{
				case "ISimpleMarkerSymbol":
					ISimpleMarkerSymbol SMS = default(ISimpleMarkerSymbol);
                    SMS = symbol.MarkerSymbol as ISimpleMarkerSymbol;
					StructStorage.MarkerSymbol_SimpleMarker = StoreSimpleMarker(SMS);
					StructStorage.kindOfMarkerStruct = MarkerStructs.StructSimpleMarkerSymbol;
					break;
				case "ICharacterMarkerSymbol":
					ICharacterMarkerSymbol CMS = default(ICharacterMarkerSymbol);
                    CMS = symbol.MarkerSymbol as ICharacterMarkerSymbol;
					StructStorage.MarkerSymbol_CharacterMarker = StoreCharacterMarker(CMS);
					StructStorage.kindOfMarkerStruct = MarkerStructs.StructCharacterMarkerSymbol;
					break;
				case "IPictureMarkerSymbol":
					IPictureMarkerSymbol PMS = default(IPictureMarkerSymbol);
                    PMS = symbol.MarkerSymbol as IPictureMarkerSymbol;
					StructStorage.MarkerSymbol_PictureMarker = StorePictureMarker(PMS);
					StructStorage.kindOfMarkerStruct = MarkerStructs.StructPictureMarkerSymbol;
					break;
				case "IArrowMarkerSymbol":
					IArrowMarkerSymbol AMS = default(IArrowMarkerSymbol);
                    AMS = symbol.MarkerSymbol as IArrowMarkerSymbol;
					StructStorage.MarkerSymbol_ArrowMarker = StoreArrowMarker(AMS);
					StructStorage.kindOfMarkerStruct = MarkerStructs.StructArrowMarkerSymbol;
					break;
				case "IMultiLayerMarkerSymbol":
					IMultiLayerMarkerSymbol MLMS = default(IMultiLayerMarkerSymbol);
                    MLMS = symbol.MarkerSymbol as IMultiLayerMarkerSymbol;
					StructStorage.MarkerSymbol_MultilayerMarker = StoreMultiLayerMarker(MLMS);
					StructStorage.kindOfMarkerStruct = MarkerStructs.StructMultilayerMarkerSymbol;
					break;
				case "false":
					InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreMarkerLine");
					break;
			}
			return StructStorage;
		}
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
		private StructMultilayerLineSymbol StoreMultilayerLines(IMultiLayerLineSymbol symbol)
		{
			StructMultilayerLineSymbol StructStorage = new StructMultilayerLineSymbol();
			StructStorage.MultiLineLayers = new ArrayList();
			int i = 0;
			StructStorage.LayerCount = symbol.LayerCount;
			for (i = 0; i <= symbol.LayerCount - 1; i++)
			{
				switch (LineSymbolScan(symbol.Layer[i]))
				{
					case "ICartographicLineSymbol":
						ICartographicLineSymbol CLS = default(ICartographicLineSymbol);
                        CLS = symbol.get_Layer(i) as ICartographicLineSymbol;
						StructStorage.MultiLineLayers.Add(StoreCartographicLine(CLS));
						break;
					case "IHashLineSymbol":
						IHashLineSymbol HLS = default(IHashLineSymbol);
                        HLS = symbol.get_Layer(i) as IHashLineSymbol;
						StructStorage.MultiLineLayers.Add(StoreHashLine(HLS));
						break;
					case "IMarkerLineSymbol":
						IMarkerLineSymbol MLS = default(IMarkerLineSymbol);
                        MLS = symbol.get_Layer(i) as IMarkerLineSymbol;
						StructStorage.MultiLineLayers.Add(StoreMarkerLine(MLS));
						break;
					case "ISimpleLineSymbol":
						ISimpleLineSymbol SLS = default(ISimpleLineSymbol);
                        SLS = symbol.get_Layer(i) as ISimpleLineSymbol;
						StructStorage.MultiLineLayers.Add(StoreSimpleLine(SLS));
						break;
					case "IPictureLineSymbol":
						IPictureLineSymbol PLS = default(IPictureLineSymbol);
                        PLS = symbol.get_Layer(i) as IPictureLineSymbol;
						StructStorage.MultiLineLayers.Add(StorePictureLine(PLS));
						break;
					case "IMultiLayerLineSymbol":
						IMultiLayerLineSymbol MLLS = default(IMultiLayerLineSymbol);
                        MLLS = symbol.get_Layer(i) as IMultiLayerLineSymbol;
						StructStorage.MultiLineLayers.Add(StoreMultilayerLines(MLLS)); //Hier ist ein rekursiver Aufruf
						break;
					case "false":
						InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreMultilayerLines");
						break;
				}
			}
			return StructStorage;
		}
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
			switch (LineSymbolScan(symbol.Outline)) //symbol.Outline ist ein Liniensymbol
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
					InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreSimpleFill");
					break;
			}
			return StructStorage;
		}
		private StructMarkerFillSymbol StoreMarkerFill(IMarkerFillSymbol symbol)
		{
			StructMarkerFillSymbol StructStorage = new StructMarkerFillSymbol();
			StructStorage.Color = GimmeStringForColor(symbol.Color);
			StructStorage.Transparency = symbol.Color.Transparency;
			StructStorage.GridAngle = symbol.GridAngle;
			switch (MarkerSymbolScan(symbol.MarkerSymbol))
			{
				case "ISimpleMarkerSymbol":
					ISimpleMarkerSymbol SMS = default(ISimpleMarkerSymbol);
                    SMS = symbol.MarkerSymbol as ISimpleMarkerSymbol;
					StructStorage.MarkerSymbol_SimpleMarker = StoreSimpleMarker(SMS);
					StructStorage.kindOfMarkerStruct = MarkerStructs.StructSimpleMarkerSymbol;
					break;
				case "ICharacterMarkerSymbol":
					ICharacterMarkerSymbol CMS = default(ICharacterMarkerSymbol);
                    CMS = symbol.MarkerSymbol as ICharacterMarkerSymbol;
					StructStorage.MarkerSymbol_CharacterMarker = StoreCharacterMarker(CMS);
					StructStorage.kindOfMarkerStruct = MarkerStructs.StructCharacterMarkerSymbol;
					break;
				case "IPictureMarkerSymbol":
					IPictureMarkerSymbol PMS = default(IPictureMarkerSymbol);
                    PMS = symbol.MarkerSymbol as IPictureMarkerSymbol;
					StructStorage.MarkerSymbol_PictureMarker = StorePictureMarker(PMS);
					StructStorage.kindOfMarkerStruct = MarkerStructs.StructPictureMarkerSymbol;
					break;
				case "IArrowMarkerSymbol":
					IArrowMarkerSymbol AMS = default(IArrowMarkerSymbol);
                    AMS = symbol.MarkerSymbol as IArrowMarkerSymbol;
					StructStorage.MarkerSymbol_ArrowMarker = StoreArrowMarker(AMS);
					StructStorage.kindOfMarkerStruct = MarkerStructs.StructArrowMarkerSymbol;
					break;
				case "IMultiLayerMarkerSymbol":
					IMultiLayerMarkerSymbol MLMS = default(IMultiLayerMarkerSymbol);
                    MLMS = symbol.MarkerSymbol as IMultiLayerMarkerSymbol; 
					StructStorage.MarkerSymbol_MultilayerMarker = StoreMultiLayerMarker(MLMS);
					StructStorage.kindOfMarkerStruct = MarkerStructs.StructMultilayerMarkerSymbol;
					break;
				case "false":
					InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreMarkerFill");
					break;
			}
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
					InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreMarkerFill");
					break;
			}
			return StructStorage;
		}
		private StructLineFillSymbol StoreLineFill(ILineFillSymbol symbol)
		{
			StructLineFillSymbol StructStorage = new StructLineFillSymbol();
			StructStorage.Angle = symbol.Angle;
			StructStorage.Color = GimmeStringForColor(symbol.Color);
			StructStorage.Transparency = symbol.Color.Transparency;
			StructStorage.Offset = symbol.Offset;
			StructStorage.Separation = symbol.Separation;
			switch (LineSymbolScan(symbol.LineSymbol))
			{
				case "ICartographicLineSymbol":
					ICartographicLineSymbol CLS = default(ICartographicLineSymbol);
                    CLS = symbol.LineSymbol as ICartographicLineSymbol;
					StructStorage.LineSymbol_CartographicLine = StoreCartographicLine(CLS);
					StructStorage.kindOfLineStruct = LineStructs.StructCartographicLineSymbol;
					break;
				case "IMarkerLineSymbol":
					IMarkerLineSymbol MLS = default(IMarkerLineSymbol);
                    MLS = symbol.LineSymbol as IMarkerLineSymbol;
					StructStorage.LineSymbol_MarkerLine = StoreMarkerLine(MLS);
					StructStorage.kindOfLineStruct = LineStructs.StructMarkerLineSymbol;
					break;
				case "IHashLineSymbol":
					IHashLineSymbol HLS = default(IHashLineSymbol);
                    HLS = symbol.LineSymbol as IHashLineSymbol;
					StructStorage.LineSymbol_HashLine = StoreHashLine(HLS);
					StructStorage.kindOfLineStruct = LineStructs.StructHashLineSymbol;
					break;
				case "ISimpleLineSymbol":
					ISimpleLineSymbol SLS = default(ISimpleLineSymbol);
                    SLS = symbol.LineSymbol as ISimpleLineSymbol;
					StructStorage.LineSymbol_SimpleLine = StoreSimpleLine(SLS);
					StructStorage.kindOfLineStruct = LineStructs.StructSimpleLineSymbol;
					break;
				case "IPictureLineSymbol":
					IPictureLineSymbol PLS = default(IPictureLineSymbol);
                    PLS = symbol.LineSymbol as IPictureLineSymbol;
					StructStorage.LineSymbol_PictureLine = StorePictureLine(PLS);
					StructStorage.kindOfLineStruct = LineStructs.StructPictureLineSymbol;
					break;
				case "IMultiLayerLineSymbol":
					IMultiLayerLineSymbol MLLS = default(IMultiLayerLineSymbol);
                    MLLS = symbol.LineSymbol as IMultiLayerLineSymbol;
					StructStorage.LineSymbol_MultiLayerLines = StoreMultilayerLines(MLLS);
					StructStorage.kindOfLineStruct = LineStructs.StructMultilayerLineSymbol;
					break;
				case "false":
					InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreLineFill");
					break;
			}
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
					InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreLineFill");
					break;
			}
			return StructStorage;
		}
		private StructDotDensityFillSymbol StoreDotDensityFill(IDotDensityFillSymbol symbol)
		{
			StructDotDensityFillSymbol StructStorage = new StructDotDensityFillSymbol();
			ISymbolArray objSymbolArray = default(ISymbolArray);
			StructStorage.SymbolList = new ArrayList();
            objSymbolArray = symbol as ISymbolArray;
			int i = 0;
			StructStorage.BackgroundColor = GimmeStringForColor(symbol.BackgroundColor);
			StructStorage.BackgroundTransparency = symbol.BackgroundColor.Transparency;
			StructStorage.Color = GimmeStringForColor(symbol.Color);
			StructStorage.Transparency = symbol.Color.Transparency;
			StructStorage.FixedPlacement = symbol.FixedPlacement;
			StructStorage.DotSpacing = symbol.DotSpacing;
			StructStorage.SymbolCount = objSymbolArray.SymbolCount;
			
			for (i = 0; i <= objSymbolArray.SymbolCount - 1; i++)
			{
				if (objSymbolArray.Symbol[i] is IMarkerSymbol) //Nur der Marker macht hier als Symbol 黚erhaupt Sinn
				{
					IMarkerSymbol MS = default(IMarkerSymbol);
                    MS = objSymbolArray.Symbol[i] as IMarkerSymbol;
					//!!!ACHTUNG!!! In der ArrayList wird immer abwechselnd ein Symbol und danach die zugeh鰎ige Symbolanzahl
					//abgespeichert. Das ist nicht elegant aber sp鋞er geschickter beim auslesen!
					StructStorage.SymbolList = new ArrayList();
					switch (MarkerSymbolScan(MS))
					{
						case "ISimpleMarkerSymbol":
							ISimpleMarkerSymbol SMS = default(ISimpleMarkerSymbol);
                            SMS = MS as ISimpleMarkerSymbol;
							StructStorage.SymbolList.Add(StoreSimpleMarker(SMS));
							StructStorage.SymbolList.Add(symbol.DotCount[i]);
							break;
						case "ICharacterMarkerSymbol":
							ICharacterMarkerSymbol CMS = default(ICharacterMarkerSymbol);
                            CMS = MS as ICharacterMarkerSymbol;
							StructStorage.SymbolList.Add(StoreCharacterMarker(CMS));
							StructStorage.SymbolList.Add(symbol.DotCount[i]);
							break;
						case "IPictureMarkerSymbol":
							IPictureMarkerSymbol PMS = default(IPictureMarkerSymbol);
                            PMS = MS as IPictureMarkerSymbol;
							StructStorage.SymbolList.Add(StorePictureMarker(PMS));
							StructStorage.SymbolList.Add(symbol.DotCount[i]);
							break;
						case "IArrowMarkerSymbol":
							IArrowMarkerSymbol AMS = default(IArrowMarkerSymbol);
                            AMS = MS as IArrowMarkerSymbol;
							StructStorage.SymbolList.Add(StoreArrowMarker(AMS));
							StructStorage.SymbolList.Add(symbol.DotCount[i]);
							break;
						case "IMultiLayerMarkerSymbol":
							IMultiLayerMarkerSymbol MLMS = default(IMultiLayerMarkerSymbol);
                            MLMS = MS as IMultiLayerMarkerSymbol;
							StructStorage.SymbolList.Add(StoreMultiLayerMarker(MLMS));
							StructStorage.SymbolList.Add(symbol.DotCount[i]);
							break;
						case "false":
							InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreDotDensityFill");
							break;
					}
				}
			}
			
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
					InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreLineFill");
					break;
			}
			
			return StructStorage;
		}
		private StructPictureFillSymbol StorePictureFill(IPictureFillSymbol symbol)
		{
			StructPictureFillSymbol StructStorage = new StructPictureFillSymbol();
			StructStorage.Angle = symbol.Angle;
			StructStorage.BackgroundColor = this.GimmeStringForColor(symbol.BackgroundColor);
			StructStorage.BackgroundTransparency = symbol.BackgroundColor.Transparency;
			StructStorage.Color = this.GimmeStringForColor(symbol.Color);
			StructStorage.Transparency = symbol.Color.Transparency;
			//objPicture = Microsoft.VisualBasic.Compatibility.VB6.IPictureDispToImage(symbol.Picture) 'doesn't work with esri
			//StructStorage.Picture = symbol.Picture.
			StructStorage.XScale = symbol.XScale;
			StructStorage.YScale = symbol.YScale;
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
					InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StorePictureFill");
					break;
			}
			return StructStorage;
		}
		private StructGradientFillSymbol StoreGradientFill(IGradientFillSymbol symbol)
		{
			StructGradientFillSymbol StructStorage = new StructGradientFillSymbol();
			StructStorage.Color = GimmeStringForColor(symbol.Color);
			StructStorage.Transparency = symbol.Color.Transparency;
			StructStorage.Colors = GimmeArrayListForColorRamp(symbol.ColorRamp);
			StructStorage.GradientAngle = symbol.GradientAngle;
			StructStorage.GradientPercentage = symbol.GradientPercentage;
			StructStorage.IntervallCount = symbol.IntervalCount;
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
					InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StorePictureFill");
					break;
			}
			return StructStorage;
		}
		private StructMultilayerFillSymbol StoreMultiLayerFill(IMultiLayerFillSymbol symbol)
		{
			StructMultilayerFillSymbol StructStorage = new StructMultilayerFillSymbol();
			StructStorage.LayerCount = symbol.LayerCount;
			StructStorage.MultiFillLayers = new ArrayList();
			int i = 0;
			for (i = 0; i <= symbol.LayerCount - 1; i++)
			{
				switch (FillSymbolScan(symbol.Layer[i]))
				{
					case "ISimpleFillSymbol":
						ISimpleFillSymbol SFS = default(ISimpleFillSymbol);
                        SFS = symbol.get_Layer(i) as ISimpleFillSymbol;
						StructStorage.MultiFillLayers.Add(StoreSimpleFill(SFS));
						break;
					case "IMarkerFillSymbol":
						IMarkerFillSymbol MFS = default(IMarkerFillSymbol);
                        MFS = symbol.get_Layer(i) as IMarkerFillSymbol;
						StructStorage.MultiFillLayers.Add(StoreMarkerFill(MFS));
						break;
					case "ILineFillSymbol":
						ILineFillSymbol LFS = default(ILineFillSymbol);
                        LFS = symbol.get_Layer(i) as ILineFillSymbol;
						StructStorage.MultiFillLayers.Add(StoreLineFill(LFS));
						break;
					case "IPictureFillSymbol":
						IPictureFillSymbol PFS = default(IPictureFillSymbol);
                        PFS = symbol.get_Layer(i) as IPictureFillSymbol;
						StructStorage.MultiFillLayers.Add(StorePictureFill(PFS));
						break;
					case "IDotDensityFillSymbol":
						IDotDensityFillSymbol DFS = default(IDotDensityFillSymbol);
						DFS = symbol.get_Layer(i) as IDotDensityFillSymbol;
						StructStorage.MultiFillLayers.Add(StoreDotDensityFill(DFS));
						break;
					case "IGradientFillSymbol":
						IGradientFillSymbol GFS = default(IGradientFillSymbol);
                        GFS = symbol.get_Layer(i) as IGradientFillSymbol;
						StructStorage.MultiFillLayers.Add(StoreGradientFill(GFS));
						break;
					case "IMultiLayerFillSymbol":
						IMultiLayerFillSymbol MLFS = default(IMultiLayerFillSymbol);
						MLFS = symbol;
						StructStorage.MultiFillLayers.Add(StoreMultiLayerFill(MLFS)); //Hier ist ein rekursiver Aufruf
						break;
					case "false":
						InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreMultilayerFill");
						break;
				}
			}
			return StructStorage;
		}
		private StructBarChartSymbol StoreBarChart(IBarChartSymbol symbol)
		{
			StructBarChartSymbol StructStorage = new StructBarChartSymbol();
			StructStorage.ShowAxes = symbol.ShowAxes;
			StructStorage.Spacing = symbol.Spacing;
			StructStorage.VerticalBars = symbol.VerticalBars;
			StructStorage.Width = symbol.Width;
			switch (LineSymbolScan(symbol.Axes))
			{
				case "ICartographicLineSymbol":
					ICartographicLineSymbol CLS = default(ICartographicLineSymbol);
                    CLS = symbol.Axes as ICartographicLineSymbol;
					StructStorage.Axes_CartographicLine = StoreCartographicLine(CLS);
					StructStorage.kindOfAxeslineStruct = LineStructs.StructCartographicLineSymbol;
					break;
				case "IMarkerLineSymbol":
					IMarkerLineSymbol MLS = default(IMarkerLineSymbol);
                    MLS = symbol.Axes as IMarkerLineSymbol;
					StructStorage.Axes_MarkerLine = StoreMarkerLine(MLS);
					StructStorage.kindOfAxeslineStruct = LineStructs.StructMarkerLineSymbol;
					break;
				case "IHashLineSymbol":
					IHashLineSymbol HLS = default(IHashLineSymbol);
                    HLS = symbol.Axes as IHashLineSymbol;
					StructStorage.Axes_HashLine = StoreHashLine(HLS);
					StructStorage.kindOfAxeslineStruct = LineStructs.StructHashLineSymbol;
					break;
				case "ISimpleLineSymbol":
					ISimpleLineSymbol SLS = default(ISimpleLineSymbol);
                    SLS = symbol.Axes as ISimpleLineSymbol;
					StructStorage.Axes_SimpleLine = StoreSimpleLine(SLS);
					StructStorage.kindOfAxeslineStruct = LineStructs.StructSimpleLineSymbol;
					break;
				case "IPictureLineSymbol":
					IPictureLineSymbol PLS = default(IPictureLineSymbol);
                    PLS = symbol.Axes as IPictureLineSymbol;
					StructStorage.Axes_PictureLine = StorePictureLine(PLS);
					StructStorage.kindOfAxeslineStruct = LineStructs.StructPictureLineSymbol;
					break;
				case "IMultiLayerLineSymbol":
					IMultiLayerLineSymbol MLLS = default(IMultiLayerLineSymbol);
                    MLLS = symbol.Axes as IMultiLayerLineSymbol;
					StructStorage.Axes_MultiLayerLines = StoreMultilayerLines(MLLS);
					StructStorage.kindOfAxeslineStruct = LineStructs.StructMultilayerLineSymbol;
					break;
				case "false":
					InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreBarChart");
					break;
			}
			return StructStorage;
		}
		private StructPieChartSymbol StorePieChart(IPieChartSymbol symbol)
		{
			StructPieChartSymbol StructStorage = new StructPieChartSymbol();
			StructStorage.Clockwise = symbol.Clockwise;
			StructStorage.UseOutline = symbol.UseOutline;
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
					InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StorePictureFill");
					break;
			}
			return StructStorage;
		}
		private StructStackedChartSymbol StoreStackedChart(IStackedChartSymbol symbol)
		{
			StructStackedChartSymbol StructStorage = new StructStackedChartSymbol();
			StructStorage.Fixed = symbol.Fixed;
			StructStorage.UseOutline = symbol.UseOutline;
			StructStorage.VerticalBar = symbol.VerticalBar;
			StructStorage.Width = symbol.Width;
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
					InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StorePictureFill");
					break;
			}
			return StructStorage;
		}
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
		private bool CentralProcessingFunc()
		{
            frmMotherform.CHLabelTop("正在分析ArcMap符号...");
            bool blnAnswer = false;
			Output_SLD objOutputSLD;
			if (GetProcesses() == false)
			{
				MyTermination();
				return false;
			}
			if (GetApplication() == false)
			{
				MyTermination();
				return false;
			}
			if (GetMap() == false)
			{
				MyTermination();
				return false;
			}
			if (AnalyseLayerSymbology() == false)
			{
				MyTermination();
				return false;
			}
			if (m_cFilename == null || m_cFilename == "")
			{
                frmMotherform.CHLabelTop(string.Format("ArcMap符号分析完成"));
                blnAnswer = MessageBox.Show("请先选择SLD文件保存路径","提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes;
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
						objOutputSLD = new Output_SLD(frmMotherform, this, m_cFilename); 
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
				objOutputSLD = new Output_SLD(frmMotherform, this, m_cFilename);
			}
			frmMotherform.CHLabelBottom("");
			frmMotherform.CHLabelSmall("");
			frmMotherform.ReadBackValues(); 
			return false;
		}
		
#region 
        /// <summary>
        /// 查找系统线程中是否运行ArcMap
        /// </summary>
        /// <returns></returns>
		private bool GetProcesses()
		{
			Process objArcGISProcess = new Process();
			bool bSwitch = false; 
            frmMotherform.CHLabelTop("查找运行的ArcGIS应用程序...");
            try
			{
				foreach (Process objProcess in Process.GetProcesses())
				{
					if (objProcess.ProcessName == "ArcMap")
					{
						bSwitch = true;
					}
				}
				
				if (bSwitch == true)
				{
					return true;
				}
				else
				{
                    frmMotherform.CHLabelTop("必须先运行ArcMap程序！");
                    return false;
				}
			}
			catch (Exception ex)
			{
				ErrorMsg("获取应用程序失败", ex.Message, ex.StackTrace, "GetProcesses");
				return false;
			}
		}
        /// <summary>
        /// 获取线程中的ArcMap线程
        /// </summary>
        /// <returns></returns>
		private bool GetApplication()
		{
			long Zahl = 0;
			m_ObjApp = null;
            frmMotherform.CHLabelTop(string.Format("获取ArcMap线程..."));
            try
			{
				m_ObjAppROT = new AppROT();
				Zahl = m_ObjAppROT.Count;
				
				if (Zahl > 1)
				{
                    MessageBox.Show(string.Format("只能运行一个ArcMap应用程序"), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
				else
				{
					if (m_ObjAppROT.Item[0] is IMxApplication) 
					{
						m_ObjApp = m_ObjAppROT.Item[0];
						m_ObjDoc = m_ObjApp.Document as IMxDocument;
                        m_ObjObjectCreator = m_ObjApp as IObjectFactory;
						return true;
					}
				}
                return false;
				
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Referenzieren auf die ArcMap-Instanz ", ex.Message, ex.StackTrace, "GetApplication");
				return false;
			}
		}
        /// <summary>
        /// 获取地图文档信息
        /// </summary>
        /// <returns></returns>
		private bool GetMap()
		{
            frmMotherform.CHLabelTop(string.Format("获取当前的地图信息..."));
            try
			{
				if (m_ObjDoc.Maps.Count> 1) 
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
        /// 地图符号分析
        /// </summary>
        /// <returns></returns>
		private bool AnalyseLayerSymbology()
		{
			ILayer objLayer = default(ILayer); 
			int iNumberLayers = 0; 
			string cLayerName = ""; 
			ISymbol objFstOrderSymbol; 
			m_StrProject = new StructProject(); 
			iNumberLayers = m_ObjMap.LayerCount;
			m_StrProject.LayerList = new ArrayList();
			try
			{
				int i = 0;
				for (i = 0; i <= iNumberLayers - 1; i++)
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
					IGroupLayer objGRL = default(IGroupLayer);
					ICompositeLayer objCompLayer = default(ICompositeLayer);
                    objGRL = objLayer as IGroupLayer;
                    objCompLayer = objGRL as ICompositeLayer;
					for (j = 0; j <= objCompLayer.Count- 1; j++)
					{
						SpreadLayerStructure(objCompLayer.Layer[j]); 
					}
				}
				else if (objLayer is IFeatureLayer)
				{
					if (objLayer is IGeoFeatureLayer)
					{
						IGeoFeatureLayer objGFL = default(IGeoFeatureLayer);
                        objGFL = objLayer as IGeoFeatureLayer;
						if (objGFL.Renderer is IUniqueValueRenderer)
						{
							IUniqueValueRenderer objRenderer = default(IUniqueValueRenderer);
                            objRenderer = objGFL.Renderer as IUniqueValueRenderer;
							m_StrProject.LayerList.Add(StoreStructUVRenderer(objRenderer, objLayer as IFeatureLayer));
							AddOneToLayerNumber();
						}
						if (objGFL.Renderer is ISimpleRenderer)
						{
							ISimpleRenderer objRenderer = default(ISimpleRenderer);
                            objRenderer = objGFL.Renderer as ISimpleRenderer;
                            m_StrProject.LayerList.Add(StoreStructSimpleRenderer(objRenderer, objLayer as IFeatureLayer));
							AddOneToLayerNumber();
						}
						if (objGFL.Renderer is IClassBreaksRenderer)
						{
							IClassBreaksRenderer objRenderer = default(IClassBreaksRenderer);
                            objRenderer = objGFL.Renderer as IClassBreaksRenderer;
							m_StrProject.LayerList.Add(StoreStructCBRenderer(objRenderer, objLayer as IFeatureLayer));
							AddOneToLayerNumber();
						}
						//andere Renderer werden evtl. sp鋞er abgedeckt
						//If TypeOf objGFL.Renderer Is IChartRenderer Then
						//    Dim objRenderer As IChartRenderer
						//    objRenderer = objGFL.Renderer
						//    objFstOrderSymbol = objRenderer.Symbol
						//    'TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO
						//End If
						//If TypeOf objGFL.Renderer Is IDotDensityRenderer Then
						//    Dim objRenderer As IDotDensityRenderer
						//    objRenderer = objGFL.Renderer
						//    'TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO
						//End If
						//If TypeOf objGFL.Renderer Is IProportionalSymbolRenderer Then
						//    Dim objRenderer As IProportionalSymbolRenderer
						//    objRenderer = objGFL.Renderer
						//    'TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO
						//End If
						//If TypeOf objGFL.Renderer Is IScaleDependentRenderer Then
						//    Dim objRenderer As IScaleDependentRenderer
						//    objRenderer = objGFL.Renderer
						//    'TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO
						//End If
					}
					
				}
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
		
		
#region Hilfsfunktionen
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
		private bool GimmeUniqeValuesForFieldname(ITable Table, string FieldName)
		{
			IQueryDef pQueryDef = default(IQueryDef);
			IRow pRow = default(IRow);
			ICursor pCursor = default(ICursor);
			IFeatureCursor pFeatureCursor;
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
				//Hier wird die erhaltene Spalte durchlaufen und in der Arraylist abgespeichert
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
				ErrorMsg("Fehler beim Generieren der UniqueValues", ex.Message, ex.StackTrace, "GimmeUniqeValuesForFieldname");
                return false;
			}
		}
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
		private void GimmeUniqueValuesFromShape(ITable Table, ArrayList FieldNames)
		{
			IQueryFilter pQueryFilter = default(IQueryFilter);
			pQueryFilter = new QueryFilter();
			ICursor pCursor = default(ICursor);
			IDataStatistics pData = default(IDataStatistics);
			pData = new DataStatistics();
			short i = 0;
			short bla;
			IEnumerator objEnum = default(IEnumerator);
			ArrayList al = default(ArrayList);
			
			try
			{
				if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
				{
					if (MessageBox.Show("ACHTUNG: Sie haben Layer aus einem Shapefile mit mehr als 1 klassifizierenden Feld. " + 
						"Wenn Sie diese Art von Layer analysieren wollen, kann das sehr lange dauern (etliche Minuten bis im 2-stelligen" + 
						"Bereich, je nach Prozessorleistung und Datenmenge TIP: Speichern Sie die Inhalte in " + 
						"einer Personal GDB oder SDE und starten Sie die Analyse erneut-dies dauert je nach Datenmenge nur ein paar Sekunden). " + 
						"Wollen Sie trotzdem mit der Analyse fortfahren?", "ACHTUNG-RECHENINTENSIVE FUNKTION", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
					{
						for (i = 0; i <= FieldNames.Count - 1; i++)
						{
							this.frmMotherform.CHLabelSmall("Bitte warten - Klassifizierungsfeld " + System.Convert.ToString(i + 1) + " von " + System.Convert.ToString(FieldNames.Count));
                            pData.Field = FieldNames[i].ToString();
							pQueryFilter.SubFields = FieldNames[i].ToString();
							pCursor = Table.Search(pQueryFilter, false);
							pData.Cursor = pCursor;
							frmMotherform.DoEvents();
							objEnum = pData.UniqueValues;
							al = new ArrayList();
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
					else
					{
						this.MyTermination();
					}
				}
				else if (frmMotherform.m_enumLang == Motherform.Language.English)
				{
					if (MessageBox.Show("ATTENTION: You have a layer from a shape-file with more than one classifying field. " + 
						"If you want to analyse that kind of layer, it can take A LOT OF TIME (several Minutes until hours " + 
						"depending on your processor and the dimension of your data TIP: Store your data in " + 
						"a personal GDB or an ArcSDE-DB and start the analyse again-that takes only a few seconds). " + 
						"do you anyhow want to continue?", "ATTENTION!-CALCULATION INTENSIVE FUNCTION", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
					{
						for (i = 0; i <= FieldNames.Count - 1; i++)
						{
							this.frmMotherform.CHLabelSmall("Please wait - classified Field Nr. " + System.Convert.ToString(i + 1) + " of " + System.Convert.ToString(FieldNames.Count));
							pData.Field = FieldNames[i].ToString();
							pQueryFilter.SubFields = FieldNames[i].ToString();
							pCursor = Table.Search(pQueryFilter, false);
							pData.Cursor = pCursor;
							frmMotherform.DoEvents();
							objEnum = pData.UniqueValues;
							al = new ArrayList();
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
					else
					{
						this.MyTermination();
					}
				}
				
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Erstellen der UniqueValues", ex.Message, ex.StackTrace, "GimmeUniqueValuesFromShape");
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
		
		private ArrayList GimmeSeperateFieldValues(string value, string FieldDelimiter)
		{
			ArrayList alSepValues = new ArrayList();
			IFeatureCursor objFeatCurs;
			IQueryFilter objQueryFilter;
			objQueryFilter = new QueryFilter();
			IFeature objFeature;
			string cCompare;
			int zahl;
			
			try
			{
				switch (m_alClassifiedFields.Count)
				{
					case 2:
						ArrayList al1_1 = default(ArrayList);
						ArrayList al2_1 = default(ArrayList);
						al1_1 = (ArrayList) (m_alClassifiedFields[0]);
						al2_1 = (ArrayList) (m_alClassifiedFields[1]);
						int i_1 = 0;
						int j_1 = 0;
						for (i_1 = 0; i_1 <= al1_1.Count - 1; i_1++)
						{
							for (j_1 = 0; j_1 <= al2_1.Count - 1; j_1++)
							{
								if (((al1_1[i_1]).ToString() + FieldDelimiter + (al2_1[j_1]).ToString() ) == value)
								{
									alSepValues.Add(al1_1[i_1]);
									alSepValues.Add(al2_1[j_1]);
                                    return alSepValues;
                                }
							}
						}
						break;
					case 3:
						ArrayList al1 = default(ArrayList);
						ArrayList al2 = default(ArrayList);
						ArrayList al3 = default(ArrayList);
						al1 = (ArrayList) (m_alClassifiedFields[0]);
						al2 = (ArrayList) (m_alClassifiedFields[1]);
						al3 = (ArrayList) (m_alClassifiedFields[2]);
						int i = 0;
						int j = 0;
						int k = 0;
						for (i = 0; i <=(m_alClassifiedFields[0] as ArrayList).Count - 1; i++)
						{
							for (j = 0; j <= (m_alClassifiedFields[1] as ArrayList).Count - 1; j++)
							{
								for (k = 0; k <= (m_alClassifiedFields[2] as ArrayList).Count - 1; k++)
								{
									if (((al1[i]).ToString() + FieldDelimiter + (al2[j]).ToString() + FieldDelimiter + (al3[k]).ToString() ) == value)
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
			IEnumColors EColors = default(IEnumColors);
			ArrayList AL = default(ArrayList);
			int i = 0;
			EColors = ColorRamp.Colors;
			for (i = 0; i <= ColorRamp.Size - 1; i++)
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
			IRgbColor objRGB = default(IRgbColor);
			if (color.Transparency == 0)
			{
				cCol = "";
			}
			else
			{
				objRGB = new RgbColor();
				
				objRGB.RGB = color.RGB;
				cRed = CheckDigits(Conversion.Hex(objRGB.Red).ToString());
				cGreen = CheckDigits(Conversion.Hex(objRGB.Green).ToString());
				cBlue = CheckDigits(Conversion.Hex(objRGB.Blue).ToString());
				cCol = "#" + cRed + cGreen + cBlue;
			}
			
			return cCol;
		}
        /// <summary>
        /// 获取十进制数据
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
        /// 根据图层获取注记符号
        /// </summary>
        /// <param name="objLayer"></param>
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
                    annoPropsColl.QueryItem(0, out  annoLayerProps, out null_ESRIArcGISCartoIElementCollection, out null_ESRIArcGISCartoIElementCollection2);
					if (annoLayerProps is ILabelEngineLayerProperties&& annoLayerProps.DisplayAnnotation)
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
			MessageBox.Show(Message + "\r\n" + ExMessage + "\r\n" + Stack, "ArcGIS_SLD_Converter | Analize_ArcMap_Symbols | " + FunctionName, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
			MessageBox.Show(Message, "ArcGIS_SLD_Converter | Analize_ArcMap_Symbols | " + FunctionName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return null;
		}
        /// <summary>
        /// 退出程序
        /// </summary>
        /// <returns></returns>
		public bool MyTermination()
		{
            Application.Exit();
			return default(bool);
		}
#endregion
	}
}
