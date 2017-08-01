using System;
using System.Drawing;
using System.Diagnostics;
using System.Data;
using Microsoft.VisualBasic;
using System.Collections;
using System.Windows.Forms;
using System.IO;
using Microsoft.VisualBasic.CompilerServices;

namespace ArcGIS_SLD_Converter
{
	public class Validation_Message : System.Windows.Forms.Form
	{
		
#region  
		
		public Validation_Message(string ValidMessage, Motherform mother)
		{
			InitializeComponent();
			m_cValidationMessage = ValidMessage;
			this.txtValidMessage.Text = m_cValidationMessage;
			frmMotherform = mother;
			
		}
		
	
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
		
	
		private System.ComponentModel.Container components = null;
		

		internal System.Windows.Forms.RichTextBox txtValidMessage;
		internal System.Windows.Forms.Button btnSave;
		internal System.Windows.Forms.Label Label1;
		internal System.Windows.Forms.SaveFileDialog dlgSave;
		[System.Diagnostics.DebuggerStepThrough()]private void InitializeComponent()
		{
            this.txtValidMessage = new System.Windows.Forms.RichTextBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.Label1 = new System.Windows.Forms.Label();
            this.dlgSave = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // txtValidMessage
            // 
            this.txtValidMessage.Location = new System.Drawing.Point(0, 0);
            this.txtValidMessage.Name = "txtValidMessage";
            this.txtValidMessage.Size = new System.Drawing.Size(538, 233);
            this.txtValidMessage.TabIndex = 0;
            this.txtValidMessage.Text = "";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(202, 241);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(61, 17);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "...";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // Label1
            // 
            this.Label1.Location = new System.Drawing.Point(10, 241);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(192, 17);
            this.Label1.TabIndex = 2;
            this.Label1.Text = "点击保存";
            // 
            // Validation_Message
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(450, 268);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtValidMessage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Validation_Message";
            this.Text = "日志";
            this.TopMost = true;
            this.Closed += new System.EventHandler(this.Validation_Message_Closed);
            this.ResumeLayout(false);

		}
		
#endregion
		

		private string m_cValidationMessage;
		private string m_cMessageFilename;
		private Motherform frmMotherform;
		/// <summary>
        /// 保存按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void btnSave_Click(System.Object sender, System.EventArgs e)
		{
			try
			{
				dlgSave.CheckFileExists = false;
				dlgSave.CheckPathExists = true;
				dlgSave.DefaultExt = "txt";
				dlgSave.Filter = "日志文件 (*.txt)|*.txt";
				dlgSave.AddExtension = true;
				dlgSave.InitialDirectory = System.IO.Path.GetDirectoryName(m_cMessageFilename);
				dlgSave.OverwritePrompt = true;
				dlgSave.CreatePrompt = false;
				if (dlgSave.ShowDialog() == DialogResult.OK)
				{
					m_cMessageFilename = dlgSave.FileName;
				}
				NewTextFile();
			}
			catch (Exception ex)
			{
				MessageBox.Show("保存日志信息出错:"+ex.Message);
				ProjectData.EndApp();
			}
			this.Close();
			this.Dispose();
		}
		/// <summary>
        /// 将日志信息写入已有的文件中
        /// </summary>
        /// <returns></returns>
		private object NewTextFile()
		{
			FileStream objFile = default(FileStream);
			StreamWriter objWriter = default(StreamWriter);
			string[] tempArray = null;
			int i = 0;
			
			try
			{
				objFile = File.OpenWrite(m_cMessageFilename);
				objWriter = new StreamWriter(objFile);
				tempArray = txtValidMessage.Lines;
				for (i = 0; i <= tempArray.GetUpperBound(0); i++)
				{
					objWriter.WriteLine(tempArray[i]);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("写入日志文件出错:"+ex.Message);
			}
			finally
			{
				objWriter.Close();
				objFile.Close();
			}
			return null;
		}
        /// <summary>
        /// 日志信息关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void Validation_Message_Closed(object sender, System.EventArgs e)
		{
			frmMotherform.MinimizeWindow2();
		}
		
		
	}
	
}
