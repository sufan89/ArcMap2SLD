// VBConversions Note: VB project level imports
using System;
using System.Drawing;
using System.Diagnostics;
using System.Data;
using Microsoft.VisualBasic;
using System.Collections;
using System.Windows.Forms;
// End of VB project level imports

using System.Xml;
using System.IO;

//#####################################################################################################################
//**************************************************Class: Common XML-Handle ******************************************
//********************AUTHOR: Erich Lieberum, UMGIS Informatik GmbH, Darmstadt, Germany 2005***************************
//********************In friendly abandonment for non commercial use***************************************************
//********************The class is ready for read/write use************************************************************
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
//The class provides XML-functions for a simple XML-doc with max. 3 hierarchy levels. The levels are called: 1.Section
//2. Entry, 3.Subentry. The Document Element is called "Root"
//CHANGES:
//#####################################################################################################################

namespace ArcGIS_SLD_Converter
{
	public class CommonXMLHandle
	{
		private string cXMLFileName;
		private XmlDocument objDOC;
		private bool bDocIsOpen;
		private XmlElement objActiveRoot;
		private XmlElement objActiveSection;
		private XmlElement objActiveEntry;
		private XmlElement objActiveSubEntry;
		private XmlAttribute objActiveAttribute;
		private XmlAttribute objActiveSubentryAttribute;
		private string cMessage;
		private XmlMode DocMode;
		
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
		
		// Name des XML-Dokuments
public string XMLfilename
		{
			get
			{
				string returnValue = "";
				returnValue = cXMLFileName;
				return returnValue;
			}
			set
			{
				cXMLFileName = value;
			}
		}
		
		// Auftretende Fehlermeldungen
public string ErrorMessage
		{
			get
			{
				string returnValue = "";
				returnValue = cMessage;
				return returnValue;
			}
		}
		
		// 謋fnen des XML-Dokuments
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
				cMessage = "Fehler beim 謋fnen des Dokuments (" + ex.Message.ToString() + ")";
				return false;
			}
			return true;
		}
		// Speichern des XML-Dokuments
		public bool SaveDoc()
		{
			try
			{
				objDOC.Save(cXMLFileName);
			}
			catch (Exception ex)
			{
				cMessage = "Fehler beim Speichern des Dokuments (" + ex.Message.ToString() + ")";
				return false;
			}
			return true;
		}
		
		//********************************************************************************************
		// Suchen einer Root-Knoten (0. Ebene)
		// -------------------------------
		// R點kgabewerte: True = Root-Knoten  wurde gefunden
		//                false = Root-Knoten  wurde nicht gefunden
		// Zeiger sitzt auf gefundenem Root-Knoten
		//********************************************************************************************
		public bool FindRoot(string RootName)
		{
			if (bDocIsOpen == false)
			{
				cMessage = "Dokument ist nicht ge鰂fnet!";
				return false;
			}
			int i = 0;
			
			XmlNode objNodeRoot = default(XmlNode);
			objNodeRoot = objDOC.FirstChild;
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
			cMessage = "Root-Knoten nicht vorhanden!";
			return false;
			
		}
		//********************************************************************************************
		// Suchen einer Sektion (1. Ebene)
		// -------------------------------
		// R點kgabewerte: True = Sektion wurde gefunden
		//                false = Sektion wurde nicht gefunden
		// Zeiger sitzt auf gefundenem Knoten
		//********************************************************************************************
		public bool FindSection(string Section)
		{
			if (bDocIsOpen == false)
			{
				cMessage = "Dokument ist nicht ge鰂fnet!";
				return false;
			}
			int i = 0;
			//Dim objRoot As XmlNode
			//objRoot = objDOC.FirstChild
			
			XmlNode objNodeSection = default(XmlNode);
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
			cMessage = "Sektion nicht vorhanden!";
			return false;
			
		}
		
		
		//********************************************************************************************
		// Suchen eines Sektions-Attributs (1. Ebene)
		// -------------------------------
		// R點kgabewerte: True = Attribut wurde gefunden
		//                false = Attribut wurde nicht gefunden
		// Zeiger sitzt auf gefundenem Attribut
		// Eine Sektion muss vorher ausgew鋒lt werden
		//********************************************************************************************
		public bool FindSectionAttribute(string SectionAttributeName)
		{
			if (!(DocMode == XmlMode.xmlSectionSelected))
			{
				cMessage = "Keine Eintrag ausgew鋒lt!";
				return false;
			}
			int i = 0;
			
			XmlAttribute objSectionAttribute = default(XmlAttribute);
			if (objActiveSection.Attributes.Count == 0)
			{
				cMessage = "Keine Attribute vorhanden!";
				return false;
			}
			objSectionAttribute = objActiveEntry.Attributes[0];
			foreach (XmlAttribute tempLoopVar_objSectionAttribute in objActiveSection.Attributes)
			{
				objSectionAttribute = tempLoopVar_objSectionAttribute;
				if (objSectionAttribute.Name.ToUpper() == SectionAttributeName.ToUpper())
				{
					objActiveAttribute = objSectionAttribute;
					DocMode = XmlMode.xmlSectionSelected;
					cMessage = "";
					return true;
				}
			}
			cMessage = "Attribut nicht vorhanden!";
			return false;
			
		}
		
		//********************************************************************************************
		// 躡erpr黤ung, ob 黚erhaupt irgendeine Sektion angelegt wurde
		// -------------------------------
		// R點kgabewerte: True = eine Sektion wurde gefunden
		//                false = keune Sektion wurde gefunden
		// Zeiger sitzt auf gefundenem Knoten
		//********************************************************************************************
		
		//********************************************************************************************
		public bool FindAnySection()
		{
			try
			{
				if (bDocIsOpen == false)
				{
					cMessage = "Dokument ist nicht ge鰂fnet!";
					return false;
				}
				if (objActiveRoot == null)
				{
                    objActiveRoot = objDOC.FirstChild as XmlElement;
				}
				if (objActiveRoot.FirstChild == null)
				{
					return false;
				}
				else
				{
                    objActiveSection = objActiveRoot.FirstChild as XmlElement;
					return true;
				}
                return false;
				
			}
			catch (Exception)
			{
				cMessage = "Fehler in Funktion FindAnySection";
                return false;
			}
		}
		
		
		
		// Suchen eines Eintrags (2. Ebene)
		// -------------------------------
		// R點kgabewerte: True = Eintrag wurde gefunden
		//                false = Eintrag wurde nicht gefunden
		// Zeiger sitzt auf gefundenem Knoten
		// Eine Sektion muss vorher ausgew鋒lt werden
		//********************************************************************************************
		public bool FindEntry(string Entry)
		{
			if (DocMode == XmlMode.xmlDocClosed | DocMode == XmlMode.xmlDocOpen)
			{
				cMessage = "Keine Sektion ausgew鋒lt!";
				return false;
			}
			int i = 0;
			
			XmlNode objNodeEntry = default(XmlNode);
			
			objNodeEntry = objActiveSection.FirstChild;
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
			cMessage = "Eintrag nicht vorhanden!";
			return false;
			
		}
		
		//********************************************************************************************
		// Suchen eines Attributs (2. Ebene)
		// -------------------------------
		// R點kgabewerte: True = Attribut wurde gefunden
		//                false = Attribut wurde nicht gefunden
		// Zeiger sitzt auf gefundenem Attribut
		// Ein Eintrag muss vorher ausgew鋒lt werden
		//********************************************************************************************
		public bool FindAttribute(string AttributeName)
		{
			if (!(DocMode == XmlMode.xmlEntrySelected))
			{
				cMessage = "Keine Eintrag ausgew鋒lt!";
				return false;
			}
			int i = 0;
			
			XmlAttribute objAttribute = default(XmlAttribute);
			if (objActiveEntry.Attributes.Count == 0)
			{
				cMessage = "Keine Attribute vorhanden!";
				return false;
			}
			objAttribute = objActiveEntry.Attributes[0];
			foreach (XmlAttribute tempLoopVar_objAttribute in objActiveEntry.Attributes)
			{
				objAttribute = tempLoopVar_objAttribute;
				if (objAttribute.Name.ToUpper() == AttributeName.ToUpper())
				{
					objActiveAttribute = objAttribute;
					DocMode = XmlMode.xmlEntrySelected;
					cMessage = "";
					return true;
				}
			}
			cMessage = "Attribut nicht vorhanden!";
			return false;
			
		}
		
		
		//********************************************************************************************
		// Abrufen eines Eintrags-Wertes
		// -----------------------------
		//********************************************************************************************
		public string getEntryValue()
		{
			try
			{
				cMessage = "";
				return objActiveEntry.InnerText;
			}
			catch (Exception ex)
			{
				cMessage = "Kein Wert gefunden (" + ex.Message.ToString() + ")";
				return "";
			}
		}
		//躡erladen:
		public string getEntryValue(string Entryname)
		{
			try
			{
				cMessage = "";
				FindEntry(Entryname);
				return objActiveEntry.InnerText;
			}
			catch (Exception ex)
			{
				cMessage = "Kein Wert gefunden (" + ex.Message.ToString() + ")";
				return "";
			}
		}
		
		//********************************************************************************************
		// Abrufen eines Eintragsnamens
		// ---------------------------------------
		//********************************************************************************************
		public string getEntryName()
		{
			try
			{
				cMessage = "";
				return objActiveEntry.Name;
			}
			catch (Exception ex)
			{
				cMessage = "Kein Name gefunden (" + ex.Message.ToString() + ")";
				return "";
			}
		}
		
		//********************************************************************************************
		// Abrufen eines Section-Namens
		// -----------------------------
		//********************************************************************************************
		public string getSectionName()
		{
			try
			{
				cMessage = "";
				return objActiveSection.Name;
			}
			catch (Exception ex)
			{
				cMessage = "Kein Name gefunden (" + ex.Message.ToString() + ")";
				return "";
			}
		}
		
		//********************************************************************************************
		// Abrufen eines Sektionswertes
		// aktive Sektion
		// ---------------------------------------
		//********************************************************************************************
		public string getSectionValue()
		{
			try
			{
				cMessage = "";
				return objActiveSection.InnerText;
			}
			catch (Exception ex)
			{
				cMessage = "Kein Name gefunden (" + ex.Message.ToString() + ")";
				return "";
			}
		}
		
		//********************************************************************************************
		// Abrufen eines Attributwertes
		// aktives Attribut
		// ---------------------------------------
		//********************************************************************************************
		public string getAttributeValue()
		{
			try
			{
				cMessage = "";
				return objActiveAttribute.Value;
			}
			catch (Exception ex)
			{
				cMessage = "Kein Wert gefunden (" + ex.Message.ToString() + ")";
				return "";
			}
		}
		//躡erladen:
		public string getAttributeValue(string AttributName)
		{
			try
			{
				cMessage = "";
				FindAttribute(AttributName);
				return objActiveAttribute.Value;
			}
			catch (Exception ex)
			{
				cMessage = "Kein Wert gefunden (" + ex.Message.ToString() + ")";
				return "";
			}
		}
		
		
		//********************************************************************************************
		// Abrufen eines Attributnamens
		// aktives Attribut
		// ---------------------------------------
		//********************************************************************************************
		public string getAttributeName()
		{
			try
			{
				cMessage = "";
				return objActiveAttribute.Name;
			}
			catch (Exception ex)
			{
				cMessage = "Kein Name gefunden (" + ex.Message.ToString() + ")";
				return "";
			}
		}
		
		//********************************************************************************************
		// Setzen eines Sektionswertes
		// aktiver Sektion
		// ---------------------------------------
		//********************************************************************************************
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
				cMessage = "Fehler beim Schreiben (" + ex.Message.ToString() + ")";
				return false;
			}
			cMessage = "";
			return true;
		}
		
		
		//********************************************************************************************
		// Setzen eines Eintragswertes
		// aktiver Eintrag
		// ---------------------------------------
		//********************************************************************************************
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
				cMessage = "Fehler beim Schreiben (" + ex.Message.ToString() + ")";
				return false;
			}
			cMessage = "";
			return true;
		}
		
		//********************************************************************************************
		// Setzen eines Attributwertes
		// aktives Attribut
		// ---------------------------------------
		//********************************************************************************************
		public bool SetAttributeValue(string NewValue)
		{
			try
			{
				cMessage = "";
				objActiveAttribute.Value = NewValue;
				this.SaveDoc();
			}
			catch (Exception ex)
			{
				cMessage = "Fehler beim Schreiben (" + ex.Message.ToString() + ")";
				return false;
			}
			cMessage = "";
			return true;
		}
		
		//********************************************************************************************
		// L鰏chen einer Sektion
		// aktiver Eintrag
		// ---------------------------------------
		//********************************************************************************************
		public bool DeleteSection()
		{
			try
			{
				objActiveRoot.RemoveChild(objActiveSection);
				this.SaveDoc();
			}
			catch (Exception ex)
			{
				cMessage = "Fehler beim L鰏chen des Entrys (" + ex.Message.ToString() + ")";
				return false;
			}
			cMessage = "";
			return true;
		}
		
		//********************************************************************************************
		// L鰏chen eines Eintrags
		// aktiver Eintrag
		// ---------------------------------------
		//********************************************************************************************
		public bool DeleteEntry()
		{
			try
			{
				objActiveSection.RemoveChild(objActiveEntry);
				this.SaveDoc();
			}
			catch (Exception ex)
			{
				cMessage = "Fehler beim L鰏chen des Eintrags (" + ex.Message.ToString() + ")";
				return false;
			}
			cMessage = "";
			return true;
		}
		//********************************************************************************************
		// L鰏chen eines Attributs
		// aktives Attribut
		// ---------------------------------------
		//********************************************************************************************
		public bool DeleteAttribute()
		{
			try
			{
				objActiveEntry.Attributes.Remove(objActiveAttribute);
				this.SaveDoc();
			}
			catch (Exception ex)
			{
				cMessage = "Fehler beim L鰏chen des Attributs (" + ex.Message.ToString() + ")";
				return false;
			}
			cMessage = "";
			return true;
		}
		
		//********************************************************************************************
		// Anlegen einer Sektion
		// die Neue Sektion entspricht der aktiven Sektion
		// ---------------------------------------
		//********************************************************************************************
		public bool CreateSection(string Section)
		{
			try
			{
				//Dim objRoot As XmlElement = objDOC.FirstChild
				if (objActiveRoot == null)
				{
					objActiveRoot = objDOC.FirstChild as XmlElement;
				}
				XmlElement objNewSection = default(XmlElement);
				objNewSection = objDOC.CreateElement(Section);
				objActiveRoot.AppendChild(objNewSection);
				objActiveSection = objNewSection;
				this.SaveDoc();
			}
			catch (Exception ex)
			{
				cMessage = "Fehler beim Anlegen einer neuen Sektion (" + ex.Message.ToString() + ")";
				return false;
			}
			cMessage = "";
			return true;
		}
		
		
		//********************************************************************************************
		// Anlegen eines Attributs zur Sektion
		// neues Attribut = aktives Attribut
		// Sektion muss vorher gew鋒lt sein
		//********************************************************************************************
		public bool CreateSectionAttribute(string AttributeName)
		{
			try
			{
				XmlAttribute objNewSectionAttribute = default(XmlAttribute);
				objNewSectionAttribute = objDOC.CreateAttribute(AttributeName);
				objActiveSection.Attributes.Append(objNewSectionAttribute);
				objActiveAttribute = objNewSectionAttribute;
				this.SaveDoc();
			}
			catch (Exception ex)
			{
				cMessage = "Fehler beim Anlegen eines neuen Attributs (" + ex.Message.ToString() + ")";
				return false;
			}
			cMessage = "";
			return true;
		}
		
		
		//********************************************************************************************
		// Anlegen eines Eintrags
		// der neue Eintrag = der aktiver Eintrag
		// Sektion muss vorher gew鋒lt sein
		//********************************************************************************************
		public bool CreateEntry(string Entry)
		{
			try
			{
				XmlElement objNewEntry = default(XmlElement);
				objNewEntry = objDOC.CreateElement(Entry);
				objActiveSection.AppendChild(objNewEntry);
				objActiveEntry = objNewEntry;
				this.SaveDoc();
			}
			catch (Exception ex)
			{
				cMessage = "Fehler beim Anlegen einer neuen Sektion (" + ex.Message.ToString() + ")";
				return false;
			}
			cMessage = "";
			return true;
		}
		//********************************************************************************************
		// Anlegen eines Attributs
		// neues Attribut = aktives Attribut
		// Sektion und Eintrag m黶sen vorher gew鋒lt sein
		//********************************************************************************************
		public bool CreateAttribute(string AttributeName)
		{
			try
			{
				XmlAttribute objNewAttribute = default(XmlAttribute);
				objNewAttribute = objDOC.CreateAttribute(AttributeName);
				objActiveEntry.Attributes.Append(objNewAttribute);
				objActiveAttribute = objNewAttribute;
				this.SaveDoc();
			}
			catch (Exception ex)
			{
				cMessage = "Fehler beim Anlegen eines neuen Attributs (" + ex.Message.ToString() + ")";
				return false;
			}
			cMessage = "";
			return true;
		}
		~CommonXMLHandle()
		{
			//base.Finalize();
		}
		//********************************************************************************************
		// Anzahl der Eintr鋑e in einer Sektion
		//********************************************************************************************
public int EntryCount
		{
			get
			{
				if (objActiveEntry == null)
				{
					return -1;
				}
				else
				{
					return objActiveEntry.ChildNodes.Count;
				}
			}
		}
		
		//********************************************************************************************
		// Erstellen eines Arrays mit allen Eintragsnamen in einer Sektion
		// ---------------------------------------------------------------
		// R點kgabewerte: Array = Alle Eintragsnamen
		//                Nothing = Fehler
		// Zeiger sitzt auf letztem Eintrag
		// Eine Sektion muss vorher ausgew鋒lt werden
		//********************************************************************************************
		public Array getEntryArray()
		{
			try
			{
				
				int nNodeCount = objActiveSection.ChildNodes.Count;
				Array tmpArray = System.Array.CreateInstance(typeof(string), nNodeCount);
				
				XmlNode objNodeEntry = default(XmlNode);
				
				objNodeEntry = objActiveSection.FirstChild;
				int i = 0;
				while (!(objNodeEntry == null))
				{
					objActiveEntry = objNodeEntry as XmlElement;
					tmpArray.SetValue(objNodeEntry.Name, i);
					i++;
					objNodeEntry = objNodeEntry.NextSibling;
				}
				return (tmpArray);
				
			}
			catch (Exception ex)
			{
				cMessage = "Fehler beim Erstellen des Arrays (" + ex.Message.ToString() + ")";
				return null;
			}
			
		}
		//躡erladen
		public Array getEntryArray(string EntryName)
		{
			try
			{
				FindEntry(EntryName);
				int nNodeCount = objActiveSection.ChildNodes.Count;
				Array tmpArray = System.Array.CreateInstance(typeof(string), nNodeCount);
				
				XmlNode objNodeEntry = default(XmlNode);
				
				objNodeEntry = objActiveSection.FirstChild;
				int i = 0;
				while (!(objNodeEntry == null))
				{
					objActiveEntry = objNodeEntry as XmlElement;
					tmpArray.SetValue(objNodeEntry.Name, i);
					i++;
					objNodeEntry = objNodeEntry.NextSibling;
				}
				return (tmpArray);
				
			}
			catch (Exception ex)
			{
				cMessage = "Fehler beim Erstellen des Arrays (" + ex.Message.ToString() + ")";
				return null;
			}
			
		}
		
		//********************************************************************************************
		// Erstellen eines Arrays mit allen InnerText-Elementen der Entrys in einer Sektion
		// ---------------------------------------------------------------
		// R點kgabewerte: Array = Alle InnerText-Elemente
		//                Nothing = Fehler
		// Zeiger sitzt auf letztem Eintrag
		// Eine Sektion muss vorher ausgew鋒lt werden
		//********************************************************************************************
		public Array getInnerTextArray()
		{
			try
			{
				
				int nNodeCount = objActiveSection.ChildNodes.Count;
				Array tmpArray = System.Array.CreateInstance(typeof(string), nNodeCount);
				
				XmlNode objNodeEntry = default(XmlNode);
				
				objNodeEntry = objActiveSection.FirstChild;
				int i = 0;
				while (!(objNodeEntry == null))
				{
					objActiveEntry = objNodeEntry as XmlElement;
					tmpArray.SetValue(objNodeEntry.InnerText, i);
					i++;
					objNodeEntry = objNodeEntry.NextSibling;
				}
				return (tmpArray);
				
			}
			catch (Exception ex)
			{
				cMessage = "Fehler beim Erstellen des Arrays (" + ex.Message.ToString() + ")";
				return null;
			}
			
		}
		
		
		//********************************************************************************************
		// Erstellen eines Arrays mit allen Sections einer XML-Datei
		// ---------------------------------------------------------------
		// R點kgabewerte: Array = Alle Eintragsnamen
		//                Nothing = Fehler
		// Zeiger sitzt auf letztem Eintrag
		// Eine Sektion muss vorher ausgew鋒lt werden
		//********************************************************************************************
		public Array GetSectionArray()
		{
			try
			{
				XmlNode objNodeRoot = default(XmlNode);
				objNodeRoot = objDOC.FirstChild;
				if (objActiveRoot == null)
				{
					objActiveRoot = objNodeRoot as XmlElement;
				}
				int nNodeCount = objNodeRoot.ChildNodes.Count;
				Array tmpArray = System.Array.CreateInstance(typeof(string), nNodeCount);
				XmlElement objNodeSection = default(XmlElement);


                objNodeSection = objNodeRoot.FirstChild as XmlElement;
				int i = 0;
				while (!(objNodeSection == null))
				{
					objActiveSection = objNodeSection;
					tmpArray.SetValue(objNodeSection.Name, i);
					i++;
					objNodeSection = objNodeSection.NextSibling as XmlElement;
				}
				return (tmpArray);
			}
			catch (Exception ex)
			{
				cMessage = "Fehler beim Erstellen des Arrays (" + ex.Message.ToString() + ")";
                return null;
			}
		}
		
		
		//********************************************************************************************
		// Erstellen eines Arrays mit allen Attributnamen eines Entrys
		// ---------------------------------------------------------------
		// R點kgabewerte: Array = Alle Eintragsnamen
		//                Nothing = Fehler
		// Zeiger sitzt auf letztem Eintrag
		// Eine Sektion muss vorher ausgew鋒lt werden
		//********************************************************************************************
		public Array getAttributeNameArray()
		{
			try
			{
				
				int nAttributeCount = objActiveEntry.Attributes.Count;
				Array tmpArray = System.Array.CreateInstance(typeof(string), nAttributeCount);
				
				XmlAttribute objEntryAttribute = default(XmlAttribute);
				
				if (nAttributeCount > 0)
				{
					int i = 0;
					foreach (XmlAttribute tempLoopVar_objEntryAttribute in objActiveEntry.Attributes)
					{
						objEntryAttribute = tempLoopVar_objEntryAttribute;
						tmpArray.SetValue(objEntryAttribute.Name, i);
						i++;
					}
				}
				return (tmpArray);
				
			}
			catch (Exception ex)
			{
				cMessage = "Fehler beim Erstellen des Arrays (" + ex.Message.ToString() + ")";
				return null;
			}
			
		}
		
		
		//********************************************************************************************
		// Erstellen eines Arrays mit allen Attributwerten eines Entrys
		// ---------------------------------------------------------------
		// R點kgabewerte: Array = Alle Eintragsnamen
		//                Nothing = Fehler
		// Zeiger sitzt auf letztem Eintrag
		// Eine Sektion muss vorher ausgew鋒lt werden
		//********************************************************************************************
		public Array getAttributeArray()
		{
			try
			{
				
				int nAttributeCount = objActiveEntry.Attributes.Count;
				Array tmpArray = System.Array.CreateInstance(typeof(string), nAttributeCount);
				
				XmlAttribute objEntryAttribute = default(XmlAttribute);
				
				if (nAttributeCount > 0)
				{
					int i = 0;
					foreach (XmlAttribute tempLoopVar_objEntryAttribute in objActiveEntry.Attributes)
					{
						objEntryAttribute = tempLoopVar_objEntryAttribute;
						tmpArray.SetValue(objEntryAttribute.Value, i);
						i++;
					}
				}
				return (tmpArray);
				
			}
			catch (Exception ex)
			{
				cMessage = "Fehler beim Erstellen des Arrays (" + ex.Message.ToString() + ")";
				return null;
			}
			
		}
		
		
		//********************************************************************************************
		// Erstellen einer neuen XML-Datei
		// ---------------------------------------------------------------
		// Voraussetzung : Der Dateiname muss bekannt sein (Property "XMLfilename")
		// Parameter :
		//       RootName = Name des Stammverzeichnises
		//       OverWrite = Soll bestehende Datei 黚erschrieben werden (Ja / Nein)
		// R點kgabewerte:
		//       true = Ausf黨rung korrekt; Datei wurde erstellt
		//       false = Bei der Ausf黨rung trat ein Fehler auf
		// Zeiger sitzt auf Root-Knoten
		//********************************************************************************************
		public bool CreateNewFile(string RootName, bool OverWrite)
		{
			try
			{
				if (RootName == "" || RootName == null)
				{
					cMessage = "Fehler beim Anlegen der XML-Datei (Knotenname muss angegeben werden)";
					return false;
					
				}
				if (File.Exists(cXMLFileName) == true)
				{
					// cXMLfilenameTMP = AppDomain.CurrentDomain.BaseDirectory & "\UMGIS_ADR_Control.CFG"
					if (OverWrite == false)
					{
						cMessage = "Fehler beim Anlegen der XML-Datei (Datei besteht bereits)";
						return false;
					}
					File.Delete(cXMLFileName);
				}
				
				// If File.Exists(cXMLfilenameTMP) = False Then
				// XML-datei anlegen
				XmlDocument objXMLdoc = new XmlDocument();
				XmlElement objRoot = default(XmlElement);
				
				objRoot = objXMLdoc.CreateElement(RootName);
				
				objXMLdoc.AppendChild(objRoot);
				
				objActiveRoot = objRoot;
				
				objXMLdoc.Save(cXMLFileName);
				
				objDOC = objXMLdoc;
				bDocIsOpen = true;
				DocMode = XmlMode.xmlDocOpen;
				
				return true;
				
			}
			catch (Exception ex)
			{
				cMessage = "Fehler beim Anlegen der XML-Datei (" + ex.Message.ToString() + ")";
				return false;
			}
		}
		//********************************************************************************************
		// Erstellen einer neuen XML-Datei
		// ---------------------------------------------------------------
		// Voraussetzung : Der Dateiname muss bekannt sein (Property "XMLfilename")
		// Parameter :
		//       RootName = Name des Stammverzeichnises
		//       OverWrite = Soll bestehende Datei 黚erschrieben werden (Ja / Nein)
		//       CodeNode = Code
		// R點kgabewerte:
		//       true = Ausf黨rung korrekt; Datei wurde erstellt
		//       false = Bei der Ausf黨rung trat ein Fehler auf
		// Zeiger sitzt auf Root-Knoten
		//********************************************************************************************
		public bool CreateNewFile(string RootName, bool OverWrite, string 
			XMLVersion, string XMLEncoding, 
			string XMLStandAlone)
		{
			try
			{
				if (RootName == "" || RootName == null)
				{
					cMessage = "Fehler beim Anlegen der XML-Datei (Knotenname muss angegeben werden)";
					return false;
					
				}
				if (File.Exists(cXMLFileName) == true)
				{
					// cXMLfilenameTMP = AppDomain.CurrentDomain.BaseDirectory & "\UMGIS_ADR_Control.CFG"
					if (OverWrite == false)
					{
						cMessage = "Fehler beim Anlegen der XML-Datei (Datei besteht bereits)";
						return false;
					}
					File.Delete(cXMLFileName);
				}
				
				// If File.Exists(cXMLfilenameTMP) = False Then
				// XML-datei anlegen
				XmlDocument objXMLdoc = new XmlDocument();
				XmlElement objRoot = default(XmlElement);
				XmlDeclaration objXMLdecl = default(XmlDeclaration);
				
				//Add the new node to the document.
				
				objXMLdecl = objXMLdoc.CreateXmlDeclaration(XMLVersion, XMLEncoding, XMLStandAlone);
				
				//objXMLdoc.CreateXmlDeclaration(XMLVersion, XMLEncoding, XMLStandAlone)
				
				//objRoot = objXMLdoc.CreateElement(CodeNodeName)
				//objXMLdoc.AppendChild(objRoot)
				
				objRoot = objXMLdoc.CreateElement(RootName);
				objXMLdoc.AppendChild(objRoot);
				
				objActiveRoot = objRoot;
				
				objXMLdoc.InsertBefore(objXMLdecl, objRoot);
				
				objXMLdoc.Save(cXMLFileName);
				
				objDOC = objXMLdoc;
				bDocIsOpen = true;
				DocMode = XmlMode.xmlDocOpen;
				
				return true;
				
			}
			catch (Exception ex)
			{
				cMessage = "Fehler beim Anlegen der XML-Datei (" + ex.Message.ToString() + ")";
				return false;
			}
			
		}
		
		
		//********************************************************************************************
		//Funktion legt die zu dem 黚ergebenen AttributValue zugeh鰎enden Active Section, Active Entry
		//und Active Attribute fest
		// ---------------------------------------------------------------
		//Voraussetzung:XMLOpen und DateiPfadString wurde 黚ergeben
		// R點kgabewerte: Bool
		//                FALSE = Fehler
		// Zeiger sitzt auf letztem Eintrag
		//********************************************************************************************
		public bool SetSectionEntryAttributeActive(string cAttributeValue)
		{
			try
			{
				Array SectionArray = default(Array);
				Array EntryArray = default(Array);
				Array AttributeNameArray = default(Array);
				int i = 0;
				int j = 0;
				int k = 0;
				bool bSwitch;
				bSwitch = false;
				
				SectionArray = GetSectionArray();
				for (i = SectionArray.GetLowerBound(0); i <= SectionArray.GetUpperBound(0); i++)
				{
					FindSection(SectionArray.GetValue(i).ToString());
					EntryArray = getEntryArray();
					for (j = EntryArray.GetLowerBound(0); j <= EntryArray.GetUpperBound(0); j++)
					{
						FindEntry(EntryArray.GetValue(j).ToString());
						AttributeNameArray = getAttributeNameArray();
						for (k = AttributeNameArray.GetLowerBound(0); k <= AttributeNameArray.GetUpperBound(0); k++)
						{
							FindAttribute(AttributeNameArray.GetValue(k).ToString());
							if (cAttributeValue == getAttributeValue())
							{
								bSwitch = true;
								return true;
							}
						}
					}
				}
				if (bSwitch == false)
				{
					return false;
				}
                return true;
			}
			catch (Exception ex)
			{
				cMessage = "Fehler bei der Auswahl der Section, Entry, Attribute(" + ex.Message.ToString() + ")";
				return false;
			}
			
			
		}
		
		//********************************************************************************************
		// Anzahl der Attributes in eines Eintrags
		//********************************************************************************************
public int AttributeCount
		{
			get
			{
				if (objActiveEntry == null)
				{
					return -1;
				}
				else
				{
					return objActiveEntry.Attributes.Count;
				}
			}
		}
		
		//********************************************************************************************
		// Gibt die Inhalte aller Entries einer Sektion aus in eine Arraylist
		//Voraussetzung: eine Section wurde aktiv gesetzt
		//********************************************************************************************
		public ArrayList GetAllEntryValues()
		{
			ArrayList Al = new ArrayList();
			XmlNode oNode = default(XmlNode);
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
					cMessage = "Section wurde nicht aktiv gesetzt";
					return default(ArrayList);
				}
				Al.Sort();
				return Al;
			}
			catch (Exception)
			{
				cMessage = "Fehler im CommonXMLHandle/GetAllEntryInnerText";
			}
			return default(ArrayList);
		}
		
		
		
		//####################################################################################################
		//######################################Die 3. Dimension-Subentries###################################
		//####################################################################################################
		//********************************************************************************************
		// Weiterentwicklung der Klasse clsXMLHandle
		// Neu:
		// * 3. Ebene (SubEntry)
		// * Attribute auch f黵 1. und 3. Ebene (Sektion und SubEntry)
		// Erich Lieberum (03.02.2004)
		//********************************************************************************************
		
		
		//********************************************************************************************
		// Suchen eines Unter-Eintrags (3. Ebene)
		// -------------------------------
		// R點kgabewerte: True = Unter-Eintrag wurde gefunden
		//                false = Unter-Eintrag wurde nicht gefunden
		// Zeiger sitzt auf gefundenem Knoten
		// Eine Eintrag muss vorher ausgew鋒lt werden
		//********************************************************************************************
		public bool FindSubEntry(string SubEntry)
		{
			if (!(DocMode == XmlMode.xmlEntrySelected))
			{
				cMessage = "Keine Eintrag ausgew鋒lt!";
				return false;
			}
			int i = 0;
			
			XmlNode objNodeSubEntry = default(XmlNode);
			
			objNodeSubEntry = objActiveEntry.FirstChild;
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
			cMessage = "Eintrag nicht vorhanden!";
			return false;
			
		}
		
		//********************************************************************************************
		// Anlegen eines Unter-Eintrags
		// der neue Unter-Eintrag = der aktiver Unter-Eintrag
		// Eintrag muss vorher gew鋒lt sein
		//********************************************************************************************
		public bool CreateSubEntry(string SubEntry)
		{
			try
			{
				XmlElement objNewSubEntry = default(XmlElement);
				objNewSubEntry = objDOC.CreateElement(SubEntry);
				objActiveEntry.AppendChild(objNewSubEntry);
				objActiveSubEntry = objNewSubEntry;
				this.SaveDoc();
			}
			catch (Exception ex)
			{
				cMessage = "Fehler beim Anlegen einer neuen Sektion (" + ex.Message.ToString() + ")";
				return false;
			}
			cMessage = "";
			return true;
		}
		
		
		//********************************************************************************************
		// Anlegen eines Attributs zum Unter-Eintrag
		// neues Attribut = aktives Attribut
		// Unter-Eintrag muss vorher gew鋒lt sein
		//********************************************************************************************
		public bool CreateSubEntryAttribute(string AttributeName)
		{
			try
			{
				XmlAttribute objNewSubEntryAttribute = default(XmlAttribute);
				objNewSubEntryAttribute = objDOC.CreateAttribute(AttributeName);
				objActiveSubEntry.Attributes.Append(objNewSubEntryAttribute);
				objActiveSubentryAttribute = objNewSubEntryAttribute;
				this.SaveDoc();
			}
			catch (Exception ex)
			{
				cMessage = "Fehler beim Anlegen eines neuen Attributs (" + ex.Message.ToString() + ")";
				return false;
			}
			cMessage = "";
			return true;
		}
		
		
		//********************************************************************************************
		// Suchen eines Untereintrags-Attributs (3. Ebene)
		// -------------------------------
		// R點kgabewerte: True = Attribut wurde gefunden
		//                false = Attribut wurde nicht gefunden
		// Zeiger sitzt auf gefundenem Attribut
		// Ein Untereintrag muss vorher ausgew鋒lt werden
		//********************************************************************************************
		public bool FindSubEntryAttribute(string SubEntryAttributeName)
		{
			if (!(DocMode == XmlMode.xmlEntrySelected))
			{
				cMessage = "Keine Eintrag ausgew鋒lt!";
				return false;
			}
			int i = 0;
			
			XmlAttribute objSubEntryAttribute = default(XmlAttribute);
			if (objActiveSubEntry.Attributes.Count == 0)
			{
				cMessage = "Keine Attribute vorhanden!";
				return false;
			}
			objSubEntryAttribute = objActiveSubEntry.Attributes[0];
			foreach (XmlAttribute tempLoopVar_objSubEntryAttribute in objActiveSubEntry.Attributes)
			{
				objSubEntryAttribute = tempLoopVar_objSubEntryAttribute;
				if (objSubEntryAttribute.Name.ToUpper() == SubEntryAttributeName.ToUpper())
				{
					objActiveSubentryAttribute = objSubEntryAttribute;
					DocMode = XmlMode.xmlEntrySelected;
					cMessage = "";
					return true;
				}
			}
			cMessage = "Attribut nicht vorhanden!";
			return false;
			
		}
		
		
		//********************************************************************************************
		// Setzen eines Untereintrags-Attributwertes (3. Ebene)
		// -------------------------------
		// R點kgabewerte: True = Attributwert wurde gesetzt
		//                false = Attributwert wurde nicht gesetzt
		// Zeiger sitzt auf gefundenem Attribut
		// Ein Untereintragsattribut muss vorher ausgew鋒lt werden
		//********************************************************************************************
		public bool SetSubEntryAttribute(string SubEntryAttributeValue)
		{
			//If Not DocMode = XmlMode.xmlEntrySelected Then
			//    cMessage = ("Kein Subentryattribut ausgew鋒lt!")
			//    Return False
			//End If
			
			try
			{
				if (!(objActiveSubentryAttribute == null))
				{
					objActiveSubentryAttribute.Value = SubEntryAttributeValue;
					cMessage = "";
					return true;
				}
				else
				{
					cMessage = "Es ist kein aktives Attribut vorhanden";
				}
			}
			catch (Exception)
			{
				cMessage = "Fehler bei 躡ergabe an das Active Attribute";
			}
			return default(bool);
		}
		
		
		//********************************************************************************************
		// Setzen eines Unter-Eintragswertes
		//
		// Unter-Eintrag muss vorher gew鋒lt sein
		//********************************************************************************************
		public bool SetSubEntryValue(string NewSubEntryValue)
		{
			try
			{
				cMessage = "";
				objActiveSubEntry.InnerText = NewSubEntryValue;
				this.SaveDoc();
			}
			catch (Exception ex)
			{
				cMessage = "Fehler beim Schreiben (" + ex.Message.ToString() + ")";
				return false;
			}
			cMessage = "";
			return true;
		}
		
		
		//********************************************************************************************
		// Erzeugen eines arrays aus den Subentrienames
		// -------------------------------
		// R點kgabe: Array
		// Zeiger muss vorher auf einem Eintrag stehen
		//Zeiger sitzt auf letztem Subentry
		//********************************************************************************************
		public Array GetSubEntryArray()
		{
			if (!(DocMode == XmlMode.xmlEntrySelected))
			{
				cMessage = "Kein Subentryattribut ausgew鋒lt!";
				return default(Array);
			}
			try
			{
				int nNodeCount = objActiveEntry.ChildNodes.Count;
				Array tmpArray = System.Array.CreateInstance(typeof(string), nNodeCount);
				
				XmlNode objNodeSubEntry = default(XmlNode);
				
				objNodeSubEntry = objActiveEntry.FirstChild;
				int i = 0;
				while (!(objNodeSubEntry == null))
				{
					objActiveSubEntry = objNodeSubEntry as XmlElement;
					tmpArray.SetValue(objNodeSubEntry.Name, i);
					i++;
					objNodeSubEntry = objNodeSubEntry.NextSibling;
				}
				return (tmpArray);
			}
			catch (Exception)
			{
				cMessage = "Fehler bei 躡ergabe an das Active Attribute";
                return null;
			}
			
		}
		
		
		//********************************************************************************************
		// Der Wert der Subentryeigenschaft wird 黚ergeben
		// -------------------------------
		// R點kgabe: String
		// Zeiger muss vorher auf einem Attribut stehen
		//********************************************************************************************
		public string GetSubentryAttribute()
		{
			try
			{
				if (!(objActiveSubentryAttribute == null))
				{
					cMessage = "";
					return objActiveSubentryAttribute.Value;
				}
			}
			catch (Exception)
			{
				cMessage = "Fehler bei Erhalt des Active Attribute";
			}
			return "";
		}
		
		//********************************************************************************************
		// Der Wert des Subentry wird 黚ergeben
		// -------------------------------
		// R點kgabe: String
		// Zeiger muss vorher auf einem aktiven Eintrag (sollte in dem Subentry sein) stehen
		//********************************************************************************************
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
				cMessage = "Fehler bei Erhalt des Aktiven Eintrags";
			}
			return "";
		}
		
		//********************************************************************************************
		// Der Wert des Subentry wird 黚ergeben
		// -------------------------------
		// R點kgabe: String
		// Zeiger muss vorher auf einem aktiven Eintrag (sollte in dem Subentry sein) stehen
		//********************************************************************************************
		public string GetSubentryName()
		{
			try
			{
				if (!(objActiveSubEntry == null))
				{
					cMessage = "";
					return objActiveSubEntry.Name;
				}
			}
			catch (Exception)
			{
				cMessage = "Fehler bei Erhalt des Aktiven Eintragsnamens";
			}
			return "";
		}
	}
	
}
