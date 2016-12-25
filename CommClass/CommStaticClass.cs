using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ArcGIS_SLD_Converter
{
  public static  class CommStaticClass
    {
        /// <summary>
        /// 获取颜色字符串
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string GimmeStringForColor(IColor color)
        {
            string cCol = "";
            string cRed = "";
            string cGreen = "";
            string cBlue = "";
            IRgbColor objRGB;
            if (color == null) return cCol;
            if (color.Transparency == 0)
            {
                cCol = "";
            }
            else
            {
                objRGB = new RgbColor();

                objRGB.RGB = color.RGB;
                //十进制颜色数字需要转换成16进制
                cRed = CheckDigits(objRGB.Red.ToString("X"));
                cGreen = CheckDigits(objRGB.Green.ToString("X"));
                cBlue = CheckDigits(objRGB.Blue.ToString("X"));
                cCol = "#" + cRed + cGreen + cBlue;
            }

            return cCol;
        }
        /// <summary>
        /// 16进制颜色补位
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string CheckDigits(string value)
        {
            string cReturn = value;
            if (cReturn.Length == 1)
            {
                cReturn = cReturn.Insert(0, "0");
            }
            return cReturn;
        }
        /// <summary>
        /// 获取唯一值
        /// </summary>
        /// <param name="Table">数据表</param>
        /// <param name="FieldName">字段名称</param>
        /// <returns></returns>
        public static IList<string> GimmeUniqeValuesForFieldname(ITable Table, string FieldName)
        {
            IList<string> tempList = new List<string>();
            try
            {
                IDataset pDataset = Table as IDataset;
                IFeatureWorkspace pFeatureWorkspace = pDataset.Workspace as IFeatureWorkspace;
                IQueryDef pQueryDef = pFeatureWorkspace.CreateQueryDef();
                pQueryDef.Tables = pDataset.Name;
                pQueryDef.SubFields = "DISTINCT(" + FieldName + ")";
                ICursor pCursor = pQueryDef.Evaluate();
                IRow pRow = pCursor.NextRow();
                while (!(pRow == null))
                {
                    tempList.Add(pRow.Value[0].ToString());
                    pRow = pCursor.NextRow();
                }
                return tempList;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return tempList;
            }
        }
        /// <summary>
        /// 获取唯一值
        /// </summary>
        /// <param name="Table">数据表</param>
        /// <param name="FieldName">字段名称</param>
        /// <param name="JoinedTables">连接表</param>
        /// <returns></returns>
        public static IList<string> GimmeUniqeValuesForFieldname(ITable Table, string FieldName, IList<string> JoinedTables)
        {
            IList<string> alUniqueVal = new List<string>();
            try
            {
                string cMember = "";
                foreach (string tempLoopVar_cMember in JoinedTables)
                {
                    cMember = tempLoopVar_cMember;
                    cMember = "," + cMember;
                }

                IDataset pDataset = Table as IDataset;
                IFeatureWorkspace pFeatureWorkspace = pDataset.Workspace as IFeatureWorkspace;
                IQueryDef pQueryDef = pFeatureWorkspace.CreateQueryDef();
                pQueryDef.Tables = pDataset.Name + cMember;
                pQueryDef.SubFields = "DISTINCT(" + FieldName + ")";
                ICursor pCursor = pQueryDef.Evaluate();
                IRow pRow = pCursor.NextRow();
                while (!(pRow == null))
                {
                    alUniqueVal.Add(pRow.Value[0].ToString());
                    pRow = pCursor.NextRow();
                }
                return alUniqueVal;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.Source + " " + ex.StackTrace);
                return alUniqueVal;
            }

        }
        /// <summary>
        /// 从shape文件中获取唯一值
        /// </summary>
        /// <param name="Table"></param>
        /// <param name="FieldNames"></param>
        public static IList<string> GimmeUniqueValuesFromShape(ITable Table, IList<string> FieldNames)
        {
            IQueryFilter pQueryFilter = new QueryFilter();
            IDataStatistics pData = new DataStatistics();
            IList<string> alUniqueVal = new List<string>();
            try
            {
                for (int i = 0; i <= FieldNames.Count - 1; i++)
                {
                    pData.Field = FieldNames[i].ToString();
                    pQueryFilter.SubFields = FieldNames[i].ToString();
                    ICursor pCursor = Table.Search(pQueryFilter, false);
                    pData.Cursor = pCursor;
                    IEnumerator objEnum = pData.UniqueValues;
                    objEnum.MoveNext();
                    while (!(objEnum.Current == null))
                    {
                        alUniqueVal.Add(objEnum.Current.ToString());
                        objEnum.MoveNext();
                    }
                }
                return alUniqueVal;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + " " + ex.Source + " " + ex.StackTrace);
                return alUniqueVal;
            }
        }
        /// <summary>
        /// 获取唯一值渲染方式中指定序号中符号对应的值
        /// </summary>
        /// <param name="Renderer"></param>
        /// <param name="Index"></param>
        /// <returns></returns>
        public static IList<string> getUVFieldValues(IUniqueValueRenderer Renderer, int Index)
        {
            IList<string> Fieldvalues = new List<string>();
            int iFieldCount = 0;
            int Index2 = 0;
            iFieldCount = Renderer.FieldCount;
            if (iFieldCount > 0)
            {
                string Label = string.Empty;
                string Label2 = string.Empty;
                ISymbol objSymbol = default(ISymbol);
                int iNumberOfSymbols = Renderer.ValueCount;
                Label = Renderer.Label[Renderer.Value[Index]];
                Fieldvalues.Add(Renderer.Value[Index]);
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
            return Fieldvalues;
        }
        /// <summary>
        /// 获取符号颜色数组
        /// </summary>
        /// <param name="ColorRamp"></param>
        /// <returns></returns>
        public static IList<string> GimmeArrayListForColorRamp(IColorRamp ColorRamp)
        {
            IEnumColors EColors = ColorRamp.Colors;
            IList<string> AL = new List<string>();
            for (int i = 0; i <= ColorRamp.Size - 1; i++)
            {
                AL.Add(GimmeStringForColor(EColors.Next()));
            }
            return AL;
        }
    }
}
