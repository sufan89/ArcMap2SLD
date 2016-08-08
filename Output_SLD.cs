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
using stdole;
//using ArcGIS_SLD_Converter.Analize_ArcMap_Symbols;
using Microsoft.VisualBasic.CompilerServices;

//####################################################################################################################
//*******************ArcGIS_SLD_Converter*****************************************************************************
//*******************Class: Output_SLD********************************************************************************
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
//The class Output_SLD is called by the class Analize_ArcMap_Symbols. It receives the Object of Analize_ArcMap_Symbols.
//So it possesses the structs with all stored data. The Output_SLD creates the final SLD. Main function is "CentralProcessingFunc"
//it rules all the other movements. The SLD-Skeleton is built by "WriteToSLD". The mapping from the ArcMap-symbols to the
//SLD symbols is managed by the function "GetValueFromSymbolstruct()". The XML-file of the SLD is built by the class
//"XMLHandle" The function "WriteToSLD" passes over each package of SLD-XML-code to the class "XMLHandle"
//CHANGES:
//04.02.2006: Language-customizing english/german
//11.01.2007: Changing the floating point values from commaseparated to point separated (which is OGC conformous)
//28.02.2007: - Added changes for Ionic RSW WMS: Rule.Name and FeatureTypeName
//            - Improved code readability and consistency by moving common code
//              blocks for point and line symbolizers to separate functions.
//              Same for marker code to GetMarkerValue function.
//            - Improved point symbol conversion to well known marker types,
//              including simple, arrow symbols. For some character marker
//              symbol fonts, consisting mainly of circles and rectangles,
//              it tries to map the characters to well-known markers.
//            - Improved transparent color handling: polygons with outline but no fill, and point symbols.
//27.03.2007: - For simpleRenderer added Rule Name and Title elements, needed for Ionic.
//            - For simple polygon features, do not default to solid fill, but look at polygon features.
//            - For multi-layer symbols create a symbolizer for each layer.
//            - For line features, added stroke-dasharray CSS based on the dash-type (simple lines)
//              or the template (cartographic lines) defined for a symbol.
//              Also implements HashLineSymbols using this.
//            - Improved detection of no outline by checking for outline size of 0
//            - For polygon features, don't include a fill CSS if the feature has no, or a transparent, fill.
//            - For polygon features, don't include a stroke CSS if there is no border.
//            - Determine opacity based on the transparency set in the symbol color.
//            - Simplified color handling by always having no color if the color is transparent or unused.
//25.04.2007: - Added TextSymbolizer support for simple annotation with a single feature as label.
//12.09.2007: - Changed the logical Operator "AND" to the OGC-compliant "And". Thus changed the "LUT_SLD_mapping_file.xml"
//            - Changed the format of the stroke-dasharray to the OGC-compliant format without the suffixes "px"
//30.11.2007: - LowerBoundary and UpperBoundary gives now also OGC compliant Values with decimal point instaed comma
//09.06.2008: - Add support for grouped values in UniqueValueRenderer, generating an OGC "Or" for the group of values.
//            - Bugfix for OGC And (in LUT_SLD_mapping_file.xml the OGC "And" tag was spelled as "AND").
//10.09.2008: - Added support for separation of  layers in multiple files
//23.10.2008: - Bugfix at ClassBreaksRenderer. The CommaToPoint Function was not found
//08.06.2011: - (ARIS) Added new flavor of SLD that does not refernce layer names (to be used with WorldMap).
//####################################################################################################################


namespace ArcGIS_SLD_Converter
{
	public class Output_SLD
	{
		
		
		//##################################################################################################
		//######################################## DEKLARATIONEN ###########################################
		//##################################################################################################
		
#region Membervariablen
		private Motherform frmMotherForm;
		private Analize_ArcMap_Symbols m_objData;
		private Analize_ArcMap_Symbols.StructProject m_strDataSavings;
		private XMLHandle m_objXMLHandle;
		private string m_cFilename; //The whole Path and Filename of the SLD
		private string m_cFile; //The filename of the SLD
		private string m_cPath; //Der Pfad zur SLD-Datei
		private bool m_bSepFiles; //Layers in separate files
		private string m_bIncludeLayerNames; //Include layer names
#endregion
		
		//##################################################################################################
		//######################################## ENUMERATIONEN ###########################################
		//##################################################################################################
		
#region Enums
		
		
#endregion
		
		//##################################################################################################
		//########################################### ROUTINEN #############################################
		//##################################################################################################
		
#region Routinen
		//This one is used if all Layers are written in one File. Then Filename contains both: path and Filename of SLD to create
		public Output_SLD(Motherform Mother, Analize_ArcMap_Symbols Analize, string Filename)
		{
			frmMotherForm = Mother;
			m_cFilename = Filename;
			m_bSepFiles = frmMotherForm.GetInfoSeparateLayers;
			m_cFile = frmMotherForm.GetSLDFile;
			m_cPath = frmMotherForm.GetSLDPath;
			m_objData = Analize;
            m_strDataSavings = (Analize_ArcMap_Symbols.StructProject)m_objData.GetProjectData;
			m_bIncludeLayerNames = frmMotherForm.GetInfoIncludeLayerNames;
			CentralProcessingFunc();
		}
		
		public Output_SLD()
		{
			//m_cFilename = "C:\Krimskrams\bla.sld"      'Zu Testzwecken
			CentralProcessingFunc();
		}
		
#endregion //Die Public Sub New()
		
		//##################################################################################################
		//######################################### FUNKTIONEN #############################################
		//##################################################################################################
		
#region Steuerungsfunktionen //(incl. SLD-Tag Schreibanweisungen)
		
		//************************************************************************************************
		//Die Funktion steuert die Prozesse zentral.
		//************************************************************************************************
		private bool CentralProcessingFunc()
		{
			bool bSuccess;
			bSuccess = false;
			if (frmMotherForm.m_enumLang == Motherform.Language.Deutsch)
			{
				frmMotherForm.CHLabelTop("Die Ausgabe in SLD");
				frmMotherForm.CHLabelBottom("Gespeicherte Daten werden verarbeitet");
			}
			else if (frmMotherForm.m_enumLang == Motherform.Language.English)
			{
				frmMotherForm.CHLabelTop("The Output in SLD");
				frmMotherForm.CHLabelBottom("the stored data is beeing processed");
			}
			
			// m_objXMLHandle = New XMLHandle
			//CreateSLD()
			if (WriteToSLD() == true)
			{
				bSuccess = true;
			}
			if (frmMotherForm.m_enumLang == Motherform.Language.Deutsch)
			{
				frmMotherForm.CHLabelTop("Fertig");
			}
			else if (frmMotherForm.m_enumLang == Motherform.Language.English)
			{
				frmMotherForm.CHLabelTop("Ready");
			}
			
			if (bSuccess == true)
			{
				if (frmMotherForm.m_enumLang == Motherform.Language.Deutsch)
				{
					frmMotherForm.CHLabelBottom("Die Datei wurde angelegt.");
				}
				else if (frmMotherForm.m_enumLang == Motherform.Language.English)
				{
					frmMotherForm.CHLabelBottom("The file has been generated.");
				}
				
				if (frmMotherForm.chkValidate.Checked == true)
				{
					ValidateSLD ValSLD = new ValidateSLD(frmMotherForm);
				}
				else
				{
					frmMotherForm.CHLabelSmall("");
				}
			}
			else
			{
				if (frmMotherForm.m_enumLang == Motherform.Language.Deutsch)
				{
					frmMotherForm.CHLabelBottom("Die Datei konnte nicht angelegt werden");
					frmMotherForm.CHLabelSmall("");
				}
				else if (frmMotherForm.m_enumLang == Motherform.Language.English)
				{
					frmMotherForm.CHLabelBottom("Could\'t generate the file");
					frmMotherForm.CHLabelSmall("");
				}
				
			}
			
			return default(bool);
		}
		
		
		//************************************************************************************************
		//Creates SLD Doc: Document Description, Root-Node
		//************************************************************************************************
		private bool CreateSLD(string FileName, bool bIncludeLayerNames)
		{
			m_objXMLHandle = new XMLHandle(FileName, bIncludeLayerNames);
			m_objXMLHandle.CreateNewFile(true, bIncludeLayerNames);
			return default(bool);
		}
		
		
		//************************************************************************************************
		//Extracts the data from the structs and writes to SLD
		//Firstly is decided, if the Layers are written in one or in separate files
		//************************************************************************************************
		public bool WriteToSLD()
		{
			int i = 0;
			int j = 0;
			int l = 0;
			string cLayerName = "";
			ArrayList objFieldValues = default(ArrayList);
			bool bDoOneLayer = default(bool);
			double dummy = 0; //to buffer the double coming from the layer list
			
			//the decision if separate Layers or one Layer
			if (m_bSepFiles == true)
			{
				bDoOneLayer = false;
			}
			else
			{
				bDoOneLayer = true;
				//creation of the SLD with only one File using the user defined filename
				CreateSLD(m_cFilename, bool.Parse(m_bIncludeLayerNames));
			}
			
			
			try
			{
				for (i = 0; i <= m_strDataSavings.LayerCount - 1; i++)
                {
                    #region 获取图层名称
                    string strDatasetName = "";
                    ArrayList objSymbols = default(ArrayList); //Die ArrayList mit den Symbolen eines Layers
                    if (m_strDataSavings.LayerList[i] is Analize_ArcMap_Symbols.StructUniqueValueRenderer)
                    {
                        Analize_ArcMap_Symbols.StructUniqueValueRenderer temp = (Analize_ArcMap_Symbols.StructUniqueValueRenderer)m_strDataSavings.LayerList[i];
                        strDatasetName = temp.DatasetName;
                        objSymbols = temp.SymbolList;
                        cLayerName = System.Convert.ToString(temp.LayerName);
                    }
                    else if (m_strDataSavings.LayerList[i] is Analize_ArcMap_Symbols.StructClassBreaksRenderer)
                    {
                        Analize_ArcMap_Symbols.StructClassBreaksRenderer temp = (Analize_ArcMap_Symbols.StructClassBreaksRenderer)m_strDataSavings.LayerList[i];
                        strDatasetName = temp.DatasetName;
                        objSymbols = temp.SymbolList;
                        cLayerName = System.Convert.ToString(temp.LayerName);
                    }
                    else if (m_strDataSavings.LayerList[i] is Analize_ArcMap_Symbols.StructSimpleRenderer)
                    {
                        Analize_ArcMap_Symbols.StructSimpleRenderer temp = (Analize_ArcMap_Symbols.StructSimpleRenderer)m_strDataSavings.LayerList[i];
                        strDatasetName = temp.DatasetName;
                        objSymbols = temp.SymbolList;
                        cLayerName = System.Convert.ToString(temp.LayerName);
                    }
                    #endregion

                    if (frmMotherForm.m_enumLang == Motherform.Language.Deutsch)
					{
						frmMotherForm.CHLabelBottom("Verarbeite Layer " + cLayerName);
					}
					else if (frmMotherForm.m_enumLang == Motherform.Language.English)
					{
						frmMotherForm.CHLabelBottom("processing layer " + cLayerName);
					}
					
					//Creation of several SLD with that format: /UserDefinedPath/UserDefinedName_LayerName.sld
					if (bDoOneLayer == false)
					{
						CreateSLD(m_cFilename + "_" + cLayerName + ".sld", bool.Parse(m_bIncludeLayerNames));
					}



					//XML-Schreibanweisungen auf Projektebene und Layerebene
					if (Convert.ToBoolean(m_bIncludeLayerNames))
					{
						//' ARIS: Standard SLD output
						m_objXMLHandle.CreateElement("NamedLayer");
						m_objXMLHandle.CreateElement("LayerName");
                        m_objXMLHandle.SetElementText(System.Convert.ToString(strDatasetName));
						m_objXMLHandle.CreateElement("UserStyle");
						m_objXMLHandle.CreateElement("StyleName");
						m_objXMLHandle.SetElementText("Style1");
						m_objXMLHandle.CreateElement("FeatureTypeStyle");
						m_objXMLHandle.CreateElement("FeatureTypeName");
                        m_objXMLHandle.SetElementText(System.Convert.ToString(strDatasetName));
					}
					else
					{
						//' ARIS: WorldMap SLD output
						m_objXMLHandle.CreateElement("FeatureTypeStyle");
						m_objXMLHandle.CreateElement("FeatureTypeName");
                        m_objXMLHandle.SetElementText(System.Convert.ToString(strDatasetName));
						m_objXMLHandle.CreateElement("FeatureTypeTitle");
                        m_objXMLHandle.SetElementText(System.Convert.ToString(strDatasetName));
					}
					
					//XML-Schreibanweisungen auf Layerebene und auf Symbolebene
					for (j = 0; j <= objSymbols.Count - 1; j++) //IN DER SCHLEIFE AUF SYMBOLEBENE objSymbols(j) repr鋝entiert 1 Symbol!!!
					{
						if (frmMotherForm.m_enumLang == Motherform.Language.Deutsch)
						{
							frmMotherForm.CHLabelSmall("Symbol " + (j + 1).ToString() + " von " + objSymbols.Count.ToString());
						}
						else if (frmMotherForm.m_enumLang == Motherform.Language.English)
						{
							frmMotherForm.CHLabelSmall("Symbol " + (j + 1).ToString() + " of " + objSymbols.Count.ToString());
                        }

                        #region
                        string StrLabel = "";
                        double StrLowerLimit = 0.00;
                        double StrUpperLimit = 0.00;
                        if (objSymbols[j] is Analize_ArcMap_Symbols.StructSimpleMarkerSymbol)
                        {
                            Analize_ArcMap_Symbols.StructSimpleMarkerSymbol temp=(Analize_ArcMap_Symbols.StructSimpleMarkerSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructCharacterMarkerSymbol)
                        {
                            Analize_ArcMap_Symbols.StructCharacterMarkerSymbol temp = (Analize_ArcMap_Symbols.StructCharacterMarkerSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructPictureMarkerSymbol)
                        {
                            Analize_ArcMap_Symbols.StructPictureMarkerSymbol temp = (Analize_ArcMap_Symbols.StructPictureMarkerSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructArrowMarkerSymbol)
                        {
                            Analize_ArcMap_Symbols.StructArrowMarkerSymbol temp = (Analize_ArcMap_Symbols.StructArrowMarkerSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructSimpleLineSymbol)
                        {
                            Analize_ArcMap_Symbols.StructSimpleLineSymbol temp = (Analize_ArcMap_Symbols.StructSimpleLineSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructCartographicLineSymbol)
                        {
                            Analize_ArcMap_Symbols.StructCartographicLineSymbol temp = (Analize_ArcMap_Symbols.StructCartographicLineSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructHashLineSymbol)
                        {
                            Analize_ArcMap_Symbols.StructHashLineSymbol temp = (Analize_ArcMap_Symbols.StructHashLineSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructMarkerLineSymbol)
                        {
                            Analize_ArcMap_Symbols.StructMarkerLineSymbol temp = (Analize_ArcMap_Symbols.StructMarkerLineSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructPictureLineSymbol)
                        {
                            Analize_ArcMap_Symbols.StructPictureLineSymbol temp = (Analize_ArcMap_Symbols.StructPictureLineSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructSimpleFillSymbol)
                        {
                            Analize_ArcMap_Symbols.StructSimpleFillSymbol temp = (Analize_ArcMap_Symbols.StructSimpleFillSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructMarkerFillSymbol)
                        {
                            Analize_ArcMap_Symbols.StructMarkerFillSymbol temp = (Analize_ArcMap_Symbols.StructMarkerFillSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructLineFillSymbol)
                        {
                            Analize_ArcMap_Symbols.StructLineFillSymbol temp = (Analize_ArcMap_Symbols.StructLineFillSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructDotDensityFillSymbol)
                        {
                            Analize_ArcMap_Symbols.StructDotDensityFillSymbol temp = (Analize_ArcMap_Symbols.StructDotDensityFillSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructPictureFillSymbol)
                        {
                            Analize_ArcMap_Symbols.StructPictureFillSymbol temp = (Analize_ArcMap_Symbols.StructPictureFillSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructGradientFillSymbol)
                        {
                            Analize_ArcMap_Symbols.StructGradientFillSymbol temp = (Analize_ArcMap_Symbols.StructGradientFillSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructBarChartSymbol)
                        {
                            Analize_ArcMap_Symbols.StructBarChartSymbol temp = (Analize_ArcMap_Symbols.StructBarChartSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructPieChartSymbol)
                        {
                            Analize_ArcMap_Symbols.StructPieChartSymbol temp = (Analize_ArcMap_Symbols.StructPieChartSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructStackedChartSymbol)
                        {
                            Analize_ArcMap_Symbols.StructStackedChartSymbol temp = (Analize_ArcMap_Symbols.StructStackedChartSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructTextSymbol)
                        {
                            Analize_ArcMap_Symbols.StructTextSymbol temp = (Analize_ArcMap_Symbols.StructTextSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructMultilayerMarkerSymbol)
                        {
                            Analize_ArcMap_Symbols.StructMultilayerMarkerSymbol temp = (Analize_ArcMap_Symbols.StructMultilayerMarkerSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructMultilayerLineSymbol)
                        {
                            Analize_ArcMap_Symbols.StructMultilayerLineSymbol temp = (Analize_ArcMap_Symbols.StructMultilayerLineSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        else if (objSymbols[j] is Analize_ArcMap_Symbols.StructMultilayerFillSymbol)
                        {
                            Analize_ArcMap_Symbols.StructMultilayerFillSymbol temp = (Analize_ArcMap_Symbols.StructMultilayerFillSymbol)objSymbols[j];
                            StrLabel = temp.Label;
                            StrLowerLimit = temp.LowerLimit;
                            StrUpperLimit = temp.UpperLimit;
                            objFieldValues = temp.Fieldvalues;
                        }
                        #endregion

                        //HIER DIE UNTERSCHEIDUNGEN NACH DEN EINZELNEN RENDERERN: UNIQUEVALUERENDERER
						if (m_strDataSavings.LayerList[i] is Analize_ArcMap_Symbols.StructUniqueValueRenderer)
						{
							Analize_ArcMap_Symbols.StructUniqueValueRenderer objStructUVR = new Analize_ArcMap_Symbols.StructUniqueValueRenderer();
                            objStructUVR = (Analize_ArcMap_Symbols.StructUniqueValueRenderer)m_strDataSavings.LayerList[i]; //Zuweisung des StructsUVR. Repr鋝entiert je einen Layer!!!
							m_objXMLHandle.CreateElement("Rule");
							m_objXMLHandle.CreateElement("RuleName");
                            m_objXMLHandle.SetElementText(StrLabel);
							m_objXMLHandle.CreateElement("Title");
                            m_objXMLHandle.SetElementText(StrLabel);
							m_objXMLHandle.CreateElement("Filter");
							if (frmMotherForm.chkScale.Checked == true)
							{
								m_objXMLHandle.CreateElement("MinScale");
								m_objXMLHandle.SetElementText(frmMotherForm.cboLowScale.Text);
								m_objXMLHandle.CreateElement("MaxScale");
								m_objXMLHandle.SetElementText(frmMotherForm.cboHighScale.Text);
							}
							if (objStructUVR.FieldCount > 1) //Nur wenn nach mehr als 1 Feld klassifiziert wurde, wird der <AND>-Tag gesetzt
							{
								m_objXMLHandle.CreateElement("And");
								for (l = 0; l <= objStructUVR.FieldCount - 1; l++) //Die Schleife ist nur daf黵 da, falls nach mehreren Feldern klassifiziert wurde
								{
									m_objXMLHandle.CreateElement("PropertyIsEqualTo"); //Sie schreibt pro Feld nach dem klass. wurde das <PropertyIsEqualTo> und alle Kinder
									m_objXMLHandle.CreateElement("PropertyName");
									m_objXMLHandle.SetElementText(System.Convert.ToString(objStructUVR.FieldNames[l]));
									m_objXMLHandle.CreateElement("Fieldvalue");
									m_objXMLHandle.SetElementText(System.Convert.ToString(objFieldValues[l]));
								}
							}
							else if (objStructUVR.FieldCount == 1)
							{
								if (objFieldValues.Count > 1)
								{
									m_objXMLHandle.CreateElement("Or");
								}
								for (l = 0; l <= objFieldValues.Count - 1; l++) //If multiple values grouped in same class
								{
									m_objXMLHandle.CreateElement("PropertyIsEqualTo"); //Sie schreibt pro Feld nach dem klass. wurde das <PropertyIsEqualTo> und alle Kinder
									m_objXMLHandle.CreateElement("PropertyName");
									m_objXMLHandle.SetElementText(System.Convert.ToString(objStructUVR.FieldNames[0]));
									m_objXMLHandle.CreateElement("Fieldvalue");
									m_objXMLHandle.SetElementText(System.Convert.ToString(objFieldValues[l]));
								}
							}
							//UNTERSCHEIDUNG NACH FEATURECLASS DES BETREFFENDEN SYMBOLS
							switch (objStructUVR.FeatureCls)
							{
								case Analize_ArcMap_Symbols.FeatureClass.PointFeature:
									WritePointFeatures(objSymbols[j]);
									break;
								case Analize_ArcMap_Symbols.FeatureClass.LineFeature:
									WriteLineFeatures(objSymbols[j]);
									break;
								case Analize_ArcMap_Symbols.FeatureClass.PolygonFeature:
									WritePolygonFeatures(objSymbols[j]);
									break;
							}
							//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
							//HIER DIE UNTERSCHEIDUNGEN NACH DEN EINZELNEN RENDERERN: CLASSBREAKSRENDERER
						}
						else if (m_strDataSavings.LayerList[i] is Analize_ArcMap_Symbols.StructClassBreaksRenderer)
						{
							Analize_ArcMap_Symbols.StructClassBreaksRenderer objStructCBR = new Analize_ArcMap_Symbols.StructClassBreaksRenderer();
							objStructCBR = (Analize_ArcMap_Symbols.StructClassBreaksRenderer)m_strDataSavings.LayerList[i];
							m_objXMLHandle.CreateElement("Rule");
							m_objXMLHandle.CreateElement("RuleName");
                            m_objXMLHandle.SetElementText(StrLabel);
							m_objXMLHandle.CreateElement("Title");
                            m_objXMLHandle.SetElementText(StrLabel);
							m_objXMLHandle.CreateElement("Filter");
							if (frmMotherForm.chkScale.Checked == true)
							{
								m_objXMLHandle.CreateElement("MinScale");
								m_objXMLHandle.SetElementText(frmMotherForm.cboLowScale.Text);
								m_objXMLHandle.CreateElement("MaxScale");
								m_objXMLHandle.SetElementText(frmMotherForm.cboHighScale.Text);
							}
							m_objXMLHandle.CreateElement("PropertyIsBetween");
							m_objXMLHandle.CreateElement("PropertyName");
							m_objXMLHandle.SetElementText(objStructCBR.FieldName);
							m_objXMLHandle.CreateElement("LowerBoundary");
							m_objXMLHandle.CreateElement("Fieldvalue");
                            //As ArrayList member the type is no more recognized from compiler. If saving in a dummy double its recognized again
                            dummy = StrLowerLimit;
							m_objXMLHandle.SetElementText(CommaToPoint(dummy));
							m_objXMLHandle.CreateElement("UpperBoundary");
							m_objXMLHandle.CreateElement("Fieldvalue");
                            //As ArrayList member the type is no more recognized from compiler. If saving in a dummy double its recognized again
                            dummy = StrUpperLimit; 
							m_objXMLHandle.SetElementText(CommaToPoint(dummy));
							switch (objStructCBR.FeatureCls)
							{
								case Analize_ArcMap_Symbols.FeatureClass.PointFeature:
									WritePointFeatures(objSymbols[j]);
									break;
								case Analize_ArcMap_Symbols.FeatureClass.LineFeature:
									WriteLineFeatures(objSymbols[j]);
									break;
								case Analize_ArcMap_Symbols.FeatureClass.PolygonFeature:
									WritePolygonFeatures(objSymbols[j]);
									break;
							}
							//HIER DIE UNTERSCHEIDUNGEN NACH DEN EINZELNEN RENDERERN: SIMPLERENDERER
						}
						else if (m_strDataSavings.LayerList[i] is Analize_ArcMap_Symbols.StructSimpleRenderer)
						{
							Analize_ArcMap_Symbols.StructSimpleRenderer objStructSR = new Analize_ArcMap_Symbols.StructSimpleRenderer();
                            objStructSR = (Analize_ArcMap_Symbols.StructSimpleRenderer)m_strDataSavings.LayerList[i];
							m_objXMLHandle.CreateElement("Rule");
							m_objXMLHandle.CreateElement("RuleName");
                            m_objXMLHandle.SetElementText(System.Convert.ToString(objStructSR.DatasetName));
							m_objXMLHandle.CreateElement("Title");
                            m_objXMLHandle.SetElementText(System.Convert.ToString(objStructSR.DatasetName));
							switch (objStructSR.FeatureCls)
							{
								case Analize_ArcMap_Symbols.FeatureClass.PointFeature:
									WritePointFeatures(objSymbols[j]);
									break;
								case Analize_ArcMap_Symbols.FeatureClass.LineFeature:
									WriteLineFeatures(objSymbols[j]);
									break;
								case Analize_ArcMap_Symbols.FeatureClass.PolygonFeature:
									WritePolygonFeatures(objSymbols[j]);
									break;
							}
                            WriteAnnotation(objStructSR.Annotation);
						}
					}
					if (bDoOneLayer == false)
					{
						m_objXMLHandle.SaveDoc(); //If separate layer, the files have to be saved here
					}
				}
				if (bDoOneLayer == true)
				{
					m_objXMLHandle.SaveDoc(); //else the file has to be saved here
				}
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("Konnte die SLD nicht schreiben", ex.Message, ex.StackTrace, "WriteToSLD");
				return false;
			}
		}
		
		private bool WriteAnnotation(Analize_ArcMap_Symbols.StructAnnotation Annotation)
		{
			if (Annotation.IsSingleProperty && Annotation.PropertyName != "")
			{
				m_objXMLHandle.CreateElement("TextSymbolizer");
				m_objXMLHandle.CreateElement("TextLabel");
				m_objXMLHandle.CreateElement("TextLabelProperty");
				m_objXMLHandle.SetElementText(Annotation.PropertyName);
				m_objXMLHandle.CreateElement("TextFont");
				m_objXMLHandle.CreateElement("TextFontCssParameter");
				m_objXMLHandle.CreateAttribute("name");
				m_objXMLHandle.SetAttributeValue("font-family");
				m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("TextFont", Annotation.TextSymbol));
				if (GetValueFromSymbolstruct("TextFontAlt", Annotation.TextSymbol) != "")
				{
					m_objXMLHandle.CreateElement("TextFontCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("font-family");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("TextFontAlt", Annotation.TextSymbol));
				}
				m_objXMLHandle.CreateElement("TextFontCssParameter");
				m_objXMLHandle.CreateAttribute("name");
				m_objXMLHandle.SetAttributeValue("font-size");
				m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("TextFontSize", Annotation.TextSymbol));
				m_objXMLHandle.CreateElement("TextFontCssParameter");
				m_objXMLHandle.CreateAttribute("name");
				m_objXMLHandle.SetAttributeValue("font-style");
				m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("TextFontStyle", Annotation.TextSymbol));
				m_objXMLHandle.CreateElement("TextFontCssParameter");
				m_objXMLHandle.CreateAttribute("name");
				m_objXMLHandle.SetAttributeValue("font-weight");
				m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("TextFontWeight", Annotation.TextSymbol));
				m_objXMLHandle.CreateElement("TextFill");
				m_objXMLHandle.CreateElement("TextFillCssParameter");
				m_objXMLHandle.CreateAttribute("name");
				m_objXMLHandle.SetAttributeValue("fill");
				m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("TextColor", Annotation.TextSymbol));
				m_objXMLHandle.CreateElement("TextFillCssParameter");
				m_objXMLHandle.CreateAttribute("name");
				m_objXMLHandle.SetAttributeValue("fill-opacity");
				m_objXMLHandle.SetElementText("1.0");
			}
			return true;
		}
		
		//************************************************************************************************
		//This function writes the SLD features for a PointSymbolizer based on the passed (point)symbol.
		//Parameter:
		//           Symbol=Die Symbolstruktur des aktuellen Symbols aus der Symbolsammlung
		//************************************************************************************************
		private bool WritePointFeatures(object Symbol)
		{
			try
			{
				int layerIdx = 0;
				int maxLayerIdx = 1;
				
				if (Symbol is Analize_ArcMap_Symbols.StructMultilayerMarkerSymbol)
				{
					Analize_ArcMap_Symbols.StructMultilayerMarkerSymbol objTempStruct = new Analize_ArcMap_Symbols.StructMultilayerMarkerSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructMultilayerMarkerSymbol)Symbol;
					maxLayerIdx = objTempStruct.LayerCount;
				}
				for (layerIdx = 0; layerIdx <= maxLayerIdx - 1; layerIdx++)
				{
					m_objXMLHandle.CreateElement("PointSymbolizer");
					m_objXMLHandle.CreateElement("PointGraphic");
					m_objXMLHandle.CreateElement("Mark");
					m_objXMLHandle.CreateElement("PointWellKnownName");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("WellKnownName", Symbol, layerIdx));
					if (GetValueFromSymbolstruct("PointColor", Symbol, layerIdx) != "")
					{
						m_objXMLHandle.CreateElement("PointFill");
						m_objXMLHandle.CreateElement("PointFillCssParameter");
						m_objXMLHandle.CreateAttribute("name");
						m_objXMLHandle.SetAttributeValue("fill");
						m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PointColor", Symbol, layerIdx));
						m_objXMLHandle.CreateElement("PointFillCssParameter");
						m_objXMLHandle.CreateAttribute("name");
						m_objXMLHandle.SetAttributeValue("fill-opacity");
						m_objXMLHandle.SetElementText("1.0");
					}
					if (GetValueFromSymbolstruct("PointOutlineColor", Symbol, layerIdx) != "" && GetValueFromSymbolstruct("PointOutlineSize", Symbol, layerIdx) != "0")
					{
						m_objXMLHandle.CreateElement("PointStroke");
						m_objXMLHandle.CreateElement("PointStrokeCssParameter");
						m_objXMLHandle.CreateAttribute("name");
						m_objXMLHandle.SetAttributeValue("stroke");
						m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PointOutlineColor", Symbol, layerIdx));
						m_objXMLHandle.CreateElement("PointStrokeCssParameter");
						m_objXMLHandle.CreateAttribute("name");
						m_objXMLHandle.SetAttributeValue("stroke-width");
						m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PointOutlineSize", Symbol, layerIdx));
						m_objXMLHandle.CreateElement("PointStrokeCssParameter");
						m_objXMLHandle.CreateAttribute("name");
						m_objXMLHandle.SetAttributeValue("stroke-opacity");
						m_objXMLHandle.SetElementText("1.0");
					}
					m_objXMLHandle.CreateElement("PointSize");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PointSize", Symbol, layerIdx));
					m_objXMLHandle.CreateElement("PointRotation");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PointRotation", Symbol, layerIdx));
				}
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Schreiben der Schraffur", ex.Message, ex.StackTrace, "WritePointFeatures");
				return false;
			}
		}
		
		//************************************************************************************************
		//This function writes the SLD features for a LineSymbolizer based on the passed (line)symbol.
		//Parameter:
		//           Symbol=Die Symbolstruktur des aktuellen Symbols aus der Symbolsammlung
		//************************************************************************************************
		private bool WriteLineFeatures(object Symbol)
		{
			try
			{
				int layerIdx = 0;
				int maxLayerIdx = 1;
				
				if (Symbol is Analize_ArcMap_Symbols.StructMultilayerLineSymbol)
				{
					Analize_ArcMap_Symbols.StructMultilayerLineSymbol objTempStruct = new Analize_ArcMap_Symbols.StructMultilayerLineSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructMultilayerLineSymbol)Symbol;
					maxLayerIdx = objTempStruct.LayerCount;
				}
				for (layerIdx = 0; layerIdx <= maxLayerIdx - 1; layerIdx++)
				{
					if (GetValueFromSymbolstruct("LineColor", Symbol, layerIdx) != "" && GetValueFromSymbolstruct("LineWidth", Symbol, layerIdx) != "0")
					{
						m_objXMLHandle.CreateElement("LineSymbolizer");
						m_objXMLHandle.CreateElement("LineStroke");
						m_objXMLHandle.CreateElement("LineCssParameter");
						m_objXMLHandle.CreateAttribute("name");
						m_objXMLHandle.SetAttributeValue("stroke");
						m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("LineColor", Symbol, layerIdx));
						m_objXMLHandle.CreateElement("LineCssParameter");
						m_objXMLHandle.CreateAttribute("name");
						m_objXMLHandle.SetAttributeValue("stroke-width");
						m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("LineWidth", Symbol, layerIdx));
						m_objXMLHandle.CreateElement("LineCssParameter");
						m_objXMLHandle.CreateAttribute("name");
						m_objXMLHandle.SetAttributeValue("stroke-opacity");
						m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("LineOpacity", Symbol, layerIdx));
						if (GetValueFromSymbolstruct("LineDashArray", Symbol, layerIdx) != "")
						{
							m_objXMLHandle.CreateElement("LineCssParameter");
							m_objXMLHandle.CreateAttribute("name");
							m_objXMLHandle.SetAttributeValue("stroke-dasharray");
							m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("LineDashArray", Symbol, layerIdx));
						}
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Schreiben der Schraffur", ex.Message, ex.StackTrace, "WriteLineFeatures");
				return false;
			}
		}
		
		//************************************************************************************************
		//Die Funktion 黚ernimmt das aktuelle Symbol und verteilt es auf die Unterfunktionen zum schreiben
		//Bei MultilayerFillsymbol rekursiver Aufruf
		//Parameter:
		//           Symbol=Die Symbolstruktur des aktuellen Symbols aus der Symbolsammlung
		//************************************************************************************************
		private bool WritePolygonFeatures(object Symbol)
		{
			// WriteSolidFill(Symbol)
			int i = 0;
            int iSecure = 0;
			try
			{
				if (Symbol is Analize_ArcMap_Symbols.StructSimpleFillSymbol)
				{
					WriteSolidFill(Symbol);
				}
				else if (Symbol is Analize_ArcMap_Symbols.StructMarkerFillSymbol)
				{
					WriteMarkerFill(Symbol);
				}
				else if (Symbol is Analize_ArcMap_Symbols.StructLineFillSymbol)
				{
					//Die Winkel der Schraffuren Original:(Schr鋑/Horizontal,Vertikal)-SLD(Kreuzschraffur schr鋑/Kreuzschraffur Achsparallel)
                    Analize_ArcMap_Symbols.StructLineFillSymbol tempSymbol = (Analize_ArcMap_Symbols.StructLineFillSymbol)Symbol;
                    if (tempSymbol.Angle > 22.5 && tempSymbol.Angle < 67.5)
					{
                        WriteSlopedHatching(tempSymbol);
					}
                    else if (tempSymbol.Angle > 67.5 && tempSymbol.Angle < 112.5)
					{
                        WritePerpendicularHatching(tempSymbol);
					}
                    else if (tempSymbol.Angle > 112.5 && tempSymbol.Angle < 157.5)
					{
                        WriteSlopedHatching(tempSymbol);
					}
                    else if (tempSymbol.Angle > 157.5 && tempSymbol.Angle < 202.5)
					{
                        WritePerpendicularHatching(tempSymbol);
					}
                    else if (tempSymbol.Angle > 202.5 && tempSymbol.Angle < 247.5)
					{
                        WriteSlopedHatching(tempSymbol);
					}
                    else if (tempSymbol.Angle > 247.5 && tempSymbol.Angle < 292.5)
					{
                        WritePerpendicularHatching(tempSymbol);
					}
                    else if (tempSymbol.Angle > 292.5 && tempSymbol.Angle < 337.5)
					{
                        WriteSlopedHatching(tempSymbol);
					}
                    else if (tempSymbol.Angle > 337.5 && tempSymbol.Angle <= 360.0 || tempSymbol.Angle >= 0.0 && tempSymbol.Angle < 22.5)
					{
                        WritePerpendicularHatching(tempSymbol);
					}
				}
				else if (Symbol is Analize_ArcMap_Symbols.StructDotDensityFillSymbol)
				{
					WriteMarkerFill(Symbol); //Ist z.Zt. sowieso nicht m鰃lich, Dichte mit Punktf黮lungen auszugeben
				}
				else if (Symbol is Analize_ArcMap_Symbols.StructPictureFillSymbol)
				{
					//TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO
				}
				else if (Symbol is Analize_ArcMap_Symbols.StructGradientFillSymbol)
				{
					//TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO
				}
				else if (Symbol is Analize_ArcMap_Symbols.StructMultilayerFillSymbol)
				{
					Analize_ArcMap_Symbols.StructMultilayerFillSymbol MFS = new Analize_ArcMap_Symbols.StructMultilayerFillSymbol();
                    MFS = (Analize_ArcMap_Symbols.StructMultilayerFillSymbol)Symbol;
					bool bSwitch; //Wenn mehr als 3 Symbollayer sind, und einer davon ist ein SimpleFill
					bSwitch = false;
					//Hier muss aufgepasst werden: Manche Mapserver k鰊nen nur 2 Symbollayer 黚ereinander abbilden. Deshalb werden derzeit nur 2 Symbollayer gebildet
					if (MFS.LayerCount == 1)
					{
						WritePolygonFeatures(MFS.MultiFillLayers[0]);
					}
					else if (MFS.LayerCount == 2)
					{
						for (i = MFS.LayerCount - 1; i >= 0; i--)
						{
							WritePolygonFeatures(MFS.MultiFillLayers[i]); //hier rekursiver Aufruf
						}
					}
					else if (MFS.LayerCount > 2)
					{
						for (i = MFS.LayerCount - 1; i >= 0; i--)
						{
							if (iSecure <= 1)
							{
								WritePolygonFeatures(MFS.MultiFillLayers[i]);
							}
							iSecure++;
						}
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Schreiben der Schraffur", ex.Message, ex.StackTrace, "WritePolygonFeatures");
				return false;
			}
		}
		
		
		//************************************************************************************************
		//Die Funktion schreibt die SLD-Anweisung f黵 eine SOLID COLOR FL腃HENF躄LUNG
		//und 黚ernimmt die Eigenschaften aus der SimpleFill-Datenstruktur
		//Parameter:
		//           Symbol=Die Symbolstruktur des aktuellen Symbols aus der Symbolsammlung
		//************************************************************************************************
		private bool WriteSolidFill(object Symbol)
		{
			try
			{
				m_objXMLHandle.CreateElement("PolygonSymbolizer");
				if (GetValueFromSymbolstruct("PolygonColor", Symbol) != "")
				{
					m_objXMLHandle.CreateElement("Fill");
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("fill");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonColor", Symbol));
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("fill-opacity");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonOpacity", Symbol));
				}
				if (GetValueFromSymbolstruct("PolygonBorderColor", Symbol) != "" && GetValueFromSymbolstruct("PolygonBorderWidth", Symbol) != "0")
				{
					m_objXMLHandle.CreateElement("PolygonStroke");
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("stroke");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonBorderColor", Symbol));
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("stroke-width");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonBorderWidth", Symbol));
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("stroke-opacity");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonBorderOpacity", Symbol));
				}
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Schreiben der Fl鋍henf黮lung", ex.Message, ex.StackTrace, "WriteSimpleFill");
				return false;
			}
			
		}
		
		//************************************************************************************************
		//Die Funktion schreibt die SLD-Anweisung f黵 eine GEPUNKTETE FL腃HENF躄LUNG
		//und 黚ernimmt die Eigenschaften aus der MarkerFill-Datenstruktur (und z.Zt. der DotDensity-Strukt.)
		//Parameter:
		//           Symbol=Die Symbolstruktur des aktuellen Symbols aus der Symbolsammlung
		//************************************************************************************************
		private bool WriteMarkerFill(object Symbol)
		{
			try
			{
				m_objXMLHandle.CreateElement("PolygonSymbolizer");
				if (GetValueFromSymbolstruct("PointColor", Symbol) != "")
				{
					m_objXMLHandle.CreateElement("Fill");
					m_objXMLHandle.CreateElement("PolygonGraphicFill");
					m_objXMLHandle.CreateElement("PolygonGraphic");
					m_objXMLHandle.CreateElement("PolygonMark");
					m_objXMLHandle.CreateElement("PolygonSize");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PointSize", Symbol));
					m_objXMLHandle.CreateElement("PolygonWellKnownName");
					m_objXMLHandle.SetElementText("circle");
					m_objXMLHandle.CreateElement("PolygonGraphicParamFill");
					m_objXMLHandle.CreateElement("PolygonGraphicCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("fill");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PointColor", Symbol));
					m_objXMLHandle.CreateElement("PolygonGraphicCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("fill-opacity");
					m_objXMLHandle.SetElementText("1.0");
				}
				if (GetValueFromSymbolstruct("PolygonBorderColor", Symbol) != "" && GetValueFromSymbolstruct("PolygonBorderWidth", Symbol) != "0")
				{
					m_objXMLHandle.CreateElement("PolygonStroke");
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("stroke");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonBorderColor", Symbol));
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("stroke-width");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonBorderWidth", Symbol));
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("stroke-opacity");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonBorderOpacity", Symbol));
				}
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Schreiben der Punktf黮lung", ex.Message, ex.StackTrace, "WriteMarkerFill");
				return false;
			}
			
		}
		
		//************************************************************************************************
		//Die Funktion schreibt die SLD-Anweisung f黵 eine SCHR腉E KREUZSCHRAFFUR-FL腃HENF躄LUNG
		//und 黚ernimmt die Eigenschaften aus der LineFill-Datenstruktur.
		//Parameter:
		//           Symbol=Die Symbolstruktur des aktuellen Symbols aus der Symbolsammlung
		//************************************************************************************************
        private bool WriteSlopedHatching(Analize_ArcMap_Symbols.StructLineFillSymbol Symbol)
		{
			double dDummy = 0;
			try
			{
				m_objXMLHandle.CreateElement("PolygonSymbolizer");
				if (Symbol.Color != "")
				{
					m_objXMLHandle.CreateElement("Fill");
					m_objXMLHandle.CreateElement("PolygonGraphicFill");
					m_objXMLHandle.CreateElement("PolygonGraphic");
					//.SetElementText(GetValueFromSymbolstruct("LineWidth", Symbol))
					m_objXMLHandle.CreateElement("PolygonMark");
					m_objXMLHandle.CreateElement("PolygonSize");
					//Schraffurgr鲞e
					dDummy = System.Convert.ToDouble(Symbol.Separation + 5);
					m_objXMLHandle.SetElementText(CommaToPoint(dDummy));
					m_objXMLHandle.CreateElement("PolygonWellKnownName");
					m_objXMLHandle.SetElementText("x"); //Macht die schr鋑e Kreuzschraffur
					m_objXMLHandle.CreateElement("PolygonGraphicParamFill");
					m_objXMLHandle.CreateElement("PolygonGraphicCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("fill");
					//Die Schraffurfarbe
					m_objXMLHandle.SetElementText(System.Convert.ToString(Symbol.Color));
					//.SetElementText(GetValueFromSymbolstruct("LineColor", Symbol))
					m_objXMLHandle.CreateElement("PolygonGraphicCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("fill-opacity");
					m_objXMLHandle.SetElementText("1.0");
				}
				if (GetValueFromSymbolstruct("PolygonBorderColor", Symbol) != "" && GetValueFromSymbolstruct("PolygonBorderWidth", Symbol) != "0")
				{
					m_objXMLHandle.CreateElement("PolygonStroke");
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("stroke");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonBorderColor", Symbol));
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("stroke-width");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonBorderWidth", Symbol));
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("stroke-opacity");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonBorderOpacity", Symbol));
				}
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Schreiben der Schraffur", ex.Message, ex.StackTrace, "WriteSlopedHatching");
				return false;
			}
		}
		
		
		//************************************************************************************************
		//Die Funktion schreibt die SLD-Anweisung f黵 eine SENKRECHTE KREUZSCHRAFFUR-FL腃HENF躄LUNG
		//und 黚ernimmt die Eigenschaften aus der LineFill-Datenstruktur.
		//Parameter:
		//           Symbol=Die Symbolstruktur des aktuellen Symbols aus der Symbolsammlung
		//************************************************************************************************
        private bool WritePerpendicularHatching(Analize_ArcMap_Symbols.StructLineFillSymbol Symbol)
		{
			double dDummy = 0;
			try
			{
				m_objXMLHandle.CreateElement("PolygonSymbolizer");
				if (Symbol.Color != "")
				{
					m_objXMLHandle.CreateElement("Fill");
					m_objXMLHandle.CreateElement("PolygonGraphicFill");
					m_objXMLHandle.CreateElement("PolygonGraphic");
					//.SetElementText(GetValueFromSymbolstruct("LineWidth", Symbol))
					m_objXMLHandle.CreateElement("PolygonMark");
					m_objXMLHandle.CreateElement("PolygonSize");
					//Schraffurgr鲞e
					dDummy = System.Convert.ToDouble(Symbol.Separation + 5);
					m_objXMLHandle.SetElementText(CommaToPoint(dDummy));
					m_objXMLHandle.CreateElement("PolygonWellKnownName");
					m_objXMLHandle.SetElementText("cross");
					m_objXMLHandle.CreateElement("PolygonGraphicParamFill");
					m_objXMLHandle.CreateElement("PolygonGraphicCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("fill");
					//Die Schraffurfarbe
					m_objXMLHandle.SetElementText(System.Convert.ToString(Symbol.Color));
					//.SetElementText(GetValueFromSymbolstruct("LineColor", Symbol))
					m_objXMLHandle.CreateElement("PolygonGraphicCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("fill-opacity");
					m_objXMLHandle.SetElementText("1.0");
				}
				if (GetValueFromSymbolstruct("PolygonBorderColor", Symbol) != "" && GetValueFromSymbolstruct("PolygonBorderWidth", Symbol) != "0")
				{
					m_objXMLHandle.CreateElement("PolygonStroke");
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("stroke");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonBorderColor", Symbol));
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("stroke-width");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonBorderWidth", Symbol));
					m_objXMLHandle.CreateElement("PolyCssParameter");
					m_objXMLHandle.CreateAttribute("name");
					m_objXMLHandle.SetAttributeValue("stroke-opacity");
					m_objXMLHandle.SetElementText(GetValueFromSymbolstruct("PolygonBorderOpacity", Symbol));
				}
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Schreiben der Punktf黮lung", ex.Message, ex.StackTrace, "WritePerpendicularHatching");
				return false;
			}
		}
		
#endregion
		
		
#region Symbolstructanalysefunktionen
		
		
		//******************************************************************************************************************
		//DIESE FUNKTION GILT Z.ZT. NUR ZU TESTZWECKEN UND MUSS IM REALEN GEBRAUCH VERVOLLST腘DIGT WERDEN !!!
		//1. Die Funktion extrahiert die gew黱schten Werte aus den jeweiligen Datenstrukturen
		//2. Hier werden die Generalisierungen gemacht von der komplexen ArcMap-Symbolisierung zur einfachen sld-Symbolisierung
		//TODO: Vervollst鋘digung der case's f黵 jede Struktur
		//Parameter:     SymbolStructure= die jeweilige Symbol-Datenstruktur
		//               ValueNameOfValueYouWant= ein String, der den Wert repr鋝entiert, den man erhalten m鯿hte
		//z.Zt. m鰃liche Werte f黵 ValueNameOfValueYouWant sind:
		//F黵 Polygone:  "PolygonColor, PolygonBorderWidth, PolygonBorderColor, PointSize, PointColor, LineWidth, LineColor"
		//F黵 Linien:    "LineWidth, LineColor"
		//F黵 Punkte:    "PointColor, PointSize, PointRotation, PointOutlineColor, WellKnownName"
		//******************************************************************************************************************
		private string GetValueFromSymbolstruct(string ValueNameOfValueYouWant, object SymbolStructure)
		{
			return GetValueFromSymbolstruct(ValueNameOfValueYouWant, SymbolStructure, 0);
		}
		
		//******************************************************************************************************************
		//DIESE FUNKTION GILT Z.ZT. NUR ZU TESTZWECKEN UND MUSS IM REALEN GEBRAUCH VERVOLLST腘DIGT WERDEN !!!
		//1. Die Funktion extrahiert die gew黱schten Werte aus den jeweiligen Datenstrukturen
		//2. Hier werden die Generalisierungen gemacht von der komplexen ArcMap-Symbolisierung zur einfachen sld-Symbolisierung
		//TODO: Vervollst鋘digung der case's f黵 jede Struktur
		//Parameter:     SymbolStructure= die jeweilige Symbol-Datenstruktur
		//               ValueNameOfValueYouWant= ein String, der den Wert repr鋝entiert, den man erhalten m鯿hte
		//               LayerIdx = for multilayer symbols, the index of the layer to use.
		//z.Zt. m鰃liche Werte f黵 ValueNameOfValueYouWant sind:
		//F黵 Polygone:  "PolygonColor, PolygonBorderWidth, PolygonBorderColor, PointSize, PointColor, LineWidth, LineColor"
		//F黵 Linien:    "LineWidth, LineColor"
		//F黵 Punkte:    "PointColor, PointSize, PointRotation, PointOutlineColor, WellKnownName"
		//******************************************************************************************************************
		private string GetValueFromSymbolstruct(string ValueNameOfValueYouWant, object SymbolStructure, int LayerIdx)
		{
			string cReturn = "";
			bool bSwitch;
			bSwitch = false; //(ben鰐igt f黵 Multilayersymbole)der Schalter wird umgelegt, wenn es kein simple.. Symbol gibt. Dann wird der Wert des ersten Symbols genommen
			cReturn = "0"; //Wenn keiner der 黚ergebenen ValueNames passt, wird 0 zur點kgegeben
			try
			{
				if (SymbolStructure is Analize_ArcMap_Symbols.StructSimpleMarkerSymbol)
				{
					cReturn = GetMarkerValue(ValueNameOfValueYouWant, SymbolStructure);
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructCharacterMarkerSymbol)
				{
					cReturn = GetMarkerValue(ValueNameOfValueYouWant, SymbolStructure);
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructPictureMarkerSymbol)
				{
					cReturn = GetMarkerValue(ValueNameOfValueYouWant, SymbolStructure);
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructArrowMarkerSymbol)
				{
					cReturn = GetMarkerValue(ValueNameOfValueYouWant, SymbolStructure);
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructSimpleLineSymbol)
				{
					Analize_ArcMap_Symbols.StructSimpleLineSymbol objTempStruct = new Analize_ArcMap_Symbols.StructSimpleLineSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructSimpleLineSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "LineWidth".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.Width);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineColor".ToUpper())
					{
						cReturn = objTempStruct.Color;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineOpacity".ToUpper())
					{
						double tmpTransparency = objTempStruct.Transparency;
						cReturn = CommaToPoint(tmpTransparency / 255.0);
						//Case "PointRotation".ToUpper
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineDashArray".ToUpper())
					{
						switch (objTempStruct.publicStyle)
						{
							case "esriSLSDash":
								cReturn = "10.0 10.0";
								break;
							case "esriSLSDashDot":
								cReturn = "10.0 10.0 1.0 10.0";
								break;
							case "esriSLSDashDotDot":
								cReturn = "10.0 10.0 1.0 10.0 1.0 10.0";
								break;
							case "esriSLSDot":
								cReturn = "1.0 5.0";
								break;
							default:
								cReturn = "";
								break;
						}
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructCartographicLineSymbol)
				{
					Analize_ArcMap_Symbols.StructCartographicLineSymbol objTempStruct = new Analize_ArcMap_Symbols.StructCartographicLineSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructCartographicLineSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "LineWidth".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.Width);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineColor".ToUpper())
					{
						cReturn = objTempStruct.Color;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineOpacity".ToUpper())
					{
						double tmpTransparency = objTempStruct.Transparency;
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineDashArray".ToUpper())
					{
						int dashIdx = 0;
						double size = 0;
						cReturn = "";
						for (dashIdx = 0; dashIdx <= objTempStruct.DashArray.Count - 1; dashIdx++)
						{
							if (dashIdx > 0)
							{
								cReturn = cReturn + " ";
							}
							size = System.Convert.ToDouble(objTempStruct.DashArray[dashIdx]);
							cReturn = cReturn + CommaToPoint(size);
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "")
					{
					}
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructHashLineSymbol)
				{
					Analize_ArcMap_Symbols.StructHashLineSymbol objTempStruct = new Analize_ArcMap_Symbols.StructHashLineSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructHashLineSymbol)SymbolStructure;
					if ((ValueNameOfValueYouWant.ToUpper() == "LineWidth".ToUpper()) || (ValueNameOfValueYouWant.ToUpper() == "LineDashArray".ToUpper()))
					{
						switch (objTempStruct.kindOfLineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructCartographicLineSymbol:
								cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.HashSymbol_CartographicLine);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.HashSymbol_MarkerLine);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.HashSymbol_MultiLayerLines, LayerIdx);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.HashSymbol_PictureLine);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.HashSymbol_SimpleLine);
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineColor".ToUpper())
					{
						cReturn = objTempStruct.Color;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineOpacity".ToUpper())
					{
						double tmpTransparency = objTempStruct.Transparency;
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "")
					{
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructMarkerLineSymbol)
				{
					Analize_ArcMap_Symbols.StructMarkerLineSymbol objTempStruct = new Analize_ArcMap_Symbols.StructMarkerLineSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructMarkerLineSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "LineWidth".ToUpper())
					{
						InfoMsg("Abfrage von Linienbreite der Markerlines ist im Augenblick nicht implementiert", "GetValueFromSymbolstruct");
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineColor".ToUpper())
					{
						cReturn = objTempStruct.Color;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineOpacity".ToUpper())
					{
						double tmpTransparency = objTempStruct.Transparency;
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "")
					{
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructPictureLineSymbol)
				{
					Analize_ArcMap_Symbols.StructPictureLineSymbol objTempStruct = new Analize_ArcMap_Symbols.StructPictureLineSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructPictureLineSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "LineWidth".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.Width);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineColor".ToUpper())
					{
						cReturn = objTempStruct.BackgroundColor;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineOpacity".ToUpper())
					{
						double tmpTransparency = objTempStruct.BackgroundTransparency;
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "")
					{
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructSimpleFillSymbol)
				{
					Analize_ArcMap_Symbols.StructSimpleFillSymbol objTempStruct = new Analize_ArcMap_Symbols.StructSimpleFillSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructSimpleFillSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "PolygonColor".ToUpper())
					{
						cReturn = objTempStruct.Color;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonOpacity".ToUpper())
					{
						double tmpTransparency = objTempStruct.Transparency;
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderWidth".ToUpper())
					{
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_HashLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_MarkerLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineWidth", objTempStruct.Outline_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_PictureLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_SimpleLine.Width);
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderColor".ToUpper())
					{
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = objTempStruct.Outline_HashLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = objTempStruct.Outline_MarkerLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineColor", objTempStruct.Outline_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = objTempStruct.Outline_PictureLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = objTempStruct.Outline_SimpleLine.Color;
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderOpacity".ToUpper())
					{
						double tmpTransparency = 255.0;
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								tmpTransparency = objTempStruct.Outline_HashLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								tmpTransparency = objTempStruct.Outline_MarkerLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								tmpTransparency = System.Convert.ToDouble(255 * double.Parse(GetValueFromSymbolstruct("LineOpacity", objTempStruct.Outline_MultiLayerLines)));
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								tmpTransparency = objTempStruct.Outline_PictureLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								tmpTransparency = objTempStruct.Outline_SimpleLine.Transparency;
								break;
						}
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructMarkerFillSymbol)
				{
					Analize_ArcMap_Symbols.StructMarkerFillSymbol objTempStruct = new Analize_ArcMap_Symbols.StructMarkerFillSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructMarkerFillSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "PolygonColor".ToUpper())
					{
						cReturn = objTempStruct.Color;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonOpacity".ToUpper())
					{
						double tmpTransparency = objTempStruct.Transparency;
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderWidth".ToUpper())
					{
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_HashLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_MarkerLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineWidth", objTempStruct.Outline_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_PictureLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_SimpleLine.Width);
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderColor".ToUpper())
					{
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = objTempStruct.Outline_HashLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = objTempStruct.Outline_MarkerLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineColor", objTempStruct.Outline_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = objTempStruct.Outline_PictureLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = objTempStruct.Outline_SimpleLine.Color;
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderOpacity".ToUpper())
					{
						double tmpTransparency = 255.0;
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								tmpTransparency = objTempStruct.Outline_HashLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								tmpTransparency = objTempStruct.Outline_MarkerLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								tmpTransparency = System.Convert.ToDouble(255 * double.Parse(GetValueFromSymbolstruct("LineOpacity", objTempStruct.Outline_MultiLayerLines)));
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								tmpTransparency = objTempStruct.Outline_PictureLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								tmpTransparency = objTempStruct.Outline_SimpleLine.Transparency;
								break;
						}
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointSize".ToUpper())
					{
						switch (objTempStruct.kindOfMarkerStruct)
						{
							case Analize_ArcMap_Symbols.MarkerStructs.StructArrowMarkerSymbol:
								cReturn = CommaToPoint(objTempStruct.MarkerSymbol_ArrowMarker.Size);
								break;
							case Analize_ArcMap_Symbols.MarkerStructs.StructCharacterMarkerSymbol:
								cReturn = CommaToPoint(objTempStruct.MarkerSymbol_CharacterMarker.Size);
								break;
							case Analize_ArcMap_Symbols.MarkerStructs.StructMultilayerMarkerSymbol:
								cReturn = GetValueFromSymbolstruct("PointSize", objTempStruct.MarkerSymbol_MultilayerMarker);
								break;
							case Analize_ArcMap_Symbols.MarkerStructs.StructPictureMarkerSymbol:
								cReturn = CommaToPoint(objTempStruct.MarkerSymbol_PictureMarker.Size);
								break;
							case Analize_ArcMap_Symbols.MarkerStructs.StructSimpleMarkerSymbol:
								cReturn = CommaToPoint(objTempStruct.MarkerSymbol_SimpleMarker.Size);
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointColor".ToUpper())
					{
						switch (objTempStruct.kindOfMarkerStruct)
						{
							case Analize_ArcMap_Symbols.MarkerStructs.StructArrowMarkerSymbol:
								cReturn = objTempStruct.MarkerSymbol_ArrowMarker.Color;
								break;
							case Analize_ArcMap_Symbols.MarkerStructs.StructCharacterMarkerSymbol:
								cReturn = objTempStruct.MarkerSymbol_CharacterMarker.Color;
								break;
							case Analize_ArcMap_Symbols.MarkerStructs.StructMultilayerMarkerSymbol:
								cReturn = GetValueFromSymbolstruct("PointColor", objTempStruct.MarkerSymbol_MultilayerMarker);
								break;
							case Analize_ArcMap_Symbols.MarkerStructs.StructPictureMarkerSymbol:
								cReturn = objTempStruct.MarkerSymbol_PictureMarker.Color;
								break;
							case Analize_ArcMap_Symbols.MarkerStructs.StructSimpleMarkerSymbol:
								cReturn = objTempStruct.MarkerSymbol_SimpleMarker.Color;
								break;
						}
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructLineFillSymbol)
				{
					Analize_ArcMap_Symbols.StructLineFillSymbol objTempStruct = new Analize_ArcMap_Symbols.StructLineFillSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructLineFillSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "PolygonColor".ToUpper())
					{
						cReturn = objTempStruct.Color;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonOpacity".ToUpper())
					{
						double tmpTransparency = objTempStruct.Transparency;
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderWidth".ToUpper())
					{
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_HashLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_MarkerLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineWidth", objTempStruct.Outline_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_PictureLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_SimpleLine.Width);
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderColor".ToUpper())
					{
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = objTempStruct.Outline_HashLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = objTempStruct.Outline_MarkerLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineColor", objTempStruct.Outline_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = objTempStruct.Outline_PictureLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = objTempStruct.Outline_SimpleLine.Color;
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderOpacity".ToUpper())
					{
						double tmpTransparency = 255.0;
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								tmpTransparency = objTempStruct.Outline_HashLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								tmpTransparency = objTempStruct.Outline_MarkerLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								tmpTransparency = System.Convert.ToDouble(255 * double.Parse(GetValueFromSymbolstruct("LineOpacity", objTempStruct.Outline_MultiLayerLines)));
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								tmpTransparency = objTempStruct.Outline_PictureLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								tmpTransparency = objTempStruct.Outline_SimpleLine.Transparency;
								break;
						}
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineWidth".ToUpper())
					{
						switch (objTempStruct.kindOfLineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructCartographicLineSymbol:
								cReturn = CommaToPoint(objTempStruct.LineSymbol_CartographicLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = CommaToPoint(objTempStruct.LineSymbol_HashLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = CommaToPoint(objTempStruct.LineSymbol_MarkerLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineWidth", objTempStruct.LineSymbol_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = CommaToPoint(objTempStruct.LineSymbol_PictureLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = CommaToPoint(objTempStruct.LineSymbol_SimpleLine.Width);
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineColor".ToUpper())
					{
						switch (objTempStruct.kindOfLineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructCartographicLineSymbol:
								cReturn = objTempStruct.LineSymbol_CartographicLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = objTempStruct.LineSymbol_HashLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = objTempStruct.LineSymbol_MarkerLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineColor", objTempStruct.LineSymbol_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = objTempStruct.LineSymbol_PictureLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = objTempStruct.LineSymbol_SimpleLine.Color;
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "LineOpacity".ToUpper())
					{
						double tmpTransparency = 255.0;
						switch (objTempStruct.kindOfLineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructCartographicLineSymbol:
								tmpTransparency = objTempStruct.LineSymbol_CartographicLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								tmpTransparency = objTempStruct.LineSymbol_HashLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								tmpTransparency = objTempStruct.LineSymbol_MarkerLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								tmpTransparency = System.Convert.ToDouble(255 * double.Parse(GetValueFromSymbolstruct("LineOpacity", objTempStruct.LineSymbol_MultiLayerLines)));
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								tmpTransparency = objTempStruct.LineSymbol_PictureLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								tmpTransparency = objTempStruct.LineSymbol_SimpleLine.Transparency;
								break;
						}
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructDotDensityFillSymbol)
				{
					Analize_ArcMap_Symbols.StructDotDensityFillSymbol objTempStruct = new Analize_ArcMap_Symbols.StructDotDensityFillSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructDotDensityFillSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "PolygonColor".ToUpper())
					{
						cReturn = objTempStruct.BackgroundColor;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonOpacity".ToUpper())
					{
						double tmpTransparency = objTempStruct.BackgroundTransparency;
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderWidth".ToUpper())
					{
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_HashLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_MarkerLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineWidth", objTempStruct.Outline_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_PictureLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_SimpleLine.Width);
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderColor".ToUpper())
					{
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = objTempStruct.Outline_HashLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = objTempStruct.Outline_MarkerLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineColor", objTempStruct.Outline_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = objTempStruct.Outline_PictureLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = objTempStruct.Outline_SimpleLine.Color;
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderOpacity".ToUpper())
					{
						double tmpTransparency = 255.0;
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								tmpTransparency = objTempStruct.Outline_HashLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								tmpTransparency = objTempStruct.Outline_MarkerLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								tmpTransparency = System.Convert.ToDouble(255 * double.Parse(GetValueFromSymbolstruct("LineOpacity", objTempStruct.Outline_MultiLayerLines)));
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								tmpTransparency = objTempStruct.Outline_PictureLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								tmpTransparency = objTempStruct.Outline_SimpleLine.Transparency;
								break;
						}
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointSize".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.DotSize);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointColor".ToUpper())
					{
						cReturn = objTempStruct.Color;
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructPictureFillSymbol)
				{
					Analize_ArcMap_Symbols.StructPictureFillSymbol objTempStruct = new Analize_ArcMap_Symbols.StructPictureFillSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructPictureFillSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "PolygonColor".ToUpper())
					{
						cReturn = objTempStruct.BackgroundColor;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonOpacity".ToUpper())
					{
						double tmpTransparency = objTempStruct.BackgroundTransparency;
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderWidth".ToUpper())
					{
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_HashLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_MarkerLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineWidth", objTempStruct.Outline_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_PictureLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_SimpleLine.Width);
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderColor".ToUpper())
					{
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = objTempStruct.Outline_HashLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = objTempStruct.Outline_MarkerLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineColor", objTempStruct.Outline_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = objTempStruct.Outline_PictureLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = objTempStruct.Outline_SimpleLine.Color;
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderOpacity".ToUpper())
					{
						double tmpTransparency = 255.0;
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								tmpTransparency = objTempStruct.Outline_HashLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								tmpTransparency = objTempStruct.Outline_MarkerLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								tmpTransparency = System.Convert.ToDouble(255 * double.Parse(GetValueFromSymbolstruct("LineOpacity", objTempStruct.Outline_MultiLayerLines)));
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								tmpTransparency = objTempStruct.Outline_PictureLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								tmpTransparency = objTempStruct.Outline_SimpleLine.Transparency;
								break;
						}
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructGradientFillSymbol)
				{
					Analize_ArcMap_Symbols.StructGradientFillSymbol objTempStruct = new Analize_ArcMap_Symbols.StructGradientFillSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructGradientFillSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "PolygonColor".ToUpper())
					{
						cReturn = objTempStruct.Color;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonOpacity".ToUpper())
					{
						double tmpTransparency = objTempStruct.Transparency;
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderWidth".ToUpper())
					{
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_HashLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_MarkerLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineWidth", objTempStruct.Outline_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_PictureLine.Width);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = CommaToPoint(objTempStruct.Outline_SimpleLine.Width);
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderColor".ToUpper())
					{
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								cReturn = objTempStruct.Outline_HashLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								cReturn = objTempStruct.Outline_MarkerLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								cReturn = GetValueFromSymbolstruct("LineColor", objTempStruct.Outline_MultiLayerLines);
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								cReturn = objTempStruct.Outline_PictureLine.Color;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								cReturn = objTempStruct.Outline_SimpleLine.Color;
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderOpacity".ToUpper())
					{
						double tmpTransparency = 255.0;
						switch (objTempStruct.kindOfOutlineStruct)
						{
							case Analize_ArcMap_Symbols.LineStructs.StructHashLineSymbol:
								tmpTransparency = objTempStruct.Outline_HashLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMarkerLineSymbol:
								tmpTransparency = objTempStruct.Outline_MarkerLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructMultilayerLineSymbol:
								tmpTransparency = System.Convert.ToDouble(255 * double.Parse(GetValueFromSymbolstruct("LineOpacity", objTempStruct.Outline_MultiLayerLines)));
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructPictureLineSymbol:
								tmpTransparency = objTempStruct.Outline_PictureLine.Transparency;
								break;
							case Analize_ArcMap_Symbols.LineStructs.StructSimpleLineSymbol:
								tmpTransparency = objTempStruct.Outline_SimpleLine.Transparency;
								break;
						}
						cReturn = CommaToPoint(tmpTransparency / 255.0);
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructBarChartSymbol)
				{
					Analize_ArcMap_Symbols.StructBarChartSymbol objTempStruct;
                    objTempStruct = (Analize_ArcMap_Symbols.StructBarChartSymbol)SymbolStructure;
					switch (ValueNameOfValueYouWant.ToUpper())
					{
						case "":
							break;
							
						//case "":
							//break;
							
						//case "":
							//break;
							
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructPieChartSymbol)
				{
					Analize_ArcMap_Symbols.StructPieChartSymbol objTempStruct;
                    objTempStruct = (Analize_ArcMap_Symbols.StructPieChartSymbol)SymbolStructure;
					switch (ValueNameOfValueYouWant.ToUpper())
					{
						case "":
							break;
							
						//case "":
							//break;
							
						//case "":
							//break;
							
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructStackedChartSymbol)
				{
					Analize_ArcMap_Symbols.StructStackedChartSymbol objTempStruct;
                    objTempStruct = (Analize_ArcMap_Symbols.StructStackedChartSymbol)SymbolStructure;
					switch (ValueNameOfValueYouWant.ToUpper())
					{
						case "":
							break;
							
						//case "":
                            //break;
							
						//case "":
                            //break;
							
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructTextSymbol)
				{
					Analize_ArcMap_Symbols.StructTextSymbol objTempStruct = new Analize_ArcMap_Symbols.StructTextSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructTextSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "TextColor".ToUpper())
					{
						cReturn = objTempStruct.Color;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "TextFont".ToUpper())
					{
						cReturn = objTempStruct.Font;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "TextFontAlt".ToUpper())
					{
						switch (objTempStruct.Font.ToUpper())
						{
							case "ARIAL":
							case "ARIAL BLACK":
							case "HELVETICA":
							case "LUCIDA SANS UNICODE":
							case "MICROSOFT SANS SERIF":
							case "TAHOMA":
							case "VERDANA":
								cReturn = "Sans-Serif";
								break;
							case "COURIER":
							case "COURIER NEW":
							case "LUCIDA CONSOLE":
								cReturn = "Monospaced";
								break;
							case "PALATINO LINOTYPE":
							case "TIMES":
							case "TIMES NEW ROMAN":
								cReturn = "Serif";
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "TextFontSize".ToUpper())
					{
						cReturn = System.Convert.ToString(objTempStruct.Size);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "TextFontStyle".ToUpper())
					{
						cReturn = objTempStruct.Style;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "TextFontWeight".ToUpper())
					{
						cReturn = objTempStruct.Weight;
					}
					return cReturn;
					//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
					//Die Multilayer-Symbolstructs
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructMultilayerMarkerSymbol)
				{
					Analize_ArcMap_Symbols.StructMultilayerMarkerSymbol objTempStruct = new Analize_ArcMap_Symbols.StructMultilayerMarkerSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructMultilayerMarkerSymbol)SymbolStructure;
					cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.MultiMarkerLayers[LayerIdx]);
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructMultilayerLineSymbol)
				{
					Analize_ArcMap_Symbols.StructMultilayerLineSymbol objTempStruct = new Analize_ArcMap_Symbols.StructMultilayerLineSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructMultilayerLineSymbol)SymbolStructure;
					short i = 0;
					if (objTempStruct.LayerCount > 1)
					{
						for (i = 0; i <= objTempStruct.LayerCount - 1; i++)
						{
							if (objTempStruct.MultiLineLayers[i] is Analize_ArcMap_Symbols.StructSimpleLineSymbol)
							{
								Analize_ArcMap_Symbols.StructSimpleLineSymbol SLFS = new Analize_ArcMap_Symbols.StructSimpleLineSymbol();
                                SLFS = (Analize_ArcMap_Symbols.StructSimpleLineSymbol)objTempStruct.MultiLineLayers[i];
								cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, SLFS);
								bSwitch = true;
							}
						}
						if (bSwitch == false)
						{
							cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.MultiLineLayers[LayerIdx]);
						}
					}
					else
					{
						cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.MultiLineLayers[LayerIdx]);
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructMultilayerFillSymbol)
				{
					Analize_ArcMap_Symbols.StructMultilayerFillSymbol objTempStruct = new Analize_ArcMap_Symbols.StructMultilayerFillSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructMultilayerFillSymbol)SymbolStructure;
					short i = 0;
					if (objTempStruct.LayerCount > 1)
					{
						for (i = 0; i <= objTempStruct.LayerCount - 1; i++)
						{
							if (((((ValueNameOfValueYouWant.ToUpper() == "PolygonColor".ToUpper()) || (ValueNameOfValueYouWant.ToUpper() == "PolygonOpacity".ToUpper())) || (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderWidth".ToUpper())) || (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderColor".ToUpper())) || (ValueNameOfValueYouWant.ToUpper() == "PolygonBorderOpacity".ToUpper()))
							{
								if (objTempStruct.MultiFillLayers[i] is Analize_ArcMap_Symbols.StructSimpleFillSymbol)
								{
									Analize_ArcMap_Symbols.StructSimpleFillSymbol SSFS = new Analize_ArcMap_Symbols.StructSimpleFillSymbol();
                                    SSFS = (Analize_ArcMap_Symbols.StructSimpleFillSymbol)objTempStruct.MultiFillLayers[i];
									cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, SSFS);
									bSwitch = true;
								}
							}
							else if ((ValueNameOfValueYouWant.ToUpper() == "PointColor".ToUpper()) || (ValueNameOfValueYouWant.ToUpper() == "PointSize".ToUpper()))
							{
								if (objTempStruct.MultiFillLayers[i] is Analize_ArcMap_Symbols.StructMarkerFillSymbol)
								{
									Analize_ArcMap_Symbols.StructMarkerFillSymbol SMFS = new Analize_ArcMap_Symbols.StructMarkerFillSymbol();
                                    SMFS = (Analize_ArcMap_Symbols.StructMarkerFillSymbol)objTempStruct.MultiFillLayers[i];
									cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, SMFS);
									bSwitch = true;
								}
							}
							else if (((ValueNameOfValueYouWant.ToUpper() == "LineWidth".ToUpper()) || (ValueNameOfValueYouWant.ToUpper() == "LineColor".ToUpper())) || (ValueNameOfValueYouWant.ToUpper() == "LineOpacity".ToUpper()))
							{
								if (objTempStruct.MultiFillLayers[i] is Analize_ArcMap_Symbols.StructLineFillSymbol)
								{
									Analize_ArcMap_Symbols.StructLineFillSymbol SLFS = new Analize_ArcMap_Symbols.StructLineFillSymbol();
                                    SLFS = (Analize_ArcMap_Symbols.StructLineFillSymbol)objTempStruct.MultiFillLayers[i];
									cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, SLFS);
									bSwitch = true;
								}
							}
						}
						if (bSwitch == false)
						{
							cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.MultiFillLayers[LayerIdx]);
						}
					}
					else
					{
						cReturn = GetValueFromSymbolstruct(ValueNameOfValueYouWant, objTempStruct.MultiFillLayers[LayerIdx]);
					}
					return cReturn;
				}
				return cReturn;
			}
			catch (Exception ex)
			{
				ErrorMsg("Konnte den Wert aus der SymbolStruct nicht auswerten.", ex.Message, ex.StackTrace, "GetValueFromSymbolstruct");
                return cReturn;
			}
		}
		
		//******************************************************************************************************************
		//This function tries to guess the SLD marker information from the ESRI point symbol.
		//It is incomplete by default because ESRI has many font sets.
		//The mapping is imperfect since only five well known names exist, with so many symbols in ESRI.
		//Parameter:     SymbolStructure= The point Symbol-Datastructure
		//               ValueNameOfValueYouWant= ein String, der den Wert repr鋝entiert, den man erhalten m鯿hte
		//z.Zt. m鰃liche Werte f黵 ValueNameOfValueYouWant sind:
		//"PointColor, PointSize, PointRotation, PointOutlineColor, WellKnownName"
		//******************************************************************************************************************
		private string GetMarkerValue(string ValueNameOfValueYouWant, object SymbolStructure)
		{
			string cReturn = "";
			string cColor = "";
			string cOutlineColor = "";
			bool bSwitch;
			bSwitch = false; //(ben鰐igt f黵 Multilayersymbole)der Schalter wird umgelegt, wenn es kein simple.. Symbol gibt. Dann wird der Wert des ersten Symbols genommen
			cReturn = "0"; //Wenn keiner der 黚ergebenen ValueNames passt, wird 0 zur點kgegeben
			try
			{
				if (SymbolStructure is Analize_ArcMap_Symbols.StructSimpleMarkerSymbol)
				{
					Analize_ArcMap_Symbols.StructSimpleMarkerSymbol objTempStruct = new Analize_ArcMap_Symbols.StructSimpleMarkerSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructSimpleMarkerSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "WellKnownName".ToUpper())
					{
						switch (objTempStruct.Style)
						{
							case "esriSMSCircle":
								cReturn = "circle";
								break;
							case "esriSMSSquare":
								cReturn = "square";
								break;
							case "esriSMSCross":
								cReturn = "cross";
								break;
							case "esriSMSX":
								cReturn = "x";
								break;
							case "esriSMSDiamond":
								cReturn = "triangle";
								break;
							default:
								cReturn = "star";
								break;
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointColor".ToUpper())
					{
						if (objTempStruct.Filled)
						{
							cReturn = objTempStruct.Color;
						}
						else
						{
							cReturn = "";
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointSize".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.Size);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointRotation".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.Angle);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointOutlineColor".ToUpper())
					{
						if (objTempStruct.Outline)
						{
							cReturn = objTempStruct.OutlineColor;
						}
						else
						{
							cReturn = "";
						}
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointOutlineSize".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.OutlineSize);
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructCharacterMarkerSymbol)
				{
					Analize_ArcMap_Symbols.StructCharacterMarkerSymbol objTempStruct = new Analize_ArcMap_Symbols.StructCharacterMarkerSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructCharacterMarkerSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "WellKnownName".ToUpper())
					{
						cReturn = "circle"; //Default
						switch (objTempStruct.Font.ToUpper())
						{
							case "ESRI DEFAULT MARKER":
								if (((((((((((((((objTempStruct.CharacterIndex == 33) || (objTempStruct.CharacterIndex == 40)) || (objTempStruct.CharacterIndex == 46)) || (objTempStruct.CharacterIndex == 53)) || (objTempStruct.CharacterIndex >= 60 && objTempStruct.CharacterIndex <= 67)) || (objTempStruct.CharacterIndex == 72)) || (objTempStruct.CharacterIndex >= 79 && objTempStruct.CharacterIndex <= 82)) || (objTempStruct.CharacterIndex >= 90 && objTempStruct.CharacterIndex <= 93)) || (objTempStruct.CharacterIndex == 171)) || (objTempStruct.CharacterIndex == 172)) || (objTempStruct.CharacterIndex == 183)) || (objTempStruct.CharacterIndex == 196)) || (objTempStruct.CharacterIndex == 199)) || (objTempStruct.CharacterIndex == 200)) || (objTempStruct.CharacterIndex == 8729))
								{
									cReturn = "circle";
								}
								else if ((((((((((((((((objTempStruct.CharacterIndex == 34) || (objTempStruct.CharacterIndex == 41)) || (objTempStruct.CharacterIndex == 47)) || (objTempStruct.CharacterIndex == 54)) || (objTempStruct.CharacterIndex == 74)) || (objTempStruct.CharacterIndex == 83)) || (objTempStruct.CharacterIndex == 84)) || (objTempStruct.CharacterIndex == 104)) || (objTempStruct.CharacterIndex == 174)) || (objTempStruct.CharacterIndex == 175)) || (objTempStruct.CharacterIndex == 179)) || (objTempStruct.CharacterIndex == 190)) || (objTempStruct.CharacterIndex == 192)) || (objTempStruct.CharacterIndex == 194)) || (objTempStruct.CharacterIndex == 198)) || (objTempStruct.CharacterIndex == 201))
								{
									cReturn = "square";
								}
								else if ((((((((objTempStruct.CharacterIndex == 35) || (objTempStruct.CharacterIndex == 42)) || (objTempStruct.CharacterIndex == 48)) || (objTempStruct.CharacterIndex == 55)) || (objTempStruct.CharacterIndex == 73)) || (objTempStruct.CharacterIndex == 86)) || (objTempStruct.CharacterIndex == 184)) || (objTempStruct.CharacterIndex == 185))
								{
									cReturn = "triangle";
								}
								else if (objTempStruct.CharacterIndex == 68)
								{
									cReturn = "X";
								}
								else if ((((objTempStruct.CharacterIndex == 69) || (objTempStruct.CharacterIndex == 70)) || (objTempStruct.CharacterIndex == 71)) || (objTempStruct.CharacterIndex >= 203 && objTempStruct.CharacterIndex <= 211))
								{
									cReturn = "cross";
								}
								else if ((((((objTempStruct.CharacterIndex == 94) || (objTempStruct.CharacterIndex == 95)) || (objTempStruct.CharacterIndex == 96)) || (objTempStruct.CharacterIndex == 106)) || (objTempStruct.CharacterIndex == 107)) || (objTempStruct.CharacterIndex == 108))
								{
									cReturn = "star";
								}
								break;
							case "ESRI IGL FONT22":
								if (((((objTempStruct.CharacterIndex >= 65 && objTempStruct.CharacterIndex <= 69) || (objTempStruct.CharacterIndex >= 93 && objTempStruct.CharacterIndex <= 96)) || (objTempStruct.CharacterIndex == 103)) || (objTempStruct.CharacterIndex == 105)) || (objTempStruct.CharacterIndex == 106))
								{
									cReturn = "circle";
								}
								else if ((((objTempStruct.CharacterIndex == 70) || (objTempStruct.CharacterIndex == 71)) || (objTempStruct.CharacterIndex >= 88 && objTempStruct.CharacterIndex <= 92)) || (objTempStruct.CharacterIndex >= 118 && objTempStruct.CharacterIndex <= 121))
								{
									cReturn = "square";
								}
								else if ((((((((objTempStruct.CharacterIndex == 72) || (objTempStruct.CharacterIndex == 73)) || (objTempStruct.CharacterIndex == 75)) || (objTempStruct.CharacterIndex == 81)) || (objTempStruct.CharacterIndex == 85)) || (objTempStruct.CharacterIndex == 86)) || (objTempStruct.CharacterIndex >= 99 && objTempStruct.CharacterIndex <= 102)) || (objTempStruct.CharacterIndex == 104))
								{
									cReturn = "triangle";
								}
								else if (objTempStruct.CharacterIndex >= 114 && objTempStruct.CharacterIndex <= 117)
								{
									cReturn = "X";
								}
								break;
							case "ESRI GEOMETRIC SYMBOLS":
								if ((((((((((((((((((((((objTempStruct.CharacterIndex >= 33 && objTempStruct.CharacterIndex <= 35) || (objTempStruct.CharacterIndex >= 39 && objTempStruct.CharacterIndex <= 41)) || (objTempStruct.CharacterIndex == 47)) || (objTempStruct.CharacterIndex == 48)) || (objTempStruct.CharacterIndex >= 56 && objTempStruct.CharacterIndex <= 58)) || (objTempStruct.CharacterIndex == 65)) || (objTempStruct.CharacterIndex >= 68 && objTempStruct.CharacterIndex <= 71)) || (objTempStruct.CharacterIndex >= 74 && objTempStruct.CharacterIndex <= 77)) || (objTempStruct.CharacterIndex == 82)) || (objTempStruct.CharacterIndex == 83)) || (objTempStruct.CharacterIndex >= 86 && objTempStruct.CharacterIndex <= 89)) || (objTempStruct.CharacterIndex >= 92 && objTempStruct.CharacterIndex <= 95)) || (objTempStruct.CharacterIndex >= 98 && objTempStruct.CharacterIndex <= 101)) || (objTempStruct.CharacterIndex >= 104 && objTempStruct.CharacterIndex <= 107)) || (objTempStruct.CharacterIndex >= 110 && objTempStruct.CharacterIndex <= 113)) || (objTempStruct.CharacterIndex >= 116 && objTempStruct.CharacterIndex <= 125)) || (objTempStruct.CharacterIndex == 161)) || (objTempStruct.CharacterIndex == 171)) || (objTempStruct.CharacterIndex >= 177 && objTempStruct.CharacterIndex <= 186)) || (objTempStruct.CharacterIndex == 244)) || (objTempStruct.CharacterIndex >= 246 && objTempStruct.CharacterIndex <= 249)) || (objTempStruct.CharacterIndex == 8729))
								{
									cReturn = "circle";
								}
								else if (((((((((((((((((((((((objTempStruct.CharacterIndex == 37) || (objTempStruct.CharacterIndex == 42)) || (objTempStruct.CharacterIndex == 43)) || (objTempStruct.CharacterIndex == 50)) || (objTempStruct.CharacterIndex == 55)) || (objTempStruct.CharacterIndex == 67)) || (objTempStruct.CharacterIndex == 73)) || (objTempStruct.CharacterIndex == 79)) || (objTempStruct.CharacterIndex == 85)) || (objTempStruct.CharacterIndex == 91)) || (objTempStruct.CharacterIndex == 97)) || (objTempStruct.CharacterIndex == 103)) || (objTempStruct.CharacterIndex == 109)) || (objTempStruct.CharacterIndex == 115)) || (objTempStruct.CharacterIndex == 170)) || (objTempStruct.CharacterIndex == 172)) || (objTempStruct.CharacterIndex >= 200 && objTempStruct.CharacterIndex <= 205)) || (objTempStruct.CharacterIndex == 208)) || (objTempStruct.CharacterIndex == 209)) || (objTempStruct.CharacterIndex == 210)) || (objTempStruct.CharacterIndex >= 226 && objTempStruct.CharacterIndex <= 241)) || (objTempStruct.CharacterIndex == 243)) || (objTempStruct.CharacterIndex == 250))
								{
									cReturn = "square";
								}
								else if ((((((((((((((((((((objTempStruct.CharacterIndex == 36) || (objTempStruct.CharacterIndex == 46)) || (objTempStruct.CharacterIndex == 49)) || (objTempStruct.CharacterIndex == 66)) || (objTempStruct.CharacterIndex == 72)) || (objTempStruct.CharacterIndex == 78)) || (objTempStruct.CharacterIndex == 84)) || (objTempStruct.CharacterIndex == 90)) || (objTempStruct.CharacterIndex == 96)) || (objTempStruct.CharacterIndex == 102)) || (objTempStruct.CharacterIndex == 108)) || (objTempStruct.CharacterIndex == 114)) || (objTempStruct.CharacterIndex == 162)) || (objTempStruct.CharacterIndex == 168)) || (objTempStruct.CharacterIndex == 169)) || (objTempStruct.CharacterIndex == 175)) || (objTempStruct.CharacterIndex == 176)) || (objTempStruct.CharacterIndex >= 186 && objTempStruct.CharacterIndex <= 190)) || (objTempStruct.CharacterIndex >= 213 && objTempStruct.CharacterIndex <= 220)) || (objTempStruct.CharacterIndex == 245))
								{
									cReturn = "triangle";
								}
								else if (((objTempStruct.CharacterIndex >= 195 && objTempStruct.CharacterIndex <= 199) || (objTempStruct.CharacterIndex == 206)) || (objTempStruct.CharacterIndex == 207))
								{
									cReturn = "X";
								}
								break;
						}
					}
					else if ((ValueNameOfValueYouWant.ToUpper() == "PointColor".ToUpper()) || (ValueNameOfValueYouWant.ToUpper() == "PointOutlineColor".ToUpper()))
					{
						cColor = objTempStruct.Color; //Default
						cOutlineColor = ""; //Default
						switch (objTempStruct.Font.ToUpper())
						{
							case "ESRI DEFAULT MARKER":
								if ((((((((((((((((((((((((((objTempStruct.CharacterIndex >= 33 && objTempStruct.CharacterIndex <= 39) || (objTempStruct.CharacterIndex >= 67 && objTempStruct.CharacterIndex <= 69)) || (objTempStruct.CharacterIndex == 71)) || (objTempStruct.CharacterIndex == 81)) || (objTempStruct.CharacterIndex == 88)) || (objTempStruct.CharacterIndex >= 97 && objTempStruct.CharacterIndex <= 103)) || (objTempStruct.CharacterIndex == 107)) || (objTempStruct.CharacterIndex == 113)) || (objTempStruct.CharacterIndex == 116)) || (objTempStruct.CharacterIndex == 118)) || (objTempStruct.CharacterIndex == 161)) || (objTempStruct.CharacterIndex == 163)) || (objTempStruct.CharacterIndex == 165)) || (objTempStruct.CharacterIndex == 167)) || (objTempStruct.CharacterIndex == 168)) || (objTempStruct.CharacterIndex == 172)) || (objTempStruct.CharacterIndex == 174)) || (objTempStruct.CharacterIndex == 175)) || (objTempStruct.CharacterIndex == 179)) || (objTempStruct.CharacterIndex >= 182 && objTempStruct.CharacterIndex <= 186)) || (objTempStruct.CharacterIndex == 190)) || (objTempStruct.CharacterIndex >= 192 && objTempStruct.CharacterIndex <= 201)) || (objTempStruct.CharacterIndex >= 203 && objTempStruct.CharacterIndex <= 211)) || (objTempStruct.CharacterIndex == 215)) || (objTempStruct.CharacterIndex == 219)) || (objTempStruct.CharacterIndex == 8729))
								{
									cColor = objTempStruct.Color;
									cOutlineColor = "";
								}
								else
								{
									cColor = "";
									cOutlineColor = objTempStruct.Color;
								}
								break;
							case "ESRI IGL FONT22":
								if (((objTempStruct.CharacterIndex >= 72 && objTempStruct.CharacterIndex <= 80) || (objTempStruct.CharacterIndex == 100)) || (objTempStruct.CharacterIndex >= 118 && objTempStruct.CharacterIndex <= 121))
								{
									cColor = objTempStruct.Color;
									cOutlineColor = "";
								}
								else
								{
									cColor = "";
									cOutlineColor = objTempStruct.Color;
								}
								break;
							case "ESRI GEOMETRIC SYMBOLS":
								if (((((((((((objTempStruct.CharacterIndex >= 34 && objTempStruct.CharacterIndex <= 40) || (objTempStruct.CharacterIndex == 120)) || (objTempStruct.CharacterIndex >= 161 && objTempStruct.CharacterIndex <= 167)) || (objTempStruct.CharacterIndex == 187)) || (objTempStruct.CharacterIndex == 188)) || (objTempStruct.CharacterIndex >= 194 && objTempStruct.CharacterIndex <= 200)) || (objTempStruct.CharacterIndex >= 202 && objTempStruct.CharacterIndex <= 215)) || (objTempStruct.CharacterIndex == 217)) || (objTempStruct.CharacterIndex == 218)) || (objTempStruct.CharacterIndex >= 221 && objTempStruct.CharacterIndex <= 229)) || (objTempStruct.CharacterIndex >= 231 && objTempStruct.CharacterIndex <= 249))
								{
									cColor = objTempStruct.Color;
									cOutlineColor = "";
								}
								else
								{
									cColor = "";
									cOutlineColor = objTempStruct.Color;
								}
								break;
						}
						if (ValueNameOfValueYouWant.ToUpper() == "PointColor".ToUpper())
						{
							cReturn = cColor;
						}
						else
						{
							cReturn = cOutlineColor;
						}
						return cReturn;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointSize".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.Size);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointRotation".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.Angle);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointOutlineSize".ToUpper())
					{
						cReturn = "1";
						switch (objTempStruct.Font.ToUpper())
						{
							case "ESRI GEOMETRIC SYMBOLS":
								if (((((((((objTempStruct.CharacterIndex == 85) || (objTempStruct.CharacterIndex == 88)) || (objTempStruct.CharacterIndex == 89)) || (objTempStruct.CharacterIndex == 91)) || (objTempStruct.CharacterIndex == 94)) || (objTempStruct.CharacterIndex == 95)) || (objTempStruct.CharacterIndex == 97)) || (objTempStruct.CharacterIndex == 98)) || (objTempStruct.CharacterIndex == 100))
								{
									cReturn = "2";
								}
								else if ((((((((((objTempStruct.CharacterIndex == 41) || (objTempStruct.CharacterIndex >= 65 && objTempStruct.CharacterIndex <= 83)) || (objTempStruct.CharacterIndex >= 122 && objTempStruct.CharacterIndex <= 125)) || (objTempStruct.CharacterIndex == 168)) || (objTempStruct.CharacterIndex == 169)) || (objTempStruct.CharacterIndex == 188)) || (objTempStruct.CharacterIndex == 189)) || (objTempStruct.CharacterIndex == 216)) || (objTempStruct.CharacterIndex == 230)) || (objTempStruct.CharacterIndex == 250))
								{
									cReturn = "3";
								}
								else if (((objTempStruct.CharacterIndex == 161) || (objTempStruct.CharacterIndex >= 170 && objTempStruct.CharacterIndex <= 178)) || (objTempStruct.CharacterIndex == 186))
								{
									cReturn = "4";
								}
								break;
						}
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructPictureMarkerSymbol)
				{
					Analize_ArcMap_Symbols.StructPictureMarkerSymbol objTempStruct = new Analize_ArcMap_Symbols.StructPictureMarkerSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructPictureMarkerSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "WellKnownName".ToUpper())
					{
						cReturn = "circle"; //TODO
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointColor".ToUpper())
					{
						cReturn = objTempStruct.BackgroundColor;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointSize".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.Size);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointRotation".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.Angle);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointOutlineColor".ToUpper())
					{
						cReturn = ""; //TODO
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointOutlineSize".ToUpper())
					{
						cReturn = "0"; //TODO
					}
					return cReturn;
				}
				else if (SymbolStructure is Analize_ArcMap_Symbols.StructArrowMarkerSymbol)
				{
					Analize_ArcMap_Symbols.StructArrowMarkerSymbol objTempStruct = new Analize_ArcMap_Symbols.StructArrowMarkerSymbol();
                    objTempStruct = (Analize_ArcMap_Symbols.StructArrowMarkerSymbol)SymbolStructure;
					if (ValueNameOfValueYouWant.ToUpper() == "WellKnownName".ToUpper())
					{
						cReturn = "triangle";
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointColor".ToUpper())
					{
						cReturn = objTempStruct.Color;
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointSize".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.Size);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointRotation".ToUpper())
					{
						cReturn = CommaToPoint(objTempStruct.Angle);
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointOutlineColor".ToUpper())
					{
						cReturn = ""; //Never an outline
					}
					else if (ValueNameOfValueYouWant.ToUpper() == "PointOutlineSize".ToUpper())
					{
						cReturn = "0"; //Never an outline
					}
					return cReturn;
				}
			}
			catch (Exception ex)
			{
				ErrorMsg("Konnte den Wert aus der SymbolStruct nicht auswerten.", ex.Message, ex.StackTrace, "GetValueFromSymbolstruct");
			}
			return cReturn;
		}
#endregion
		
#region Hilfsfunktionen
		
		//************************************************************************************************
		//****** 躡ernimmt einen double der Form X,Y und wandelt ihn in die Form string X.Y **************
		//************************************************************************************************
		private string CommaToPoint(double value)
		{
			string cReturn = "";
			cReturn = value.ToString();
			cReturn = cReturn.Replace(",", ".");
			return cReturn;
		}
		//躡erladen 躡ernimmt einen String X,Y und wandelt ihn in die Form X.Y
		private string CommaToPoint(string value)
		{
			string cReturn = "";
			cReturn = value;
			cReturn = cReturn.Replace(",", ".");
			return cReturn;
		}
		
		
		//************************************************************************************************
		//********************************* Die zentrale Fehlermeldung ***********************************
		//************************************************************************************************
		private object ErrorMsg(string message, string exMessage, string stack, string functionname)
		{
			MessageBox.Show(message + "." + "\r\n" + exMessage + "\r\n" + stack, "ArcGIS_SLD_Converter | Output_SLD | " + functionname, MessageBoxButtons.OK, MessageBoxIcon.Error);
			MyTermination();
			return null;
		}
		
		//************************************************************************************************
		//********************************* Die zentrale Infomeldung ***********************************
		//************************************************************************************************
		private object InfoMsg(string message, string functionname)
		{
			MessageBox.Show(message, "ArcGIS_SLD_Converter | Output_SLD | " + functionname, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return null;
		}
		
		//************************************************************************************************
		//********************************* Beenden der Anwendung ****************************************
		//************************************************************************************************
		public object MyTermination()
		{
			ProjectData.EndApp();
			m_objData.MyTermination();
			//oder: application.exit
			return null;
		}
		
		
#endregion
		
	}
	
}
