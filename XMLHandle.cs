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

namespace ArcGIS_SLD_Converter
{
	public class XMLHandle
	{
#region Membervariablen
		
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

		private StringDictionary m_objNameDict;

		private StringDictionary m_objNamespaceDict; 

		private Hashtable m_objXPathDict; 
        /// <summary>
        /// 根节点要素
        /// </summary>
		private XmlElement m_objRoot; 
        /// <summary>
        /// 节点级别
        /// </summary>
		private short m_iLevelCount; 
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

		private ArcGIS_SLD_Converter.Store2Fields m_objNamespaceURL; 

		private XmlNamespaceManager m_objNSManager; 

		private string m_sLUTFile; 
        /// <summary>
        /// 跟节点名称
        /// </summary>
		private string m_sRootNodeName; 
		
#endregion

#region Enums
		/// <summary>
        /// XML文档状态
        /// </summary>
		private enum XMLState
		{
			xmlDocClosed = 0,
			xmlDocOpen = 1
		}

#endregion
		

		
#region Routinen
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
			m_iLevelCount = (short) 0;
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
						if (!string.IsNullOrEmpty(m_cXMLFilename))
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
						else
						{
							throw (new Exception("文件路径不正确"));
						}
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
		public bool NavigateElement(string AliasTagName)
		{
			XPathNavigator objNav = default(XPathNavigator); //Das Navigatorobjekt, mit dem im Dokument navigiert wird
			StringCollection objXPathColl = default(StringCollection); //Die XPath-Ausdr點ke, die den ogc-Tag repr鋝entieren welcher durch den AliasTagName benannt wird
			XmlNodeList objNodelist = default(XmlNodeList);
			XmlNode objEvalNode = default(XmlNode);
			XmlNode objTempNode = default(XmlNode);
			XmlNode objTempNode2 = default(XmlNode);
			short i = 0;
			short j;
			short iInsurance = 0;
			iInsurance = (short) 0;
			i = (short) 0;
			bool bSwitch; //Der Flag steuert die do while und wird dann true, wenn der knoten, der mit dem XPath-Ausdruck 黚ereinstimmt gefunden wurde
			bSwitch = false;
			
			try
			{
				if (m_objXPathDict.ContainsKey(AliasTagName))
				{
                    objXPathColl = (StringCollection)m_objXPathDict[AliasTagName];
					objTempNode = m_objActiveNode; //Aktiver Knoten
					objNav = objTempNode.CreateNavigator();
					while (!(bSwitch == true))
					{
						//Wenn mehr als 1 Knoten auf der Ebene gibt, m黶sen alle Knoten getestet werden
						if (objTempNode.ParentNode.ChildNodes.Count > 1)
						{
							objTempNode2 = objTempNode;
							//Die Do-until Schleife testet alle Geschwisterknoten des gerade aktiven Knotens
							while (!(objTempNode == null))
							{
								for (i = 0; i <= objXPathColl.Count - 1; i++) //L鋟ft solange es XPathausdr點ke zum auswerten gibt
								{
									objNodelist = m_objActiveNode.SelectNodes(objXPathColl[i], m_objNSManager); //alle in Frage kommenden Knoten
									objEvalNode = objNodelist[objNodelist.Count - 1]; //Es muss immer zum letzten Knoten gegangen werden, da die Datei seriell geschrieben wird
									//Wenn die Bedingung erf黮lt ist, wird der objTempNode (der ja der richtige Knoten ist) zum aktiven Knoten
									if ((objEvalNode == objTempNode) == true)
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
						else //Wenn es nur einen Knoten auf der Ebene gibt
						{
							for (i = 0; i <= objXPathColl.Count - 1; i++) //L鋟ft solange es XPathausdr點ke zum auswerten gibt
							{
								objNodelist = m_objActiveNode.SelectNodes(objXPathColl[i], m_objNSManager); //alle in Frage kommenden Knoten
								objEvalNode = objNodelist[objNodelist.Count - 1]; //Es muss immer zum letzten Knoten gegangen werden, da die Datei seriell geschrieben wird
								//Wenn die Bedingung erf黮lt ist, wird der objTempNode (der ja der richtige Knoten ist) zum aktiven Knoten
								if ((objEvalNode == objTempNode) == true)
								{
									m_objActiveNode = objTempNode;
									bSwitch = true;
									return true;
								}
							}
						}
						
						//Wenn alle XPathausdr點ke nicht zutrafen, wird zum Elternknoten navigiert
						if (!(objNav.Matches("/"))) //Hier die Navigation r點kw鋜ts im Tree
						{
							objNav.MoveToParent();
							objTempNode = objTempNode.ParentNode;
						}
						//Dient lediglich zur Sicherheit, da die do while ewig laufen kann, wenn Bedingung nicht erf黮lt ist
						iInsurance++;
						if (iInsurance > 100) //Keiner nimmt 100 passende XPath-Ausdr點ke in die LUT auf!
						{
							throw (new Exception("Kein g黮tiger XPathausdruck f黵 \'" + AliasTagName + "\' gefunden - Sicherheitsabbruch"));
						}
					}
				}
				else
				{
					throw (new Exception("Die Datei " + m_sLUTFile + " enth鋖t den Tag-Alias \'" + AliasTagName + "\' nicht."));
				}
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("Der XPath-Ausdruck \'" + AliasTagName + "\' in der LUT-Datei stimmt nicht; oder Navigieren nicht m鰃lich.", ex.Message, ex.StackTrace, "NavigateElement");
				return false;
			}
		}
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
					m_objActiveNode = objNode; //Der gerade gemachte Knoten wird aktiv gesetzt
					return true;
				}
			}
			catch (Exception ex)
			{
				ErrorMsg("Konnte Elementknoten nicht erstellen.", ex.Message, ex.StackTrace, "CreateElement");
				return false;
			}
			return default(bool);
		}
		public bool SetElementText(string InnerText)
		{
			try
			{
				m_objActiveNode.InnerText = InnerText;
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("Konnte Elementtext nicht schreiben.", ex.Message, ex.StackTrace, "SetElementText");
				return false;
			}
		}
		public bool CreateAttribute(string AttributeName)
		{
			XmlAttribute objXmlAttribute = default(XmlAttribute);
			try
			{
				objXmlAttribute = m_objDoc.CreateAttribute(AttributeName);
				m_objActiveNode.Attributes.Append(objXmlAttribute);
				m_objActiveAttribute = objXmlAttribute; //Das gerade gemachte Attribut wird aktiv gesetzt
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("Konnte Attribut nicht erstellen", ex.Message, ex.StackTrace, "CreateAttribute");
				return false;
			}
		}
		public bool SetAttributeValue(string AttributeValue)
		{
			try
			{
				m_objActiveAttribute.Value = AttributeValue;
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("Konnte Attribut nicht erstellen", ex.Message, ex.StackTrace, "SetAttributeValue");
				return false;
			}
		}
		private short ParseDoc_iLevelCount = 0;
		public bool ParseDoc(XmlElement CurrentNode)
		{
			XmlElement objNode = default(XmlElement); //Der jeweilige Kindknoten
			// static short iLevelCount = 0; VBConversions Note: Static variable moved to class level and renamed ParseDoc_iLevelCount. Local static variables are not supported in C#. //Durch die Deklaration als static vergisst die Variable ihren Wert nicht bei Verlassen der Funktion
			ParseDoc_iLevelCount++; //Anzahl der Levels im XML-Dok ab dem CurrentNode
			
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
								if (objNode.FirstChild is XmlElement) //Auch InnerText oder Attribute ist ein ChildNode
								{
									ParseDoc(objNode); //Rekursion
								}
							}
                            objNode = objNode.NextSibling as XmlElement;
						}
					}
				}
				else
				{
					MessageBox.Show("Das Dokument ist noch nicht ge鰂fnet");
					return default(bool);
				}
				m_iLevelCount = ParseDoc_iLevelCount;
				ParseDoc_iLevelCount = (short) 0;
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("Das Dokument ist unbrauchbar", ex.Message, ex.StackTrace, "ParseDoc");
                return false;
			}
		}
		public bool CreateNewFile(bool OverWrite, bool blnIncludeLayerNames)
		{
			XmlDeclaration objDeclare = default(XmlDeclaration);
			short i = 0;
			short j = 0;
			string cNamePre = "";
			
			try
			{
				if (File.Exists(m_cXMLFilename) == true)
				{
					if (OverWrite == false)
					{
						throw (new Exception("Fehler beim Anlegen der XML-Datei (Datei besteht bereits)"));
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
					//' ARIS: standard SLD
					CreateAttribute("version");
					SetAttributeValue(m_cSLDVersion);
				}
				else
				{
					//' ARIS: WorldMap SLD
					CreateAttribute("xmlns");
					SetAttributeValue(m_SLDXmlns);
				}
				//Hier werden die Namespaces geschrieben
				for (i = 0; i <= m_objNamespaceURL.Count - 1; i++)
				{
					CreateAttribute("xmlns" + ":" + m_objNamespaceURL.get_GetString1ByIndex(i));
					SetAttributeValue(m_objNamespaceURL.get_GetString2ByIndex(i));
				}
				
				objDeclare = m_objDoc.CreateXmlDeclaration(m_cXMLVersion, m_cXMLEncoding, "yes"); //Version muss z.Zt. 1.0 sein!
				m_objDoc.InsertBefore(objDeclare, m_objRoot);
				SaveDoc(); //Speichert das aktuelle Dokument mit dem aktuellen Dateinamen
				m_enDocMode = XMLState.xmlDocOpen;
				//Der Namespacemanager, der n鰐ig ist, f黵 die XPath-Navigation mit Namespacepr鋐ixen
				m_objNSManager = new XmlNamespaceManager(m_objDoc.NameTable);
				for (j = 0; j <= m_objNamespaceURL.Count - 1; j++)
				{
					m_objNSManager.AddNamespace(m_objNamespaceURL.get_GetString1ByIndex(j), m_objNamespaceURL.get_GetString2ByIndex(j));
				}
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Anlegen der XML-Datei (" + ex.Message.ToString() + ")", ex.Message, ex.StackTrace, "CreateNewFile");
				return false;
			}
		}
#endregion
		
		
#region Hilfsfunktionen
        /// <summary>
        /// 读取XML文档配置信息
        /// </summary>
        /// <returns></returns>
		private bool ReadLUT()
		{
			string cFilename = "";
			XmlDocument objLUTDoc = default(XmlDocument); 
			XmlElement objRoot = default(XmlElement);
			XmlElement objNode = default(XmlElement);
			XmlElement objNode2 = default(XmlElement);
			XmlElement objNode3 = default(XmlElement);
			StringCollection objXPathExp = default(StringCollection); 
			m_objNameDict = new StringDictionary();
			m_objNamespaceDict = new StringDictionary();
			m_objXPathDict = new Hashtable();
            //命名空间
			m_objNamespaceURL = new ArcGIS_SLD_Converter.Store2Fields();
			
			try
			{
				cFilename = AppDomain.CurrentDomain.BaseDirectory + m_sLUTFile;
				if (File.Exists(cFilename))
				{
					objLUTDoc = new XmlDocument();
					objLUTDoc.Load(cFilename);
					objRoot = objLUTDoc.DocumentElement;
                    objNode = objRoot.FirstChild as XmlElement;
					while (!(objNode == null))
					{
					
						if (objNode.Name == "sldSyntax")
						{
                            objNode2 = objNode.FirstChild as XmlElement;
							while (!(objNode2 == null))
							{
								m_objNameDict.Add(objNode2.Name, objNode2.GetAttribute("ogcTag"));
								m_objNamespaceDict.Add(objNode2.Name, objNode2.GetAttribute("Namespace"));
								objXPathExp = new StringCollection();
                                objNode3 = objNode2.FirstChild as XmlElement;
								
								while (!(objNode3 == null))
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
							while (!(objNode2 == null))
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
										while (!(objNode3 == null))
										{
											m_objNamespaceURL.Add2Strings(objNode3.Name, objNode3.InnerText);
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
				ErrorMsg("Die Datei " + m_sLUTFile + " muss im Anwendungsverzeichnis stehen. Bitte kopieren SIe die Datei an diese Stelle und starten Sie die Anwendung erneut.", ex.Message, ex.StackTrace, "ReadLUT");
                return false;
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim 鰂fnen der Konfigurationsdatei", ex.Message, ex.StackTrace, "ReadLUT");
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
				if (m_objNameDict.ContainsKey(Value) == true)
				{
					cRightTag = m_objNameDict[Value];
				}
				else
				{
					throw (new Exception("Der Tag " + Value + " ist noch nicht in die LUT-Datei aufgenommen"));
					return "ERROR:" + Value;
				}
				return cRightTag;
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Beziehen der OGC-Tagnames", ex.Message, ex.StackTrace, "GetOGCName");
			}
			return "";
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
					return "ERROR:" + Value;
				}
				return cRightTag;
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Beziehen der Namensraumk黵zel", ex.Message, ex.StackTrace, "GetNamespacePrefix");
			}
			
			return "";
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
				if (m_objNamespaceURL.get_ContainsString1(System.Convert.ToString(Value)) == true)
				{
					cRightTag = m_objNamespaceURL.get_GetString2ForString1(System.Convert.ToString(Value));
				}
				else
				{
					return "ERROR:" + System.Convert.ToString(Value);
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
			MessageBox.Show(message + "." + "\r\n" + exMessage + "\r\n" + stack, "ArcGIS_SLD_Converter | XMLHandle | " + functionname, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        /// <summary>
        /// 关闭程序
        /// </summary>
        /// <returns></returns>
		private void MyTermination()
		{
			ProjectData.EndApp();
		}
#endregion
	}
	
}
