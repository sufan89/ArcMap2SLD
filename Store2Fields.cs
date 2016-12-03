using System;
using System.Drawing;
using System.Diagnostics;
using System.Data;
using Microsoft.VisualBasic;
using System.Collections;
using System.Windows.Forms;
namespace ArcGIS_SLD_Converter
{
    /// <summary>
    /// 
    /// </summary>
	public class Store2Fields
	{
        /// <summary>
        /// 数组1
        /// </summary>
		private ArrayList al1;
        /// <summary>
        /// 数组2
        /// </summary>
		private ArrayList al2;
		/// <summary>
        /// 初始化
        /// </summary>
		public Store2Fields()
		{
			al1 = new ArrayList();
			al2 = new ArrayList();
		}
		/// <summary>
        /// 添加数组元素
        /// </summary>
        /// <param name="String1">数组1</param>
        /// <param name="String2">数组2</param>
		public void Add2Strings(string String1, string String2)
		{
			al1.Add(String1);
			al2.Add(String2);
		}
		/// <summary>
        /// 根据数组1索引获取数组1的元素
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 根据数组2索引获取数组2的元素
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
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
		/// <summary>
        /// 根据数组1的要素获取数组2相应索引的要素
        /// </summary>
        /// <param name="String1"></param>
        /// <returns></returns>
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
		/// <summary>
        /// 获取数组1的元素个数
        /// </summary>
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
	}
	
}
