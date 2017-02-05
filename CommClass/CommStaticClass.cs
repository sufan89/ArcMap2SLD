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

        public static IList<int> EsriMarkcircleChartIndex = new int[] { 33, 40, 46, 53, 60, 61, 62, 63, 64, 65, 66, 67, 72, 79, 80, 81, 82, 90, 91, 92, 93, 171, 172, 183, 196, 199, 200, 8729 };

        public static IList<int> EsriMarksquareChartIndex = new int[] { 34, 41, 47, 54, 74, 83, 84, 104, 174, 175, 179, 190, 192, 194, 198, 201 };

        public static IList<int> EsriMarktriangleChartIndex = new int[] { 35, 42, 48, 55, 73, 86, 184, 185 };

        public static IList<int> EsriMarkcrossChartIndex = new int[] { 69, 70, 71, 203, 211 };

        public static IList<int> EsriMarkstarChartIndex = new int[] { 94, 95, 96, 106, 107, 108 };

        public static IList<int> ESRIIGLFONT22circleChartIndex = new int[] { 65, 66, 67, 68, 69, 108, 93, 94, 95, 96, 103, 105, 106 };

        public static IList<int> ESRIIGLFONT22squareChartIndex = new int[] { 70, 71, 88, 89, 90, 91, 92, 118, 119, 120, 121};

        public static IList<int> ESRIIGLFONT22triangleChartIndex = new int[] { 72, 73, 75, 81, 85, 86, 99, 100, 101, 102, 104 };

        public static IList<int> SYMBOLScircleChartIndex = new int[] { 33, 34, 35, 41, 42, 43, 44, 45, 46, 47, 48, 56, 57, 
            58, 65, 68, 69, 70, 71, 74, 75, 76, 77, 82, 83, 86, 87, 88, 89, 92, 93, 94, 95, 98, 99,100,101,104,105,106,107,
            110,111,112,113,116,117,118,119,120,121,122,123,124,125,161,171,177,178,179,180,181,182,183,184,185,186,244,246,247,248,249,8729};

        public static IList<int> SYMBOLSsquareChartIndex = new int[] { 37, 42, 43, 50, 55, 67, 73, 79, 85, 91, 97, 103, 109, 115, 170, 172, 200,
            201, 202, 203, 204, 205, 208, 209, 210, 226,227,228,229,230,231,232,233,234,235,236,237,238,239,240,241,243,250};

        public static IList<int> SYMBOLStriangleChartIndex = new int[] { 36, 46, 49, 66, 72, 78, 84, 90, 96, 102, 108, 114, 162, 168,
        169,175,176,186,187,188,189,190,213,214,215,216,217,218,219,220,245};

        public static IList<int> SYMBOLSXChartIndex = new int[] { 195, 196, 197, 198, 199, 206, 207};

        public static IList<int> MarkChartIndex = new int[] { 33, 34, 35, 36, 37, 38, 39, 67, 68, 69, 71, 81, 88, 97, 98, 99, 100, 101, 102, 103,
        107,113,116,118,161,163,165,167,168,172,174,175,179,182,183,184,185,186,190,192,193,194,195,196,197,198,199,200,201,203,
        204,205,206,207,208,209,210,211,215,219,8729};

        public static IList<int> FONT22ChartIndex = new int[] { 72, 73, 74, 75, 76, 77, 78, 79, 80, 100, 118, 119, 120, 121};

        public static IList<int> SYMBOLSColorChartIndex = new int[] { 34, 35, 36, 37, 38, 39, 40, 120, 161, 162, 163, 164, 165, 166,
        167,187,188,194,195,196,197,198,199,200,202,203,204,205,206,207,208,209,210,211,212,213,214,215,217,218,221,222,223,224,225,
        226,227,228,229,231,232,233,234,235,236,237,238,239,240,241,242,243,244,245,246,247,248,249};

        public static IList<int> SYMBOLS2ChartIndex = new int[] { 85, 88, 89, 91, 94, 95, 97, 98, 100};

        public static IList<int> SYMBOLS3ChartIndex = new int[] { 41, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 122, 123,124,125,
        168,169,188,189,216,230,250};

        public static IList<int> SYMBOLS4ChartIndex = new int[] { 161, 170, 171, 172, 173, 174, 175, 176, 177, 178, 186};
    }
}
