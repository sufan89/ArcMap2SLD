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
        /// 是否有标记
        /// </summary>
        public bool m_isLabel { get; set; }
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
        public ShapeType m_LayerShapeType { get; set; }

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
                m_LayerShapeType = ShapeType.None;
                if (player is IFeatureLayer)
                {
                    IFeatureLayer pFeatureLayer = player as IFeatureLayer;
                    switch (pFeatureLayer.FeatureClass.ShapeType)
                    {
                        case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryLine:
                        case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolyline:
                            m_LayerShapeType = ShapeType.Line;
                            break;
                        case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPoint:
                        case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryMultipoint:
                            m_LayerShapeType = ShapeType.Point;
                            break;
                        case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon:
                            m_LayerShapeType = ShapeType.Polygon;
                            break;
                        default:
                            m_LayerShapeType = ShapeType.None;
                            break;
                    }
                    IGeoFeatureLayer pGeoFeatureLayer = player as IGeoFeatureLayer;
                    m_isLabel = pGeoFeatureLayer.DisplayAnnotation;
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
    /// <summary>
    /// 图形类型
    /// </summary>
    public enum ShapeType
    {
        Point=1,
        Line=2,
        Polygon=3,
        None=4
    }
}
