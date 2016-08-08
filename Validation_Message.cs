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
using Microsoft.VisualBasic.CompilerServices;

//####################################################################################################################
//*******************ArcGIS_SLD_Converter*****************************************************************************
//*******************Class: Validation Message************************************************************************
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
//It displays the messages coming from the class ValidateSLD
//CHANGES:
//####################################################################################################################

namespace ArcGIS_SLD_Converter
{
	public class Validation_Message : System.Windows.Forms.Form
	{
		
#region  Vom Windows Form Designer generierter Code
		
		public Validation_Message(string ValidMessage, Motherform mother)
		{
			
			// Dieser Aufruf ist f黵 den Windows Form-Designer erforderlich.
			InitializeComponent();
			
			// Initialisierungen nach dem Aufruf InitializeComponent() hinzuf黦en
			m_cValidationMessage = ValidMessage;
			this.txtValidMessage.Text = m_cValidationMessage;
			frmMotherform = mother;
			
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
		internal System.Windows.Forms.RichTextBox txtValidMessage;
		internal System.Windows.Forms.Button btnSave;
		internal System.Windows.Forms.Label Label1;
		internal System.Windows.Forms.SaveFileDialog dlgSave;
		[System.Diagnostics.DebuggerStepThrough()]private void InitializeComponent()
		{
			this.txtValidMessage = new System.Windows.Forms.RichTextBox();
			base.Closed += new System.EventHandler(Validation_Message_Closed);
			this.btnSave = new System.Windows.Forms.Button();
			this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
			this.Label1 = new System.Windows.Forms.Label();
			this.dlgSave = new System.Windows.Forms.SaveFileDialog();
			this.SuspendLayout();
			//
			//txtValidMessage
			//
			this.txtValidMessage.Location = new System.Drawing.Point(0, 0);
			this.txtValidMessage.Name = "txtValidMessage";
			this.txtValidMessage.Size = new System.Drawing.Size(448, 216);
			this.txtValidMessage.TabIndex = 0;
			this.txtValidMessage.Text = "";
			//
			//btnSave
			//
			this.btnSave.Location = new System.Drawing.Point(168, 224);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(24, 16);
			this.btnSave.TabIndex = 1;
			this.btnSave.Text = "...";
			//
			//Label1
			//
			this.Label1.Location = new System.Drawing.Point(8, 224);
			this.Label1.Name = "Label1";
			this.Label1.Size = new System.Drawing.Size(160, 16);
			this.Label1.TabIndex = 2;
			this.Label1.Text = "Speichern der Nachricht unter";
			//
			//Validation_Message
			//
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(450, 244);
			this.Controls.Add(this.Label1);
			this.Controls.Add(this.btnSave);
			this.Controls.Add(this.txtValidMessage);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "Validation_Message";
			this.Text = "Nachricht der G黮tigkeits黚erpr黤ung";
			this.TopMost = true;
			this.ResumeLayout(false);
			
		}
		
#endregion
		
		//##################################################################################################
		//######################################## DEKLARATIONEN ###########################################
		//##################################################################################################
		private string m_cValidationMessage;
		private string m_cMessageFilename;
		private Motherform frmMotherform;
		
		
		//##################################################################################################
		//########################################### ROUTINEN #############################################
		//##################################################################################################
		
		
		private void btnSave_Click(System.Object sender, System.EventArgs e)
		{
			try
			{
				dlgSave.CheckFileExists = false;
				dlgSave.CheckPathExists = true;
				dlgSave.DefaultExt = "txt";
				dlgSave.Filter = "Textdateien (*.txt)|*.txt";
				dlgSave.AddExtension = true;
				dlgSave.InitialDirectory = System.IO.Path.GetDirectoryName(m_cMessageFilename);
				dlgSave.OverwritePrompt = true;
				dlgSave.CreatePrompt = false;
				if (dlgSave.ShowDialog() == DialogResult.OK)
				{
					m_cMessageFilename = dlgSave.FileName;
				}
				NewTextFile();
			}
			catch (Exception)
			{
				MessageBox.Show("Fehler beim speichern der Textdatei f黵 die G黮tigkeits-躡erpr黤ung");
				ProjectData.EndApp();
			}
			this.Close();
			this.Dispose();
		}
		
		//##################################################################################################
		//######################################### FUNKTIONEN #############################################
		//##################################################################################################
		
		//************************************************************************************************
		//Die Funktion schreibt die Fehlermeldung in eine Textdatei
		//************************************************************************************************
		private object NewTextFile()
		{
			FileStream objFile = default(FileStream);
			StreamWriter objWriter = default(StreamWriter);
			string[] tempArray = null;
			int i = 0;
			
			try
			{
				objFile = File.OpenWrite(m_cMessageFilename);
				objWriter = new StreamWriter(objFile);
				tempArray = txtValidMessage.Lines;
				for (i = 0; i <= tempArray.GetUpperBound(0); i++)
				{
					objWriter.WriteLine(tempArray[i]);
				}
			}
			catch (Exception)
			{
				MessageBox.Show("Fehler beim speichern der Textdatei f黵 die G黮tigkeits-躡erpr黤ung");
			}
			finally
			{
				objWriter.Close();
				objFile.Close();
			}
			return null;
		}
		
		
		//##################################################################################################
		//############################################# EVENTS #############################################
		//##################################################################################################
		
		private void Validation_Message_Closed(object sender, System.EventArgs e)
		{
			frmMotherform.MinimizeWindow2();
		}
		
		
	}
	
}
