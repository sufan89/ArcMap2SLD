using System;
using System.Drawing;
using System.Diagnostics;
using System.Data;
using Microsoft.VisualBasic;
using System.Collections;
using System.Windows.Forms;
using System.Xml;
using System.IO;
namespace ArcGIS_SLD_Converter
{
	public class CommonXMLHandle
	{
		private string cXMLFileName;

		private XmlDocument objDOC;
        /// <summary>
        /// XML打开状态
        /// </summary>
		private bool bDocIsOpen;

		private XmlElement objActiveRoot;

		private XmlElement objActiveSection;

		private XmlElement objActiveEntry;

		private XmlElement objActiveSubEntry;

		private XmlAttribute objActiveAttribute;

		private XmlAttribute objActiveSubentryAttribute;

		private string cMessage;
        /// <summary>
        /// XML状态
        /// </summary>
		private XmlMode DocMode;
		/// <summary>
        /// XML状态
        /// </summary>
		private enum XmlMode
		{
			xmlDocClosed = 0,
			xmlDocOpen = 1,
			xmlSectionSelected = 2,
			xmlEntrySelected = 3,
			xmlRootSelected = 4
		}
		
		public CommonXMLHandle()
		{
			bDocIsOpen = false;
			DocMode = XmlMode.xmlDocClosed;
		}
		
		/// <summary>
        /// XML文件名称
        /// </summary>
        public string XMLfilename
		        {
			        get
			        {
				        return cXMLFileName;
			        }
			        set
			        {
				        cXMLFileName = value;
			        }
		        }
		/// <summary>
        /// 打开文档
        /// </summary>
        /// <returns></returns>
		public bool OpenDoc()
		{
			try
			{
				objDOC = new XmlDocument();
				objDOC.Load(cXMLFileName);
				bDocIsOpen = true;
				DocMode = XmlMode.xmlDocOpen;
			}
			catch (Exception ex)
			{
				cMessage = "打开XML文档 (" + ex.Message.ToString() + ")";
				return false;
			}
			return true;
		}
		/// <summary>
        /// 保存XML文档
        /// </summary>
        /// <returns></returns>
		public bool SaveDoc()
		{
			try
			{
				objDOC.Save(cXMLFileName);
			}
			catch (Exception ex)
			{
				cMessage = "保存XML文档(" + ex.Message.ToString() + ")";
				return false;
			}
			return true;
		}
		/// <summary>
        /// 根据节点名称获取根节点
        /// </summary>
        /// <param name="RootName"></param>
        /// <returns></returns>
		public bool FindRoot(string RootName)
		{
			if (bDocIsOpen == false)
			{
				cMessage = "XML文档未打开";
				return false;
			}
			XmlNode objNodeRoot = objDOC.FirstChild;
			if (objNodeRoot is XmlDeclaration)
			{
				objNodeRoot = objDOC.SelectSingleNode(RootName);
			}
			
			while (!(objNodeRoot == null))
			{
				if (objNodeRoot.Name.ToUpper() == RootName.ToUpper())
				{
                    objActiveRoot = objNodeRoot as XmlElement;
					DocMode = XmlMode.xmlRootSelected;
					cMessage = "";
					return true;
				}
				objNodeRoot = objDOC.NextSibling;
			}
			cMessage = "无法获取指定根节点";
			return false;
			
		}
        /// <summary>
        /// 查找Section
        /// </summary>
        /// <param name="Section"></param>
        /// <returns></returns>
		public bool FindSection(string Section)
		{
			if (bDocIsOpen == false)
			{
                cMessage = "XML文档未打开";
				return false;
			}
			int i = 0;
			XmlNode objNodeSection ;
			if (objActiveRoot == null)
			{
                objActiveRoot = objDOC.FirstChild as XmlElement;
			}
			objNodeSection = objActiveRoot.FirstChild;
			while (!(objNodeSection == null))
			{
				if (objNodeSection.Name.ToUpper() == Section.ToUpper())
				{
                    objActiveSection = objNodeSection as XmlElement;
					DocMode = XmlMode.xmlSectionSelected;
					cMessage = "";
					return true;
				}
				objNodeSection = objNodeSection.NextSibling;
			}
			cMessage = "Sektion 查询出错";
			return false;
		}
		/// <summary>
        /// 查找指定节点
        /// </summary>
        /// <param name="Entry"></param>
        /// <returns></returns>
		public bool FindEntry(string Entry)
		{
			if (DocMode == XmlMode.xmlDocClosed | DocMode == XmlMode.xmlDocOpen)
			{
				cMessage = "XML文档不可用";
				return false;
			}
			XmlNode objNodeEntry = objActiveSection.FirstChild;
			while (!(objNodeEntry == null))
			{
				if (objNodeEntry.Name.ToUpper() == Entry.ToUpper())
				{
					objActiveEntry = objNodeEntry as XmlElement;
					DocMode = XmlMode.xmlEntrySelected;
					cMessage = "";
					return true;
				}
				objNodeEntry = objNodeEntry.NextSibling;
			}
			cMessage = "获取指定节点错误";
			return false;
			
		}
        /// <summary>
        /// 获取当前活动节点的内容
        /// </summary>
        /// <returns></returns>
		public string getEntryValue()
		{
			try
			{
				cMessage = "";
				return objActiveEntry.InnerText;
			}
			catch (Exception ex)
			{
				cMessage = "(" + ex.Message.ToString() + ")";
				return "";
			}
		}
        /// <summary>
        /// 获取选择节点内容
        /// </summary>
        /// <returns></returns>
		public string getSectionValue()
		{
			try
			{
				cMessage = "";
				return objActiveSection.InnerText;
			}
			catch (Exception ex)
			{
				cMessage = " (" + ex.Message.ToString() + ")";
				return "";
			}
		}
        /// <summary>
        /// 设置选择节点值
        /// </summary>
        /// <param name="NewValue"></param>
        /// <returns></returns>
		public bool SetSectionValue(string NewValue)
		{
			try
			{
				cMessage = "";
				objActiveSection.InnerText = NewValue;
				this.SaveDoc();
			}
			catch (Exception ex)
			{
				cMessage = " (" + ex.Message.ToString() + ")";
				return false;
			}
			cMessage = "";
			return true;
		}
        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="NewValue"></param>
        /// <returns></returns>
		public bool SetEntryValue(string NewValue)
		{
			try
			{
				cMessage = "";
				objActiveEntry.InnerText = NewValue;
				this.SaveDoc();
			}
			catch (Exception ex)
			{
				cMessage = "(" + ex.Message.ToString() + ")";
				return false;
			}
			cMessage = "";
			return true;
		}
        /// <summary>
        /// 创建新节点
        /// </summary>
        /// <param name="Entry"></param>
        /// <returns></returns>
		public bool CreateEntry(string Entry)
		{
			try
			{
				XmlElement objNewEntry = objDOC.CreateElement(Entry);
				objActiveSection.AppendChild(objNewEntry);
				objActiveEntry = objNewEntry;
				this.SaveDoc();
			}
			catch (Exception ex)
			{
				cMessage = " (" + ex.Message.ToString() + ")";
				return false;
			}
			cMessage = "";
			return true;
		}
        /// <summary>
        /// 获取当前活动节点子节点数组
        /// </summary>
        /// <returns></returns>
		public ArrayList GetAllEntryValues()
		{
			ArrayList Al = new ArrayList();
			XmlNode oNode;
			try
			{
				if (!(objActiveSection == null))
				{
					oNode = objActiveSection.FirstChild;
					while (!(oNode == null))
					{
						Al.Add(oNode.InnerText);
						oNode = oNode.NextSibling;
					}
				}
				else
				{
					cMessage = "";
					return default(ArrayList);
				}
				Al.Sort();
                return Al;
			}
			catch (Exception)
			{
				cMessage = "";
			}
			return default(ArrayList);
		}
        /// <summary>
        /// 获取当前活动节点
        /// </summary>
        /// <param name="SubEntry"></param>
        /// <returns></returns>
		public bool FindSubEntry(string SubEntry)
		{
			if (!(DocMode == XmlMode.xmlEntrySelected))
			{
				cMessage = "当前状态不正确";
				return false;
			}
			XmlNode objNodeSubEntry = objActiveEntry.FirstChild;
			while (!(objNodeSubEntry == null))
			{
				if (objNodeSubEntry.Name.ToUpper() == SubEntry.ToUpper())
				{
                    objActiveSubEntry = objNodeSubEntry as XmlElement;
					DocMode = XmlMode.xmlEntrySelected;
					cMessage = "";
					return true;
				}
				objNodeSubEntry = objNodeSubEntry.NextSibling;
			}
			cMessage = "";
			return false;
			
		}
	    /// <summary>
        /// 获取当前活动节点内容
        /// </summary>
        /// <returns></returns>
		public string GetSubentryValue()
		{
			try
			{
				if (!(objActiveSubEntry == null))
				{
					cMessage = "";
					return objActiveSubEntry.InnerText;
				}
			}
			catch (Exception)
			{
				cMessage = "获取当前活动节点内容出错";
			}
			return "";
		}
	}
	
}
