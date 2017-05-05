using System;
using System.Drawing;
using System.Diagnostics;
using System.Data;
using Microsoft.VisualBasic;
using System.Collections;
using System.Windows.Forms;
using System.Xml;
using System.Text;
using System.IO;
using Microsoft.VisualBasic.CompilerServices;
using ESRI.ArcGIS;
using ESRI.ArcGIS.ArcMapUI;

namespace ArcGIS_SLD_Converter
{
	public class Motherform : System.Windows.Forms.Form
	{

        /// <summary>
        /// 初始化函数
        /// </summary>
        /// <param name="mainDocument"></param>
        public Motherform(IMxDocument mainDocument)
        {
            InitializeComponent();
            m_bLabel = false;
            m_bIncludeLayerNames = true;
            SetSizeOpen();
            m_MianDocument = mainDocument;
            string tempStr = System.IO.Path.GetDirectoryName(GetType().Assembly.Location);
            string LogFileName = tempStr +"\\"+ DateTime.Now.ToString("yyyyMMddHHmmss")+".log";
            ptLogManager.Create_LogFile(LogFileName);

        }
		
#region  窗体设计代码
		
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!(components == null))
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
        private System.ComponentModel.IContainer components;
		internal System.Windows.Forms.Button Button1;
		internal System.Windows.Forms.Button Button2;
		internal System.Windows.Forms.SaveFileDialog dlgSave;
		internal System.Windows.Forms.Label Label3;
		internal System.Windows.Forms.TextBox txtFileName;
		internal System.Windows.Forms.GroupBox GroupBox2;
		internal System.Windows.Forms.Label lblTop;
		internal System.Windows.Forms.Label lblBottom;
		internal System.Windows.Forms.Label lblSmall;
		internal System.Windows.Forms.CheckBox chkValidate;
		internal System.Windows.Forms.TextBox txtSLDxsd;
		internal System.Windows.Forms.OpenFileDialog OpenXSD;
		internal System.Windows.Forms.PictureBox PictureBox1;
		internal System.Windows.Forms.ImageList ImageList1;
		internal System.Windows.Forms.MainMenu MainMenu1;
		internal System.Windows.Forms.MenuItem MenuItem1;
		internal System.Windows.Forms.MenuItem MenuItem7;
		internal System.Windows.Forms.MenuItem MenuItem8;
		internal System.Windows.Forms.MenuItem MenuItem9;
		internal System.Windows.Forms.MenuItem mnuIncludeLayerNames;
		internal System.Windows.Forms.MenuItem MenuItem11;
		[System.Diagnostics.DebuggerStepThrough()]
        private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Motherform));
            this.Button1 = new System.Windows.Forms.Button();
            this.Button2 = new System.Windows.Forms.Button();
            this.txtFileName = new System.Windows.Forms.TextBox();
            this.dlgSave = new System.Windows.Forms.SaveFileDialog();
            this.Label3 = new System.Windows.Forms.Label();
            this.OpenXSD = new System.Windows.Forms.OpenFileDialog();
            this.GroupBox2 = new System.Windows.Forms.GroupBox();
            this.lblSmall = new System.Windows.Forms.Label();
            this.lblBottom = new System.Windows.Forms.Label();
            this.lblTop = new System.Windows.Forms.Label();
            this.chkValidate = new System.Windows.Forms.CheckBox();
            this.txtSLDxsd = new System.Windows.Forms.TextBox();
            this.PictureBox1 = new System.Windows.Forms.PictureBox();
            this.ImageList1 = new System.Windows.Forms.ImageList(this.components);
            this.MainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.MenuItem1 = new System.Windows.Forms.MenuItem();
            this.MenuItem7 = new System.Windows.Forms.MenuItem();
            this.MenuItem8 = new System.Windows.Forms.MenuItem();
            this.MenuItem9 = new System.Windows.Forms.MenuItem();
            this.MenuItem11 = new System.Windows.Forms.MenuItem();
            this.mnuIncludeLayerNames = new System.Windows.Forms.MenuItem();
            this.GroupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // Button1
            // 
            this.Button1.BackColor = System.Drawing.Color.Transparent;
            this.Button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Button1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Button1.Location = new System.Drawing.Point(8, 168);
            this.Button1.Name = "Button1";
            this.Button1.Size = new System.Drawing.Size(96, 24);
            this.Button1.TabIndex = 0;
            this.Button1.Text = "开始转换";
            this.Button1.UseVisualStyleBackColor = false;
            this.Button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // Button2
            // 
            this.Button2.BackColor = System.Drawing.Color.Transparent;
            this.Button2.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Button2.Location = new System.Drawing.Point(62, 18);
            this.Button2.Name = "Button2";
            this.Button2.Size = new System.Drawing.Size(52, 18);
            this.Button2.TabIndex = 1;
            this.Button2.Text = "..";
            this.Button2.UseVisualStyleBackColor = false;
            this.Button2.Click += new System.EventHandler(this.Button2_Click);
            // 
            // txtFileName
            // 
            this.txtFileName.Location = new System.Drawing.Point(120, 16);
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.Size = new System.Drawing.Size(288, 21);
            this.txtFileName.TabIndex = 2;
            // 
            // Label3
            // 
            this.Label3.BackColor = System.Drawing.Color.Transparent;
            this.Label3.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.Label3.Location = new System.Drawing.Point(8, 20);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(48, 16);
            this.Label3.TabIndex = 5;
            this.Label3.Text = "SLD Speicherort";
            this.Label3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // GroupBox2
            // 
            this.GroupBox2.BackColor = System.Drawing.Color.Transparent;
            this.GroupBox2.Controls.Add(this.lblSmall);
            this.GroupBox2.Controls.Add(this.lblBottom);
            this.GroupBox2.Controls.Add(this.lblTop);
            this.GroupBox2.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.GroupBox2.Location = new System.Drawing.Point(120, 168);
            this.GroupBox2.Name = "GroupBox2";
            this.GroupBox2.Size = new System.Drawing.Size(288, 96);
            this.GroupBox2.TabIndex = 10;
            this.GroupBox2.TabStop = false;
            this.GroupBox2.Text = "提示信息";
            // 
            // lblSmall
            // 
            this.lblSmall.BackColor = System.Drawing.Color.Transparent;
            this.lblSmall.Location = new System.Drawing.Point(8, 72);
            this.lblSmall.Name = "lblSmall";
            this.lblSmall.Size = new System.Drawing.Size(272, 16);
            this.lblSmall.TabIndex = 2;
            // 
            // lblBottom
            // 
            this.lblBottom.BackColor = System.Drawing.Color.Transparent;
            this.lblBottom.Location = new System.Drawing.Point(8, 40);
            this.lblBottom.Name = "lblBottom";
            this.lblBottom.Size = new System.Drawing.Size(272, 32);
            this.lblBottom.TabIndex = 1;
            // 
            // lblTop
            // 
            this.lblTop.BackColor = System.Drawing.Color.Transparent;
            this.lblTop.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTop.Location = new System.Drawing.Point(8, 16);
            this.lblTop.Name = "lblTop";
            this.lblTop.Size = new System.Drawing.Size(272, 24);
            this.lblTop.TabIndex = 0;
            // 
            // chkValidate
            // 
            this.chkValidate.BackColor = System.Drawing.Color.Transparent;
            this.chkValidate.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.chkValidate.Location = new System.Drawing.Point(8, 43);
            this.chkValidate.Name = "chkValidate";
            this.chkValidate.Size = new System.Drawing.Size(120, 24);
            this.chkValidate.TabIndex = 11;
            this.chkValidate.Text = "SLD定义文件";
            this.chkValidate.UseVisualStyleBackColor = false;
            this.chkValidate.CheckedChanged += new System.EventHandler(this.chkValidate_CheckedChanged);
            // 
            // txtSLDxsd
            // 
            this.txtSLDxsd.Location = new System.Drawing.Point(120, 43);
            this.txtSLDxsd.Name = "txtSLDxsd";
            this.txtSLDxsd.Size = new System.Drawing.Size(288, 21);
            this.txtSLDxsd.TabIndex = 13;
            this.txtSLDxsd.Visible = false;
            // 
            // PictureBox1
            // 
            this.PictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.PictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("PictureBox1.Image")));
            this.PictureBox1.Location = new System.Drawing.Point(40, 216);
            this.PictureBox1.Name = "PictureBox1";
            this.PictureBox1.Size = new System.Drawing.Size(32, 32);
            this.PictureBox1.TabIndex = 15;
            this.PictureBox1.TabStop = false;
            // 
            // ImageList1
            // 
            this.ImageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ImageList1.ImageStream")));
            this.ImageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.ImageList1.Images.SetKeyName(0, "");
            // 
            // MainMenu1
            // 
            this.MainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.MenuItem1});
            // 
            // MenuItem1
            // 
            this.MenuItem1.Index = 0;
            this.MenuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.MenuItem7,
            this.mnuIncludeLayerNames});
            this.MenuItem1.Text = "高级";
            // 
            // MenuItem7
            // 
            this.MenuItem7.Index = 0;
            this.MenuItem7.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.MenuItem8,
            this.MenuItem9,
            this.MenuItem11});
            this.MenuItem7.Text = "图层设置";
            // 
            // MenuItem8
            // 
            this.MenuItem8.Checked = true;
            this.MenuItem8.Index = 0;
            this.MenuItem8.RadioCheck = true;
            this.MenuItem8.Text = "转换所有图层";
            this.MenuItem8.Click += new System.EventHandler(this.MenuItem8_Click);
            // 
            // MenuItem9
            // 
            this.MenuItem9.Index = 1;
            this.MenuItem9.RadioCheck = true;
            this.MenuItem9.Text = "转换可视图层";
            this.MenuItem9.Click += new System.EventHandler(this.MenuItem9_Click);
            // 
            // MenuItem11
            // 
            this.MenuItem11.Index = 2;
            this.MenuItem11.Text = "是否保存为单个文件";
            this.MenuItem11.Click += new System.EventHandler(this.MenuItem11_Click);
            // 
            // mnuIncludeLayerNames
            // 
            this.mnuIncludeLayerNames.Checked = true;
            this.mnuIncludeLayerNames.Index = 1;
            this.mnuIncludeLayerNames.Text = "是否包含图层名";
            this.mnuIncludeLayerNames.Click += new System.EventHandler(this.mnuIncludeLayerNames_Click);
            // 
            // Motherform
            // 
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.SteelBlue;
            this.ClientSize = new System.Drawing.Size(423, 275);
            this.Controls.Add(this.PictureBox1);
            this.Controls.Add(this.txtSLDxsd);
            this.Controls.Add(this.chkValidate);
            this.Controls.Add(this.GroupBox2);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.txtFileName);
            this.Controls.Add(this.Button2);
            this.Controls.Add(this.Button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Menu = this.MainMenu1;
            this.Name = "Motherform";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ArcGIS符号转换成SLD";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.Brown;
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Motherform_Closing);
            this.Load += new System.EventHandler(this.Motherform_Load);
            this.GroupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		
#endregion
		

#region 成员
		private Analize_ArcMap_Symbols AnalizeArcMap; 
		private string m_cSLDFilename; 
		private string m_cSLDPath; 
		private string m_cSLDFile; 

		private string m_cSLDTempFilename; 

		private string m_cXSDFilename;

        private IMxDocument m_MianDocument;
		private bool m_bLabel;
        /// <summary>
        ///是否转换全部图层，
        /// </summary>
		internal bool m_bAllLayers; 
        /// <summary>
        /// 是否存储为一个SLD文件
        /// </summary>
		private bool m_bSeparateFiles;
        /// <summary>
        /// 是否包含图层名称
        /// </summary>
		private bool m_bIncludeLayerNames; 

		private string m_cLUTFilename;
#region 枚举类
		
		/// <summary>
        /// 保存文件类型
        /// </summary>
		internal enum Fileinfo
		{
			Name = 0, //保存类型为文件
			Path = 1 //保存类型为文件路径
		}
		
#endregion
		
#endregion
		

#region 方法
        /// <summary>
        /// 获取保存文件信息
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="WhatDoIWant"></param>
        /// <returns></returns>
		private string GetFileInfo(string FileName, Fileinfo WhatDoIWant)
		{
			int iLastIndex = 0;
			string cWantedSubstring = "";
			iLastIndex = FileName.LastIndexOf("\\");
			if (WhatDoIWant == Fileinfo.Name)
			{
				cWantedSubstring = FileName.Substring(iLastIndex + 1);
			}
			else if (WhatDoIWant == Fileinfo.Path)
			{
				cWantedSubstring = FileName.Remove(iLastIndex, FileName.Substring(iLastIndex).Length);
			}
			return cWantedSubstring;
		}
#endregion
		

        #region 界面信息修改
        /// <summary>
        /// 更新提示框底部信息
        /// </summary>
        /// <param name="value"></param>
		public void CHLabelBottom(string value)
		{
			this.lblBottom.Text = value;
			this.Refresh();
		}
		/// <summary>
        /// 更新提示框头部信息
        /// </summary>
        /// <param name="value"></param>
		public void CHLabelTop(string value)
		{
			this.lblTop.Text = value;
			this.Refresh();
		}
		
		public void CHLabelSmall(string value)
		{
			this.lblSmall.Text = value;
			this.Refresh();
		}
		/// <summary>
        /// 窗体最小化
        /// </summary>
		internal void MinimizeWindow()
		{
			DateTime dCurrentTime = default(DateTime);
			dCurrentTime = DateTime.Now;
			DateTime dTargetTime = default(DateTime);
			dTargetTime = dCurrentTime.AddSeconds(5);
			
			while (dCurrentTime < dTargetTime)
			{
				dCurrentTime = DateTime.Now;
			}
			this.CHLabelSmall("");
			this.CHLabelBottom("");
			this.CHLabelTop("");
			SetSizeClose();
		}
		/// <summary>
        /// 窗体隐藏提示信息
        /// </summary>
		internal void MinimizeWindow2()
		{
			this.CHLabelSmall("");
			this.CHLabelBottom("");
			this.CHLabelTop("");
			SetSizeClose();
		}
		/// <summary>
        /// 
        /// </summary>
		private void SetSizeOpen()
		{
			this.Height = 232;
			GroupBox2.Height = 24;
		}
		
		private void SetSizeClose()
		{
			this.Height = 252;
			GroupBox2.Height = 24;
		}
		/// <summary>
		/// 关闭应用程序
		/// </summary>
		public void MyTermination()
		{
			this.Close();
			this.Dispose();
			ProjectData.EndApp();
		}
		/// <summary>
        /// 处理所有的当前在消息队列中的Windows消息
		/// </summary>
		public void DoEvents()
		{
			Application.DoEvents();
		}
        /// <summary>
        /// 显示错误信息
        /// </summary>
        /// <param name="Message"></param>
        /// <param name="ExMessage"></param>
        /// <param name="FunctionName"></param>
		private void ErrorMsg(string Message, string ExMessage, string FunctionName)
		{
            ptLogManager.WriteMessage(string.Format("{0}{1}{2} 方法名称:{3}", Message, Environment.NewLine, ExMessage, FunctionName));
		}
        /// <summary>
        /// 显示提示信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="functionname"></param>
		private void InfoMsg(string message, string functionname)
		{
            ptLogManager.WriteMessage(string.Format("{0} 方法名称:{1}",message,functionname));

        }
        /// <summary>
        /// 显示图片
        /// </summary>
        internal void ShowWorld()
		{
			PictureBox1.Visible = true;
			PictureBox1.Refresh();
			this.Refresh();
		}
        /// <summary>
        /// 隐藏图片
        /// </summary>
        internal void HideWorld()
		{
			PictureBox1.Visible = false;
		}
        #endregion

        #region 属性
        /// <summary>
        /// SLD文件名称
        /// </summary>
        public string GetSLDFilename
        {
            get
            {
                return m_cSLDFilename;
            }
        }
        /// <summary>
        /// 是否包含图层名称
        /// </summary>
        public string GetInfoIncludeLayerNames
        {
            get
            {
                return m_bIncludeLayerNames.ToString();
            }
        }
        /// <summary>
        /// 是否所有符号信息存储在一个SLD文件中
        /// </summary>
        public bool GetInfoSeparateLayers
		{
			get
			{
				return m_bSeparateFiles;
			}
		}
        /// <summary>
        /// SLD文件路径
        /// </summary>
        public string GetSLDPath
		{
			get
			{
				return m_cSLDPath;
			}
		}
        /// <summary>
        /// 获取SLD文件路径
        /// </summary>
        public string GetSLDFile
		{
			get
			{
				return m_cSLDFile;
			}
		}
        /// <summary>
        /// 获取XSD文件路径
        /// </summary>
        public string GetXSDFilename
		{
			get
			{
				return m_cXSDFilename;
			}
		}
        /// <summary>
        /// 获取SLD配置文件路径
        /// </summary>
        public string GetSLDFileFromConfigXML
		{
			get
			{
				return m_cSLDTempFilename;
			}
		}
        #endregion
		
        #region 窗体事件
		/// <summary>
		/// 进行转换
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Button1_Click(System.Object sender, System.EventArgs e)
		{
			this.Height = 324;
			this.GroupBox2.Height = 96;

			AnalizeArcMap = new Analize_ArcMap_Symbols(this, m_cSLDFilename,m_MianDocument);

			if (this.chkValidate.Checked == false)
			{
				MinimizeWindow();
			}

			if (this.lblSmall.Text == "")
			{
				MinimizeWindow();
			}
		}
		/// <summary>
		/// 选择SLD保存路径
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void Button2_Click(System.Object sender, System.EventArgs e)
		{
			System.IO.StreamWriter swOutput;
			if (System.IO.File.Exists(this.GetSLDFileFromConfigXML))
			{
				this.dlgSave.InitialDirectory = this.GetSLDFileFromConfigXML;
			}
			dlgSave.CheckFileExists = false;
			dlgSave.CheckPathExists = true;
			dlgSave.DefaultExt = "sld";
			dlgSave.Filter = "SLD (*.sld)|*.sld";
			dlgSave.AddExtension = true;
			dlgSave.OverwritePrompt = true;
			dlgSave.CreatePrompt = false;
			if (dlgSave.ShowDialog() == DialogResult.OK)
			{
				m_cSLDFilename = dlgSave.FileName; 
				m_cSLDPath = GetFileInfo(m_cSLDFilename, Fileinfo.Path);
				m_cSLDFile = GetFileInfo(m_cSLDFilename, Fileinfo.Name);
				txtFileName.Text = m_cSLDFilename;
			}
		}
		/// <summary>
        /// 加载XSD文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void chkValidate_CheckedChanged(object sender, System.EventArgs e)
		{
			if (chkValidate.Checked == true)
			{
				txtSLDxsd.Visible = true;
				OpenXSD.CheckFileExists = false;
				OpenXSD.CheckPathExists = true;
				OpenXSD.Filter = "Schemadateien (*.xsd)|*.xsd";
				OpenXSD.InitialDirectory = System.IO.Path.GetDirectoryName(m_cSLDFilename);
				if (OpenXSD.ShowDialog() == DialogResult.OK)
				{
					m_cXSDFilename = OpenXSD.FileName;
					txtSLDxsd.Text = m_cXSDFilename;
				}
				else
				{
					m_cXSDFilename = "";
					chkValidate.Checked = false;
					txtSLDxsd.Visible = false;
					return;
				}
			}
			else
			{
				txtSLDxsd.Visible = false;
			}
		}
        /// <summary>
        /// 获取全部图层信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void MenuItem8_Click(System.Object sender, System.EventArgs e)
		{
			m_bAllLayers = true;
			MenuItem8.Checked = true;
			MenuItem9.Checked = false;
		}
		/// <summary>
        /// 转换选择图层
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void MenuItem9_Click(System.Object sender, System.EventArgs e)
		{
			m_bAllLayers = false;
			MenuItem8.Checked = false;
			MenuItem9.Checked = true;
		}
		/// <summary>
        /// 设置是否存为单个文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void MenuItem11_Click(object sender, System.EventArgs e)
		{
			if (m_bSeparateFiles == false)
			{
				m_bSeparateFiles = true;
				MenuItem11.Checked = true;
			}
			else
			{
				m_bSeparateFiles = false;
				MenuItem11.Checked = false;
			}
		}
		/// <summary>
        /// 窗体关闭事件、保存比例尺配置信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void Motherform_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{

		}
#endregion
		/// <summary>
        /// 窗体加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void Motherform_Load(System.Object sender, System.EventArgs e)
		{
            //读取配置信息
            string TempLutFileName = "";
            if (m_bIncludeLayerNames)
            {
                TempLutFileName = CommXmlHandle.c_strLUT_Standard;
            }
            else
            {
                TempLutFileName = CommXmlHandle.c_strLUT_WorldMap;
            }
            if (!CommXmlHandle.ReadLUT(Path.GetDirectoryName(GetType().Assembly.Location), TempLutFileName))
            {
                MessageBox.Show("读取配置信息有误！","提示",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
		}
		/// <summary>
        /// 是否包含图层名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void mnuIncludeLayerNames_Click(System.Object sender, System.EventArgs e)
		{
            if (mnuIncludeLayerNames.Checked)
            {
                mnuIncludeLayerNames.Checked = false;
                m_bIncludeLayerNames = false;
                if (!CommXmlHandle.ReadLUT(Path.GetDirectoryName( GetType().Assembly.Location), CommXmlHandle.c_strLUT_WorldMap))
                {
                    MessageBox.Show("读取配置信息有误！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                mnuIncludeLayerNames.Checked = true;
                m_bIncludeLayerNames = true;
                if (!CommXmlHandle.ReadLUT(Path.GetDirectoryName(GetType().Assembly.Location), CommXmlHandle.c_strLUT_Standard))
                {
                    MessageBox.Show("读取配置信息有误！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
		}
		
	}
	
}
