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
using System.Text;
using System.IO;
using Microsoft.VisualBasic.CompilerServices;
using ESRI.ArcGIS;

//####################################################################################################################
//*******************ArcGIS_SLD_Converter*****************************************************************************
//*******************Class: Motherform********************************************************************************
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
//The Button1 triggers the analysis of the ArcMap symbols of the class "Analize_ArcMap_Symbols"
//CHANGES:
//04.02.2006: Language-customizing english/german
//04.02.2006: Tooltips
//28.02.2007: Small code cleanup in using constants.
//25.04.2007: Added option to menu to enable or disable checking all layers (or just selected layers).
//04.09.2008: Added option to menu to enable users to save layers in separate files if necessary
//10.09.2008: Separation of SLD-filename and path. Storing in separate variables (reacheable through properties)
//07.06.2011: (ARIS) Upgraded code to be compatible with ArcGIS 10.
//08.06.2011: (ARIS) Added new flavor of SLD that does not refernce layer names (to be used with WorldMap).
//####################################################################################################################


namespace ArcGIS_SLD_Converter
{
	public class Motherform : System.Windows.Forms.Form
	{
		[STAThread]
		static void Main()
		{
            //' ARIS: Before using any ArcObjects: bind to ESRI ArcGIS Desktop!
            if (!RuntimeManager.Bind(ProductCode.EngineOrDesktop))
            {
                MessageBox.Show("Unable to bind to ArcGIS runtime. Application will be shut down.");
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
			System.Windows.Forms.Application.Run(new Motherform());
		}
		
		
#region  Vom Windows Form Designer generierter Code
		
		public Motherform()
		{
			

			
			// Dieser Aufruf ist f黵 den Windows Form-Designer erforderlich.
			InitializeComponent();
			
			// Initialisierungen nach dem Aufruf InitializeComponent() hinzuf黦en
			m_bLabel = false;
			m_bTooltip = false;
			m_bAllLayers = true;
			m_enumLang = Language.English; //' ARIS: Default language is english
			m_bIncludeLayerNames = true; //' ARIS: Default output is SLD with layer names
			SetSizeOpen();
			InitCommonXML();
			InitControlNameVariables();
			Preconfigure();
			
		}
		
		// Die Form 黚erschreibt den L鰏chvorgang der Basisklasse, um Komponenten zu bereinigen.
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (!(components == null))
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		// F黵 Windows Form-Designer erforderlich
		private System.ComponentModel.Container components = null;
		
		//HINWEIS: Die folgende Prozedur ist f黵 den Windows Form-Designer erforderlich
		//Sie kann mit dem Windows Form-Designer modifiziert werden.
		//Verwenden Sie nicht den Code-Editor zur Bearbeitung.
		internal System.Windows.Forms.Button Button1;
		internal System.Windows.Forms.Button Button2;
		internal System.Windows.Forms.SaveFileDialog dlgSave;
		internal System.Windows.Forms.Label Label3;
		internal System.Windows.Forms.TextBox txtFileName;
		internal System.Windows.Forms.GroupBox GroupBox1;
		internal System.Windows.Forms.RadioButton chkArcIMS;
		internal System.Windows.Forms.RadioButton chkUMN;
		internal System.Windows.Forms.GroupBox GroupBox2;
		internal System.Windows.Forms.Label lblTop;
		internal System.Windows.Forms.Label lblBottom;
		internal System.Windows.Forms.Label lblSmall;
		internal System.Windows.Forms.CheckBox chkValidate;
		internal System.Windows.Forms.GroupBox GroupBox3;
		internal System.Windows.Forms.TextBox txtSLDxsd;
		internal System.Windows.Forms.ComboBox cboLowScale;
		internal System.Windows.Forms.Label Label1;
		internal System.Windows.Forms.Label Label2;
		internal System.Windows.Forms.Label Label4;
		internal System.Windows.Forms.Label Label5;
		internal System.Windows.Forms.OpenFileDialog OpenXSD;
		internal System.Windows.Forms.PictureBox PictureBox1;
		internal System.Windows.Forms.ComboBox cboHighScale;
		internal System.Windows.Forms.CheckBox chkScale;
		internal System.Windows.Forms.Label Label6;
		internal System.Windows.Forms.ImageList ImageList1;
		internal System.Windows.Forms.MainMenu MainMenu1;
		internal System.Windows.Forms.MenuItem MenuItem1;
		internal System.Windows.Forms.MenuItem MenuItem2;
		internal System.Windows.Forms.MenuItem MenuItem3;
		internal System.Windows.Forms.MenuItem MenuItem4;
		internal System.Windows.Forms.MenuItem MenuItem5;
		internal System.Windows.Forms.MenuItem MenuItem6;
		internal System.Windows.Forms.MenuItem MenuItem7;
		internal System.Windows.Forms.MenuItem MenuItem8;
		internal System.Windows.Forms.MenuItem MenuItem9;
		internal System.Windows.Forms.ToolTip ToolTip1;
		internal System.Windows.Forms.MenuItem MenuItem10;
		internal System.Windows.Forms.MenuItem mnuIncludeLayerNames;
		internal System.Windows.Forms.MenuItem MenuItem11;
		[System.Diagnostics.DebuggerStepThrough()]private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			base.Closing += new System.ComponentModel.CancelEventHandler(Motherform_Closing);
			base.Load += new System.EventHandler(Motherform_Load);
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Motherform));
			this.Button1 = new System.Windows.Forms.Button();
			this.Button1.Click += new System.EventHandler(this.Button1_Click);
			this.Button2 = new System.Windows.Forms.Button();
			this.Button2.Click += new System.EventHandler(this.Button2_Click);
			this.txtFileName = new System.Windows.Forms.TextBox();
			this.dlgSave = new System.Windows.Forms.SaveFileDialog();
			this.Label3 = new System.Windows.Forms.Label();
			this.OpenXSD = new System.Windows.Forms.OpenFileDialog();
			this.GroupBox1 = new System.Windows.Forms.GroupBox();
			this.chkUMN = new System.Windows.Forms.RadioButton();
			this.chkArcIMS = new System.Windows.Forms.RadioButton();
			this.GroupBox2 = new System.Windows.Forms.GroupBox();
			this.lblSmall = new System.Windows.Forms.Label();
			this.lblBottom = new System.Windows.Forms.Label();
			this.lblTop = new System.Windows.Forms.Label();
			this.chkValidate = new System.Windows.Forms.CheckBox();
			this.chkValidate.CheckedChanged += new System.EventHandler(this.chkValidate_CheckedChanged);
			this.GroupBox3 = new System.Windows.Forms.GroupBox();
			this.Label6 = new System.Windows.Forms.Label();
			this.chkScale = new System.Windows.Forms.CheckBox();
			this.Label5 = new System.Windows.Forms.Label();
			this.Label4 = new System.Windows.Forms.Label();
			this.Label2 = new System.Windows.Forms.Label();
			this.cboHighScale = new System.Windows.Forms.ComboBox();
			this.cboHighScale.Leave += new System.EventHandler(this.cboHighScale_Leave);
			this.cboHighScale.Enter += new System.EventHandler(this.cboHighScale_Enter);
			this.cboHighScale.DropDown += new System.EventHandler(this.cboHighScale_DropDown);
			this.Label1 = new System.Windows.Forms.Label();
			this.cboLowScale = new System.Windows.Forms.ComboBox();
			this.cboLowScale.Leave += new System.EventHandler(this.cboLowScale_Leave);
			this.cboLowScale.Enter += new System.EventHandler(this.cboLowScale_Enter);
			this.cboLowScale.DropDown += new System.EventHandler(this.cboLowScale_DropDown);
			this.txtSLDxsd = new System.Windows.Forms.TextBox();
			this.PictureBox1 = new System.Windows.Forms.PictureBox();
			this.ImageList1 = new System.Windows.Forms.ImageList(this.components);
			this.MainMenu1 = new System.Windows.Forms.MainMenu(this.components);
			this.MenuItem1 = new System.Windows.Forms.MenuItem();
			this.MenuItem2 = new System.Windows.Forms.MenuItem();
			this.MenuItem3 = new System.Windows.Forms.MenuItem();
			this.MenuItem3.Click += new System.EventHandler(this.MenuItem3_Click);
			this.MenuItem4 = new System.Windows.Forms.MenuItem();
			this.MenuItem4.Click += new System.EventHandler(this.MenuItem4_Click);
			this.MenuItem5 = new System.Windows.Forms.MenuItem();
			this.MenuItem6 = new System.Windows.Forms.MenuItem();
			this.MenuItem6.Click += new System.EventHandler(this.MenuItem6_Click);
			this.MenuItem7 = new System.Windows.Forms.MenuItem();
			this.MenuItem8 = new System.Windows.Forms.MenuItem();
			this.MenuItem8.Click += new System.EventHandler(this.MenuItem8_Click);
			this.MenuItem9 = new System.Windows.Forms.MenuItem();
			this.MenuItem9.Click += new System.EventHandler(this.MenuItem9_Click);
			this.MenuItem11 = new System.Windows.Forms.MenuItem();
			this.MenuItem11.Click += new System.EventHandler(this.MenuItem11_Click);
			this.mnuIncludeLayerNames = new System.Windows.Forms.MenuItem();
			this.mnuIncludeLayerNames.Click += new System.EventHandler(this.mnuIncludeLayerNames_Click);
			this.MenuItem10 = new System.Windows.Forms.MenuItem();
			this.MenuItem10.Click += new System.EventHandler(this.MenuItem10_Click);
			this.ToolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.GroupBox1.SuspendLayout();
			this.GroupBox2.SuspendLayout();
			this.GroupBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize) this.PictureBox1).BeginInit();
			this.SuspendLayout();
			//
			//Button1
			//
			this.Button1.BackColor = System.Drawing.Color.MidnightBlue;
			this.Button1.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (10.0F), System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
			this.Button1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.Button1.Location = new System.Drawing.Point(8, 168);
			this.Button1.Name = "Button1";
			this.Button1.Size = new System.Drawing.Size(96, 24);
			this.Button1.TabIndex = 0;
			this.Button1.Text = "SLD";
			this.Button1.UseVisualStyleBackColor = false;
			//
			//Button2
			//
			this.Button2.BackColor = System.Drawing.Color.MidnightBlue;
			this.Button2.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.Button2.Location = new System.Drawing.Point(96, 18);
			this.Button2.Name = "Button2";
			this.Button2.Size = new System.Drawing.Size(18, 18);
			this.Button2.TabIndex = 1;
			this.Button2.Text = "..";
			this.Button2.UseVisualStyleBackColor = false;
			//
			//txtFileName
			//
			this.txtFileName.Location = new System.Drawing.Point(120, 16);
			this.txtFileName.Name = "txtFileName";
			this.txtFileName.Size = new System.Drawing.Size(288, 20);
			this.txtFileName.TabIndex = 2;
			//
			//Label3
			//
			this.Label3.BackColor = System.Drawing.Color.Transparent;
			this.Label3.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.Label3.Location = new System.Drawing.Point(8, 20);
			this.Label3.Name = "Label3";
			this.Label3.Size = new System.Drawing.Size(88, 16);
			this.Label3.TabIndex = 5;
			this.Label3.Text = "SLD Speicherort";
			this.Label3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			//
			//GroupBox1
			//
			this.GroupBox1.BackColor = System.Drawing.Color.Transparent;
			this.GroupBox1.Controls.Add(this.chkUMN);
			this.GroupBox1.Controls.Add(this.chkArcIMS);
			this.GroupBox1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.GroupBox1.Location = new System.Drawing.Point(8, 48);
			this.GroupBox1.Name = "GroupBox1";
			this.GroupBox1.Size = new System.Drawing.Size(96, 80);
			this.GroupBox1.TabIndex = 9;
			this.GroupBox1.TabStop = false;
			this.GroupBox1.Text = "Serverdaten";
			//
			//chkUMN
			//
			this.chkUMN.Checked = true;
			this.chkUMN.Location = new System.Drawing.Point(8, 48);
			this.chkUMN.Name = "chkUMN";
			this.chkUMN.Size = new System.Drawing.Size(72, 16);
			this.chkUMN.TabIndex = 1;
			this.chkUMN.TabStop = true;
			this.chkUMN.Text = "Shapefile";
			//
			//chkArcIMS
			//
			this.chkArcIMS.Location = new System.Drawing.Point(8, 24);
			this.chkArcIMS.Name = "chkArcIMS";
			this.chkArcIMS.Size = new System.Drawing.Size(72, 16);
			this.chkArcIMS.TabIndex = 0;
			this.chkArcIMS.Text = "ArcSDE";
			//
			//GroupBox2
			//
			this.GroupBox2.BackColor = System.Drawing.Color.Transparent;
			this.GroupBox2.Controls.Add(this.lblSmall);
			this.GroupBox2.Controls.Add(this.lblBottom);
			this.GroupBox2.Controls.Add(this.lblTop);
			this.GroupBox2.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.GroupBox2.Location = new System.Drawing.Point(120, 168);
			this.GroupBox2.Name = "GroupBox2";
			this.GroupBox2.Size = new System.Drawing.Size(288, 96);
			this.GroupBox2.TabIndex = 10;
			this.GroupBox2.TabStop = false;
			this.GroupBox2.Text = "Infofeld";
			//
			//lblSmall
			//
			this.lblSmall.BackColor = System.Drawing.Color.Transparent;
			this.lblSmall.Location = new System.Drawing.Point(8, 72);
			this.lblSmall.Name = "lblSmall";
			this.lblSmall.Size = new System.Drawing.Size(272, 16);
			this.lblSmall.TabIndex = 2;
			//
			//lblBottom
			//
			this.lblBottom.BackColor = System.Drawing.Color.Transparent;
			this.lblBottom.Location = new System.Drawing.Point(8, 40);
			this.lblBottom.Name = "lblBottom";
			this.lblBottom.Size = new System.Drawing.Size(272, 32);
			this.lblBottom.TabIndex = 1;
			//
			//lblTop
			//
			this.lblTop.BackColor = System.Drawing.Color.Transparent;
			this.lblTop.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (8.25F), System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
			this.lblTop.Location = new System.Drawing.Point(8, 16);
			this.lblTop.Name = "lblTop";
			this.lblTop.Size = new System.Drawing.Size(272, 24);
			this.lblTop.TabIndex = 0;
			//
			//chkValidate
			//
			this.chkValidate.BackColor = System.Drawing.Color.Transparent;
			this.chkValidate.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.chkValidate.Location = new System.Drawing.Point(8, 136);
			this.chkValidate.Name = "chkValidate";
			this.chkValidate.Size = new System.Drawing.Size(120, 24);
			this.chkValidate.TabIndex = 11;
			this.chkValidate.Text = "SLD-Validierung";
			this.chkValidate.UseVisualStyleBackColor = false;
			//
			//GroupBox3
			//
			this.GroupBox3.BackColor = System.Drawing.Color.Transparent;
			this.GroupBox3.Controls.Add(this.Label6);
			this.GroupBox3.Controls.Add(this.chkScale);
			this.GroupBox3.Controls.Add(this.Label5);
			this.GroupBox3.Controls.Add(this.Label4);
			this.GroupBox3.Controls.Add(this.Label2);
			this.GroupBox3.Controls.Add(this.cboHighScale);
			this.GroupBox3.Controls.Add(this.Label1);
			this.GroupBox3.Controls.Add(this.cboLowScale);
			this.GroupBox3.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
			this.GroupBox3.Location = new System.Drawing.Point(120, 48);
			this.GroupBox3.Name = "GroupBox3";
			this.GroupBox3.Size = new System.Drawing.Size(288, 80);
			this.GroupBox3.TabIndex = 12;
			this.GroupBox3.TabStop = false;
			this.GroupBox3.Text = "Darstellungsbereich";
			//
			//Label6
			//
			this.Label6.Location = new System.Drawing.Point(16, 24);
			this.Label6.Name = "Label6";
			this.Label6.Size = new System.Drawing.Size(48, 16);
			this.Label6.TabIndex = 7;
			this.Label6.Text = "in SLD?";
			//
			//chkScale
			//
			this.chkScale.Location = new System.Drawing.Point(16, 48);
			this.chkScale.Name = "chkScale";
			this.chkScale.Size = new System.Drawing.Size(48, 24);
			this.chkScale.TabIndex = 6;
			this.chkScale.Text = "O.K.";
			//
			//Label5
			//
			this.Label5.Location = new System.Drawing.Point(192, 24);
			this.Label5.Name = "Label5";
			this.Label5.Size = new System.Drawing.Size(88, 16);
			this.Label5.TabIndex = 5;
			this.Label5.Text = "Kleiner Ma遱tab";
			//
			//Label4
			//
			this.Label4.Location = new System.Drawing.Point(80, 24);
			this.Label4.Name = "Label4";
			this.Label4.Size = new System.Drawing.Size(88, 16);
			this.Label4.TabIndex = 4;
			this.Label4.Text = "Gro遝r Ma遱tab";
			//
			//Label2
			//
			this.Label2.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (8.25F), System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
			this.Label2.Location = new System.Drawing.Point(176, 52);
			this.Label2.Name = "Label2";
			this.Label2.Size = new System.Drawing.Size(16, 16);
			this.Label2.TabIndex = 3;
			this.Label2.Text = "1:";
			//
			//cboHighScale
			//
			this.cboHighScale.Location = new System.Drawing.Point(192, 48);
			this.cboHighScale.Name = "cboHighScale";
			this.cboHighScale.Size = new System.Drawing.Size(80, 21);
			this.cboHighScale.TabIndex = 2;
			//
			//Label1
			//
			this.Label1.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (8.25F), System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
			this.Label1.Location = new System.Drawing.Point(64, 52);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(16, 16);
			this.Label1.TabIndex = 1;
			this.Label1.Text = "1:";
			//
			//cboLowScale
			//
			this.cboLowScale.Location = new System.Drawing.Point(80, 48);
			this.cboLowScale.Name = "cboLowScale";
			this.cboLowScale.Size = new System.Drawing.Size(80, 21);
			this.cboLowScale.TabIndex = 0;
			//
			//txtSLDxsd
			//
			this.txtSLDxsd.Location = new System.Drawing.Point(120, 136);
			this.txtSLDxsd.Name = "txtSLDxsd";
			this.txtSLDxsd.Size = new System.Drawing.Size(288, 20);
			this.txtSLDxsd.TabIndex = 13;
			this.txtSLDxsd.Visible = false;
			//
			//PictureBox1
			//
			this.PictureBox1.BackColor = System.Drawing.Color.Transparent;
			this.PictureBox1.Image = (System.Drawing.Image) (resources.GetObject("PictureBox1.Image"));
			this.PictureBox1.Location = new System.Drawing.Point(40, 216);
			this.PictureBox1.Name = "PictureBox1";
			this.PictureBox1.Size = new System.Drawing.Size(32, 32);
			this.PictureBox1.TabIndex = 15;
			this.PictureBox1.TabStop = false;
			//
			//ImageList1
			//
			this.ImageList1.ImageStream = (System.Windows.Forms.ImageListStreamer) (resources.GetObject("ImageList1.ImageStream"));
			this.ImageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.ImageList1.Images.SetKeyName(0, "");
			//
			//MainMenu1
			//
			this.MainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {this.MenuItem1, this.MenuItem10});
			//
			//MenuItem1
			//
			this.MenuItem1.Index = 0;
			this.MenuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {this.MenuItem2, this.MenuItem5, this.MenuItem7, this.mnuIncludeLayerNames});
			this.MenuItem1.Text = "Extras";
			//
			//MenuItem2
			//
			this.MenuItem2.Index = 0;
			this.MenuItem2.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {this.MenuItem3, this.MenuItem4});
			this.MenuItem2.RadioCheck = true;
			this.MenuItem2.Text = "Sprache/Language";
			//
			//MenuItem3
			//
			this.MenuItem3.Index = 0;
			this.MenuItem3.RadioCheck = true;
			this.MenuItem3.Text = "Deutsch";
			//
			//MenuItem4
			//
			this.MenuItem4.Checked = true;
			this.MenuItem4.Index = 1;
			this.MenuItem4.RadioCheck = true;
			this.MenuItem4.Text = "English";
			//
			//MenuItem5
			//
			this.MenuItem5.Index = 1;
			this.MenuItem5.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {this.MenuItem6});
			this.MenuItem5.Text = "Tooltips";
			//
			//MenuItem6
			//
			this.MenuItem6.Index = 0;
			this.MenuItem6.Text = "ein/on";
			//
			//MenuItem7
			//
			this.MenuItem7.Index = 2;
			this.MenuItem7.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {this.MenuItem8, this.MenuItem9, this.MenuItem11});
			this.MenuItem7.Text = "Layers";
			//
			//MenuItem8
			//
			this.MenuItem8.Checked = true;
			this.MenuItem8.Index = 0;
			this.MenuItem8.RadioCheck = true;
			this.MenuItem8.Text = "Alle/All Layers";
			//
			//MenuItem9
			//
			this.MenuItem9.Index = 1;
			this.MenuItem9.RadioCheck = true;
			this.MenuItem9.Text = "Ausgew鋒lte/Selected Layers";
			//
			//MenuItem11
			//
			this.MenuItem11.Index = 2;
			this.MenuItem11.Text = "In Separate Dateien/In separate Files";
			//
			//mnuIncludeLayerNames
			//
			this.mnuIncludeLayerNames.Checked = true;
			this.mnuIncludeLayerNames.Index = 3;
			this.mnuIncludeLayerNames.Text = "mnuIncludeLayerNames";
			//
			//MenuItem10
			//
			this.MenuItem10.Index = 1;
			this.MenuItem10.Text = "躡er";
			//
			//ToolTip1
			//
			this.ToolTip1.Active = false;
			//
			//Motherform
			//
			this.BackColor = System.Drawing.Color.SteelBlue;
			this.ClientSize = new System.Drawing.Size(423, 275);
			this.Controls.Add(this.PictureBox1);
			this.Controls.Add(this.txtSLDxsd);
			this.Controls.Add(this.GroupBox3);
			this.Controls.Add(this.chkValidate);
			this.Controls.Add(this.GroupBox2);
			this.Controls.Add(this.GroupBox1);
			this.Controls.Add(this.Label3);
			this.Controls.Add(this.txtFileName);
			this.Controls.Add(this.Button2);
			this.Controls.Add(this.Button1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.Icon = (System.Drawing.Icon) (resources.GetObject("$this.Icon"));
			this.MaximizeBox = false;
			this.Menu = this.MainMenu1;
			this.Name = "Motherform";
			this.Text = "  ArcGIS-map to SLD Converter";
			this.TopMost = true;
			this.TransparencyKey = System.Drawing.Color.Brown;
			this.GroupBox1.ResumeLayout(false);
			this.GroupBox2.ResumeLayout(false);
			this.GroupBox3.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize) this.PictureBox1).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
			
		}
		
#endregion
		
		//##################################################################################################
		//######################################## DEKLARATIONEN ###########################################
		//##################################################################################################
#region Membervariablen
		private Analize_ArcMap_Symbols AnalizeArcMap; //Die Instanz der Analysefunktion
		private string m_cSLDFilename; //The path plus Filename of the SLD
		private string m_cSLDPath; //The path to the SLD
		private string m_cSLDFile; //Solely the filename of the SLD
		private string m_cSLDTempFilename; //Der vorl鋟fige Dateiname aus der Config.-XML
		private string m_cXSDFilename; //der Dateiname der Schemadatei
		private CommonXMLHandle m_objCommonXML = new CommonXMLHandle(); //Instanz des "einfachen" XML-Handle
		private ArrayList m_ScaleAl; //Die Arraylist mit den Ma遱tabseintr鋑en
		private string m_cHighScale; //Der aktuelle Text der rechten Combobox
		private string m_cLowScale; //Der aktuelle Text der linken Combobox
		
		private bool m_bLabel; //???
		private bool m_bTooltip; //Tooltips an/aus
		internal bool m_bAllLayers; //All layers processed or not
		internal Language m_enumLang; //die Sprache
		private bool m_bSeparateFiles; //Put each Layer in a separate file yes/no
		private bool m_bIncludeLayerNames; //Use standard sld format (when false: format for WorldMap users)
		private string m_cLUTFilename;
		
		//Die Variablen f黵 die Steuerelementnamen
		private string m_text_eng_Label3;
		private string m_text_eng_Label4;
		private string m_text_eng_Label5;
		private string m_text_eng_Label6;
		private string m_text_ger_Label3;
		private string m_text_ger_Label4;
		private string m_text_ger_Label5;
		private string m_text_ger_Label6;
		
		private string m_ttip_eng_Label3;
		private string m_ttip_eng_Label4;
		private string m_ttip_eng_Label5;
		private string m_ttip_eng_Label6;
		private string m_ttip_ger_Label3;
		private string m_ttip_ger_Label4;
		private string m_ttip_ger_Label5;
		private string m_ttip_ger_Label6;
		//___________________________________________
		
		private string m_text_eng_GroupBox1;
		private string m_text_eng_GroupBox2;
		private string m_text_eng_GroupBox3;
		private string m_text_ger_GroupBox1;
		private string m_text_ger_GroupBox2;
		private string m_text_ger_GroupBox3;
		
		private string m_ttip_eng_GroupBox1;
		private string m_ttip_eng_GroupBox2;
		private string m_ttip_eng_GroupBox3;
		private string m_ttip_ger_GroupBox1;
		private string m_ttip_ger_GroupBox2;
		private string m_ttip_ger_GroupBox3;
		//___________________________________________
		
		private string m_text_eng_chkArcIMS;
		private string m_text_eng_chkUMN;
		private string m_text_ger_chkArcIMS;
		private string m_text_ger_chkUMN;
		
		private string m_ttip_eng_chkArcIMS;
		private string m_ttip_eng_chkUMN;
		private string m_ttip_ger_chkArcIMS;
		private string m_ttip_ger_chkUMN;
		
		private string m_text_eng_chkValidate;
		private string m_text_ger_chkValidate;
		
		private string m_ttip_eng_chkValidate;
		private string m_ttip_ger_chkValidate;
		
		//___________________________________________
		
		private string m_text_eng_Button1;
		private string m_text_ger_Button1;
		
		private string m_ttip_eng_Button1;
		private string m_ttip_ger_Button1;
		//___________________________________________
		
		private string m_text_eng_MenuItem10;
		private string m_text_ger_MenuItem10;
		
		private string m_ttip_eng_MenuItem10;
		private string m_ttip_ger_MenuItem10;
		
		private string m_text_eng_mnuIncludeLayerNames;
		private string m_text_ger_mnuIncludeLayerNames;
		
		private string m_ttip_eng_mnuIncludeLayerNames;
		private string m_ttip_ger_mnuIncludeLayerNames;
		//___________________________________________
		
		//Label-text for the aboutFrame
		internal string m_text_eng_AboutLabel1;
		internal string m_text_ger_AboutLabel1;
		
		internal string m_ttip_eng_AboutLabel1;
		internal string m_ttip_ger_AboutLabel1;
		
		//___________________________________________
		
		//Label-text for the aboutFrame
		internal string m_text_eng_AboutLabel2;
		internal string m_text_ger_AboutLabel2;
		
		internal string m_ttip_eng_AboutLabel2;
		internal string m_ttip_ger_AboutLabel2;
		
		//___________________________________________
		
		//Label-text for the aboutFrame
		internal string m_text_eng_AboutLabel3;
		internal string m_text_ger_AboutLabel3;
		
		internal string m_ttip_eng_AboutLabel3;
		internal string m_ttip_ger_AboutLabel3;
		
		
		
		//##################################################################################################
		//######################################## ENUMERATIONEN ###########################################
		//##################################################################################################
		
#region Enums
		
		//Die Sprache
		internal enum Language
		{
			Deutsch = 0,
			English = 1
		}
		
		internal enum Fileinfo
		{
			Name = 0, //The pure name of the file
			Path = 1 //the pure path to the file
		}
		
#endregion
		
		
		
		
#endregion
		
		//##################################################################################################
		//######################################### FUNKTIONEN #############################################
		//##################################################################################################
#region Funktionen
		//Einfacher Sortieralgorithmus f黵 stringinhalte
		private ArrayList SortMe(ArrayList al)
		{
			ArrayList al2 = new ArrayList();
			string cStoreString = "";
			short i = 0;
			short iTmpDigit = 0;
			
			try
			{
				while (!(al.Count == 1))
				{
					cStoreString = al[0].ToString();
					iTmpDigit = (short) 0;
					for (i = 1; i <= al.Count - 1; i++)
					{
						if (int.Parse(cStoreString) > System.Convert.ToInt32(al[i]))
						{
							cStoreString = al[i].ToString();
							iTmpDigit = i;
						}
					}
					al2.Add(cStoreString);
					al.RemoveAt(iTmpDigit);
				}
				al2.Add(al[0]);
				return al2;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Uuuhps " + ex.Message);
                return al2;
			}
			
		}
		
		//************************************************************************************************
		//Passes back either the pure filename or the path to the SLD without filename. Used for splitting
		//the SLD in separate files
		//************************************************************************************************
		private string GetFileInfo(string FileName, Fileinfo WhatDoIWant)
		{
			int iLastIndex = 0;
			string cWantedSubstring = "";
			cWantedSubstring = "";
			
			iLastIndex = FileName.LastIndexOf("\\");
			if (WhatDoIWant == Fileinfo.Name)
			{
				cWantedSubstring = FileName.Substring(iLastIndex + 1);
			}
			else if (WhatDoIWant == Fileinfo.Path)
			{
				cWantedSubstring = FileName.Remove(iLastIndex, FileName.Substring(iLastIndex).Length);
			}
			return cWantedSubstring;
		}
		
		
		
#endregion
		
		//##################################################################################################
		//########################################### ROUTINEN #############################################
		//##################################################################################################
#region Routinen
		public void CHLabelBottom(string value)
		{
			this.lblBottom.Text = value;
			this.Refresh();
		}
		
		public void CHLabelTop(string value)
		{
			this.lblTop.Text = value;
			this.Refresh();
		}
		
		public void CHLabelSmall(string value)
		{
			this.lblSmall.Text = value;
			this.Refresh();
		}
		
		internal void MinimizeWindow()
		{
			DateTime dCurrentTime = default(DateTime);
			dCurrentTime = DateTime.Now;
			DateTime dTargetTime = default(DateTime);
			dTargetTime = dCurrentTime.AddSeconds(5);
			
			while (dCurrentTime < dTargetTime)
			{
				dCurrentTime = DateTime.Now;
			}
			this.CHLabelSmall("");
			this.CHLabelBottom("");
			this.CHLabelTop("");
			SetSizeClose();
		}
		
		internal void MinimizeWindow2()
		{
			this.CHLabelSmall("");
			this.CHLabelBottom("");
			this.CHLabelTop("");
			SetSizeClose();
		}
		
		private void SetSizeOpen()
		{
			this.Height = 232;
			GroupBox2.Height = 24;
		}
		
		private void SetSizeClose()
		{
			this.Height = 252;
			GroupBox2.Height = 24;
		}
		
		public void MyTermination()
		{
			this.Close();
			this.Dispose();
			ProjectData.EndApp();
			//oder: application.exit
		}
		
		public void DoEvents()
		{
			Application.DoEvents();
		}
		
		//************************************************************************************************
		//Initialisiert das Commmon XML-Handle                                                           *
		//************************************************************************************************
		private void InitCommonXML()
		{
			
			try
			{
				m_objCommonXML.XMLfilename = AppDomain.CurrentDomain.BaseDirectory + "Preconfigure_Converter.xml";
				if (m_objCommonXML.OpenDoc() == true)
				{
					
				}
				else
				{
					if (m_enumLang == Language.Deutsch)
					{
						InfoMsg("Die Konfigurationsdatei Preconfigure_Converter.xml wurde nicht im Anwendungsverzeichnis gefunden." + 
							" Bitte stellen Sie eine aktuelle Preconfigure_Converter.xml in das Anwendungsverzeichnis und starten SIe die Anwendung erneut." + "\r\n", "Preconfigure");
						MyTermination();
					}
					else if (m_enumLang == Language.English)
					{
						InfoMsg("The Configuration-file Preconfigure_Converter.xml wasn\'t found at the application home-directory." + 
							" please copy a Preconfigure_Converter.xml to the application home-directory and restart the application." + "\r\n", "Preconfigure");
						MyTermination();
					}
					
				}
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Einlesen der Vorkonfigurations-XML-Datei" + "\r\n", ex.Message, "InitCommonXML");
				MyTermination();
			}
		}
		
		//************************************************************************************************
		//Initialisiert die Steuerelementnamens-Variablen*
		//************************************************************************************************
		private void InitControlNameVariables()
		{
			
			try
			{
				if (m_objCommonXML.FindRoot("preconfigure") == true)
				{
					if (m_objCommonXML.FindSection("Label3") == true)
					{
						m_objCommonXML.FindEntry("english");
						m_objCommonXML.FindSubEntry("text");
						m_text_eng_Label3 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_eng_Label3 = m_objCommonXML.GetSubentryValue();
						
						m_objCommonXML.FindEntry("deutsch");
						m_objCommonXML.FindSubEntry("text");
						m_text_ger_Label3 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_ger_Label3 = m_objCommonXML.GetSubentryValue();
					}
					if (m_objCommonXML.FindSection("Label4") == true)
					{
						m_objCommonXML.FindEntry("english");
						m_objCommonXML.FindSubEntry("text");
						m_text_eng_Label4 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_eng_Label4 = m_objCommonXML.GetSubentryValue();
						
						m_objCommonXML.FindEntry("deutsch");
						m_objCommonXML.FindSubEntry("text");
						m_text_ger_Label4 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_ger_Label4 = m_objCommonXML.GetSubentryValue();
					}
					if (m_objCommonXML.FindSection("Label5") == true)
					{
						m_objCommonXML.FindEntry("english");
						m_objCommonXML.FindSubEntry("text");
						m_text_eng_Label5 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_eng_Label5 = m_objCommonXML.GetSubentryValue();
						
						m_objCommonXML.FindEntry("deutsch");
						m_objCommonXML.FindSubEntry("text");
						m_text_ger_Label5 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_ger_Label5 = m_objCommonXML.GetSubentryValue();
					}
					if (m_objCommonXML.FindSection("Label5") == true)
					{
						m_objCommonXML.FindEntry("english");
						m_objCommonXML.FindSubEntry("text");
						m_text_eng_Label5 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_eng_Label5 = m_objCommonXML.GetSubentryValue();
						
						m_objCommonXML.FindEntry("deutsch");
						m_objCommonXML.FindSubEntry("text");
						m_text_ger_Label5 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_ger_Label5 = m_objCommonXML.GetSubentryValue();
					}
					if (m_objCommonXML.FindSection("Label6") == true)
					{
						m_objCommonXML.FindEntry("english");
						m_objCommonXML.FindSubEntry("text");
						m_text_eng_Label6 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_eng_Label6 = m_objCommonXML.GetSubentryValue();
						
						m_objCommonXML.FindEntry("deutsch");
						m_objCommonXML.FindSubEntry("text");
						m_text_ger_Label6 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_ger_Label6 = m_objCommonXML.GetSubentryValue();
					}
					if (m_objCommonXML.FindSection("GroupBox1") == true)
					{
						m_objCommonXML.FindEntry("english");
						m_objCommonXML.FindSubEntry("text");
						m_text_eng_GroupBox1 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_eng_GroupBox1 = m_objCommonXML.GetSubentryValue();
						
						m_objCommonXML.FindEntry("deutsch");
						m_objCommonXML.FindSubEntry("text");
						m_text_ger_GroupBox1 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_ger_GroupBox1 = m_objCommonXML.GetSubentryValue();
					}
					if (m_objCommonXML.FindSection("GroupBox2") == true)
					{
						m_objCommonXML.FindEntry("english");
						m_objCommonXML.FindSubEntry("text");
						m_text_eng_GroupBox2 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_eng_GroupBox2 = m_objCommonXML.GetSubentryValue();
						
						m_objCommonXML.FindEntry("deutsch");
						m_objCommonXML.FindSubEntry("text");
						m_text_ger_GroupBox2 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_ger_GroupBox2 = m_objCommonXML.GetSubentryValue();
					}
					if (m_objCommonXML.FindSection("GroupBox2") == true)
					{
						m_objCommonXML.FindEntry("english");
						m_objCommonXML.FindSubEntry("text");
						m_text_eng_GroupBox2 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_eng_GroupBox2 = m_objCommonXML.GetSubentryValue();
						
						m_objCommonXML.FindEntry("deutsch");
						m_objCommonXML.FindSubEntry("text");
						m_text_ger_GroupBox2 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_ger_GroupBox2 = m_objCommonXML.GetSubentryValue();
					}
					if (m_objCommonXML.FindSection("GroupBox3") == true)
					{
						m_objCommonXML.FindEntry("english");
						m_objCommonXML.FindSubEntry("text");
						m_text_eng_GroupBox3 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_eng_GroupBox3 = m_objCommonXML.GetSubentryValue();
						
						m_objCommonXML.FindEntry("deutsch");
						m_objCommonXML.FindSubEntry("text");
						m_text_ger_GroupBox3 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_ger_GroupBox3 = m_objCommonXML.GetSubentryValue();
					}
					if (m_objCommonXML.FindSection("boxSLDType") == true)
					{
						m_objCommonXML.FindEntry("english");
						m_objCommonXML.FindSubEntry("text");
						m_objCommonXML.FindSubEntry("tooltip");
						
						m_objCommonXML.FindEntry("deutsch");
						m_objCommonXML.FindSubEntry("text");
						m_objCommonXML.FindSubEntry("tooltip");
					}
					if (m_objCommonXML.FindSection("chkArcIMS") == true)
					{
						m_objCommonXML.FindEntry("english");
						m_objCommonXML.FindSubEntry("text");
						m_text_eng_chkArcIMS = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_eng_chkArcIMS = m_objCommonXML.GetSubentryValue();
						
						m_objCommonXML.FindEntry("deutsch");
						m_objCommonXML.FindSubEntry("text");
						m_text_ger_chkArcIMS = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_ger_chkArcIMS = m_objCommonXML.GetSubentryValue();
					}
					if (m_objCommonXML.FindSection("chkUMN") == true)
					{
						m_objCommonXML.FindEntry("english");
						m_objCommonXML.FindSubEntry("text");
						m_text_eng_chkUMN = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_eng_chkUMN = m_objCommonXML.GetSubentryValue();
						
						m_objCommonXML.FindEntry("deutsch");
						m_objCommonXML.FindSubEntry("text");
						m_text_ger_chkUMN = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_ger_chkUMN = m_objCommonXML.GetSubentryValue();
					}
					if (m_objCommonXML.FindSection("chkValidate") == true)
					{
						m_objCommonXML.FindEntry("english");
						m_objCommonXML.FindSubEntry("text");
						m_text_eng_chkValidate = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_eng_chkValidate = m_objCommonXML.GetSubentryValue();
						
						m_objCommonXML.FindEntry("deutsch");
						m_objCommonXML.FindSubEntry("text");
						m_text_ger_chkValidate = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_ger_chkValidate = m_objCommonXML.GetSubentryValue();
					}
					if (m_objCommonXML.FindSection("chkSLDStandard") == true)
					{
						m_objCommonXML.FindEntry("english");
						m_objCommonXML.FindSubEntry("text");
						m_objCommonXML.FindSubEntry("tooltip");
						
						m_objCommonXML.FindEntry("deutsch");
						m_objCommonXML.FindSubEntry("text");
						m_objCommonXML.FindSubEntry("tooltip");
					}
					if (m_objCommonXML.FindSection("chkSLDWorldMap") == true)
					{
						m_objCommonXML.FindEntry("english");
						m_objCommonXML.FindSubEntry("text");
						m_objCommonXML.FindSubEntry("tooltip");
						
						m_objCommonXML.FindEntry("deutsch");
						m_objCommonXML.FindSubEntry("text");
						m_objCommonXML.FindSubEntry("tooltip");
					}
					if (m_objCommonXML.FindSection("Button1") == true)
					{
						m_objCommonXML.FindEntry("english");
						m_objCommonXML.FindSubEntry("text");
						m_text_eng_Button1 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_eng_Button1 = m_objCommonXML.GetSubentryValue();
						
						m_objCommonXML.FindEntry("deutsch");
						m_objCommonXML.FindSubEntry("text");
						m_text_ger_Button1 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_ger_Button1 = m_objCommonXML.GetSubentryValue();
					}
					if (m_objCommonXML.FindSection("MenuItem10") == true)
					{
						m_objCommonXML.FindEntry("english");
						m_objCommonXML.FindSubEntry("text");
						m_text_eng_MenuItem10 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_eng_MenuItem10 = m_objCommonXML.GetSubentryValue();
						
						m_objCommonXML.FindEntry("deutsch");
						m_objCommonXML.FindSubEntry("text");
						m_text_ger_MenuItem10 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_ger_MenuItem10 = m_objCommonXML.GetSubentryValue();
					}
					if (m_objCommonXML.FindSection("mnuIncludeLayerNames") == true)
					{
						m_objCommonXML.FindEntry("english");
						m_objCommonXML.FindSubEntry("text");
						m_text_eng_mnuIncludeLayerNames = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_eng_mnuIncludeLayerNames = m_objCommonXML.GetSubentryValue();
						
						m_objCommonXML.FindEntry("deutsch");
						m_objCommonXML.FindSubEntry("text");
						m_text_ger_mnuIncludeLayerNames = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_ger_mnuIncludeLayerNames = m_objCommonXML.GetSubentryValue();
					}
					if (m_objCommonXML.FindSection("AboutLabel1") == true)
					{
						m_objCommonXML.FindEntry("english");
						m_objCommonXML.FindSubEntry("text");
						m_text_eng_AboutLabel1 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_eng_AboutLabel1 = m_objCommonXML.GetSubentryValue();
						
						m_objCommonXML.FindEntry("deutsch");
						m_objCommonXML.FindSubEntry("text");
						m_text_ger_AboutLabel1 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_ger_AboutLabel1 = m_objCommonXML.GetSubentryValue();
					}
					if (m_objCommonXML.FindSection("AboutLabel2") == true)
					{
						m_objCommonXML.FindEntry("english");
						m_objCommonXML.FindSubEntry("text");
						m_text_eng_AboutLabel2 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_eng_AboutLabel2 = m_objCommonXML.GetSubentryValue();
						
						m_objCommonXML.FindEntry("deutsch");
						m_objCommonXML.FindSubEntry("text");
						m_text_ger_AboutLabel2 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_ger_AboutLabel2 = m_objCommonXML.GetSubentryValue();
					}
					if (m_objCommonXML.FindSection("AboutLabel3") == true)
					{
						m_objCommonXML.FindEntry("english");
						m_objCommonXML.FindSubEntry("text");
						m_text_eng_AboutLabel3 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_eng_AboutLabel3 = m_objCommonXML.GetSubentryValue();
						
						m_objCommonXML.FindEntry("deutsch");
						m_objCommonXML.FindSubEntry("text");
						m_text_ger_AboutLabel3 = m_objCommonXML.GetSubentryValue();
						m_objCommonXML.FindSubEntry("tooltip");
						m_ttip_ger_AboutLabel3 = m_objCommonXML.GetSubentryValue();
					}
				}
			}
			catch (Exception ex)
			{
				ErrorMsg("Die Elemente in der Vorkonfigurations-XML-Datei sind nicht korrekt." + "\r\n", ex.Message, "InitControlNameVariables");
				
			}
		}
		
		//************************************************************************************************
		//Die Initialisierung der Steuerelemente mit labels
		//************************************************************************************************
		private void InitControls()
		{
			switch (this.m_enumLang)
			{
				case Language.Deutsch:
					this.Label3.Text = m_text_ger_Label3;
					this.Label4.Text = m_text_ger_Label4;
					this.Label5.Text = m_text_ger_Label5;
					this.Label6.Text = m_text_ger_Label6;
					this.GroupBox1.Text = m_text_ger_GroupBox1;
					this.GroupBox2.Text = m_text_ger_GroupBox2;
					this.GroupBox3.Text = m_text_ger_GroupBox3;
					this.chkArcIMS.Text = m_text_ger_chkArcIMS;
					this.chkUMN.Text = m_text_ger_chkUMN;
					this.chkValidate.Text = m_text_ger_chkValidate;
					this.Button1.Text = m_text_ger_Button1;
					this.MenuItem10.Text = m_text_ger_MenuItem10;
					this.mnuIncludeLayerNames.Text = m_text_ger_mnuIncludeLayerNames;
					break;
				default: //' Default language is english!
					this.Label3.Text = m_text_eng_Label3;
					this.Label4.Text = m_text_eng_Label4;
					this.Label5.Text = m_text_eng_Label5;
					this.Label6.Text = m_text_eng_Label6;
					this.GroupBox1.Text = m_text_eng_GroupBox1;
					this.GroupBox2.Text = m_text_eng_GroupBox2;
					this.GroupBox3.Text = m_text_eng_GroupBox3;
					this.chkArcIMS.Text = m_text_eng_chkArcIMS;
					this.chkUMN.Text = m_text_eng_chkUMN;
					this.chkValidate.Text = m_text_eng_chkValidate;
					this.Button1.Text = m_text_eng_Button1;
					this.MenuItem10.Text = m_text_eng_MenuItem10;
					this.mnuIncludeLayerNames.Text = m_text_eng_mnuIncludeLayerNames;
					break;
			}
		}
		
		//************************************************************************************************
		//Die Initialisierung der Steuerelemente mit Tooltips*
		//************************************************************************************************
		private void InitTooltips()
		{
			switch (this.m_enumLang)
			{
				case Language.Deutsch:
					this.ToolTip1.SetToolTip(this.Label3, m_ttip_ger_Label3);
					this.ToolTip1.SetToolTip(this.Label4, m_ttip_ger_Label4);
					this.ToolTip1.SetToolTip(this.Label5, m_ttip_ger_Label5);
					this.ToolTip1.SetToolTip(this.Label6, m_ttip_ger_Label6);
					this.ToolTip1.SetToolTip(this.GroupBox1, m_ttip_ger_GroupBox1);
					this.ToolTip1.SetToolTip(this.GroupBox2, m_ttip_ger_GroupBox2);
					this.ToolTip1.SetToolTip(this.GroupBox3, m_ttip_ger_GroupBox3);
					this.ToolTip1.SetToolTip(this.chkArcIMS, m_ttip_ger_chkArcIMS);
					this.ToolTip1.SetToolTip(this.chkUMN, m_ttip_ger_chkUMN);
					this.ToolTip1.SetToolTip(this.chkValidate, m_ttip_ger_chkValidate);
					this.ToolTip1.SetToolTip(this.Button1, m_ttip_ger_Button1);
					break;
				default: //' Default language is english!
					this.ToolTip1.SetToolTip(this.Label3, m_ttip_eng_Label3);
					this.ToolTip1.SetToolTip(this.Label4, m_ttip_eng_Label4);
					this.ToolTip1.SetToolTip(this.Label5, m_ttip_eng_Label5);
					this.ToolTip1.SetToolTip(this.Label6, m_ttip_eng_Label6);
					this.ToolTip1.SetToolTip(this.GroupBox1, m_ttip_eng_GroupBox1);
					this.ToolTip1.SetToolTip(this.GroupBox2, m_ttip_eng_GroupBox2);
					this.ToolTip1.SetToolTip(this.GroupBox3, m_ttip_eng_GroupBox3);
					this.ToolTip1.SetToolTip(this.chkArcIMS, m_ttip_eng_chkArcIMS);
					this.ToolTip1.SetToolTip(this.chkUMN, m_ttip_eng_chkUMN);
					this.ToolTip1.SetToolTip(this.chkValidate, m_ttip_eng_chkValidate);
					this.ToolTip1.SetToolTip(this.Button1, m_ttip_eng_Button1);
					break;
			}
			
		}
		
		//************************************************************************************************
		//Liest die Daten f黵 die Vorkonfiguration des Formulars aus der XML (i.A. Die Scales-Comboboxen)*
		//************************************************************************************************
		private void Preconfigure()
		{
			short i = 0;
			ArrayList alTmp = default(ArrayList);
			
			try
			{
				if (m_objCommonXML.FindRoot("preconfigure") == true)
				{
					//Ab hier die Ma遱t鋌e
					if (m_objCommonXML.FindSection("scales") == true)
					{
						m_ScaleAl = m_objCommonXML.GetAllEntryValues();
					}
				}
				
				alTmp = SortMe(m_ScaleAl);
				m_ScaleAl = (ArrayList) null;
				m_ScaleAl = alTmp;
				cboLowScale.Items.Clear();
				cboHighScale.Items.Clear();
				for (i = 0; i <= m_ScaleAl.Count - 1; i++)
				{
					cboLowScale.Items.Add(m_ScaleAl[i]);
					cboHighScale.Items.Add(m_ScaleAl[i]);
				}
				cboLowScale.Text = System.Convert.ToString(cboLowScale.Items[0]);
				cboHighScale.Text = System.Convert.ToString(cboLowScale.Items[m_ScaleAl.Count - 1]);
				
				//Ab hier der SLD-Dateipfad/Name
				if (m_objCommonXML.FindSection("SLDFilename") == true)
				{
					m_cSLDTempFilename = m_objCommonXML.getSectionValue();
				}
				
				//Ab hier die Checkbox Scales
				if (m_objCommonXML.FindSection("Checkboxes") == true)
				{
					if (m_objCommonXML.FindEntry("chkScale") == true)
					{
						if (m_objCommonXML.getEntryValue().ToUpper() == "true".ToUpper())
						{
							chkScale.Checked = true;
						}
						else if (m_objCommonXML.getEntryValue().ToUpper() == "false".ToUpper())
						{
							chkScale.Checked = false;
						}
						else
						{
							chkScale.Checked = false;
						}
					}
				}
				
				//ab hier werden die Labelnamen auf Englisch/Deutsch ausgelesen
				
				
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Einlesen der Vorkonfigurations-XML-Datei" + "\r\n", ex.Message, "Preconfiguration");
				MyTermination();
			}
		}
		
		//************************************************************************************************
		//Schreibt die letzten Benutzereinstellungen zur點k in die Preconfigure_Converter.xml
		//************************************************************************************************
		internal void ReadBackValues()
		{
			
			try
			{
				//Ab hier der SLD-Dateipfad/Name
				if (m_objCommonXML.FindSection("SLDFilename") == true)
				{
					m_objCommonXML.SetSectionValue(txtFileName.Text);
				}
				//Ab hier die Checkbox Scales
				if (m_objCommonXML.FindSection("Checkboxes") == true)
				{
					if (m_objCommonXML.FindEntry("chkScale") == true)
					{
						if (chkScale.Checked == true)
						{
							m_objCommonXML.SetEntryValue("true");
						}
						else
						{
							m_objCommonXML.SetEntryValue("false");
						}
					}
				}
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Auslesen in die Vorkonfigurations-XML-Datei" + "\r\n", ex.Message, "ReadBackValues");
				MyTermination();
			}
		}
		
		//************************************************************************************************
		//**********************Liest die Scales-Daten aus der XML erneut in die Comboboxen***************
		//************************************************************************************************
		private void LoadCboScalesNew()
		{
			string cHighScale = "";
			string cLowScale = "";
			short i = 0;
			ArrayList alTmp = default(ArrayList);
			cHighScale = cboHighScale.Text;
			cLowScale = cboLowScale.Text;
			
			try
			{
				if (m_objCommonXML.FindRoot("preconfigure") == true)
				{
					if (m_objCommonXML.FindSection("scales") == true)
					{
						m_ScaleAl = m_objCommonXML.GetAllEntryValues();
					}
				}
				alTmp = SortMe(m_ScaleAl);
				m_ScaleAl = (ArrayList) null;
				m_ScaleAl = alTmp;
				cboLowScale.Items.Clear();
				cboHighScale.Items.Clear();
				for (i = 0; i <= m_ScaleAl.Count - 1; i++)
				{
					cboLowScale.Items.Add(m_ScaleAl[i]);
					cboHighScale.Items.Add(m_ScaleAl[i]);
				}
				cboLowScale.Text = cLowScale;
				cboHighScale.Text = cHighScale;
			}
			catch (Exception ex)
			{
				ErrorMsg("Fehler beim Einlesen der Vorkonfigurations-XML-Datei", ex.Message, "Preconfiguration");
			}
		}
		
		
		
		//************************************************************************************************
		//********************************* Die zentrale Fehlermeldung ***********************************
		//************************************************************************************************
		private void ErrorMsg(string Message, string ExMessage, string FunctionName)
		{
			MessageBox.Show(Message + "\r\n" + ExMessage, "ArcGIS_SLD_Converter | Analize_ArcMap_Symbols | " + FunctionName, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
		
		//************************************************************************************************
		//********************************* Die zentrale Infomeldung ***********************************
		//************************************************************************************************
		private void InfoMsg(string message, string functionname)
		{
			MessageBox.Show(message, "Analize_ArcMap_Symbols | Output_SLD | " + functionname, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}
		
		//************************************************************************************************
		//zeigt das Wartesymbol an
		//************************************************************************************************
		internal void ShowWorld()
		{
			PictureBox1.Visible = true;
			PictureBox1.Refresh();
			this.Refresh();
		}
		
		//************************************************************************************************
		//zeigt das Wartesymbol an
		//************************************************************************************************
		internal void HideWorld()
		{
			PictureBox1.Visible = false;
		}
#endregion
		
		//##################################################################################################
		//######################################### PROPERTIES #############################################
		//##################################################################################################
#region Properties
		//returns the whole path and filename
public string GetSLDFilename
		{
			get
			{
				return m_cSLDFilename;
			}
		}
		
public string GetInfoIncludeLayerNames
		{
			get
			{
				return System.Convert.ToString( m_bIncludeLayerNames);
			}
		}
		
public bool GetInfoSeparateLayers
		{
			get
			{
				return m_bSeparateFiles;
			}
		}
		
		//returns only the path of the SLD
public string GetSLDPath
		{
			get
			{
				return m_cSLDPath;
			}
		}
		
		//returns only the Name of the file
public string GetSLDFile
		{
			get
			{
				return m_cSLDFile;
			}
		}
		
		
public string GetXSDFilename
		{
			get
			{
				return m_cXSDFilename;
			}
		}
		
public string GetSLDFileFromConfigXML
		{
			get
			{
				return m_cSLDTempFilename;
			}
		}
		
public string GetCurrentLanguage
		{
			get
			{
				if (m_enumLang == Language.Deutsch)
				{
					return "Deutsch";
				}
				else if (m_enumLang == Language.English)
				{
					return "English";
				}
				
				return "";
			}
		}
#endregion
		
		//##################################################################################################
		//############################################# EVENTS #############################################
		//##################################################################################################
#region Events
		//Der SLD-Button st鲞t die Analyse an
		private void Button1_Click(System.Object sender, System.EventArgs e)
		{
			//Die Gr鲞e von Formular und Infofeld wird ge鋘dert
			this.Height = 324;
			this.GroupBox2.Height = 96;
			//Hier noch die 躡erpr黤ung, ob Ober und Untergrenze des Ma遱tabs korrekt gew鋒lt wurden
			if (this.chkScale.Checked == true)
			{
				if (int.Parse(this.cboHighScale.Text) < int.Parse(this.cboLowScale.Text))
				{
					string cTmp = "";
					cTmp = cboHighScale.Text;
					cboHighScale.Text = cboLowScale.Text;
					cboLowScale.Text = cTmp;
				}
			}
			//Die Analyse wird gestartet
			AnalizeArcMap = new Analize_ArcMap_Symbols(this, m_cSLDFilename);
			if (this.chkValidate.Checked == false)
			{
				MinimizeWindow();
			}
			if (this.lblSmall.Text == "Die SLD ist g黮tig")
			{
				MinimizeWindow();
			}
		}
		
		//Der SLD speichern unter Button
		private void Button2_Click(System.Object sender, System.EventArgs e)
		{
			System.IO.StreamWriter swOutput;
			// File oFile = default(File); VBConversions Warning: Cannot declare a variable of a static type in C#
			
			if (System.IO.File.Exists(this.GetSLDFileFromConfigXML))
			{
				this.dlgSave.InitialDirectory = this.GetSLDFileFromConfigXML;
			}
			dlgSave.CheckFileExists = false;
			dlgSave.CheckPathExists = true;
			dlgSave.DefaultExt = "sld";
			dlgSave.Filter = "SLD-Dateien (*.sld)|*.sld";
			dlgSave.AddExtension = true;
			dlgSave.OverwritePrompt = true;
			dlgSave.CreatePrompt = false;
			if (dlgSave.ShowDialog() == DialogResult.OK)
			{
				m_cSLDFilename = dlgSave.FileName; //The Path plus Filename
				m_cSLDPath = GetFileInfo(m_cSLDFilename, Fileinfo.Path);
				m_cSLDFile = GetFileInfo(m_cSLDFilename, Fileinfo.Name);
				txtFileName.Text = m_cSLDFilename;
			}
		}
		
		//das Validierungsoptionsfeld
		private void chkValidate_CheckedChanged(object sender, System.EventArgs e)
		{
			if (chkValidate.Checked == true)
			{
				txtSLDxsd.Visible = true;
				OpenXSD.CheckFileExists = false;
				OpenXSD.CheckPathExists = true;
				OpenXSD.Filter = "Schemadateien (*.xsd)|*.xsd";
				OpenXSD.InitialDirectory = System.IO.Path.GetDirectoryName(m_cSLDFilename);
				if (OpenXSD.ShowDialog() == DialogResult.OK)
				{
					m_cXSDFilename = OpenXSD.FileName;
					txtSLDxsd.Text = m_cXSDFilename;
				}
				else
				{
					m_cXSDFilename = "";
					chkValidate.Checked = false;
					txtSLDxsd.Visible = false;
					return;
				}
			}
			else
			{
				txtSLDxsd.Visible = false;
			}
		}
		
		
		
		//############################################################################################################
		//Die folgenden Ereignisse beziehen sich alle auf die Ma遱tabscomboboxen
		//############################################################################################################
		
		//Die 躡erpr黤ung, da?auch nur Zahlen in Comboboxfeld eingegeben werden. Wenn Zahl noch nicht in XML-
		//datei enthalten, wird sie 黚ernommen in XML-Datei
		private void cboHighScale_Leave(object sender, System.EventArgs e)
		{
			short i;
			short j = 0;
			bool bIsNumber = true;
			string cComboText = "";
			cComboText = cboHighScale.Text;
			
			try
			{
				for (j = 0; j <= cComboText.Length - 1; j++)
				{
					if (!(cComboText[j] == '0' || cComboText[j] == '1' || cComboText[j] == '2' || cComboText[j] == '3' || cComboText[j] == '4' || cComboText[j] == '5' || cComboText[j] == '6' || cComboText[j] == '7' || cComboText[j] == '8' || cComboText[j] == '9'))
					{
						if (m_enumLang == Language.Deutsch)
						{
							MessageBox.Show("Bitte geben Sie nur ZAHLEN ohne Komma zwischen 10 und 100000000 in das Textfeld ein!", null, MessageBoxButtons.OK, MessageBoxIcon.Information);
						}
						else if (m_enumLang == Language.English)
						{
							MessageBox.Show("Please insert only NUMBERS between 10 und 100000000 into the text field!", null, MessageBoxButtons.OK, MessageBoxIcon.Information);
						}
						cboHighScale.Text = m_cHighScale;
						bIsNumber = false;
						return;
					}
				}
				if (bIsNumber == true)
				{
					if (!m_ScaleAl.Contains(cboHighScale.Text))
					{
						//Hier die Aufnahme in die Datei Preconfigure_Converter.xml
						m_objCommonXML.FindSection("scales");
						m_objCommonXML.CreateEntry("scale");
						m_objCommonXML.SetEntryValue(cboHighScale.Text);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Unbekannter Fehler." + "\r\n" + ex.Message + "\r\n" + ex.StackTrace);
			}
		}
		
		
		//Die 躡erpr黤ung, da?auch nur Zahlen in Comboboxfeld eingegeben werden. Wenn Zahl noch nicht in XML-
		//datei enthalten, wird sie 黚ernommen in XML-Datei
		private void cboLowScale_Leave(object sender, System.EventArgs e)
		{
			short i;
			short j = 0;
			bool bIsNumber = true;
			string cComboText = "";
			cComboText = cboLowScale.Text;
			
			try
			{
				for (j = 0; j <= cComboText.Length - 1; j++)
				{
					if (!(cComboText[j] == '0' || cComboText[j] == '1' || cComboText[j] == '2' || cComboText[j] == '3' || cComboText[j] == '4' || cComboText[j] == '5' || cComboText[j] == '6' || cComboText[j] == '7' || cComboText[j] == '8' || cComboText[j] == '9'))
					{
						if (m_enumLang == Language.Deutsch)
						{
							MessageBox.Show("Bitte geben Sie nur ZAHLEN ohne Komma zwischen 10 und 100000000 in das Textfeld ein!", null, MessageBoxButtons.OK, MessageBoxIcon.Information);
						}
						else if (m_enumLang == Language.English)
						{
							MessageBox.Show("Please insert only NUMBERS between 10 und 100000000 into the text field!", null, MessageBoxButtons.OK, MessageBoxIcon.Information);
						}
						cboLowScale.Text = m_cLowScale;
						bIsNumber = false;
						return;
					}
				}
				if (bIsNumber == true)
				{
					if (!m_ScaleAl.Contains(cboLowScale.Text))
					{
						//Hier die Aufnahme in die Datei Preconfigure_Converter.xml
						m_objCommonXML.FindSection("scales");
						m_objCommonXML.CreateEntry("scale");
						m_objCommonXML.SetEntryValue(cboLowScale.Text);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Unbekannter Fehler." + "\r\n" + ex.Message + "\r\n" + ex.StackTrace);
			}
		}
		
		private void cboHighScale_Enter(object sender, System.EventArgs e)
		{
			m_cHighScale = cboHighScale.Text;
		}
		
		private void cboLowScale_Enter(object sender, System.EventArgs e)
		{
			m_cLowScale = cboLowScale.Text;
		}
		
		private void cboHighScale_DropDown(object sender, System.EventArgs e)
		{
			LoadCboScalesNew();
		}
		
		private void cboLowScale_DropDown(object sender, System.EventArgs e)
		{
			LoadCboScalesNew();
		}
		
		//############################################################################################################
		//Die folgenden Ereignisse beziehen sich alle auf die Men黮eiste
		//############################################################################################################
		
		//Men? Deutsch
		private void MenuItem3_Click(System.Object sender, System.EventArgs e)
		{
			MenuItem3.Checked = true;
			MenuItem4.Checked = false;
			this.m_enumLang = Language.Deutsch;
			InitControls();
			InitTooltips();
		}
		
		//Men? English
		private void MenuItem4_Click(System.Object sender, System.EventArgs e)
		{
			MenuItem3.Checked = false;
			MenuItem4.Checked = true;
			this.m_enumLang = Language.English;
			InitControls();
			InitTooltips();
		}
		
		//Men? Tooltips
		private void MenuItem6_Click(System.Object sender, System.EventArgs e)
		{
			if (m_bTooltip == false)
			{
				InitTooltips();
				ToolTip1.Active = true;
				MenuItem6.Text = "aus/off";
				m_bTooltip = true;
			}
			else
			{
				ToolTip1.Active = false;
				MenuItem6.Text = "ein/on";
				m_bTooltip = false;
			}
		}
		
		//Men? Layers All
		private void MenuItem8_Click(System.Object sender, System.EventArgs e)
		{
			m_bAllLayers = true;
			MenuItem8.Checked = true;
			MenuItem9.Checked = false;
		}
		
		//Men? Layers Selected
		private void MenuItem9_Click(System.Object sender, System.EventArgs e)
		{
			m_bAllLayers = false;
			MenuItem8.Checked = false;
			MenuItem9.Checked = true;
		}
		
		//Menu: AboutBox
		private void MenuItem10_Click(System.Object sender, System.EventArgs e)
		{
			//MessageBox.Show(m_enumLang.ToString)
			AboutFrame frmAboutBox = default(AboutFrame);
			frmAboutBox = new AboutFrame(System.Convert.ToString(m_enumLang.ToString()));
			frmAboutBox.Show();
			frmAboutBox.Visible = true;
		}
		
		//Menu: Layers in different Files
		private void MenuItem11_Click(object sender, System.EventArgs e)
		{
			if (m_bSeparateFiles == false)
			{
				m_bSeparateFiles = true;
				MenuItem11.Checked = true;
			}
			else
			{
				m_bSeparateFiles = false;
				MenuItem11.Checked = false;
			}
		}
		
		
		//Beim Schlie遝n der Anwendung werden die Werte zur點kgeschrieben in die Datei PreconfigureConverter
		private void Motherform_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			ReadBackValues();
		}
#endregion
		
		private void Motherform_Load(System.Object sender, System.EventArgs e)
		{
			//' Initialize form
			InitControls();
			InitTooltips();
		}
		
		private void mnuIncludeLayerNames_Click(System.Object sender, System.EventArgs e)
		{
			mnuIncludeLayerNames.Checked = !mnuIncludeLayerNames.Checked;
			m_bIncludeLayerNames = mnuIncludeLayerNames.Checked;
		}
		
	}
	
}
