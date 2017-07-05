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
                        XmlElement NameLayerelment= m_newXmlDoc.DocumentElement.AppendChild(CommXmlHandle.CreateElement("NamedLayer", m_newXmlDoc)) as XmlElement;
                        NameLayerelment.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("LayerName", m_newXmlDoc, strDatasetName));
                        XmlElement UserStyleElment = CommXmlHandle.CreateElement("UserStyle", m_newXmlDoc);
                        UserStyleElment.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("StyleName", m_newXmlDoc, "Style1"));
                        NameLayerelment.AppendChild(UserStyleElment);
                        XmlElement TypeStyleElement = CommXmlHandle.CreateElement("FeatureTypeStyle", m_newXmlDoc);
                        TypeStyleElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("FeatureTypeName", m_newXmlDoc, strDatasetName));
                        UserStyleElment.AppendChild(TypeStyleElement);
                        pRender.GetRendXmlNode(m_newXmlDoc, TypeStyleElement);
                    }
                    else
                    {
                        //tempElement = m_newXmlDoc.DocumentElement.AppendChild(CommXmlHandle.CreateElement("FeatureTypeStyle", m_newXmlDoc)) as XmlElement;
                        //tempElement = tempElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("FeatureTypeName", m_newXmlDoc, strDatasetName)) as XmlElement;
                        //tempElement = tempElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("FeatureTypeTitle", m_newXmlDoc, strDatasetName)) as XmlElement;
                    }
                    #endregion
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
                ptLogManager.WriteMessage(string.Format("SLD文件写入错误:方法名称【{0}】{1}{2}{3}{4}","WriteToSLD",Environment.NewLine, ex.Message,Environment.NewLine, ex.StackTrace));
                return false;
            }
        }
        #endregion
	}

}
