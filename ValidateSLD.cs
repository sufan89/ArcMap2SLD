// VBConversions Note: VB project level imports
using System;
using System.Drawing;
using System.Diagnostics;
using System.Data;
using Microsoft.VisualBasic;
using System.Collections;
using System.Windows.Forms;
// End of VB project level imports

using System.IO;
using System.Xml;
using System.Xml.Schema;

//####################################################################################################################
//*******************ArcGIS_SLD_Converter / Rel. 1.2.1****************************************************************
//*******************Class: ValidateSLD*******************************************************************************
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
//The class is made to validate the resulting SLD. It needs the OGC-schemas: epr.xsd, filter.xsd, geometry.xsd
//StyledLayerDescriptor.xsd and XLinks.xsd. The resulting messages are displayed in "Validation_Message".
//CHANGES:
//04.02.2006: Language-customizing english/german
//####################################################################################################################


namespace ArcGIS_SLD_Converter
{
	public class ValidateSLD
	{
		
		private string m_cResultSLD; //Der Dateipfad zur SLD
		private string m_cSchemaXSD; //Der Dateipfad zur Schemadatei
		private Motherform frmMotherform; //Instanz des Mutterformulars
		private Validation_Message m_frmValMess; //Die Instanz des Meldungsformulars
		private string m_cValidatMessage; //Der Text der Fehlermeldung
		private string m_cAltValidatMessage; //Alternativ Der Text der Fehlermeldung im Kontext der SLD-Struktur
		private bool m_bValidatMessage; //Der Flag wird gesetzt, wenn ein Formular erzeugt wurde
		private bool m_bValid; //Der Flag, ob die SLD g黮tig ist
		private bool m_bFirstError; //Damit sicher ist, dass die Kopfzeile der Meldung nur 1x geschrieben wird
		//Die XML-Informationen 黚er das SLD-Dokument:
		private int m_iLinenumber;
		private string m_cNodeType;
		private string m_cNodeName;
		private string m_cNodePosition;
		
		
		//##################################################################################################
		//######################################### FUNKTIONEN #############################################
		//##################################################################################################
		
		
		//************************************************************************************************
		//**********************************Die Steuerfunktion********************************************
		//************************************************************************************************
		private bool CentralProcessing()
		{
			if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
			{
				frmMotherform.CHLabelTop("躡erpr黤ung der SLD-Datei");
				frmMotherform.CHLabelBottom("躡erpr黤ung l鋟ft");
				frmMotherform.CHLabelSmall("躡erpr黤ung kann ein paar Minuten dauern");
			}
			else if (frmMotherform.m_enumLang == Motherform.Language.English)
			{
				frmMotherform.CHLabelTop("Checking the SLD-file");
				frmMotherform.CHLabelBottom("examination running");
				frmMotherform.CHLabelSmall("examination can take some time");
			}
			frmMotherform.ShowWorld();
			string[] args = new string[] {m_cResultSLD, m_cSchemaXSD};
			Run(args);
			frmMotherform.HideWorld();
			//Meldung wird nur angezeigt, wenn 黚erhaupt ein 躡erpr黤ungsereignis ausgel鰏t wurde
			if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
			{
				frmMotherform.CHLabelBottom("躡erpr黤ung abgeschlossen");
			}
			else if (frmMotherform.m_enumLang == Motherform.Language.English)
			{
				frmMotherform.CHLabelBottom("examination terminated");
			}
			
			if (m_bValid == false)
			{
				CreateValidationMessage();
				if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
				{
					frmMotherform.CHLabelSmall("Die SLD ist ung黮tig");
				}
				else if (frmMotherform.m_enumLang == Motherform.Language.English)
				{
					frmMotherform.CHLabelSmall("The SLD isn\'t valid");
				}
				
			}
			else
			{
				if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
				{
					frmMotherform.CHLabelSmall("Die SLD ist g黮tig");
				}
				else if (frmMotherform.m_enumLang == Motherform.Language.English)
				{
					frmMotherform.CHLabelSmall("The SLD is valid");
				}
				
			}
			return default(bool);
		}
		
		
		//##################################################################################################
		//########################################### ROUTINEN #############################################
		//##################################################################################################
		
		
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
		
		
		//************************************************************************************************
		//********************************Die G黮tigkeits黚erpr黤ung**************************************
		//************************************************************************************************
		public void Run(string[] args)
		{
			XmlValidatingReader reader = default(XmlValidatingReader);
			XmlSchemaCollection xsc = new XmlSchemaCollection();
			
			try
			{
				xsc.Add(null, new XmlTextReader(args[1]));
				reader = new XmlValidatingReader(new XmlTextReader(args[0]));
				IXmlLineInfo lineInfo = (IXmlLineInfo) reader; //Objekt f黵 Zeilennummern, etc.
				// Set the validation event handler
				ValidationEventHandler valdel = new ValidationEventHandler(ValidationEvent);
				
				reader.ValidationEventHandler += valdel;
				reader.Schemas.Add(xsc);
				reader.ValidationType = ValidationType.Schema;
				
				while (reader.Read())
				{
					int i;
					//For i = 0 To (reader.Depth - 1)
					//    Console.Write(Chr(9))
					//Next
					//Console.WriteLine("{0}<{1}>{2}", reader.NodeType, reader.Name, reader.Value)
					
					if (reader.NodeType == XmlNodeType.Element)
					{
						while (reader.MoveToNextAttribute())
						{
							m_iLinenumber = Convert.ToInt16(lineInfo.LineNumber.ToString());
							m_cNodePosition = lineInfo.LinePosition.ToString();
							m_cNodeType = reader.NodeType.ToString();
							m_cNodeName = reader.Name.ToString();
							//For i = 0 To (reader.Depth - 1)
							//    Console.Write(Chr(9))
							//Next
							//Console.WriteLine("{0}<{1}>{2}", reader.NodeType, reader.Name, reader.Value)
						}
					}
				}
				
			}
			catch (XmlSchemaException e)
			{
				if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
				{
					m_cValidatMessage = m_cValidatMessage + "XML-SCHEMA AUSNAHME!" + "\r\n" + "\r\n";
					m_cValidatMessage = m_cValidatMessage + "Die Schemadatei, die Sie geladen haben ist ung黮tig. " + "\r\n" + "M鰃licherweise haben Sie die Namespace-URL, die Sie in der folgenden Fehlermeldung wiederfinden nicht richtig eingebunden: " + "\r\n" + e.Message + "\r\n" + "Ausgel鰏ter Fehler in Zeile Nr.: " + System.Convert.ToString(e.LinePosition) + "\r\n";
					m_cValidatMessage = m_cValidatMessage + "\r\n" + "\r\n";
					m_bValid = false;
				}
				else if (frmMotherform.m_enumLang == Motherform.Language.English)
				{
					m_cValidatMessage = m_cValidatMessage + "XML-SCHEMA EXCEPTION!" + "\r\n" + "\r\n";
					m_cValidatMessage = m_cValidatMessage + "The schema-file you\'ve loaded seems to be unvalid. " + "\r\n" + "Perhaps you haven\'t included the namespace-URL well, you\'ll find in the following errormessage: " + "\r\n" + e.Message + "\r\n" + "Causing error in line Nr.: " + System.Convert.ToString(e.LinePosition) + "\r\n";
					m_cValidatMessage = m_cValidatMessage + "\r\n" + "\r\n";
					m_bValid = false;
				}
			}
			catch (XmlException e)
			{
				if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
				{
					m_cValidatMessage = m_cValidatMessage + "XML-SYNTAX AUSNAHME!" + "\r\n" + "\r\n";
					m_cValidatMessage = m_cValidatMessage + "Die Datei enth鋖t einen XML-Syntaxfehler. " + "\r\n" + e.Message;
					m_cValidatMessage = m_cValidatMessage + "\r\n" + "\r\n";
					m_bValid = false;
				}
				else if (frmMotherform.m_enumLang == Motherform.Language.English)
				{
					m_cValidatMessage = m_cValidatMessage + "XML-SYNTAX EXCEPTION!" + "\r\n" + "\r\n";
					m_cValidatMessage = m_cValidatMessage + "the file comprises an XML-Syntax error. " + "\r\n" + e.Message;
					m_cValidatMessage = m_cValidatMessage + "\r\n" + "\r\n";
					m_bValid = false;
				}
			}
			catch (Exception e)
			{
				if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
				{
					m_cValidatMessage = m_cValidatMessage + "SCHWERE AUSNAHME!" + "\r\n" + "\r\n";
					m_cValidatMessage = m_cValidatMessage + "Unbekannter Fehler beim G黮tigkeitstest der SLD" + "\r\n" + e.Message;
					m_cValidatMessage = m_cValidatMessage + e.StackTrace + "\r\n";
					m_cValidatMessage = m_cValidatMessage + "\r\n" + "\r\n";
					m_bValid = false;
				}
				else if (frmMotherform.m_enumLang == Motherform.Language.English)
				{
					m_cValidatMessage = m_cValidatMessage + "SERIOUS EXCEPTION!" + "\r\n" + "\r\n";
					m_cValidatMessage = m_cValidatMessage + "Unknown error while validating SLD" + "\r\n" + e.Message;
					m_cValidatMessage = m_cValidatMessage + e.StackTrace + "\r\n";
					m_cValidatMessage = m_cValidatMessage + "\r\n" + "\r\n";
					m_bValid = false;
				}
			}
			finally
			{
				if (!(reader == null))
				{
					reader.Close();
				}
			}
		}
		
		
		//************************************************************************************************
		//***********************Der Eventhandler f黵 die G黮tigkeits黚erpr黤ung**************************
		//************************************************************************************************
		private void ValidationEvent(object errorid, ValidationEventArgs args)
		{
			m_bValid = false;
			//Schreibt 1x den Messageheader
			if (m_bFirstError == false)
			{
				WriteMessageHeader();
			}
			
			m_cValidatMessage = m_cValidatMessage + "Validierungs-Meldung: " + "\r\n" + args.Message + "\r\n";
			
			if (args.Severity == XmlSeverityType.Warning)
			{
				if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
				{
					m_cValidatMessage = m_cValidatMessage + "Es wurde kein Schema gefunden, um die G黮tigkeitspr黤ung zu erzwingen." + "\r\n" + "\r\n";
				}
				else if (frmMotherform.m_enumLang == Motherform.Language.English)
				{
					m_cValidatMessage = m_cValidatMessage + "There was no schema-file found to enforce the validation." + "\r\n" + "\r\n";
				}
			}
			else if (args.Severity == XmlSeverityType.Error)
			{
				if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
				{
					m_cValidatMessage = m_cValidatMessage + "W鋒rend der Instanz黚erpr黤ung wurde ein ung黮tiges " + m_cNodeType + " in der SLD entdeckt: " + "\r\n";
				}
				else if (frmMotherform.m_enumLang == Motherform.Language.English)
				{
					m_cValidatMessage = m_cValidatMessage + "During the examination was an unvalid " + m_cNodeType + " found in the SLD: " + "\r\n";
				}
			}
			
			if (!(args.Exception == null)) // XSD schema validation error
			{
				if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
				{
					m_cValidatMessage = m_cValidatMessage + "Das " + m_cNodeType + " \'" + m_cNodeName + "\' " + " in Zeile: " + System.Convert.ToString(m_iLinenumber) + " an der Position " + m_cNodePosition + "\r\n";
				}
				else if (frmMotherform.m_enumLang == Motherform.Language.English)
				{
					m_cValidatMessage = m_cValidatMessage + "The " + m_cNodeType + " \'" + m_cNodeName + "\' " + " at line: " + System.Convert.ToString(m_iLinenumber) + " , Position: " + m_cNodePosition + "\r\n";
				}
			}
			m_cValidatMessage = m_cValidatMessage + "~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~" + "\r\n";
			//if (myXmlValidatingReader.Reader.LineNumber > 0)
			//    Console.WriteLine("Line: " & myXmlValidatingReader.Reader.LineNumber & " Position: " & myXmlValidatingReader.Reader.LinePosition)
			//end if
		}
		
		//************************************************************************************************
		//*************************Die Routine schreibt den Header der Textmeldung************************
		//************************************************************************************************
		private void WriteMessageHeader()
		{
			m_bFirstError = true;
			if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
			{
				m_cValidatMessage = m_cValidatMessage + "Bei der 躡erpr黤ung der G黮tigkeit des SLD-Dokuments mit der Schemadatei " + "\r\n";
				m_cValidatMessage = m_cValidatMessage + m_cSchemaXSD;
				m_cValidatMessage = m_cValidatMessage + " wurden ung黮tige Inhalte entdeckt. Vielleicht haben Sie die SLD gegen ein " + "\r\n";
				m_cValidatMessage = m_cValidatMessage + "veraltetes Schema validiert. In diesem Fall k鰊nen Sie ein neueres Schema " + "\r\n";
				m_cValidatMessage = m_cValidatMessage + "einbinden und erhalten evtl. die Best鋞igung f黵 die Korrektheit ihrer Datei." + "\r\n";
				m_cValidatMessage = m_cValidatMessage + "\r\n" + "\r\n";
			}
			else if (frmMotherform.m_enumLang == Motherform.Language.English)
			{
				m_cValidatMessage = m_cValidatMessage + "Unvalid contents were found during examination on validity of the SLD with the schema-file  " + "\r\n";
				m_cValidatMessage = m_cValidatMessage + m_cSchemaXSD;
				m_cValidatMessage = m_cValidatMessage + " . Maybe you have checked the SLD against a deprecated schema. " + "\r\n";
				m_cValidatMessage = m_cValidatMessage + "In that case you may take a newer schema and then perhaps get an " + "\r\n";
				m_cValidatMessage = m_cValidatMessage + "affirmation of the validity of your SLD-file." + "\r\n";
				m_cValidatMessage = m_cValidatMessage + "\r\n" + "\r\n";
			}
		}
		
		//************************************************************************************************
		//************Das Anzeigeformular wird gemacht. Ihm wird die Fehlermeldung 黚ergeben**************
		//************************************************************************************************
		private void CreateValidationMessage()
		{
			m_bValidatMessage = true;
			m_bValid = false;
			m_frmValMess = new Validation_Message(m_cValidatMessage, frmMotherform);
			m_frmValMess.Visible = true;
			m_frmValMess.Show();
			m_frmValMess.Activate();
		}
		
	}
	
}
