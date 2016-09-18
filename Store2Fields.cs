using System;
using System.Drawing;
using System.Diagnostics;
using System.Data;
using Microsoft.VisualBasic;
using System.Collections;
using System.Windows.Forms;
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
