using ESRI.ArcGIS.ArcMapUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ArcGIS_SLD_Converter
{
    public partial class MainForm : Form
    {
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="logStr"></param>
        public delegate void WriteConverterLogDelegate(string logStr); 
        public MainForm(IMxDocument mainDocument)
        {
            InitializeComponent();
            m_MianDocument = mainDocument;
            string tempStr = System.IO.Path.GetDirectoryName(GetType().Assembly.Location);
            string LogFileName = tempStr + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".log";
            ptLogManager.Create_LogFile(LogFileName);
        }
        /// <summary>
        /// 是否保存为单个SLD文件
        /// </summary>
        private bool m_IsSingleFile = false;
        /// <summary>
        /// SLD保存路径
        /// </summary>
        private string m_SelectSaveFile = string.Empty;
        /// <summary>
        /// SLD文件中是否包含图层名称
        /// </summary>
        private bool m_IncludeLayerName = false;
        /// <summary>
        /// 转换所有图层
        /// </summary>
        private bool m_AllLayer = true;
        /// <summary>
        /// 验证文件名称
        /// </summary>
        private string m_cXSDFilename = string.Empty;
        /// <summary>
        /// 地图文档
        /// </summary>
        private IMxDocument m_MianDocument;
        /// <summary>
        /// 选择保存路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGet_Click(object sender, EventArgs e)
        {
            //保存为单个SLD文件
            if (cbSaveSingleFile.Checked)
            {
                m_IsSingleFile = true;
                SaveFileDialog SelectFileDia = new SaveFileDialog();
                SelectFileDia.Filter = "*.sld|*.sld";
                if (!string.IsNullOrEmpty(m_SelectSaveFile))
                {
                    string filePath = System.IO.Path.GetFullPath(m_SelectSaveFile);
                    SelectFileDia.InitialDirectory = filePath;
                }
                if (SelectFileDia.ShowDialog() != DialogResult.OK) return;
                m_SelectSaveFile = SelectFileDia.FileName;
            }
            else //每个图层都保存为一个SLD文件
            {
                m_IsSingleFile = false;
                FolderBrowserDialog SelectFilePath = new FolderBrowserDialog();
                if (!string.IsNullOrEmpty(m_SelectSaveFile))
                {
                    string filePath = System.IO.Path.GetFullPath(m_SelectSaveFile);
                    SelectFilePath.SelectedPath = m_SelectSaveFile;
                }
                if (SelectFilePath.ShowDialog() != DialogResult.OK) return;
                m_SelectSaveFile = SelectFilePath.SelectedPath;
            }
        }
        /// <summary>
        /// 是否包含图层名状态改变
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbIncludeLayerName_CheckedChanged(object sender, EventArgs e)
        {
            m_IncludeLayerName = cbIncludeLayerName.Checked;
            //重新读取配置文件
            ReadXmlConfig();
        }
        /// <summary>
        /// 是否保存为单个SLD文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbSaveSingleFile_CheckedChanged(object sender, EventArgs e)
        {
            m_IsSingleFile = cbSaveSingleFile.Checked;
        }
        /// <summary>
        /// 是否转换所有图层
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbAllLayer_CheckedChanged(object sender, EventArgs e)
        {
            m_AllLayer = rbAllLayer.Checked;
        }
        /// <summary>
        /// 是否只转换可视图层
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbVisibleLayer_CheckedChanged(object sender, EventArgs e)
        {
            m_AllLayer = !rbVisibleLayer.Checked;
        }
        /// <summary>
        /// 是否验证SLD文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkValidate_CheckedChanged(object sender, EventArgs e)
        {
            if (chkValidate.Checked == true)
            {
                txtXsdFilePath.Visible = true;
                OpenFileDialog OpenXSD = new OpenFileDialog();
                OpenXSD.CheckFileExists = false;
                OpenXSD.CheckPathExists = true;
                OpenXSD.Filter = "Schemadateien (*.xsd)|*.xsd";
                if (!string.IsNullOrEmpty(m_SelectSaveFile))
                {
                    OpenXSD.InitialDirectory = System.IO.Path.GetDirectoryName(m_SelectSaveFile);
                }
                if (OpenXSD.ShowDialog() == DialogResult.OK)
                {
                    m_cXSDFilename = OpenXSD.FileName;
                    txtXsdFilePath.Text = m_cXSDFilename;
                }
                else
                {
                    m_cXSDFilename = "";
                    chkValidate.Checked = false;
                    txtXsdFilePath.Visible = false;
                    return;
                }
            }
            else
            {
                txtXsdFilePath.Visible = false;
            }
        }
        /// <summary>
        /// 开始进行图层符号转换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btStart_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 窗体加载时，读取配置文件信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Load(object sender, EventArgs e)
        {
            ReadXmlConfig();
        }
        /// <summary>
        /// 读取XML配置文件信息
        /// </summary>
        private void ReadXmlConfig()
        {
            //读取配置信息
            string TempLutFileName = "";
            if (m_IncludeLayerName)
            {
                TempLutFileName = CommXmlHandle.c_strLUT_Standard;
            }
            else
            {
                TempLutFileName = CommXmlHandle.c_strLUT_WorldMap;
            }
            if (!CommXmlHandle.ReadLUT(Path.GetDirectoryName(GetType().Assembly.Location), TempLutFileName))
            {
                MessageBox.Show("读取配置信息有误！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// 写日志信息
        /// </summary>
        /// <param name="strLog"></param>
        private void WriteConvertLog(string strLog)
        {
            txtMessage.Select(txtMessage.Text.Length, 0);
            txtMessage.ScrollToCaret();
            txtMessage.AppendText(string.Format("{0}:{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff"), strLog));
            txtMessage.Refresh();
        }
    }
}
