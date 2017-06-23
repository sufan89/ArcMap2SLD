using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

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
                m_ptLayer = new ptLayer();
                m_ptLayer.InitailGeneralInfo(pFeatureLayer);
                m_ptLayer.m_LayerRender = this;
            }
        }
        /// <summary>
        /// 初始化符号信息
        /// </summary>
        protected virtual void InitialSymbol()
        {
            
        }
        /// <summary>
        /// 获取渲染的xml节点
        /// </summary>
        /// <param name="xmlDoc">SLD文档对象</param>
        /// <param name="RootXmlElement">当前的节点对象</param>
        /// <returns></returns>
        public virtual XmlElement GetRendXmlNode(XmlDocument xmlDoc,XmlElement RootXmlElement)
        {
            return null;
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
        public ptLayer m_ptLayer { get; }
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
                            FieldNames.Add(m_pUniqueRender.Field[i - 1].ToLower());
                        }

                        CommStaticClass.GimmeUniqueValuesFromShape(m_FeatureClass as ITable, FieldNames);
                    }
                    //数据源为其他
                    else
                    {
                        for (int i = 1; i <= FieldCount; i++)
                        {
                            FieldNames.Add(m_pUniqueRender.Field[i - 1].ToLower());
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
                    FieldNames.Add(m_pUniqueRender.Field[FieldCount - 1].ToLower());
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
                ptLogManager.WriteMessage(ex.Message);
            }

        }
        public override XmlElement GetRendXmlNode(XmlDocument xmlDoc, XmlElement RootXmlElement)
        {
            try
            {
                XmlElement pAnnotationElment=null;
                //如果有标注，则添加标注信息
                if (AnnotationClass.IsSingleProperty && !string.IsNullOrEmpty(AnnotationClass.PropertyName))
                {
                     pAnnotationElment = AnnotationClass.GetSymbolNode(xmlDoc);
                }
                //开始解析渲染符号信息
                for (int i = 0; i < SymbolList.Count; i++)
                {
                    XmlElement pRuleElement = default(XmlElement);
                    ptSymbolClass pSymbolClass = SymbolList[i];
                    //生成Rule节点信息
                    pRuleElement = CommXmlHandle.CreateElement("Rule", xmlDoc);
                    pRuleElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("RuleName", xmlDoc, pSymbolClass.Label));
                    pRuleElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("Title", xmlDoc, pSymbolClass.Label));
                    XmlElement pFilterElement = CommXmlHandle.CreateElement("Filter", xmlDoc);
                    //设置符号选择器
                    //多字段多值组合符号
                    if (this.FieldCount > 1)
                    {
                        XmlElement pAndElement = CommXmlHandle.CreateElement("And", xmlDoc);
                        for (int l = 0; l <= FieldCount - 1; l++)
                        {
                            XmlElement pEqualToElment = CommXmlHandle.CreateElement("PropertyIsEqualTo", xmlDoc);
                            pEqualToElment.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("PropertyName", xmlDoc, FieldNames[l]));
                            pEqualToElment.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("Fieldvalue", xmlDoc, pSymbolClass.Fieldvalues[l])) ;
                            pAndElement.AppendChild(pEqualToElment);
                        }
                        pFilterElement.AppendChild(pAndElement);
                    }
                    //单字段多值同一符号
                    else if (FieldCount == 1)
                    {
                        XmlElement pOrElement = default(XmlElement);
                        if (pSymbolClass.Fieldvalues.Count > 1)
                        {
                            pOrElement = CommXmlHandle.CreateElement("Or", xmlDoc);
                        }
                        for (int l = 0; l <= pSymbolClass.Fieldvalues.Count - 1; l++)
                        {
                            if (pSymbolClass.Fieldvalues.Count > 1)
                            {
                                XmlElement pEqualToElment = CommXmlHandle.CreateElement("PropertyIsEqualTo", xmlDoc);
                                pEqualToElment.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("PropertyName", xmlDoc, FieldNames[l]));
                                pEqualToElment.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("Fieldvalue", xmlDoc, pSymbolClass.Fieldvalues[l]));
                                pOrElement.AppendChild(pEqualToElment);
                            }
                            else
                            {
                                XmlElement pEqualToElment = CommXmlHandle.CreateElement("PropertyIsEqualTo", xmlDoc);
                                pEqualToElment.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("PropertyName", xmlDoc, FieldNames[l]));
                                pEqualToElment.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("Fieldvalue", xmlDoc, pSymbolClass.Fieldvalues[l]));
                                pFilterElement.AppendChild(pEqualToElment);
                            }
                        }
                        if(pSymbolClass.Fieldvalues.Count > 1) pFilterElement.AppendChild(pOrElement);
                    }
                    pRuleElement.AppendChild(pFilterElement);
                    //设置显示比例尺
                    if (!double.IsNaN(m_ptLayer.m_MaxScale) && !double.IsNaN(m_ptLayer.m_MinScale))
                    {
                        pRuleElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("MinScale", xmlDoc, m_ptLayer.m_MaxScale.ToString()));
                        pRuleElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("MaxScale", xmlDoc, m_ptLayer.m_MinScale.ToString()));
                    }
                    //获取符号节点
                    IList<XmlElement> pSymbolizedNode = pSymbolClass.GetSymbolNode(xmlDoc);
                    foreach (XmlElement pElement in pSymbolizedNode)
                    {
                        pRuleElement.AppendChild(pElement);
                    }
                    if (pAnnotationElment != null)
                    {
                        pRuleElement.AppendChild(pAnnotationElment);
                    }
                    RootXmlElement.AppendChild(pRuleElement);
                }
            }
            catch (Exception ex)
            {
                ptLogManager.WriteMessage(string.Format("解析符号信息失败:{0}{1}{2}{3}",Environment.NewLine,ex.Message,Environment.NewLine,ex.StackTrace));
            }
            return RootXmlElement;
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
        public override XmlElement GetRendXmlNode(XmlDocument xmlDoc, XmlElement RootXmlElement)
        {
            try
            {
                //如果有标注，则添加标注信息
                XmlElement pAnnotationElment = null;
                if (AnnotationClass.IsSingleProperty && !string.IsNullOrEmpty(AnnotationClass.PropertyName))
                {
                    pAnnotationElment = AnnotationClass.GetSymbolNode(xmlDoc);
                }
                //开始解析渲染符号信息
                for (int i = 0; i < SymbolList.Count; i++)
                {
                    XmlElement pRuleElement = default(XmlElement);
                    ptSymbolClass pSymbolClass = SymbolList[i];
                    //生成Rule节点信息
                    pRuleElement = CommXmlHandle.CreateElement("Rule", xmlDoc);
                    pRuleElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("RuleName", xmlDoc, pSymbolClass.Label));
                    pRuleElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("Title", xmlDoc, pSymbolClass.Label));
                    if (pSymbolClass.LowerLimit != 0.00 && pSymbolClass.UpperLimit != 0.00)
                    {
                        //写条件节点
                        XmlElement pFilterElement = CommXmlHandle.CreateElement("Filter", xmlDoc);
                        XmlElement pBetweenElement = CommXmlHandle.CreateElement("PropertyIsBetween", xmlDoc);
                        pFilterElement.AppendChild(pBetweenElement);
                        XmlElement pPropertyNameElement = CommXmlHandle.CreateElementAndSetElemnetText("PropertyName", xmlDoc, this.FieldName);
                        pBetweenElement.AppendChild(pPropertyNameElement);
                        XmlElement pLowerBoundaryElement = CommXmlHandle.CreateElement("LowerBoundary", xmlDoc);
                        pBetweenElement.AppendChild(pLowerBoundaryElement);
                        XmlElement pLowerValue = CommXmlHandle.CreateElementAndSetElemnetText("", xmlDoc, CommStaticClass.CommaToPoint(pSymbolClass.LowerLimit));
                        pLowerBoundaryElement.AppendChild(pLowerValue);

                        XmlElement pUpperElement = CommXmlHandle.CreateElement("UpperBoundary", xmlDoc);
                        pBetweenElement.AppendChild(pUpperElement);
                        XmlElement pUpperValueElement = CommXmlHandle.CreateElementAndSetElemnetText("Fieldvalue", xmlDoc, CommStaticClass.CommaToPoint(pSymbolClass.UpperLimit));
                        pUpperElement.AppendChild(pUpperValueElement);

                        pRuleElement.AppendChild(pFilterElement);
                    }
                    //设置显示比例尺
                    if (!double.IsNaN(m_ptLayer.m_MaxScale) && !double.IsNaN(m_ptLayer.m_MinScale))
                    {
                        pRuleElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("MinScale", xmlDoc, m_ptLayer.m_MaxScale.ToString()));
                        pRuleElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("MaxScale", xmlDoc, m_ptLayer.m_MinScale.ToString()));
                    }
                    //获取符号节点
                    IList<XmlElement> pSymbolizedNode = pSymbolClass.GetSymbolNode(xmlDoc);
                    foreach (XmlElement pElement in pSymbolizedNode)
                    {
                        pRuleElement.AppendChild(pElement);
                    }
                    if (pAnnotationElment != null)
                    {
                        pRuleElement.AppendChild(pAnnotationElment);
                    }
                    RootXmlElement.AppendChild(pRuleElement);
                }
            }
            catch (Exception ex)
            {
                ptLogManager.WriteMessage(string.Format("解析符号信息失败:{0}{1}{2}{3}", Environment.NewLine, ex.Message, Environment.NewLine, ex.StackTrace));
            }
            return RootXmlElement;
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
            InitialSymbol();
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
        public override XmlElement GetRendXmlNode(XmlDocument xmlDoc, XmlElement RootXmlElement)
        {
            try
            {
                //如果有标注，则添加标注信息
                XmlElement pAnnotationElment = null;
                if (AnnotationClass.IsSingleProperty && !string.IsNullOrEmpty(AnnotationClass.PropertyName))
                {
                     pAnnotationElment = AnnotationClass.GetSymbolNode(xmlDoc);
                }
                //开始解析渲染符号信息
                for (int i = 0; i < SymbolList.Count; i++)
                {
                    XmlElement pRuleElement = default(XmlElement);
                    ptSymbolClass pSymbolClass = SymbolList[i];
                    //生成Rule节点信息
                    pRuleElement = CommXmlHandle.CreateElement("Rule", xmlDoc);
                    pRuleElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("RuleName", xmlDoc, string.IsNullOrEmpty(pSymbolClass.Label)?string.Format("rule{0}",i):pSymbolClass.Label));
                    pRuleElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("Title", xmlDoc, string.IsNullOrEmpty(pSymbolClass.Label) ? string.Format("rule{0}", i) : pSymbolClass.Label));
                    //设置显示比例尺
                    if (!double.IsNaN(m_ptLayer.m_MaxScale) && !double.IsNaN(m_ptLayer.m_MinScale))
                    {
                        pRuleElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("MinScale", xmlDoc, m_ptLayer.m_MaxScale.ToString()));
                        pRuleElement.AppendChild(CommXmlHandle.CreateElementAndSetElemnetText("MaxScale", xmlDoc, m_ptLayer.m_MinScale.ToString()));
                    }
                    //获取符号节点
                    IList<XmlElement> pSymbolizedNode = pSymbolClass.GetSymbolNode(xmlDoc);
                    foreach (XmlElement pElement in pSymbolizedNode)
                    {
                        pRuleElement.AppendChild(pElement);
                    }
                    if (pAnnotationElment != null)
                    {
                        pRuleElement.AppendChild(pAnnotationElment);
                    }
                    RootXmlElement.AppendChild(pRuleElement);
                }
            }
            catch (Exception ex)
            {
                ptLogManager.WriteMessage(string.Format("解析符号信息失败:{0}{1}{2}{3}", Environment.NewLine, ex.Message, Environment.NewLine, ex.StackTrace));
            }
            return RootXmlElement;
        }
    }
}
