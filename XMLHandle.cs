using System;
using System.Drawing;
using System.Diagnostics;
using System.Data;
using Microsoft.VisualBasic;
using System.Collections;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Collections.Specialized;
using System.Xml.XPath;
using Microsoft.VisualBasic.CompilerServices;
using System.Collections.Generic;

namespace ArcGIS_SLD_Converter
{
	public class XMLHandle
	{
        #region 全局变量
		
		private const string c_strLUT_Standard = "LUT_sld_mapping_file.xml";

		private const string c_strLUT_WorldMap = "LUT_sld_WorldMap_mapping_file.xml";
		/// <summary>
        /// 标准根节点名称
        /// </summary>
		private const string c_strRootNodeName_Standard = "StyledLayerDescriptor";
        /// <summary>
        /// 世界地图根节点名称
        /// </summary>
		private const string c_strRootNodeName_WorldMap = "UserStyle";
	    /// <summary>
        /// 保存的XML文件名称
        /// </summary>
		private string m_cXMLFilename; 
        /// <summary>
        /// 当前XML文档状态
        /// </summary>
		private XMLState m_enDocMode; 
        /// <summary>
        /// 全局处理XML文档对象
        /// </summary>
		private XmlDocument m_objDoc; 

		private Dictionary<string,string> m_objNameDict;

        private Dictionary<string, string> m_objNamespaceDict; 

		private Dictionary<string,IList<string>> m_objXPathDict; 
        /// <summary>
        /// 根节点要素
        /// </summary>
		private XmlElement m_objRoot; 
        /// <summary>
        /// 节点级别
        /// </summary>
		private int m_iLevelCount; 
        /// <summary>
        /// 当前活动节点
        /// </summary>
		private XmlNode m_objActiveNode; 
        /// <summary>
        /// 当前活动节点属性
        /// </summary>
		private XmlAttribute m_objActiveAttribute; 
        /// <summary>
        /// XML版本
        /// </summary>
		private string m_cXMLVersion;
        /// <summary>
        /// SLD版本
        /// </summary>
		private string m_cSLDVersion;

		private string m_SLDXmlns;
        /// <summary>
        /// XML编码规则
        /// </summary>
		private string m_cXMLEncoding;

		//private ArcGIS_SLD_Converter.Store2Fields m_objNamespaceURL;

        private Dictionary<string, string> m_objNamespaceURL;
        /// <summary>
        /// XML命名管理
        /// </summary>
		private XmlNamespaceManager m_objNSManager; 

		private string m_sLUTFile; 
        /// <summary>
        /// 跟节点名称
        /// </summary>
		private string m_sRootNodeName; 
		
        #endregion

        #region 枚举变量
		/// <summary>
        /// XML文档状态
        /// </summary>
		private enum XMLState
		{
			xmlDocClosed = 0,
			xmlDocOpen = 1
		}
        #endregion

        #region 方法
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="FileName">SLD文件名称</param>
        /// <param name="bIncludeLayerNames">是否包含图层名称</param>
		public XMLHandle(string FileName, bool bIncludeLayerNames)
		{
			m_cXMLFilename = FileName;

			if (bIncludeLayerNames)
			{
				m_sLUTFile = c_strLUT_Standard;
				m_sRootNodeName = c_strRootNodeName_Standard;
			}
			else
			{
				m_sLUTFile = c_strLUT_WorldMap;
				m_sRootNodeName = c_strRootNodeName_WorldMap;
			}

			start(bIncludeLayerNames);
		}
		/// <summary>
        /// 开始执行
        /// </summary>
        /// <param name="bIncludeLayerNames"></param>
		public void start(bool bIncludeLayerNames)
		{
			
			ReadLUT();
			m_enDocMode = XMLState.xmlDocClosed;
			m_objDoc = new XmlDocument();
			m_iLevelCount = 0;
			if (!string.IsNullOrEmpty(m_cXMLFilename))
			{
				OpenDoc();
			}
			
		}
        /// <summary>
        /// 打开文档
        /// </summary>
		public void OpenDoc()
		{
			try
			{
				if (m_enDocMode == XMLState.xmlDocClosed)
				{
					if (File.Exists(m_cXMLFilename))
					{
							m_objDoc = new XmlDocument();
							m_objDoc.Load(m_cXMLFilename);
							m_objRoot = m_objDoc.DocumentElement;
							if (m_objRoot == null)
							{
								throw (new Exception("打开文档不正确！"));
							}
							m_enDocMode = XMLState.xmlDocOpen;
					}
				}
				else
				{
					MessageBox.Show("打开XML文档错误");
					return;
				}
			}
			catch (Exception ex)
			{
				ErrorMsg("打开XML文档错误", ex.Message, ex.StackTrace, "OpenDoc");
			}
		}
        #endregion
        #region 读写函数
        /// <summary>
        /// 创建导航节点
        /// </summary>
        /// <param name="AliasTagName"></param>
        /// <returns></returns>
		public bool NavigateElement(string AliasTagName)
		{
			XPathNavigator objNav; 
			StringCollection objXPathColl; 
			XmlNodeList objNodelist;
			XmlNode objEvalNode ;
			XmlNode objTempNode ;
			XmlNode objTempNode2 ;
			int  iInsurance = 0;
			bool bSwitch = false;
            try
			{
				if (m_objXPathDict.ContainsKey(AliasTagName))
				{
                    objXPathColl = (StringCollection)m_objXPathDict[AliasTagName];
					objTempNode = m_objActiveNode; 
					objNav = objTempNode.CreateNavigator();
					while (!bSwitch)
					{
						if (objTempNode.ParentNode.ChildNodes.Count > 1)
						{
							objTempNode2 = objTempNode;
							while (!(objTempNode == null))
							{
								for (int i = 0; i <= objXPathColl.Count - 1; i++) 
								{
									objNodelist = m_objActiveNode.SelectNodes(objXPathColl[i], m_objNSManager); 
									objEvalNode = objNodelist[objNodelist.Count - 1];
									if (objEvalNode == objTempNode)
									{
										m_objActiveNode = objTempNode;
										bSwitch = true;
										return true;
									}
								}
								objTempNode = objTempNode.PreviousSibling;
							}
							objTempNode = objTempNode2;
						}
						else 
						{
							for (int i = 0; i <= objXPathColl.Count - 1; i++) 
							{
								objNodelist = m_objActiveNode.SelectNodes(objXPathColl[i], m_objNSManager); 
								objEvalNode = objNodelist[objNodelist.Count - 1];
								if (objEvalNode == objTempNode)
								{
									m_objActiveNode = objTempNode;
									bSwitch = true;
									return true;
								}
							}
						}
						if (!(objNav.Matches("/"))) 
						{
							objNav.MoveToParent();
							objTempNode = objTempNode.ParentNode;
						}
						iInsurance++;
						if (iInsurance > 100) 
						{
							throw (new Exception(AliasTagName + "节点数过多"));
						}
					}
				}
				else
				{
					throw (new Exception( m_sLUTFile + " " + AliasTagName));
				}
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg(AliasTagName , ex.Message, ex.StackTrace, "NavigateElement");
				return false;
			}
		}
        /// <summary>
        ///  创建指定元素
        /// </summary>
        /// <param name="AliasTagName"></param>
        /// <returns></returns>
        public bool CreateElement(string AliasTagName)
		{
			XmlElement objNode = default(XmlElement);
			string cNamespacePrefix = "";
			string cNamespaceURL = "";
			try
			{
				if (NavigateElement(AliasTagName) == true)
				{
					cNamespacePrefix = GetNamespacePrefix(AliasTagName);
					cNamespaceURL = GetNamespaceURL(cNamespacePrefix);
					objNode = m_objDoc.CreateElement(cNamespacePrefix, GetOGCName(AliasTagName), cNamespaceURL);
					m_objActiveNode.AppendChild(objNode);
					m_objActiveNode = objNode; 
					return true;
				}
			}
			catch (Exception ex)
			{
				ErrorMsg("创建XML元素出错", ex.Message, ex.StackTrace, "CreateElement");
				return false;
			}
			return false;
		}
        /// <summary>
        /// 设置XML要素内容
        /// </summary>
        /// <param name="InnerText"></param>
        /// <returns></returns>
		public bool SetElementText(string InnerText)
		{
			try
			{
				m_objActiveNode.InnerText = InnerText;
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("设置XML要素内容出错", ex.Message, ex.StackTrace, "SetElementText");
				return false;
			}
		}
        /// <summary>
        /// 创建指定属性
        /// </summary>
        /// <param name="AttributeName"></param>
        /// <returns></returns>
		public bool CreateAttribute(string AttributeName)
		{
			XmlAttribute objXmlAttribute = default(XmlAttribute);
			try
			{
				objXmlAttribute = m_objDoc.CreateAttribute(AttributeName);
				m_objActiveNode.Attributes.Append(objXmlAttribute);
				m_objActiveAttribute = objXmlAttribute; 
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("创建指定属性出错", ex.Message, ex.StackTrace, "CreateAttribute");
				return false;
			}
		}
        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="AttributeValue"></param>
        /// <returns></returns>
		public bool SetAttributeValue(string AttributeValue)
		{
			try
			{
				m_objActiveAttribute.Value = AttributeValue;
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("设置属性值出错", ex.Message, ex.StackTrace, "SetAttributeValue");
				return false;
			}
		}
		/// <summary>
        /// 复制XML文档
        /// </summary>
        /// <param name="CurrentNode"></param>
        /// <returns></returns>
		public bool ParseDoc(XmlElement CurrentNode)
		{
             int  ParseDoc_iLevelCount = 0;
			XmlElement objNode = default(XmlElement); 
			ParseDoc_iLevelCount++;
			try
			{
				if (m_enDocMode == XMLState.xmlDocOpen)
				{
					if (CurrentNode.HasChildNodes)
					{
                        objNode = CurrentNode.FirstChild as XmlElement;
						while (!(objNode == null))
						{
							if (objNode.HasChildNodes)
							{
								if (objNode.FirstChild is XmlElement) 
								{
									ParseDoc(objNode);
								}
							}
                            objNode = objNode.NextSibling as XmlElement;
						}
					}
				}
				else
				{
					MessageBox.Show("不能复制XML文档");
					return default(bool);
				}
				m_iLevelCount = ParseDoc_iLevelCount;
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("复制XML文档出错", ex.Message, ex.StackTrace, "ParseDoc");
                return false;
			}
		}
        /// <summary>
        /// 新建一个XML文档
        /// </summary>
        /// <param name="OverWrite">是否重写</param>
        /// <param name="blnIncludeLayerNames">是否包含图层名称</param>
        /// <returns></returns>
		public bool CreateNewFile(bool OverWrite, bool blnIncludeLayerNames)
		{
			XmlDeclaration objDeclare = default(XmlDeclaration);
			string cNamePre = "";
			
			try
			{
				if (File.Exists(m_cXMLFilename))
				{
					if (OverWrite == false)
					{
						throw (new Exception("当前XML文件已存在"));
						return false;
					}
					File.Delete(m_cXMLFilename);
				}

				cNamePre = GetNamespacePrefix(m_sRootNodeName);
				
				m_objDoc = new XmlDocument();
				m_objRoot = m_objDoc.CreateElement(cNamePre, GetOGCName(m_sRootNodeName), GetNamespaceURL(cNamePre));
				m_objDoc.AppendChild(m_objRoot);
				m_objActiveNode = m_objRoot;
				
				if (blnIncludeLayerNames)
				{
					//标准SLD文件
					CreateAttribute("version");
					SetAttributeValue(m_cSLDVersion);
				}
				else
				{
					//世界地图SLD
					CreateAttribute("xmlns");
					SetAttributeValue(m_SLDXmlns);
				}
				//写入XML命名空间
				foreach (string key in m_objNamespaceURL.Keys)
				{
                    CreateAttribute("xmlns" + ":" + key);
					SetAttributeValue(m_objNamespaceURL[key]);
				}
				
				objDeclare = m_objDoc.CreateXmlDeclaration(m_cXMLVersion, m_cXMLEncoding, "yes"); //XML版本和XML编码规则
				m_objDoc.InsertBefore(objDeclare, m_objRoot);
				SaveDoc(); //保存新建的文档
				m_enDocMode = XMLState.xmlDocOpen;
				//XML命名空间管理器
				m_objNSManager = new XmlNamespaceManager(m_objDoc.NameTable);
                foreach (string key in m_objNamespaceURL.Keys)
				{
                    m_objNSManager.AddNamespace(key, m_objNamespaceURL[key]);
				}
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("新建XML文档失败 (" + ex.Message + ")", ex.Message, ex.StackTrace, "CreateNewFile");
				return false;
			}
		}
        #endregion
		
		
#region 公共方法
        /// <summary>
        /// 读取XML文档配置信息
        /// </summary>
        /// <returns></returns>
		private bool ReadLUT()
		{
			string cFilename = "";
			XmlDocument objLUTDoc; 
			XmlElement objRoot ;
			XmlElement objNode ;
			XmlElement objNode2;
			XmlElement objNode3 ;
			IList<string> objXPathExp; 
			m_objNameDict = new Dictionary<string,string>();
            m_objNamespaceDict = new Dictionary<string, string>();
			m_objXPathDict = new Dictionary<string,IList<string>>();
            //命名空间
			m_objNamespaceURL = new Dictionary<string,string>();
			try
			{
                //获取程序集所在的文件夹
                string tempStr = System.IO.Path.GetDirectoryName(GetType().Assembly.Location);
                cFilename = tempStr +"\\"+ m_sLUTFile;
				if (File.Exists(cFilename))
				{
					objLUTDoc = new XmlDocument();
					objLUTDoc.Load(cFilename);
					objRoot = objLUTDoc.DocumentElement;
                    objNode = objRoot.FirstChild as XmlElement;
					while (objNode!= null)
					{
						if (objNode.Name == "sldSyntax")
						{
                            objNode2 = objNode.FirstChild as XmlElement;
							while (objNode2 != null)
							{
								m_objNameDict.Add(objNode2.Name, objNode2.GetAttribute("ogcTag"));
								m_objNamespaceDict.Add(objNode2.Name, objNode2.GetAttribute("Namespace"));
								objXPathExp = new List<string>();
                                objNode3 = objNode2.FirstChild as XmlElement;
								while (objNode3 != null)
								{
									objXPathExp.Add(objNode3.InnerText);
                                    objNode3 = objNode3.NextSibling as XmlElement;
								}
								m_objXPathDict.Add(objNode2.Name, objXPathExp);
                                objNode2 = objNode2.NextSibling as XmlElement;
							}
							
						}
						else if (objNode.Name == "sldConfiguration")
						{
                            objNode2 = objNode.FirstChild as XmlElement;
							while (objNode2!= null)
							{
								switch (objNode2.Name)
								{
									case "xmlVersion":
										m_cXMLVersion = objNode2.InnerText;
										break;
									case "xmlEncoding":
										m_cXMLEncoding = objNode2.InnerText;
										break;
									case "Namespaces":
                                        objNode3 = objNode2.FirstChild as XmlElement;
										while (objNode3 != null)
										{
											m_objNamespaceURL.Add(objNode3.Name, objNode3.InnerText);
                                            objNode3 = objNode3.NextSibling as XmlElement;
										}
										break;
									case "sldVersion":
										m_cSLDVersion = objNode2.InnerText;
										break;
									case "sldXMLNS":
										m_SLDXmlns = objNode2.InnerText;
										break;
								}
                                objNode2 = objNode2.NextSibling as XmlElement;
							}
						}
                        objNode = objNode.NextSibling as XmlElement;
					}
				}
				else
				{
                    MessageBox.Show(cFilename);
                    throw (new FileNotFoundException(cFilename));
					return false;
				}
				return true;
			}
			catch (FileNotFoundException ex)
			{
				ErrorMsg(m_sLUTFile, ex.Message, ex.StackTrace, "ReadLUT");
                return false;
			}
			catch (Exception ex)
			{
				ErrorMsg("读取SLD配置信息出错", ex.Message, ex.StackTrace, "ReadLUT");
                return false;
			}
		}
        /// <summary>
        /// 获取OGC名称
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
		private string GetOGCName(string Value)
		{
			string cRightTag = "";
			try
			{
				if (m_objNameDict.ContainsKey(Value))
				{
					cRightTag = m_objNameDict[Value];
				}
				else
				{
					throw (new Exception(Value + "不存在"));
					return cRightTag;
				}
				return cRightTag;
			}
			catch (Exception ex)
			{
				ErrorMsg("获取OGC名称出错", ex.Message, ex.StackTrace, "GetOGCName");
			}
			return cRightTag;
		}
        /// <summary>
        /// 获取XML命名空间前缀
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
		private string GetNamespacePrefix(string Value)
		{
			string cRightTag = "";
			try
			{
				if (m_objNamespaceDict.ContainsKey(Value) == true)
				{
					cRightTag = m_objNamespaceDict[Value];
				}
				else
				{
					return cRightTag;
				}
				return cRightTag;
			}
			catch (Exception ex)
			{
				ErrorMsg("获取XML命名空间出错", ex.Message, ex.StackTrace, "GetNamespacePrefix");
			}
			
			return cRightTag;
        }
        /// <summary>
        /// 获取命名空间URL
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
		private string GetNamespaceURL(object Value)
		{
			string cRightTag = "";
			try
			{
                if (Value == null) return cRightTag;
                if (m_objNamespaceURL.ContainsKey(Value.ToString()))
				{
					cRightTag = m_objNamespaceURL[Value.ToString()];
				}
				else
				{
					return cRightTag;
				}
				return cRightTag;
			}
			catch (Exception ex)
			{
				ErrorMsg("获取命名空间失败", ex.Message, ex.StackTrace, "GetNamespaceURL");
			}
			return "";
		}
        /// <summary>
        /// 错误信息，并关闭程序
        /// </summary>
        /// <param name="message">信息</param>
        /// <param name="exMessage"></param>
        /// <param name="stack"></param>
        /// <param name="functionname">方法名称</param>
        /// <returns></returns>
		private object ErrorMsg(string message, string exMessage, string stack, string functionname)
		{
			MessageBox.Show(message + "." + "\r\n" + exMessage + "\r\n" + stack, functionname, MessageBoxButtons.OK, MessageBoxIcon.Error);
			MyTermination();
			return null;
		}
        /// <summary>
        /// 保存文档
        /// </summary>
        /// <returns></returns>
		public bool SaveDoc()
		{
			try
			{
				m_objDoc.Save(m_cXMLFilename);
			}
			catch (Exception ex)
			{
				ErrorMsg("文件保存失败", ex.Message, ex.StackTrace, "SaveDoc");
			}
			return true;
		}
#endregion
	}
	
}
