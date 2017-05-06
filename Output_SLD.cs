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
using System.IO;

namespace ArcGIS_SLD_Converter
{
    public class Output_SLD
    {
        #region 全局变量
        private Motherform frmMotherForm;
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
        /// <summary>
        /// 
        /// </summary>
        private string m_bIncludeLayerNames;
        private XmlDocument m_newXmlDoc;
        #endregion

        #region Routinen
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="Mother"></param>
        /// <param name="Analize"></param>
        /// <param name="Filename"></param>
        public Output_SLD(Motherform Mother, ProjectClass projectData, string Filename)
        {
            frmMotherForm = Mother;
            m_cFilename = Filename;

            m_bSepFiles = frmMotherForm.GetInfoSeparateLayers;

            m_cFile = frmMotherForm.GetSLDFile;
            m_cPath = frmMotherForm.GetSLDPath;
            //图层分析信息
            m_strDataSavings = projectData;
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
                m_newXmlDoc=CommXmlHandle.CreateNewFile(FileName, true, bIncludeLayerNames);
                if (m_newXmlDoc == null) return false;
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
            string sldFileName = "";
            if (m_bSepFiles)
            {
                //if (System.IO.Directory.Exists(m_cPath))
                //{
                //    System.IO.Directory.Delete(m_cPath, true);
                //}
                //System.IO.Directory.CreateDirectory(m_cPath);
                bDoOneLayer = false;
            }
            else
            {
                bDoOneLayer = true;
                sldFileName = m_cFilename;
                if (!CreateSLD(sldFileName, bool.Parse(m_bIncludeLayerNames)))
                {
                    ptLogManager.WriteMessage(string.Format("创建SLD文件失败:{0}", sldFileName));
                    return false;
                }
            }
            try
            {
                XmlElement tempElement = null;
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
                        sldFileName = m_cPath + "\\" + cLayerName + ".sld";
                        if (!CreateSLD(sldFileName, bool.Parse(m_bIncludeLayerNames)))
                        {
                            ptLogManager.WriteMessage(string.Format("创建SLD文件失败:{0}", sldFileName));
                            break;
                        }
                    }
                    #region 创建基础节点
                    if (Convert.ToBoolean(m_bIncludeLayerNames))
                    {
                        tempElement= m_newXmlDoc.DocumentElement.AppendChild( CommXmlHandle.CreateElement("NamedLayer", m_newXmlDoc)) as XmlElement;
                        XmlElement pElement = CommXmlHandle.CreateElementAndSetElemnetText("LayerName", m_newXmlDoc, strDatasetName);
                        tempElement=tempElement.AppendChild(pElement) as XmlElement;
                        tempElement = tempElement.AppendChild(CommXmlHandle.CreateElement("UserStyle", m_newXmlDoc)) as XmlElement;
                        tempElement = tempElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("StyleName", m_newXmlDoc, "Style1")) as XmlElement;
                        tempElement = tempElement.AppendChild(CommXmlHandle.CreateElement("FeatureTypeStyle", m_newXmlDoc)) as XmlElement;
                        tempElement = tempElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("FeatureTypeName", m_newXmlDoc, strDatasetName)) as XmlElement;
                    }
                    else
                    {
                        tempElement = m_newXmlDoc.DocumentElement.AppendChild(CommXmlHandle.CreateElement("FeatureTypeStyle", m_newXmlDoc)) as XmlElement;
                        tempElement = tempElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("FeatureTypeName", m_newXmlDoc, strDatasetName)) as XmlElement;
                        tempElement = tempElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("FeatureTypeTitle", m_newXmlDoc, strDatasetName)) as XmlElement;
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
                                m_objXMLHandle.SetElementText(pLayer.m_MaxScale.ToString());
                                m_objXMLHandle.CreateElement("MaxScale");
                                m_objXMLHandle.SetElementText(pLayer.m_MinScale.ToString());
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
                                WriteLineFeatures(pSymbolClass as ptLineSymbolClass);
                            }
                            else if (pSymbolClass is ptFillSymbolClass)
                            {
                                WritePolygonFeatures(pSymbolClass as ptFillSymbolClass);
                            }
                        }
                        #endregion

                        #region 分类值渲染方式
                        else if (pRender is ptClassBreaksRendererCalss)
                        {
                            ptClassBreaksRendererCalss objStructCBR = pRender as ptClassBreaksRendererCalss;
                            m_objXMLHandle.CreateElement("Rule");
                            m_objXMLHandle.CreateElement("RuleName");
                            m_objXMLHandle.SetElementText(StrLabel);
                            m_objXMLHandle.CreateElement("Title");
                            m_objXMLHandle.SetElementText(StrLabel);
                            m_objXMLHandle.CreateElement("Filter");
                            if (!double.IsNaN(pLayer.m_MaxScale) && !double.IsNaN(pLayer.m_MinScale))
                            {
                                m_objXMLHandle.CreateElement("MinScale");
                                m_objXMLHandle.SetElementText(pLayer.m_MaxScale.ToString());
                                m_objXMLHandle.CreateElement("MaxScale");
                                m_objXMLHandle.SetElementText(pLayer.m_MinScale.ToString());
                            }
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
                            if (pSymbolClass is ptMarkerSymbolClass)
                            {
                                WritePointFeatures(pSymbolClass as ptMarkerSymbolClass);
                            }
                            else if (pSymbolClass is ptLineSymbolClass)
                            {
                                WriteLineFeatures(pSymbolClass as ptLineSymbolClass);
                            }
                            else if (pSymbolClass is ptFillSymbolClass)
                            {
                                WritePolygonFeatures(pSymbolClass as ptFillSymbolClass);
                            }
                        }
                        #endregion

                        #region 简单渲染方式
                        else if (pRender is ptSimpleRendererClass)
                        {
                            ptSimpleRendererClass objStructSR = pRender as ptSimpleRendererClass;
                            m_objXMLHandle.CreateElement("Rule");
                            m_objXMLHandle.CreateElement("RuleName");
                            m_objXMLHandle.SetElementText(objStructSR.m_DatasetName);
                            m_objXMLHandle.CreateElement("Title");
                            m_objXMLHandle.SetElementText(objStructSR.m_DatasetName);
                            if (!double.IsNaN(pLayer.m_MaxScale) && !double.IsNaN(pLayer.m_MinScale))
                            {
                                m_objXMLHandle.CreateElement("MinScale");
                                m_objXMLHandle.SetElementText(pLayer.m_MaxScale.ToString());
                                m_objXMLHandle.CreateElement("MaxScale");
                                m_objXMLHandle.SetElementText(pLayer.m_MinScale.ToString());
                            }
                            if (pSymbolClass is ptMarkerSymbolClass)
                            {
                                WritePointFeatures(pSymbolClass as ptMarkerSymbolClass);
                            }
                            else if (pSymbolClass is ptLineSymbolClass)
                            {
                                WriteLineFeatures(pSymbolClass as ptLineSymbolClass);
                            }
                            else if (pSymbolClass is ptFillSymbolClass)
                            {
                                WritePolygonFeatures(pSymbolClass as ptFillSymbolClass);
                            }
                            WriteAnnotation(objStructSR.AnnotationClass);
                        }
                        #endregion
                    }
                    if (bDoOneLayer == false)
                    {
                        CommXmlHandle.SaveDoc(m_newXmlDoc, sldFileName);
                    }
                }
                if (bDoOneLayer == true)
                {
                    CommXmlHandle.SaveDoc(m_newXmlDoc, sldFileName);
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
        private bool WriteAnnotation(AnnotationClass Annotation)
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
                    XmlNode pTempNode = m_objXMLHandle.m_objActiveNode.Clone();
                    if (Symbol is ptMultilayerMarkerSymbolClass)
                    {
                        ptMultilayerMarkerSymbolClass pMMS = Symbol as ptMultilayerMarkerSymbolClass;
                        if (pMMS.MultiMarkerLayers[layerIdx] is ptPictureMarkerSymbolClass)
                        {
                            m_objXMLHandle.CreateElement("PointExternalGraphic");
                            m_objXMLHandle.CreateElement("PointOnlineResource");
                            //先保存图片到SLD文件夹
                            string imagefile = m_cPath + "\\" + Guid.NewGuid() + ".png";
                            ptPictureMarkerSymbolClass picSymbole = pMMS.MultiMarkerLayers[layerIdx] as ptPictureMarkerSymbolClass;
                            Image pimage = IPictureConverter.IPictureToImage(picSymbole.Picture);
                            Graphics g = Graphics.FromImage(pimage);
                            Bitmap pbitmap = new Bitmap(pimage);
                            pbitmap.MakeTransparent(Color.Black);
                            pbitmap.Save(imagefile, System.Drawing.Imaging.ImageFormat.Png);
                            //设置当前就节点属性
                            m_objXMLHandle.CreateAttribute("xmlns:xlink");
                            m_objXMLHandle.SetAttributeValue("http://www.w3.org/1999/xlink");
                            m_objXMLHandle.CreateAttribute("xlink:type");
                            m_objXMLHandle.SetAttributeValue("simple");
                            m_objXMLHandle.CreateAttribute("xlink:href");
                            m_objXMLHandle.SetAttributeValue(string.Format("file:\\{0}", imagefile.Replace('/', '\\')));
                            m_objXMLHandle.CreateElement("PointFormat");
                            m_objXMLHandle.SetElementText("image/png");
                        }
                        else
                        {
                            m_objXMLHandle.CreateElement("Mark");
                            m_objXMLHandle.CreateElement("PointWellKnownName");
                            m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("WellKnownName", Symbol, layerIdx));
                        }
                    }
                    else
                    {
                        m_objXMLHandle.CreateElement("Mark");
                        m_objXMLHandle.CreateElement("PointWellKnownName");
                        m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("WellKnownName", Symbol, layerIdx));
                    }
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
		private bool WriteLineFeatures(ptLineSymbolClass Symbol)
        {
            try
            {
                int maxLayerIdx = 1;
                if (Symbol is ptMultilayerLineSymbolClass)
                {
                    ptMultilayerLineSymbolClass objTempStruct = Symbol as ptMultilayerLineSymbolClass;
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
		private bool WritePolygonFeatures(ptFillSymbolClass Symbol)
        {
            int iSecure = 0;
            try
            {
                if (Symbol is ptSimpleFillSymbolClass)
                {
                    WriteSolidFill(Symbol);
                }
                else if (Symbol is ptMarkerFillSymbolClass)
                {
                    WriteMarkerFill(Symbol);
                }
                else if (Symbol is ptLineFillSymbolClass)
                {
                    ptLineFillSymbolClass tempSymbol = Symbol as ptLineFillSymbolClass;
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
                else if (Symbol is ptDotDensityFillSymbolClass)
                {
                    WriteMarkerFill(Symbol);
                }
                else if (Symbol is ptPictureLineSymbolClass)
                {

                }
                else if (Symbol is ptGradientFillSymbolClass)
                {

                }
                else if (Symbol is ptMultilayerFillSymbolClass)
                {
                    ptMultilayerFillSymbolClass MFS = Symbol as ptMultilayerFillSymbolClass;
                    bool bSwitch;
                    bSwitch = false;
                    if (MFS.LayerCount == 1)
                    {
                        WritePolygonFeatures(MFS.MultiFillSymbol[0]);
                    }
                    else if (MFS.LayerCount == 2)
                    {
                        for (int i = MFS.LayerCount - 1; i >= 0; i--)
                        {
                            WritePolygonFeatures(MFS.MultiFillSymbol[i]);
                        }
                    }
                    else if (MFS.LayerCount > 2)
                    {
                        for (int i = MFS.LayerCount - 1; i >= 0; i--)
                        {
                            if (iSecure <= 1)
                            {
                                WritePolygonFeatures(MFS.MultiFillSymbol[i]);
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
		private bool WriteSolidFill(ptSymbolClass Symbol)
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
		private bool WriteMarkerFill(ptSymbolClass Symbol)
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
        private bool WriteSlopedHatching(ptLineFillSymbolClass Symbol)
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
        private bool WritePerpendicularHatching(ptLineFillSymbolClass Symbol)
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
        private string GetValueFromSymbolstruct(string ValueNameOfValueYouWant, ptSymbolClass SymbolStructure)
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
		private string GetValueFromSymbolstruct(string ValueNameOfValueYouWant, ptSymbolClass SymbolStructure, int LayerIdx)
        {
            string cReturn = "0";
            bool bSwitch = false;
            try
            {

                #region 多图层符号
                if (SymbolStructure is ptMultilayerMarkerSymbolClass)
                {
                    ptMultilayerMarkerSymbolClass objTempStruct = SymbolStructure as ptMultilayerMarkerSymbolClass;
                    cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.MultiMarkerLayers[LayerIdx]);
                    return cReturn;
                }
                else if (SymbolStructure is ptMultilayerLineSymbolClass)
                {
                    ptMultilayerLineSymbolClass objTempStruct = SymbolStructure as ptMultilayerLineSymbolClass;
                    if (objTempStruct.LayerCount > 1)
                    {
                        for (int i = 0; i <= objTempStruct.LayerCount - 1; i++)
                        {
                            if (objTempStruct.MultiLineSymbol[i] is ptSimpleLineSymbolClass)
                            {
                                ptSimpleLineSymbolClass SLFS = objTempStruct.MultiLineSymbol[i] as ptSimpleLineSymbolClass;
                                cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, SLFS);
                                bSwitch = true;
                            }
                        }
                        if (bSwitch == false)
                        {
                            cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.MultiLineSymbol[LayerIdx]);
                        }
                    }
                    else
                    {
                        cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.MultiLineSymbol[LayerIdx]);
                    }
                    return cReturn;
                }
                else if (SymbolStructure is ptMultilayerFillSymbolClass)
                {
                    ptMultilayerFillSymbolClass objTempStruct = SymbolStructure as ptMultilayerFillSymbolClass;
                    if (objTempStruct.LayerCount > 1)
                    {
                        for (int i = 0; i <= objTempStruct.LayerCount - 1; i++)
                        {
                            if (((((ValueNameOfValueYouWant.ToUpper() == "PolygonColor".ToUpper()) || (ValueNameOfValueYouWant.ToUpper() == "PolygonOpacity".ToUpper())) || (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderWidth".ToUpper())) || (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderColor".ToUpper())) || (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderOpacity".ToUpper()))
                            {
                                if (objTempStruct.MultiFillSymbol[i] is ptSimpleFillSymbolClass)
                                {
                                    ptSimpleFillSymbolClass SSFS = objTempStruct.MultiFillSymbol[i] as ptSimpleFillSymbolClass;
                                    cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, SSFS);
                                    bSwitch = true;
                                }
                            }
                            else if ((ValueNameOfValueYouWant.ToUpper() == "PointColor".ToUpper()) || (ValueNameOfValueYouWant.ToUpper() == "PointSize".ToUpper()))
                            {
                                if (objTempStruct.MultiFillSymbol[i] is ptMarkerFillSymbolClass)
                                {
                                    ptMarkerFillSymbolClass SMFS = objTempStruct.MultiFillSymbol[i] as ptMarkerFillSymbolClass;
                                    cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, SMFS);
                                    bSwitch = true;
                                }
                            }
                            else if (((ValueNameOfValueYouWant.ToUpper() == "LineWidth".ToUpper()) || (ValueNameOfValueYouWant.ToUpper() == "LineColor".ToUpper())) || (ValueNameOfValueYouWant.ToUpper() == "LineOpacity".ToUpper()))
                            {
                                if (objTempStruct.MultiFillSymbol[i] is ptLineFillSymbolClass)
                                {
                                    ptLineFillSymbolClass SLFS = objTempStruct.MultiFillSymbol[i] as ptLineFillSymbolClass;
                                    cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, SLFS);
                                    bSwitch = true;
                                }
                            }
                        }
                        if (bSwitch == false)
                        {
                            cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.MultiFillSymbol[LayerIdx]);
                        }
                    }
                    else
                    {
                        cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.MultiFillSymbol[LayerIdx]);
                    }
                    return cReturn;
                }
                #endregion

                #region  处理点符号
                else if (SymbolStructure is ptMarkerSymbolClass)
                {
                    cReturn = GetMarkerValue(ValueNameOfValueYouWant, SymbolStructure as ptMarkerSymbolClass);
                    return cReturn;
                }
                #endregion

                #region 处理线符号
                else if (SymbolStructure is ptSimpleLineSymbolClass)
                {
                    ptSimpleLineSymbolClass objTempStruct = SymbolStructure as ptSimpleLineSymbolClass;
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
                else if (SymbolStructure is ptCartographicLineSymbol)
                {
                    ptCartographicLineSymbol objTempStruct = SymbolStructure as ptCartographicLineSymbol;
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
                else if (SymbolStructure is ptHashLineSymbolClass)
                {
                    ptHashLineSymbolClass objTempStruct = SymbolStructure as ptHashLineSymbolClass;
                    if ((ValueNameOfValueYouWant.ToUpper() == "LineWidth".ToUpper()) || (ValueNameOfValueYouWant.ToUpper() == "LineDashArray".ToUpper()))
                    {
                        cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.HashSymbol);
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
                else if (SymbolStructure is ptMarkerLineSymbolClass)
                {
                    ptMarkerLineSymbolClass objTempStruct = SymbolStructure as ptMarkerLineSymbolClass;
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
                else if (SymbolStructure is ptPictureLineSymbolClass)
                {
                    ptPictureLineSymbolClass objTempStruct = SymbolStructure as ptPictureLineSymbolClass;
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
                else if (SymbolStructure is ptSimpleFillSymbolClass)
                {
                    ptSimpleFillSymbolClass objTempStruct = SymbolStructure as ptSimpleFillSymbolClass;
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
                        if (objTempStruct.OutlineSymbol is ptMultilayerLineSymbolClass)
                        {
                            cReturn = GetValueFromSymbolstruct("LineWidth", objTempStruct.OutlineSymbol);
                        }
                        else
                        {
                            cReturn = CommaToPoint(objTempStruct.OutlineSymbol.Width);
                        }
                    }
                    else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderColor".ToUpper())
                    {
                        if (objTempStruct.OutlineSymbol is ptMultilayerLineSymbolClass)
                        {
                            cReturn = GetValueFromSymbolstruct("LineColor", objTempStruct.OutlineSymbol);
                        }
                        else
                        {
                            cReturn = objTempStruct.OutlineSymbol.Color;
                        }
                    }
                    else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderOpacity".ToUpper())
                    {
                        double tmpTransparency = 255.0;
                        if (objTempStruct.OutlineSymbol is ptMultilayerLineSymbolClass)
                        {
                            tmpTransparency = System.Convert.ToDouble(255 * double.Parse(GetValueFromSymbolstruct("LineOpacity", objTempStruct.OutlineSymbol)));
                        }
                        else
                        {
                            tmpTransparency = objTempStruct.OutlineSymbol.Transparency;
                        }
                        cReturn = CommaToPoint(tmpTransparency / 255.0);
                    }
                    return cReturn;
                }
                else if (SymbolStructure is ptMarkerFillSymbolClass)
                {
                    ptMarkerFillSymbolClass objTempStruct = SymbolStructure as ptMarkerFillSymbolClass;
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
                        if (objTempStruct.OutlineSymbol is ptMultilayerLineSymbolClass)
                        {
                            cReturn = GetValueFromSymbolstruct("LineWidth", objTempStruct.OutlineSymbol);
                        }
                        else
                        {
                            cReturn = CommaToPoint(objTempStruct.OutlineSymbol.Width);
                        }
                    }
                    else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderColor".ToUpper())
                    {
                        if (objTempStruct.OutlineSymbol is ptMultilayerLineSymbolClass)
                        {
                            cReturn = GetValueFromSymbolstruct("LineColor", objTempStruct.OutlineSymbol);
                        }
                        else
                        {
                            cReturn = objTempStruct.OutlineSymbol.Color;
                        }
                    }
                    else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderOpacity".ToUpper())
                    {
                        double tmpTransparency = 255.0;
                        if (objTempStruct.OutlineSymbol is ptMultilayerLineSymbolClass)
                        {
                            tmpTransparency = System.Convert.ToDouble(255 * double.Parse(GetValueFromSymbolstruct("LineOpacity", objTempStruct.OutlineSymbol)));
                        }
                        else
                        {
                            tmpTransparency = objTempStruct.OutlineSymbol.Transparency;
                        }
                        cReturn = CommaToPoint(tmpTransparency / 255.0);
                    }
                    else if (ValueNameOfValueYouWant.ToUpper() == "PointSize".ToUpper())
                    {
                        if (objTempStruct.MarkerSymbol is ptMultilayerMarkerSymbolClass)
                        {
                            cReturn = GetValueFromSymbolstruct("PointSize", objTempStruct.MarkerSymbol);
                        }
                        else
                        {
                            cReturn = CommaToPoint(objTempStruct.MarkerSymbol.Size);
                        }
                    }
                    else if (ValueNameOfValueYouWant.ToUpper() == "PointColor".ToUpper())
                    {
                        if (objTempStruct.MarkerSymbol is ptMultilayerMarkerSymbolClass)
                        {
                            cReturn = GetValueFromSymbolstruct("PointColor", objTempStruct.MarkerSymbol);
                        }
                        else
                        {
                            cReturn = objTempStruct.MarkerSymbol.Color;
                        }
                    }
                    return cReturn;
                }
                else if (SymbolStructure is ptLineFillSymbolClass)
                {
                    ptLineFillSymbolClass objTempStruct = SymbolStructure as ptLineFillSymbolClass;
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
                        if (objTempStruct.OutlineSymbol is ptMultilayerLineSymbolClass)
                        {
                            cReturn = GetValueFromSymbolstruct("LineWidth", objTempStruct.OutlineSymbol);
                        }
                        else
                        {
                            cReturn = CommaToPoint(objTempStruct.OutlineSymbol.Width);
                        }
                    }
                    else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderColor".ToUpper())
                    {
                        if (objTempStruct.OutlineSymbol is ptMultilayerLineSymbolClass)
                        {
                            cReturn = GetValueFromSymbolstruct("LineColor", objTempStruct.OutlineSymbol);
                        }
                        else
                        {
                            cReturn = objTempStruct.OutlineSymbol.Color;
                        }
                    }
                    else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderOpacity".ToUpper())
                    {
                        double tmpTransparency = 255.0;
                        if (objTempStruct.OutlineSymbol is ptMultilayerLineSymbolClass)
                        {
                            tmpTransparency = System.Convert.ToDouble(255 * double.Parse(GetValueFromSymbolstruct("LineOpacity", objTempStruct.OutlineSymbol)));
                        }
                        else
                        {
                            tmpTransparency = objTempStruct.OutlineSymbol.Transparency;
                        }
                        cReturn = CommaToPoint(tmpTransparency / 255.0);
                    }
                    else if (ValueNameOfValueYouWant.ToUpper() == "LineWidth".ToUpper())
                    {
                        if (objTempStruct.LineSymbol is ptMultilayerLineSymbolClass)
                        {
                            cReturn = GetValueFromSymbolstruct("LineWidth", objTempStruct.LineSymbol);
                        }
                        else
                        {
                            cReturn = CommaToPoint(objTempStruct.LineSymbol.Width);
                        }
                    }
                    else if (ValueNameOfValueYouWant.ToUpper() == "LineColor".ToUpper())
                    {
                        if (objTempStruct.LineSymbol is ptMultilayerLineSymbolClass)
                        {
                            cReturn = GetValueFromSymbolstruct("LineColor", objTempStruct.LineSymbol);
                        }
                        else
                        {
                            cReturn = objTempStruct.LineSymbol.Color;
                        }
                    }
                    else if (ValueNameOfValueYouWant.ToUpper() == "LineOpacity".ToUpper())
                    {
                        double tmpTransparency = 255.0;
                        if (objTempStruct.LineSymbol is ptMultilayerLineSymbolClass)
                        {
                            tmpTransparency = System.Convert.ToDouble(255 * double.Parse(GetValueFromSymbolstruct("LineOpacity", objTempStruct.LineSymbol)));
                        }
                        else
                        {
                            tmpTransparency = objTempStruct.LineSymbol.Transparency;
                        }
                        cReturn = CommaToPoint(tmpTransparency / 255.0);
                    }
                    return cReturn;
                }
                else if (SymbolStructure is ptDotDensityFillSymbolClass)
                {
                    ptDotDensityFillSymbolClass objTempStruct = SymbolStructure as ptDotDensityFillSymbolClass;
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
                        if (objTempStruct.OutlineSymbol is ptMultilayerLineSymbolClass)
                        {
                            cReturn = GetValueFromSymbolstruct("LineWidth", objTempStruct.OutlineSymbol);
                        }
                        else
                        {
                            cReturn = CommaToPoint(objTempStruct.OutlineSymbol.Width);
                        }
                    }
                    else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderColor".ToUpper())
                    {
                        if (objTempStruct.OutlineSymbol is ptMultilayerLineSymbolClass)
                        {
                            cReturn = GetValueFromSymbolstruct("LineColor", objTempStruct.OutlineSymbol);
                        }
                        else
                        {
                            cReturn = objTempStruct.OutlineSymbol.Color;
                        }
                    }
                    else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderOpacity".ToUpper())
                    {
                        double tmpTransparency = 255.0;
                        if (objTempStruct.OutlineSymbol is ptMultilayerLineSymbolClass)
                        {
                            tmpTransparency = System.Convert.ToDouble(255 * double.Parse(GetValueFromSymbolstruct("LineOpacity", objTempStruct.OutlineSymbol)));
                        }
                        else
                        {
                            tmpTransparency = objTempStruct.OutlineSymbol.Transparency;
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
                else if (SymbolStructure is ptPictureFillSymbolClass)
                {
                    ptPictureFillSymbolClass objTempStruct = SymbolStructure as ptPictureFillSymbolClass;
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
                        if (objTempStruct.OutlineSymbol is ptMultilayerLineSymbolClass)
                        {
                            cReturn = GetValueFromSymbolstruct("LineWidth", objTempStruct.OutlineSymbol);
                        }
                        else
                        {
                            cReturn = CommaToPoint(objTempStruct.OutlineSymbol.Width);
                        }
                    }
                    else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderColor".ToUpper())
                    {
                        if (objTempStruct.OutlineSymbol is ptMultilayerLineSymbolClass)
                        {
                            cReturn = GetValueFromSymbolstruct("LineColor", objTempStruct.OutlineSymbol);
                        }
                        else
                        {
                            cReturn = objTempStruct.OutlineSymbol.Color;
                        }
                    }
                    else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderOpacity".ToUpper())
                    {
                        double tmpTransparency = 255.0;
                        if (objTempStruct.OutlineSymbol is ptMultilayerLineSymbolClass)
                        {
                            tmpTransparency = System.Convert.ToDouble(255 * double.Parse(GetValueFromSymbolstruct("LineOpacity", objTempStruct.OutlineSymbol)));
                        }
                        else
                        {
                            tmpTransparency = objTempStruct.OutlineSymbol.Transparency;
                        }
                        cReturn = CommaToPoint(tmpTransparency / 255.0);
                    }
                    return cReturn;
                }
                else if (SymbolStructure is ptGradientFillSymbolClass)
                {
                    ptGradientFillSymbolClass objTempStruct = SymbolStructure as ptGradientFillSymbolClass;
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
                        if (objTempStruct.OutlineSymbol is ptMultilayerLineSymbolClass)
                        {
                            cReturn = GetValueFromSymbolstruct("LineWidth", objTempStruct.OutlineSymbol);
                        }
                        else
                        {
                            cReturn = CommaToPoint(objTempStruct.OutlineSymbol.Width);
                        }
                    }
                    else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderColor".ToUpper())
                    {
                        if (objTempStruct.OutlineSymbol is ptMultilayerLineSymbolClass)
                        {
                            cReturn = GetValueFromSymbolstruct("LineColor", objTempStruct.OutlineSymbol);
                        }
                        else
                        {
                            cReturn = objTempStruct.OutlineSymbol.Color;
                        }
                    }
                    else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderOpacity".ToUpper())
                    {
                        double tmpTransparency = 255.0;
                        if (objTempStruct.OutlineSymbol is ptMultilayerLineSymbolClass)
                        {
                            tmpTransparency = System.Convert.ToDouble(255 * double.Parse(GetValueFromSymbolstruct("LineOpacity", objTempStruct.OutlineSymbol)));
                        }
                        else
                        {
                            tmpTransparency = objTempStruct.OutlineSymbol.Transparency;
                        }
                        cReturn = CommaToPoint(tmpTransparency / 255.0);
                    }
                    return cReturn;
                }
                #endregion

                #region 统计图符号（不支持）
                else if (SymbolStructure is ptBarChartSymbolClass)
                {
                    ptBarChartSymbolClass objTempStruct = SymbolStructure as ptBarChartSymbolClass;
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
                else if (SymbolStructure is ptPieChartSymbolClass)
                {
                    ptPieChartSymbolClass objTempStruct = SymbolStructure as ptPieChartSymbolClass;
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
                else if (SymbolStructure is ptStackedChartSymbolClass)
                {
                    ptStackedChartSymbolClass objTempStruct = SymbolStructure as ptStackedChartSymbolClass;
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
                else if (SymbolStructure is TextSymbolClass)
                {
                    TextSymbolClass objTempStruct = SymbolStructure as TextSymbolClass;
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
		private string GetMarkerValue(string ValueNameOfValueYouWant, ptMarkerSymbolClass SymbolStructure)
        {
            string cReturn = "0";
            string cColor = "";
            string cOutlineColor = "";
            bool bSwitch = false;
            try
            {
                #region 简单标记符号
                if (SymbolStructure is ptSimpleMarkerSymbolClass)
                {
                    ptSimpleMarkerSymbolClass objTempStruct = objTempStruct = SymbolStructure as ptSimpleMarkerSymbolClass;
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
                        if (objTempStruct.Outline) { cReturn = objTempStruct.OutlineColor; }
                        else { cReturn = ""; }
                    }
                    else if (ValueNameOfValueYouWant.ToUpper() == "PointOutlineSize".ToUpper())
                    {
                        cReturn = CommaToPoint(objTempStruct.OutlineSize);
                    }
                    return cReturn;
                }
                #endregion

                #region 字符集标记符号
                else if (SymbolStructure is ptCharacterMarkerSymbolClass)
                {
                    ptCharacterMarkerSymbolClass objTempStruct = SymbolStructure as ptCharacterMarkerSymbolClass;
                    if (ValueNameOfValueYouWant.ToUpper() == "WellKnownName".ToUpper())
                    {
                        cReturn = string.Format("ttf://{0}#0x{1}", objTempStruct.Font, objTempStruct.CharacterIndex.ToString("X"));
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
                else if (SymbolStructure is ptPictureMarkerSymbolClass)
                {
                    ptPictureMarkerSymbolClass objTempStruct = SymbolStructure as ptPictureMarkerSymbolClass;
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
                else if (SymbolStructure is ptArrowMarkerSymbolClass)
                {
                    ptArrowMarkerSymbolClass objTempStruct = SymbolStructure as ptArrowMarkerSymbolClass;
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
            ptLogManager.WriteMessage(string.Format("{0}{1}{2}{3}{4} 方法名称:{5}", message, Environment.NewLine, exMessage, Environment.NewLine, stack, functionname));
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
            ptLogManager.WriteMessage(string.Format("{0} 方法名称:{1}", message, functionname));
            return null;
        }
#endregion
	}

    public sealed class IPictureConverter : AxHost
    {
        private IPictureConverter() : base("") { }

        #region IPictureDisp
        public static stdole.IPictureDisp ImageToIPictureDisp(Image image)
        {
            return (stdole.IPictureDisp)GetIPictureDispFromPicture(image);
        }

        public static Image IPictureDispToImage(stdole.IPictureDisp pictureDisp)
        {
            return GetPictureFromIPictureDisp(pictureDisp);
        }
        #endregion

        #region IPicture
        public static stdole.IPicture ImageToIPicture(Image image)
        {
            return (stdole.IPicture)GetIPictureFromPicture(image);
        }

        public static Image IPictureToImage(stdole.IPicture picture)
        {
            return GetPictureFromIPicture(picture);
        }
        #endregion
    }

}
