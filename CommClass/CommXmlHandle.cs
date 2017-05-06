using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ArcGIS_SLD_Converter
{
    public static class CommXmlHandle
    {
        public const string c_strLUT_Standard = "LUT_sld_mapping_file.xml";

        public const string c_strLUT_WorldMap = "LUT_sld_WorldMap_mapping_file.xml";
        /// <summary>
        /// 标准根节点名称
        /// </summary>
        private const string c_strRootNodeName_Standard = "StyledLayerDescriptor";
        /// <summary>
        /// 世界地图根节点名称
        /// </summary>
		private const string c_strRootNodeName_WorldMap = "UserStyle";
        /// <summary>
        /// OGC名称
        /// </summary>
        public static Dictionary<string, string> m_dicOGCName { get; set; }
        /// <summary>
        /// 命名空间
        /// </summary>
        public static Dictionary<string, string> m_dicNamespace { get; set; }
        /// <summary>
        /// 节点路径
        /// </summary>
        public static Dictionary<string, IList<string>> m_dicPath { get; set; }
        /// <summary>
        /// 命名空间URL
        /// </summary>
        public static Dictionary<string, string> m_dicNameUrl { get; set; }
        /// <summary>
        /// XML文档版本信息
        /// </summary>
        public static string m_xmlVersion { get; set; }
        /// <summary>
        /// sld文档版本信息
        /// </summary>
        public static string m_sldVersion { get; set; }
        /// <summary>
        /// XML文档编码信息
        /// </summary>
        public static string m_xmlEncoding { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public static string m_SLDXmlns { get; set; }
        /// <summary>
        /// 读取XML文档配置信息
        /// </summary>
        /// <returns></returns>
        public static bool ReadLUT(string assemblyPath,string strLutFile)
        {
            string cFilename = "";
            XmlDocument objLUTDoc;
            XmlElement objRoot;
            XmlElement objNode;
            XmlElement objNode2;
            XmlElement objNode3;
            IList<string> objXPathExp;
            m_dicOGCName = new Dictionary<string, string>();
            m_dicNamespace = new Dictionary<string, string>();
            m_dicPath = new Dictionary<string, IList<string>>();
            //命名空间
            m_dicNameUrl = new Dictionary<string, string>();
            try
            {
                //获取程序集所在的文件夹
                cFilename = assemblyPath + "\\TempXmlDoc\\" + strLutFile;
                if (File.Exists(cFilename))
                {
                    objLUTDoc = new XmlDocument();
                    objLUTDoc.Load(cFilename);
                    objRoot = objLUTDoc.DocumentElement;
                    objNode = objRoot.FirstChild as XmlElement;
                    while (objNode != null)
                    {
                        if (objNode.Name == "sldSyntax")
                        {
                            objNode2 = objNode.FirstChild as XmlElement;
                            while (objNode2 != null)
                            {
                                m_dicOGCName.Add(objNode2.Name, objNode2.GetAttribute("ogcTag"));
                                m_dicNamespace.Add(objNode2.Name, objNode2.GetAttribute("Namespace"));
                                objXPathExp = new List<string>();
                                objNode3 = objNode2.FirstChild as XmlElement;
                                while (objNode3 != null)
                                {
                                    objXPathExp.Add(objNode3.InnerText);
                                    objNode3 = objNode3.NextSibling as XmlElement;
                                }
                                m_dicPath.Add(objNode2.Name, objXPathExp);
                                objNode2 = objNode2.NextSibling as XmlElement;
                            }

                        }
                        else if (objNode.Name == "sldConfiguration")
                        {
                            objNode2 = objNode.FirstChild as XmlElement;
                            while (objNode2 != null)
                            {
                                switch (objNode2.Name)
                                {
                                    case "xmlVersion":
                                        m_xmlVersion = objNode2.InnerText;
                                        break;
                                    case "xmlEncoding":
                                        m_xmlEncoding = objNode2.InnerText;
                                        break;
                                    case "Namespaces":
                                        objNode3 = objNode2.FirstChild as XmlElement;
                                        while (objNode3 != null)
                                        {
                                            m_dicNameUrl.Add(objNode3.Name, objNode3.InnerText);
                                            objNode3 = objNode3.NextSibling as XmlElement;
                                        }
                                        break;
                                    case "sldVersion":
                                        m_sldVersion = objNode2.InnerText;
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
                    throw (new FileNotFoundException(cFilename));
                    return false;
                }
                return true;
            }
            catch (FileNotFoundException ex)
            {
                ptLogManager.WriteMessage(string.Format("配置文件{0}不存在:{1}{2} 方法名称:ReadLUT", strLutFile, Environment.NewLine, ex.Message));
                return false;
            }
            catch (Exception ex)
            {
                ptLogManager.WriteMessage(string.Format("读取SLD配置信息出错:{0},{1}{2}方法名称:ReadLUT", Environment.NewLine, ex.Message, ex.StackTrace));
                return false;
            }
        }
        /// <summary>
        /// 新建一个XML文档
        /// </summary>
        /// <param name="OverWrite">是否重写</param>
        /// <param name="blnIncludeLayerNames">是否包含图层名称</param>
        /// <returns></returns>
        public static XmlDocument CreateNewFile(string strFileName,bool OverWrite, bool blnIncludeLayerNames)
        {
            XmlDeclaration objDeclare = default(XmlDeclaration);
            XmlDocument newDoc = null;
            string cNamePre = "";
            string RootNodeName = "";
            if (blnIncludeLayerNames)
            {
                RootNodeName = c_strRootNodeName_Standard;
            }
            else
            {
                RootNodeName = c_strRootNodeName_WorldMap;
            }
            try
            {
                if (File.Exists(strFileName))
                {
                    if (OverWrite == false)
                    {
                        throw (new Exception("当前XML文件已存在"));
                        return newDoc;
                    }
                    File.Delete(strFileName);
                }
                cNamePre = GetNamespacePrefix(RootNodeName);
                newDoc = new XmlDocument();
                XmlElement RootElement = newDoc.CreateElement(cNamePre, GetOGCName(RootNodeName), GetNamespaceURL(cNamePre));
                newDoc.AppendChild(RootElement);
                if (blnIncludeLayerNames)
                {
                    //标准SLD文件
                   XmlAttribute rootAttribute= CreateAttribute("version", RootElement, newDoc);
                   SetAttributeValue(m_xmlVersion,rootAttribute);
                }
                else
                {
                    //世界地图SLD
                    XmlAttribute rootAttribute=CreateAttribute("xmlns", RootElement, newDoc);
                    SetAttributeValue(m_SLDXmlns, rootAttribute);
                }
                //写入XML命名空间
                foreach (string key in m_dicNameUrl.Keys)
                {
                    XmlAttribute tempAttribute= CreateAttribute("xmlns" + ":" + key, RootElement, newDoc);
                    SetAttributeValue(m_dicNameUrl[key], tempAttribute);
                }

                objDeclare = newDoc.CreateXmlDeclaration(m_xmlVersion, m_xmlEncoding, "yes"); //XML版本和XML编码规则
                newDoc.InsertBefore(objDeclare, RootElement);
                SaveDoc(newDoc, strFileName); //保存新建的文档
                //XML命名空间管理器
                XmlNamespaceManager  m_objNSManager = new XmlNamespaceManager(newDoc.NameTable);
                foreach (string key in m_dicNamespace.Keys)
                {
                    m_objNSManager.AddNamespace(key, m_dicNamespace[key]);
                }
                return newDoc;
            }
            catch (Exception ex)
            {
                ptLogManager.WriteMessage(string.Format("新建XML文档失败:{0}{1}{2}{3}",Environment.NewLine, ex.Message,Environment.NewLine,ex.StackTrace));
                return newDoc;
            }
        }
        private static string GetNamespacePrefix(string Value)
        {
            string cRightTag = "";
            try
            {
                if (m_dicNamespace.ContainsKey(Value) == true)
                {
                    cRightTag = m_dicNamespace[Value];
                }
                else
                {
                    return cRightTag;
                }
                return cRightTag;
            }
            catch (Exception ex)
            {
                ptLogManager.WriteMessage(string.Format("获取XML命名空间出错:{0}{1}{2}{3}",Environment.NewLine,ex.Message,Environment.NewLine,ex.StackTrace));
            }

            return cRightTag;
        }
        /// <summary>
        /// 获取OGC名称
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        private static string GetOGCName(string Value)
        {
            string cRightTag = "";
            try
            {
                if (m_dicOGCName.ContainsKey(Value))
                {
                    cRightTag = m_dicOGCName[Value];
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
                ptLogManager.WriteMessage(string.Format("获取OGC名称出错:{0}{1};{2};{3};GetOGCName", ex.Message,Environment.NewLine, ex.StackTrace,Environment.NewLine));
            }
            return cRightTag;
        }
        /// <summary>
        /// 获取命名空间URL
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        private static string GetNamespaceURL(string Value)
        {
            string cRightTag = "";
            try
            {
                if (string.IsNullOrEmpty(Value)) return cRightTag;
                if (m_dicNameUrl.ContainsKey(Value.ToString()))
                {
                    cRightTag = m_dicNameUrl[Value];
                }
                else
                {
                    return cRightTag;
                }
                return cRightTag;
            }
            catch (Exception ex)
            {
                ptLogManager.WriteMessage(string.Format("获取命名空间失败:{0}{1}{2}{3}",Environment.NewLine,ex.Message,Environment.NewLine,ex.StackTrace));
            }
            return cRightTag;
        }
        /// <summary>
        ///  创建指定元素
        /// </summary>
        /// <param name="AliasTagName"></param>
        /// <returns></returns>
        public static XmlElement CreateElement(string AliasTagName,XmlDocument XmlDoc)
        {
            XmlElement objNode = default(XmlElement);
            string cNamespacePrefix = "";
            string cNamespaceURL = "";
            try
            {
                    cNamespacePrefix = GetNamespacePrefix(AliasTagName);
                    cNamespaceURL = GetNamespaceURL(cNamespacePrefix);
                    objNode = XmlDoc.CreateElement(cNamespacePrefix, GetOGCName(AliasTagName), cNamespaceURL);
                    return objNode;
               
            }
            catch (Exception ex)
            {
                ptLogManager.WriteMessage(string.Format("创建XML元素出错:{0}{1}{2}{3}", Environment.NewLine, ex.Message, Environment.NewLine, ex.StackTrace));
                return objNode;
            }
            return objNode;
        }
        public static XmlElement CreateElementAndSetElemnetText(string TageName, XmlDocument XmlDoc, string ElementText)
        {
            XmlElement objNode = default(XmlElement);
            string cNamespacePrefix = "";
            string cNamespaceURL = "";
            try
            {
                cNamespacePrefix = GetNamespacePrefix(TageName);
                cNamespaceURL = GetNamespaceURL(cNamespacePrefix);
                objNode = XmlDoc.CreateElement(cNamespacePrefix, GetOGCName(TageName), cNamespaceURL);
                SetElementText(ElementText, objNode);
                return objNode;

            }
            catch (Exception ex)
            {
                ptLogManager.WriteMessage(string.Format("创建XML元素出错:{0}{1}{2}{3}", Environment.NewLine, ex.Message, Environment.NewLine, ex.StackTrace));
                return objNode;
            }
            return objNode;
        }
        /// <summary>
        /// 设置XML要素内容
        /// </summary>
        /// <param name="InnerText"></param>
        /// <returns></returns>
		public static bool SetElementText(string InnerText,XmlElement xmlElementNode)
        {
            try
            {
                xmlElementNode.InnerText = InnerText;
                return true;
            }
            catch (Exception ex)
            {
                ptLogManager.WriteMessage(string.Format("设置XML要素内容出错:{0}{1}{2}{3}", Environment.NewLine, ex.Message, Environment.NewLine, ex.StackTrace));
                return false;
            }
        }
        /// <summary>
        /// 创建指定属性
        /// </summary>
        /// <param name="AttributeName"></param>
        /// <returns></returns>
		public static XmlAttribute CreateAttribute(string AttributeName,XmlElement xmlElementNode,XmlDocument xmlDoc)
        {
            XmlAttribute objXmlAttribute = default(XmlAttribute);
            try
            {
                objXmlAttribute = xmlDoc.CreateAttribute(AttributeName);
                xmlElementNode.Attributes.Append(objXmlAttribute);
                return objXmlAttribute;
            }
            catch (Exception ex)
            {
                ptLogManager.WriteMessage(string.Format("创建指定属性出错:{0}{1}{2}{3}", Environment.NewLine, ex.Message, Environment.NewLine, ex.StackTrace));
                return objXmlAttribute;
            }
        }
        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <param name="AttributeValue"></param>
        /// <returns></returns>
		public static bool SetAttributeValue(string AttributeValue, XmlAttribute xmlAttribute)
        {
            try
            {
                xmlAttribute.Value = AttributeValue;
                return true;
            }
            catch (Exception ex)
            {
                ptLogManager.WriteMessage(string.Format("设置属性值出错:{0}{1}{2}{3}",Environment.NewLine,ex.Message,Environment.NewLine,ex.StackTrace));
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool SaveDoc(XmlDocument xmlDoc,string strFileName)
        {
            try
            {
                xmlDoc.Save(strFileName);
            }
            catch (Exception ex)
            {
                ptLogManager.WriteMessage(string.Format("文件保存失败:{0}{1}{2}{3}", Environment.NewLine, ex.Message, Environment.NewLine, ex.StackTrace));
            }
            return true;
        }
    }
}
