using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ArcGIS_SLD_Converter
{
    /// <summary>
    /// 渲染方式
    /// </summary>
    public class ptRender
    {
        public ptRender(IFeatureRenderer pFeatureRender,ILayer pLayer)
        {
            if (pLayer != null)
            {
                IFeatureLayer pFeatureLayer = pLayer as IFeatureLayer;
                m_FeatureClass = pFeatureLayer.FeatureClass;
                m_LayerName = pFeatureLayer.Name;
                IDataset pds = pFeatureLayer.FeatureClass as IDataset;
                m_DatasetName = pds.Name;
                AnnotationClass = new AnnotationClass(pFeatureLayer);
                SymbolList = new List<ptSymbolClass>();

            }
        }
        /// <summary>
        /// 初始化符号信息
        /// </summary>
        protected virtual void InitialSymbol()
        {
 
        }
        /// <summary>
        /// 渲染的要素类
        /// </summary>
        public IFeatureClass m_FeatureClass{get; set;}
        /// <summary>
        /// 渲染图层名称
        /// </summary>
        public string m_LayerName{get; set;}
        /// <summary>
        /// 数据集名称
        /// </summary>
        public string m_DatasetName{get; set;}
        /// <summary>
        /// 标记
        /// </summary>
        public AnnotationClass AnnotationClass { get; set; }
        /// <summary>
        /// 符号列表
        /// </summary>
        public IList<ptSymbolClass> SymbolList { get; set; }
    }
    /// <summary>
    /// 唯一值渲染
    /// </summary>
    public class ptUniqueValueRendererClass : ptRender
    {
        public ptUniqueValueRendererClass(IFeatureRenderer pFeatureRender, IFeatureLayer pFeatureLayer) : 
            base( pFeatureRender, pFeatureLayer)
        {
            m_pUniqueRender = pFeatureRender as IUniqueValueRenderer;
            m_pFeatureLayer = pFeatureLayer;
            ValueCount = m_pUniqueRender.ValueCount;
            FieldCount = m_pUniqueRender.FieldCount;
            FieldNames = new List<string>();
            InitialSymbol();
        }
        private IUniqueValueRenderer m_pUniqueRender;
        private IFeatureLayer m_pFeatureLayer;
        /// <summary>
        /// 唯一值数量
        /// </summary>
        public int ValueCount { get; set;}
        /// <summary>
        /// 字段数量
        /// </summary>
        public int FieldCount { get; set; }
        /// <summary>
        /// 所有字段名称
        /// </summary>
        public IList<string> FieldNames { get; set; }

        public string StylePath { get; set; }
        /// <summary>
        /// 初始化符号信息
        /// </summary>
        protected override void InitialSymbol()
        {
            base.InitialSymbol();

            if (m_pUniqueRender == null || m_pFeatureLayer == null) return;
            //是否是多个字段
            bool bNoSepFieldVal = false;
            //是否是连接表
            bool bIsJoined = false;
            try
            {
                IDisplayTable pDisplayTable = m_pFeatureLayer as IDisplayTable;
                ITable pTable = pDisplayTable.DisplayTable;
                IDataset objDataset = m_FeatureClass as IDataset; 
                //是否是关系表
                if (pTable is IRelQueryTable)
                {
                    bIsJoined = true;
                }

                if (FieldCount > 1)
                {
                    bNoSepFieldVal = true;
                }
                //唯一值字段有多个
                if (bNoSepFieldVal)
                {
                    //数据源为SHAPE文件
                    if (objDataset.Workspace.Type == esriWorkspaceType.esriFileSystemWorkspace)
                    {
                        for (int i = 1; i <= FieldCount; i++)
                        {
                            FieldNames.Add(m_pUniqueRender.Field[i - 1]);
                        }

                        CommStaticClass.GimmeUniqueValuesFromShape(m_FeatureClass as ITable, FieldNames);
                    }
                    //数据源为其他
                    else
                    {
                        for (int i = 1; i <= FieldCount; i++)
                        {
                            FieldNames.Add(m_pUniqueRender.Field[i - 1]);
                            //属性表有连接表                                        
                            if (pTable is IRelQueryTable)
                            {
                                IRelQueryTable pRelQueryTable = default(IRelQueryTable);
                                ITable pDestTable = default(ITable);
                                IDataset pDataSet = default(IDataset);
                                IList<string> alJoinedTableNames = new List<string>();
                                while (pTable is IRelQueryTable)
                                {
                                    pRelQueryTable = pTable as IRelQueryTable;
                                    pDestTable = pRelQueryTable.DestinationTable;
                                    pDataSet = pDestTable as IDataset;

                                    pTable = pRelQueryTable.SourceTable;
                                    alJoinedTableNames.Add(pDataSet.Name);
                                }
                                CommStaticClass.GimmeUniqeValuesForFieldname(m_FeatureClass as ITable, m_pUniqueRender.Field[i - 1], alJoinedTableNames);
                                pTable = pDisplayTable.DisplayTable;
                            }
                            //属性表没有连接表
                            else
                            {
                                CommStaticClass.GimmeUniqeValuesForFieldname(m_FeatureClass as ITable, m_pUniqueRender.Field[i - 1]);
                            }
                        }
                    }
                }
                //唯一值字段只有一个
                else
                {
                    FieldNames.Add(m_pUniqueRender.Field[FieldCount - 1]);
                }
                
                //开始解析符号
                for (int j = 0; j <= ValueCount - 1; j++)
                {
                    ISymbol pSymbol = m_pUniqueRender.get_Symbol(m_pUniqueRender.get_Value (j));
                    ptSymbolFactory pSymbolFac=new ptSymbolFactory (pSymbol);
                    ptSymbolClass pSymbolClass = pSymbolFac.GetSymbolClass(m_pUniqueRender.Label[m_pUniqueRender.get_Value(j)],CommStaticClass.getUVFieldValues(m_pUniqueRender, j)
                        ,0,0);
                    SymbolList.Add(pSymbolClass);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
    }
    /// <summary>
    /// 分类渲染
    /// </summary>
    public class ptClassBreaksRendererCalss : ptRender
    {
        /// <summary>
        /// 分类渲染
        /// </summary>
        /// <param name="pFeatureRender"></param>
        /// <param name="pFeatureLayer"></param>
        public ptClassBreaksRendererCalss(IFeatureRenderer pFeatureRender, IFeatureLayer pFeatureLayer) : 
            base(pFeatureRender, pFeatureLayer)
        {
            m_pClassBreaksRender = pFeatureRender as IClassBreaksRenderer;
            m_pFeatureLayer = pFeatureLayer;
            BreakCount = m_pClassBreaksRender.BreakCount;
            FieldName = m_pClassBreaksRender.Field;
            NormFieldName = m_pClassBreaksRender.NormField;
            InitialSymbol();
        }
        private IClassBreaksRenderer m_pClassBreaksRender;
        private IFeatureLayer m_pFeatureLayer;

        public int BreakCount { get; set; }
        public string FieldName { get; set; }
        public string NormFieldName { get; set; }
        /// <summary>
        /// 初始化符号信息
        /// </summary>
        protected override void InitialSymbol()
        {
            base.InitialSymbol();
            IClassBreaksUIProperties objClassBreaksProp = m_pClassBreaksRender as IClassBreaksUIProperties;
            try
            {
                for (int i = 0; i < BreakCount; i++)
                {
                    double cLowerLimit = objClassBreaksProp.LowBreak[i];
                    double cUpperLimit = m_pClassBreaksRender.Break[i];
                    ISymbol pSymbol = m_pClassBreaksRender.get_Symbol(i);
                    ptSymbolFactory pSymbolFac = new ptSymbolFactory(pSymbol);
                    ptSymbolClass pSymbolClass = pSymbolFac.GetSymbolClass(m_pClassBreaksRender.Label[i], new List<string>()
                        , cUpperLimit, cLowerLimit);
                    SymbolList.Add(pSymbolClass);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
    /// <summary>
    /// 简单渲染方式
    /// </summary>
    public class ptSimpleRendererClass : ptRender
    {
        /// <summary>
        /// 简单渲染方式
        /// </summary>
        /// <param name="pFeatureRender">渲染方式</param>
        /// <param name="pFeatureLayer">渲染的目标图层</param>
        public ptSimpleRendererClass(IFeatureRenderer pFeatureRender, IFeatureLayer pFeatureLayer) : 
            base(pFeatureRender, pFeatureLayer)
        {
             m_pSimpleRender = pFeatureRender as ISimpleRenderer;
             m_pFeatureLayer = pFeatureLayer;
        }
        private ISimpleRenderer m_pSimpleRender;
        IFeatureLayer m_pFeatureLayer;
        protected override void InitialSymbol()
        {
            base.InitialSymbol();
            ISymbol pSymbol = m_pSimpleRender.Symbol;
            ptSymbolFactory pSymbolFac = new ptSymbolFactory(pSymbol);
            ptSymbolClass pSymbolClass = pSymbolFac.GetSymbolClass(m_pSimpleRender.Label, new List<string>()
                , 0, 0);
            SymbolList.Add(pSymbolClass);
        }
    }
}
