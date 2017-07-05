using ESRI.ArcGIS.ArcMapUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ArcGIS_SLD_Converter
{
    public partial class MainForm : Form
    {
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
        private string m_cXSDFilename = string.Empty;
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
    }
}
