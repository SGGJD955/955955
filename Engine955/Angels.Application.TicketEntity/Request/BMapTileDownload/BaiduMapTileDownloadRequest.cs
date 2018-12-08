using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Angels.Application.TicketEntity.Request.BMapTileDownload
{
    /// <summary>
    /// 百度瓦片地图个性化下载请求参数
    /// </summary>
    public class BaiduMapTileDownloadRequest
    {
        /// <summary>
        /// 请求唯一ID
        /// </summary>
        public string GUID { get; set; }
        /// <summary>
        /// 左坐标
        /// </summary>
        public double LefttextBox { get; set; }
        /// <summary>
        /// 底部坐标
        /// </summary>
        public double BottomtextBox { get; set; }
        /// <summary>
        /// 右部坐标
        /// </summary>
        public double RighttextBox { get; set; }
        /// <summary>
        /// 头部坐标
        /// </summary>
        public double UptextBox { get; set; }

        /// <summary>
        /// 保存路径
        /// </summary>
        public string SavePathText { get; set; }
        /// <summary>
        /// 地图层级列表数组
        /// </summary>
        public int[] LevelList { get; set; }

        ///// <summary>
        ///// 选中的图层节点信息
        ///// </summary>
        //public List<TreeNodeRequest> List_TreeNode { get; set; }

        /// <summary>
        /// 图层信息拼接好的字符串
        /// </summary>
        public string LayerStr { get; set; }

        /// <summary>
        /// 并行线程数
        /// </summary>
        public int TaskCount { get; set; }
        /// <summary>
        /// 任务名称
        /// </summary>
        public string TName { get; set; }
        /// <summary>
        /// 接口链接
        /// </summary>
        public string URL { get; set; }

    }

    /// <summary>
    /// 选中的节点信息（请求参数）
    /// </summary>
    public class TreeNodeRequest
    {
        /// <summary>
        /// 层级
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 父级节点
        /// </summary>
        public TreeNodeRequest Parent { get; set; }

        /// <summary>
        /// 节点文本信息
        /// </summary>
        public string Text { get; set; }
    }

}
