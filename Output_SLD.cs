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
using System.Collections.Generic;

namespace ArcGIS_SLD_Converter
{
	public class Output_SLD
	{
#region 全局变量
		private Motherform frmMotherForm;
        /// <summary>
        /// 符号分析对象
        /// </summary>
		private Analize_ArcMap_Symbols m_objData;
        /// <summary>
        /// 分析图层信息
        /// </summary>
        private ProjectClass m_strDataSavings;
        /// <summary>
        /// XML处理对象
        /// </summary>
		private XMLHandle m_objXMLHandle;
        /// <summary>
        /// SLD文件名称
        /// </summary>
		private string m_cFilename; 

		private string m_cFile; 

		private string m_cPath; 
        /// <summary>
        /// 是否存在一个SLD文件还是根据图层来进行分别存储SLD
        /// </summary>
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
            //图层分析信息
            m_strDataSavings = m_objData.GetProjectData;
			m_bIncludeLayerNames = frmMotherForm.GetInfoIncludeLayerNames;
            //输出SLD文件
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
            //输出SLD文件
            bSuccess = WriteToSLD();
            frmMotherForm.CHLabelTop(string.Format("开始..."));
            if (bSuccess)
			{
                frmMotherForm.CHLabelBottom(string.Format("成功创建文件..."));
                //如果描述文件存在，则加载设置的XML头文件
                if (frmMotherForm.chkValidate.Checked == true)
				{
                    //验证SLD文件是否可用
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
            try
            {
                m_objXMLHandle = new XMLHandle(FileName, bIncludeLayerNames);
                m_objXMLHandle.CreateNewFile(true, bIncludeLayerNames);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
		}
        /// <summary>
        /// 将分析的符号信息写入SLD
        /// </summary>
        /// <returns></returns>
		public bool WriteToSLD()
		{
			string cLayerName = "";//图层名称
			IList<string> objFieldValues = new List<string>();//字段值列表
			bool bDoOneLayer = false;
			double dummy = 0;
             
			if (m_bSepFiles)
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
				foreach (string key in m_strDataSavings.m_LayerRender.Keys)
                {
                    #region 获取图层名称
                    string strDatasetName = "";//数据集名称
                    IList<ptSymbolClass> objSymbols = new List<ptSymbolClass>(); //符号列表
                    ptLayer pLayer = m_strDataSavings.m_LayerRender[key];
                    strDatasetName = pLayer.m_LayerRender.m_DatasetName;
                    cLayerName = pLayer.m_LayerRender.m_LayerName;
                    objSymbols = pLayer.m_LayerRender.SymbolList;
                    ptRender pRender = pLayer.m_LayerRender;
                    #endregion

                    frmMotherForm.CHLabelBottom(string.Format("正在处理图层【{0}】...", cLayerName));
                    //是否每个图层都要新建一个SLD文件
                    if (!bDoOneLayer)
					{
						CreateSLD(m_cFilename + "_" + cLayerName + ".sld", bool.Parse(m_bIncludeLayerNames));
					}

                    #region 创建基础节点
                    if (Convert.ToBoolean(m_bIncludeLayerNames))
					{
						m_objXMLHandle.CreateElement("NamedLayer");
						m_objXMLHandle.CreateElement("LayerName");
                        m_objXMLHandle.SetElementText(strDatasetName);
						m_objXMLHandle.CreateElement("UserStyle");
						m_objXMLHandle.CreateElement("StyleName");
						m_objXMLHandle.SetElementText("Style1");
						m_objXMLHandle.CreateElement("FeatureTypeStyle");
						m_objXMLHandle.CreateElement("FeatureTypeName");
                        m_objXMLHandle.SetElementText(strDatasetName);
					}
					else
					{
						m_objXMLHandle.CreateElement("FeatureTypeStyle");
						m_objXMLHandle.CreateElement("FeatureTypeName");
                        m_objXMLHandle.SetElementText(strDatasetName);
						m_objXMLHandle.CreateElement("FeatureTypeTitle");
                        m_objXMLHandle.SetElementText(strDatasetName);
					}
                    #endregion 

                    for (int j = 0; j <= objSymbols.Count - 1; j++)
					{

						frmMotherForm.CHLabelSmall("写入符号 " + (j + 1).ToString() + " 中 " + objSymbols.Count.ToString());

                        ptSymbolClass pSymbolClass = objSymbols[j];
                        #region 读取符号基础信息
                        string StrLabel = pSymbolClass.Label;//标题
                        double StrLowerLimit = pSymbolClass.LowerLimit;
                        double StrUpperLimit = pSymbolClass.UpperLimit;
                        objFieldValues = pSymbolClass.Fieldvalues;
                        #endregion

                        #region 唯一值渲染
                        if (pRender is ptUniqueValueRendererClass)
						{
                            //Analize_ArcMap_Symbols.StructUniqueValueRenderer objStructUVR = (Analize_ArcMap_Symbols.StructUniqueValueRenderer)m_strDataSavings.LayerList[i];
                            ptUniqueValueRendererClass pUniqueRender = pRender as ptUniqueValueRendererClass;
							m_objXMLHandle.CreateElement("Rule");
							m_objXMLHandle.CreateElement("RuleName");
                            m_objXMLHandle.SetElementText(StrLabel);
							m_objXMLHandle.CreateElement("Title");
                            m_objXMLHandle.SetElementText(StrLabel);
							m_objXMLHandle.CreateElement("Filter");
                            //设置显示比例尺
                            if (!double.IsNaN(pLayer.m_MaxScale) && !double.IsNaN(pLayer.m_MinScale))
                            {
                                m_objXMLHandle.CreateElement("MinScale");
                                m_objXMLHandle.SetElementText(pLayer.m_MinScale.ToString());
                                m_objXMLHandle.CreateElement("MaxScale");
                                m_objXMLHandle.SetElementText(pLayer.m_MaxScale.ToString());
                            }
                         
                            //多字段多值组合符号
                            if (pUniqueRender.FieldCount > 1) 
							{
								m_objXMLHandle.CreateElement("And");
								for (int l = 0; l <= pUniqueRender.FieldCount - 1; l++) 
								{
									m_objXMLHandle.CreateElement("PropertyIsEqualTo"); 
									m_objXMLHandle.CreateElement("PropertyName");
									m_objXMLHandle.SetElementText(System.Convert.ToString(pUniqueRender.FieldNames[l]));
									m_objXMLHandle.CreateElement("Fieldvalue");
									m_objXMLHandle.SetElementText(System.Convert.ToString(objFieldValues[l]));
								}
							}
                            //单字段多值同一符号
							else if (pUniqueRender.FieldCount == 1)
							{
								if (objFieldValues.Count > 1)
								{
									m_objXMLHandle.CreateElement("Or");
								}
								for (int l = 0; l <= objFieldValues.Count - 1; l++) 
								{
									m_objXMLHandle.CreateElement("PropertyIsEqualTo"); 
									m_objXMLHandle.CreateElement("PropertyName");
									m_objXMLHandle.SetElementText(System.Convert.ToString(pUniqueRender.FieldNames[0]));
									m_objXMLHandle.CreateElement("Fieldvalue");
									m_objXMLHandle.SetElementText(System.Convert.ToString(objFieldValues[l]));
								}
							}

                            if (pSymbolClass is ptMarkerSymbolClass)
                            {
                                WritePointFeatures(pSymbolClass as ptMarkerSymbolClass);
                            }
                            else if (pSymbolClass is ptLineSymbolClass)
                            {
                                WriteLineFeatures(pSymbolClass);
                            }
                            else if(pSymbolClass is ptFillSymbolClass)
                            {
                                WritePolygonFeatures(pSymbolClass);
                            }
						}
                        #endregion

                        #region 分类值渲染方式
                        else if (m_strDataSavings.LayerList[i] is Analize_ArcMap_Symbols.StructClassBreaksRenderer)
						{
							Analize_ArcMap_Symbols.StructClassBreaksRenderer objStructCBR = (Analize_ArcMap_Symbols.StructClassBreaksRenderer)m_strDataSavings.LayerList[i];
							m_objXMLHandle.CreateElement("Rule");
							m_objXMLHandle.CreateElement("RuleName");
                            m_objXMLHandle.SetElementText(StrLabel);
							m_objXMLHandle.CreateElement("Title");
                            m_objXMLHandle.SetElementText(StrLabel);
							m_objXMLHandle.CreateElement("Filter");
							//if (frmMotherForm.chkScale.Checked == true)
							//{
							//	m_objXMLHandle.CreateElement("MinScale");
							//	m_objXMLHandle.SetElementText(frmMotherForm.cboLowScale.Text);
							//	m_objXMLHandle.CreateElement("MaxScale");
							//	m_objXMLHandle.SetElementText(frmMotherForm.cboHighScale.Text);
							//}
							m_objXMLHandle.CreateElement("PropertyIsBetween");
							m_objXMLHandle.CreateElement("PropertyName");
							m_objXMLHandle.SetElementText(objStructCBR.FieldName);
							m_objXMLHandle.CreateElement("LowerBoundary");
							m_objXMLHandle.CreateElement("Fieldvalue");
                            dummy = StrLowerLimit;
							m_objXMLHandle.SetElementText(CommaToPoint(dummy));
							m_objXMLHandle.CreateElement("UpperBoundary");
							m_objXMLHandle.CreateElement("Fieldvalue");
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
						}
                        #endregion

                        #region 简单渲染方式
                        else if (m_strDataSavings.LayerList[i] is Analize_ArcMap_Symbols.StructSimpleRenderer)
						{
							Analize_ArcMap_Symbols.StructSimpleRenderer objStructSR = new Analize_ArcMap_Symbols.StructSimpleRenderer();
                            objStructSR = (Analize_ArcMap_Symbols.StructSimpleRenderer)m_strDataSavings.LayerList[i];
							m_objXMLHandle.CreateElement("Rule");
							m_objXMLHandle.CreateElement("RuleName");
                            m_objXMLHandle.SetElementText(objStructSR.DatasetName);
							m_objXMLHandle.CreateElement("Title");
                            m_objXMLHandle.SetElementText(objStructSR.DatasetName);
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
                        #endregion
                    }
                    if (bDoOneLayer == false)
					{
						m_objXMLHandle.SaveDoc(); 
					}
				}
				if (bDoOneLayer == true)
				{
					m_objXMLHandle.SaveDoc(); 
				}
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("SLD文件写入错误", ex.Message, ex.StackTrace, "WriteToSLD");
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
        /// <summary>
        /// 解析点要素符号
        /// </summary>
        /// <param name="Symbol"></param>
        /// <returns></returns>
		private bool WritePointFeatures(ptMarkerSymbolClass Symbol)
		{
			try
			{
				int layerIdx = 0;
				int maxLayerIdx = 1;
				
				if (Symbol is ptMultilayerMarkerSymbolClass)
				{
                    ptMultilayerMarkerSymbolClass pMultiMarkerSymbol = Symbol as ptMultilayerMarkerSymbolClass;
                    maxLayerIdx = pMultiMarkerSymbol.LayerCount;
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
				ErrorMsg("解析点符号出错", ex.Message, ex.StackTrace, "WritePointFeatures");
				return false;
			}
		}
        /// <summary>
        /// 解析线要素符号
        /// </summary>
        /// <param name="Symbol"></param>
        /// <returns></returns>
		private bool WriteLineFeatures(object Symbol)
		{
			try
			{
				int maxLayerIdx = 1;
				if (Symbol is Analize_ArcMap_Symbols.StructMultilayerLineSymbol)
				{
                    Analize_ArcMap_Symbols.StructMultilayerLineSymbol objTempStruct = (Analize_ArcMap_Symbols.StructMultilayerLineSymbol)Symbol;
					maxLayerIdx = objTempStruct.LayerCount;
				}
				for (int layerIdx = 0; layerIdx <= maxLayerIdx - 1; layerIdx++)
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
				ErrorMsg("解析线符号出错", ex.Message, ex.StackTrace, "WriteLineFeatures");
				return false;
			}
		}
        /// <summary>
        /// 解析面要素图层符号
        /// </summary>
        /// <param name="Symbol"></param>
        /// <returns></returns>
		private bool WritePolygonFeatures(object Symbol)
		{
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
					WriteMarkerFill(Symbol); 
				}
				else if (Symbol is Analize_ArcMap_Symbols.StructPictureFillSymbol)
				{
					
				}
				else if (Symbol is Analize_ArcMap_Symbols.StructGradientFillSymbol)
				{
					
				}
				else if (Symbol is Analize_ArcMap_Symbols.StructMultilayerFillSymbol)
				{
					Analize_ArcMap_Symbols.StructMultilayerFillSymbol MFS = MFS = (Analize_ArcMap_Symbols.StructMultilayerFillSymbol)Symbol;
                    bool bSwitch; 
					bSwitch = false;
					if (MFS.LayerCount == 1)
					{
						WritePolygonFeatures(MFS.MultiFillLayers[0]);
					}
					else if (MFS.LayerCount == 2)
					{
						for (int i = MFS.LayerCount - 1; i >= 0; i--)
						{
							WritePolygonFeatures(MFS.MultiFillLayers[i]); 
						}
					}
					else if (MFS.LayerCount > 2)
					{
						for (int i = MFS.LayerCount - 1; i >= 0; i--)
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
				ErrorMsg("解析面要素图层符号", ex.Message, ex.StackTrace, "WritePolygonFeatures");
				return false;
			}
		}
        /// <summary>
        /// 写入简单面符号
        /// </summary>
        /// <param name="Symbol"></param>
        /// <returns></returns>
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
				ErrorMsg("写入简单面符号到SLD文件中出错", ex.Message, ex.StackTrace, "WriteSimpleFill");
				return false;
			}
			
		}
        /// <summary>
        /// 写入标记面符号
        /// </summary>
        /// <param name="Symbol"></param>
        /// <returns></returns>
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
				ErrorMsg("写入标记面符号到SLD文件出错", ex.Message, ex.StackTrace, "WriteMarkerFill");
				return false;
			}
			
		}
        /// <summary>
        /// 写入斜线标记面符号
        /// </summary>
        /// <param name="Symbol"></param>
        /// <returns></returns>
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
					m_objXMLHandle.CreateElement("PolygonMark");
					m_objXMLHandle.CreateElement("PolygonSize");
					dDummy = Symbol.Separation + 5;
					m_objXMLHandle.SetElementText(CommaToPoint(dDummy));
					m_objXMLHandle.CreateElement("PolygonWellKnownName");
					m_objXMLHandle.SetElementText("x"); 
					m_objXMLHandle.CreateElement("PolygonGraphicParamFill");
					m_objXMLHandle.CreateElement("PolygonGraphicCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("fill");
					m_objXMLHandle.SetElementText(System.Convert.ToString(Symbol.Color));
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
				ErrorMsg("写入线标记面符号到SLD文件出错", ex.Message, ex.StackTrace, "WriteSlopedHatching");
				return false;
			}
		}
        /// <summary>
        /// 垂直线填充符号
        /// </summary>
        /// <param name="Symbol"></param>
        /// <returns></returns>
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
					m_objXMLHandle.CreateElement("PolygonMark");
					m_objXMLHandle.CreateElement("PolygonSize");
					dDummy = System.Convert.ToDouble(Symbol.Separation + 5);
					m_objXMLHandle.SetElementText(CommaToPoint(dDummy));
					m_objXMLHandle.CreateElement("PolygonWellKnownName");
					m_objXMLHandle.SetElementText("cross");
					m_objXMLHandle.CreateElement("PolygonGraphicParamFill");
					m_objXMLHandle.CreateElement("PolygonGraphicCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("fill");
					m_objXMLHandle.SetElementText(System.Convert.ToString(Symbol.Color));
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
				ErrorMsg("写入线填充符号出错", ex.Message, ex.StackTrace, "WritePerpendicularHatching");
				return false;
			}
		}
#endregion

#region 符号转换
		private string GetValueFromSymbolstruct(string ValueNameOfValueYouWant, object SymbolStructure)
		{
			return GetValueFromSymbolstruct(ValueNameOfValueYouWant, SymbolStructure, 0);
		}
        /// <summary>
        /// 从符号类中获取指点名称信息
        /// </summary>
        /// <param name="ValueNameOfValueYouWant"></param>
        /// <param name="SymbolStructure"></param>
        /// <param name="LayerIdx"></param>
        /// <returns></returns>
		private string GetValueFromSymbolstruct(string ValueNameOfValueYouWant, object SymbolStructure, int LayerIdx)
		{
			string cReturn = "0";
			bool bSwitch = false;
			try
			{
                #region  处理点符号
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
                #endregion

                #region 处理线符号
                else if (SymbolStructure is Analize_ArcMap_Symbols.StructSimpleLineSymbol)
				{
                    Analize_ArcMap_Symbols.StructSimpleLineSymbol objTempStruct = (Analize_ArcMap_Symbols.StructSimpleLineSymbol)SymbolStructure;
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
                    Analize_ArcMap_Symbols.StructCartographicLineSymbol  objTempStruct = (Analize_ArcMap_Symbols.StructCartographicLineSymbol)SymbolStructure;
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
						cReturn = "";
						for (int dashIdx = 0; dashIdx <= objTempStruct.DashArray.Count - 1; dashIdx++)
						{
							if (dashIdx > 0)
							{
								cReturn = cReturn + " ";
							}
                            double size = System.Convert.ToDouble(objTempStruct.DashArray[dashIdx]);
							cReturn = cReturn + CommaToPoint(size);
						}
					}
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructHashLineSymbol)
				{
                    Analize_ArcMap_Symbols.StructHashLineSymbol objTempStruct = (Analize_ArcMap_Symbols.StructHashLineSymbol)SymbolStructure;
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
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructMarkerLineSymbol)
				{
                    Analize_ArcMap_Symbols.StructMarkerLineSymbol objTempStruct = (Analize_ArcMap_Symbols.StructMarkerLineSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "LineWidth".ToUpper())
					{
						InfoMsg("无线宽", "GetValueFromSymbolstruct");
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
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructPictureLineSymbol)
				{
                    Analize_ArcMap_Symbols.StructPictureLineSymbol objTempStruct = (Analize_ArcMap_Symbols.StructPictureLineSymbol)SymbolStructure;
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
					return cReturn;
				}
                #endregion

                #region 处理面符号
                else if (SymbolStructure is Analize_ArcMap_Symbols.StructSimpleFillSymbol)
				{
                    Analize_ArcMap_Symbols.StructSimpleFillSymbol objTempStruct = (Analize_ArcMap_Symbols.StructSimpleFillSymbol)SymbolStructure;
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
                    Analize_ArcMap_Symbols.StructMarkerFillSymbol objTempStruct = (Analize_ArcMap_Symbols.StructMarkerFillSymbol)SymbolStructure;
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
                    Analize_ArcMap_Symbols.StructLineFillSymbol objTempStruct = (Analize_ArcMap_Symbols.StructLineFillSymbol)SymbolStructure;
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
                    Analize_ArcMap_Symbols.StructDotDensityFillSymbol objTempStruct = (Analize_ArcMap_Symbols.StructDotDensityFillSymbol)SymbolStructure;
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
                    Analize_ArcMap_Symbols.StructPictureFillSymbol objTempStruct = (Analize_ArcMap_Symbols.StructPictureFillSymbol)SymbolStructure;
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
                    Analize_ArcMap_Symbols.StructGradientFillSymbol objTempStruct = (Analize_ArcMap_Symbols.StructGradientFillSymbol)SymbolStructure;
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
                #endregion

                #region 统计图符号（不支持）
                else if (SymbolStructure is Analize_ArcMap_Symbols.StructBarChartSymbol)
				{
                    Analize_ArcMap_Symbols.StructBarChartSymbol objTempStruct = (Analize_ArcMap_Symbols.StructBarChartSymbol)SymbolStructure;
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
                #endregion

                #region 文本符号
                else if (SymbolStructure is Analize_ArcMap_Symbols.StructTextSymbol)
				{
                    Analize_ArcMap_Symbols.StructTextSymbol objTempStruct = (Analize_ArcMap_Symbols.StructTextSymbol)SymbolStructure;
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
				}
                #endregion

                #region 多图层符号
                else if (SymbolStructure is Analize_ArcMap_Symbols.StructMultilayerMarkerSymbol)
				{
                    Analize_ArcMap_Symbols.StructMultilayerMarkerSymbol objTempStruct = (Analize_ArcMap_Symbols.StructMultilayerMarkerSymbol)SymbolStructure;
					cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.MultiMarkerLayers[LayerIdx]);
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructMultilayerLineSymbol)
				{
                    Analize_ArcMap_Symbols.StructMultilayerLineSymbol objTempStruct = (Analize_ArcMap_Symbols.StructMultilayerLineSymbol)SymbolStructure;
					if (objTempStruct.LayerCount > 1)
					{
						for (int i = 0; i <= objTempStruct.LayerCount - 1; i++)
						{
							if (objTempStruct.MultiLineLayers[i] is Analize_ArcMap_Symbols.StructSimpleLineSymbol)
							{
                                Analize_ArcMap_Symbols.StructSimpleLineSymbol SLFS  = (Analize_ArcMap_Symbols.StructSimpleLineSymbol)objTempStruct.MultiLineLayers[i];
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
                    Analize_ArcMap_Symbols.StructMultilayerFillSymbol objTempStruct = (Analize_ArcMap_Symbols.StructMultilayerFillSymbol)SymbolStructure;
					if (objTempStruct.LayerCount > 1)
					{
						for (int i = 0; i <= objTempStruct.LayerCount - 1; i++)
						{
							if (((((ValueNameOfValueYouWant.ToUpper() == "PolygonColor".ToUpper()) || (ValueNameOfValueYouWant.ToUpper() == "PolygonOpacity".ToUpper())) || (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderWidth".ToUpper())) || (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderColor".ToUpper())) || (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderOpacity".ToUpper()))
							{
								if (objTempStruct.MultiFillLayers[i] is Analize_ArcMap_Symbols.StructSimpleFillSymbol)
								{
                                    Analize_ArcMap_Symbols.StructSimpleFillSymbol SSFS = (Analize_ArcMap_Symbols.StructSimpleFillSymbol)objTempStruct.MultiFillLayers[i];
									cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, SSFS);
									bSwitch = true;
								}
							}
							else if ((ValueNameOfValueYouWant.ToUpper() == "PointColor".ToUpper()) || (ValueNameOfValueYouWant.ToUpper() == "PointSize".ToUpper()))
							{
								if (objTempStruct.MultiFillLayers[i] is Analize_ArcMap_Symbols.StructMarkerFillSymbol)
								{
                                    Analize_ArcMap_Symbols.StructMarkerFillSymbol SMFS = (Analize_ArcMap_Symbols.StructMarkerFillSymbol)objTempStruct.MultiFillLayers[i];
									cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, SMFS);
									bSwitch = true;
								}
							}
							else if (((ValueNameOfValueYouWant.ToUpper() == "LineWidth".ToUpper()) || (ValueNameOfValueYouWant.ToUpper() == "LineColor".ToUpper())) || (ValueNameOfValueYouWant.ToUpper() == "LineOpacity".ToUpper()))
							{
								if (objTempStruct.MultiFillLayers[i] is Analize_ArcMap_Symbols.StructLineFillSymbol)
								{
                                    Analize_ArcMap_Symbols.StructLineFillSymbol SLFS = (Analize_ArcMap_Symbols.StructLineFillSymbol)objTempStruct.MultiFillLayers[i];
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
                #endregion

                return cReturn;
			}
			catch (Exception ex)
			{
				ErrorMsg("获取出错", ex.Message, ex.StackTrace, "GetValueFromSymbolstruct");
                return cReturn;
			}
		}
        /// <summary>
        /// 获取标记值
        /// </summary>
        /// <param name="ValueNameOfValueYouWant"></param>
        /// <param name="SymbolStructure"></param>
        /// <returns></returns>
		private string GetMarkerValue(string ValueNameOfValueYouWant, object SymbolStructure)
		{
			string cReturn = "0";
			string cColor = "";
			string cOutlineColor = "";
            bool bSwitch = false; 
			try
			{
                #region 简单标记符号
                if (SymbolStructure is Analize_ArcMap_Symbols.StructSimpleMarkerSymbol)
				{
                    Analize_ArcMap_Symbols.StructSimpleMarkerSymbol objTempStruct = objTempStruct = (Analize_ArcMap_Symbols.StructSimpleMarkerSymbol)SymbolStructure;
                    #region WellKnownName
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
                    #endregion 

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
						if (objTempStruct.Outline){cReturn = objTempStruct.OutlineColor;}
						else{cReturn = "";}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointOutlineSize".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.OutlineSize);
					}
					return cReturn;
				}
                #endregion

                #region 字符集标记符号
                else if (SymbolStructure is Analize_ArcMap_Symbols.StructCharacterMarkerSymbol)
				{
                    Analize_ArcMap_Symbols.StructCharacterMarkerSymbol objTempStruct = (Analize_ArcMap_Symbols.StructCharacterMarkerSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "WellKnownName".ToUpper())
					{
						cReturn = "circle";
						switch (objTempStruct.Font.ToUpper())
						{
							case "ESRI DEFAULT MARKER":
                                if (CommStaticClass.EsriMarkcircleChartIndex.Contains(objTempStruct.CharacterIndex))
								{
									cReturn = "circle";
								}

                                else if (CommStaticClass.EsriMarksquareChartIndex.Contains(objTempStruct.CharacterIndex))
								{
									cReturn = "square";
								}
                                else if (CommStaticClass.EsriMarktriangleChartIndex.Contains(objTempStruct.CharacterIndex))
								{
									cReturn = "triangle";
								}
								else if (objTempStruct.CharacterIndex == 68)
								{
									cReturn = "X";
								}
                                else if (CommStaticClass.EsriMarkcrossChartIndex.Contains(objTempStruct.CharacterIndex))
								{
									cReturn = "cross";
								}
                                else if (CommStaticClass.EsriMarkstarChartIndex.Contains(objTempStruct.CharacterIndex))
								{
									cReturn = "star";
								}
								break;
							case "ESRI IGL FONT22":
                                if (CommStaticClass.ESRIIGLFONT22circleChartIndex.Contains(objTempStruct.CharacterIndex))
								{
									cReturn = "circle";
								}
                                else if (CommStaticClass.ESRIIGLFONT22squareChartIndex.Contains(objTempStruct.CharacterIndex))
								{
									cReturn = "square";
								}
                                else if (CommStaticClass.ESRIIGLFONT22triangleChartIndex.Contains(objTempStruct.CharacterIndex))
								{
									cReturn = "triangle";
								}
								else if (objTempStruct.CharacterIndex >= 114 && objTempStruct.CharacterIndex <= 117)
								{
									cReturn = "X";
								}
								break;
							case "ESRI GEOMETRIC SYMBOLS":
                                if (CommStaticClass.SYMBOLScircleChartIndex.Contains(objTempStruct.CharacterIndex))
								{
									cReturn = "circle";
								}
                                else if (CommStaticClass.SYMBOLSsquareChartIndex.Contains(objTempStruct.CharacterIndex))
								{
									cReturn = "square";
								}
                                else if (CommStaticClass.SYMBOLStriangleChartIndex.Contains(objTempStruct.CharacterIndex))
								{
									cReturn = "triangle";
								}
                                else if (CommStaticClass.SYMBOLSXChartIndex.Contains(objTempStruct.CharacterIndex))
								{
									cReturn = "X";
								}
								break;
						}
					}
					else if ((ValueNameOfValueYouWant.ToUpper() == "PointColor".ToUpper()) || (ValueNameOfValueYouWant.ToUpper() == "PointOutlineColor".ToUpper()))
					{
						cColor = objTempStruct.Color; 
						cOutlineColor = ""; 
						switch (objTempStruct.Font.ToUpper())
						{
							case "ESRI DEFAULT MARKER":
                                if (CommStaticClass.MarkChartIndex.Contains(objTempStruct.CharacterIndex))
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
                                if (CommStaticClass.FONT22ChartIndex.Contains(objTempStruct.CharacterIndex))
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
                                if (CommStaticClass.SYMBOLSColorChartIndex.Contains(objTempStruct.CharacterIndex))
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
                                if (CommStaticClass.SYMBOLS2ChartIndex.Contains(objTempStruct.CharacterIndex))
								{
									cReturn = "2";
								}
                                else if (CommStaticClass.SYMBOLS3ChartIndex.Contains(objTempStruct.CharacterIndex))
								{
									cReturn = "3";
								}
                                else if (CommStaticClass.SYMBOLS4ChartIndex.Contains(objTempStruct.CharacterIndex))
								{
									cReturn = "4";
								}
								break;
						}
					}
					return cReturn;
				}
                #endregion

                #region 图片标记符号
                else if (SymbolStructure is Analize_ArcMap_Symbols.StructPictureMarkerSymbol)
				{
                    Analize_ArcMap_Symbols.StructPictureMarkerSymbol objTempStruct = (Analize_ArcMap_Symbols.StructPictureMarkerSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "WellKnownName".ToUpper())
					{
						cReturn = "circle"; 
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
						cReturn = ""; 
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointOutlineSize".ToUpper())
					{
						cReturn = "0"; 
					}
					return cReturn;
				}
                #endregion

                #region 箭头标记符号
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
						cReturn = ""; 
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointOutlineSize".ToUpper())
					{
						cReturn = "0";
					}
					return cReturn;
				}
                #endregion
            }
            catch (Exception ex)
			{
				ErrorMsg("获取点符号信息失败", ex.Message, ex.StackTrace, "GetValueFromSymbolstruct");
			}
			return cReturn;
		}
#endregion
		
#region 
        /// <summary>
        /// 数值中去掉逗号
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
		private string CommaToPoint(double value)
		{
			string cReturn = "";
			cReturn = value.ToString();
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
		public void MyTermination()
		{
			ProjectData.EndApp();
			m_objData.MyTermination();
			
		}
#endregion
	}
	
}
