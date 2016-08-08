// VBConversions Note: VB project level imports
using System;
using System.Drawing;
using System.Diagnostics;
using System.Data;
using Microsoft.VisualBasic;
using System.Collections;
using System.Windows.Forms;
// End of VB project level imports

using ESRI.ArcGIS.Framework;
//using ESRI.ArcGIS.Framework.AppROTClass;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
//using ESRI.ArcGIS.Carto.FeatureLayerClass;
using ESRI.ArcGIS.Display;
using stdole;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using System.IO;
using Microsoft.VisualBasic.CompilerServices;

//####################################################################################################################
//*******************ArcGIS_SLD_Converter*****************************************************************************
//*******************Class: Analize_ArcMap_Symbols********************************************************************
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
//application is written in VisualBasic.NET and uses the .NET 1.1
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
//Analyze_ArcMap_Symbols is the central analyzing class of the application. It analyzes the ArcMap styling attributes
//and stores the data in structs. Each renderer has its own struct. Each symbol has its own struct. The renderer itself
//stores the symbol structs. The class is triggered by the form-class "Motherform",button "Button1" pushes the analysis
//by the event "Button1_Click". The analysis starts at the "Analyze_ArcMap_Symbols.CentralProcessingFunc()". When ended
//the analysis, the class  calls the class "Output_SLD" and passes the structs with the data to the class.
//CHANGES:
//01.02.2006: Bugfix: in function StoreLineFill()
//04.02.2006: Language-customizing english/german
//04.02.2006: Tooltips
//22.01.2007: Bugfix: in StorLineFill.ICartographicLineSymbol
//26.01.2007: The converter now supports Group Layers (They cannot be displayed because there's still no such structure
//            in SLD, but the FeatureLayers inside the Group Layers are beeing stored. The the analysis of function
//            AnalyseLayerSymbology was outsourced to the new function SpreadLayerStructure. There a recursive call rules
//            the Layer order. The routine AddOneToLayerNumber counts the Layernumbers up.
//27.03.2007: - Extract transparency based on the color.
//            - Extract dashed line settings for cartographic lines and simple lines.
//            - Simplified color handling by always having no color (empty string)
//              if the color is transparent or style indicates color is not used.
//            - Fixed several bugs where wrong object was used causing a crash, in methods
//              StoreLineFill, StoreDotDensityFill, StorePictureFill, StoreGradientFill, StoreBarChart, StorePieChart, StoreStackedChart
//28.03.2007: Added TextSymbolizer support for simple annotation with a single feature as label.
//25.04.2007: Allow conversion of only layers that are visible, ignore those not visible.
//            Using option from motherform menu to decide whether to convert all
//            layers or just selected layers.
//12.09.2007: Fix to handle cartographic lines with empty Template pattern.
//09.06.2008: Add support for grouped values in UniqueValueRenderer.
//10.09.2008: Some small code improvements
//####################################################################################################################



namespace ArcGIS_SLD_Converter
{
	public class Analize_ArcMap_Symbols
	{
		
		//##################################################################################################
		//#################################### DEKLARATIONEN ###############################################
		//##################################################################################################
		
#region Membervariablen
		private IMxDocument m_ObjDoc; //Das augenblicklich aktive Dokument
		private IApplication m_ObjApp; //Die Schnittstelle zur ArcMap-Instanz
		private IAppROT m_ObjAppROT; //Die Schnittstelle zum Erlangen der laufenden ArcMap-Instanz
		private IObjectFactory m_ObjObjectCreator; //Die Schnittstelle, um ArcMap-Objekte generieren zu k鰊nen!!!
		private IMap m_ObjMap; //Die Schnittstelle zum aktuellen Kartenobjekt
		private Motherform frmMotherform; //Die Instanz der aufrufenden Mutterklasse
		internal StructProject m_StrProject; //Die Instanz der Datenstruktur, die alle Layer enth鋖t
		private ArrayList m_al1; //jede Arraylist enth鋖t die normalisierten Werte aus einer Tabellenspalte
		private ArrayList m_al2;
		private ArrayList m_al3;
		private ArrayList m_alClassifiedFields; //enth鋖t jeweils wiederum arraylist(s) von den normalisierten Werten aus den Feldern, nach denen klassifiziert wurde ( m_al1, m_al2, [m_al3])
		private string m_cFilename;
#endregion
		
		//##################################################################################################
		//######################################## ENUMERATIONEN ###########################################
		//##################################################################################################
		
#region Enums
		
		//Die FeatureClass wird hier festgelegt; (ob Punkt-, Linien-, oder Polygonfeature)
		internal enum FeatureClass
		{
			PointFeature = 0,
			LineFeature = 1,
			PolygonFeature = 2
		}
		
		internal enum MarkerStructs
		{
			StructSimpleMarkerSymbol,
			StructCharacterMarkerSymbol,
			StructPictureMarkerSymbol,
			StructArrowMarkerSymbol,
			StructMultilayerMarkerSymbol
		}
		
		internal enum LineStructs
		{
			StructSimpleLineSymbol = 0,
			StructMarkerLineSymbol = 1,
			StructHashLineSymbol = 2,
			StructPictureLineSymbol = 3,
			StructMultilayerLineSymbol = 4,
			StructCartographicLineSymbol = 5
		}
		
		
#endregion
		
		//##################################################################################################
		//#################################### DATENSTRUKTUREN #############################################
		//##################################################################################################
		
#region Datenstrukturen
		//************************************ Datenstruktur auf Projektebene *********************************
		
		//Die Datenstruktur f黵 die Layer (alle Layer in dem Projekt stecken hier drin
        public class ptRender
        {
            
        }
        public class ptSymbol
        {
 
        }
		internal struct StructProject
		{
			public ArrayList LayerList; //Hier stecken alle Layer als StructLayer-Sammlung drin
			public int LayerCount; //Anzahl der Layer
		}
		//************************************ Datenstrukturen auf Layerebene *********************************
		
		//Die Datenstruktur f黵 den UniqueValueRenderer
		internal struct StructUniqueValueRenderer
		{
			public FeatureClass FeatureCls; //Ob Punkt-, Linien-, oder Polygonfeature
			public string LayerName; //Der Layername (ist nicht der Name, nach dem klassifiziert wird)
			public string DatasetName; //Der Datasetname (Der Name, nach dem klassifiziert wird)
			public int ValueCount; //Die Anzahl der Wertefelder bzw. Symbole des Layers basierend auf der aktuellen Klassifizierung
			public ArrayList SymbolList; //Die Sammlung der einzelnen Attributauspr鋑ungen (Symbole) als Struct*Symbol...
			public int FieldCount; //Die Anzahl der Tabellenfelder, nach denen klassifiziert wird (0: kein Feld; 1 Feld; 2 Felder; 3 Felder max.)
			public ArrayList FieldNames; //Die Tabellenfelder, nach denen Klassifiziert wird (max. 3 Felder) als Strings
			public string StylePath; //Der Pfad zur Stildefinition oder Stildatei (entspr. Layer Propertys->Symbology->Categorys->Match to Symbol in a Style in ArcMap!)
			public StructAnnotation Annotation; //Annotation label based on feature
		}
		//Die Datenstruktur f黵 den ClassBreaksRenderer
		internal struct StructClassBreaksRenderer
		{
			public FeatureClass FeatureCls; //Ob Punkt-, Linien-, oder Polygonfeature
			public string LayerName; //Der Layername (ist nicht der Name, nach dem klassifiziert wird)
			public string DatasetName; //Der Datasetname (Der Name, nach dem klassifiziert wird)
			public int BreakCount; //Die Anzahl der Wertefelder bzw. Symbole des Layers basierend auf der aktuellen Klassifizierung
			public string FieldName; //Das Tabellenfeld, nach dem Klassifiziert wird
			public string NormFieldName; //Das Tabellenfeld, nach dem normalisiert wird
			public ArrayList SymbolList; //Die Sammlung der einzelnen Attributauspr鋑ungen (Symbole) als Struct*Symbol...
			public StructAnnotation Annotation; //Annotation label based on feature
		}
		//Die Datenstruktur f黵 den SimpleRenderer
		internal struct StructSimpleRenderer
		{
			public FeatureClass FeatureCls; //Ob Punkt-, Linien-, oder Polygonfeature
			public string LayerName; //Der Layername (ist nicht der Name, nach dem klassifiziert wird)
			public string DatasetName; //Der Datasetname (Der Name, nach dem klassifiziert wird)
			public ArrayList SymbolList; //Die Sammlung der einzelnen Attributauspr鋑ungen (Symbole) als Struct*Symbol... In diesem Fall jew nur 1 Symbol
			public StructAnnotation Annotation; //Annotation label based on feature
		}
		//********************************* Datenstrukturen auf Symbolebene **********************************
		
		//Die Datenstruktur f黵 SimpleMarkerSymbol
		internal struct StructSimpleMarkerSymbol
		{
			public double Angle; //Der Winkel des Symbols
			public bool Filled; //Whether fill color or not
			public string Color; //Die Farbe des Symbols in der Webschreibweise als #ByteByteByte
			public bool Outline; //Outline des Symbols
			public string OutlineColor; //Die Farbe der Outline in der Webschreibweise als #ByteByteByte
			public double OutlineSize; //die Dicke der Outline
			public double Size; //Die Symbolgr鲞e
			public string Style; //der esriSimpleMarkerStyle
			public double XOffset; //der Offset zur Koordinate des Sy<mbols
			public double YOffset; //der Offset zur Koordinate des Sy<mbols
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Daten ermittelt auf Rendererlevel:
			public string Label; //Die Feldwertbezeichnung (Label)    [alle Renderer]
			public ArrayList Fieldvalues; //Der Feldwert(e), nach dem Klassifiziert und dieses Symbol zugewiesen wird [Alle Renderer]. Die Reihenfolge der Werte entspricht auch der Reihenfolge der Felder (Spalten), wie sie im Renderereobjekt unter 'FieldNames as ArrayList' zu finden ist
			public double UpperLimit; //Obergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
			public double LowerLimit; //Untergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
		}
		//Die Datenstruktur f黵 CharacterMarkerSymbol
		internal struct StructCharacterMarkerSymbol
		{
			public double Angle; //Winkel des "Buchstabens"
			public int CharacterIndex; //Stelle des Buchstabens in der ASCII (ANSII)-Tabelle des zugeh鰎igen Fonts
			public string Color; //Die Farbe
			public string Font; //Der Font des Zeichens (Buchstabens)
			public double Size; //Die Schriftgr鲞e
			public double XOffset; //der Offset zur Koordinate des Sy<mbols
			public double YOffset; //der Offset zur Koordinate des Sy<mbols
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Daten ermittelt auf Rendererlevel:
			public string Label; //Die Feldwertbezeichnung (Label)    [alle Renderer]
			public ArrayList Fieldvalues; //Der Feldwert(e), nach dem Klassifiziert und dieses Symbol zugewiesen wird [Alle Renderer]. Die Reihenfolge der Werte entspricht auch der Reihenfolge der Felder (Spalten), wie sie im Renderereobjekt unter 'FieldNames as ArrayList' zu finden ist
			public double UpperLimit; //Obergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
			public double LowerLimit; //Untergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
		}
		//Die Datenstruktur f黵 PictureMarkerSymbol
		internal struct StructPictureMarkerSymbol
		{
			public double Angle; //Winkel
			public string BackgroundColor; //Die Farbe des BGColor in der Webschreibweise als #ByteByteByte
			public string Color; //Keine Ahnung welche Farbe das ist
			public IPicture Picture; //Das Bild
			public double Size; //Die Bildgr鲞e
			public double XOffset; //der Offset zur Koordinate des Sy<mbols
			public double YOffset; //der Offset zur Koordinate des Sy<mbols
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Daten ermittelt auf Rendererlevel:
			public string Label; //Die Feldwertbezeichnung (Label)    [alle Renderer]
			public ArrayList Fieldvalues; //Der Feldwert(e), nach dem Klassifiziert und dieses Symbol zugewiesen wird [Alle Renderer]. Die Reihenfolge der Werte entspricht auch der Reihenfolge der Felder (Spalten), wie sie im Renderereobjekt unter 'FieldNames as ArrayList' zu finden ist
			public double UpperLimit; //Obergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
			public double LowerLimit; //Untergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
		}
		//Die Datenstruktur f黵 PictureMarkerSymbol
		internal struct StructArrowMarkerSymbol
		{
			public double Angle; //Der Winkel
			public string Color; //Die Farbe
			public double Length; //Die Pfeill鋘ge
			public double Size; //Die Die Pfeilgr鲞e
			public string Style; //Der Esrieigene Pfeilstyle
			public double Width; //Die Pfeilbreite
			public double XOffset; //der Offset zur Koordinate des Sy<mbols
			public double YOffset; //der Offset zur Koordinate des Sy<mbols
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Daten ermittelt auf Rendererlevel:
			public string Label; //Die Feldwertbezeichnung (Label)    [alle Renderer]
			public ArrayList Fieldvalues; //Der Feldwert(e), nach dem Klassifiziert und dieses Symbol zugewiesen wird [Alle Renderer]. Die Reihenfolge der Werte entspricht auch der Reihenfolge der Felder (Spalten), wie sie im Renderereobjekt unter 'FieldNames as ArrayList' zu finden ist
			public double UpperLimit; //Obergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
			public double LowerLimit; //Untergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
		}
		//____________________________________________________________________________________________________
		
		//Die Datenstruktur f黵 SimpleLineSymbol
		internal struct StructSimpleLineSymbol
		{
			public string Color; //Die Farbe des Symbols in der Webschreibweise als #ByteByteByte
			public byte Transparency; //Transparancy of the color.
			public string publicStyle; //der esriSimpleLineStyle
			public double Width; //die Linienbreite
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Daten ermittelt auf Rendererlevel:
			public string Label; //Die Feldwertbezeichnung (Label)    [alle Renderer]
			public ArrayList Fieldvalues; //Der Feldwert(e), nach dem Klassifiziert und dieses Symbol zugewiesen wird [Alle Renderer]. Die Reihenfolge der Werte entspricht auch der Reihenfolge der Felder (Spalten), wie sie im Renderereobjekt unter 'FieldNames as ArrayList' zu finden ist
			public double UpperLimit; //Obergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
			public double LowerLimit; //Untergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
		}
		//Die Datenstruktur f黵 CartographicLineSymbol
		internal struct StructCartographicLineSymbol
		{
			public string Color; //Die Farbe des Symbols in der Webschreibweise als #ByteByteByte
			public byte Transparency; //Transparancy of the color.
			public double Width; //Die Strichbreite
			public string Join; //sagt aus, wie die Linien verbunden sind
			public double MiterLimit; //Schwellenwert, ab welchem Abstand Zwickel angezeigt werden
			public string Cap; //Die Form des Linienendes
			public ArrayList DashArray; //The dasharray ("Template" in terms of ESRI) for dashed lines
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Daten ermittelt auf Rendererlevel:
			public string Label; //Die Feldwertbezeichnung (Label)    [alle Renderer]
			public ArrayList Fieldvalues; //Der Feldwert(e), nach dem Klassifiziert und dieses Symbol zugewiesen wird [Alle Renderer]. Die Reihenfolge der Werte entspricht auch der Reihenfolge der Felder (Spalten), wie sie im Renderereobjekt unter 'FieldNames as ArrayList' zu finden ist
			public double UpperLimit; //Obergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
			public double LowerLimit; //Untergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
		}
		// Die Datenstruktur f黵 HashLineSymbol
		internal struct StructHashLineSymbol
		{
			public double Angle; //Der Winkel
			public string Color; //Die Farbe der Linie
			public byte Transparency; //Transparancy of the color.
			public double Width; //der Abstand der einzelnen Striche
			//____________________________________________________
			public LineStructs kindOfLineStruct; //steht drin, welcher struct im jeweiligen Fall benutzt wurde
			public StructSimpleLineSymbol HashSymbol_SimpleLine;
			public StructCartographicLineSymbol HashSymbol_CartographicLine;
			public StructMarkerLineSymbol HashSymbol_MarkerLine;
			//Public HashSymbol_HashLine as StructHashLineSymbol        geht nicht,da eine Struktur sich nicht selbst enthalten kann (s. StructHashLineSymbol)
			public StructPictureLineSymbol HashSymbol_PictureLine;
			public StructMultilayerLineSymbol HashSymbol_MultiLayerLines;
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Daten ermittelt auf Rendererlevel:
			public string Label; //Die Feldwertbezeichnung (Label)    [alle Renderer]
			public ArrayList Fieldvalues; //Der Feldwert(e), nach dem Klassifiziert und dieses Symbol zugewiesen wird [Alle Renderer]. Die Reihenfolge der Werte entspricht auch der Reihenfolge der Felder (Spalten), wie sie im Renderereobjekt unter 'FieldNames as ArrayList' zu finden ist
			public double UpperLimit; //Obergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
			public double LowerLimit; //Untergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
		}
		//Die Datenstruktur f黵 MarkerLineSymbol
		internal struct StructMarkerLineSymbol
		{
			public string Color; //Die Farbe
			public byte Transparency; //Transparancy of the color.
			public double Width; //Der Abstand der einzelnen Marker
			//____________________________________________________
			public MarkerStructs kindOfMarkerStruct; //steht drin, welcher struct im jeweiligen Fall benutzt wurde
			public StructSimpleMarkerSymbol MarkerSymbol_SimpleMarker;
			public StructCharacterMarkerSymbol MarkerSymbol_CharacterMarker;
			public StructPictureMarkerSymbol MarkerSymbol_PictureMarker;
			public StructArrowMarkerSymbol MarkerSymbol_ArrowMarker;
			public StructMultilayerMarkerSymbol MarkerSymbol_MultilayerMarker;
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Daten ermittelt auf Rendererlevel:
			public string Label; //Die Feldwertbezeichnung (Label)    [alle Renderer]
			public ArrayList Fieldvalues; //Der Feldwert(e), nach dem Klassifiziert und dieses Symbol zugewiesen wird [Alle Renderer]. Die Reihenfolge der Werte entspricht auch der Reihenfolge der Felder (Spalten), wie sie im Renderereobjekt unter 'FieldNames as ArrayList' zu finden ist
			public double UpperLimit; //Obergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
			public double LowerLimit; //Untergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
		}
		//Die Datenstruktur f黵 PictureLineSymbol
		internal struct StructPictureLineSymbol
		{
			public string BackgroundColor; //Hintergrundfarbe
			public byte BackgroundTransparency; //Transparancy of the color.
			public string Color; //?
			public byte Transparency; //Transparancy of the color.
			public double Offset; //?
			public IPicture Picture; //Das Bild
			public bool Rotate; //Bild rotieren
			public double Width; //Gr鲞e
			public double XScale; //Seitenverh鋖tnis X
			public double YScale; //Seitenverh鋖tnis Y
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Daten ermittelt auf Rendererlevel:
			public string Label; //Die Feldwertbezeichnung (Label)    [alle Renderer]
			public ArrayList Fieldvalues; //Der Feldwert(e), nach dem Klassifiziert und dieses Symbol zugewiesen wird [Alle Renderer]. Die Reihenfolge der Werte entspricht auch der Reihenfolge der Felder (Spalten), wie sie im Renderereobjekt unter 'FieldNames as ArrayList' zu finden ist
			public double UpperLimit; //Obergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
			public double LowerLimit; //Untergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
		}
		//____________________________________________________________________________________________________
		
		//Die Datenstruktur f黵 SimpleFillSymbol
		internal struct StructSimpleFillSymbol
		{
			public string Color; //Die Farbe des Fillsymbols
			public string Style; //esriSimpleFillStyle
			public byte Transparency;
			//____________________________________________________
			public LineStructs kindOfOutlineStruct; //steht drin, welcher struct im jeweiligen Fall benutzt wurde
			public StructSimpleLineSymbol Outline_SimpleLine;
			public StructCartographicLineSymbol Outline_CartographicLine;
			public StructMarkerLineSymbol Outline_MarkerLine;
			public StructHashLineSymbol Outline_HashLine;
			public StructPictureLineSymbol Outline_PictureLine;
			public StructMultilayerLineSymbol Outline_MultiLayerLines;
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Daten ermittelt auf Rendererlevel:
			public string Label; //Die Feldwertbezeichnung (Label)    [alle Renderer]
			public ArrayList Fieldvalues; //Der Feldwert(e), nach dem Klassifiziert und dieses Symbol zugewiesen wird [Alle Renderer]. Die Reihenfolge der Werte entspricht auch der Reihenfolge der Felder (Spalten), wie sie im Renderereobjekt unter 'FieldNames as ArrayList' zu finden ist
			public double UpperLimit; //Obergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
			public double LowerLimit; //Untergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
		}
		//Die Datenstruktur f黵 MarkerFillSymbol
		internal struct StructMarkerFillSymbol
		{
			public string Color; //Die F黮lfarbe
			public byte Transparency;
			public double GridAngle; //der Winkel des Grids
			//____________________________________________________
			public MarkerStructs kindOfMarkerStruct; //steht drin, welcher struct im jeweiligen Fall benutzt wurde
			public StructSimpleMarkerSymbol MarkerSymbol_SimpleMarker;
			public StructCharacterMarkerSymbol MarkerSymbol_CharacterMarker;
			public StructPictureMarkerSymbol MarkerSymbol_PictureMarker;
			public StructArrowMarkerSymbol MarkerSymbol_ArrowMarker;
			public StructMultilayerMarkerSymbol MarkerSymbol_MultilayerMarker;
			//____________________________________________________
			public LineStructs kindOfOutlineStruct; //steht drin, welcher struct im jeweiligen Fall benutzt wurde
			public StructSimpleLineSymbol Outline_SimpleLine;
			public StructCartographicLineSymbol Outline_CartographicLine;
			public StructMarkerLineSymbol Outline_MarkerLine;
			public StructHashLineSymbol Outline_HashLine;
			public StructPictureLineSymbol Outline_PictureLine;
			public StructMultilayerLineSymbol Outline_MultiLayerLines;
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Daten ermittelt auf Rendererlevel:
			public string Label; //Die Feldwertbezeichnung (Label)    [alle Renderer]
			public ArrayList Fieldvalues; //Der Feldwert(e), nach dem Klassifiziert und dieses Symbol zugewiesen wird [Alle Renderer]. Die Reihenfolge der Werte entspricht auch der Reihenfolge der Felder (Spalten), wie sie im Renderereobjekt unter 'FieldNames as ArrayList' zu finden ist
			public double UpperLimit; //Obergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
			public double LowerLimit; //Untergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
		}
		//Die Datenstruktur f黵 LineFillSymbol
		internal struct StructLineFillSymbol
		{
			public double Angle; //Der Linienwinkel
			public string Color; //Die Hintergrundfarbe
			public byte Transparency;
			public double Offset; //der Linienversatz bei z.B. versch.-farbigen gegeneinander verschobenen Linien
			public double Separation; //Der Linienabstand
			//____________________________________________________
			public LineStructs kindOfLineStruct; //steht drin, welcher struct im jeweiligen Fall benutzt wurde
			public StructSimpleLineSymbol LineSymbol_SimpleLine;
			public StructCartographicLineSymbol LineSymbol_CartographicLine;
			public StructMarkerLineSymbol LineSymbol_MarkerLine;
			public StructHashLineSymbol LineSymbol_HashLine;
			public StructPictureLineSymbol LineSymbol_PictureLine;
			public StructMultilayerLineSymbol LineSymbol_MultiLayerLines;
			//____________________________________________________
			public LineStructs kindOfOutlineStruct; //steht drin, welcher struct im jeweiligen Fall benutzt wurde
			public StructSimpleLineSymbol Outline_SimpleLine;
			public StructCartographicLineSymbol Outline_CartographicLine;
			public StructMarkerLineSymbol Outline_MarkerLine;
			public StructHashLineSymbol Outline_HashLine;
			public StructPictureLineSymbol Outline_PictureLine;
			public StructMultilayerLineSymbol Outline_MultiLayerLines;
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Daten ermittelt auf Rendererlevel:
			public string Label; //Die Feldwertbezeichnung (Label)    [alle Renderer]
			public ArrayList Fieldvalues; //Der Feldwert(e), nach dem Klassifiziert und dieses Symbol zugewiesen wird [Alle Renderer]. Die Reihenfolge der Werte entspricht auch der Reihenfolge der Felder (Spalten), wie sie im Renderereobjekt unter 'FieldNames as ArrayList' zu finden ist
			public double UpperLimit; //Obergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
			public double LowerLimit; //Untergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
		}
		//Die Datenstruktur f黵 DotDensityFillSymbol
		internal struct StructDotDensityFillSymbol
		{
			public string BackgroundColor; //Die Hintergrundfarbe
			public byte BackgroundTransparency; //Transparancy of the color.
			public string Color; //
			public byte Transparency;
			public int DotCount; //Anzahl der Punkte
			public double DotSize; //Symbolgr鲞e
			public double DotSpacing; //Distanz zwischen den Mittelpunkten der Symbole
			public bool FixedPlacement; //Bei true, werdfen die Symbole immer an der gleichen Stelle plaziert, andernfalls random verteilt
			public ArrayList SymbolList; //Das Array mit den einzelnen Marker-Symbolen des DotDensityFillSymbol
			public int SymbolCount; //Die Anzahl der unterschiedlichen Symbole
			//____________________________________________________
			public LineStructs kindOfOutlineStruct; //steht drin, welcher struct im jeweiligen Fall benutzt wurde
			public StructSimpleLineSymbol Outline_SimpleLine;
			public StructCartographicLineSymbol Outline_CartographicLine;
			public StructMarkerLineSymbol Outline_MarkerLine;
			public StructHashLineSymbol Outline_HashLine;
			public StructPictureLineSymbol Outline_PictureLine;
			public StructMultilayerLineSymbol Outline_MultiLayerLines;
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Daten ermittelt auf Rendererlevel:
			public string Label; //Die Feldwertbezeichnung (Label)    [alle Renderer]
			public ArrayList Fieldvalues; //Der Feldwert(e), nach dem Klassifiziert und dieses Symbol zugewiesen wird [Alle Renderer]. Die Reihenfolge der Werte entspricht auch der Reihenfolge der Felder (Spalten), wie sie im Renderereobjekt unter 'FieldNames as ArrayList' zu finden ist
			public double UpperLimit; //Obergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
			public double LowerLimit; //Untergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
		}
		//Die Datenstruktur f黵 PictureFillSymbol
		internal struct StructPictureFillSymbol
		{
			public double Angle; //Der Winkel der Bilddrehung
			public string BackgroundColor; //die Hintergrundfarbe
			public byte BackgroundTransparency; //Transparancy of the color.
			public string Color; //
			public byte Transparency;
			public IPictureDisp Picture; //Das Bild
			public double XScale; //Seitenverh鋖tnis X
			public double YScale; //Seitenverh鋖tnis Y
			//____________________________________________________
			public LineStructs kindOfOutlineStruct; //steht drin, welcher struct im jeweiligen Fall benutzt wurde
			public StructSimpleLineSymbol Outline_SimpleLine;
			public StructCartographicLineSymbol Outline_CartographicLine;
			public StructMarkerLineSymbol Outline_MarkerLine;
			public StructHashLineSymbol Outline_HashLine;
			public StructPictureLineSymbol Outline_PictureLine;
			public StructMultilayerLineSymbol Outline_MultiLayerLines;
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Daten ermittelt auf Rendererlevel:
			public string Label; //Die Feldwertbezeichnung (Label)    [alle Renderer]
			public ArrayList Fieldvalues; //Der Feldwert(e), nach dem Klassifiziert und dieses Symbol zugewiesen wird [Alle Renderer]. Die Reihenfolge der Werte entspricht auch der Reihenfolge der Felder (Spalten), wie sie im Renderereobjekt unter 'FieldNames as ArrayList' zu finden ist
			public double UpperLimit; //Obergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
			public double LowerLimit; //Untergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
		}
		//Die Datenstruktur f黵 GradientFillSymbol
		internal struct StructGradientFillSymbol
		{
			public string Color; //
			public byte Transparency;
			public ArrayList Colors; //die einzelnen Farben der ColorRamp as string
			public double GradientAngle; //Neigungswinkel f黵 das F黮lmuster
			public double GradientPercentage; //Zahl zw. 0 und 1. 1=Farbverlauf 黚er die gesamte Fl鋍he. 0.5=H鋖fte der Fl鋍he wird mit Farbverlauf dargestellt
			public int IntervallCount; //Anzahl der Farben im Farbverlauf
			public string Style; //der esriGradientFillStyle
			//____________________________________________________
			public LineStructs kindOfOutlineStruct; //steht drin, welcher struct im jeweiligen Fall benutzt wurde
			public StructSimpleLineSymbol Outline_SimpleLine;
			public StructCartographicLineSymbol Outline_CartographicLine;
			public StructMarkerLineSymbol Outline_MarkerLine;
			public StructHashLineSymbol Outline_HashLine;
			public StructPictureLineSymbol Outline_PictureLine;
			public StructMultilayerLineSymbol Outline_MultiLayerLines;
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Daten ermittelt auf Rendererlevel:
			public string Label; //Die Feldwertbezeichnung (Label)    [alle Renderer]
			public ArrayList Fieldvalues; //Der Feldwert(e), nach dem Klassifiziert und dieses Symbol zugewiesen wird [Alle Renderer]. Die Reihenfolge der Werte entspricht auch der Reihenfolge der Felder (Spalten), wie sie im Renderereobjekt unter 'FieldNames as ArrayList' zu finden ist
			public double UpperLimit; //Obergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
			public double LowerLimit; //Untergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
		}
		//____________________________________________________________________________________________________
		
		
		//Die Datenstruktur f黵 BarChartSymbol
		internal struct StructBarChartSymbol
		{
			public bool ShowAxes; //...
			public double Spacing; //Der Platz zwischen den Balken
			public bool VerticalBars; //...
			public double Width; //...
			//____________________________________________________
			public LineStructs kindOfAxeslineStruct; //steht drin, welcher struct im jeweiligen Fall benutzt wurde
			public StructSimpleLineSymbol Axes_SimpleLine;
			public StructCartographicLineSymbol Axes_CartographicLine;
			public StructMarkerLineSymbol Axes_MarkerLine;
			public StructHashLineSymbol Axes_HashLine;
			public StructPictureLineSymbol Axes_PictureLine;
			public StructMultilayerLineSymbol Axes_MultiLayerLines;
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Daten ermittelt auf Rendererlevel:
			public string Label; //Die Feldwertbezeichnung (Label)    [alle Renderer]
			public ArrayList Fieldvalues; //Der Feldwert(e), nach dem Klassifiziert und dieses Symbol zugewiesen wird [Alle Renderer]. Die Reihenfolge der Werte entspricht auch der Reihenfolge der Felder (Spalten), wie sie im Renderereobjekt unter 'FieldNames as ArrayList' zu finden ist
			public double UpperLimit; //Obergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
			public double LowerLimit; //Untergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
		}
		//Die Datenstruktur f黵 PieChartSymbol
		internal struct StructPieChartSymbol
		{
			public bool Clockwise; //Uhrzeigersinn oder nicht
			public bool UseOutline; //...
			//____________________________________________________
			public LineStructs kindOfOutlineStruct; //steht drin, welcher struct im jeweiligen Fall benutzt wurde
			public StructSimpleLineSymbol Outline_SimpleLine;
			public StructCartographicLineSymbol Outline_CartographicLine;
			public StructMarkerLineSymbol Outline_MarkerLine;
			public StructHashLineSymbol Outline_HashLine;
			public StructPictureLineSymbol Outline_PictureLine;
			public StructMultilayerLineSymbol Outline_MultiLayerLines;
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Daten ermittelt auf Rendererlevel:
			public string Label; //Die Feldwertbezeichnung (Label)    [alle Renderer]
			public ArrayList Fieldvalues; //Der Feldwert(e), nach dem Klassifiziert und dieses Symbol zugewiesen wird [Alle Renderer]. Die Reihenfolge der Werte entspricht auch der Reihenfolge der Felder (Spalten), wie sie im Renderereobjekt unter 'FieldNames as ArrayList' zu finden ist
			public double UpperLimit; //Obergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
			public double LowerLimit; //Untergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
		}
		//Die Datenstruktur f黵 StackedChartSymbol
		internal struct StructStackedChartSymbol
		{
			public bool Fixed; //...
			public bool UseOutline; //...
			public bool VerticalBar;
			public double Width; //...
			//____________________________________________________
			public LineStructs kindOfOutlineStruct; //steht drin, welcher struct im jeweiligen Fall benutzt wurde
			public StructSimpleLineSymbol Outline_SimpleLine;
			public StructCartographicLineSymbol Outline_CartographicLine;
			public StructMarkerLineSymbol Outline_MarkerLine;
			public StructHashLineSymbol Outline_HashLine;
			public StructPictureLineSymbol Outline_PictureLine;
			public StructMultilayerLineSymbol Outline_MultiLayerLines;
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Daten ermittelt auf Rendererlevel:
			public string Label; //Die Feldwertbezeichnung (Label)    [alle Renderer]
			public ArrayList Fieldvalues; //Der Feldwert(e), nach dem Klassifiziert und dieses Symbol zugewiesen wird [Alle Renderer]. Die Reihenfolge der Werte entspricht auch der Reihenfolge der Felder (Spalten), wie sie im Renderereobjekt unter 'FieldNames as ArrayList' zu finden ist
			public double UpperLimit; //Obergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
			public double LowerLimit; //Untergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
		}
		//____________________________________________________________________________________________________
		
		//Die Datenstruktur f黵 TextSymbol
		internal struct StructTextSymbol
		{
			public double Angle; //Der Textwinkel
			public string Color; //...
			public string Font; //...
			public string Style; //Font style in CSS notation, e.g. italic or normal
			public string Weight; //Font weight in CSS notation, e.g. bold or normal
			public string HorizontalAlignment; // die Horizontale Textausrichtung als esriTextHorizontalAlignment
			public bool RightToLeft; //
			public double Size; //...
			public string Text; //...
			public string VerticalAlignment; //die Verticale Textausrichtung als esriTextVerticalAlignment
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Daten ermittelt auf Rendererlevel:
			public string Label; //Die Feldwertbezeichnung (Label)    [alle Renderer]
			public ArrayList Fieldvalues; //Der Feldwert(e), nach dem Klassifiziert und dieses Symbol zugewiesen wird [Alle Renderer]. Die Reihenfolge der Werte entspricht auch der Reihenfolge der Felder (Spalten), wie sie im Renderereobjekt unter 'FieldNames as ArrayList' zu finden ist
			public double UpperLimit; //Obergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
			public double LowerLimit; //Untergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
		}
		//*************************** Die Strukturen f黵 dieMultilayer-Symbole ******************************
		
		//Das Marker-Multilayersymbol
		internal struct StructMultilayerMarkerSymbol
		{
			public ArrayList MultiMarkerLayers; //Das Symbol kann aus mehreren Markersymbollayern zusammengestzt sein
			public int LayerCount; //Anzahl der Layer
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Daten ermittelt auf Rendererlevel:
			public string Label; //Die Feldwertbezeichnung (Label)    [alle Renderer]
			public ArrayList Fieldvalues; //Der Feldwert(e), nach dem Klassifiziert und dieses Symbol zugewiesen wird [Alle Renderer]. Die Reihenfolge der Werte entspricht auch der Reihenfolge der Felder (Spalten), wie sie im Renderereobjekt unter 'FieldNames as ArrayList' zu finden ist
			public double UpperLimit; //Obergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
			public double LowerLimit; //Untergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
		}
		//Das Line-Multilayersymbol
		internal struct StructMultilayerLineSymbol
		{
			public ArrayList MultiLineLayers; //Das Symbol kann aus mehreren Linesymbollayern zusammengestzt sein
			public int LayerCount; //Anzahl der Layer
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Daten ermittelt auf Rendererlevel:
			public string Label; //Die Feldwertbezeichnung (Label)    [alle Renderer]
			public ArrayList Fieldvalues; //Der Feldwert(e), nach dem Klassifiziert und dieses Symbol zugewiesen wird [Alle Renderer]. Die Reihenfolge der Werte entspricht auch der Reihenfolge der Felder (Spalten), wie sie im Renderereobjekt unter 'FieldNames as ArrayList' zu finden ist
			public double UpperLimit; //Obergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
			public double LowerLimit; //Untergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
		}
		//Das Fill-Multilayersymbol
		internal struct StructMultilayerFillSymbol
		{
			public ArrayList MultiFillLayers; //Das Symbol kann aus mehreren Fillsymbollayern zusammengestzt sein
			public int LayerCount; //Anzahl der Layer
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ Daten ermittelt auf Rendererlevel:
			public string Label; //Die Feldwertbezeichnung (Label)    [alle Renderer]
			public ArrayList Fieldvalues; //Der Feldwert(e), nach dem Klassifiziert und dieses Symbol zugewiesen wird [Alle Renderer]. Die Reihenfolge der Werte entspricht auch der Reihenfolge der Felder (Spalten), wie sie im Renderereobjekt unter 'FieldNames as ArrayList' zu finden ist
			public double UpperLimit; //Obergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
			public double LowerLimit; //Untergrenze der das aktuelle Symbol betr. Range beim ClassBreaksRenderer
		}
		internal struct StructAnnotation
		{
			public bool IsSingleProperty;
			public string PropertyName;
			public StructTextSymbol TextSymbol;
		}
#endregion //Die Deklaration der Datenstrukturen, in denen alle analysierten Daten erfasst werden
		
		//##################################################################################################
		//########################################### ROUTINEN #############################################
		//##################################################################################################
		
#region Routinen //u.a. die public Sub New()
		
		public Analize_ArcMap_Symbols(Motherform value, string Filename)
		{
			m_cFilename = Filename; //1.)
			frmMotherform = value; //2.)
			
			m_ObjApp = null; //Um sicherzustellen, da?kein Unsinn initialisiert ist
			CentralProcessingFunc();
		}
		
		//************************************************************************************************
		//Every time when a layer is added to the List the function adds one to the layer number
		//************************************************************************************************
		private void AddOneToLayerNumber()
		{
			m_StrProject.LayerCount++;
		}
		
#endregion
		
		//##################################################################################################
		//######################################### PROPERTIES #############################################
		//##################################################################################################
		
#region Properties
		
		//Gibt die Datenstruktur mit den gespeicherten Projektdaten weiter
public object GetProjectData
		{
			get
			{
				return m_StrProject;
			}
		}
		
#endregion
		
		//##################################################################################################
		//######################################### FUNKTIONEN #############################################
		//##################################################################################################
		
#region Speicherfuntionen der Datenstrukturen
		
		//************************************************************************************************
		//Speichert die Daten in einer SimpleRenderer-Datenstruktur
		//************************************************************************************************
		private StructSimpleRenderer StoreStructSimpleRenderer(ISimpleRenderer Renderer, IFeatureLayer Layer)
		{
			StructSimpleRenderer strRenderer = new StructSimpleRenderer();
			ISymbol objFstOrderSymbol = default(ISymbol);
			IDataset objDataset = default(IDataset);
			objDataset = Layer.FeatureClass as IDataset;
			strRenderer.SymbolList = new ArrayList();
			
			try
			{
				
				strRenderer.LayerName = Layer.Name;
				strRenderer.DatasetName = objDataset.Name;
				strRenderer.Annotation = GetAnnotation(Layer);
				//AB HIER BEGINNEN DIE FALLUNTERSCHEIDUNGEN DER SYMBOLE
				objFstOrderSymbol = Renderer.Symbol; //Die Zuweisung der jeweiligen einzelnen Symbole
				//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
				if (objFstOrderSymbol is ITextSymbol)
				{
					StructTextSymbol strTS = new StructTextSymbol();
					ITextSymbol objSymbol = default(ITextSymbol);
					objSymbol = objFstOrderSymbol as ITextSymbol;
					strTS = StoreText(objSymbol);
					//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
					strTS.Label = Renderer.Label;
					strRenderer.SymbolList.Add(strTS);
					
				}
				//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx()
				if (objFstOrderSymbol is IMarkerSymbol)
				{
					strRenderer.FeatureCls = FeatureClass.PointFeature;
					IMarkerSymbol objSymbol = default(IMarkerSymbol);
                    objSymbol = objFstOrderSymbol as IMarkerSymbol;
					switch (MarkerSymbolScan(objSymbol))
					{
						case "ISimpleMarkerSymbol":
							ISimpleMarkerSymbol SMS = default(ISimpleMarkerSymbol);
                            SMS = objSymbol as ISimpleMarkerSymbol;
							StructSimpleMarkerSymbol strSMS = new StructSimpleMarkerSymbol();
							strSMS = StoreSimpleMarker(SMS);
							//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
							strSMS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strSMS);
							break;
							
						case "ICharacterMarkerSymbol":
							ICharacterMarkerSymbol CMS = default(ICharacterMarkerSymbol);
							CMS = objSymbol as ICharacterMarkerSymbol ;
							StructCharacterMarkerSymbol strCMS = new StructCharacterMarkerSymbol();
							strCMS = StoreCharacterMarker(CMS);
							//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
							strCMS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strCMS);
							break;
							
						case "IPictureMarkerSymbol":
							IPictureMarkerSymbol PMS = default(IPictureMarkerSymbol);
                            PMS = objSymbol as IPictureMarkerSymbol;
							StructPictureMarkerSymbol strPMS = new StructPictureMarkerSymbol();
							strPMS = StorePictureMarker(PMS);
							//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
							strPMS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strPMS);
							break;
							
						case "IArrowMarkerSymbol":
							IArrowMarkerSymbol AMS = default(IArrowMarkerSymbol);
                            AMS = objSymbol as IArrowMarkerSymbol;
							StructArrowMarkerSymbol strAMS = new StructArrowMarkerSymbol();
							strAMS = StoreArrowMarker(AMS);
							//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
							strAMS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strAMS);
							break;
							
						case "IMultiLayerMarkerSymbol":
							IMultiLayerMarkerSymbol MLMS = default(IMultiLayerMarkerSymbol);
                            MLMS = objSymbol as IMultiLayerMarkerSymbol;
							StructMultilayerMarkerSymbol strMLMS = new StructMultilayerMarkerSymbol();
							strMLMS = StoreMultiLayerMarker(MLMS);
							//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
							strMLMS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strMLMS);
							break;
							
						case "false":
							InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreStructLayer");
							break;
					}
				}
				//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
				if (objFstOrderSymbol is ILineSymbol)
				{
					strRenderer.FeatureCls = FeatureClass.LineFeature;
					ILineSymbol objSymbol = default(ILineSymbol);
                    objSymbol = objFstOrderSymbol as ILineSymbol;
					switch (LineSymbolScan(objSymbol))
					{
						case "ICartographicLineSymbol":
							ICartographicLineSymbol CLS = default(ICartographicLineSymbol);
                            CLS = objSymbol as ICartographicLineSymbol;
							StructCartographicLineSymbol strCLS = new StructCartographicLineSymbol();
							strCLS = StoreCartographicLine(CLS);
							//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
							strCLS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strCLS);
							break;
							
						case "IHashLineSymbol":
							IHashLineSymbol HLS = default(IHashLineSymbol);
                            HLS = objSymbol as IHashLineSymbol;
							StructHashLineSymbol strHLS = new StructHashLineSymbol();
							strHLS = StoreHashLine(HLS);
							//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
							strHLS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strHLS);
							break;
							
						case "IMarkerLineSymbol":
							IMarkerLineSymbol MLS = default(IMarkerLineSymbol);
                            MLS = objSymbol as IMarkerLineSymbol;
							StructMarkerLineSymbol strMLS = new StructMarkerLineSymbol();
							strMLS = StoreMarkerLine(MLS);
							//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
							strMLS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strMLS);
							break;
							
						case "ISimpleLineSymbol":
							ISimpleLineSymbol SLS = default(ISimpleLineSymbol);
                            SLS = objSymbol as ISimpleLineSymbol;
							StructSimpleLineSymbol strSLS = new StructSimpleLineSymbol();
							strSLS = StoreSimpleLine(SLS);
							//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
							strSLS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strSLS);
							break;
							
						case "IPictureLineSymbol":
							IPictureLineSymbol PLS = default(IPictureLineSymbol);
                            PLS = objSymbol as IPictureLineSymbol;
							StructPictureLineSymbol strPLS = new StructPictureLineSymbol();
							strPLS = StorePictureLine(PLS);
							//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
							strPLS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strPLS);
							break;
							
						case "IMultiLayerLineSymbol":
							IMultiLayerLineSymbol MLLS = default(IMultiLayerLineSymbol);
                            MLLS = objSymbol as IMultiLayerLineSymbol;
							StructMultilayerLineSymbol strMLLS = new StructMultilayerLineSymbol();
							strMLLS = StoreMultilayerLines(MLLS);
							//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
							strMLLS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strMLLS);
							break;
							
						case "false":
							InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreStructLayer");
							break;
					}
					
				}
				//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
				if (objFstOrderSymbol is IFillSymbol)
				{
					strRenderer.FeatureCls = FeatureClass.PolygonFeature;
					IFillSymbol objSymbol = default(IFillSymbol);
                    objSymbol = objFstOrderSymbol as IFillSymbol;
					switch (FillSymbolScan(objSymbol))
					{
						case "ISimpleFillSymbol":
							ISimpleFillSymbol SFS = default(ISimpleFillSymbol);
                            SFS = objSymbol as ISimpleFillSymbol;
							StructSimpleFillSymbol strSFS = new StructSimpleFillSymbol();
							strSFS = StoreSimpleFill(SFS);
							//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
							strSFS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strSFS);
							break;
							
						case "IMarkerFillSymbol":
							IMarkerFillSymbol MFS = default(IMarkerFillSymbol);
                            MFS = objSymbol as IMarkerFillSymbol;
							StructMarkerFillSymbol strMFS = new StructMarkerFillSymbol();
							strMFS = StoreMarkerFill(MFS);
							//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
							strMFS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strMFS);
							break;
							
						case "ILineFillSymbol":
							ILineFillSymbol LFS = default(ILineFillSymbol);
                            LFS = objSymbol as ILineFillSymbol;
							StructLineFillSymbol strLFS = new StructLineFillSymbol();
							strLFS = StoreLineFill(LFS);
							//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
							strLFS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strLFS);
							break;
							
						case "IDotDensityFillSymbol":
							IDotDensityFillSymbol DFS = default(IDotDensityFillSymbol);
                            DFS = objSymbol as IDotDensityFillSymbol;
							StructDotDensityFillSymbol strDFS = new StructDotDensityFillSymbol();
							strDFS = StoreDotDensityFill(DFS);
							//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
							strDFS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strDFS);
							break;
							
						case "IPictureFillSymbol":
							IPictureFillSymbol PFS = default(IPictureFillSymbol);
                            PFS = objSymbol as IPictureFillSymbol;
							StructPictureFillSymbol strPFS = new StructPictureFillSymbol();
							strPFS = StorePictureFill(PFS);
							//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
							strPFS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strPFS);
							break;
							
						case "IGradientFillSymbol":
							IGradientFillSymbol GFS = default(IGradientFillSymbol);
                            GFS = objSymbol as IGradientFillSymbol;
							StructGradientFillSymbol strGFS = new StructGradientFillSymbol();
							strGFS = StoreGradientFill(GFS);
							//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
							strGFS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strGFS);
							break;
							
						case "IMultiLayerFillSymbol":
							IMultiLayerFillSymbol MLFS = default(IMultiLayerFillSymbol);
                            MLFS = objSymbol as IMultiLayerFillSymbol;
							StructMultilayerFillSymbol strMLFS = new StructMultilayerFillSymbol();
							strMLFS = StoreMultiLayerFill(MLFS);
							//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
							strMLFS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strMLFS);
							break;
							
						case "false":
							InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreStructLayer");
							break;
					}
				}
				//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
				if (objFstOrderSymbol is I3DChartSymbol)
				{
					I3DChartSymbol objSymbol = default(I3DChartSymbol);
                    objSymbol = objFstOrderSymbol as I3DChartSymbol;
					switch (IIIDChartSymbolScan(objSymbol))
					{
						case "IBarChartSymbol":
							IBarChartSymbol BCS = default(IBarChartSymbol);
                            BCS = objSymbol as IBarChartSymbol;
							StructBarChartSymbol strBCS = new StructBarChartSymbol();
							strBCS = StoreBarChart(BCS);
							//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
							strBCS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strBCS);
							break;
							
						case "IPieChartSymbol":
							IPieChartSymbol PCS = default(IPieChartSymbol);
                            PCS = objSymbol as IPieChartSymbol;
							StructPieChartSymbol strPCS = new StructPieChartSymbol();
							strPCS = StorePieChart(PCS);
							//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
							strPCS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strPCS);
							break;
							
						case "IStackedChartSymbol":
							IStackedChartSymbol SCS = default(IStackedChartSymbol);
                            SCS = objSymbol as IStackedChartSymbol;
							StructStackedChartSymbol strSCS = new StructStackedChartSymbol();
							strSCS = StoreStackedChart(SCS);
							//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
							strSCS.Label = Renderer.Label;
							strRenderer.SymbolList.Add(strSCS);
							break;
							
						case "false":
							InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreStructLayer");
							break;
					}
				}
				//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
				return strRenderer;
			}
			catch (Exception ex)
			{
				
				ErrorMsg("Fehler beim Speichern der Symbole in den Layerstrukturen", ex.Message, ex.StackTrace, "StoreStructSimpleRenderer");
                return strRenderer;
			}
		}
		
		
		//************************************************************************************************
		//Speichert die Daten in einer ClassBreaksRenderer-Datenstruktur. Gespeichert wird auf 2 Ebenen:
		//1. Daten auf Symbolebene: Symbole werden abgefragt und in Fallunterscheidung auf Untersymbole verteilt
		//dort werden die symboleigenen Daten in den Symboldatenstrukturen gespeichert
		//2. Daten auf Rendererebene: Daten, die nur auf Rendererebene verf黦bar sind, werden hier ebenfalls auf
		//Symbolebene gespeichert, damit sie direkt im Zusammenhang mit dem Symbol verf黦bar sind.
		//Parameter: Renderer:   das Rendererobjekt des aktuellen Layers
		//           Layer:      das Layerobjekt des aktuellen Layers
		//************************************************************************************************
		private StructClassBreaksRenderer StoreStructCBRenderer(IClassBreaksRenderer Renderer, IFeatureLayer Layer)
		{
			StructClassBreaksRenderer strRenderer = new StructClassBreaksRenderer(); //Hierin wird das eine Rendererobjekt gespeichert plus zus鋞zliche Layerinformationen
			strRenderer.SymbolList = new ArrayList();
			int iNumberOfSymbols = 0; //Anzahl aller Symbole des Rendererobjekts
			iNumberOfSymbols = Renderer.BreakCount; //Anzahl der Symbole
			strRenderer.BreakCount = Renderer.BreakCount;
			ISymbol objFstOrderSymbol = default(ISymbol); //Das gerade aktuelle Symbol des durchlaufenen Rendererobjekts
			IDataset objDataset = default(IDataset);
            objDataset = Layer.FeatureClass as IDataset;
			bool bIsJoined;
			bIsJoined = false;
			
			try
			{
				strRenderer.LayerName = Layer.Name;
				strRenderer.DatasetName = objDataset.Name;
				strRenderer.Annotation = GetAnnotation(Layer);
				
				//diese Objekte dienen lediglich der 躡erpr黤ung, ob eine andere Tabelle an die Featuretable gejoint ist
				//++++++++++++++++++++++++++++++++++++++++++
				ITable pTable = default(ITable);
				IDisplayTable pDisplayTable = default(IDisplayTable);
                pDisplayTable = Layer as IDisplayTable;
				pTable = pDisplayTable.DisplayTable;
				if (pTable is IRelQueryTable) //Dann ist eine Tabelle drangejoint
				{
					bIsJoined = true;
				}
				//++++++++++++++++++++++++++++++++++++++++++
				
				if (bIsJoined == false) //Wenn eine Tabelle drangejoint wurde, ist sowieso schon der Datasetname mit dabei
				{
					//Je nachdem, welcher Zielkartendienst gew鋒lt wurde, muss Fieldname ein anderes Format besitzen
					if (frmMotherform.chkArcIMS.Checked == true)
					{
						strRenderer.FieldName = objDataset.Name + "." + Renderer.Field;
						strRenderer.NormFieldName = objDataset.Name + "." + Renderer.NormField;
					}
					else
					{
						strRenderer.FieldName = Renderer.Field;
						strRenderer.NormFieldName = Renderer.NormField;
					}
				}
				//AB HIER BEGINNEN DIE FALLUNTERSCHEIDUNGEN DER SYMBOLE
				int j = 0;
				for (j = 0; j <= iNumberOfSymbols - 1; j++)
				{
					frmMotherform.CHLabelSmall("Symbol " + (j + 1).ToString() + " von " + iNumberOfSymbols.ToString()); //Anzeige auf dem Label
					objFstOrderSymbol = Renderer.Symbol[j]; //Die Zuweisung der jeweiligen einzelnen Symbole
					IClassBreaksUIProperties objClassBreaksProp = default(IClassBreaksUIProperties);
                    objClassBreaksProp = Renderer as IClassBreaksUIProperties;
					string cLowerLimit = "";
					string cUpperLimit = "";
					cLowerLimit = (objClassBreaksProp.LowBreak[j]).ToString(); //Die Untergrenze der aktuellen Klasse
					cUpperLimit = (Renderer.Break[j]).ToString();
					
					//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
					if (objFstOrderSymbol is ITextSymbol)
					{
						StructTextSymbol strTS = new StructTextSymbol();
						ITextSymbol objSymbol = default(ITextSymbol);
                        objSymbol = objFstOrderSymbol as ITextSymbol;
						strTS = StoreText(objSymbol);
						//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
						strTS.Label = Renderer.Label[j];
						strTS.LowerLimit = double.Parse(cLowerLimit);
						strTS.UpperLimit = double.Parse(cUpperLimit);
						strRenderer.SymbolList.Add(strTS);
						
					}
					//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx()
					if (objFstOrderSymbol is IMarkerSymbol)
					{
						strRenderer.FeatureCls = FeatureClass.PointFeature;
						IMarkerSymbol objSymbol = default(IMarkerSymbol);
                        objSymbol = objFstOrderSymbol as IMarkerSymbol;
						switch (MarkerSymbolScan(objSymbol))
						{
							case "ISimpleMarkerSymbol":
								ISimpleMarkerSymbol SMS = default(ISimpleMarkerSymbol);
                                SMS = objSymbol as ISimpleMarkerSymbol;
								StructSimpleMarkerSymbol strSMS = new StructSimpleMarkerSymbol();
								strSMS = StoreSimpleMarker(SMS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strSMS.Label = Renderer.Label[j];
								strSMS.LowerLimit = double.Parse(cLowerLimit);
								strSMS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strSMS);
								break;
								
							case "ICharacterMarkerSymbol":
								ICharacterMarkerSymbol CMS = default(ICharacterMarkerSymbol);
                                CMS = objSymbol as ICharacterMarkerSymbol;
								StructCharacterMarkerSymbol strCMS = new StructCharacterMarkerSymbol();
								strCMS = StoreCharacterMarker(CMS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strCMS.Label = Renderer.Label[j];
								strCMS.LowerLimit = double.Parse(cLowerLimit);
								strCMS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strCMS);
								break;
								
							case "IPictureMarkerSymbol":
								IPictureMarkerSymbol PMS = default(IPictureMarkerSymbol);
                                PMS = objSymbol as IPictureMarkerSymbol;
								StructPictureMarkerSymbol strPMS = new StructPictureMarkerSymbol();
								strPMS = StorePictureMarker(PMS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strPMS.Label = Renderer.Label[j];
								strPMS.LowerLimit = double.Parse(cLowerLimit);
								strPMS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strPMS);
								break;
								
							case "IArrowMarkerSymbol":
								IArrowMarkerSymbol AMS = default(IArrowMarkerSymbol);
                                AMS = objSymbol as IArrowMarkerSymbol;
								StructArrowMarkerSymbol strAMS = new StructArrowMarkerSymbol();
								strAMS = StoreArrowMarker(AMS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strAMS.Label = Renderer.Label[j];
								strAMS.LowerLimit = double.Parse(cLowerLimit);
								strAMS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strAMS);
								break;
								
							case "IMultiLayerMarkerSymbol":
								IMultiLayerMarkerSymbol MLMS = default(IMultiLayerMarkerSymbol);
                                MLMS = objSymbol as IMultiLayerMarkerSymbol;
								StructMultilayerMarkerSymbol strMLMS = new StructMultilayerMarkerSymbol();
								strMLMS = StoreMultiLayerMarker(MLMS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strMLMS.Label = Renderer.Label[j];
								strMLMS.LowerLimit = double.Parse(cLowerLimit);
								strMLMS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strMLMS);
								break;
								
							case "false":
								InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreStructLayer");
								break;
						}
					}
					//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
					if (objFstOrderSymbol is ILineSymbol)
					{
						strRenderer.FeatureCls = FeatureClass.LineFeature;
						ILineSymbol objSymbol = default(ILineSymbol);
                        objSymbol = objFstOrderSymbol as ILineSymbol;
						switch (LineSymbolScan(objSymbol))
						{
							case "ICartographicLineSymbol":
								ICartographicLineSymbol CLS = default(ICartographicLineSymbol);
                                CLS = objSymbol as ICartographicLineSymbol;
								StructCartographicLineSymbol strCLS = new StructCartographicLineSymbol();
								strCLS = StoreCartographicLine(CLS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strCLS.Label = Renderer.Label[j];
								strCLS.LowerLimit = double.Parse(cLowerLimit);
								strCLS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strCLS);
								break;
								
							case "IHashLineSymbol":
								IHashLineSymbol HLS = default(IHashLineSymbol);
                                HLS = objSymbol as IHashLineSymbol;
								StructHashLineSymbol strHLS = new StructHashLineSymbol();
								strHLS = StoreHashLine(HLS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strHLS.Label = Renderer.Label[j];
								strHLS.LowerLimit = double.Parse(cLowerLimit);
								strHLS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strHLS);
								break;
								
							case "IMarkerLineSymbol":
								IMarkerLineSymbol MLS = default(IMarkerLineSymbol);
                                MLS = objSymbol as IMarkerLineSymbol;
								StructMarkerLineSymbol strMLS = new StructMarkerLineSymbol();
								strMLS = StoreMarkerLine(MLS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strMLS.Label = Renderer.Label[j];
								strMLS.LowerLimit = double.Parse(cLowerLimit);
								strMLS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strMLS);
								break;
								
							case "ISimpleLineSymbol":
								ISimpleLineSymbol SLS = default(ISimpleLineSymbol);
                                SLS = objSymbol as ISimpleLineSymbol;
								StructSimpleLineSymbol strSLS = new StructSimpleLineSymbol();
								strSLS = StoreSimpleLine(SLS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strSLS.Label = Renderer.Label[j];
								strSLS.LowerLimit = double.Parse(cLowerLimit);
								strSLS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strSLS);
								break;
								
							case "IPictureLineSymbol":
								IPictureLineSymbol PLS = default(IPictureLineSymbol);
                                PLS = objSymbol as IPictureLineSymbol;
								StructPictureLineSymbol strPLS = new StructPictureLineSymbol();
								strPLS = StorePictureLine(PLS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strPLS.Label = Renderer.Label[j];
								strPLS.LowerLimit = double.Parse(cLowerLimit);
								strPLS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strPLS);
								break;
								
							case "IMultiLayerLineSymbol":
								IMultiLayerLineSymbol MLLS = default(IMultiLayerLineSymbol);
                                MLLS = objSymbol as IMultiLayerLineSymbol;
								StructMultilayerLineSymbol strMLLS = new StructMultilayerLineSymbol();
								strMLLS = StoreMultilayerLines(MLLS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strMLLS.Label = Renderer.Label[j];
								strMLLS.LowerLimit = double.Parse(cLowerLimit);
								strMLLS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strMLLS);
								break;
								
							case "false":
								InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreStructLayer");
								break;
						}
						
					}
					//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
					if (objFstOrderSymbol is IFillSymbol)
					{
						strRenderer.FeatureCls = FeatureClass.PolygonFeature;
						IFillSymbol objSymbol = default(IFillSymbol);
                        objSymbol = objFstOrderSymbol as IFillSymbol;
						switch (FillSymbolScan(objSymbol))
						{
							case "ISimpleFillSymbol":
								ISimpleFillSymbol SFS = default(ISimpleFillSymbol);
                                SFS = objSymbol as ISimpleFillSymbol;
								StructSimpleFillSymbol strSFS = new StructSimpleFillSymbol();
								strSFS = StoreSimpleFill(SFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strSFS.Label = Renderer.Label[j];
								strSFS.LowerLimit = double.Parse(cLowerLimit);
								strSFS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strSFS);
								break;
								
							case "IMarkerFillSymbol":
								IMarkerFillSymbol MFS = default(IMarkerFillSymbol);
                                MFS = objSymbol as IMarkerFillSymbol;
								StructMarkerFillSymbol strMFS = new StructMarkerFillSymbol();
								strMFS = StoreMarkerFill(MFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strMFS.Label = Renderer.Label[j];
								strMFS.LowerLimit = double.Parse(cLowerLimit);
								strMFS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strMFS);
								break;
								
							case "ILineFillSymbol":
								ILineFillSymbol LFS = default(ILineFillSymbol);
                                LFS = objSymbol as ILineFillSymbol;
								StructLineFillSymbol strLFS = new StructLineFillSymbol();
								strLFS = StoreLineFill(LFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strLFS.Label = Renderer.Label[j];
								strLFS.LowerLimit = double.Parse(cLowerLimit);
								strLFS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strLFS);
								break;
								
							case "IDotDensityFillSymbol":
								IDotDensityFillSymbol DFS = default(IDotDensityFillSymbol);
                                DFS = objSymbol as IDotDensityFillSymbol;
								StructDotDensityFillSymbol strDFS = new StructDotDensityFillSymbol();
								strDFS = StoreDotDensityFill(DFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strDFS.Label = Renderer.Label[j];
								strDFS.LowerLimit = double.Parse(cLowerLimit);
								strDFS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strDFS);
								break;
								
							case "IPictureFillSymbol":
								IPictureFillSymbol PFS = default(IPictureFillSymbol);
                                PFS = objSymbol as IPictureFillSymbol;
								StructPictureFillSymbol strPFS = new StructPictureFillSymbol();
								strPFS = StorePictureFill(PFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strPFS.Label = Renderer.Label[j];
								strPFS.LowerLimit = double.Parse(cLowerLimit);
								strPFS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strPFS);
								break;
								
							case "IGradientFillSymbol":
								IGradientFillSymbol GFS = default(IGradientFillSymbol);
                                GFS = objSymbol as IGradientFillSymbol;
								StructGradientFillSymbol strGFS = new StructGradientFillSymbol();
								strGFS = StoreGradientFill(GFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strGFS.Label = Renderer.Label[j];
								strGFS.LowerLimit = double.Parse(cLowerLimit);
								strGFS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strGFS);
								break;
								
							case "IMultiLayerFillSymbol":
								IMultiLayerFillSymbol MLFS = default(IMultiLayerFillSymbol);
                                MLFS = objSymbol as IMultiLayerFillSymbol;
								StructMultilayerFillSymbol strMLFS = new StructMultilayerFillSymbol();
								strMLFS = StoreMultiLayerFill(MLFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strMLFS.Label = Renderer.Label[j];
								strMLFS.LowerLimit = double.Parse(cLowerLimit);
								strMLFS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strMLFS);
								break;
								
							case "false":
								InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreStructLayer");
								break;
						}
					}
					//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
					if (objFstOrderSymbol is I3DChartSymbol)
					{
						I3DChartSymbol objSymbol = default(I3DChartSymbol);
                        objSymbol = objFstOrderSymbol as I3DChartSymbol;
						switch (IIIDChartSymbolScan(objSymbol))
						{
							case "IBarChartSymbol":
								IBarChartSymbol BCS = default(IBarChartSymbol);
                                BCS = objSymbol as IBarChartSymbol;
								StructBarChartSymbol strBCS = new StructBarChartSymbol();
								strBCS = StoreBarChart(BCS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strBCS.Label = Renderer.Label[j];
								strBCS.LowerLimit = double.Parse(cLowerLimit);
								strBCS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strBCS);
								break;
								
							case "IPieChartSymbol":
								IPieChartSymbol PCS = default(IPieChartSymbol);
                                PCS = objSymbol as IPieChartSymbol;
								StructPieChartSymbol strPCS = new StructPieChartSymbol();
								strPCS = StorePieChart(PCS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strPCS.Label = Renderer.Label[j];
								strPCS.LowerLimit = double.Parse(cLowerLimit);
								strPCS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strPCS);
								break;
								
							case "IStackedChartSymbol":
								IStackedChartSymbol SCS = default(IStackedChartSymbol);
                                SCS = objSymbol as IStackedChartSymbol;
								StructStackedChartSymbol strSCS = new StructStackedChartSymbol();
								strSCS = StoreStackedChart(SCS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strSCS.Label = Renderer.Label[j];
								strSCS.LowerLimit = double.Parse(cLowerLimit);
								strSCS.UpperLimit = double.Parse(cUpperLimit);
								strRenderer.SymbolList.Add(strSCS);
								break;
								
							case "false":
								InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreStructLayer");
								break;
						}
					}
					//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
				}
				return strRenderer;
			}
			catch (Exception ex)
			{
				
				ErrorMsg("Fehler beim Speichern der Symbole in den Layerstrukturen", ex.Message, ex.StackTrace, "StoreStructCBRenderer");
                return strRenderer;
			}
		}
		
		//************************************************************************************************
		//Die Funktion durchl鋟ft die die diversen Symbolarten, verteilt sie auf die jeweiligen Unter-
		//symbolarten, und speichert diese dann in der Datenstruktur StructUniqueValueRenderer ab.
		//In der Funktion werden Daten auf 2 Ebenen gespeichert: Auf Rendererebene (die ersten Speicherungen);
		//und auf Symbolebene (die Fallunterscheidungen der einzelnen Symbolarten)
		//Parameter: Renderer: das aktuelle Rendererobjekt. Pro Layer eines
		//           Layer: das aktuelle Layerobjekt
		//************************************************************************************************
		private StructUniqueValueRenderer StoreStructUVRenderer(IUniqueValueRenderer Renderer, IFeatureLayer Layer)
		{
			StructUniqueValueRenderer strRenderer = new StructUniqueValueRenderer(); //Hierin wird das eine Rendererobjekt gespeichert plus zus鋞zliche Layerinformationen
			int iNumberOfSymbols = 0; //Anzahl aller Symbole des Rendererobjekts
			iNumberOfSymbols = Renderer.ValueCount; //Anzahl der Symbole
			ISymbol objFstOrderSymbol = default(ISymbol); //Das gerade aktuelle Symbol des durchlaufenen Rendererobjekts
			ArrayList alFieldNames = new ArrayList(); //Die ArrayLisit mit den Feldnamen
			bool bNoSepFieldVal; //Wenn nur nach einem Feld klassifiziert wurde muss der Flag auf true gesetzt werden
			ITable objTable = default(ITable); //Der FeatureTable, der an den Layer gebunden ist
			IDataset objDataset = default(IDataset); //Der aktuelle Dataset
            objTable = Layer.FeatureClass as ITable;
            objDataset = objTable as IDataset;
			strRenderer.SymbolList = new ArrayList();
			bNoSepFieldVal = false; //!!!! Damit nicht in die aufwendige Funktion GimmeSeperateFieldValues(..) gegangen werden mu? wenn nicht n鰐ig
			m_alClassifiedFields = new ArrayList(); //enth鋖t jeweils wiederum arraylist(s) von den normalisierten Werten aus den Feldern, nach denen klassifiziert wurde
			bool bIsJoined;
			bIsJoined = false;
			
			
			try
			{
				strRenderer.ValueCount = iNumberOfSymbols;
				strRenderer.LayerName = Layer.Name;
				strRenderer.DatasetName = objDataset.Name;
				strRenderer.Annotation = GetAnnotation(Layer);
				//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
				//Hier werden die Informationen 黚er Felder nach denen klassifiziert wurde gesammelt
				int iFieldCount = 0; //Anzahl der Spalten, nach denen klassifiziert wurde
				iFieldCount = Renderer.FieldCount;
				strRenderer.FieldCount = iFieldCount;
				if (iFieldCount > 1) //!!!! Damit nicht in die aufwendige Funktion GimmeSeperateFieldValues(..) gegangen werden mu? wenn nicht n鰐ig
				{
					bNoSepFieldVal = true; //und damit nicht die aufw鋘digen UniqueValue-Listen erzeugt werden m黶sen, wenn nicht notwendig
				}
				
				//diese Objekte dienen lediglich der 躡erpr黤ung, ob eine andere Tabelle an die Featuretable gejoint ist
				//++++++++++++++++++++++++++++++++++++++++++
				ITable pTable = default(ITable);
				IDisplayTable pDisplayTable = default(IDisplayTable);
                pDisplayTable = Layer as IDisplayTable;
				pTable = pDisplayTable.DisplayTable;
				if (pTable is IRelQueryTable) //Dann ist eine Tabelle drangejoint
				{
					bIsJoined = true;
				}
				//++++++++++++++++++++++++++++++++++++++++++
				
				//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
				//Das ganze nachfolgende Konstrukt dient nur dem Zweck, Unique-Value-Listen aus den Datenbanktabellen zu erstellen,
				//um sp鋞er mit der Funktion GimmeSeperateFieldValues die separaten Spaltenwerte, die zu einem Symbol geh鰎en, zu erhalten,
				//wenn (und nur wenn) nach mehr als 1 Wert klassifiziert wurde (da esri in diesem Fall nur einen aus allen Feldwerten
				//zusammengesetzten String liefert)
				int i = 0;
				if (bNoSepFieldVal == true) //Feldnamen (Spaltennamen) werden nur dann gespeichert, wenn nach mehr als 1 Feld klassifiziert wurde. (da sehr aufw鋘dige Sache!)
				{
					
					//Wenn der aktuelle Layer aus einem Shapefile stammt, ist das weitere Vorgehen
					//unterschiedlich zu dem Fall, wenn er aus DB's stammt:
					if (objDataset.Workspace.Type == esriWorkspaceType.esriFileSystemWorkspace)
					{
						for (i = 1; i <= iFieldCount; i++)
						{
							alFieldNames.Add(Renderer.Field[i - 1]); //Die Spaltennamen werden alle abgespeichert
						}
						GimmeUniqueValuesFromShape(objTable, alFieldNames);
						//GimmeUniqueValuesFromShape(Layer, alFieldNames)
						strRenderer.FieldNames = alFieldNames;
					}
					else
					{
						for (i = 1; i <= iFieldCount; i++)
						{
							alFieldNames.Add(Renderer.Field[i - 1]); //Die Spaltennamen werden alle abgespeichert
							//wenn eine andere Tabelle an die Featuretable gejoint ist
							if (pTable is IRelQueryTable)
							{
								// ++ Get the list of joined tables
								IRelQueryTable pRelQueryTable = default(IRelQueryTable);
								ITable pDestTable = default(ITable);
								IDataset pDataSet = default(IDataset);
								//Dim cTable As String
								ArrayList alJoinedTableNames = new ArrayList();
								while (pTable is IRelQueryTable)
								{
                                    pRelQueryTable = pTable as IRelQueryTable;
									pDestTable = pRelQueryTable.DestinationTable;
                                    pDataSet = pDestTable as IDataset;
									//cTable = cTable & pDataSet.Name
									pTable = pRelQueryTable.SourceTable;
									alJoinedTableNames.Add(pDataSet.Name);
								}
								GimmeUniqeValuesForFieldname(objTable, System.Convert.ToString(Renderer.Field[i - 1]), alJoinedTableNames); //Hier wird die Funktion aufgerufen, um die Unique Values des jew. Feldes abzuspeichern
								pTable = pDisplayTable.DisplayTable; //Zur點ksetzen des pTable, damit beim n鋍hsten Durchlauf wieder abgefragt werden kann
							}
							else //Bei nichtgejointer Tabelle  nat黵lich Aufruf der Funktion ohne die gejointen Tabellennamen
							{
								GimmeUniqeValuesForFieldname(objTable, Renderer.Field[i - 1]); //Hier wird die Funktion aufgerufen, um die Unique Values des jew. Feldes abzuspeichern
							}
						}
						strRenderer.FieldNames = alFieldNames;
					}
				}
				else
				{
					alFieldNames.Add(Renderer.Field[iFieldCount - 1]);
					strRenderer.FieldNames = alFieldNames;
				}
				
				//Je nachdem, welcher Zielkartendienst gew鋒lt wurde, muss Fieldname ein anderes Format besitzen
				if (bIsJoined == false) //Wenn eine Tabelle drangejoint wurde, ist sowieso schon der Datasetname mit dabei
				{
					int idummy;
					if (frmMotherform.chkArcIMS.Checked == true)
					{
						for (i = 0; i <= strRenderer.FieldNames.Count - 1; i++)
						{
							strRenderer.FieldNames[i] = objDataset.Name + "." + System.Convert.ToString(strRenderer.FieldNames[i]);
						}
					}
				}
				
				
				//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
				
				//AB HIER BEGINNEN DIE FALLUNTERSCHEIDUNGEN DER SYMBOLE
				int j = 0;
				for (j = 0; j <= iNumberOfSymbols - 1; j++)
				{
					frmMotherform.CHLabelSmall("Symbol " + (j + 1).ToString() + " von " + iNumberOfSymbols.ToString()); //Anzeige auf dem Label
					objFstOrderSymbol = Renderer.Symbol[Renderer.get_Value(j)]; //Die Zuweisung der jeweiligen einzelnen Symbole
					//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
					if (objFstOrderSymbol is ITextSymbol)
					{
						StructTextSymbol strTS = new StructTextSymbol();
						ITextSymbol objSymbol = default(ITextSymbol);
                        objSymbol = objFstOrderSymbol as ITextSymbol;
						strTS = StoreText(objSymbol);
						//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
						strTS.Label = Renderer.Label[Renderer.get_Value(j)];
						strTS.Fieldvalues = getUVFieldValues(Renderer, j);
						strRenderer.SymbolList.Add(strTS);
					}
					//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx()
					if (objFstOrderSymbol is IMarkerSymbol)
					{
						strRenderer.FeatureCls = FeatureClass.PointFeature;
						IMarkerSymbol objSymbol = default(IMarkerSymbol);
                        objSymbol = objFstOrderSymbol as IMarkerSymbol;
						switch (MarkerSymbolScan(objSymbol))
						{
							case "ISimpleMarkerSymbol":
								ISimpleMarkerSymbol SMS = default(ISimpleMarkerSymbol);
                                SMS = objSymbol as ISimpleMarkerSymbol;
								StructSimpleMarkerSymbol strSMS = new StructSimpleMarkerSymbol();
								strSMS = StoreSimpleMarker(SMS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strSMS.Label = Renderer.Label[Renderer.get_Value(j)];
								strSMS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strSMS);
								break;
								
							case "ICharacterMarkerSymbol":
								ICharacterMarkerSymbol CMS = default(ICharacterMarkerSymbol);
                                CMS = objSymbol as ICharacterMarkerSymbol;
								StructCharacterMarkerSymbol strCMS = new StructCharacterMarkerSymbol();
								strCMS = StoreCharacterMarker(CMS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strCMS.Label = Renderer.Label[Renderer.get_Value(j)];
								strCMS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strCMS);
								break;
								
							case "IPictureMarkerSymbol":
								IPictureMarkerSymbol PMS = default(IPictureMarkerSymbol);
                                PMS = objSymbol as IPictureMarkerSymbol;
								StructPictureMarkerSymbol strPMS = new StructPictureMarkerSymbol();
								strPMS = StorePictureMarker(PMS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strPMS.Label = Renderer.Label[Renderer.get_Value(j)];
								strPMS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strPMS);
								break;
								
							case "IArrowMarkerSymbol":
								IArrowMarkerSymbol AMS = default(IArrowMarkerSymbol);
                                AMS = objSymbol as IArrowMarkerSymbol;
								StructArrowMarkerSymbol strAMS = new StructArrowMarkerSymbol();
								strAMS = StoreArrowMarker(AMS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strAMS.Label = Renderer.Label[Renderer.get_Value(j)];
								strAMS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strAMS);
								break;
								
							case "IMultiLayerMarkerSymbol":
								IMultiLayerMarkerSymbol MLMS = default(IMultiLayerMarkerSymbol);
                                MLMS = objSymbol as IMultiLayerMarkerSymbol;
								StructMultilayerMarkerSymbol strMLMS = new StructMultilayerMarkerSymbol();
								strMLMS = StoreMultiLayerMarker(MLMS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strMLMS.Label = Renderer.Label[Renderer.get_Value(j)];
								strMLMS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strMLMS);
								break;
								
							case "false":
								InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreStructLayer");
								break;
						}
					}
					//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
					if (objFstOrderSymbol is ILineSymbol)
					{
						strRenderer.FeatureCls = FeatureClass.LineFeature;
						ILineSymbol objSymbol = default(ILineSymbol);
                        objSymbol = objFstOrderSymbol as ILineSymbol;
						switch (LineSymbolScan(objSymbol))
						{
							case "ICartographicLineSymbol":
								ICartographicLineSymbol CLS = default(ICartographicLineSymbol);
                                CLS = objSymbol as ICartographicLineSymbol;
								StructCartographicLineSymbol strCLS = new StructCartographicLineSymbol();
								strCLS = StoreCartographicLine(CLS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strCLS.Label = Renderer.Label[Renderer.get_Value(j)];
								strCLS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strCLS);
								break;
								
							case "IHashLineSymbol":
								IHashLineSymbol HLS = default(IHashLineSymbol);
                                HLS = objSymbol as IHashLineSymbol;
								StructHashLineSymbol strHLS = new StructHashLineSymbol();
								strHLS = StoreHashLine(HLS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strHLS.Label = Renderer.Label[Renderer.get_Value(j)];
								strHLS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strHLS);
								break;
								
							case "IMarkerLineSymbol":
								IMarkerLineSymbol MLS = default(IMarkerLineSymbol);
                                MLS = objSymbol as IMarkerLineSymbol;
								StructMarkerLineSymbol strMLS = new StructMarkerLineSymbol();
								strMLS = StoreMarkerLine(MLS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strMLS.Label = Renderer.Label[Renderer.get_Value(j)];
								strMLS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strMLS);
								break;
								
							case "ISimpleLineSymbol":
								ISimpleLineSymbol SLS = default(ISimpleLineSymbol);
                                SLS = objSymbol as ISimpleLineSymbol;
								StructSimpleLineSymbol strSLS = new StructSimpleLineSymbol();
								strSLS = StoreSimpleLine(SLS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strSLS.Label = Renderer.Label[Renderer.get_Value(j)];
								strSLS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strSLS);
								break;
								
							case "IPictureLineSymbol":
								IPictureLineSymbol PLS = default(IPictureLineSymbol);
                                PLS = objSymbol as IPictureLineSymbol;
								StructPictureLineSymbol strPLS = new StructPictureLineSymbol();
								strPLS = StorePictureLine(PLS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strPLS.Label = Renderer.Label[Renderer.get_Value(j)];
								strPLS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strPLS);
								break;
								
							case "IMultiLayerLineSymbol":
								IMultiLayerLineSymbol MLLS = default(IMultiLayerLineSymbol);
                                MLLS = objSymbol as IMultiLayerLineSymbol;
								StructMultilayerLineSymbol strMLLS = new StructMultilayerLineSymbol();
								strMLLS = StoreMultilayerLines(MLLS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strMLLS.Label = Renderer.Label[Renderer.get_Value(j)];
								strMLLS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strMLLS);
								break;
								
							case "false":
								InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreStructLayer");
								break;
						}
						
					}
					//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
					if (objFstOrderSymbol is IFillSymbol)
					{
						strRenderer.FeatureCls = FeatureClass.PolygonFeature;
						IFillSymbol objSymbol = default(IFillSymbol);
                        objSymbol = objFstOrderSymbol as IFillSymbol;
						switch (FillSymbolScan(objSymbol))
						{
							case "ISimpleFillSymbol":
								ISimpleFillSymbol SFS = default(ISimpleFillSymbol);
                                SFS = objSymbol as ISimpleFillSymbol;
								StructSimpleFillSymbol strSFS = new StructSimpleFillSymbol();
								strSFS = StoreSimpleFill(SFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strSFS.Label = Renderer.Label[Renderer.get_Value(j)];
								strSFS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strSFS);
								break;
								
							case "IMarkerFillSymbol":
								IMarkerFillSymbol MFS = default(IMarkerFillSymbol);
                                MFS = objSymbol as IMarkerFillSymbol;
								StructMarkerFillSymbol strMFS = new StructMarkerFillSymbol();
								strMFS = StoreMarkerFill(MFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strMFS.Label = Renderer.Label[Renderer.get_Value(j)];
								strMFS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strMFS);
								break;
								
							case "ILineFillSymbol":
								ILineFillSymbol LFS = default(ILineFillSymbol);
                                LFS = objSymbol as ILineFillSymbol;
								StructLineFillSymbol strLFS = new StructLineFillSymbol();
								strLFS = StoreLineFill(LFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strLFS.Label = Renderer.Label[Renderer.get_Value(j)];
								strLFS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strLFS);
								break;
								
							case "IDotDensityFillSymbol":
								IDotDensityFillSymbol DFS = default(IDotDensityFillSymbol);
                                DFS = objSymbol as IDotDensityFillSymbol;
								StructDotDensityFillSymbol strDFS = new StructDotDensityFillSymbol();
								strDFS = StoreDotDensityFill(DFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strDFS.Label = Renderer.Label[Renderer.get_Value(j)];
								strDFS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strDFS);
								break;
								
							case "IPictureFillSymbol":
								IPictureFillSymbol PFS = default(IPictureFillSymbol);
                                PFS = objSymbol as IPictureFillSymbol;
								StructPictureFillSymbol strPFS = new StructPictureFillSymbol();
								strPFS = StorePictureFill(PFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strPFS.Label = Renderer.Label[Renderer.get_Value(j)];
								strPFS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strPFS);
								break;
								
							case "IGradientFillSymbol":
								IGradientFillSymbol GFS = default(IGradientFillSymbol);
                                GFS = objSymbol as IGradientFillSymbol;
								StructGradientFillSymbol strGFS = new StructGradientFillSymbol();
								strGFS = StoreGradientFill(GFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strGFS.Label = Renderer.Label[Renderer.get_Value(j)];
								strGFS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strGFS);
								break;
								
							case "IMultiLayerFillSymbol":
								IMultiLayerFillSymbol MLFS = default(IMultiLayerFillSymbol);
                                MLFS = objSymbol as IMultiLayerFillSymbol;
								StructMultilayerFillSymbol strMLFS = new StructMultilayerFillSymbol();
								strMLFS = StoreMultiLayerFill(MLFS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strMLFS.Label = Renderer.Label[Renderer.get_Value(j)];
								strMLFS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strMLFS);
								break;
								
							case "false":
								InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreStructLayer");
								break;
						}
					}
					//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
					if (objFstOrderSymbol is I3DChartSymbol)
					{
						I3DChartSymbol objSymbol = default(I3DChartSymbol);
                        objSymbol = objFstOrderSymbol as I3DChartSymbol;
						switch (IIIDChartSymbolScan(objSymbol))
						{
							case "IBarChartSymbol":
								IBarChartSymbol BCS = default(IBarChartSymbol);
                                BCS = objSymbol as IBarChartSymbol;
								StructBarChartSymbol strBCS = new StructBarChartSymbol();
								strBCS = StoreBarChart(BCS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strBCS.Label = Renderer.Label[Renderer.get_Value(j)];
								strBCS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strBCS);
								break;
								
							case "IPieChartSymbol":
								IPieChartSymbol PCS = default(IPieChartSymbol);
                                PCS = objSymbol as IPieChartSymbol;
								StructPieChartSymbol strPCS = new StructPieChartSymbol();
								strPCS = StorePieChart(PCS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strPCS.Label = Renderer.Label[Renderer.get_Value(j)];
								strPCS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strPCS);
								break;
								
							case "IStackedChartSymbol":
								IStackedChartSymbol SCS = default(IStackedChartSymbol);
                                SCS = objSymbol as IStackedChartSymbol;
								StructStackedChartSymbol strSCS = new StructStackedChartSymbol();
								strSCS = StoreStackedChart(SCS);
								//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
								strSCS.Label = Renderer.Label[Renderer.get_Value(j)];
								strSCS.Fieldvalues = getUVFieldValues(Renderer, j);
								strRenderer.SymbolList.Add(strSCS);
								break;
								
							case "false":
								InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreStructLayer");
								break;
						}
					}
					//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
				}
				return strRenderer;
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Speichern der Symbole in den Layerstrukturen", ex.Message, ex.StackTrace, "StoreStructUVRenderer");
                return strRenderer;
			}
		}
		
		//************************************************************************************************
		//For Unique Value renderers get the field value or values for the given value index.
		//With multiple fields, the result is a list of the values for each of the fields.
		//With a single field, the result is a list with either the single value, or a
		//list with multiple values if several of the next value indices belong to the
		//same group.
		//************************************************************************************************
		private ArrayList getUVFieldValues(IUniqueValueRenderer Renderer, int Index)
		{
			//Es macht nur Sinn hier Fieldvalues einzuf黦en, wenn 黚erhaupt nach einem Feld klassifiziert wurde
			ArrayList Fieldvalues = default(ArrayList);
			int iFieldCount = 0; //Anzahl der Spalten, nach denen klassifiziert wurde
			int Index2 = 0;
			
			iFieldCount = Renderer.FieldCount;
			Fieldvalues = (ArrayList) null;
			if (iFieldCount > 0)
			{
				bool bNoSepFieldVal; //Wenn nur nach einem Feld klassifiziert wurde muss der Flag auf true gesetzt werden
				string Label;
				string Label2 = "";
				ISymbol objSymbol = default(ISymbol);
				int iNumberOfSymbols = 0; //Anzahl aller Symbole des Rendererobjekts
				iNumberOfSymbols = Renderer.ValueCount; //Anzahl der Symbole
				bNoSepFieldVal = false;
				if (iFieldCount > 1) //!!!! Damit nicht in die aufwendige Funktion GimmeSeperateFieldValues(..) gegangen werden mu? wenn nicht n鰐ig
				{
					bNoSepFieldVal = true; //und damit nicht die aufw鋘digen UniqueValue-Listen erzeugt werden m黶sen, wenn nicht notwendig
				}
				
				Label = Renderer.Label[Renderer.Value[Index]];
				if (bNoSepFieldVal == false)
				{
					Fieldvalues = new ArrayList();
					Fieldvalues.Add(Renderer.Value[Index]); //Wenn nur nach 1 Feld klassifiziert wurde, muss nicht in GimmeSeperateFieldValues gegangen werden (Zeit!!!)
					//Find grouped values with the same label (values where no symbol defined).
					Index2 = Index + 1;
					while (Index2 < iNumberOfSymbols)
					{
						objSymbol = Renderer.Symbol[Renderer.Value[Index2]];
						Label2 = Renderer.Label[Renderer.Value[Index2]];
						if (objSymbol == null && Label == Label2)
						{
							Fieldvalues.Add(Renderer.Value[Index2]);
						}
						else
						{
							break;
						}
						Index2++;
					}
				}
				else
				{
					Fieldvalues = GimmeSeperateFieldValues(Renderer.Value[Index], Renderer.FieldDelimiter);
				}
			}
			return Fieldvalues;
		}
		
		//************************************************************************************************
		//Die Funktion speichert die SimpleMarker in seiner Datenstruktur
		//************************************************************************************************
		private StructSimpleMarkerSymbol StoreSimpleMarker(ISimpleMarkerSymbol symbol)
		{
			StructSimpleMarkerSymbol StructStorage = new StructSimpleMarkerSymbol();
			StructStorage.Angle = symbol.Angle;
			StructStorage.Filled = symbol.Color.Transparency != 0;
			StructStorage.Color = GimmeStringForColor(symbol.Color);
			StructStorage.Outline = symbol.Outline;
			StructStorage.OutlineColor = GimmeStringForColor(symbol.OutlineColor);
			StructStorage.OutlineSize = symbol.OutlineSize;
			StructStorage.Size = symbol.Size;
			StructStorage.Style = symbol.Style.ToString();
			StructStorage.XOffset = symbol.XOffset;
			StructStorage.YOffset = symbol.YOffset;
			return StructStorage;
		}
		
		
		//************************************************************************************************
		//Die Funktion speichert die CharacterMarker in seiner Datenstruktur
		//************************************************************************************************
		private StructCharacterMarkerSymbol StoreCharacterMarker(ICharacterMarkerSymbol symbol)
		{
			StructCharacterMarkerSymbol StructStorage = new StructCharacterMarkerSymbol();
			StructStorage.Angle = symbol.Angle;
			StructStorage.CharacterIndex = symbol.CharacterIndex;
			StructStorage.Color = GimmeStringForColor(symbol.Color);
			StructStorage.Font = symbol.Font.Name;
			StructStorage.Size = symbol.Size;
			StructStorage.XOffset = symbol.XOffset;
			StructStorage.YOffset = symbol.YOffset;
			return StructStorage;
		}
		
		//************************************************************************************************
		//Die Funktion speichert die PictureMarker in seiner Datenstruktur
		//************************************************************************************************
		private StructPictureMarkerSymbol StorePictureMarker(IPictureMarkerSymbol symbol)
		{
			StructPictureMarkerSymbol StructStorage = new StructPictureMarkerSymbol();
			StructStorage.Angle = symbol.Angle;
			StructStorage.BackgroundColor = GimmeStringForColor(symbol.BackgroundColor);
			StructStorage.Color = GimmeStringForColor(symbol.Color);
			StructStorage.Picture = symbol.Picture as IPicture;
			StructStorage.Size = symbol.Size;
			StructStorage.XOffset = symbol.XOffset;
			StructStorage.YOffset = symbol.YOffset;
			return StructStorage;
		}
		
		//************************************************************************************************
		//Die Funktion speichert die ArrowMarker in seiner Datenstruktur
		//************************************************************************************************
		private StructArrowMarkerSymbol StoreArrowMarker(IArrowMarkerSymbol symbol)
		{
			StructArrowMarkerSymbol StructStorage = new StructArrowMarkerSymbol();
			StructStorage.Angle = symbol.Angle;
			StructStorage.Color = GimmeStringForColor(symbol.Color);
			StructStorage.Length = symbol.Length;
			StructStorage.Size = symbol.Size;
			StructStorage.Style = symbol.Style.ToString();
			StructStorage.Width = symbol.Width;
			StructStorage.XOffset = symbol.XOffset;
			StructStorage.YOffset = symbol.YOffset;
			return StructStorage;
		}
		
		//************************************************************************************************
		//Die Funktion speichert die MultiLayerMarker in ArrayList. Wieviel Layer wird in LayerCount gesp.
		//Das MultilayerMarker-Symbol kann nicht selbst nocheinmal aus einem MultilayerMarker bestehen
		//************************************************************************************************
		private StructMultilayerMarkerSymbol StoreMultiLayerMarker(IMultiLayerMarkerSymbol symbol)
		{
			StructMultilayerMarkerSymbol StructStorage = new StructMultilayerMarkerSymbol();
			StructStorage.MultiMarkerLayers = new ArrayList();
			int i = 0;
			StructStorage.LayerCount = symbol.LayerCount;
			for (i = 0; i <= symbol.LayerCount - 1; i++) //Damit alle Layer erfasst werden
			{
				switch (MarkerSymbolScan(symbol.Layer[i])) //Das Multilayersymbol besteht aus mehreren Layern, die alle durchlaufen werden m黶sen
				{
					case "ISimpleMarkerSymbol":
						ISimpleMarkerSymbol SMS = default(ISimpleMarkerSymbol);
                        SMS = symbol.get_Layer(i) as ISimpleMarkerSymbol;
						StructStorage.MultiMarkerLayers.Add(StoreSimpleMarker(SMS)); //Der Layer wird der Arraylist von Multilayern hinzugef黦t
						break;
					case "ICharacterMarkerSymbol":
						ICharacterMarkerSymbol CMS = default(ICharacterMarkerSymbol);
                        CMS = symbol.get_Layer(i) as ICharacterMarkerSymbol;
						StructStorage.MultiMarkerLayers.Add(StoreCharacterMarker(CMS)); //Der Layer wird der Arraylist von Multilayern hinzugef黦t
						break;
					case "IPictureMarkerSymbol":
						IPictureMarkerSymbol PMS = default(IPictureMarkerSymbol);
                        PMS = symbol.get_Layer(i) as IPictureMarkerSymbol;
						StructStorage.MultiMarkerLayers.Add(StorePictureMarker(PMS)); //Der Layer wird der Arraylist von Multilayern hinzugef黦t
						break;
					case "IArrowMarkerSymbol":
						IArrowMarkerSymbol AMS = default(IArrowMarkerSymbol);
                        AMS = symbol.get_Layer(i) as IArrowMarkerSymbol;
						StructStorage.MultiMarkerLayers.Add(StoreArrowMarker(AMS)); //Der Layer wird der Arraylist von Multilayern hinzugef黦t
						break;
					case "false":
						InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreMultiLayerMarker");
						break;
				}
			}
			return StructStorage;
		}
		//_______________________________________________________________________________________________________________________
		
		//************************************************************************************************
		//Die Funktion speichert die SimpleLines in seiner Datenstruktur
		//************************************************************************************************
		private StructSimpleLineSymbol StoreSimpleLine(ISimpleLineSymbol symbol)
		{
			StructSimpleLineSymbol StructStorage = new StructSimpleLineSymbol();
			if (symbol.Style == esriSimpleLineStyle.esriSLSNull)
			{
				StructStorage.Color = "";
			}
			else
			{
				StructStorage.Color = GimmeStringForColor(symbol.Color);
			}
			StructStorage.Transparency = symbol.Color.Transparency;
			StructStorage.publicStyle = symbol.Style.ToString();
			StructStorage.Width = symbol.Width;
			return StructStorage;
		}
		
		//************************************************************************************************
		//Die Funktion speichert die SimpleLines in seiner Datenstruktur
		//************************************************************************************************
		private StructCartographicLineSymbol StoreCartographicLine(ICartographicLineSymbol symbol)
		{
			StructCartographicLineSymbol StructStorage = new StructCartographicLineSymbol();
			StructStorage.Color = GimmeStringForColor(symbol.Color);
			StructStorage.Transparency = symbol.Color.Transparency;
			StructStorage.Width = symbol.Width;
			StructStorage.Join = symbol.Join.ToString();
			StructStorage.MiterLimit = symbol.MiterLimit;
			StructStorage.Cap = symbol.Cap.ToString();
			StructStorage.DashArray = new ArrayList();
			if (symbol is ILineProperties)
			{
				ILineProperties lineProperties = default(ILineProperties);
				ITemplate template = default(ITemplate);
				double markLen = 0;
				double gapLen = 0;
				double interval = 0;
				int templateIdx = 0;
                lineProperties = symbol as ILineProperties;
				if (lineProperties.Template is ITemplate)
				{
					template = lineProperties.Template;
					interval = template.Interval;
					for (templateIdx = 0; templateIdx <= template.PatternElementCount - 1; templateIdx++)
					{
                        template.GetPatternElement(templateIdx, out markLen, out gapLen);
						StructStorage.DashArray.Add(markLen * interval);
						StructStorage.DashArray.Add(gapLen * interval);
					}
				}
			}
			return StructStorage;
		}
		
		//************************************************************************************************
		//Die Funktion speichert die HashLines in seiner Datenstruktur
		//************************************************************************************************
		private StructHashLineSymbol StoreHashLine(IHashLineSymbol symbol)
		{
			StructHashLineSymbol StructStorage = new StructHashLineSymbol();
			StructStorage.Angle = symbol.Angle;
			StructStorage.Color = GimmeStringForColor(symbol.Color);
			StructStorage.Transparency = symbol.Color.Transparency;
			switch (LineSymbolScan(symbol.HashSymbol)) //symbol.HashSymbol ist ein Liniensymbol. deshalb bei case... kein IHashSymbol/StructHashLineSymbol
			{
				case "ICartographicLineSymbol":
					ICartographicLineSymbol CLS = default(ICartographicLineSymbol);
                    CLS = symbol.HashSymbol as ICartographicLineSymbol;
					StructStorage.HashSymbol_CartographicLine = StoreCartographicLine(CLS);
					StructStorage.kindOfLineStruct = LineStructs.StructCartographicLineSymbol;
					break;
				case "IMarkerLineSymbol":
					IMarkerLineSymbol MLS = default(IMarkerLineSymbol);
                    MLS = symbol.HashSymbol as IMarkerLineSymbol;
					StructStorage.HashSymbol_MarkerLine = StoreMarkerLine(MLS);
					StructStorage.kindOfLineStruct = LineStructs.StructMarkerLineSymbol;
					break;
				case "ISimpleLineSymbol":
					ISimpleLineSymbol SLS = default(ISimpleLineSymbol);
                    SLS = symbol.HashSymbol as ISimpleLineSymbol;
					StructStorage.HashSymbol_SimpleLine = StoreSimpleLine(SLS);
					StructStorage.kindOfLineStruct = LineStructs.StructSimpleLineSymbol;
					break;
				case "IPictureLineSymbol":
					IPictureLineSymbol PLS = default(IPictureLineSymbol);
                    PLS = symbol.HashSymbol as IPictureLineSymbol;
					StructStorage.HashSymbol_PictureLine = StorePictureLine(PLS);
					StructStorage.kindOfLineStruct = LineStructs.StructPictureLineSymbol;
					break;
				case "IMultiLayerLineSymbol":
					IMultiLayerLineSymbol MLLS = default(IMultiLayerLineSymbol);
                    MLLS = symbol.HashSymbol as IMultiLayerLineSymbol;
					StructStorage.HashSymbol_MultiLayerLines = StoreMultilayerLines(MLLS);
					StructStorage.kindOfLineStruct = LineStructs.StructMultilayerLineSymbol;
					break;
				case "false":
					InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreHashLine");
					break;
			}
			return StructStorage;
		}
		
		//************************************************************************************************
		//Die Funktion speichert die MarkerLines in seiner Datenstruktur
		//************************************************************************************************
		private StructMarkerLineSymbol StoreMarkerLine(IMarkerLineSymbol symbol)
		{
			StructMarkerLineSymbol StructStorage = new StructMarkerLineSymbol();
			StructStorage.Color = GimmeStringForColor(symbol.Color);
			StructStorage.Transparency = symbol.Color.Transparency;
			StructStorage.Width = symbol.Width;
			switch (MarkerSymbolScan(symbol.MarkerSymbol))
			{
				case "ISimpleMarkerSymbol":
					ISimpleMarkerSymbol SMS = default(ISimpleMarkerSymbol);
                    SMS = symbol.MarkerSymbol as ISimpleMarkerSymbol;
					StructStorage.MarkerSymbol_SimpleMarker = StoreSimpleMarker(SMS);
					StructStorage.kindOfMarkerStruct = MarkerStructs.StructSimpleMarkerSymbol;
					break;
				case "ICharacterMarkerSymbol":
					ICharacterMarkerSymbol CMS = default(ICharacterMarkerSymbol);
                    CMS = symbol.MarkerSymbol as ICharacterMarkerSymbol;
					StructStorage.MarkerSymbol_CharacterMarker = StoreCharacterMarker(CMS);
					StructStorage.kindOfMarkerStruct = MarkerStructs.StructCharacterMarkerSymbol;
					break;
				case "IPictureMarkerSymbol":
					IPictureMarkerSymbol PMS = default(IPictureMarkerSymbol);
                    PMS = symbol.MarkerSymbol as IPictureMarkerSymbol;
					StructStorage.MarkerSymbol_PictureMarker = StorePictureMarker(PMS);
					StructStorage.kindOfMarkerStruct = MarkerStructs.StructPictureMarkerSymbol;
					break;
				case "IArrowMarkerSymbol":
					IArrowMarkerSymbol AMS = default(IArrowMarkerSymbol);
                    AMS = symbol.MarkerSymbol as IArrowMarkerSymbol;
					StructStorage.MarkerSymbol_ArrowMarker = StoreArrowMarker(AMS);
					StructStorage.kindOfMarkerStruct = MarkerStructs.StructArrowMarkerSymbol;
					break;
				case "IMultiLayerMarkerSymbol":
					IMultiLayerMarkerSymbol MLMS = default(IMultiLayerMarkerSymbol);
                    MLMS = symbol.MarkerSymbol as IMultiLayerMarkerSymbol;
					StructStorage.MarkerSymbol_MultilayerMarker = StoreMultiLayerMarker(MLMS);
					StructStorage.kindOfMarkerStruct = MarkerStructs.StructMultilayerMarkerSymbol;
					break;
				case "false":
					InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreMarkerLine");
					break;
			}
			return StructStorage;
		}
		
		//************************************************************************************************
		//Die Funktion speichert die PictureLines in seiner Datenstruktur
		//************************************************************************************************
		private StructPictureLineSymbol StorePictureLine(IPictureLineSymbol symbol)
		{
			StructPictureLineSymbol StructStorage = new StructPictureLineSymbol();
			StructStorage.BackgroundColor = GimmeStringForColor(symbol.BackgroundColor);
			StructStorage.BackgroundTransparency = symbol.BackgroundColor.Transparency;
			StructStorage.Color = GimmeStringForColor(symbol.Color);
			StructStorage.Transparency = symbol.Color.Transparency;
			StructStorage.Offset = symbol.Offset;
			StructStorage.Picture = symbol.Picture as IPicture;
			StructStorage.Rotate = symbol.Rotate;
			StructStorage.Width = symbol.Width;
			StructStorage.XScale = symbol.XScale;
			StructStorage.YScale = symbol.YScale;
			return StructStorage;
		}
		
		//************************************************************************************************
		//Die Funktion speichert die MultilayerLines in seiner Datenstruktur
		//************************************************************************************************
		private StructMultilayerLineSymbol StoreMultilayerLines(IMultiLayerLineSymbol symbol)
		{
			StructMultilayerLineSymbol StructStorage = new StructMultilayerLineSymbol();
			StructStorage.MultiLineLayers = new ArrayList();
			int i = 0;
			StructStorage.LayerCount = symbol.LayerCount;
			for (i = 0; i <= symbol.LayerCount - 1; i++)
			{
				switch (LineSymbolScan(symbol.Layer[i]))
				{
					case "ICartographicLineSymbol":
						ICartographicLineSymbol CLS = default(ICartographicLineSymbol);
                        CLS = symbol.get_Layer(i) as ICartographicLineSymbol;
						StructStorage.MultiLineLayers.Add(StoreCartographicLine(CLS));
						break;
					case "IHashLineSymbol":
						IHashLineSymbol HLS = default(IHashLineSymbol);
                        HLS = symbol.get_Layer(i) as IHashLineSymbol;
						StructStorage.MultiLineLayers.Add(StoreHashLine(HLS));
						break;
					case "IMarkerLineSymbol":
						IMarkerLineSymbol MLS = default(IMarkerLineSymbol);
                        MLS = symbol.get_Layer(i) as IMarkerLineSymbol;
						StructStorage.MultiLineLayers.Add(StoreMarkerLine(MLS));
						break;
					case "ISimpleLineSymbol":
						ISimpleLineSymbol SLS = default(ISimpleLineSymbol);
                        SLS = symbol.get_Layer(i) as ISimpleLineSymbol;
						StructStorage.MultiLineLayers.Add(StoreSimpleLine(SLS));
						break;
					case "IPictureLineSymbol":
						IPictureLineSymbol PLS = default(IPictureLineSymbol);
                        PLS = symbol.get_Layer(i) as IPictureLineSymbol;
						StructStorage.MultiLineLayers.Add(StorePictureLine(PLS));
						break;
					case "IMultiLayerLineSymbol":
						IMultiLayerLineSymbol MLLS = default(IMultiLayerLineSymbol);
                        MLLS = symbol.get_Layer(i) as IMultiLayerLineSymbol;
						StructStorage.MultiLineLayers.Add(StoreMultilayerLines(MLLS)); //Hier ist ein rekursiver Aufruf
						break;
					case "false":
						InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreMultilayerLines");
						break;
				}
			}
			return StructStorage;
		}
		
		//_______________________________________________________________________________________________________________________
		
		
		//************************************************************************************************
		//Die Funktion speichert die SimpleFills in seiner Datenstruktur
		//************************************************************************************************
		private StructSimpleFillSymbol StoreSimpleFill(ISimpleFillSymbol symbol)
		{
			StructSimpleFillSymbol StructStorage = new StructSimpleFillSymbol();
			if (symbol.Style == esriSimpleFillStyle.esriSFSHollow)
			{
				StructStorage.Color = "";
			}
			else
			{
				StructStorage.Color = GimmeStringForColor(symbol.Color);
			}
			StructStorage.Style = symbol.Style.ToString();
			StructStorage.Transparency = symbol.Color.Transparency;
			switch (LineSymbolScan(symbol.Outline)) //symbol.Outline ist ein Liniensymbol
			{
				case "ICartographicLineSymbol":
					ICartographicLineSymbol CLS = default(ICartographicLineSymbol);
                    CLS = symbol.Outline as ICartographicLineSymbol;
					StructStorage.Outline_CartographicLine = StoreCartographicLine(CLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructCartographicLineSymbol;
					break;
				case "IMarkerLineSymbol":
					IMarkerLineSymbol MLS = default(IMarkerLineSymbol);
                    MLS = symbol.Outline as IMarkerLineSymbol;
					StructStorage.Outline_MarkerLine = StoreMarkerLine(MLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructMarkerLineSymbol;
					break;
				case "IHashLineSymbol":
					IHashLineSymbol HLS = default(IHashLineSymbol);
                    HLS = symbol.Outline as IHashLineSymbol;
					StructStorage.Outline_HashLine = StoreHashLine(HLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructHashLineSymbol;
					break;
				case "ISimpleLineSymbol":
					ISimpleLineSymbol SLS = default(ISimpleLineSymbol);
                    SLS = symbol.Outline as ISimpleLineSymbol;
					StructStorage.Outline_SimpleLine = StoreSimpleLine(SLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructSimpleLineSymbol;
					break;
				case "IPictureLineSymbol":
					IPictureLineSymbol PLS = default(IPictureLineSymbol);
                    PLS = symbol.Outline as IPictureLineSymbol;
					StructStorage.Outline_PictureLine = StorePictureLine(PLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructPictureLineSymbol;
					break;
				case "IMultiLayerLineSymbol":
					IMultiLayerLineSymbol MLLS = default(IMultiLayerLineSymbol);
                    MLLS = symbol.Outline as IMultiLayerLineSymbol;
					StructStorage.Outline_MultiLayerLines = StoreMultilayerLines(MLLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructMultilayerLineSymbol;
					break;
				case "false":
					InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreSimpleFill");
					break;
			}
			return StructStorage;
		}
		
		
		//************************************************************************************************
		//Die Funktion speichert die MarkerFillSymbols in seiner Datenstruktur
		//************************************************************************************************
		private StructMarkerFillSymbol StoreMarkerFill(IMarkerFillSymbol symbol)
		{
			StructMarkerFillSymbol StructStorage = new StructMarkerFillSymbol();
			StructStorage.Color = GimmeStringForColor(symbol.Color);
			StructStorage.Transparency = symbol.Color.Transparency;
			StructStorage.GridAngle = symbol.GridAngle;
			switch (MarkerSymbolScan(symbol.MarkerSymbol))
			{
				case "ISimpleMarkerSymbol":
					ISimpleMarkerSymbol SMS = default(ISimpleMarkerSymbol);
                    SMS = symbol.MarkerSymbol as ISimpleMarkerSymbol;
					StructStorage.MarkerSymbol_SimpleMarker = StoreSimpleMarker(SMS);
					StructStorage.kindOfMarkerStruct = MarkerStructs.StructSimpleMarkerSymbol;
					break;
				case "ICharacterMarkerSymbol":
					ICharacterMarkerSymbol CMS = default(ICharacterMarkerSymbol);
                    CMS = symbol.MarkerSymbol as ICharacterMarkerSymbol;
					StructStorage.MarkerSymbol_CharacterMarker = StoreCharacterMarker(CMS);
					StructStorage.kindOfMarkerStruct = MarkerStructs.StructCharacterMarkerSymbol;
					break;
				case "IPictureMarkerSymbol":
					IPictureMarkerSymbol PMS = default(IPictureMarkerSymbol);
                    PMS = symbol.MarkerSymbol as IPictureMarkerSymbol;
					StructStorage.MarkerSymbol_PictureMarker = StorePictureMarker(PMS);
					StructStorage.kindOfMarkerStruct = MarkerStructs.StructPictureMarkerSymbol;
					break;
				case "IArrowMarkerSymbol":
					IArrowMarkerSymbol AMS = default(IArrowMarkerSymbol);
                    AMS = symbol.MarkerSymbol as IArrowMarkerSymbol;
					StructStorage.MarkerSymbol_ArrowMarker = StoreArrowMarker(AMS);
					StructStorage.kindOfMarkerStruct = MarkerStructs.StructArrowMarkerSymbol;
					break;
				case "IMultiLayerMarkerSymbol":
					IMultiLayerMarkerSymbol MLMS = default(IMultiLayerMarkerSymbol);
                    MLMS = symbol.MarkerSymbol as IMultiLayerMarkerSymbol; 
					StructStorage.MarkerSymbol_MultilayerMarker = StoreMultiLayerMarker(MLMS);
					StructStorage.kindOfMarkerStruct = MarkerStructs.StructMultilayerMarkerSymbol;
					break;
				case "false":
					InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreMarkerFill");
					break;
			}
			switch (LineSymbolScan(symbol.Outline))
			{
				case "ICartographicLineSymbol":
					ICartographicLineSymbol CLS = default(ICartographicLineSymbol);
                    CLS = symbol.Outline as ICartographicLineSymbol;
					StructStorage.Outline_CartographicLine = StoreCartographicLine(CLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructCartographicLineSymbol;
					break;
				case "IMarkerLineSymbol":
					IMarkerLineSymbol MLS = default(IMarkerLineSymbol);
                    MLS = symbol.Outline as IMarkerLineSymbol;
					StructStorage.Outline_MarkerLine = StoreMarkerLine(MLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructMarkerLineSymbol;
					break;
				case "IHashLineSymbol":
					IHashLineSymbol HLS = default(IHashLineSymbol);
                    HLS = symbol.Outline as IHashLineSymbol;
					StructStorage.Outline_HashLine = StoreHashLine(HLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructHashLineSymbol;
					break;
				case "ISimpleLineSymbol":
					ISimpleLineSymbol SLS = default(ISimpleLineSymbol);
                    SLS = symbol.Outline as ISimpleLineSymbol;
					StructStorage.Outline_SimpleLine = StoreSimpleLine(SLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructSimpleLineSymbol;
					break;
				case "IPictureLineSymbol":
					IPictureLineSymbol PLS = default(IPictureLineSymbol);
                    PLS = symbol.Outline as IPictureLineSymbol;
					StructStorage.Outline_PictureLine = StorePictureLine(PLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructPictureLineSymbol;
					break;
				case "IMultiLayerLineSymbol":
					IMultiLayerLineSymbol MLLS = default(IMultiLayerLineSymbol);
                    MLLS = symbol.Outline as IMultiLayerLineSymbol;
					StructStorage.Outline_MultiLayerLines = StoreMultilayerLines(MLLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructMultilayerLineSymbol;
					break;
				case "false":
					InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreMarkerFill");
					break;
			}
			return StructStorage;
		}
		
		//************************************************************************************************
		//Die Funktion speichert die LineFillSymbols in seiner Datenstruktur
		//************************************************************************************************
		private StructLineFillSymbol StoreLineFill(ILineFillSymbol symbol)
		{
			StructLineFillSymbol StructStorage = new StructLineFillSymbol();
			StructStorage.Angle = symbol.Angle;
			StructStorage.Color = GimmeStringForColor(symbol.Color);
			StructStorage.Transparency = symbol.Color.Transparency;
			StructStorage.Offset = symbol.Offset;
			StructStorage.Separation = symbol.Separation;
			switch (LineSymbolScan(symbol.LineSymbol))
			{
				case "ICartographicLineSymbol":
					ICartographicLineSymbol CLS = default(ICartographicLineSymbol);
                    CLS = symbol.LineSymbol as ICartographicLineSymbol;
					StructStorage.LineSymbol_CartographicLine = StoreCartographicLine(CLS);
					StructStorage.kindOfLineStruct = LineStructs.StructCartographicLineSymbol;
					break;
				case "IMarkerLineSymbol":
					IMarkerLineSymbol MLS = default(IMarkerLineSymbol);
                    MLS = symbol.LineSymbol as IMarkerLineSymbol;
					StructStorage.LineSymbol_MarkerLine = StoreMarkerLine(MLS);
					StructStorage.kindOfLineStruct = LineStructs.StructMarkerLineSymbol;
					break;
				case "IHashLineSymbol":
					IHashLineSymbol HLS = default(IHashLineSymbol);
                    HLS = symbol.LineSymbol as IHashLineSymbol;
					StructStorage.LineSymbol_HashLine = StoreHashLine(HLS);
					StructStorage.kindOfLineStruct = LineStructs.StructHashLineSymbol;
					break;
				case "ISimpleLineSymbol":
					ISimpleLineSymbol SLS = default(ISimpleLineSymbol);
                    SLS = symbol.LineSymbol as ISimpleLineSymbol;
					StructStorage.LineSymbol_SimpleLine = StoreSimpleLine(SLS);
					StructStorage.kindOfLineStruct = LineStructs.StructSimpleLineSymbol;
					break;
				case "IPictureLineSymbol":
					IPictureLineSymbol PLS = default(IPictureLineSymbol);
                    PLS = symbol.LineSymbol as IPictureLineSymbol;
					StructStorage.LineSymbol_PictureLine = StorePictureLine(PLS);
					StructStorage.kindOfLineStruct = LineStructs.StructPictureLineSymbol;
					break;
				case "IMultiLayerLineSymbol":
					IMultiLayerLineSymbol MLLS = default(IMultiLayerLineSymbol);
                    MLLS = symbol.LineSymbol as IMultiLayerLineSymbol;
					StructStorage.LineSymbol_MultiLayerLines = StoreMultilayerLines(MLLS);
					StructStorage.kindOfLineStruct = LineStructs.StructMultilayerLineSymbol;
					break;
				case "false":
					InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreLineFill");
					break;
			}
			switch (LineSymbolScan(symbol.Outline))
			{
				case "ICartographicLineSymbol":
					ICartographicLineSymbol CLS = default(ICartographicLineSymbol);
                    CLS = symbol.Outline as ICartographicLineSymbol;
					StructStorage.Outline_CartographicLine = StoreCartographicLine(CLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructCartographicLineSymbol;
					break;
				case "IMarkerLineSymbol":
					IMarkerLineSymbol MLS = default(IMarkerLineSymbol);
                    MLS = symbol.Outline as IMarkerLineSymbol;
					StructStorage.Outline_MarkerLine = StoreMarkerLine(MLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructMarkerLineSymbol;
					break;
				case "IHashLineSymbol":
					IHashLineSymbol HLS = default(IHashLineSymbol);
                    HLS = symbol.Outline as IHashLineSymbol;
					StructStorage.Outline_HashLine = StoreHashLine(HLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructHashLineSymbol;
					break;
				case "ISimpleLineSymbol":
					ISimpleLineSymbol SLS = default(ISimpleLineSymbol);
                    SLS = symbol.Outline as ISimpleLineSymbol;
					StructStorage.Outline_SimpleLine = StoreSimpleLine(SLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructSimpleLineSymbol;
					break;
				case "IPictureLineSymbol":
					IPictureLineSymbol PLS = default(IPictureLineSymbol);
                    PLS = symbol.Outline as IPictureLineSymbol;
					StructStorage.Outline_PictureLine = StorePictureLine(PLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructPictureLineSymbol;
					break;
				case "IMultiLayerLineSymbol":
					IMultiLayerLineSymbol MLLS = default(IMultiLayerLineSymbol);
                    MLLS = symbol.Outline as IMultiLayerLineSymbol;
					StructStorage.Outline_MultiLayerLines = StoreMultilayerLines(MLLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructMultilayerLineSymbol;
					break;
				case "false":
					InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreLineFill");
					break;
			}
			return StructStorage;
		}
		
		//************************************************************************************************
		//Die Funktion speichert die DotDensityFillSymbols in seiner Datenstruktur
		//************************************************************************************************
		private StructDotDensityFillSymbol StoreDotDensityFill(IDotDensityFillSymbol symbol)
		{
			StructDotDensityFillSymbol StructStorage = new StructDotDensityFillSymbol();
			ISymbolArray objSymbolArray = default(ISymbolArray);
			StructStorage.SymbolList = new ArrayList();
            objSymbolArray = symbol as ISymbolArray;
			int i = 0;
			StructStorage.BackgroundColor = GimmeStringForColor(symbol.BackgroundColor);
			StructStorage.BackgroundTransparency = symbol.BackgroundColor.Transparency;
			StructStorage.Color = GimmeStringForColor(symbol.Color);
			StructStorage.Transparency = symbol.Color.Transparency;
			StructStorage.FixedPlacement = symbol.FixedPlacement;
			StructStorage.DotSpacing = symbol.DotSpacing;
			StructStorage.SymbolCount = objSymbolArray.SymbolCount;
			
			for (i = 0; i <= objSymbolArray.SymbolCount - 1; i++)
			{
				if (objSymbolArray.Symbol[i] is IMarkerSymbol) //Nur der Marker macht hier als Symbol 黚erhaupt Sinn
				{
					IMarkerSymbol MS = default(IMarkerSymbol);
                    MS = objSymbolArray.Symbol[i] as IMarkerSymbol;
					//!!!ACHTUNG!!! In der ArrayList wird immer abwechselnd ein Symbol und danach die zugeh鰎ige Symbolanzahl
					//abgespeichert. Das ist nicht elegant aber sp鋞er geschickter beim auslesen!
					StructStorage.SymbolList = new ArrayList();
					switch (MarkerSymbolScan(MS))
					{
						case "ISimpleMarkerSymbol":
							ISimpleMarkerSymbol SMS = default(ISimpleMarkerSymbol);
                            SMS = MS as ISimpleMarkerSymbol;
							StructStorage.SymbolList.Add(StoreSimpleMarker(SMS));
							StructStorage.SymbolList.Add(symbol.DotCount[i]);
							break;
						case "ICharacterMarkerSymbol":
							ICharacterMarkerSymbol CMS = default(ICharacterMarkerSymbol);
                            CMS = MS as ICharacterMarkerSymbol;
							StructStorage.SymbolList.Add(StoreCharacterMarker(CMS));
							StructStorage.SymbolList.Add(symbol.DotCount[i]);
							break;
						case "IPictureMarkerSymbol":
							IPictureMarkerSymbol PMS = default(IPictureMarkerSymbol);
                            PMS = MS as IPictureMarkerSymbol;
							StructStorage.SymbolList.Add(StorePictureMarker(PMS));
							StructStorage.SymbolList.Add(symbol.DotCount[i]);
							break;
						case "IArrowMarkerSymbol":
							IArrowMarkerSymbol AMS = default(IArrowMarkerSymbol);
                            AMS = MS as IArrowMarkerSymbol;
							StructStorage.SymbolList.Add(StoreArrowMarker(AMS));
							StructStorage.SymbolList.Add(symbol.DotCount[i]);
							break;
						case "IMultiLayerMarkerSymbol":
							IMultiLayerMarkerSymbol MLMS = default(IMultiLayerMarkerSymbol);
                            MLMS = MS as IMultiLayerMarkerSymbol;
							StructStorage.SymbolList.Add(StoreMultiLayerMarker(MLMS));
							StructStorage.SymbolList.Add(symbol.DotCount[i]);
							break;
						case "false":
							InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreDotDensityFill");
							break;
					}
				}
			}
			
			switch (LineSymbolScan(symbol.Outline))
			{
				case "ICartographicLineSymbol":
					ICartographicLineSymbol CLS = default(ICartographicLineSymbol);
                    CLS = symbol.Outline as ICartographicLineSymbol;
					StructStorage.Outline_CartographicLine = StoreCartographicLine(CLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructCartographicLineSymbol;
					break;
				case "IMarkerLineSymbol":
					IMarkerLineSymbol MLS = default(IMarkerLineSymbol);
                    MLS = symbol.Outline as IMarkerLineSymbol;
					StructStorage.Outline_MarkerLine = StoreMarkerLine(MLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructMarkerLineSymbol;
					break;
				case "IHashLineSymbol":
					IHashLineSymbol HLS = default(IHashLineSymbol);
                    HLS = symbol.Outline as IHashLineSymbol;
					StructStorage.Outline_HashLine = StoreHashLine(HLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructHashLineSymbol;
					break;
				case "ISimpleLineSymbol":
					ISimpleLineSymbol SLS = default(ISimpleLineSymbol);
                    SLS = symbol.Outline as ISimpleLineSymbol;
					StructStorage.Outline_SimpleLine = StoreSimpleLine(SLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructSimpleLineSymbol;
					break;
				case "IPictureLineSymbol":
					IPictureLineSymbol PLS = default(IPictureLineSymbol);
                    PLS = symbol.Outline as IPictureLineSymbol; 
					StructStorage.Outline_PictureLine = StorePictureLine(PLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructPictureLineSymbol;
					break;
				case "IMultiLayerLineSymbol":
					IMultiLayerLineSymbol MLLS = default(IMultiLayerLineSymbol);
                    MLLS = symbol.Outline as IMultiLayerLineSymbol;
					StructStorage.Outline_MultiLayerLines = StoreMultilayerLines(MLLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructMultilayerLineSymbol;
					break;
				case "false":
					InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreLineFill");
					break;
			}
			
			return StructStorage;
		}
		
		//************************************************************************************************
		//Die Funktion speichert die PictureFillSymbols in seiner Datenstruktur
		//************************************************************************************************
		private StructPictureFillSymbol StorePictureFill(IPictureFillSymbol symbol)
		{
			StructPictureFillSymbol StructStorage = new StructPictureFillSymbol();
			StructStorage.Angle = symbol.Angle;
			StructStorage.BackgroundColor = this.GimmeStringForColor(symbol.BackgroundColor);
			StructStorage.BackgroundTransparency = symbol.BackgroundColor.Transparency;
			StructStorage.Color = this.GimmeStringForColor(symbol.Color);
			StructStorage.Transparency = symbol.Color.Transparency;
			//objPicture = Microsoft.VisualBasic.Compatibility.VB6.IPictureDispToImage(symbol.Picture) 'doesn't work with esri
			//StructStorage.Picture = symbol.Picture.
			StructStorage.XScale = symbol.XScale;
			StructStorage.YScale = symbol.YScale;
			switch (LineSymbolScan(symbol.Outline))
			{
				case "ICartographicLineSymbol":
					ICartographicLineSymbol CLS = default(ICartographicLineSymbol);
                    CLS = symbol.Outline as ICartographicLineSymbol;
					StructStorage.Outline_CartographicLine = StoreCartographicLine(CLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructCartographicLineSymbol;
					break;
				case "IMarkerLineSymbol":
					IMarkerLineSymbol MLS = default(IMarkerLineSymbol);
                    MLS = symbol.Outline as IMarkerLineSymbol;
					StructStorage.Outline_MarkerLine = StoreMarkerLine(MLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructMarkerLineSymbol;
					break;
				case "IHashLineSymbol":
					IHashLineSymbol HLS = default(IHashLineSymbol);
                    HLS = symbol.Outline as IHashLineSymbol;
					StructStorage.Outline_HashLine = StoreHashLine(HLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructHashLineSymbol;
					break;
				case "ISimpleLineSymbol":
					ISimpleLineSymbol SLS = default(ISimpleLineSymbol);
                    SLS = symbol.Outline as ISimpleLineSymbol;
					StructStorage.Outline_SimpleLine = StoreSimpleLine(SLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructSimpleLineSymbol;
					break;
				case "IPictureLineSymbol":
					IPictureLineSymbol PLS = default(IPictureLineSymbol);
                    PLS = symbol.Outline as IPictureLineSymbol;
					StructStorage.Outline_PictureLine = StorePictureLine(PLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructPictureLineSymbol;
					break;
				case "IMultiLayerLineSymbol":
					IMultiLayerLineSymbol MLLS = default(IMultiLayerLineSymbol);
                    MLLS = symbol.Outline as IMultiLayerLineSymbol;
					StructStorage.Outline_MultiLayerLines = StoreMultilayerLines(MLLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructMultilayerLineSymbol;
					break;
				case "false":
					InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StorePictureFill");
					break;
			}
			return StructStorage;
		}
		
		//************************************************************************************************
		//Die Funktion speichert die GradientFillSymbols in seiner Datenstruktur
		//************************************************************************************************
		private StructGradientFillSymbol StoreGradientFill(IGradientFillSymbol symbol)
		{
			StructGradientFillSymbol StructStorage = new StructGradientFillSymbol();
			StructStorage.Color = GimmeStringForColor(symbol.Color);
			StructStorage.Transparency = symbol.Color.Transparency;
			StructStorage.Colors = GimmeArrayListForColorRamp(symbol.ColorRamp);
			StructStorage.GradientAngle = symbol.GradientAngle;
			StructStorage.GradientPercentage = symbol.GradientPercentage;
			StructStorage.IntervallCount = symbol.IntervalCount;
			switch (LineSymbolScan(symbol.Outline))
			{
				case "ICartographicLineSymbol":
					ICartographicLineSymbol CLS = default(ICartographicLineSymbol);
                    CLS = symbol.Outline as ICartographicLineSymbol;
					StructStorage.Outline_CartographicLine = StoreCartographicLine(CLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructCartographicLineSymbol;
					break;
				case "IMarkerLineSymbol":
					IMarkerLineSymbol MLS = default(IMarkerLineSymbol);
                    MLS = symbol.Outline as IMarkerLineSymbol;
					StructStorage.Outline_MarkerLine = StoreMarkerLine(MLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructMarkerLineSymbol;
					break;
				case "IHashLineSymbol":
					IHashLineSymbol HLS = default(IHashLineSymbol);
                    HLS = symbol.Outline as IHashLineSymbol;
					StructStorage.Outline_HashLine = StoreHashLine(HLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructHashLineSymbol;
					break;
				case "ISimpleLineSymbol":
					ISimpleLineSymbol SLS = default(ISimpleLineSymbol);
                    SLS = symbol.Outline as ISimpleLineSymbol;
					StructStorage.Outline_SimpleLine = StoreSimpleLine(SLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructSimpleLineSymbol;
					break;
				case "IPictureLineSymbol":
					IPictureLineSymbol PLS = default(IPictureLineSymbol);
                    PLS = symbol.Outline as IPictureLineSymbol;
					StructStorage.Outline_PictureLine = StorePictureLine(PLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructPictureLineSymbol;
					break;
				case "IMultiLayerLineSymbol":
					IMultiLayerLineSymbol MLLS = default(IMultiLayerLineSymbol);
                    MLLS = symbol.Outline as IMultiLayerLineSymbol;
					StructStorage.Outline_MultiLayerLines = StoreMultilayerLines(MLLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructMultilayerLineSymbol;
					break;
				case "false":
					InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StorePictureFill");
					break;
			}
			return StructStorage;
		}
		
		//************************************************************************************************
		//Die Funktion speichert die MultiLayerFillSymbols in seiner Datenstruktur
		//************************************************************************************************
		private StructMultilayerFillSymbol StoreMultiLayerFill(IMultiLayerFillSymbol symbol)
		{
			StructMultilayerFillSymbol StructStorage = new StructMultilayerFillSymbol();
			StructStorage.LayerCount = symbol.LayerCount;
			StructStorage.MultiFillLayers = new ArrayList();
			int i = 0;
			for (i = 0; i <= symbol.LayerCount - 1; i++)
			{
				switch (FillSymbolScan(symbol.Layer[i]))
				{
					case "ISimpleFillSymbol":
						ISimpleFillSymbol SFS = default(ISimpleFillSymbol);
                        SFS = symbol.get_Layer(i) as ISimpleFillSymbol;
						StructStorage.MultiFillLayers.Add(StoreSimpleFill(SFS));
						break;
					case "IMarkerFillSymbol":
						IMarkerFillSymbol MFS = default(IMarkerFillSymbol);
                        MFS = symbol.get_Layer(i) as IMarkerFillSymbol;
						StructStorage.MultiFillLayers.Add(StoreMarkerFill(MFS));
						break;
					case "ILineFillSymbol":
						ILineFillSymbol LFS = default(ILineFillSymbol);
                        LFS = symbol.get_Layer(i) as ILineFillSymbol;
						StructStorage.MultiFillLayers.Add(StoreLineFill(LFS));
						break;
					case "IPictureFillSymbol":
						IPictureFillSymbol PFS = default(IPictureFillSymbol);
                        PFS = symbol.get_Layer(i) as IPictureFillSymbol;
						StructStorage.MultiFillLayers.Add(StorePictureFill(PFS));
						break;
					case "IDotDensityFillSymbol":
						IDotDensityFillSymbol DFS = default(IDotDensityFillSymbol);
						DFS = symbol.get_Layer(i) as IDotDensityFillSymbol;
						StructStorage.MultiFillLayers.Add(StoreDotDensityFill(DFS));
						break;
					case "IGradientFillSymbol":
						IGradientFillSymbol GFS = default(IGradientFillSymbol);
                        GFS = symbol.get_Layer(i) as IGradientFillSymbol;
						StructStorage.MultiFillLayers.Add(StoreGradientFill(GFS));
						break;
					case "IMultiLayerFillSymbol":
						IMultiLayerFillSymbol MLFS = default(IMultiLayerFillSymbol);
						MLFS = symbol;
						StructStorage.MultiFillLayers.Add(StoreMultiLayerFill(MLFS)); //Hier ist ein rekursiver Aufruf
						break;
					case "false":
						InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreMultilayerFill");
						break;
				}
			}
			return StructStorage;
		}
		
		//_______________________________________________________________________________________________________________________
		
		//************************************************************************************************
		//Die Funktion speichert die BarChartSymbols in seiner Datenstruktur
		//************************************************************************************************
		private StructBarChartSymbol StoreBarChart(IBarChartSymbol symbol)
		{
			StructBarChartSymbol StructStorage = new StructBarChartSymbol();
			StructStorage.ShowAxes = symbol.ShowAxes;
			StructStorage.Spacing = symbol.Spacing;
			StructStorage.VerticalBars = symbol.VerticalBars;
			StructStorage.Width = symbol.Width;
			switch (LineSymbolScan(symbol.Axes))
			{
				case "ICartographicLineSymbol":
					ICartographicLineSymbol CLS = default(ICartographicLineSymbol);
                    CLS = symbol.Axes as ICartographicLineSymbol;
					StructStorage.Axes_CartographicLine = StoreCartographicLine(CLS);
					StructStorage.kindOfAxeslineStruct = LineStructs.StructCartographicLineSymbol;
					break;
				case "IMarkerLineSymbol":
					IMarkerLineSymbol MLS = default(IMarkerLineSymbol);
                    MLS = symbol.Axes as IMarkerLineSymbol;
					StructStorage.Axes_MarkerLine = StoreMarkerLine(MLS);
					StructStorage.kindOfAxeslineStruct = LineStructs.StructMarkerLineSymbol;
					break;
				case "IHashLineSymbol":
					IHashLineSymbol HLS = default(IHashLineSymbol);
                    HLS = symbol.Axes as IHashLineSymbol;
					StructStorage.Axes_HashLine = StoreHashLine(HLS);
					StructStorage.kindOfAxeslineStruct = LineStructs.StructHashLineSymbol;
					break;
				case "ISimpleLineSymbol":
					ISimpleLineSymbol SLS = default(ISimpleLineSymbol);
                    SLS = symbol.Axes as ISimpleLineSymbol;
					StructStorage.Axes_SimpleLine = StoreSimpleLine(SLS);
					StructStorage.kindOfAxeslineStruct = LineStructs.StructSimpleLineSymbol;
					break;
				case "IPictureLineSymbol":
					IPictureLineSymbol PLS = default(IPictureLineSymbol);
                    PLS = symbol.Axes as IPictureLineSymbol;
					StructStorage.Axes_PictureLine = StorePictureLine(PLS);
					StructStorage.kindOfAxeslineStruct = LineStructs.StructPictureLineSymbol;
					break;
				case "IMultiLayerLineSymbol":
					IMultiLayerLineSymbol MLLS = default(IMultiLayerLineSymbol);
                    MLLS = symbol.Axes as IMultiLayerLineSymbol;
					StructStorage.Axes_MultiLayerLines = StoreMultilayerLines(MLLS);
					StructStorage.kindOfAxeslineStruct = LineStructs.StructMultilayerLineSymbol;
					break;
				case "false":
					InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StoreBarChart");
					break;
			}
			return StructStorage;
		}
		
		//************************************************************************************************
		//Die Funktion speichert die PieChartSymbols in seiner Datenstruktur
		//************************************************************************************************
		private StructPieChartSymbol StorePieChart(IPieChartSymbol symbol)
		{
			StructPieChartSymbol StructStorage = new StructPieChartSymbol();
			StructStorage.Clockwise = symbol.Clockwise;
			StructStorage.UseOutline = symbol.UseOutline;
			switch (LineSymbolScan(symbol.Outline))
			{
				case "ICartographicLineSymbol":
					ICartographicLineSymbol CLS = default(ICartographicLineSymbol);
                    CLS = symbol.Outline as ICartographicLineSymbol;
					StructStorage.Outline_CartographicLine = StoreCartographicLine(CLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructCartographicLineSymbol;
					break;
				case "IMarkerLineSymbol":
					IMarkerLineSymbol MLS = default(IMarkerLineSymbol);
                    MLS = symbol.Outline as IMarkerLineSymbol;
					StructStorage.Outline_MarkerLine = StoreMarkerLine(MLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructMarkerLineSymbol;
					break;
				case "IHashLineSymbol":
					IHashLineSymbol HLS = default(IHashLineSymbol);
                    HLS = symbol.Outline as IHashLineSymbol;
					StructStorage.Outline_HashLine = StoreHashLine(HLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructHashLineSymbol;
					break;
				case "ISimpleLineSymbol":
					ISimpleLineSymbol SLS = default(ISimpleLineSymbol);
                    SLS = symbol.Outline as ISimpleLineSymbol;
					StructStorage.Outline_SimpleLine = StoreSimpleLine(SLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructSimpleLineSymbol;
					break;
				case "IPictureLineSymbol":
					IPictureLineSymbol PLS = default(IPictureLineSymbol);
                    PLS = symbol.Outline as IPictureLineSymbol;
					StructStorage.Outline_PictureLine = StorePictureLine(PLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructPictureLineSymbol;
					break;
				case "IMultiLayerLineSymbol":
					IMultiLayerLineSymbol MLLS = default(IMultiLayerLineSymbol);
                    MLLS = symbol.Outline as IMultiLayerLineSymbol;
					StructStorage.Outline_MultiLayerLines = StoreMultilayerLines(MLLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructMultilayerLineSymbol;
					break;
				case "false":
					InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StorePictureFill");
					break;
			}
			return StructStorage;
		}
		
		//************************************************************************************************
		//Die Funktion speichert die StackedChartSymbols in seiner Datenstruktur
		//************************************************************************************************
		private StructStackedChartSymbol StoreStackedChart(IStackedChartSymbol symbol)
		{
			StructStackedChartSymbol StructStorage = new StructStackedChartSymbol();
			StructStorage.Fixed = symbol.Fixed;
			StructStorage.UseOutline = symbol.UseOutline;
			StructStorage.VerticalBar = symbol.VerticalBar;
			StructStorage.Width = symbol.Width;
			switch (LineSymbolScan(symbol.Outline))
			{
				case "ICartographicLineSymbol":
					ICartographicLineSymbol CLS = default(ICartographicLineSymbol);
                    CLS = symbol.Outline as ICartographicLineSymbol;
					StructStorage.Outline_CartographicLine = StoreCartographicLine(CLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructCartographicLineSymbol;
					break;
				case "IMarkerLineSymbol":
					IMarkerLineSymbol MLS = default(IMarkerLineSymbol);
                    MLS = symbol.Outline as IMarkerLineSymbol;
					StructStorage.Outline_MarkerLine = StoreMarkerLine(MLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructMarkerLineSymbol;
					break;
				case "IHashLineSymbol":
					IHashLineSymbol HLS = default(IHashLineSymbol);
                    HLS = symbol.Outline as IHashLineSymbol;
					StructStorage.Outline_HashLine = StoreHashLine(HLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructHashLineSymbol;
					break;
				case "ISimpleLineSymbol":
					ISimpleLineSymbol SLS = default(ISimpleLineSymbol);
                    SLS = symbol.Outline as ISimpleLineSymbol;
					StructStorage.Outline_SimpleLine = StoreSimpleLine(SLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructSimpleLineSymbol;
					break;
				case "IPictureLineSymbol":
					IPictureLineSymbol PLS = default(IPictureLineSymbol);
                    PLS = symbol.Outline as IPictureLineSymbol;
					StructStorage.Outline_PictureLine = StorePictureLine(PLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructPictureLineSymbol;
					break;
				case "IMultiLayerLineSymbol":
					IMultiLayerLineSymbol MLLS = default(IMultiLayerLineSymbol);
                    MLLS = symbol.Outline as IMultiLayerLineSymbol;
					StructStorage.Outline_MultiLayerLines = StoreMultilayerLines(MLLS);
					StructStorage.kindOfOutlineStruct = LineStructs.StructMultilayerLineSymbol;
					break;
				case "false":
					InfoMsg("Seit Erstellen der Programmversion ist eine neue Symbolvariante zu den esri-Symbolen hinzugekommen", "StorePictureFill");
					break;
			}
			return StructStorage;
		}
		
		//_______________________________________________________________________________________________________________________
		
		//************************************************************************************************
		//Die Funktion speichert TextSymbol in seiner Datenstruktur
		//************************************************************************************************
		private StructTextSymbol StoreText(ITextSymbol symbol)
		{
			StructTextSymbol StructStorage = new StructTextSymbol();
			StructStorage.Angle = symbol.Angle;
			StructStorage.Color = GimmeStringForColor(symbol.Color);
			StructStorage.Font = symbol.Font.Name;
			StructStorage.Style = "normal";
			if (symbol.Font.Italic)
			{
				StructStorage.Style = "italic";
			}
			StructStorage.Weight = "normal";
			if (symbol.Font.Bold)
			{
				StructStorage.Weight = "bold";
			}
			StructStorage.HorizontalAlignment = symbol.HorizontalAlignment.ToString();
			StructStorage.RightToLeft = symbol.RightToLeft;
			StructStorage.Size = symbol.Size;
			StructStorage.Text = symbol.Text;
			StructStorage.VerticalAlignment = symbol.VerticalAlignment.ToString();
			return StructStorage;
		}
		
#endregion //Speicherungen auf Symbolebene und auf Rendererebene
		
		//************************************************************************************************
		//Die Funktion steuert die Prozesse zentral. Sie ist somit die erste Funktion in dieser Klasse
		//Wird von public Sub New() aufgerufen
		//************************************************************************************************
		//ARIS: Compacted the code blocks for english and german to 1 block
		//************************************************************************************************
		private bool CentralProcessingFunc()
		{
			if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
			{
				frmMotherform.CHLabelTop("Die Analyse des ArcMap-Projekts l鋟ft");
			}
			else if (frmMotherform.m_enumLang == Motherform.Language.English)
			{
				frmMotherform.CHLabelTop("Analysis of the ArcMap-Project is running");
			}
			
			bool blnAnswer = default(bool);
			Output_SLD objOutputSLD;
			
			if (GetProcesses() == false)
			{
				MyTermination();
				return false;
			}
			if (GetApplication() == false)
			{
				MyTermination();
				return false;
			}
			if (GetMap() == false)
			{
				MyTermination();
				return false;
			}
			if (AnalyseLayerSymbology() == false)
			{
				MyTermination();
				return false;
			}
			
			//Sicherheitsabfrage, falls noch kein Filename f黵 das SLD angegeben wurde. Dann Aufruf der Ausgabeklasse
			if (m_cFilename == null || m_cFilename == "")
			{
				if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
				{
					frmMotherform.CHLabelTop("Die Analyse des ArcMap-Projekts ist beendet");
				}
				else if (frmMotherform.m_enumLang == Motherform.Language.English)
				{
					frmMotherform.CHLabelTop("Analysis of the ArcMap-Project has finished");
				}
				if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
				{
					blnAnswer = MessageBox.Show("Sie haben noch keinen Dateinamen/Speicherort angegeben. Wenn Sie jetzt keinen Dateinamen angeben" + 
						",wird die Anwendung beendet." + "\r\n" + "Wollen Sie jetzt einen Dateinamen angeben?", "ArcGIS_SLD_Converter | " + 
						"Analize_ArcMap_Symbols | CentralProcessingFunc", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes;
				}
				else if (frmMotherform.m_enumLang == Motherform.Language.English)
				{
					blnAnswer = MessageBox.Show("You haven\'t specified an SLD store location until now. If you don\'t specify a location" + 
						",the application will be terminated." + "\r\n" + "Do you want to specify a location now?", "ArcGIS_SLD_Converter | " + 
						"Analize_ArcMap_Symbols | CentralProcessingFunc", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes;
				}
				if (blnAnswer)
				{
					if (File.Exists(frmMotherform.GetSLDFileFromConfigXML))
					{
						frmMotherform.dlgSave.InitialDirectory = frmMotherform.GetSLDFileFromConfigXML;
					}
					if (frmMotherform.dlgSave.ShowDialog() == DialogResult.OK)
					{
						frmMotherform.dlgSave.CheckFileExists = false;
						frmMotherform.dlgSave.CheckPathExists = true;
						frmMotherform.dlgSave.DefaultExt = "sld";
						frmMotherform.dlgSave.Filter = "SLD-files (*.sld)|*.sld";
						frmMotherform.dlgSave.AddExtension = true;
						frmMotherform.dlgSave.InitialDirectory = System.IO.Path.GetDirectoryName(m_cFilename);
						frmMotherform.dlgSave.OverwritePrompt = true;
						frmMotherform.dlgSave.CreatePrompt = false;
						if (frmMotherform.dlgSave.ShowDialog() == DialogResult.OK)
						{
							m_cFilename = frmMotherform.dlgSave.FileName;
							frmMotherform.txtFileName.Text = m_cFilename;
						}
						objOutputSLD = new Output_SLD(frmMotherform, this, m_cFilename); //Aufruf der Ausgabeklasse
					}
					else
					{
						MyTermination();
					}
				}
				else
				{
					MyTermination();
				}
			}
			else
			{
				objOutputSLD = new Output_SLD(frmMotherform, this, m_cFilename); //Aufruf der Ausgabeklasse
			}
			frmMotherform.CHLabelBottom("");
			frmMotherform.CHLabelSmall("");
			frmMotherform.ReadBackValues(); //Liest die Benutzerdefinierten Einstellungen in die XML-Datei zur點k
			return default(bool);
		}
		
#region Zentrale Verwaltungsfunktionen
		//************************************************************************************************
		//Hier werden alle laufenden Prozesse auf dem System durchgesehen, um zu sehen, ob ArcMap
		//gestartet ist.
		//************************************************************************************************
		private bool GetProcesses()
		{
			Process objArcGISProcess = new Process();
			
			bool bSwitch = false; //der Schalter ist notwendig, weil eine ganze Anzahl Prozesse durchlaufen wird
			
			if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
			{
				frmMotherform.CHLabelBottom("Suche ArcMap-Prozess");
			}
			else if (frmMotherform.m_enumLang == Motherform.Language.English)
			{
				frmMotherform.CHLabelTop("Searching an ArcMap process");
			}
			try
			{
				foreach (Process objProcess in Process.GetProcesses())
				{
					if (objProcess.ProcessName == "ArcMap")
					{
						bSwitch = true;
					}
				}
				
				if (bSwitch == true)
				{
					return true;
				}
				else
				{
					if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
					{
						MessageBox.Show("Sie m黶sen erst ArcMap 鰂fnen!");
					}
					else if (frmMotherform.m_enumLang == Motherform.Language.English)
					{
						frmMotherform.CHLabelTop("You must open ArcMap first");
					}
					return false;
				}
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Durchsehen der laufenden Prozesse auf dem System", ex.Message, ex.StackTrace, "GetProcesses");
				return false;
			}
		}
		
		
		//************************************************************************************************
		//Der erste zentrale Punkt in der Anwendung: hier wird die Referenz auf die laufende ArcMap-Instanz
		//geholt (f黵 exe-Anwendung sehr wichtig!!!)
		//************************************************************************************************
		private bool GetApplication()
		{
			long Zahl = 0;
			m_ObjApp = null;
			
			if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
			{
				frmMotherform.CHLabelBottom("Hole Verweis auf laufende ArcMap-Sitzung");
			}
			else if (frmMotherform.m_enumLang == Motherform.Language.English)
			{
				frmMotherform.CHLabelTop("get reference on running ArcMap-session");
			}
			
			try
			{
				m_ObjAppROT = new AppROT();
				Zahl = m_ObjAppROT.Count;
				
				if (Zahl > 1)
				{
					if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
					{
						MessageBox.Show("Sie haben mehrere ArcMap Sessions gleichzeitig ge鰂fnet." + 
							"Bitte schlie遝n Sie alle ArcMap-Anwendungen bis auf jene, von der Sie das" + 
							" SLD-Dokument generieren m鯿hten und starten Sie die Anwendung erneut!", "Bitte Beachten!", MessageBoxButtons.OK, MessageBoxIcon.Information);
						return false;
					}
					else if (frmMotherform.m_enumLang == Motherform.Language.English)
					{
						MessageBox.Show("You started several ArcMap-sessions at one time." + 
							"Please close all sessions except of that, you want to analyse and" + 
							" start the application again!", "Please notice!", MessageBoxButtons.OK, MessageBoxIcon.Information);
						return false;
					}
					
				}
				else
				{
					if (m_ObjAppROT.Item[0] is IMxApplication) //躡erpr黤ung, ob das richtige Objekt erhalten wurde
					{
						m_ObjApp = m_ObjAppROT.Item[0];
						m_ObjDoc = m_ObjApp.Document as IMxDocument;
                        m_ObjObjectCreator = m_ObjApp as IObjectFactory;
						return true;
					}
				}
                return false;
				
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Referenzieren auf die ArcMap-Instanz ", ex.Message, ex.StackTrace, "GetApplication");
				return false;
			}
		}
		
		
		//************************************************************************************************
		//Das Kartenobjekt: Hier wird auf das Kartenobjekt referenziert. Wenn es mehrere Karten gibt, erfolg
		//eine Abfrage, ob die aktive Karte die Karte ist, die umgewandelt werden soll
		//************************************************************************************************
		private bool GetMap()
		{
			
			if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
			{
				frmMotherform.CHLabelBottom("Verweis auf das aktuelle Kartenfenster");
			}
			else if (frmMotherform.m_enumLang == Motherform.Language.English)
			{
				frmMotherform.CHLabelTop("Reference on the current Session");
			}
			
			try
			{
				if (m_ObjDoc.Maps.Count> 1) //Wenn mehr es mehr als eine Karte in dem Dokument gibt
				{
					if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
					{
						if (MessageBox.Show("Ist die von Ihnen zum Umwandeln in SLD gew黱schte Karte gerade die aktive Karte? " + 
							"Wenn ja, dr點ken Sie \'Ja\' wenn nicht, dr點ken Sie \'Nein\', w鋒len in ArcMap die richtige Karte aus, " + 
							"und bet鋞igen den Befehl noch einmal", "Auswahl der gew黱schten Karte", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
						{
							m_ObjMap = m_ObjDoc.FocusMap;
							return true;
						}
						else
						{
							return false;
						}
					}
					else if (frmMotherform.m_enumLang == Motherform.Language.English)
					{
						if (MessageBox.Show("Is that current map this one you want to turn into SLD? " + 
							"if yes push \'yes\' if no push \'no\' and choose the right map in ArcMap " + 
							"and use that procedure again", "Choice of the right map", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
						{
							m_ObjMap = m_ObjDoc.FocusMap;
							return true;
						}
						else
						{
							return false;
						}
					}
				}
				else
				{
					m_ObjMap = m_ObjDoc.FocusMap;
					return true;
				}
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Erhalten des aktuellen Kartenobjekts ", ex.Message, ex.StackTrace, "GetMap");
				return false;
			}
			return default(bool);
		}
		
		//************************************************************************************************
		//Hier ist der zweite zentrale Punkt der Anwendung zu finden:
		//Die Layer der aktiven Karte werden durchlaufen und alle Layerobjekte in der Collection zusammen-
		// gefasst. Au遝rdem: Collection von jedem Rendererobjekt jedes FeatureLayers.
		//Haupts鋍hlich: die Symbolwerte der einzelnen Layer werden durchlaufen und die Einzelsymbole in
		//einer Datenstruktur gespeichert
		//************************************************************************************************
		private bool AnalyseLayerSymbology()
		{
			ILayer objLayer = default(ILayer); //Die Schnittstelle zum aktuellen Layer
			int iNumberLayers = 0; //Die Anzahl aller Layer
			string cLayerName = ""; //Der Name des aktuellen Layers
			ISymbol objFstOrderSymbol; //Das ISymbol des entsprechenden Renderers
			m_StrProject = new StructProject(); //Projektstruct wird hier initialisiert
			iNumberLayers = m_ObjMap.LayerCount;
			m_StrProject.LayerList = new ArrayList();
			
			try
			{
				int i = 0;
				//Steps through all layers of the first level
				for (i = 0; i <= iNumberLayers - 1; i++)
				{
					objLayer = m_ObjMap.Layer[i];
					cLayerName = objLayer.Name;
					if (frmMotherform.m_bAllLayers == false && objLayer.Visible == false)
					{
						if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
						{
							frmMotherform.CHLabelBottom("Layer " + cLayerName + " wird wird 黚ersprungen, weil sie nicht sichtbar ist.");
						}
						else if (frmMotherform.m_enumLang == Motherform.Language.English)
						{
							frmMotherform.CHLabelBottom("Layer " + cLayerName + " is skipped, because it is not visible");
						}
					}
					else
					{
						if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
						{
							frmMotherform.CHLabelBottom("Layer " + cLayerName + " wird gerade analysiert");
						}
						else if (frmMotherform.m_enumLang == Motherform.Language.English)
						{
							frmMotherform.CHLabelBottom("Layer " + cLayerName + " is beeing analysed");
						}
						SpreadLayerStructure(objLayer);
					}
					frmMotherform.CHLabelSmall("");
				}
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler bei der Analyse des esri-Projekts", ex.Message, ex.StackTrace, "AnalyseLayerSymbology");
				if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
				{
					ErrorMsg("Fehler bei der Analyse des esri-Projekts", ex.Message, ex.StackTrace, "AnalyseLayerSymbology");
				}
				else if (frmMotherform.m_enumLang == Motherform.Language.English)
				{
					ErrorMsg("Exception in ArcMap project analysis", ex.Message, ex.StackTrace, "AnalyseLayerSymbology");
				}
				return false;
			}
		}
		
		//************************************************************************************************
		//Analizes the Layer structure of the passed first Level-Layer. If the passed Layer is a GeoFeature
		//Layer (or an object beyond that) it will be analized and stored in the Layerstructs and the project
		//struct. If the passed Layer is a group layer, it will be ignored
		//************************************************************************************************
		private void SpreadLayerStructure(ILayer objLayer)
		{
			
			try
			{
				//recursive call if the current layer is a group layer. Solution is that Group Layers will be ignored
				if (objLayer is IGroupLayer)
				{
					int j = 0;
					IGroupLayer objGRL = default(IGroupLayer);
					ICompositeLayer objCompLayer = default(ICompositeLayer);
                    objGRL = objLayer as IGroupLayer;
                    objCompLayer = objGRL as ICompositeLayer;
					for (j = 0; j <= objCompLayer.Count- 1; j++)
					{
						//'Because of this if clause group Layer will be ignored
						//If TypeOf objCompLayer.Layer(j) Is IFeatureLayer Then
						
						//End If
						SpreadLayerStructure(objCompLayer.Layer[j]); //recursive call
						
					}
				}
				else if (objLayer is IFeatureLayer)
				{
					if (objLayer is IGeoFeatureLayer)
					{
						IGeoFeatureLayer objGFL = default(IGeoFeatureLayer);
                        objGFL = objLayer as IGeoFeatureLayer;
						//Hier die Unterscheidung der Renderertypen
						if (objGFL.Renderer is IUniqueValueRenderer)
						{
							IUniqueValueRenderer objRenderer = default(IUniqueValueRenderer);
                            objRenderer = objGFL.Renderer as IUniqueValueRenderer;
							m_StrProject.LayerList.Add(StoreStructUVRenderer(objRenderer, objLayer as IFeatureLayer));
							AddOneToLayerNumber();
						}
						if (objGFL.Renderer is ISimpleRenderer)
						{
							ISimpleRenderer objRenderer = default(ISimpleRenderer);
                            objRenderer = objGFL.Renderer as ISimpleRenderer;
                            m_StrProject.LayerList.Add(StoreStructSimpleRenderer(objRenderer, objLayer as IFeatureLayer));
							AddOneToLayerNumber();
						}
						if (objGFL.Renderer is IClassBreaksRenderer)
						{
							IClassBreaksRenderer objRenderer = default(IClassBreaksRenderer);
                            objRenderer = objGFL.Renderer as IClassBreaksRenderer;
							m_StrProject.LayerList.Add(StoreStructCBRenderer(objRenderer, objLayer as IFeatureLayer));
							AddOneToLayerNumber();
						}
						//andere Renderer werden evtl. sp鋞er abgedeckt
						//If TypeOf objGFL.Renderer Is IChartRenderer Then
						//    Dim objRenderer As IChartRenderer
						//    objRenderer = objGFL.Renderer
						//    objFstOrderSymbol = objRenderer.Symbol
						//    'TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO
						//End If
						//If TypeOf objGFL.Renderer Is IDotDensityRenderer Then
						//    Dim objRenderer As IDotDensityRenderer
						//    objRenderer = objGFL.Renderer
						//    'TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO
						//End If
						//If TypeOf objGFL.Renderer Is IProportionalSymbolRenderer Then
						//    Dim objRenderer As IProportionalSymbolRenderer
						//    objRenderer = objGFL.Renderer
						//    'TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO
						//End If
						//If TypeOf objGFL.Renderer Is IScaleDependentRenderer Then
						//    Dim objRenderer As IScaleDependentRenderer
						//    objRenderer = objGFL.Renderer
						//    'TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO
						//End If
					}
					
				}
				else
				{
					if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
					{
						InfoMsg("Die Layerart ihres ArcMap-Projekts wird derzeit noch nicht unterst黷zt.", "SpreadLayerStructure");
					}
					else if (frmMotherform.m_enumLang == Motherform.Language.English)
					{
						InfoMsg("The kind of Layer you use in your ArcMap project is currently not beeing supported.", "SpreadLayerStructure");
					}
					MyTermination();
				}
			}
			catch (Exception)
			{
				if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
				{
					InfoMsg("Unerwarteter Fehler beim Speichern in den Layerstrukturen", "SpreadLayerStructure");
				}
				else if (frmMotherform.m_enumLang == Motherform.Language.English)
				{
					InfoMsg("Unexpected Error during storing in layerstructs", "SpreadLayerStructure");
				}
			}
		}
		
		
		
		//'************************************************************************************************
		//'Hier ist der zweite zentrale Punkt der Anwendung zu finden:
		//'Die Layer der aktiven Karte werden durchlaufen und alle Layerobjekte in der Collection zusammen-
		//' gefasst. Au遝rdem: Collection von jedem Rendererobjekt jedes FeatureLayers.
		//'Haupts鋍hlich: die Symbolwerte der einzelnen Layer werden durchlaufen und die Einzelsymbole in
		//'einer Datenstruktur gespeichert
		//'************************************************************************************************
		//Private Function AnalyseLayerSymbology() As Boolean
		//    Dim objLayer As ILayer  'Die Schnittstelle zum aktuellen Layer
		//    Dim iNumberLayers As Integer    'Die Anzahl aller Layer
		//    Dim cLayerName As String     'Der Name des aktuellen Layers
		//    Dim objFstOrderSymbol As ISymbol    'Das ISymbol des entsprechenden Renderers
		//    m_StrProject = New StructProject    'Projektstruct wird hier initialisiert
		//    iNumberLayers = m_ObjMap.LayerCount()
		//    m_StrProject.LayerCount = iNumberLayers 'Anzahl der Layer in Projektstruct gespeichert
		//    m_StrProject.LayerList = New ArrayList
		
		//    Try
		//        Dim i As Integer
		//        For i = 0 To iNumberLayers - 1
		//            objLayer = m_ObjMap.Layer(i)
		//            cLayerName = objLayer.Name
		//            If frmMotherform.m_enumLang = Motherform.Language.Deutsch Then
		//                frmMotherform.CHLabelBottom("Layer " & cLayerName & " wird gerade analysiert")
		//            ElseIf frmMotherform.m_enumLang = Motherform.Language.English Then
		//                frmMotherform.CHLabelBottom("Layer " & cLayerName & " is beeing analysed")
		//            End If
		
		//            'Hier die erste Unterscheidung der Layer: ob IFeatureLayer (sind wohl die meisten Layer)
		//            'evtl testen, ob es noch weitere relevante 黚ergeordnete Layertypen gibt!!
		//            If TypeOf objLayer Is IFeatureLayer Then
		//                'Hier die zweite Unterscheidung der Layer: ob IGeoFeatureLayer (hier kann es andere
		//                'M鰃lichkeiten geben - z.B. Layer mit Text...) untergeordnete Layertypen
		
		
		
		//                If TypeOf objLayer Is IGeoFeatureLayer Then
		//                    Dim objGFL As IGeoFeatureLayer
		//                    objGFL = objLayer
		//                    'Hier die Unterscheidung der Renderertypen
		//                    If TypeOf objGFL.Renderer Is IUniqueValueRenderer Then
		//                        Dim objRenderer As IUniqueValueRenderer
		//                        objRenderer = objGFL.Renderer
		//                        m_StrProject.LayerList.Add(StoreStructUVRenderer(objRenderer, objLayer))
		//                    End If
		//                    If TypeOf objGFL.Renderer Is ISimpleRenderer Then
		//                        Dim objRenderer As ISimpleRenderer
		//                        objRenderer = objGFL.Renderer
		//                        m_StrProject.LayerList.Add(StoreStructSimpleRenderer(objRenderer, objLayer))
		//                    End If
		//                    If TypeOf objGFL.Renderer Is IClassBreaksRenderer Then
		//                        Dim objRenderer As IClassBreaksRenderer
		//                        objRenderer = objGFL.Renderer
		//                        m_StrProject.LayerList.Add(StoreStructCBRenderer(objRenderer, objLayer))
		//                    End If
		//                    'andere Renderer werden evtl. sp鋞er abgedeckt
		//                    'If TypeOf objGFL.Renderer Is IChartRenderer Then
		//                    '    Dim objRenderer As IChartRenderer
		//                    '    objRenderer = objGFL.Renderer
		//                    '    objFstOrderSymbol = objRenderer.Symbol
		//                    '    'TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO
		//                    'End If
		//                    'If TypeOf objGFL.Renderer Is IDotDensityRenderer Then
		//                    '    Dim objRenderer As IDotDensityRenderer
		//                    '    objRenderer = objGFL.Renderer
		//                    '    'TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO
		//                    'End If
		//                    'If TypeOf objGFL.Renderer Is IProportionalSymbolRenderer Then
		//                    '    Dim objRenderer As IProportionalSymbolRenderer
		//                    '    objRenderer = objGFL.Renderer
		//                    '    'TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO
		//                    'End If
		//                    'If TypeOf objGFL.Renderer Is IScaleDependentRenderer Then
		//                    '    Dim objRenderer As IScaleDependentRenderer
		//                    '    objRenderer = objGFL.Renderer
		//                    '    'TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO TODO
		//                    'End If
		//                End If
		//            End If
		//            frmMotherform.CHLabelSmall("")
		//        Next
		//        Return True
		//    Catch ex As Exception
		//        MessageBox.Show(ex.StackTrace & " " & ex.Source)
		//        ErrorMsg("Fehler bei der Analyse des esri-Projekts", ex.Message, ex.StackTrace, "AnalyseLayerSymbology")
		//        Return False
		//    End Try
		//End Function
#endregion //Verweis auf die n鰐igen ArcMap-Instanzen;
		
		
#region Hilfsfunktionen
		//************************************************************************************************
		//Wenn das aktuelle Symbol ein MarkerSymbol ist, wird das MarkerSymbol, und alle darunter liegenden
		//Objekte (Symbolobjekte) durchgesucht
		//************************************************************************************************
		private string MarkerSymbolScan(IMarkerSymbol Symbol)
		{
			string cValue = "";
			if (Symbol is ISimpleMarkerSymbol)
			{
				cValue = "ISimpleMarkerSymbol";
				return cValue;
			}
			else if (Symbol is ICartographicMarkerSymbol)
			{
				ICartographicMarkerSymbol ICMS = default(ICartographicMarkerSymbol);
                ICMS = Symbol as ICartographicMarkerSymbol;
				if (ICMS is ICharacterMarkerSymbol)
				{
					cValue = "ICharacterMarkerSymbol";
					return cValue;
				}
				else if (ICMS is IPictureMarkerSymbol)
				{
					cValue = "IPictureMarkerSymbol";
					return cValue;
				}
			}
			else if (Symbol is IArrowMarkerSymbol)
			{
				cValue = "IArrowMarkerSymbol";
				return cValue;
			}
			else if (Symbol is IMultiLayerMarkerSymbol)
			{
				cValue = "IMultiLayerMarkerSymbol";
				return cValue;
			}
			else
			{
				cValue = "false";
				return cValue;
			}
            return cValue;
		}
		
		
		//************************************************************************************************
		//Wenn das aktuelle Symbol ein LineSymbol ist, wird das LineSymbol, und alle darunter liegenden
		//Objekte (Symbolobjekte) durchgesucht
		//************************************************************************************************
		private string LineSymbolScan(ILineSymbol Symbol)
		{
			string cValue = "";
			bool bSwitch;
			bSwitch = false;
			if (Symbol is ICartographicLineSymbol)
			{
				ICartographicLineSymbol ICLS = default(ICartographicLineSymbol);
                ICLS = Symbol as ICartographicLineSymbol;
				if (ICLS is IHashLineSymbol)
				{
					cValue = "IHashLineSymbol";
					bSwitch = true;
					return cValue;
				}
				else if (ICLS is IMarkerLineSymbol)
				{
					cValue = "IMarkerLineSymbol";
					bSwitch = true;
					return cValue;
				}
				if (bSwitch == false)
				{
					cValue = "ICartographicLineSymbol";
					return cValue;
				}
                return cValue;
			}
			else if (Symbol is ISimpleLineSymbol)
			{
				cValue = "ISimpleLineSymbol";
				return cValue;
			}
			else if (Symbol is IPictureLineSymbol)
			{
				cValue = "IPictureLineSymbol";
				return cValue;
			}
			else if (Symbol is IMultiLayerLineSymbol)
			{
				cValue = "IMultiLayerLineSymbol";
				return cValue;
			}
			else
			{
				cValue = "false";
				return cValue;
			}
            return cValue;
		}
		
		
		//************************************************************************************************
		//Wenn das aktuelle Symbol ein FillSymbol ist, wird das FillSymbol, und alle darunter liegenden
		//Objekte (Symbolobjekte) durchgesucht
		//************************************************************************************************
		private string FillSymbolScan(IFillSymbol Symbol)
		{
			string cValue = "";
			if (Symbol is ISimpleFillSymbol)
			{
				cValue = "ISimpleFillSymbol";
				return cValue;
			}
			else if (Symbol is IMarkerFillSymbol)
			{
				cValue = "IMarkerFillSymbol";
				return cValue;
			}
			else if (Symbol is ILineFillSymbol)
			{
				cValue = "ILineFillSymbol";
				return cValue;
			}
			else if (Symbol is IDotDensityFillSymbol)
			{
				cValue = "IDotDensityFillSymbol";
				return cValue;
			}
			else if (Symbol is IPictureFillSymbol)
			{
				cValue = "IPictureFillSymbol";
				return cValue;
			}
			else if (Symbol is IGradientFillSymbol)
			{
				cValue = "IGradientFillSymbol";
				return cValue;
			}
			else if (Symbol is IMultiLayerFillSymbol)
			{
				cValue = "IMultiLayerFillSymbol";
				return cValue;
			}
			else
			{
				cValue = "false";
				return cValue;
			}
		}
		
		
		//************************************************************************************************
		//Wenn das aktuelle Symbol ein 3DChartSymbol ist, wird das 3DChartSymbol, und alle darunter liegenden
		//Objekte (Symbolobjekte) durchgesucht
		//************************************************************************************************
		private string IIIDChartSymbolScan(I3DChartSymbol Symbol)
		{
			string cValue = "";
			if (Symbol is IBarChartSymbol)
			{
				cValue = "IBarChartSymbol";
				return cValue;
			}
			else if (Symbol is IPieChartSymbol)
			{
				cValue = "IPieChartSymbol";
				return cValue;
			}
			else if (Symbol is IStackedChartSymbol)
			{
				cValue = "IStackedChartSymbol";
				return cValue;
			}
			else
			{
				cValue = "false";
				return cValue;
			}
		}
		
		
		//************************************************************************************************
		//Die Funktion gibt eine Liste mit UniqeValues zu dem angegebenen Feld aus
		//Parameter:
		//       Table: Das FeatureTable-Objekt
		//       FieldName: Der Spaltenname der betroffenen Spalte, nach der klassifiziert wurde
		//************************************************************************************************
		private bool GimmeUniqeValuesForFieldname(ITable Table, string FieldName)
		{
			IQueryDef pQueryDef = default(IQueryDef);
			IRow pRow = default(IRow);
			ICursor pCursor = default(ICursor);
			IFeatureCursor pFeatureCursor;
			IFeatureWorkspace pFeatureWorkspace = default(IFeatureWorkspace);
			IDataset pDataset = default(IDataset);
			ArrayList alUniqueVal = new ArrayList();
			
			
			try
			{
				pDataset = Table as IDataset;
				pFeatureWorkspace = pDataset.Workspace as IFeatureWorkspace;
				pQueryDef = pFeatureWorkspace.CreateQueryDef();
				pQueryDef.Tables = pDataset.Name;
				pQueryDef.SubFields = "DISTINCT(" + FieldName + ")";
				pCursor = pQueryDef.Evaluate();
				//Hier wird die erhaltene Spalte durchlaufen und in der Arraylist abgespeichert
				pRow = pCursor.NextRow();
				while (!(pRow == null))
				{
					alUniqueVal.Add(pRow.Value[0]);
					pRow = pCursor.NextRow();
				}
				
				
				m_alClassifiedFields.Add(alUniqueVal);
				return true;
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Generieren der UniqueValues", ex.Message, ex.StackTrace, "GimmeUniqeValuesForFieldname");
                return false;
			}
		}
		
		
		//************************************************************************************************
		//Die Funktion gibt eine Liste mit UniqeValues zu dem angegebenen Feld aus
		//Parameter:
		//           Table: Das FeatureTable-Objekt
		//           FieldName: Der Spaltenname der betroffenen Spalte, nach der klassifiziert wurde
		//           JoinedTables: Eine Arraylist mit den Namen aller an die Haupttabelle drangejointen Tabellen
		//************************************************************************************************
		private bool GimmeUniqeValuesForFieldname(ITable Table, string FieldName, ArrayList JoinedTables)
		{
			IQueryDef pQueryDef = default(IQueryDef);
			IRow pRow = default(IRow);
			ICursor pCursor = default(ICursor);
			IFeatureWorkspace pFeatureWorkspace = default(IFeatureWorkspace);
			IDataset pDataset = default(IDataset);
			ArrayList alUniqueVal = new ArrayList();
			
			
			try
			{
				string cMember = "";
				foreach (string tempLoopVar_cMember in JoinedTables)
				{
					cMember = tempLoopVar_cMember;
					cMember = "," + cMember;
				}
				
				pDataset = Table as IDataset;
				pFeatureWorkspace = pDataset.Workspace as IFeatureWorkspace;
				pQueryDef = pFeatureWorkspace.CreateQueryDef();
				pQueryDef.Tables = pDataset.Name + cMember;
				pQueryDef.SubFields = "DISTINCT(" + FieldName + ")";
				pCursor = pQueryDef.Evaluate();
				//Hier wird die erhaltene Spalte durchlaufen und in der Arraylist abgespeichert
				pRow = pCursor.NextRow();
				while (!(pRow == null))
				{
					alUniqueVal.Add(pRow.Value[0]);
					pRow = pCursor.NextRow();
				}
				m_alClassifiedFields.Add(alUniqueVal);
				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + " " + ex.Source + " " + ex.StackTrace);
                return false;
			}
			
		}
		
		//************************************************************************************************
		//Die Funktion gibt eine Liste mit UniqeValues zu dem angegebenen Feld aus f黵 Shape-based Projekte
		//Parameter:
		//           Table: Das FeatureTable-Objekt
		//           FieldName: Der Spaltenname der betroffenen Spalte, nach der klassifiziert wurde
		//************************************************************************************************
		private void GimmeUniqueValuesFromShape(ITable Table, ArrayList FieldNames)
		{
			IQueryFilter pQueryFilter = default(IQueryFilter);
			pQueryFilter = new QueryFilter();
			ICursor pCursor = default(ICursor);
			IDataStatistics pData = default(IDataStatistics);
			pData = new DataStatistics();
			short i = 0;
			short bla;
			IEnumerator objEnum = default(IEnumerator);
			ArrayList al = default(ArrayList);
			
			try
			{
				if (frmMotherform.m_enumLang == Motherform.Language.Deutsch)
				{
					if (MessageBox.Show("ACHTUNG: Sie haben Layer aus einem Shapefile mit mehr als 1 klassifizierenden Feld. " + 
						"Wenn Sie diese Art von Layer analysieren wollen, kann das sehr lange dauern (etliche Minuten bis im 2-stelligen" + 
						"Bereich, je nach Prozessorleistung und Datenmenge TIP: Speichern Sie die Inhalte in " + 
						"einer Personal GDB oder SDE und starten Sie die Analyse erneut-dies dauert je nach Datenmenge nur ein paar Sekunden). " + 
						"Wollen Sie trotzdem mit der Analyse fortfahren?", "ACHTUNG-RECHENINTENSIVE FUNKTION", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
					{
						for (i = 0; i <= FieldNames.Count - 1; i++)
						{
							this.frmMotherform.CHLabelSmall("Bitte warten - Klassifizierungsfeld " + System.Convert.ToString(i + 1) + " von " + System.Convert.ToString(FieldNames.Count));
                            pData.Field = FieldNames[i].ToString();
							pQueryFilter.SubFields = FieldNames[i].ToString();
							pCursor = Table.Search(pQueryFilter, false);
							pData.Cursor = pCursor;
							frmMotherform.DoEvents();
							objEnum = pData.UniqueValues;
							al = new ArrayList();
							objEnum.MoveNext();
							while (!(objEnum.Current == null))
							{
								al.Add(objEnum.Current);
								objEnum.MoveNext();
							}
							al.Sort();
							m_alClassifiedFields.Add(al);
						}
					}
					else
					{
						this.MyTermination();
					}
				}
				else if (frmMotherform.m_enumLang == Motherform.Language.English)
				{
					if (MessageBox.Show("ATTENTION: You have a layer from a shape-file with more than one classifying field. " + 
						"If you want to analyse that kind of layer, it can take A LOT OF TIME (several Minutes until hours " + 
						"depending on your processor and the dimension of your data TIP: Store your data in " + 
						"a personal GDB or an ArcSDE-DB and start the analyse again-that takes only a few seconds). " + 
						"do you anyhow want to continue?", "ATTENTION!-CALCULATION INTENSIVE FUNCTION", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
					{
						for (i = 0; i <= FieldNames.Count - 1; i++)
						{
							this.frmMotherform.CHLabelSmall("Please wait - classified Field Nr. " + System.Convert.ToString(i + 1) + " of " + System.Convert.ToString(FieldNames.Count));
							pData.Field = FieldNames[i].ToString();
							pQueryFilter.SubFields = FieldNames[i].ToString();
							pCursor = Table.Search(pQueryFilter, false);
							pData.Cursor = pCursor;
							frmMotherform.DoEvents();
							objEnum = pData.UniqueValues;
							al = new ArrayList();
							objEnum.MoveNext();
							while (!(objEnum.Current == null))
							{
								al.Add(objEnum.Current);
								objEnum.MoveNext();
							}
							al.Sort();
							m_alClassifiedFields.Add(al);
						}
					}
					else
					{
						this.MyTermination();
					}
				}
				
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Erstellen der UniqueValues", ex.Message, ex.StackTrace, "GimmeUniqueValuesFromShape");
				MyTermination();
			}
		}
		
		//躡erladen:
		//Private Function GimmeUniqueValuesFromShape(ByVal FLayer As IFeatureLayer, ByVal alFields As ArrayList) As Boolean
		//    Dim al As ArrayList
		//    Dim cFieldName As String
		//    Dim pFCur As IFeatureCursor
		//    Dim pFeat As IFeature
		//    Dim pFSel As IFeatureSelection
		//    'Dim i, j As Integer
		//    pFSel = FLayer
		//    al = New ArrayList
		
		//    Select Case alFields.Count
		//        Case 2
		//            m_al1 = New ArrayList
		//            m_al2 = New ArrayList
		//        Case 3
		//            m_al1 = New ArrayList
		//            m_al2 = New ArrayList
		//            m_al3 = New ArrayList
		//    End Select
		//    Try
		//        For Each cFieldName In alFields
		//            If FLayer.FeatureClass.FindField(cFieldName) = -1 Then
		//                ErrorMsg("Fehler beim Erstellen der UniqueValues", "ein 黚ergebenes Feld wurde nicht in dem Layer gefunden", "GimmeUniqueValuesFromShape")
		//                MyTermination()
		//            End If
		//        Next
		//        If pFSel.SelectionSet.Count = 0 Then
		//            pFCur = FLayer.FeatureClass.Search(Nothing, False)
		//        Else
		//            pFSel.SelectionSet.Search(Nothing, False, pFCur)
		//        End If
		
		//        frmMotherform.ShowWorld()
		//        pFeat = pFCur.NextFeature
		//        Do Until pFeat Is Nothing
		//            al.Clear()
		//            For Each cFieldName In alFields
		//                al.Add(pFeat.Value(pFCur.FindField(cFieldName)))
		//            Next
		//            SortCursorRowValues(al)
		//            pFeat = pFCur.NextFeature
		//        Loop
		
		//        Select Case alFields.Count
		//            Case 2
		//                m_alClassifiedFields.Add(m_al1)
		//                m_alClassifiedFields.Add(m_al2)
		//            Case 3
		//                m_alClassifiedFields.Add(m_al1)
		//                m_alClassifiedFields.Add(m_al2)
		//                m_al3 = New ArrayList
		//        End Select
		//        frmMotherform.HideWorld()
		//        Return True
		//    Catch ex As Exception
		//        ErrorMsg("Fehler beim Erstellen der UniqueValues", ex.Message, "GimmeUniqueValuesFromShape")
		//        MessageBox.Show(ex.StackTrace)
		//        MyTermination()
		//    End Try
		//End Function
		
		//************************************************************************************************
		//kleine Hilfsfunktion von GimmeUniqueValuesFromShape, die Werte aus einer Cursorzeile auf
		//Arraylists verteilt
		//************************************************************************************************
		private bool SortCursorRowValues(ArrayList al)
		{
			switch (al.Count)
			{
				case 2:
					m_al1.Add(al[0]);
					m_al2.Add(al[1]);
					break;
				case 3:
					m_al1.Add(al[0]);
					m_al2.Add(al[1]);
					m_al3.Add(al[2]);
					break;
			}
			return default(bool);
		}
		
		//************************************************************************************************
		//Die Funktion gibt die separaten Inhalte der einzelnen Tabellenfelder in einer Arraylist zur點k
		//dazu benutzt sie den Value aus UniqueValues, many Fields zum Vergleich mit den Ergebnissen, die
		//von der Query zur點kgegeben wird
		//Parameter:     value (der zusammengesetzte Value einer Klassifikation, der beim Klassensymbol steht)
		//               FieldNames (Die Feldnamen [Spaltennamen] der Tabelle)
		//               FieldDelimiter (das Trennungszeichen i.a.R. ein komma)
		//               Layer (Der aktuelle, gerade durchlaufene Layer)
		//************************************************************************************************
		private ArrayList GimmeSeperateFieldValues(string value, string FieldDelimiter)
		{
			ArrayList alSepValues = new ArrayList();
			IFeatureCursor objFeatCurs;
			IQueryFilter objQueryFilter;
			objQueryFilter = new QueryFilter();
			IFeature objFeature;
			string cCompare;
			int zahl;
			
			try
			{
				switch (m_alClassifiedFields.Count)
				{
					case 2:
						ArrayList al1_1 = default(ArrayList);
						ArrayList al2_1 = default(ArrayList);
						al1_1 = (ArrayList) (m_alClassifiedFields[0]);
						al2_1 = (ArrayList) (m_alClassifiedFields[1]);
						int i_1 = 0;
						int j_1 = 0;
						for (i_1 = 0; i_1 <= al1_1.Count - 1; i_1++)
						{
							for (j_1 = 0; j_1 <= al2_1.Count - 1; j_1++)
							{
								if (((al1_1[i_1]).ToString() + FieldDelimiter + (al2_1[j_1]).ToString() ) == value)
								{
									alSepValues.Add(al1_1[i_1]);
									alSepValues.Add(al2_1[j_1]);
									goto endOfSelect;
								}
							}
						}
						break;
					case 3:
						ArrayList al1 = default(ArrayList);
						ArrayList al2 = default(ArrayList);
						ArrayList al3 = default(ArrayList);
						al1 = (ArrayList) (m_alClassifiedFields[0]);
						al2 = (ArrayList) (m_alClassifiedFields[1]);
						al3 = (ArrayList) (m_alClassifiedFields[2]);
						int i = 0;
						int j = 0;
						int k = 0;
						for (i = 0; i <=(m_alClassifiedFields[0] as ArrayList).Count - 1; i++)
						{
							for (j = 0; j <= (m_alClassifiedFields[1] as ArrayList).Count - 1; j++)
							{
								for (k = 0; k <= (m_alClassifiedFields[2] as ArrayList).Count - 1; k++)
								{
									if (((al1[i]).ToString() + FieldDelimiter + (al2[j]).ToString() + FieldDelimiter + (al3[k]).ToString() ) == value)
									{
										alSepValues.Add(al1[i]);
										alSepValues.Add(al2[j]);
										alSepValues.Add(al3[k]);
										goto endOfSelect;
									}
								}
							}
						}
						break;
				}
endOfSelect:
				return alSepValues;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
                return alSepValues;
			}
		}
		
		
		//************************************************************************************************
		//Die Funktion gibt die Colors einer ColorRamp als Arraylist zur點k
		//************************************************************************************************
		private ArrayList GimmeArrayListForColorRamp(IColorRamp ColorRamp)
		{
			IEnumColors EColors = default(IEnumColors);
			ArrayList AL = default(ArrayList);
			int i = 0;
			EColors = ColorRamp.Colors;
			for (i = 0; i <= ColorRamp.Size - 1; i++)
			{
				AL.Add(GimmeStringForColor(EColors.Next()));
			}
			return AL;
		}
		
		
		//************************************************************************************************
		//Die Funktion nimmt ein esri-IColor-Objekt entgegen, wandelt die Farbe in Web-Schreibweise um, und
		//gibt sie als string zur點k
		//If color is fully transparent, an empty string is returned.
		//************************************************************************************************
		private string GimmeStringForColor(IColor color)
		{
			string cCol = "";
			string cRed = "";
			string cGreen = "";
			string cBlue = "";
			IRgbColor objRGB = default(IRgbColor);
			
			if (color.Transparency == 0)
			{
				cCol = "";
			}
			else
			{
				objRGB = new RgbColor();
				
				objRGB.RGB = color.RGB;
				cRed = CheckDigits(Conversion.Hex(objRGB.Red).ToString());
				cGreen = CheckDigits(Conversion.Hex(objRGB.Green).ToString());
				cBlue = CheckDigits(Conversion.Hex(objRGB.Blue).ToString());
				cCol = "#" + cRed + cGreen + cBlue;
			}
			
			return cCol;
		}
		
		//************************************************************************************************
		//Die Funktion kontrolliert, ob der den Hexadezimalwert repr鋝entierende String alle 2 Stellen hat
		//(wenn der hexadez nur einstellig ist , wird auch nur eine Stelle zur點kgegeben. Ich brauche auch
		// die vorangestellte Null!
		//************************************************************************************************
		private string CheckDigits(string value)
		{
			string cReturn = "";
			cReturn = value;
			if (cReturn.Length == 1)
			{
				cReturn = cReturn.Insert(0, "0");
			}
			return cReturn;
		}
		
		//************************************************************************************************
		//Extract annotation style and property name from a layer.
		//This initial implementation only handles the most basic annotation: simple expression of a
		//single property, applicable to all features/symbols.
		//Return a StructAnnotation. The PropertyName is an empty string if there is no annotation.
		//************************************************************************************************
		private StructAnnotation GetAnnotation(IFeatureLayer objLayer)
		{
			StructAnnotation annotation = new StructAnnotation();
			
			annotation.PropertyName = "";
			if (objLayer is IGeoFeatureLayer)
			{
				IGeoFeatureLayer objGFL = default(IGeoFeatureLayer);
                objGFL = objLayer as IGeoFeatureLayer;
				
				IAnnotateLayerPropertiesCollection annoPropsColl = default(IAnnotateLayerPropertiesCollection);
				annoPropsColl = objGFL.AnnotationProperties;
				if (objGFL.DisplayAnnotation && annoPropsColl.Count > 0)
				{
					IAnnotateLayerProperties annoLayerProps = default(IAnnotateLayerProperties);
					ESRI.ArcGIS.Carto.IElementCollection null_ESRIArcGISCartoIElementCollection = null;
					ESRI.ArcGIS.Carto.IElementCollection null_ESRIArcGISCartoIElementCollection2 = null;
                    annoPropsColl.QueryItem(0, out  annoLayerProps, out null_ESRIArcGISCartoIElementCollection, out null_ESRIArcGISCartoIElementCollection2);
					if (annoLayerProps is ILabelEngineLayerProperties&& annoLayerProps.DisplayAnnotation)
					{
						ILabelEngineLayerProperties labelProps = default(ILabelEngineLayerProperties);
                        labelProps = annoLayerProps as ILabelEngineLayerProperties;
						// For the moment only implement the simplest case
						if (annoLayerProps.WhereClause == "" && labelProps.IsExpressionSimple)
						{
							annotation.IsSingleProperty = true;
							annotation.PropertyName = System.Convert.ToString(labelProps.Expression.Replace("[", "").Replace("]", ""));
							annotation.TextSymbol = StoreText(labelProps.Symbol);
						}
					}
				}
			}
			return annotation;
		}
		
		//************************************************************************************************
		//Die zentrale Fehlermeldung
		//************************************************************************************************
		private object ErrorMsg(string Message, string ExMessage, string Stack, string FunctionName)
		{
			MessageBox.Show(Message + "\r\n" + ExMessage + "\r\n" + Stack, "ArcGIS_SLD_Converter | Analize_ArcMap_Symbols | " + FunctionName, MessageBoxButtons.OK, MessageBoxIcon.Error);
			MyTermination();
			return null;
		}
		
		//************************************************************************************************
		//Die zentrale Infomeldung
		//************************************************************************************************
		private object InfoMsg(string Message, string FunctionName)
		{
			MessageBox.Show(Message, "ArcGIS_SLD_Converter | Analize_ArcMap_Symbols | " + FunctionName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			return null;
		}
		
		//************************************************************************************************
		//Beim (vorzeitigen) Beenden werden hier alle Objekte zur點kgesetzt
		//
		//************************************************************************************************
		public bool MyTermination()
		{
			ProjectData.EndApp();
			//oder: application.exit
			return default(bool);
		}
#endregion
		
	}
	
}
