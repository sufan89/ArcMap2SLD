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

//####################################################################################################################
//*******************ArcGIS_SLD_Converter*****************************************************************************
//*******************Class: XMLHandle*********************************************************************************
//*******************AUTHOR: Albrecht Weiser, University of applied Sciences in Mainz, Germany 2005*******************
//The Application was part of my Diploma thesis:**********************************************************************
//"Transforming map-properties of maps in esri-data to an OGC-conformous SLD-document, for publishing the ArcGIS-map *
//with an OGC- conformous map-server"*********************************************************************************
//ABSTRACT:
//The program so called "ArcGIS-map to SLD Converter" analyses an
//ArcMap-Project with respect to its symbolisation and assembles an SLD
//for the OGC-Web Map Service (WMS) from the gathered data. The program
//is started parallel to a running ArcMap 9.X-session. Subsequently the
//application deposits an SLD-file which complies the symbolisation of
//the available ArcMap-project. With the SLD a WMS-project may be
//classified and styled according to the preceding ArcMap-project. The
//application is written in VisualBasic.NET and uses the .NET 2.0
//Framework (plus XML files for configuration). For more informtion
//refer to:
//http://arcmap2sld.geoinform.fh-mainz.de/ArcMap2SLDConverter_Eng.htm.
//LICENSE:
//This program is free software under the license of the GNU Lesser General Public License (LGPL) As published by the Free Software Foundation.
//With the use and further development of this code you accept the terms of LGPL. For questions of the License refer to:
//http://www.gnu.org/licenses/lgpl.html
//DISCLAIMER:
//THE USE OF THE SOFTWARE ArcGIS-map to SLD Converter HAPPENS AT OWN RISK.
//I CANNOT ISSUE A GUARANTEE FOR ANY DISADVANTAGES (INCLUDING LOSS OF DATA; ETC.) THAT
//MAY ARISE FROM USING THIS SOFTWARE.
//DESCRIPTION:
//The class mainly consists of some public functions that depend to a current xml-file (the sld). There is always a pointer
//on the last created Node. (one for the last element: "m_objActiveNode" and one for the last attribute: "m_objActiveNode")
//If a calling function wants to create a new element or attribute it calls the function CreateElement or CreateAttribute.
//The parameter for the call is a trivial name of the element that wants to be created (trivial names, the according OGC-
//Names and the relating XPath devices can be found in the file LUT_sld_mapping_file.xml). This function handoff the trivial name
// to the function "NavigateElement". NavigateElement looks in the LUT_sld_mapping_file.xml for the according XPath-expression
//and with this expression navigates back in XML-hierarchy until the expression is true. Then the current element/attribute will set
//to the new activeNode and the new element/attribute can be created.
//CHANGES:
//08.06.2011: (ARIS) Added new flavor of SLD that does not refernce layer names (to be used with WorldMap).
//####################################################################################################################


namespace ArcGIS_SLD_Converter
{
	public class XMLHandle
	{
#region Membervariablen
		
		private const string c_strLUT_Standard = "LUT_sld_mapping_file.xml";
		private const string c_strLUT_WorldMap = "LUT_sld_WorldMap_mapping_file.xml";
		
		private const string c_strRootNodeName_Standard = "StyledLayerDescriptor";
		private const string c_strRootNodeName_WorldMap = "UserStyle";
		
		//Private m_objParent As Output_SLD               'Instanz der aufrufenden Klasse
		private string m_cXMLFilename; //Der ...\Pfad\Dateiname
		private XMLState m_enDocMode; //Der aktuelle Status des Dokuments (geschlossen, ge鰂fnet,...)
		private XmlDocument m_objDoc; //Das aktuelle Dokument
		private StringDictionary m_objNameDict; //Im Schl黶selfeld:Tag-Aliase aus Programmcode; im Datenfeld: ogc-Name des sld-Tags
		private StringDictionary m_objNamespaceDict; //Im Schl黶selfeld:Tag-Aliase aus Programmcode; im Datenfeld: der zum Namen geh鰎ende Namespacek黵zel
		private Hashtable m_objXPathDict; //Im Schl黶selfeld:Tag-Aliase aus Programmcode; Datenfeld:String-Collection mit zugeh鰎igen XPath-Ausdr點ken
		private XmlElement m_objRoot; //Der Wurzelknoten des xml-elements
		private short m_iLevelCount; //Die nullbasierte Anzahl der Ebenen des XML-Dokuments
		private XmlNode m_objActiveNode; //Der derzeit aktive Knoten
		private XmlAttribute m_objActiveAttribute; //Das derzeit aktive Attribut
		private string m_cXMLVersion;
		private string m_cSLDVersion;
		private string m_SLDXmlns;
		private string m_cXMLEncoding;
		private ArcGIS_SLD_Converter.Store2Fields m_objNamespaceURL; //Enth鋖t die Namespacelinks
		private XmlNamespaceManager m_objNSManager; //Enth鋖t die Namespaces und URL's
		private string m_sLUTFile; //The name of the LUT file
		private string m_sRootNodeName; //The name of the root node
		
#endregion

#region Enums
		
		//Der Status des XML-Handle wird hier festgelegt (ge鰂fnetes Dokument, geschlossen,....)
		private enum XMLState
		{
			xmlDocClosed = 0,
			xmlDocOpen = 1
		}
		
#endregion
		
#region Datenstrukturen
		
		
		
#endregion
		
		
		//##################################################################################################
		//########################################### ROUTINEN #############################################
		//##################################################################################################
		
#region Routinen
		
		//'f黵 schreibenden Zugriff
		//Public Sub New(ByVal Parent As Output_SLD)
		//    m_objParent = Parent
		//    start()
		//End Sub
		
		//'F黵 lesenden Zugriff
		//Public Sub New(ByVal FileName As String, ByVal Parent As Output_SLD)
		//    'm_objParent = Parent
		//    m_cXMLFilename = FileName
		//    start()
		//End Sub
		
		public XMLHandle(string FileName, bool bIncludeLayerNames)
		{
			//m_objParent = Parent
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
		
		public void start(bool bIncludeLayerNames)
		{
			
			ReadLUT();
			m_enDocMode = XMLState.xmlDocClosed;
			m_objDoc = new XmlDocument();
			m_iLevelCount = (short) 0;
			if (!(m_cXMLFilename == ""))
			{
				OpenDoc();
			}
			
		}
		
		//************************************************************************************************
		//READ ACCESS
		//Die Routine 鰂fnet ein vorhandenes XML-Dokument mit dem 黚ergebenen Filename und setzt den
		//XML-Status auf ge鰂fnet. Der Zeiger sitzt auf dem Root-Element
		//************************************************************************************************
		public void OpenDoc()
		{
			try
			{
				if (m_enDocMode == XMLState.xmlDocClosed)
				{
					if (File.Exists(m_cXMLFilename) == true)
					{
						if (!(m_cXMLFilename == ""))
						{
							m_objDoc = new XmlDocument();
							m_objDoc.Load(m_cXMLFilename);
							m_objRoot = m_objDoc.DocumentElement;
							if (m_objRoot == null)
							{
								throw (new Exception("Das Dokument ist leer. Bitte w鋒len Sie eine existierende, g黮tige XML-Datei aus"));
							}
							m_enDocMode = XMLState.xmlDocOpen;
						}
						else
						{
							throw (new Exception("Es wurde noch kein Dateiname/Speicherort f黵 das XML-Dokument angegeben"));
						}
					}
				}
				else
				{
					MessageBox.Show("Das Dokument ist schon ge鰂fnet!");
					return;
				}
			}
			catch (Exception ex)
			{
				ErrorMsg("Konnte XML-Dokument nicht 鰂fnen", ex.Message, ex.StackTrace, "OpenDoc");
			}
		}
		
#endregion
		
		
		//##################################################################################################
		//######################################### PROPERTIES #############################################
		//##################################################################################################
		
#region Properties
		
public string XMLFilename
		{
			get
			{
				return m_cXMLFilename;
			}
			set
			{
				m_cXMLFilename = value;
			}
		}
		
public XmlElement GetRoot
		{
			get
			{
				m_iLevelCount = (short) 0;
				return m_objRoot;
			}
		}
		
public short LevelNumber
		{
			get
			{
				return m_iLevelCount;
			}
		}
		
#endregion
		
		
		//##################################################################################################
		//######################################### FUNKTIONEN #############################################
		//##################################################################################################
		
#region Read-Write-Functions
		
		//************************************************************************************************
		//READ-ACCESS XPath/DOM
		//Hier sollte nach M鰃lichkeit nichts ver鋘dert werden !!!!!
		//Die Zentrale Navigationsfunktion. Das Prinzip: der gerade aktive Knoten wird mit einem eingelesenen
		//XPath-Ausdruck verglichen. Es wird solange in der Hierarchie nach hinten (zum Elternelement) navigiert,
		//bis der Ausdruck stimmt.Ist das der Fall, ist die richtige Ebene erreich,um das neue Element anzulegen
		//Parameter: AliasTagName=   der programminterne Aliasname (zum Vergleich des ogc-Tagnamens aus
		//                           der LUT)
		//************************************************************************************************
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
		
		
		
		//************************************************************************************************
		//WRITE-ACCESS DOM
		//Legt einen neuen Kindknoten f黵 den gerade aktiven Knoten an
		//Parameter: TagName =   der Name des zu schreibenden Knotens (TagName ist nur Vergleichswert.
		//                       Wahrer Wert wird aus der LUT genommen)
		//************************************************************************************************
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
		
		
		//************************************************************************************************
		//WRITE-ACCESS DOM
		//F黦t Text in den gerade aktiven Knoten ein
		//Parameter: InnerText =   der Text, der in dem Element stehen soll
		//************************************************************************************************
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
		
		
		//************************************************************************************************
		//WRITE-ACCESS DOM
		//Erstellt ein neues Attribut in dem aktiven Knoten
		//Parameter: AttributeName = das Attribut, das an den aktiven Knoten angeh鋘gt werden soll
		//************************************************************************************************
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
		
		
		//************************************************************************************************
		//WRITE-ACCESS DOM
		//Erstellt ein neues Attribut in dem aktiven Knoten
		//AttributeValue: AttributeValue = der Attributwert (Text) des aktiven Attributs
		//************************************************************************************************
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
		
		
		//************************************************************************************************
		//READ-ACCESS DOM-basierte Parsing-Funktion
		//Parst durch das ge鰂fnete XML-Dokument-ab dem Knoten,der 黚ergeben wird. Rekursiver Aufruf!!
		//Parameter:     CurrentNode - der Knoten, ab dem die Suche begonnen werden soll
		//************************************************************************************************
		// VBConversions Note: Former VB static variables moved to class level because they aren't supported in C#.
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
		
		
		
		
		//********************************************************************************************
		//WRITE-ACCESS
		// Erstellen einer neuen XML-Datei
		// ---------------------------------------------------------------
		// Voraussetzung : Der Dateiname muss bekannt sein (Property "XMLfilename")
		// Parameter :   OverWrite = Soll bestehende Datei 黚erschrieben werden (Ja / Nein)
		// R點kgabewerte:
		//       true = Ausf黨rung korrekt; Datei wurde erstellt
		//       false = Bei der Ausf黨rung trat ein Fehler auf
		// Zeiger sitzt auf Root-Knoten
		//********************************************************************************************
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
		
		//'************************************************************************************************
		//'Legt Eigenschaften f黵 den Root-Knoten fest
		//'************************************************************************************************
		//Public Function SetRootAttribute(ByVal AttributeName As String, ByVal AttributeValue As String) As Boolean
		//    Dim objRootAttrib As XmlAttribute
		//    Try
		//        If m_enDocMode = XMLState.xmlDocOpen Then
		//            objRootAttrib = m_objDoc.CreateAttribute(AttributeName)
		//            m_objRoot.Attributes.Append(objRootAttrib)
		//            objRootAttrib.Value = AttributeValue
		//        End If
		//    Catch ex As Exception
		//        ErrorMsg("Fehler beim Anlegen des RootAttributes", ex.Message, ex.StackTrace, SetRootAttribute)
		//    End Try
		//End Function
		
#endregion
		
		
#region Hilfsfunktionen
		
		//******************************************************************************************************
		//Read-Access DOM
		//Liest die LUT-Datei f黵 die SLD-Tagumwandlungen ein und speichert die Elemente in der StringDictionary
		//******************************************************************************************************
		private bool ReadLUT()
		{
			string cFilename = "";
			XmlDocument objLUTDoc = default(XmlDocument); //Das doc der LUT-XML f黵 die sld-Tags
			XmlElement objRoot = default(XmlElement);
			XmlElement objNode = default(XmlElement);
			XmlElement objNode2 = default(XmlElement);
			XmlElement objNode3 = default(XmlElement);
			StringCollection objXPathExp = default(StringCollection); //Die Sammlung der Xpath-Ausdr點ke
			m_objNameDict = new StringDictionary();
			m_objNamespaceDict = new StringDictionary();
			m_objXPathDict = new Hashtable();
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
						//Die Unterelemente des Knotens sldSyntax werden ausgelesen und abgespeichert
						if (objNode.Name == "sldSyntax")
						{
                            objNode2 = objNode.FirstChild as XmlElement;
							while (!(objNode2 == null))
							{
								m_objNameDict.Add(objNode2.Name, objNode2.GetAttribute("ogcTag"));
								m_objNamespaceDict.Add(objNode2.Name, objNode2.GetAttribute("Namespace"));
								objXPathExp = new StringCollection();
                                objNode3 = objNode2.FirstChild as XmlElement;
								//Die Kindelemente mit den XPath-Ausdr點ken
								while (!(objNode3 == null))
								{
									objXPathExp.Add(objNode3.InnerText);
                                    objNode3 = objNode3.NextSibling as XmlElement;
								}
								m_objXPathDict.Add(objNode2.Name, objXPathExp);
                                objNode2 = objNode2.NextSibling as XmlElement;
							}
							//Die Unterelemente des Knotens "sldConfiguration" werden ausgelesen und abgespeichert
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
		//******************************************************************************************************
		//Gibt den im String-Dictionary eingelesenen Tagnamen zur點k nach einem Vergleich mit dem Trivialnamen
		//PARAMETER:     Value = AliasTagName
		//******************************************************************************************************
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
		
		
		//******************************************************************************************************
		//Gibt den im String-Dictionary eingelesenen Namespacek黵zel zur點k nach einem Vergleich mit dem Trivialnamen
		//PARAMETER:     Value = AliasTagName
		//******************************************************************************************************
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
					throw (new Exception("Leider ist dieser Namensraumk黵zel " + Value + " noch nicht in die LUT aufgenommen"));
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
		
		
		//******************************************************************************************************
		//Gibt den im String-Dictionary eingelesenen NamespaceURL zur點k nach einem Vergleich mit dem Namespacek黵zel
		//PARAMETER:     Value = Namespacek黵zel
		//******************************************************************************************************
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
					throw (new Exception("Leider ist dieser URL " + System.Convert.ToString(Value) + " noch nicht in die LUT aufgenommen"));
					return "ERROR:" + System.Convert.ToString(Value);
				}
				return cRightTag;
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Beziehen der Namensraum-URL", ex.Message, ex.StackTrace, "GetNamespaceURL");
			}
			return "";
		}
		
		
		//************************************************************************************************
		//**********************************Die zentrale Fehlermeldung************************************
		//************************************************************************************************
		private object ErrorMsg(string message, string exMessage, string stack, string functionname)
		{
			MessageBox.Show(message + "." + "\r\n" + exMessage + "\r\n" + stack, "ArcGIS_SLD_Converter | XMLHandle | " + functionname, MessageBoxButtons.OK, MessageBoxIcon.Error);
			//WriteToFile()
			MyTermination();
			return null;
		}
		
		
		//************************************************************************************************
		//**********************************Die zentrale Infomeldung************************************
		//************************************************************************************************
		private object InfoMsg(string message, string functionname)
		{
			MessageBox.Show(message, "ArcGIS_SLD_Converter | XMLHandle | " + functionname, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return null;
		}
		
		
		//'************************************************************************************************
		//'*************************************gibt die letzte fehlerhafte SLD aus************************
		//'************************************************************************************************
		//Private Function WriteToFile()
		//    Dim dummy As XmlComment
		//    Try
		//        If Not m_objDoc Is Nothing Then
		//            dummy = m_objDoc.CreateComment("DIESE SLD IST DIE LETZTE FEHLERHAFTE SLD, DIE VOM PROGRAMM AUSGEGEBEN WURDE")
		//            m_objDoc.AppendChild(dummy)
		//            m_objDoc.Save(AppDomain.CurrentDomain.BaseDirectory & "Fehler_SLD.sld")
		//        End If
		//    Catch ex As Exception
		//        'MyTermination()
		//    End Try
		//End Function
		
		
		//************************************************************************************************
		//*************************************** Speicher-Funktion **************************************
		//Speichert das aktuelle Dokument mit dem aktuellen Dateinamen
		//************************************************************************************************
		public bool SaveDoc()
		{
			try
			{
				m_objDoc.Save(m_cXMLFilename);
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim speichern der Datei", ex.Message, ex.StackTrace, "SaveDoc");
			}
			return default(bool);
		}
		
		
		//************************************************************************************************
		//*****************************     Beenden der Anwendung     ************************************
		//************************************************************************************************
		private object MyTermination()
		{
			ProjectData.EndApp();
			//oder: application.exit
			return null;
		}
#endregion
		
		
	}
	
}
