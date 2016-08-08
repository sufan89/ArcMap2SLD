// VBConversions Note: VB project level imports
using System;
using System.Drawing;
using System.Diagnostics;
using System.Data;
using Microsoft.VisualBasic;
using System.Collections;
using System.Windows.Forms;
// End of VB project level imports


//####################################################################################################################
//*******************ArcGIS_SLD_Converter*****************************************************************************
//*******************Class: Store2Fields******************************************************************************
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
//DESCRIPTION:The class Store2Fields is simply a data container that is only needed in the case if a layer is classified
//by data that is joined to the main table
//CHANGES:
//####################################################################################################################


namespace ArcGIS_SLD_Converter
{
	public class Store2Fields
	{
		private ArrayList al1;
		private ArrayList al2;
		
		public Store2Fields()
		{
			al1 = new ArrayList();
			al2 = new ArrayList();
		}
		
		public void Add2Strings(string String1, string String2)
		{
			al1.Add(String1);
			al2.Add(String2);
		}
		
		public string get_GetString1ByIndex(int Index)
		{
			if (!(al1.Count < Index))
			{
				return al1[Index].ToString();
			}
			else
			{
				return "false";
			}
		}
		
		public string get_GetString2ByIndex(int Index)
		{
			if (!(al2.Count < Index))
			{
				return al2[Index].ToString();
			}
			else
			{
				return "false";
			}
		}
		
		public string get_GetString1ForString2(string String2)
		{
			short i = 0;
			for (i = 0; i <= al2.Count - 1; i++)
			{
				if (al2[i].ToString() == String2)
				{
					return al1[i].ToString();
				}
			}
			return "";
		}
		
		public string get_GetString2ForString1(string String1)
		{
			short i = 0;
			for (i = 0; i <= al1.Count - 1; i++)
			{
				if (al1[i].ToString() == String1)
				{
					return al2[i].ToString();
				}
			}
			return "";
		}
		
public ArrayList GetStringlist1
		{
			get
			{
				return al1;
			}
		}
		
public ArrayList GetStringlist2
		{
			get
			{
				return al2;
			}
		}
		
public int Count
		{
			get
			{
				return al1.Count;
			}
		}
		
		public bool get_ContainsString1(string String1)
		{
			bool bSwitch;
			bSwitch = false;
			short i = 0;
			for (i = 0; i <= al1.Count - 1; i++)
			{
				if (String1 == (string) al1[i])
				{
					bSwitch = true;
					return true;
				}
			}
			if (bSwitch == false)
			{
				return false;
			}
			return default(bool);
		}
		
		public bool get_ContainsString2(string String2)
		{
			bool bSwitch;
			bSwitch = false;
			short i = 0;
			for (i = 0; i <= al1.Count - 1; i++)
			{
				if (String2 == (string) al2[i])
				{
					bSwitch = true;
					return true;
				}
			}
			if (bSwitch == false)
			{
				return false;
			}
			return default(bool);
		}
		
	}
	
}
