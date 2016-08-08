// VBConversions Note: VB project level imports
using System;
using System.Drawing;
using System.Diagnostics;
using System.Data;
using Microsoft.VisualBasic;
using System.Collections;
using System.Windows.Forms;
// End of VB project level imports

using System.Reflection;

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
//The about-box of the app
//CHANGES:
//14.09.2007: The creation of the class



namespace ArcGIS_SLD_Converter
{
	public class AboutFrame : System.Windows.Forms.Form
	{
		
#region  Vom Windows Form Designer generierter Code
		
		public AboutFrame()
		{
			this.Refresh();
			// Dieser Aufruf ist f黵 den Windows Form-Designer erforderlich.
			InitializeComponent();
			// Initialisierungen nach dem Aufruf InitializeComponent() hinzuf黦en
			
			LoadText();
			
		}
		//躡erladen
		public AboutFrame(string language)
		{
			this.Refresh();
			// Dieser Aufruf ist f黵 den Windows Form-Designer erforderlich.
			InitializeComponent();
			// Initialisierungen nach dem Aufruf InitializeComponent() hinzuf黦en
			m_cLanguage = language;
			LoadText();
			
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
		internal System.Windows.Forms.Label AboutLabel3;
		internal System.Windows.Forms.Label AboutLabel2;
		internal System.Windows.Forms.Label AboutLabel1;
		[System.Diagnostics.DebuggerStepThrough()]private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutFrame));
			this.AboutLabel2 = new System.Windows.Forms.Label();
			base.Click += new System.EventHandler(AboutFrame_Click);
			this.AboutLabel2.Click += new System.EventHandler(this.AboutLabel2_Click);
			this.AboutLabel1 = new System.Windows.Forms.Label();
			this.AboutLabel1.Click += new System.EventHandler(this.AboutLabel1_Click);
			this.AboutLabel3 = new System.Windows.Forms.Label();
			this.AboutLabel3.Click += new System.EventHandler(this.AboutLabel3_Click);
			this.SuspendLayout();
			//
			//AboutLabel2
			//
			this.AboutLabel2.Location = new System.Drawing.Point(24, 40);
			this.AboutLabel2.Name = "AboutLabel2";
			this.AboutLabel2.Size = new System.Drawing.Size(424, 200);
			this.AboutLabel2.TabIndex = 0;
			//
			//AboutLabel1
			//
			this.AboutLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", (float) (8.25F), System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0));
			this.AboutLabel1.Location = new System.Drawing.Point(88, 8);
			this.AboutLabel1.Name = "AboutLabel1";
			this.AboutLabel1.Size = new System.Drawing.Size(280, 24);
			this.AboutLabel1.TabIndex = 1;
			//
			//AboutLabel3
			//
			this.AboutLabel3.Location = new System.Drawing.Point(24, 248);
			this.AboutLabel3.Name = "AboutLabel3";
			this.AboutLabel3.Size = new System.Drawing.Size(424, 200);
			this.AboutLabel3.TabIndex = 2;
			//
			//AboutFrame
			//
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.LightBlue;
			this.ClientSize = new System.Drawing.Size(472, 453);
			this.ControlBox = false;
			this.Controls.Add(this.AboutLabel3);
			this.Controls.Add(this.AboutLabel1);
			this.Controls.Add(this.AboutLabel2);
			this.Icon = (System.Drawing.Icon) (resources.GetObject("$this.Icon"));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AboutFrame";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "About ArcMap2SLD Converter";
			this.TopMost = true;
			this.ResumeLayout(false);
			
		}
		
#endregion
		private Motherform frmMotherform;
		
		private string m_cLanguage;
		
		
		private string getVersion()
		{
			System.Reflection.Assembly asm = default(System.Reflection.Assembly);
			
			asm = System.Reflection.Assembly.GetExecutingAssembly();
			string sVersion = System.Convert.ToString(asm.GetName().Version.ToString());
			
			return sVersion;
		}
		
		
		private bool LoadText()
		{
			frmMotherform = new Motherform();
			
			
			if (m_cLanguage == "Deutsch")
			{
				AboutLabel1.Text = frmMotherform.m_text_ger_AboutLabel1 + getVersion();
				AboutLabel2.Text = frmMotherform.m_text_ger_AboutLabel2;
				AboutLabel3.Text = frmMotherform.m_text_ger_AboutLabel3;
			}
			else if (m_cLanguage == "English")
			{
				AboutLabel1.Text = frmMotherform.m_text_eng_AboutLabel1 + getVersion();
				AboutLabel2.Text = frmMotherform.m_text_eng_AboutLabel2;
				AboutLabel3.Text = frmMotherform.m_text_eng_AboutLabel3;
			}
			return default(bool);
		}
		
		private void AboutFrame_Click(object sender, System.EventArgs e)
		{
			this.Close();
			this.Dispose();
		}
		
		private void AboutLabel1_Click(System.Object sender, System.EventArgs e)
		{
			this.Close();
			this.Dispose();
		}
		
		private void AboutLabel2_Click(System.Object sender, System.EventArgs e)
		{
			this.Close();
			this.Dispose();
		}
		
		private void AboutLabel3_Click(System.Object sender, System.EventArgs e)
		{
			this.Close();
			this.Dispose();
		}
		
		
	}
	
}
