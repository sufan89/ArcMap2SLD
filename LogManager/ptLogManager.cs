using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ArcGIS_SLD_Converter
{
    /// <summary>
    /// 记录日志信息
    /// </summary>
    public static class ptLogManager
    {

        public static void WriteMessage(string strMessage)
        {
            if (!string.IsNullOrEmpty(strMessage))
            {
                if (m_fs == null) return;
                if (m_fs.CanWrite)
                {
                    strMessage = string.Format("{0}{1}", Environment.NewLine, strMessage);
                    byte[] data = System.Text.Encoding.Default.GetBytes(strMessage);
                    m_fs.Write(data, 0, data.Length);
                    m_fs.Flush();
                }
            }
        }
        private static FileStream m_fs = null;
        public static void Create_LogFile(string strFileName)
        {
            try
            {
                if (string.IsNullOrEmpty(strFileName) || File.Exists(strFileName)) return;
                if (m_fs == null)
                {
                    m_fs = new FileStream(strFileName, FileMode.Create);
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(string.Format("无法创建日志文件:{0}",e.Message));
                return;
            }
        }
      
    }
}
