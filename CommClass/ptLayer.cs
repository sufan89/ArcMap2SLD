using ESRI.ArcGIS.Carto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArcGIS_SLD_Converter
{
   public class ptLayer
    {
        public ptLayer()
        {

        }
        /// <summary>
        /// 渲染方式
        /// </summary>
        public ptRender m_LayerRender
        {
            get; set;
        }
        /// <summary>
        /// 最小比例尺
        /// </summary>
        public double m_MinScale
        {
            get; set;
        }
        /// <summary>
        /// 最大比例尺
        /// </summary>
        public double m_MaxScale
        {
            get; set;
        }
        /// <summary>
        /// 图层名称
        /// </summary>
        public string m_LayerName
        {
            get; set;
        }
        /// <summary>
        /// 根据图层初始化基本信息
        /// </summary>
        /// <param name="player"></param>
        public void InitailGeneralInfo(ILayer player)
        {
            if (player == null) return;
            try
            {
                m_LayerName = player.Name;
                m_MaxScale = player.MaximumScale;
                m_MinScale = player.MinimumScale;
            }
            catch (Exception ex)
            {

            }
        }
    }
}
