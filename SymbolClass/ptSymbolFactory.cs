using ESRI.ArcGIS.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGIS_SLD_Converter
{
   public class ptSymbolFactory
    {
       /// <summary>
       /// 符号工厂
       /// </summary>
       /// <param name="pSymbol"></param>
        public ptSymbolFactory(ISymbol pSymbol)
        {
            m_pSymbol = pSymbol;
        }
        private ISymbol m_pSymbol;
        /// <summary>
        /// 解析符号信息
        /// </summary>
        /// <returns></returns>
        public ptSymbolClass GetSymbolClass(string Label, IList<string> Fieldvalues, double UpperLimit, double LowerLimit)
        {
            ptSymbolClass pSymbolClass = null;
            if (m_pSymbol == null) return null;
            //点符号
            if (m_pSymbol is IMarkerSymbol)
            {
                pSymbolClass = GetMarkerSymbolClass(m_pSymbol);
            }
            else if (m_pSymbol is ILineSymbol)
            {
                pSymbolClass = GetLineSymbolClass(m_pSymbol);
            }
            else if (m_pSymbol is IFillSymbol)
            {
                pSymbolClass = GetFillSymbolClass(m_pSymbol);
            }
            else if (m_pSymbol is I3DChartSymbol)
            {
                pSymbolClass = GetIIIDChartSymbolClass(m_pSymbol );
            }
            else if (m_pSymbol is ITextSymbol)
            {
                pSymbolClass = new TextSymbolClass(m_pSymbol);
            }
            pSymbolClass.Label = Label;
            pSymbolClass.Fieldvalues = Fieldvalues;
            pSymbolClass.UpperLimit = UpperLimit;
            pSymbolClass.LowerLimit = LowerLimit;
            return pSymbolClass;
        }
        /// <summary>
        /// 获取标记符号
        /// </summary>
        /// <param name="Symbol"></param>
        /// <returns></returns>
        private ptSymbolClass GetMarkerSymbolClass(ISymbol Symbol)
        {
            ptSymbolClass tempSymbol = null ;
            if (Symbol is ISimpleMarkerSymbol)
            {
                tempSymbol = new ptSimpleMarkerSymbolClass(Symbol);
                return tempSymbol;
            }
            else if (Symbol is ICartographicMarkerSymbol)
            {
                ICartographicMarkerSymbol ICMS = Symbol as ICartographicMarkerSymbol;
                if (ICMS is ICharacterMarkerSymbol)
                {
                    tempSymbol = new ptCharacterMarkerSymbolClass(Symbol);
                    return tempSymbol;
                }
                else if (ICMS is IPictureMarkerSymbol)
                {
                    tempSymbol = new ptPictureMarkerSymbolClass(Symbol);
                    return tempSymbol;
                }
            }
            else if (Symbol is IArrowMarkerSymbol)
            {
                tempSymbol = new ptArrowMarkerSymbolClass(Symbol);
                return tempSymbol;
            }
            else if (Symbol is IMultiLayerMarkerSymbol)
            {
                tempSymbol = new ptMultilayerMarkerSymbolClass(Symbol);
                return tempSymbol;
            }
            else
            {
                return tempSymbol;
            }
            return tempSymbol;
        }
        /// <summary>
        /// 获取线符号
        /// </summary>
        /// <param name="Symbol"></param>
        /// <returns></returns>
        private ptSymbolClass GetLineSymbolClass(ISymbol Symbol)
        {
           ptSymbolClass pSymbolClass=null;

            if (Symbol is ICartographicLineSymbol)
            {
                ICartographicLineSymbol ICLS = Symbol as ICartographicLineSymbol;
                if (ICLS is IHashLineSymbol)
                {
                    pSymbolClass = new ptHashLineSymbolClass(Symbol);
                    return pSymbolClass;
                }
                else if (ICLS is IMarkerLineSymbol)
                {
                    pSymbolClass = new ptMarkerLineSymbolClass(Symbol);
                    return pSymbolClass;
                }
                else 
                {
                    pSymbolClass = new ptCartographicLineSymbol(Symbol);
                    return pSymbolClass;
                }
            }
            else if (Symbol is ISimpleLineSymbol)
            {
                pSymbolClass = new ptSimpleLineSymbolClass(Symbol);
                return pSymbolClass;
            }
            else if (Symbol is IPictureLineSymbol)
            {
                pSymbolClass = new ptPictureLineSymbolClass(Symbol);
                return pSymbolClass;
            }
            else if (Symbol is IMultiLayerLineSymbol)
            {
                pSymbolClass = new ptMultilayerLineSymbolClass(Symbol);
                return pSymbolClass;
            }
            return pSymbolClass;
        }
        /// <summary>
        /// 获取填充符号
        /// </summary>
        /// <param name="Symbol"></param>
        /// <returns></returns>
        private ptSymbolClass GetFillSymbolClass(ISymbol Symbol)
        {
            ptSymbolClass pSymbolClass = null;
            if (Symbol is ISimpleFillSymbol)
            {
                pSymbolClass = new ptSimpleFillSymbolClass(Symbol);
                return pSymbolClass;
            }
            else if (Symbol is IMarkerFillSymbol)
            {
                pSymbolClass = new ptMarkerFillSymbolClass(Symbol);
                return pSymbolClass;
            }
            else if (Symbol is ILineFillSymbol)
            {
                pSymbolClass = new ptLineFillSymbolClass(Symbol);
                return pSymbolClass;
            }
            else if (Symbol is IDotDensityFillSymbol)
            {
                pSymbolClass = new ptDotDensityFillSymbolClass(Symbol);
                return pSymbolClass;
            }
            else if (Symbol is IPictureFillSymbol)
            {
                pSymbolClass = new ptPictureFillSymbolClass(Symbol);
                return pSymbolClass;
            }
            else if (Symbol is IGradientFillSymbol)
            {
                pSymbolClass = new ptGradientFillSymbolClass(Symbol);
                return pSymbolClass;
            }
            else if (Symbol is IMultiLayerFillSymbol)
            {
                pSymbolClass = new ptMultilayerFillSymbolClass(Symbol);
                return pSymbolClass;
            }
            return pSymbolClass;
        }
        /// <summary>
        /// 获取3D图标符号
        /// </summary>
        /// <param name="Symbol"></param>
        /// <returns></returns>
        private ptSymbolClass GetIIIDChartSymbolClass(ISymbol Symbol)
        {
            ptSymbolClass pSymbolClass = null;
            if (Symbol is IBarChartSymbol)
            {
                pSymbolClass =new ptBarChartSymbolClass(Symbol);
                return pSymbolClass;
            }
            else if (Symbol is IPieChartSymbol)
            {
                pSymbolClass = new ptPieChartSymbolClass(Symbol);
                return pSymbolClass;
            }
            else if (Symbol is IStackedChartSymbol)
            {
                pSymbolClass = new ptStackedChartSymbolClass(Symbol);
                return pSymbolClass;
            }
            return pSymbolClass;
        }
    }
}
