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
        /// 工程对象
        /// </summary>
        internal ProjectClass m_StrProject;
        /// <summary>
        /// SLD文件路径
        /// </summary>
        private string m_cFilename;
        private WriteConverterLogDelegate m_WriteLog;
        #endregion
        #region 主要处理函数 
        /// <summary>
        /// 初始化函数
        /// </summary>
        /// <param name="value">主窗体</param>
        /// <param name="Filename">保存文件路径</param>
        public Analize_ArcMap_Symbols(string Filename,IMxDocument mxDoc, WriteConverterLogDelegate WriteLog)
        {
            m_cFilename = Filename;
            m_ObjDoc = mxDoc;
            m_WriteLog = WriteLog;
            //CentralProcessingFunc();
        }
        #endregion
        /// <summary>
        /// 分析符号信息主函数
        /// </summary>
        /// <returns></returns>
        public bool CentralProcessingFunc(bool ConvertAllLayers,string sldFilePath,bool SepFile,bool IncludeNames)
        {
            try
            {
                m_WriteLog("正在分析ArcMap符号...");
                bool blnAnswer = false;
                Output_SLD objOutputSLD = null;
                if (GetMap() == false)//获取地图文档
                {
                    return false;
                }
                if (AnalyseLayerSymbology(ConvertAllLayers) == false)//分析地图文档中的图层符号信息
                {
                    return false;
                }
                if (string.IsNullOrEmpty(m_cFilename))
                {
                    blnAnswer = MessageBox.Show("请先选择SLD文件保存路径", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes;
                    if (blnAnswer)
                    {
                        SaveFileDialog GetSaveFile = new SaveFileDialog();
                        GetSaveFile.CheckFileExists = false;
                        GetSaveFile.CheckPathExists = true;
                        GetSaveFile.DefaultExt = "sld";
                        GetSaveFile.Filter = "SLD-files (*.sld)|*.sld";
                        GetSaveFile.AddExtension = true;
                        GetSaveFile.InitialDirectory = System.IO.Path.GetDirectoryName(m_cFilename);
                        GetSaveFile.OverwritePrompt = true;
                        GetSaveFile.CreatePrompt = false;
                        if (GetSaveFile.ShowDialog() == DialogResult.OK)
                        {
                            m_cFilename = GetSaveFile.FileName;
                            objOutputSLD = new Output_SLD(m_WriteLog, m_StrProject, m_cFilename, sldFilePath); //输出SLD文件
                        }
                    }
                }
                else
                {
                    objOutputSLD = new Output_SLD(m_WriteLog, m_StrProject, m_cFilename, sldFilePath);//输出SLD文件
                }
                if (objOutputSLD != null)
                {
                    objOutputSLD.CentralProcessingFunc(SepFile, IncludeNames);
                }
                return true;
            }
            catch (Exception ex)
            {
                m_WriteLog(string.Format("图层转换失败:{0}", ex.Message));
                return false;
            }
        }
        #region 
        /// <summary>
        /// 获取地图文档信息
        /// </summary>
        /// <returns></returns>
		private bool GetMap()
        {
            m_WriteLog(string.Format("获取当前的地图信息..."));
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
		private bool AnalyseLayerSymbology(bool isAllLayers)
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
                    if (isAllLayers == false && objLayer.Visible == false)
                    {
                        m_WriteLog(string.Format("图层【{0}】不可见，不进行分析", cLayerName));
                    }
                    else
                    {
                        m_WriteLog(string.Format("正在分析图层【{0}】...", cLayerName));
                        SpreadLayerStructure(objLayer);
                    }
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
                    }
                }
                //非要素图层和其他图层不分析
                else
                {
                    InfoMsg(string.Format("图层符号类型不支持"), "SpreadLayerStructure");
                }
            }
            catch (Exception e)
            {
                InfoMsg(string.Format("图层转换出错:图层名称{0}", objLayer.Name), "SpreadLayerStructure");
            }
        }
        #endregion
        #region 公共方法
        /// <summary>
        /// 错误消息处理
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="ExMessage"></param>
        /// <param name="Stack"></param>
        /// <param name="FunctionName"></param>
        /// <returns></returns>
		private void ErrorMsg(string Message, string ExMessage, string Stack, string FunctionName)
        {
            m_WriteLog(string.Format("{0}{1}{2}{3}{4} 方法名称:{5}",Message,Environment.NewLine,ExMessage,Environment.NewLine,Stack,FunctionName));
        }
        /// <summary>
        /// 消息处理
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="FunctionName"></param>
        /// <returns></returns>
		private void InfoMsg(string Message, string FunctionName)
        {
            m_WriteLog(string.Format("{0} 方法名称:{1}", Message, FunctionName));
        }
        #endregion
    }
}

