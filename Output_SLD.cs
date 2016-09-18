using System;
using System.Drawing;
using System.Diagnostics;
using System.Data;
using Microsoft.VisualBasic;
using System.Collections;
using System.Windows.Forms;
using System.Xml;
using stdole;
using Microsoft.VisualBasic.CompilerServices;
namespace ArcGIS_SLD_Converter
{
	public class Output_SLD
	{
#region 全局变量
		private Motherform frmMotherForm;
		private Analize_ArcMap_Symbols m_objData;
		private Analize_ArcMap_Symbols.StructProject m_strDataSavings;
		private XMLHandle m_objXMLHandle;
		private string m_cFilename; 
		private string m_cFile; 
		private string m_cPath; 
		private bool m_bSepFiles; 
		private string m_bIncludeLayerNames; 
#endregion	
        		
#region Routinen
		/// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="Mother"></param>
        /// <param name="Analize"></param>
        /// <param name="Filename"></param>
		public Output_SLD(Motherform Mother, Analize_ArcMap_Symbols Analize, string Filename)
		{
			frmMotherForm = Mother;
			m_cFilename = Filename;
			m_bSepFiles = frmMotherForm.GetInfoSeparateLayers;
			m_cFile = frmMotherForm.GetSLDFile;
			m_cPath = frmMotherForm.GetSLDPath;
			m_objData = Analize;
            m_strDataSavings = (Analize_ArcMap_Symbols.StructProject)m_objData.GetProjectData;
			m_bIncludeLayerNames = frmMotherForm.GetInfoIncludeLayerNames;
			CentralProcessingFunc();
		}
		
		public Output_SLD()
		{
			CentralProcessingFunc();
		}
		
#endregion 
		

		
#region 
		/// <summary>
        /// 开始分析符号，并将符号转换成SLD
        /// </summary>
        /// <returns></returns>
		private bool CentralProcessingFunc()
		{
			bool bSuccess = false;
            frmMotherForm.CHLabelTop(string.Format("输出SLD文件..."));
            frmMotherForm.CHLabelBottom(string.Format("正在输出SLD文件..."));
            if (WriteToSLD() == true)
			{
				bSuccess = true;
			}
            frmMotherForm.CHLabelTop(string.Format("开始..."));
            if (bSuccess == true)
			{
                frmMotherForm.CHLabelBottom(string.Format("成功创建文件..."));
                //如果描述文件存在，则加载设置的XML头文件
                if (frmMotherForm.chkValidate.Checked == true)
				{
					ValidateSLD ValSLD = new ValidateSLD(frmMotherForm);
				}
				else
				{
					frmMotherForm.CHLabelSmall("");
				}
			}
			else
			{
                frmMotherForm.CHLabelBottom(string.Format("无法创建文件..."));
                frmMotherForm.CHLabelSmall("");
            }
			
			return true;
		}
        /// <summary>
        /// 创建SLD文件
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="bIncludeLayerNames"></param>
        /// <returns></returns>
		private bool CreateSLD(string FileName, bool bIncludeLayerNames)
		{
			m_objXMLHandle = new XMLHandle(FileName, bIncludeLayerNames);
			m_objXMLHandle.CreateNewFile(true, bIncludeLayerNames);
			return default(bool);
		}
        /// <summary>
        /// 将分析的符号信息写入SLD
        /// </summary>
        /// <returns></returns>
		public bool WriteToSLD()
		{
			int i = 0;
			int j = 0;
			int l = 0;
			string cLayerName = "";
			ArrayList objFieldValues = default(ArrayList);
			bool bDoOneLayer = default(bool);
			double dummy = 0; 
			if (m_bSepFiles == true)
			{
				bDoOneLayer = false;
			}
			else
			{
				bDoOneLayer = true;
				CreateSLD(m_cFilename, bool.Parse(m_bIncludeLayerNames));
			}
			try
			{
				for (i = 0; i <= m_strDataSavings.LayerCount - 1; i++)
                {
                    #region 获取图层名称
                    string strDatasetName = "";
                    ArrayList objSymbols = default(ArrayList); 
                    if (m_strDataSavings.LayerList[i] is Analize_ArcMap_Symbols.StructUniqueValueRenderer)
                    {
                        Analize_ArcMap_Symbols.StructUniqueValueRenderer temp = (Analize_ArcMap_Symbols.StructUniqueValueRenderer)m_strDataSavings.LayerList[i];
                        strDatasetName = temp.DatasetName;
                        objSymbols = temp.SymbolList;
                        cLayerName = System.Convert.ToString(temp.LayerName);
                    }
                    else if (m_strDataSavings.LayerList[i] is Analize_ArcMap_Symbols.StructClassBreaksRenderer)
                    {
                        Analize_ArcMap_Symbols.StructClassBreaksRenderer temp = (Analize_ArcMap_Symbols.StructClassBreaksRenderer)m_strDataSavings.LayerList[i];
                        strDatasetName = temp.DatasetName;
                        objSymbols = temp.SymbolList;
                        cLayerName = System.Convert.ToString(temp.LayerName);
                    }
                    else if (m_strDataSavings.LayerList[i] is Analize_ArcMap_Symbols.StructSimpleRenderer)
                    {
                        Analize_ArcMap_Symbols.StructSimpleRenderer temp = (Analize_ArcMap_Symbols.StructSimpleRenderer)m_strDataSavings.LayerList[i];
                        strDatasetName = temp.DatasetName;
                        objSymbols = temp.SymbolList;
                        cLayerName = System.Convert.ToString(temp.LayerName);
                    }
                    #endregion
                    frmMotherForm.CHLabelBottom(string.Format("正在处理图层【{0}】...", cLayerName));
                    if (bDoOneLayer == false)
					{
						CreateSLD(m_cFilename + "_" + cLayerName + ".sld", bool.Parse(m_bIncludeLayerNames));
					}
					if (Convert.ToBoolean(m_bIncludeLayerNames))
					{
						//' ARIS: Standard SLD output
						m_objXMLHandle.CreateElement("NamedLayer");
						m_objXMLHandle.CreateElement("LayerName");
                        m_objXMLHandle.SetElementText(System.Convert.ToString(strDatasetName));
						m_objXMLHandle.CreateElement("UserStyle");
						m_objXMLHandle.CreateElement("StyleName");
						m_objXMLHandle.SetElementText("Style1");
						m_objXMLHandle.CreateElement("FeatureTypeStyle");
						m_objXMLHandle.CreateElement("FeatureTypeName");
                        m_objXMLHandle.SetElementText(System.Convert.ToString(strDatasetName));
					}
					else
					{
						//' ARIS: WorldMap SLD output
						m_objXMLHandle.CreateElement("FeatureTypeStyle");
						m_objXMLHandle.CreateElement("FeatureTypeName");
                        m_objXMLHandle.SetElementText(System.Convert.ToString(strDatasetName));
						m_objXMLHandle.CreateElement("FeatureTypeTitle");
                        m_objXMLHandle.SetElementText(System.Convert.ToString(strDatasetName));
					}
					
					//XML-Schreibanweisungen auf Layerebene und auf Symbolebene
					for (j = 0; j <= objSymbols.Count - 1; j++) //IN DER SCHLEIFE AUF SYMBOLEBENE objSymbols(j) repr鋝entiert 1 Symbol!!!
					{
						if (frmMotherForm.m_enumLang == Motherform.Language.Deutsch)
						{
							frmMotherForm.CHLabelSmall("Symbol " + (j + 1).ToString() + " von " + objSymbols.Count.ToString());
						}
						else if (frmMotherForm.m_enumLang == Motherform.Language.English)
						{
							frmMotherForm.CHLabelSmall("Symbol " + (j + 1).ToString() + " of " + objSymbols.Count.ToString());
                        }

                        #region
                        string StrLabel = "";
                        double StrLowerLimit = 0.00;
                        double StrUpperLimit = 0.00;
                        if (objSymbols[j] is Analize_ArcMap_Symbols.StructSimpleMarkerSymbol)
                        {
                            Analize_ArcMap_Symbols.StructSimpleMarkerSymbol temp=(Analize_ArcMap_Symbols.StructSimpleMarkerSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructCharacterMarkerSymbol)
                        {
                            Analize_ArcMap_Symbols.StructCharacterMarkerSymbol temp = (Analize_ArcMap_Symbols.StructCharacterMarkerSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructPictureMarkerSymbol)
                        {
                            Analize_ArcMap_Symbols.StructPictureMarkerSymbol temp = (Analize_ArcMap_Symbols.StructPictureMarkerSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructArrowMarkerSymbol)
                        {
                            Analize_ArcMap_Symbols.StructArrowMarkerSymbol temp = (Analize_ArcMap_Symbols.StructArrowMarkerSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructSimpleLineSymbol)
                        {
                            Analize_ArcMap_Symbols.StructSimpleLineSymbol temp = (Analize_ArcMap_Symbols.StructSimpleLineSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructCartographicLineSymbol)
                        {
                            Analize_ArcMap_Symbols.StructCartographicLineSymbol temp = (Analize_ArcMap_Symbols.StructCartographicLineSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructHashLineSymbol)
                        {
                            Analize_ArcMap_Symbols.StructHashLineSymbol temp = (Analize_ArcMap_Symbols.StructHashLineSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructMarkerLineSymbol)
                        {
                            Analize_ArcMap_Symbols.StructMarkerLineSymbol temp = (Analize_ArcMap_Symbols.StructMarkerLineSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructPictureLineSymbol)
                        {
                            Analize_ArcMap_Symbols.StructPictureLineSymbol temp = (Analize_ArcMap_Symbols.StructPictureLineSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructSimpleFillSymbol)
                        {
                            Analize_ArcMap_Symbols.StructSimpleFillSymbol temp = (Analize_ArcMap_Symbols.StructSimpleFillSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructMarkerFillSymbol)
                        {
                            Analize_ArcMap_Symbols.StructMarkerFillSymbol temp = (Analize_ArcMap_Symbols.StructMarkerFillSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructLineFillSymbol)
                        {
                            Analize_ArcMap_Symbols.StructLineFillSymbol temp = (Analize_ArcMap_Symbols.StructLineFillSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructDotDensityFillSymbol)
                        {
                            Analize_ArcMap_Symbols.StructDotDensityFillSymbol temp = (Analize_ArcMap_Symbols.StructDotDensityFillSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructPictureFillSymbol)
                        {
                            Analize_ArcMap_Symbols.StructPictureFillSymbol temp = (Analize_ArcMap_Symbols.StructPictureFillSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructGradientFillSymbol)
                        {
                            Analize_ArcMap_Symbols.StructGradientFillSymbol temp = (Analize_ArcMap_Symbols.StructGradientFillSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructBarChartSymbol)
                        {
                            Analize_ArcMap_Symbols.StructBarChartSymbol temp = (Analize_ArcMap_Symbols.StructBarChartSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructPieChartSymbol)
                        {
                            Analize_ArcMap_Symbols.StructPieChartSymbol temp = (Analize_ArcMap_Symbols.StructPieChartSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructStackedChartSymbol)
                        {
                            Analize_ArcMap_Symbols.StructStackedChartSymbol temp = (Analize_ArcMap_Symbols.StructStackedChartSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructTextSymbol)
                        {
                            Analize_ArcMap_Symbols.StructTextSymbol temp = (Analize_ArcMap_Symbols.StructTextSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructMultilayerMarkerSymbol)
                        {
                            Analize_ArcMap_Symbols.StructMultilayerMarkerSymbol temp = (Analize_ArcMap_Symbols.StructMultilayerMarkerSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructMultilayerLineSymbol)
                        {
                            Analize_ArcMap_Symbols.StructMultilayerLineSymbol temp = (Analize_ArcMap_Symbols.StructMultilayerLineSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructMultilayerFillSymbol)
                        {
                            Analize_ArcMap_Symbols.StructMultilayerFillSymbol temp = (Analize_ArcMap_Symbols.StructMultilayerFillSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        #endregion

                        //HIER DIE UNTERSCHEIDUNGEN NACH DEN EINZELNEN RENDERERN: UNIQUEVALUERENDERER
						if (m_strDataSavings.LayerList[i] is Analize_ArcMap_Symbols.StructUniqueValueRenderer)
						{
							Analize_ArcMap_Symbols.StructUniqueValueRenderer objStructUVR = new Analize_ArcMap_Symbols.StructUniqueValueRenderer();
                            objStructUVR = (Analize_ArcMap_Symbols.StructUniqueValueRenderer)m_strDataSavings.LayerList[i]; //Zuweisung des StructsUVR. Repr鋝entiert je einen Layer!!!
							m_objXMLHandle.CreateElement("Rule");
							m_objXMLHandle.CreateElement("RuleName");
                            m_objXMLHandle.SetElementText(StrLabel);
							m_objXMLHandle.CreateElement("Title");
                            m_objXMLHandle.SetElementText(StrLabel);
							m_objXMLHandle.CreateElement("Filter");
							if (frmMotherForm.chkScale.Checked == true)
							{
								m_objXMLHandle.CreateElement("MinScale");
								m_objXMLHandle.SetElementText(frmMotherForm.cboLowScale.Text);
								m_objXMLHandle.CreateElement("MaxScale");
								m_objXMLHandle.SetElementText(frmMotherForm.cboHighScale.Text);
							}
							if (objStructUVR.FieldCount > 1) //Nur wenn nach mehr als 1 Feld klassifiziert wurde, wird der <AND>-Tag gesetzt
							{
								m_objXMLHandle.CreateElement("And");
								for (l = 0; l <= objStructUVR.FieldCount - 1; l++) //Die Schleife ist nur daf黵 da, falls nach mehreren Feldern klassifiziert wurde
								{
									m_objXMLHandle.CreateElement("PropertyIsEqualTo"); //Sie schreibt pro Feld nach dem klass. wurde das <PropertyIsEqualTo> und alle Kinder
									m_objXMLHandle.CreateElement("PropertyName");
									m_objXMLHandle.SetElementText(System.Convert.ToString(objStructUVR.FieldNames[l]));
									m_objXMLHandle.CreateElement("Fieldvalue");
									m_objXMLHandle.SetElementText(System.Convert.ToString(objFieldValues[l]));
								}
							}
							else if (objStructUVR.FieldCount == 1)
							{
								if (objFieldValues.Count > 1)
								{
									m_objXMLHandle.CreateElement("Or");
								}
								for (l = 0; l <= objFieldValues.Count - 1; l++) //If multiple values grouped in same class
								{
									m_objXMLHandle.CreateElement("PropertyIsEqualTo"); //Sie schreibt pro Feld nach dem klass. wurde das <PropertyIsEqualTo> und alle Kinder
									m_objXMLHandle.CreateElement("PropertyName");
									m_objXMLHandle.SetElementText(System.Convert.ToString(objStructUVR.FieldNames[0]));
									m_objXMLHandle.CreateElement("Fieldvalue");
									m_objXMLHandle.SetElementText(System.Convert.ToString(objFieldValues[l]));
								}
							}
							//UNTERSCHEIDUNG NACH FEATURECLASS DES BETREFFENDEN SYMBOLS
							switch (objStructUVR.FeatureCls)
							{
								case Analize_ArcMap_Symbols.FeatureClass.PointFeature:
									WritePointFeatures(objSymbols[j]);
									break;
								case Analize_ArcMap_Symbols.FeatureClass.LineFeature:
									WriteLineFeatures(objSymbols[j]);
									break;
								case Analize_ArcMap_Symbols.FeatureClass.PolygonFeature:
									WritePolygonFeatures(objSymbols[j]);
									break;
							}
							//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
							//HIER DIE UNTERSCHEIDUNGEN NACH DEN EINZELNEN RENDERERN: CLASSBREAKSRENDERER
						}
						else if (m_strDataSavings.LayerList[i] is Analize_ArcMap_Symbols.StructClassBreaksRenderer)
						{
							Analize_ArcMap_Symbols.StructClassBreaksRenderer objStructCBR = new Analize_ArcMap_Symbols.StructClassBreaksRenderer();
							objStructCBR = (Analize_ArcMap_Symbols.StructClassBreaksRenderer)m_strDataSavings.LayerList[i];
							m_objXMLHandle.CreateElement("Rule");
							m_objXMLHandle.CreateElement("RuleName");
                            m_objXMLHandle.SetElementText(StrLabel);
							m_objXMLHandle.CreateElement("Title");
                            m_objXMLHandle.SetElementText(StrLabel);
							m_objXMLHandle.CreateElement("Filter");
							if (frmMotherForm.chkScale.Checked == true)
							{
								m_objXMLHandle.CreateElement("MinScale");
								m_objXMLHandle.SetElementText(frmMotherForm.cboLowScale.Text);
								m_objXMLHandle.CreateElement("MaxScale");
								m_objXMLHandle.SetElementText(frmMotherForm.cboHighScale.Text);
							}
							m_objXMLHandle.CreateElement("PropertyIsBetween");
							m_objXMLHandle.CreateElement("PropertyName");
							m_objXMLHandle.SetElementText(objStructCBR.FieldName);
							m_objXMLHandle.CreateElement("LowerBoundary");
							m_objXMLHandle.CreateElement("Fieldvalue");
                            //As ArrayList member the type is no more recognized from compiler. If saving in a dummy double its recognized again
                            dummy = StrLowerLimit;
							m_objXMLHandle.SetElementText(CommaToPoint(dummy));
							m_objXMLHandle.CreateElement("UpperBoundary");
							m_objXMLHandle.CreateElement("Fieldvalue");
                            //As ArrayList member the type is no more recognized from compiler. If saving in a dummy double its recognized again
                            dummy = StrUpperLimit; 
							m_objXMLHandle.SetElementText(CommaToPoint(dummy));
							switch (objStructCBR.FeatureCls)
							{
								case Analize_ArcMap_Symbols.FeatureClass.PointFeature:
									WritePointFeatures(objSymbols[j]);
									break;
								case Analize_ArcMap_Symbols.FeatureClass.LineFeature:
									WriteLineFeatures(objSymbols[j]);
									break;
								case Analize_ArcMap_Symbols.FeatureClass.PolygonFeature:
									WritePolygonFeatures(objSymbols[j]);
									break;
							}
							//HIER DIE UNTERSCHEIDUNGEN NACH DEN EINZELNEN RENDERERN: SIMPLERENDERER
						}
						else if (m_strDataSavings.LayerList[i] is Analize_ArcMap_Symbols.StructSimpleRenderer)
						{
							Analize_ArcMap_Symbols.StructSimpleRenderer objStructSR = new Analize_ArcMap_Symbols.StructSimpleRenderer();
                            objStructSR = (Analize_ArcMap_Symbols.StructSimpleRenderer)m_strDataSavings.LayerList[i];
							m_objXMLHandle.CreateElement("Rule");
							m_objXMLHandle.CreateElement("RuleName");
                            m_objXMLHandle.SetElementText(System.Convert.ToString(objStructSR.DatasetName));
							m_objXMLHandle.CreateElement("Title");
                            m_objXMLHandle.SetElementText(System.Convert.ToString(objStructSR.DatasetName));
							switch (objStructSR.FeatureCls)
							{
								case Analize_ArcMap_Symbols.FeatureClass.PointFeature:
									WritePointFeatures(objSymbols[j]);
									break;
								case Analize_ArcMap_Symbols.FeatureClass.LineFeature:
									WriteLineFeatures(objSymbols[j]);
									break;
								case Analize_ArcMap_Symbols.FeatureClass.PolygonFeature:
									WritePolygonFeatures(objSymbols[j]);
									break;
							}
                            WriteAnnotation(objStructSR.Annotation);
						}
					}
					if (bDoOneLayer == false)
					{
						m_objXMLHandle.SaveDoc(); //If separate layer, the files have to be saved here
					}
				}
				if (bDoOneLayer == true)
				{
					m_objXMLHandle.SaveDoc(); //else the file has to be saved here
				}
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("Konnte die SLD nicht schreiben", ex.Message, ex.StackTrace, "WriteToSLD");
				return false;
			}
		}
		/// <summary>
        /// 写标注符号信息
        /// </summary>
        /// <param name="Annotation"></param>
        /// <returns></returns>
		private bool WriteAnnotation(Analize_ArcMap_Symbols.StructAnnotation Annotation)
		{
			if (Annotation.IsSingleProperty && Annotation.PropertyName != "")
			{
				m_objXMLHandle.CreateElement("TextSymbolizer");
				m_objXMLHandle.CreateElement("TextLabel");
				m_objXMLHandle.CreateElement("TextLabelProperty");
				m_objXMLHandle.SetElementText(Annotation.PropertyName);
				m_objXMLHandle.CreateElement("TextFont");
				m_objXMLHandle.CreateElement("TextFontCssParameter");
				m_objXMLHandle.CreateAttribute("name");
				m_objXMLHandle.SetAttributeValue("font-family");
				m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("TextFont", Annotation.TextSymbol));
				if (GetValueFromSymbolstruct("TextFontAlt", Annotation.TextSymbol) != "")
				{
					m_objXMLHandle.CreateElement("TextFontCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("font-family");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("TextFontAlt", Annotation.TextSymbol));
				}
				m_objXMLHandle.CreateElement("TextFontCssParameter");
				m_objXMLHandle.CreateAttribute("name");
				m_objXMLHandle.SetAttributeValue("font-size");
				m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("TextFontSize", Annotation.TextSymbol));
				m_objXMLHandle.CreateElement("TextFontCssParameter");
				m_objXMLHandle.CreateAttribute("name");
				m_objXMLHandle.SetAttributeValue("font-style");
				m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("TextFontStyle", Annotation.TextSymbol));
				m_objXMLHandle.CreateElement("TextFontCssParameter");
				m_objXMLHandle.CreateAttribute("name");
				m_objXMLHandle.SetAttributeValue("font-weight");
				m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("TextFontWeight", Annotation.TextSymbol));
				m_objXMLHandle.CreateElement("TextFill");
				m_objXMLHandle.CreateElement("TextFillCssParameter");
				m_objXMLHandle.CreateAttribute("name");
				m_objXMLHandle.SetAttributeValue("fill");
				m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("TextColor", Annotation.TextSymbol));
				m_objXMLHandle.CreateElement("TextFillCssParameter");
				m_objXMLHandle.CreateAttribute("name");
				m_objXMLHandle.SetAttributeValue("fill-opacity");
				m_objXMLHandle.SetElementText("1.0");
			}
			return true;
		}
		private bool WritePointFeatures(object Symbol)
		{
			try
			{
				int layerIdx = 0;
				int maxLayerIdx = 1;
				
				if (Symbol is Analize_ArcMap_Symbols.StructMultilayerMarkerSymbol)
				{
					Analize_ArcMap_Symbols.StructMultilayerMarkerSymbol objTempStruct = new Analize_ArcMap_Symbols.StructMultilayerMarkerSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructMultilayerMarkerSymbol)Symbol;
					maxLayerIdx = objTempStruct.LayerCount;
				}
				for (layerIdx = 0; layerIdx <= maxLayerIdx - 1; layerIdx++)
				{
					m_objXMLHandle.CreateElement("PointSymbolizer");
					m_objXMLHandle.CreateElement("PointGraphic");
					m_objXMLHandle.CreateElement("Mark");
					m_objXMLHandle.CreateElement("PointWellKnownName");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("WellKnownName", Symbol, layerIdx));
					if (GetValueFromSymbolstruct("PointColor", Symbol, layerIdx) != "")
					{
						m_objXMLHandle.CreateElement("PointFill");
						m_objXMLHandle.CreateElement("PointFillCssParameter");
						m_objXMLHandle.CreateAttribute("name");
						m_objXMLHandle.SetAttributeValue("fill");
						m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PointColor", Symbol, layerIdx));
						m_objXMLHandle.CreateElement("PointFillCssParameter");
						m_objXMLHandle.CreateAttribute("name");
						m_objXMLHandle.SetAttributeValue("fill-opacity");
						m_objXMLHandle.SetElementText("1.0");
					}
					if (GetValueFromSymbolstruct("PointOutlineColor", Symbol, layerIdx) != "" && GetValueFromSymbolstruct("PointOutlineSize", Symbol, layerIdx) != "0")
					{
						m_objXMLHandle.CreateElement("PointStroke");
						m_objXMLHandle.CreateElement("PointStrokeCssParameter");
						m_objXMLHandle.CreateAttribute("name");
						m_objXMLHandle.SetAttributeValue("stroke");
						m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PointOutlineColor", Symbol, layerIdx));
						m_objXMLHandle.CreateElement("PointStrokeCssParameter");
						m_objXMLHandle.CreateAttribute("name");
						m_objXMLHandle.SetAttributeValue("stroke-width");
						m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PointOutlineSize", Symbol, layerIdx));
						m_objXMLHandle.CreateElement("PointStrokeCssParameter");
						m_objXMLHandle.CreateAttribute("name");
						m_objXMLHandle.SetAttributeValue("stroke-opacity");
						m_objXMLHandle.SetElementText("1.0");
					}
					m_objXMLHandle.CreateElement("PointSize");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PointSize", Symbol, layerIdx));
					m_objXMLHandle.CreateElement("PointRotation");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PointRotation", Symbol, layerIdx));
				}
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Schreiben der Schraffur", ex.Message, ex.StackTrace, "WritePointFeatures");
				return false;
			}
		}
		private bool WriteLineFeatures(object Symbol)
		{
			try
			{
				int layerIdx = 0;
				int maxLayerIdx = 1;
				
				if (Symbol is Analize_ArcMap_Symbols.StructMultilayerLineSymbol)
				{
					Analize_ArcMap_Symbols.StructMultilayerLineSymbol objTempStruct = new Analize_ArcMap_Symbols.StructMultilayerLineSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructMultilayerLineSymbol)Symbol;
					maxLayerIdx = objTempStruct.LayerCount;
				}
				for (layerIdx = 0; layerIdx <= maxLayerIdx - 1; layerIdx++)
				{
					if (GetValueFromSymbolstruct("LineColor", Symbol, layerIdx) != "" && GetValueFromSymbolstruct("LineWidth", Symbol, layerIdx) != "0")
					{
						m_objXMLHandle.CreateElement("LineSymbolizer");
						m_objXMLHandle.CreateElement("LineStroke");
						m_objXMLHandle.CreateElement("LineCssParameter");
						m_objXMLHandle.CreateAttribute("name");
						m_objXMLHandle.SetAttributeValue("stroke");
						m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("LineColor", Symbol, layerIdx));
						m_objXMLHandle.CreateElement("LineCssParameter");
						m_objXMLHandle.CreateAttribute("name");
						m_objXMLHandle.SetAttributeValue("stroke-width");
						m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("LineWidth", Symbol, layerIdx));
						m_objXMLHandle.CreateElement("LineCssParameter");
						m_objXMLHandle.CreateAttribute("name");
						m_objXMLHandle.SetAttributeValue("stroke-opacity");
						m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("LineOpacity", Symbol, layerIdx));
						if (GetValueFromSymbolstruct("LineDashArray", Symbol, layerIdx) != "")
						{
							m_objXMLHandle.CreateElement("LineCssParameter");
							m_objXMLHandle.CreateAttribute("name");
							m_objXMLHandle.SetAttributeValue("stroke-dasharray");
							m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("LineDashArray", Symbol, layerIdx));
						}
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Schreiben der Schraffur", ex.Message, ex.StackTrace, "WriteLineFeatures");
				return false;
			}
		}
		private bool WritePolygonFeatures(object Symbol)
		{
			int i = 0;
            int iSecure = 0;
			try
			{
				if (Symbol is Analize_ArcMap_Symbols.StructSimpleFillSymbol)
				{
					WriteSolidFill(Symbol);
				}
				else if (Symbol is Analize_ArcMap_Symbols.StructMarkerFillSymbol)
				{
					WriteMarkerFill(Symbol);
				}
				else if (Symbol is Analize_ArcMap_Symbols.StructLineFillSymbol)
				{
					//Die Winkel der Schraffuren Original:(Schr鋑/Horizontal,Vertikal)-SLD(Kreuzschraffur schr鋑/Kreuzschraffur Achsparallel)
                    Analize_ArcMap_Symbols.StructLineFillSymbol tempSymbol = (Analize_ArcMap_Symbols.StructLineFillSymbol)Symbol;
                    if (tempSymbol.Angle > 22.5 && tempSymbol.Angle < 67.5)
					{
                        WriteSlopedHatching(tempSymbol);
					}
                    else if (tempSymbol.Angle > 67.5 && tempSymbol.Angle < 112.5)
					{
                        WritePerpendicularHatching(tempSymbol);
					}
                    else if (tempSymbol.Angle > 112.5 && tempSymbol.Angle < 157.5)
					{
                        WriteSlopedHatching(tempSymbol);
					}
                    else if (tempSymbol.Angle > 157.5 && tempSymbol.Angle < 202.5)
					{
                        WritePerpendicularHatching(tempSymbol);
					}
                    else if (tempSymbol.Angle > 202.5 && tempSymbol.Angle < 247.5)
					{
                        WriteSlopedHatching(tempSymbol);
					}
                    else if (tempSymbol.Angle > 247.5 && tempSymbol.Angle < 292.5)
					{
                        WritePerpendicularHatching(tempSymbol);
					}
                    else if (tempSymbol.Angle > 292.5 && tempSymbol.Angle < 337.5)
					{
                        WriteSlopedHatching(tempSymbol);
					}
                    else if (tempSymbol.Angle > 337.5 && tempSymbol.Angle <= 360.0 || tempSymbol.Angle >= 0.0 && tempSymbol.Angle < 22.5)
					{
                        WritePerpendicularHatching(tempSymbol);
					}
				}
				else if (Symbol is Analize_ArcMap_Symbols.StructDotDensityFillSymbol)
				{
					WriteMarkerFill(Symbol); //Ist z.Zt. sowieso nicht m鰃lich, Dichte mit Punktf黮lungen auszugeben
				}
				else if (Symbol is Analize_ArcMap_Symbols.StructPictureFillSymbol)
				{
					//TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO
				}
				else if (Symbol is Analize_ArcMap_Symbols.StructGradientFillSymbol)
				{
					//TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO
				}
				else if (Symbol is Analize_ArcMap_Symbols.StructMultilayerFillSymbol)
				{
					Analize_ArcMap_Symbols.StructMultilayerFillSymbol MFS = new Analize_ArcMap_Symbols.StructMultilayerFillSymbol();
                    MFS = (Analize_ArcMap_Symbols.StructMultilayerFillSymbol)Symbol;
					bool bSwitch; //Wenn mehr als 3 Symbollayer sind, und einer davon ist ein SimpleFill
					bSwitch = false;
					//Hier muss aufgepasst werden: Manche Mapserver k鰊nen nur 2 Symbollayer 黚ereinander abbilden. Deshalb werden derzeit nur 2 Symbollayer gebildet
					if (MFS.LayerCount == 1)
					{
						WritePolygonFeatures(MFS.MultiFillLayers[0]);
					}
					else if (MFS.LayerCount == 2)
					{
						for (i = MFS.LayerCount - 1; i >= 0; i--)
						{
							WritePolygonFeatures(MFS.MultiFillLayers[i]); //hier rekursiver Aufruf
						}
					}
					else if (MFS.LayerCount > 2)
					{
						for (i = MFS.LayerCount - 1; i >= 0; i--)
						{
							if (iSecure <= 1)
							{
								WritePolygonFeatures(MFS.MultiFillLayers[i]);
							}
							iSecure++;
						}
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Schreiben der Schraffur", ex.Message, ex.StackTrace, "WritePolygonFeatures");
				return false;
			}
		}
		private bool WriteSolidFill(object Symbol)
		{
			try
			{
				m_objXMLHandle.CreateElement("PolygonSymbolizer");
				if (GetValueFromSymbolstruct("PolygonColor", Symbol) != "")
				{
					m_objXMLHandle.CreateElement("Fill");
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("fill");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonColor", Symbol));
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("fill-opacity");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonOpacity", Symbol));
				}
				if (GetValueFromSymbolstruct("PolygonBorderColor", Symbol) != "" && GetValueFromSymbolstruct("PolygonBorderWidth", Symbol) != "0")
				{
					m_objXMLHandle.CreateElement("PolygonStroke");
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("stroke");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonBorderColor", Symbol));
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("stroke-width");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonBorderWidth", Symbol));
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("stroke-opacity");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonBorderOpacity", Symbol));
				}
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Schreiben der Fl鋍henf黮lung", ex.Message, ex.StackTrace, "WriteSimpleFill");
				return false;
			}
			
		}
		private bool WriteMarkerFill(object Symbol)
		{
			try
			{
				m_objXMLHandle.CreateElement("PolygonSymbolizer");
				if (GetValueFromSymbolstruct("PointColor", Symbol) != "")
				{
					m_objXMLHandle.CreateElement("Fill");
					m_objXMLHandle.CreateElement("PolygonGraphicFill");
					m_objXMLHandle.CreateElement("PolygonGraphic");
					m_objXMLHandle.CreateElement("PolygonMark");
					m_objXMLHandle.CreateElement("PolygonSize");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PointSize", Symbol));
					m_objXMLHandle.CreateElement("PolygonWellKnownName");
					m_objXMLHandle.SetElementText("circle");
					m_objXMLHandle.CreateElement("PolygonGraphicParamFill");
					m_objXMLHandle.CreateElement("PolygonGraphicCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("fill");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PointColor", Symbol));
					m_objXMLHandle.CreateElement("PolygonGraphicCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("fill-opacity");
					m_objXMLHandle.SetElementText("1.0");
				}
				if (GetValueFromSymbolstruct("PolygonBorderColor", Symbol) != "" && GetValueFromSymbolstruct("PolygonBorderWidth", Symbol) != "0")
				{
					m_objXMLHandle.CreateElement("PolygonStroke");
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("stroke");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonBorderColor", Symbol));
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("stroke-width");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonBorderWidth", Symbol));
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("stroke-opacity");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonBorderOpacity", Symbol));
				}
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Schreiben der Punktf黮lung", ex.Message, ex.StackTrace, "WriteMarkerFill");
				return false;
			}
			
		}
        private bool WriteSlopedHatching(Analize_ArcMap_Symbols.StructLineFillSymbol Symbol)
		{
			double dDummy = 0;
			try
			{
				m_objXMLHandle.CreateElement("PolygonSymbolizer");
				if (Symbol.Color != "")
				{
					m_objXMLHandle.CreateElement("Fill");
					m_objXMLHandle.CreateElement("PolygonGraphicFill");
					m_objXMLHandle.CreateElement("PolygonGraphic");
					//.SetElementText(GetValueFromSymbolstruct("LineWidth", Symbol))
					m_objXMLHandle.CreateElement("PolygonMark");
					m_objXMLHandle.CreateElement("PolygonSize");
					//Schraffurgr鲞e
					dDummy = System.Convert.ToDouble(Symbol.Separation + 5);
					m_objXMLHandle.SetElementText(CommaToPoint(dDummy));
					m_objXMLHandle.CreateElement("PolygonWellKnownName");
					m_objXMLHandle.SetElementText("x"); //Macht die schr鋑e Kreuzschraffur
					m_objXMLHandle.CreateElement("PolygonGraphicParamFill");
					m_objXMLHandle.CreateElement("PolygonGraphicCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("fill");
					//Die Schraffurfarbe
					m_objXMLHandle.SetElementText(System.Convert.ToString(Symbol.Color));
					//.SetElementText(GetValueFromSymbolstruct("LineColor", Symbol))
					m_objXMLHandle.CreateElement("PolygonGraphicCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("fill-opacity");
					m_objXMLHandle.SetElementText("1.0");
				}
				if (GetValueFromSymbolstruct("PolygonBorderColor", Symbol) != "" && GetValueFromSymbolstruct("PolygonBorderWidth", Symbol) != "0")
				{
					m_objXMLHandle.CreateElement("PolygonStroke");
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("stroke");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonBorderColor", Symbol));
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("stroke-width");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonBorderWidth", Symbol));
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("stroke-opacity");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonBorderOpacity", Symbol));
				}
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Schreiben der Schraffur", ex.Message, ex.StackTrace, "WriteSlopedHatching");
				return false;
			}
		}
        private bool WritePerpendicularHatching(Analize_ArcMap_Symbols.StructLineFillSymbol Symbol)
		{
			double dDummy = 0;
			try
			{
				m_objXMLHandle.CreateElement("PolygonSymbolizer");
				if (Symbol.Color != "")
				{
					m_objXMLHandle.CreateElement("Fill");
					m_objXMLHandle.CreateElement("PolygonGraphicFill");
					m_objXMLHandle.CreateElement("PolygonGraphic");
					//.SetElementText(GetValueFromSymbolstruct("LineWidth", Symbol))
					m_objXMLHandle.CreateElement("PolygonMark");
					m_objXMLHandle.CreateElement("PolygonSize");
					//Schraffurgr鲞e
					dDummy = System.Convert.ToDouble(Symbol.Separation + 5);
					m_objXMLHandle.SetElementText(CommaToPoint(dDummy));
					m_objXMLHandle.CreateElement("PolygonWellKnownName");
					m_objXMLHandle.SetElementText("cross");
					m_objXMLHandle.CreateElement("PolygonGraphicParamFill");
					m_objXMLHandle.CreateElement("PolygonGraphicCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("fill");
					//Die Schraffurfarbe
					m_objXMLHandle.SetElementText(System.Convert.ToString(Symbol.Color));
					//.SetElementText(GetValueFromSymbolstruct("LineColor", Symbol))
					m_objXMLHandle.CreateElement("PolygonGraphicCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("fill-opacity");
					m_objXMLHandle.SetElementText("1.0");
				}
				if (GetValueFromSymbolstruct("PolygonBorderColor", Symbol) != "" && GetValueFromSymbolstruct("PolygonBorderWidth", Symbol) != "0")
				{
					m_objXMLHandle.CreateElement("PolygonStroke");
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("stroke");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonBorderColor", Symbol));
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("stroke-width");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonBorderWidth", Symbol));
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("stroke-opacity");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonBorderOpacity", Symbol));
				}
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Schreiben der Punktf黮lung", ex.Message, ex.StackTrace, "WritePerpendicularHatching");
				return false;
			}
		}
#endregion
#region Symbolstructanalysefunktionen
		private string GetValueFromSymbolstruct(string ValueNameOfValueYouWant, object SymbolStructure)
		{
			return GetValueFromSymbolstruct(ValueNameOfValueYouWant, SymbolStructure, 0);
		}
		private string GetValueFromSymbolstruct(string ValueNameOfValueYouWant, object SymbolStructure, int LayerIdx)
		{
			string cReturn = "";
			bool bSwitch;
			bSwitch = false; //(ben鰐igt f黵 Multilayersymbole)der Schalter wird umgelegt, wenn es kein simple.. Symbol gibt. Dann wird der Wert des ersten Symbols genommen
			cReturn = "0"; //Wenn keiner der 黚ergebenen ValueNames passt, wird 0 zur點kgegeben
			try
			{
				if (SymbolStructure is Analize_ArcMap_Symbols.StructSimpleMarkerSymbol)
				{
					cReturn = GetMarkerValue(ValueNameOfValueYouWant, SymbolStructure);
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructCharacterMarkerSymbol)
				{
					cReturn = GetMarkerValue(ValueNameOfValueYouWant, SymbolStructure);
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructPictureMarkerSymbol)
				{
					cReturn = GetMarkerValue(ValueNameOfValueYouWant, SymbolStructure);
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructArrowMarkerSymbol)
				{
					cReturn = GetMarkerValue(ValueNameOfValueYouWant, SymbolStructure);
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructSimpleLineSymbol)
				{
					Analize_ArcMap_Symbols.StructSimpleLineSymbol objTempStruct = new Analize_ArcMap_Symbols.StructSimpleLineSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructSimpleLineSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "LineWidth".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.Width);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineColor".ToUpper())
					{
						cReturn = objTempStruct.Color;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineOpacity".ToUpper())
					{
						double tmpTransparency = objTempStruct.Transparency;
						cReturn = CommaToPoint(tmpTransparency / 255.0);
						//Case "PointRotation".ToUpper
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineDashArray".ToUpper())
					{
						switch (objTempStruct.publicStyle)
						{
							case "esriSLSDash":
								cReturn = "10.0 10.0";
								break;
							case "esriSLSDashDot":
								cReturn = "10.0 10.0 1.0 10.0";
								break;
							case "esriSLSDashDotDot":
								cReturn = "10.0 10.0 1.0 10.0 1.0 10.0";
								break;
							case "esriSLSDot":
								cReturn = "1.0 5.0";
								break;
							default:
								cReturn = "";
								break;
						}
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructCartographicLineSymbol)
				{
					Analize_ArcMap_Symbols.StructCartographicLineSymbol objTempStruct = new Analize_ArcMap_Symbols.StructCartographicLineSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructCartographicLineSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "LineWidth".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.Width);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineColor".ToUpper())
					{
						cReturn = objTempStruct.Color;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineOpacity".ToUpper())
					{
						double tmpTransparency = objTempStruct.Transparency;
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineDashArray".ToUpper())
					{
						int dashIdx = 0;
						double size = 0;
						cReturn = "";
						for (dashIdx = 0; dashIdx <= objTempStruct.DashArray.Count - 1; dashIdx++)
						{
							if (dashIdx > 0)
							{
								cReturn = cReturn + " ";
							}
							size = System.Convert.ToDouble(objTempStruct.DashArray[dashIdx]);
							cReturn = cReturn + CommaToPoint(size);
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "")
					{
					}
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructHashLineSymbol)
				{
					Analize_ArcMap_Symbols.StructHashLineSymbol objTempStruct = new Analize_ArcMap_Symbols.StructHashLineSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructHashLineSymbol)SymbolStructure;
					if ((ValueNameOfValueYouWant.ToUpper() == "LineWidth".ToUpper()) || (ValueNameOfValueYouWant.ToUpper() == "LineDashArray".ToUpper()))
					{
						switch (objTempStruct.kindOfLineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructCartographicLineSymbol:
								cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.HashSymbol_CartographicLine);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.HashSymbol_MarkerLine);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.HashSymbol_MultiLayerLines, LayerIdx);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.HashSymbol_PictureLine);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.HashSymbol_SimpleLine);
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineColor".ToUpper())
					{
						cReturn = objTempStruct.Color;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineOpacity".ToUpper())
					{
						double tmpTransparency = objTempStruct.Transparency;
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "")
					{
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructMarkerLineSymbol)
				{
					Analize_ArcMap_Symbols.StructMarkerLineSymbol objTempStruct = new Analize_ArcMap_Symbols.StructMarkerLineSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructMarkerLineSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "LineWidth".ToUpper())
					{
						InfoMsg("Abfrage von Linienbreite der Markerlines ist im Augenblick nicht implementiert", "GetValueFromSymbolstruct");
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineColor".ToUpper())
					{
						cReturn = objTempStruct.Color;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineOpacity".ToUpper())
					{
						double tmpTransparency = objTempStruct.Transparency;
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "")
					{
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructPictureLineSymbol)
				{
					Analize_ArcMap_Symbols.StructPictureLineSymbol objTempStruct = new Analize_ArcMap_Symbols.StructPictureLineSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructPictureLineSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "LineWidth".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.Width);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineColor".ToUpper())
					{
						cReturn = objTempStruct.BackgroundColor;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineOpacity".ToUpper())
					{
						double tmpTransparency = objTempStruct.BackgroundTransparency;
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "")
					{
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructSimpleFillSymbol)
				{
					Analize_ArcMap_Symbols.StructSimpleFillSymbol objTempStruct = new Analize_ArcMap_Symbols.StructSimpleFillSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructSimpleFillSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "PolygonColor".ToUpper())
					{
						cReturn = objTempStruct.Color;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonOpacity".ToUpper())
					{
						double tmpTransparency = objTempStruct.Transparency;
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderWidth".ToUpper())
					{
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_HashLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_MarkerLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineWidth", objTempStruct.Outline_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_PictureLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_SimpleLine.Width);
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderColor".ToUpper())
					{
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = objTempStruct.Outline_HashLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = objTempStruct.Outline_MarkerLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineColor", objTempStruct.Outline_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = objTempStruct.Outline_PictureLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = objTempStruct.Outline_SimpleLine.Color;
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderOpacity".ToUpper())
					{
						double tmpTransparency = 255.0;
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								tmpTransparency = objTempStruct.Outline_HashLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								tmpTransparency = objTempStruct.Outline_MarkerLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								tmpTransparency = System.Convert.ToDouble(255 * double.Parse(GetValueFromSymbolstruct("LineOpacity", objTempStruct.Outline_MultiLayerLines)));
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								tmpTransparency = objTempStruct.Outline_PictureLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								tmpTransparency = objTempStruct.Outline_SimpleLine.Transparency;
								break;
						}
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructMarkerFillSymbol)
				{
					Analize_ArcMap_Symbols.StructMarkerFillSymbol objTempStruct = new Analize_ArcMap_Symbols.StructMarkerFillSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructMarkerFillSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "PolygonColor".ToUpper())
					{
						cReturn = objTempStruct.Color;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonOpacity".ToUpper())
					{
						double tmpTransparency = objTempStruct.Transparency;
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderWidth".ToUpper())
					{
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_HashLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_MarkerLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineWidth", objTempStruct.Outline_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_PictureLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_SimpleLine.Width);
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderColor".ToUpper())
					{
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = objTempStruct.Outline_HashLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = objTempStruct.Outline_MarkerLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineColor", objTempStruct.Outline_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = objTempStruct.Outline_PictureLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = objTempStruct.Outline_SimpleLine.Color;
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderOpacity".ToUpper())
					{
						double tmpTransparency = 255.0;
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								tmpTransparency = objTempStruct.Outline_HashLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								tmpTransparency = objTempStruct.Outline_MarkerLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								tmpTransparency = System.Convert.ToDouble(255 * double.Parse(GetValueFromSymbolstruct("LineOpacity", objTempStruct.Outline_MultiLayerLines)));
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								tmpTransparency = objTempStruct.Outline_PictureLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								tmpTransparency = objTempStruct.Outline_SimpleLine.Transparency;
								break;
						}
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointSize".ToUpper())
					{
						switch (objTempStruct.kindOfMarkerStruct)
						{
							case Analize_ArcMap_Symbols.MarkerStructs.StructArrowMarkerSymbol:
								cReturn = CommaToPoint(objTempStruct.MarkerSymbol_ArrowMarker.Size);
								break;
							case Analize_ArcMap_Symbols.MarkerStructs.StructCharacterMarkerSymbol:
								cReturn = CommaToPoint(objTempStruct.MarkerSymbol_CharacterMarker.Size);
								break;
							case Analize_ArcMap_Symbols.MarkerStructs.StructMultilayerMarkerSymbol:
								cReturn = GetValueFromSymbolstruct("PointSize", objTempStruct.MarkerSymbol_MultilayerMarker);
								break;
							case Analize_ArcMap_Symbols.MarkerStructs.StructPictureMarkerSymbol:
								cReturn = CommaToPoint(objTempStruct.MarkerSymbol_PictureMarker.Size);
								break;
							case Analize_ArcMap_Symbols.MarkerStructs.StructSimpleMarkerSymbol:
								cReturn = CommaToPoint(objTempStruct.MarkerSymbol_SimpleMarker.Size);
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointColor".ToUpper())
					{
						switch (objTempStruct.kindOfMarkerStruct)
						{
							case Analize_ArcMap_Symbols.MarkerStructs.StructArrowMarkerSymbol:
								cReturn = objTempStruct.MarkerSymbol_ArrowMarker.Color;
								break;
							case Analize_ArcMap_Symbols.MarkerStructs.StructCharacterMarkerSymbol:
								cReturn = objTempStruct.MarkerSymbol_CharacterMarker.Color;
								break;
							case Analize_ArcMap_Symbols.MarkerStructs.StructMultilayerMarkerSymbol:
								cReturn = GetValueFromSymbolstruct("PointColor", objTempStruct.MarkerSymbol_MultilayerMarker);
								break;
							case Analize_ArcMap_Symbols.MarkerStructs.StructPictureMarkerSymbol:
								cReturn = objTempStruct.MarkerSymbol_PictureMarker.Color;
								break;
							case Analize_ArcMap_Symbols.MarkerStructs.StructSimpleMarkerSymbol:
								cReturn = objTempStruct.MarkerSymbol_SimpleMarker.Color;
								break;
						}
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructLineFillSymbol)
				{
					Analize_ArcMap_Symbols.StructLineFillSymbol objTempStruct = new Analize_ArcMap_Symbols.StructLineFillSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructLineFillSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "PolygonColor".ToUpper())
					{
						cReturn = objTempStruct.Color;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonOpacity".ToUpper())
					{
						double tmpTransparency = objTempStruct.Transparency;
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderWidth".ToUpper())
					{
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_HashLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_MarkerLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineWidth", objTempStruct.Outline_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_PictureLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_SimpleLine.Width);
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderColor".ToUpper())
					{
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = objTempStruct.Outline_HashLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = objTempStruct.Outline_MarkerLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineColor", objTempStruct.Outline_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = objTempStruct.Outline_PictureLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = objTempStruct.Outline_SimpleLine.Color;
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderOpacity".ToUpper())
					{
						double tmpTransparency = 255.0;
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								tmpTransparency = objTempStruct.Outline_HashLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								tmpTransparency = objTempStruct.Outline_MarkerLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								tmpTransparency = System.Convert.ToDouble(255 * double.Parse(GetValueFromSymbolstruct("LineOpacity", objTempStruct.Outline_MultiLayerLines)));
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								tmpTransparency = objTempStruct.Outline_PictureLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								tmpTransparency = objTempStruct.Outline_SimpleLine.Transparency;
								break;
						}
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineWidth".ToUpper())
					{
						switch (objTempStruct.kindOfLineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructCartographicLineSymbol:
								cReturn = CommaToPoint(objTempStruct.LineSymbol_CartographicLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = CommaToPoint(objTempStruct.LineSymbol_HashLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = CommaToPoint(objTempStruct.LineSymbol_MarkerLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineWidth", objTempStruct.LineSymbol_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = CommaToPoint(objTempStruct.LineSymbol_PictureLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = CommaToPoint(objTempStruct.LineSymbol_SimpleLine.Width);
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineColor".ToUpper())
					{
						switch (objTempStruct.kindOfLineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructCartographicLineSymbol:
								cReturn = objTempStruct.LineSymbol_CartographicLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = objTempStruct.LineSymbol_HashLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = objTempStruct.LineSymbol_MarkerLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineColor", objTempStruct.LineSymbol_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = objTempStruct.LineSymbol_PictureLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = objTempStruct.LineSymbol_SimpleLine.Color;
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineOpacity".ToUpper())
					{
						double tmpTransparency = 255.0;
						switch (objTempStruct.kindOfLineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructCartographicLineSymbol:
								tmpTransparency = objTempStruct.LineSymbol_CartographicLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								tmpTransparency = objTempStruct.LineSymbol_HashLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								tmpTransparency = objTempStruct.LineSymbol_MarkerLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								tmpTransparency = System.Convert.ToDouble(255 * double.Parse(GetValueFromSymbolstruct("LineOpacity", objTempStruct.LineSymbol_MultiLayerLines)));
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								tmpTransparency = objTempStruct.LineSymbol_PictureLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								tmpTransparency = objTempStruct.LineSymbol_SimpleLine.Transparency;
								break;
						}
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructDotDensityFillSymbol)
				{
					Analize_ArcMap_Symbols.StructDotDensityFillSymbol objTempStruct = new Analize_ArcMap_Symbols.StructDotDensityFillSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructDotDensityFillSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "PolygonColor".ToUpper())
					{
						cReturn = objTempStruct.BackgroundColor;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonOpacity".ToUpper())
					{
						double tmpTransparency = objTempStruct.BackgroundTransparency;
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderWidth".ToUpper())
					{
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_HashLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_MarkerLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineWidth", objTempStruct.Outline_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_PictureLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_SimpleLine.Width);
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderColor".ToUpper())
					{
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = objTempStruct.Outline_HashLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = objTempStruct.Outline_MarkerLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineColor", objTempStruct.Outline_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = objTempStruct.Outline_PictureLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = objTempStruct.Outline_SimpleLine.Color;
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderOpacity".ToUpper())
					{
						double tmpTransparency = 255.0;
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								tmpTransparency = objTempStruct.Outline_HashLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								tmpTransparency = objTempStruct.Outline_MarkerLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								tmpTransparency = System.Convert.ToDouble(255 * double.Parse(GetValueFromSymbolstruct("LineOpacity", objTempStruct.Outline_MultiLayerLines)));
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								tmpTransparency = objTempStruct.Outline_PictureLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								tmpTransparency = objTempStruct.Outline_SimpleLine.Transparency;
								break;
						}
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointSize".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.DotSize);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointColor".ToUpper())
					{
						cReturn = objTempStruct.Color;
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructPictureFillSymbol)
				{
					Analize_ArcMap_Symbols.StructPictureFillSymbol objTempStruct = new Analize_ArcMap_Symbols.StructPictureFillSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructPictureFillSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "PolygonColor".ToUpper())
					{
						cReturn = objTempStruct.BackgroundColor;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonOpacity".ToUpper())
					{
						double tmpTransparency = objTempStruct.BackgroundTransparency;
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderWidth".ToUpper())
					{
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_HashLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_MarkerLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineWidth", objTempStruct.Outline_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_PictureLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_SimpleLine.Width);
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderColor".ToUpper())
					{
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = objTempStruct.Outline_HashLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = objTempStruct.Outline_MarkerLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineColor", objTempStruct.Outline_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = objTempStruct.Outline_PictureLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = objTempStruct.Outline_SimpleLine.Color;
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderOpacity".ToUpper())
					{
						double tmpTransparency = 255.0;
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								tmpTransparency = objTempStruct.Outline_HashLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								tmpTransparency = objTempStruct.Outline_MarkerLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								tmpTransparency = System.Convert.ToDouble(255 * double.Parse(GetValueFromSymbolstruct("LineOpacity", objTempStruct.Outline_MultiLayerLines)));
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								tmpTransparency = objTempStruct.Outline_PictureLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								tmpTransparency = objTempStruct.Outline_SimpleLine.Transparency;
								break;
						}
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructGradientFillSymbol)
				{
					Analize_ArcMap_Symbols.StructGradientFillSymbol objTempStruct = new Analize_ArcMap_Symbols.StructGradientFillSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructGradientFillSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "PolygonColor".ToUpper())
					{
						cReturn = objTempStruct.Color;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonOpacity".ToUpper())
					{
						double tmpTransparency = objTempStruct.Transparency;
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderWidth".ToUpper())
					{
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_HashLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_MarkerLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineWidth", objTempStruct.Outline_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_PictureLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_SimpleLine.Width);
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderColor".ToUpper())
					{
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = objTempStruct.Outline_HashLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = objTempStruct.Outline_MarkerLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineColor", objTempStruct.Outline_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = objTempStruct.Outline_PictureLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = objTempStruct.Outline_SimpleLine.Color;
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderOpacity".ToUpper())
					{
						double tmpTransparency = 255.0;
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								tmpTransparency = objTempStruct.Outline_HashLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								tmpTransparency = objTempStruct.Outline_MarkerLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								tmpTransparency = System.Convert.ToDouble(255 * double.Parse(GetValueFromSymbolstruct("LineOpacity", objTempStruct.Outline_MultiLayerLines)));
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								tmpTransparency = objTempStruct.Outline_PictureLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								tmpTransparency = objTempStruct.Outline_SimpleLine.Transparency;
								break;
						}
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructBarChartSymbol)
				{
					Analize_ArcMap_Symbols.StructBarChartSymbol objTempStruct;
                    objTempStruct = (Analize_ArcMap_Symbols.StructBarChartSymbol)SymbolStructure;
					switch (ValueNameOfValueYouWant.ToUpper())
					{
						case "":
							break;
							
						//case "":
							//break;
							
						//case "":
							//break;
							
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructPieChartSymbol)
				{
					Analize_ArcMap_Symbols.StructPieChartSymbol objTempStruct;
                    objTempStruct = (Analize_ArcMap_Symbols.StructPieChartSymbol)SymbolStructure;
					switch (ValueNameOfValueYouWant.ToUpper())
					{
						case "":
							break;
							
						//case "":
							//break;
							
						//case "":
							//break;
							
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructStackedChartSymbol)
				{
					Analize_ArcMap_Symbols.StructStackedChartSymbol objTempStruct;
                    objTempStruct = (Analize_ArcMap_Symbols.StructStackedChartSymbol)SymbolStructure;
					switch (ValueNameOfValueYouWant.ToUpper())
					{
						case "":
							break;
							
						//case "":
                            //break;
							
						//case "":
                            //break;
							
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructTextSymbol)
				{
					Analize_ArcMap_Symbols.StructTextSymbol objTempStruct = new Analize_ArcMap_Symbols.StructTextSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructTextSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "TextColor".ToUpper())
					{
						cReturn = objTempStruct.Color;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "TextFont".ToUpper())
					{
						cReturn = objTempStruct.Font;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "TextFontAlt".ToUpper())
					{
						switch (objTempStruct.Font.ToUpper())
						{
							case "ARIAL":
							case "ARIAL BLACK":
							case "HELVETICA":
							case "LUCIDA SANS UNICODE":
							case "MICROSOFT SANS SERIF":
							case "TAHOMA":
							case "VERDANA":
								cReturn = "Sans-Serif";
								break;
							case "COURIER":
							case "COURIER NEW":
							case "LUCIDA CONSOLE":
								cReturn = "Monospaced";
								break;
							case "PALATINO LINOTYPE":
							case "TIMES":
							case "TIMES NEW ROMAN":
								cReturn = "Serif";
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "TextFontSize".ToUpper())
					{
						cReturn = System.Convert.ToString(objTempStruct.Size);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "TextFontStyle".ToUpper())
					{
						cReturn = objTempStruct.Style;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "TextFontWeight".ToUpper())
					{
						cReturn = objTempStruct.Weight;
					}
					return cReturn;
					//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
					//Die Multilayer-Symbolstructs
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructMultilayerMarkerSymbol)
				{
					Analize_ArcMap_Symbols.StructMultilayerMarkerSymbol objTempStruct = new Analize_ArcMap_Symbols.StructMultilayerMarkerSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructMultilayerMarkerSymbol)SymbolStructure;
					cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.MultiMarkerLayers[LayerIdx]);
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructMultilayerLineSymbol)
				{
					Analize_ArcMap_Symbols.StructMultilayerLineSymbol objTempStruct = new Analize_ArcMap_Symbols.StructMultilayerLineSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructMultilayerLineSymbol)SymbolStructure;
					short i = 0;
					if (objTempStruct.LayerCount > 1)
					{
						for (i = 0; i <= objTempStruct.LayerCount - 1; i++)
						{
							if (objTempStruct.MultiLineLayers[i] is Analize_ArcMap_Symbols.StructSimpleLineSymbol)
							{
								Analize_ArcMap_Symbols.StructSimpleLineSymbol SLFS = new Analize_ArcMap_Symbols.StructSimpleLineSymbol();
                                SLFS = (Analize_ArcMap_Symbols.StructSimpleLineSymbol)objTempStruct.MultiLineLayers[i];
								cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, SLFS);
								bSwitch = true;
							}
						}
						if (bSwitch == false)
						{
							cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.MultiLineLayers[LayerIdx]);
						}
					}
					else
					{
						cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.MultiLineLayers[LayerIdx]);
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructMultilayerFillSymbol)
				{
					Analize_ArcMap_Symbols.StructMultilayerFillSymbol objTempStruct = new Analize_ArcMap_Symbols.StructMultilayerFillSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructMultilayerFillSymbol)SymbolStructure;
					short i = 0;
					if (objTempStruct.LayerCount > 1)
					{
						for (i = 0; i <= objTempStruct.LayerCount - 1; i++)
						{
							if (((((ValueNameOfValueYouWant.ToUpper() == "PolygonColor".ToUpper()) || (ValueNameOfValueYouWant.ToUpper() == "PolygonOpacity".ToUpper())) || (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderWidth".ToUpper())) || (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderColor".ToUpper())) || (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderOpacity".ToUpper()))
							{
								if (objTempStruct.MultiFillLayers[i] is Analize_ArcMap_Symbols.StructSimpleFillSymbol)
								{
									Analize_ArcMap_Symbols.StructSimpleFillSymbol SSFS = new Analize_ArcMap_Symbols.StructSimpleFillSymbol();
                                    SSFS = (Analize_ArcMap_Symbols.StructSimpleFillSymbol)objTempStruct.MultiFillLayers[i];
									cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, SSFS);
									bSwitch = true;
								}
							}
							else if ((ValueNameOfValueYouWant.ToUpper() == "PointColor".ToUpper()) || (ValueNameOfValueYouWant.ToUpper() == "PointSize".ToUpper()))
							{
								if (objTempStruct.MultiFillLayers[i] is Analize_ArcMap_Symbols.StructMarkerFillSymbol)
								{
									Analize_ArcMap_Symbols.StructMarkerFillSymbol SMFS = new Analize_ArcMap_Symbols.StructMarkerFillSymbol();
                                    SMFS = (Analize_ArcMap_Symbols.StructMarkerFillSymbol)objTempStruct.MultiFillLayers[i];
									cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, SMFS);
									bSwitch = true;
								}
							}
							else if (((ValueNameOfValueYouWant.ToUpper() == "LineWidth".ToUpper()) || (ValueNameOfValueYouWant.ToUpper() == "LineColor".ToUpper())) || (ValueNameOfValueYouWant.ToUpper() == "LineOpacity".ToUpper()))
							{
								if (objTempStruct.MultiFillLayers[i] is Analize_ArcMap_Symbols.StructLineFillSymbol)
								{
									Analize_ArcMap_Symbols.StructLineFillSymbol SLFS = new Analize_ArcMap_Symbols.StructLineFillSymbol();
                                    SLFS = (Analize_ArcMap_Symbols.StructLineFillSymbol)objTempStruct.MultiFillLayers[i];
									cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, SLFS);
									bSwitch = true;
								}
							}
						}
						if (bSwitch == false)
						{
							cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.MultiFillLayers[LayerIdx]);
						}
					}
					else
					{
						cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.MultiFillLayers[LayerIdx]);
					}
					return cReturn;
				}
				return cReturn;
			}
			catch (Exception ex)
			{
				ErrorMsg("Konnte den Wert aus der SymbolStruct nicht auswerten.", ex.Message, ex.StackTrace, "GetValueFromSymbolstruct");
                return cReturn;
			}
		}
		private string GetMarkerValue(string ValueNameOfValueYouWant, object SymbolStructure)
		{
			string cReturn = "";
			string cColor = "";
			string cOutlineColor = "";
			bool bSwitch;
			bSwitch = false; 
			cReturn = "0"; 
			try
			{
				if (SymbolStructure is Analize_ArcMap_Symbols.StructSimpleMarkerSymbol)
				{
					Analize_ArcMap_Symbols.StructSimpleMarkerSymbol objTempStruct = new Analize_ArcMap_Symbols.StructSimpleMarkerSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructSimpleMarkerSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "WellKnownName".ToUpper())
					{
						switch (objTempStruct.Style)
						{
							case "esriSMSCircle":
								cReturn = "circle";
								break;
							case "esriSMSSquare":
								cReturn = "square";
								break;
							case "esriSMSCross":
								cReturn = "cross";
								break;
							case "esriSMSX":
								cReturn = "x";
								break;
							case "esriSMSDiamond":
								cReturn = "triangle";
								break;
							default:
								cReturn = "star";
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointColor".ToUpper())
					{
						if (objTempStruct.Filled)
						{
							cReturn = objTempStruct.Color;
						}
						else
						{
							cReturn = "";
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointSize".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.Size);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointRotation".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.Angle);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointOutlineColor".ToUpper())
					{
						if (objTempStruct.Outline)
						{
							cReturn = objTempStruct.OutlineColor;
						}
						else
						{
							cReturn = "";
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointOutlineSize".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.OutlineSize);
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructCharacterMarkerSymbol)
				{
					Analize_ArcMap_Symbols.StructCharacterMarkerSymbol objTempStruct = new Analize_ArcMap_Symbols.StructCharacterMarkerSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructCharacterMarkerSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "WellKnownName".ToUpper())
					{
						cReturn = "circle"; //Default
						switch (objTempStruct.Font.ToUpper())
						{
							case "ESRI DEFAULT MARKER":
								if (((((((((((((((objTempStruct.CharacterIndex == 33) || (objTempStruct.CharacterIndex == 40)) || (objTempStruct.CharacterIndex == 46)) || (objTempStruct.CharacterIndex == 53)) || (objTempStruct.CharacterIndex >= 60 && objTempStruct.CharacterIndex <= 67)) || (objTempStruct.CharacterIndex == 72)) || (objTempStruct.CharacterIndex >= 79 && objTempStruct.CharacterIndex <= 82)) || (objTempStruct.CharacterIndex >= 90 && objTempStruct.CharacterIndex <= 93)) || (objTempStruct.CharacterIndex == 171)) || (objTempStruct.CharacterIndex == 172)) || (objTempStruct.CharacterIndex == 183)) || (objTempStruct.CharacterIndex == 196)) || (objTempStruct.CharacterIndex == 199)) || (objTempStruct.CharacterIndex == 200)) || (objTempStruct.CharacterIndex == 8729))
								{
									cReturn = "circle";
								}
								else if ((((((((((((((((objTempStruct.CharacterIndex == 34) || (objTempStruct.CharacterIndex == 41)) || (objTempStruct.CharacterIndex == 47)) || (objTempStruct.CharacterIndex == 54)) || (objTempStruct.CharacterIndex == 74)) || (objTempStruct.CharacterIndex == 83)) || (objTempStruct.CharacterIndex == 84)) || (objTempStruct.CharacterIndex == 104)) || (objTempStruct.CharacterIndex == 174)) || (objTempStruct.CharacterIndex == 175)) || (objTempStruct.CharacterIndex == 179)) || (objTempStruct.CharacterIndex == 190)) || (objTempStruct.CharacterIndex == 192)) || (objTempStruct.CharacterIndex == 194)) || (objTempStruct.CharacterIndex == 198)) || (objTempStruct.CharacterIndex == 201))
								{
									cReturn = "square";
								}
								else if ((((((((objTempStruct.CharacterIndex == 35) || (objTempStruct.CharacterIndex == 42)) || (objTempStruct.CharacterIndex == 48)) || (objTempStruct.CharacterIndex == 55)) || (objTempStruct.CharacterIndex == 73)) || (objTempStruct.CharacterIndex == 86)) || (objTempStruct.CharacterIndex == 184)) || (objTempStruct.CharacterIndex == 185))
								{
									cReturn = "triangle";
								}
								else if (objTempStruct.CharacterIndex == 68)
								{
									cReturn = "X";
								}
								else if ((((objTempStruct.CharacterIndex == 69) || (objTempStruct.CharacterIndex == 70)) || (objTempStruct.CharacterIndex == 71)) || (objTempStruct.CharacterIndex >= 203 && objTempStruct.CharacterIndex <= 211))
								{
									cReturn = "cross";
								}
								else if ((((((objTempStruct.CharacterIndex == 94) || (objTempStruct.CharacterIndex == 95)) || (objTempStruct.CharacterIndex == 96)) || (objTempStruct.CharacterIndex == 106)) || (objTempStruct.CharacterIndex == 107)) || (objTempStruct.CharacterIndex == 108))
								{
									cReturn = "star";
								}
								break;
							case "ESRI IGL FONT22":
								if (((((objTempStruct.CharacterIndex >= 65 && objTempStruct.CharacterIndex <= 69) || (objTempStruct.CharacterIndex >= 93 && objTempStruct.CharacterIndex <= 96)) || (objTempStruct.CharacterIndex == 103)) || (objTempStruct.CharacterIndex == 105)) || (objTempStruct.CharacterIndex == 106))
								{
									cReturn = "circle";
								}
								else if ((((objTempStruct.CharacterIndex == 70) || (objTempStruct.CharacterIndex == 71)) || (objTempStruct.CharacterIndex >= 88 && objTempStruct.CharacterIndex <= 92)) || (objTempStruct.CharacterIndex >= 118 && objTempStruct.CharacterIndex <= 121))
								{
									cReturn = "square";
								}
								else if ((((((((objTempStruct.CharacterIndex == 72) || (objTempStruct.CharacterIndex == 73)) || (objTempStruct.CharacterIndex == 75)) || (objTempStruct.CharacterIndex == 81)) || (objTempStruct.CharacterIndex == 85)) || (objTempStruct.CharacterIndex == 86)) || (objTempStruct.CharacterIndex >= 99 && objTempStruct.CharacterIndex <= 102)) || (objTempStruct.CharacterIndex == 104))
								{
									cReturn = "triangle";
								}
								else if (objTempStruct.CharacterIndex >= 114 && objTempStruct.CharacterIndex <= 117)
								{
									cReturn = "X";
								}
								break;
							case "ESRI GEOMETRIC SYMBOLS":
								if ((((((((((((((((((((((objTempStruct.CharacterIndex >= 33 && objTempStruct.CharacterIndex <= 35) || (objTempStruct.CharacterIndex >= 39 && objTempStruct.CharacterIndex <= 41)) || (objTempStruct.CharacterIndex == 47)) || (objTempStruct.CharacterIndex == 48)) || (objTempStruct.CharacterIndex >= 56 && objTempStruct.CharacterIndex <= 58)) || (objTempStruct.CharacterIndex == 65)) || (objTempStruct.CharacterIndex >= 68 && objTempStruct.CharacterIndex <= 71)) || (objTempStruct.CharacterIndex >= 74 && objTempStruct.CharacterIndex <= 77)) || (objTempStruct.CharacterIndex == 82)) || (objTempStruct.CharacterIndex == 83)) || (objTempStruct.CharacterIndex >= 86 && objTempStruct.CharacterIndex <= 89)) || (objTempStruct.CharacterIndex >= 92 && objTempStruct.CharacterIndex <= 95)) || (objTempStruct.CharacterIndex >= 98 && objTempStruct.CharacterIndex <= 101)) || (objTempStruct.CharacterIndex >= 104 && objTempStruct.CharacterIndex <= 107)) || (objTempStruct.CharacterIndex >= 110 && objTempStruct.CharacterIndex <= 113)) || (objTempStruct.CharacterIndex >= 116 && objTempStruct.CharacterIndex <= 125)) || (objTempStruct.CharacterIndex == 161)) || (objTempStruct.CharacterIndex == 171)) || (objTempStruct.CharacterIndex >= 177 && objTempStruct.CharacterIndex <= 186)) || (objTempStruct.CharacterIndex == 244)) || (objTempStruct.CharacterIndex >= 246 && objTempStruct.CharacterIndex <= 249)) || (objTempStruct.CharacterIndex == 8729))
								{
									cReturn = "circle";
								}
								else if (((((((((((((((((((((((objTempStruct.CharacterIndex == 37) || (objTempStruct.CharacterIndex == 42)) || (objTempStruct.CharacterIndex == 43)) || (objTempStruct.CharacterIndex == 50)) || (objTempStruct.CharacterIndex == 55)) || (objTempStruct.CharacterIndex == 67)) || (objTempStruct.CharacterIndex == 73)) || (objTempStruct.CharacterIndex == 79)) || (objTempStruct.CharacterIndex == 85)) || (objTempStruct.CharacterIndex == 91)) || (objTempStruct.CharacterIndex == 97)) || (objTempStruct.CharacterIndex == 103)) || (objTempStruct.CharacterIndex == 109)) || (objTempStruct.CharacterIndex == 115)) || (objTempStruct.CharacterIndex == 170)) || (objTempStruct.CharacterIndex == 172)) || (objTempStruct.CharacterIndex >= 200 && objTempStruct.CharacterIndex <= 205)) || (objTempStruct.CharacterIndex == 208)) || (objTempStruct.CharacterIndex == 209)) || (objTempStruct.CharacterIndex == 210)) || (objTempStruct.CharacterIndex >= 226 && objTempStruct.CharacterIndex <= 241)) || (objTempStruct.CharacterIndex == 243)) || (objTempStruct.CharacterIndex == 250))
								{
									cReturn = "square";
								}
								else if ((((((((((((((((((((objTempStruct.CharacterIndex == 36) || (objTempStruct.CharacterIndex == 46)) || (objTempStruct.CharacterIndex == 49)) || (objTempStruct.CharacterIndex == 66)) || (objTempStruct.CharacterIndex == 72)) || (objTempStruct.CharacterIndex == 78)) || (objTempStruct.CharacterIndex == 84)) || (objTempStruct.CharacterIndex == 90)) || (objTempStruct.CharacterIndex == 96)) || (objTempStruct.CharacterIndex == 102)) || (objTempStruct.CharacterIndex == 108)) || (objTempStruct.CharacterIndex == 114)) || (objTempStruct.CharacterIndex == 162)) || (objTempStruct.CharacterIndex == 168)) || (objTempStruct.CharacterIndex == 169)) || (objTempStruct.CharacterIndex == 175)) || (objTempStruct.CharacterIndex == 176)) || (objTempStruct.CharacterIndex >= 186 && objTempStruct.CharacterIndex <= 190)) || (objTempStruct.CharacterIndex >= 213 && objTempStruct.CharacterIndex <= 220)) || (objTempStruct.CharacterIndex == 245))
								{
									cReturn = "triangle";
								}
								else if (((objTempStruct.CharacterIndex >= 195 && objTempStruct.CharacterIndex <= 199) || (objTempStruct.CharacterIndex == 206)) || (objTempStruct.CharacterIndex == 207))
								{
									cReturn = "X";
								}
								break;
						}
					}
					else if ((ValueNameOfValueYouWant.ToUpper() == "PointColor".ToUpper()) || (ValueNameOfValueYouWant.ToUpper() == "PointOutlineColor".ToUpper()))
					{
						cColor = objTempStruct.Color; //Default
						cOutlineColor = ""; //Default
						switch (objTempStruct.Font.ToUpper())
						{
							case "ESRI DEFAULT MARKER":
								if ((((((((((((((((((((((((((objTempStruct.CharacterIndex >= 33 && objTempStruct.CharacterIndex <= 39) || (objTempStruct.CharacterIndex >= 67 && objTempStruct.CharacterIndex <= 69)) || (objTempStruct.CharacterIndex == 71)) || (objTempStruct.CharacterIndex == 81)) || (objTempStruct.CharacterIndex == 88)) || (objTempStruct.CharacterIndex >= 97 && objTempStruct.CharacterIndex <= 103)) || (objTempStruct.CharacterIndex == 107)) || (objTempStruct.CharacterIndex == 113)) || (objTempStruct.CharacterIndex == 116)) || (objTempStruct.CharacterIndex == 118)) || (objTempStruct.CharacterIndex == 161)) || (objTempStruct.CharacterIndex == 163)) || (objTempStruct.CharacterIndex == 165)) || (objTempStruct.CharacterIndex == 167)) || (objTempStruct.CharacterIndex == 168)) || (objTempStruct.CharacterIndex == 172)) || (objTempStruct.CharacterIndex == 174)) || (objTempStruct.CharacterIndex == 175)) || (objTempStruct.CharacterIndex == 179)) || (objTempStruct.CharacterIndex >= 182 && objTempStruct.CharacterIndex <= 186)) || (objTempStruct.CharacterIndex == 190)) || (objTempStruct.CharacterIndex >= 192 && objTempStruct.CharacterIndex <= 201)) || (objTempStruct.CharacterIndex >= 203 && objTempStruct.CharacterIndex <= 211)) || (objTempStruct.CharacterIndex == 215)) || (objTempStruct.CharacterIndex == 219)) || (objTempStruct.CharacterIndex == 8729))
								{
									cColor = objTempStruct.Color;
									cOutlineColor = "";
								}
								else
								{
									cColor = "";
									cOutlineColor = objTempStruct.Color;
								}
								break;
							case "ESRI IGL FONT22":
								if (((objTempStruct.CharacterIndex >= 72 && objTempStruct.CharacterIndex <= 80) || (objTempStruct.CharacterIndex == 100)) || (objTempStruct.CharacterIndex >= 118 && objTempStruct.CharacterIndex <= 121))
								{
									cColor = objTempStruct.Color;
									cOutlineColor = "";
								}
								else
								{
									cColor = "";
									cOutlineColor = objTempStruct.Color;
								}
								break;
							case "ESRI GEOMETRIC SYMBOLS":
								if (((((((((((objTempStruct.CharacterIndex >= 34 && objTempStruct.CharacterIndex <= 40) || (objTempStruct.CharacterIndex == 120)) || (objTempStruct.CharacterIndex >= 161 && objTempStruct.CharacterIndex <= 167)) || (objTempStruct.CharacterIndex == 187)) || (objTempStruct.CharacterIndex == 188)) || (objTempStruct.CharacterIndex >= 194 && objTempStruct.CharacterIndex <= 200)) || (objTempStruct.CharacterIndex >= 202 && objTempStruct.CharacterIndex <= 215)) || (objTempStruct.CharacterIndex == 217)) || (objTempStruct.CharacterIndex == 218)) || (objTempStruct.CharacterIndex >= 221 && objTempStruct.CharacterIndex <= 229)) || (objTempStruct.CharacterIndex >= 231 && objTempStruct.CharacterIndex <= 249))
								{
									cColor = objTempStruct.Color;
									cOutlineColor = "";
								}
								else
								{
									cColor = "";
									cOutlineColor = objTempStruct.Color;
								}
								break;
						}
						if (ValueNameOfValueYouWant.ToUpper() == "PointColor".ToUpper())
						{
							cReturn = cColor;
						}
						else
						{
							cReturn = cOutlineColor;
						}
						return cReturn;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointSize".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.Size);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointRotation".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.Angle);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointOutlineSize".ToUpper())
					{
						cReturn = "1";
						switch (objTempStruct.Font.ToUpper())
						{
							case "ESRI GEOMETRIC SYMBOLS":
								if (((((((((objTempStruct.CharacterIndex == 85) || (objTempStruct.CharacterIndex == 88)) || (objTempStruct.CharacterIndex == 89)) || (objTempStruct.CharacterIndex == 91)) || (objTempStruct.CharacterIndex == 94)) || (objTempStruct.CharacterIndex == 95)) || (objTempStruct.CharacterIndex == 97)) || (objTempStruct.CharacterIndex == 98)) || (objTempStruct.CharacterIndex == 100))
								{
									cReturn = "2";
								}
								else if ((((((((((objTempStruct.CharacterIndex == 41) || (objTempStruct.CharacterIndex >= 65 && objTempStruct.CharacterIndex <= 83)) || (objTempStruct.CharacterIndex >= 122 && objTempStruct.CharacterIndex <= 125)) || (objTempStruct.CharacterIndex == 168)) || (objTempStruct.CharacterIndex == 169)) || (objTempStruct.CharacterIndex == 188)) || (objTempStruct.CharacterIndex == 189)) || (objTempStruct.CharacterIndex == 216)) || (objTempStruct.CharacterIndex == 230)) || (objTempStruct.CharacterIndex == 250))
								{
									cReturn = "3";
								}
								else if (((objTempStruct.CharacterIndex == 161) || (objTempStruct.CharacterIndex >= 170 && objTempStruct.CharacterIndex <= 178)) || (objTempStruct.CharacterIndex == 186))
								{
									cReturn = "4";
								}
								break;
						}
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructPictureMarkerSymbol)
				{
					Analize_ArcMap_Symbols.StructPictureMarkerSymbol objTempStruct = new Analize_ArcMap_Symbols.StructPictureMarkerSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructPictureMarkerSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "WellKnownName".ToUpper())
					{
						cReturn = "circle"; //TODO
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointColor".ToUpper())
					{
						cReturn = objTempStruct.BackgroundColor;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointSize".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.Size);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointRotation".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.Angle);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointOutlineColor".ToUpper())
					{
						cReturn = ""; //TODO
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointOutlineSize".ToUpper())
					{
						cReturn = "0"; //TODO
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructArrowMarkerSymbol)
				{
					Analize_ArcMap_Symbols.StructArrowMarkerSymbol objTempStruct = new Analize_ArcMap_Symbols.StructArrowMarkerSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructArrowMarkerSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "WellKnownName".ToUpper())
					{
						cReturn = "triangle";
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointColor".ToUpper())
					{
						cReturn = objTempStruct.Color;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointSize".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.Size);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointRotation".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.Angle);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointOutlineColor".ToUpper())
					{
						cReturn = ""; //Never an outline
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointOutlineSize".ToUpper())
					{
						cReturn = "0"; //Never an outline
					}
					return cReturn;
				}
			}
			catch (Exception ex)
			{
				ErrorMsg("Konnte den Wert aus der SymbolStruct nicht auswerten.", ex.Message, ex.StackTrace, "GetValueFromSymbolstruct");
			}
			return cReturn;
		}
#endregion
		
#region Hilfsfunktionen
		private string CommaToPoint(double value)
		{
			string cReturn = "";
			cReturn = value.ToString();
			cReturn = cReturn.Replace(",", ".");
			return cReturn;
		}
		private string CommaToPoint(string value)
		{
			string cReturn = "";
			cReturn = value;
			cReturn = cReturn.Replace(",", ".");
			return cReturn;
		}
		/// <summary>
        /// 错误信息提示框
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exMessage"></param>
        /// <param name="stack"></param>
        /// <param name="functionname"></param>
        /// <returns></returns>
		private object ErrorMsg(string message, string exMessage, string stack, string functionname)
		{
			MessageBox.Show(string.Format("{0}.{1}{2}{3}{4}",message,Environment.NewLine,
                exMessage,Environment.NewLine,stack),string.Format( "{0}" , functionname), MessageBoxButtons.OK, MessageBoxIcon.Error);
			MyTermination();
			return null;
		}
        /// <summary>
        /// 消息提示框
        /// </summary>
        /// <param name="message"></param>
        /// <param name="functionname"></param>
        /// <returns></returns>
		private object InfoMsg(string message, string functionname)
		{
			MessageBox.Show(message, functionname, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return null;
		}
        /// <summary>
        /// 关闭程序
        /// </summary>
        /// <returns></returns>
		public object MyTermination()
		{
			ProjectData.EndApp();
			m_objData.MyTermination();
			return null;
		}
#endregion
	}
	
}
