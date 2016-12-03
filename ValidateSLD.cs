using System;
using System.Drawing;
using System.Diagnostics;
using System.Data;
using Microsoft.VisualBasic;
using System.Collections;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace ArcGIS_SLD_Converter
{
	public class ValidateSLD
	{
		/// <summary>
        /// 转换结果
        /// </summary>
		private string m_cResultSLD; 
        /// <summary>
        /// 转换模板文件
        /// </summary>
		private string m_cSchemaXSD; 
        /// <summary>
        /// 主窗体对象
        /// </summary>
		private Motherform frmMotherform; 

		private string m_cValidatMessage; 

		private string m_cAltValidatMessage; 

		private bool m_bValidatMessage; 
		private bool m_bValid; 
		private bool m_bFirstError; 
		private int m_iLinenumber;
		private string m_cNodeType;
		private string m_cNodeName;
		private string m_cNodePosition;
		
        /// <summary>
        /// 转换进度提示
        /// </summary>
        /// <returns></returns>
		private bool CentralProcessing()
		{
		
			frmMotherform.CHLabelTop("检查SLD文档格式");
			frmMotherform.CHLabelBottom("正在处理");
			frmMotherform.CHLabelSmall("可能会花费一点时间");
			//显示图片
			frmMotherform.ShowWorld();
			string[] args = new string[] {m_cResultSLD, m_cSchemaXSD};

			Run(args);
            //隐藏图片
			frmMotherform.HideWorld();

			frmMotherform.CHLabelBottom("处理完成");
			if (m_bValid == false)
			{
				CreateValidationMessage();
				frmMotherform.CHLabelSmall("SLD文件不可用");
				
				
			}
			else
			{
			  frmMotherform.CHLabelSmall("SLD文件可以使用");
			}
			return default(bool);
		}
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="Mother"></param>
		public ValidateSLD(Motherform Mother)
		{
			frmMotherform = Mother;

			m_cResultSLD = Mother.GetSLDFilename;
			m_cSchemaXSD = Mother.GetXSDFilename;

			m_bValidatMessage = false;

			m_bValid = true;

			m_bFirstError = false;

			CentralProcessing();
		}
		
		
        /// <summary>
        /// 运行函数，参数1为SLD结果路径，参数2为模板文件名
        /// </summary>
        /// <param name="args"></param>
		public void Run(string[] args)
		{
			XmlValidatingReader reader=null;
			XmlSchemaCollection xsc = new XmlSchemaCollection();
			
			try
			{
				xsc.Add(null, new XmlTextReader(args[1]));
				reader = new XmlValidatingReader(new XmlTextReader(args[0]));

				IXmlLineInfo lineInfo = (IXmlLineInfo) reader; 
			
				ValidationEventHandler valdel = new ValidationEventHandler(ValidationEvent);
				
				reader.ValidationEventHandler += valdel;
				reader.Schemas.Add(xsc);
				reader.ValidationType = ValidationType.Schema;
				while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.Element)
					{
						while (reader.MoveToNextAttribute())
						{
							m_iLinenumber = Convert.ToInt16(lineInfo.LineNumber.ToString());
							m_cNodePosition = lineInfo.LinePosition.ToString();
							m_cNodeType = reader.NodeType.ToString();
							m_cNodeName = reader.Name.ToString();
						}
					}
				}
				
			}
			catch (XmlSchemaException e)
			{
					m_cValidatMessage = m_cValidatMessage + "XML 模板错误" + "\r\n" + "\r\n";
					m_cValidatMessage = m_cValidatMessage + "加载的XML不可用 " + "\r\n" + "后语为加载正确的命名空间, 错误信息: " + "\r\n" + e.Message + "\r\n" + "行: " + Convert.ToString(e.LinePosition) + "\r\n";
					m_cValidatMessage = m_cValidatMessage + "\r\n" + "\r\n";
					m_bValid = false;
			}
			catch (XmlException e)
			{
					m_cValidatMessage = m_cValidatMessage + "XML-SYNTAX EXCEPTION!" + "\r\n" + "\r\n";
					m_cValidatMessage = m_cValidatMessage + "the file comprises an XML-Syntax error. " + "\r\n" + e.Message;
					m_cValidatMessage = m_cValidatMessage + "\r\n" + "\r\n";
					m_bValid = false;
			}
			catch (Exception e)
			{
					m_cValidatMessage = m_cValidatMessage + "SERIOUS EXCEPTION!" + "\r\n" + "\r\n";
					m_cValidatMessage = m_cValidatMessage + "Unknown error while validating SLD" + "\r\n" + e.Message;
					m_cValidatMessage = m_cValidatMessage + e.StackTrace + "\r\n";
					m_cValidatMessage = m_cValidatMessage + "\r\n" + "\r\n";
					m_bValid = false;
			}
			finally
			{
				if (!(reader == null))
				{
					reader.Close();
				}
			}
		}
        /// <summary>
        /// 开始事件
        /// </summary>
        /// <param name="errorid"></param>
        /// <param name="args"></param>
		private void ValidationEvent(object errorid, ValidationEventArgs args)
		{
			m_bValid = false;
		
			if (m_bFirstError == false)
			{
				WriteMessageHeader();
			}
			
			m_cValidatMessage = m_cValidatMessage + "Validierungs-Meldung: " + "\r\n" + args.Message + "\r\n";
			
			if (args.Severity == XmlSeverityType.Warning)
			{
				m_cValidatMessage = m_cValidatMessage + "未找到模板文件" + "\r\n" + "\r\n";
			}
			else if (args.Severity == XmlSeverityType.Error)
			{
				m_cValidatMessage = m_cValidatMessage + "在SLD模板中未找到节点: " + m_cNodeType + "\r\n";
			}
			
			if (args.Exception != null) 
			{
			
					m_cValidatMessage = m_cValidatMessage + "节点 " + m_cNodeType + " \'" + m_cNodeName + "\' " + " 在行: " + Convert.ToString(m_iLinenumber) + " , 位置: " + m_cNodePosition + "\r\n";
				
			}
			m_cValidatMessage = m_cValidatMessage + "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~" + "\r\n";
		}
		/// <summary>
        /// 开始前的提示信息
        /// </summary>
		private void WriteMessageHeader()
		{
			m_bFirstError = true;
			m_cValidatMessage = m_cValidatMessage + "无 SLD 模板文件  " + "\r\n";
			m_cValidatMessage = m_cValidatMessage + m_cSchemaXSD;
			m_cValidatMessage = m_cValidatMessage + " 检查模板文件是否存在. " + "\r\n";
			m_cValidatMessage = m_cValidatMessage + "导入一个新的目标文件 " + "\r\n";
			m_cValidatMessage = m_cValidatMessage + "确定SLD文件是否可用。" + "\r\n";
			m_cValidatMessage = m_cValidatMessage + "\r\n" + "\r\n";
		
		}
        /// <summary>
        /// 显示日志信息框
        /// </summary>
		private void CreateValidationMessage()
		{
			m_bValidatMessage = true;
			m_bValid = false;
            Validation_Message m_frmValMess = new Validation_Message(m_cValidatMessage, frmMotherform);
			m_frmValMess.Visible = true;
			m_frmValMess.Show();
			m_frmValMess.Activate();
		}
		
	}
	
}
