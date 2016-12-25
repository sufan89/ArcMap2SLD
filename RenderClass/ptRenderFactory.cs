using ESRI.ArcGIS.Carto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGIS_SLD_Converter
{
   public class ptRenderFactory
    {
        /// <summary>
        /// 渲染工厂
        /// </summary>
        /// <param name="pFeatureRender"></param>
        /// <param name="pFeatureLayer"></param>
        public ptRenderFactory(IFeatureRenderer pFeatureRender, ILayer pFeatureLayer)
        {
            m_FeatureRender = pFeatureRender;
            m_FeatureLayer = pFeatureLayer;
        }
        /// <summary>
        /// 要素渲染方式
        /// </summary>
        private IFeatureRenderer m_FeatureRender;
        /// <summary>
        /// 要素图层
        /// </summary>
        private ILayer m_FeatureLayer;
        /// <summary>
        /// 渲染解析
        /// </summary>
        /// <returns></returns>
        public ptLayer GetRenderLayer()
        {
            ptLayer pLayer=new ptLayer();
            if (m_FeatureRender == null && m_FeatureLayer == null) return pLayer;
            //初始化图层基本信息
            pLayer.InitailGeneralInfo(m_FeatureLayer);
            //解析渲染方式
            ptRender pLayerRender=null;
            if (m_FeatureRender is IUniqueValueRenderer)
            {
                pLayerRender = new ptUniqueValueRendererClass(m_FeatureRender, m_FeatureLayer as IFeatureLayer);
            }
            else if (m_FeatureRender is ISimpleRenderer)
            {
                pLayerRender = new ptSimpleRendererClass(m_FeatureRender, m_FeatureLayer as IFeatureLayer);

            }
            else if (m_FeatureRender is IClassBreaksRenderer)
            {
                pLayerRender = new ptClassBreaksRendererCalss(m_FeatureRender, m_FeatureLayer as IFeatureLayer);
            }
            pLayer.m_LayerRender = pLayerRender;
            return pLayer;
        }


    }
}
