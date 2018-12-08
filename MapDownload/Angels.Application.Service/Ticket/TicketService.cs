using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Drawing;
using System.Threading;

//using Angels.Application.IService;
using Angels.Application.TicketEntity;

using Angels.Application.TicketEntity.Request.BMapTileDownload;
using Angels.Application.TicketEntity.Request.BuildVRT;
using Angels.Application.TicketEntity.Request.ImageMosaic;
using Angels.Application.TicketEntity.Request.MapFeatureDownload;
using Angels.Application.TicketEntity.Request.WebmapDownloader;
using Angels.Application.TicketEntity.Request.Log;
using Angels.Application.TicketEntity.Common;

using Angels.Common;
using MySql.Data.MySqlClient;
using System.Data;

namespace Angels.Application.Service
{
    public class TicketService : ServiceStack.Service//, ITicketService
    {

        #region 百度瓦片地图个性化下载

        /// <summary>
        /// 百度瓦片地图个性化下载
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public WebApiResult<string> Any(BaiduMapTileDownloadRequest request)
        {
            BMapTileDownloadService service = new BMapTileDownloadService();

            return service.BaiduMapTileDownload1(request);
        }

        #endregion

        #region 生成VRT

        /// <summary>
        /// 生成VRT
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public WebApiResult<string> Any(BuildVRTByTileRequest request)
        {
            BuildVRTService service = new BuildVRTService();

            return service.BuildVRTByTile(request);
        }

        /// <summary>
        /// 索引图转RGB图，生存VRT时需要读取瓦片地图RBG图层信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public WebApiResult<string> Any(IndexToRGBRequest request)
        {
            BuildVRTService service = new BuildVRTService();

            return service.IndexToRGB(request);
        }

        #endregion

        #region 瓦片拼接

        /// <summary>
        /// 百度瓦片地图拼接
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public WebApiResult<string> Any(BaiduTileSplicingResult request)
        {
            ImageMosaicService service = new ImageMosaicService();

            return service.BaiduTileSplicing(request);
        }

        /// <summary>
        /// 高德瓦片地图拼接
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public WebApiResult<string> Any(GaodeTileSplicingResult request)
        {
            ImageMosaicService service = new ImageMosaicService();

            return service.GaodeTileSplicing(request);
        }

        /// <summary>
        /// 高德瓦片地图为索引图，需要先转化为RGB图像，并进行二值化
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public WebApiResult<string> Any(GaodeErzhihuaRequest request)
        {
            ImageMosaicService service = new ImageMosaicService();

            return service.GaodeErzhihua(request);
        } 

        #endregion

        #region 地图矢量要素下载

        /// <summary>
        /// 百度矢量要素下载
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public WebApiResult<string> Any(BaiduMapFeatureDownloadResult request)
        {
            MapFeatureDownloadService service = new MapFeatureDownloadService();

            return service.BaiduDownload(request);
        }


        /// <summary>
        /// 高德矢量要素下载
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public WebApiResult<string> Any(GaodeMapFeatureDownloadResult request)
        {
            MapFeatureDownloadService service = new MapFeatureDownloadService();

            return service.GaodeDownload(request);
        }  

        #endregion

        #region 网络地图下载

        /// <summary>
        /// 图吧POI
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public WebApiResult<string> Any(Mapbar_POIRequest request)
        {
            WebmapDownloaderService service = new WebmapDownloaderService();

            return service.Mapbar_POI_MenuItem(request);
        }

        /// <summary>
        /// 百度POI
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public WebApiResult<string> Any(Baidumap_POIRequest request)
        {
            WebmapDownloaderService service = new WebmapDownloaderService();

            return service.Baidumap_POI_MenuItem(request);
        }

        /// <summary>
        /// 百度道路瓦片
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public WebApiResult<string> Any(Baidumap_TileRequest request)
        {
            WebmapDownloaderService service = new WebmapDownloaderService();

            return service.Baidumap_Tile_MenuItem(request);
        }

        /// <summary>
        /// 百度建筑瓦片
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public WebApiResult<string> Any(Baidumap_BuildingTileRequest request)
        {
            WebmapDownloaderService service = new WebmapDownloaderService();

            return service.Baidumap_BuildingTile_MenuItem(request);
        }

        /// <summary>
        /// 高德POI
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public WebApiResult<string> Any(Amap_POIRequest request)
        {
            WebmapDownloaderService service = new WebmapDownloaderService();

            return service.Amap_POI_MenuItem(request);
        }

        /// <summary>
        /// 高德瓦片
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public WebApiResult<string> Any(Amap_TileRequest request)
        {
            WebmapDownloaderService service = new WebmapDownloaderService();

            return service.Amap_Tile_MenuItem(request);
        }

        #endregion

        #region 日志功能

        /// <summary>
        /// 日志详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public WebApiResult<string> Any(LogReadRequest request)
        {
            LogService service = new LogService();

            return service.GetList(request);
        }

        /// <summary>
        /// 线程日志详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public WebApiResult<string> Any(ThreadLogReadRequest request)
        {
            LogService service = new LogService();

            return service.GetThreadList(request);
        }

        /// <summary>
        /// 日志查询
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public WebApiResult<string> Any(LogSelectRequest request)
        {
            LogService service = new LogService();

            return service.GetStatusList(request);
        }

        /// <summary>
        /// 日志删除
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public WebApiResult<string> Any(LogDeleteRequest request)
        {
            LogService service = new LogService();

            return service.Delete(request);
        }
        
        #endregion

        /// <summary>
        /// 删除指定票据（测试）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool POST(TicketDeleteRequest deleteid)
        {
            return true;
        }

        /// <summary>
        /// 查询进度
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public WebApiResult<string> Any(ScheduleDataRequest request)
        {
            TaskService service = new TaskService();

            return service.GetSchedule(request);
        }

        /// <summary>
        /// 暂停
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public WebApiResult<string> Any(TaskStop request)
        {
            TaskService service = new TaskService();

            return service.Pause<BaiduMapTileDownloadRequest>(request);
        }
        
        /// <summary>
        /// 文件删除
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public WebApiResult<string> Any(FileDeleteRequest request)
        {
            LogService service = new LogService();

            return service.FileDelete(request);
        }

        public WebApiResult<string> Any(OpenFolderRequest request)
        {
            try
            {
                System.Diagnostics.Process.Start(request.Path);//"ExpLore", 
                return new WebApiResult<string>() { success = 1 };
            }
            catch (Exception ex)
            {
                return new WebApiResult<string>() { success = 0, msg = "错误： " + ex.ToString() };
            }
        }

        ///// <summary>
        ///// 导入 城市-ID对
        ///// </summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //public WebApiResult<string> Any(ImportCityIdRequest request)
        //{
        //    if (request.City == null || request.Id == null)
        //        return null;
        //    Angels.Application.Data.MySqlHelper mysqlHelper = new Angels.Application.Data.MySqlHelper(Config.GetValue("DataMySql"));
        //    //String ConnectionString = String.Format("server={0};user id={1};password={2};database={3};CharSet=utf8", "localhost", "root", "123456", "webmap");
        //    //mysqlHelper = new Angels.Application.Data.MySqlHelper(ConnectionString);
        //    try
        //    {
        //        List<MySqlParameter> listParam = new List<MySqlParameter>();
        //        listParam.Add(new MySqlParameter(@"Id", request.Id));
        //        listParam.Add(new MySqlParameter(@"Name", request.City));
        //        listParam.Add(new MySqlParameter(@"pId", request.pId));

        //        if (request.maptype == 0)//百度
        //        {
        //            String sqlcommand = "insert into baidu_cityid(baiduId, Name,baidupId) values(@Id, @Name,@pId)"; ;

        //            var a = mysqlHelper.ExecuteDataTable("SELECT * FROM baidu_cityid WHERE Name = " + "'" + request.City + "'");
        //            if (a.Rows.Count != 0)
        //            {
        //                sqlcommand = "update baidu_cityid SET baiduId=@Id,baidupId=@pId WHERE Name=@Name";
        //            }
        //            mysqlHelper.ExecuteNonQuery(sqlcommand, listParam.ToArray());
        //            return new WebApiResult<string>() { success = 1, msg = "更新成功" };
        //        }

        //        else if (request.maptype == 1)//高德
        //        {
        //            string sqlcommand = "insert into gaode_cityid(gaodeId, Name,gaodepId) values(@Id, @Name,@pId)";

        //            var a = mysqlHelper.ExecuteDataTable("SELECT * FROM gaode_cityid WHERE Name = " + "'" + request.City + "'");
        //            if (a.Rows.Count != 0)
        //            {
        //                sqlcommand = "update gaode_cityid SET gaodeId=@Id,gaodepId=@pId WHERE Name=@Name";
        //            }
        //            mysqlHelper.ExecuteNonQuery(sqlcommand, listParam.ToArray());
        //            return new WebApiResult<string>() { success = 1, msg = "更新成功" };
        //        }
        //        else
        //            return new WebApiResult<string>() { success = 0, msg = "输入参数错误"};
        //    }
        //    catch (Exception ex)
        //    {
        //        return new WebApiResult<string>() { success = 0, msg = "错误： " + ex.ToString() };
        //    }
        //}

        ///// <summary>
        ///// 导入 城市-拼音对
        ///// </summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //public WebApiResult<string> Any(ImportCityNameRequest request)
        //{
        //    if (request.City == null || request.Name == null)
        //        return null;
        //    Angels.Application.Data.MySqlHelper mysqlHelper = new Angels.Application.Data.MySqlHelper(Config.GetValue("DataMySql"));
        //    //String ConnectionString = String.Format("server={0};user id={1};password={2};database={3};CharSet=utf8", "localhost", "root", "123456", "webmap");
        //    //mysqlHelper = new Angels.Application.Data.MySqlHelper(ConnectionString);
        //    try
        //    {
        //        var a = mysqlHelper.ExecuteDataTable("SELECT * FROM mapbar_cityname WHERE City = " + "'" + request.City + "'");

        //        List<MySqlParameter> listParam = new List<MySqlParameter>();
        //        listParam.Add(new MySqlParameter(@"City", request.City));
        //        listParam.Add(new MySqlParameter(@"Name", request.Name));
        //        if (a.Rows.Count == 0)
        //        {                    
        //            //插入数据库
        //            String sqlcommand = "insert into mapbar_cityname(City, Name) values(@City, @Name)";
        //            mysqlHelper.ExecuteNonQuery(sqlcommand, listParam.ToArray());
        //        }
        //        else
        //        {
        //            //更新数据库
        //            String sqlcommand = "update mapbar_cityname SET Name=@Name WHERE City=@City";
        //            mysqlHelper.ExecuteNonQuery(sqlcommand, listParam.ToArray());
        //        }

        //        return new WebApiResult<string>() { success = 1, msg = "更新成功" };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new WebApiResult<string>() { success = 0, msg = "错误： " + ex.ToString() };
        //    }
        //}

        /// <summary>
        /// 导入城市ID、拼音信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public WebApiResult<string> Any(ImportCityInformRequest request)
        {
            if (request.Name == null || request.baiduId == null || request.baidupId == null || request.gaodeId == null || request.pinyin  == null )
                return new WebApiResult<string>() { success = 0, msg = "参数不全"  }; 

            Angels.Application.Data.MySqlHelper mysqlHelper = new Angels.Application.Data.MySqlHelper(Config.GetValue("DataMySql"));
            //String ConnectionString = String.Format("server={0};user id={1};password={2};database={3};CharSet=utf8", "localhost", "root", "123456", "webmap");
            //mysqlHelper = new Angels.Application.Data.MySqlHelper(ConnectionString);
            try
            {
                DataTable dt = mysqlHelper.ExecuteDataTable("SELECT * FROM cityinform WHERE Name = " + "'" + request.Name + "'");

                List<MySqlParameter> listParam = new List<MySqlParameter>();
                listParam.Add(new MySqlParameter(@"Name", request.Name));
                listParam.Add(new MySqlParameter(@"baiduId", request.baiduId));
                listParam.Add(new MySqlParameter(@"baidupId", request.baidupId));
                listParam.Add(new MySqlParameter(@"gaodeId", request.gaodeId));
                listParam.Add(new MySqlParameter(@"pinyin", request.pinyin));
                if (dt.Rows.Count == 0)
                {
                    //插入数据库
                    String sqlcommand = "insert into cityinform(Name, baiduId, baidupId, gaodeId, pinyin) values(@Name, @baiduId, @baidupId, @gaodeId, @pinyin)";
                    mysqlHelper.ExecuteNonQuery(sqlcommand, listParam.ToArray());
                }
                else
                {
                    //更新数据库
                    String sqlcommand = "update cityinform SET baiduId=@baiduId,baidupId=@baidupId,gaodeId=@gaodeId,pinyin=@pinyin WHERE Name=@Name";
                    mysqlHelper.ExecuteNonQuery(sqlcommand, listParam.ToArray());
                }

                return new WebApiResult<string>() { success = 1, msg = "更新成功" };
            }
            catch (Exception ex)
            {
                return new WebApiResult<string>() { success = 0, msg = "错误： " + ex.ToString() };
            }
        }

        /// <summary>
        /// 查询城市ID、拼音信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public WebApiResult<string> Any(GetCityInformRequest request)
        {
            Angels.Application.Data.MySqlHelper mysqlHelper = new Angels.Application.Data.MySqlHelper(Config.GetValue("DataMySql"));
            //String ConnectionString = String.Format("server={0};user id={1};password={2};database={3};CharSet=utf8", "localhost", "root", "123456", "webmap");
            //mysqlHelper = new Angels.Application.Data.MySqlHelper(ConnectionString);
            try
            {
                DataTable dt = mysqlHelper.ExecuteDataTable("SELECT * FROM cityinform");

                //DataTable dt1 = mysqlHelper.ExecuteDataTable("SELECT * FROM baidu_cityid LEFT JOIN gaode_cityid ON baidu_cityid.Name=gaode_cityid.Name");
                //DataTable dt2 = mysqlHelper.ExecuteDataTable("SELECT * FROM baidu_cityid RIGHT JOIN gaode_cityid ON baidu_cityid.Name=gaode_cityid.Name");
                //if(dt1.Rows.Count==0&&dt2.Rows.Count==0) return new WebApiResult<string>() { success = 1, msg = "[]" };

                ////获取两个数据源的并集
                //IEnumerable<DataRow> query2 = dt1.AsEnumerable().Union(dt2.AsEnumerable(), DataRowComparer.Default);
                ////两个数据源的并集集合
                //DataTable dt = query2.CopyToDataTable();

                //dt.Columns.Add("pinyin");              

                //for (int i = 0; i < dt.Rows.Count; i++)
                //{
                //    //var a = mysqlHelper.ExecuteDataTable("SELECT * FROM gaode_cityid WHERE Name = " + "'" + dt.Rows[i]["Name"] + "'");
                //    //if (a.Rows.Count != 0)
                //    //{
                //    //    dt.Rows[i]["gaodeId"] = a.Rows[0]["gaodeId"];
                //    //    dt.Rows[i]["gaodepId"] = a.Rows[0]["gaodepId"];
                //    //}
                //    //else
                //    //    dt.Rows[i]["gaodeId"] = "";

                //    var a = mysqlHelper.ExecuteDataTable("SELECT * FROM mapbar_cityname WHERE City = " + "'" + dt.Rows[i]["Name"] + "'");
                //    if (a.Rows.Count != 0)
                //        dt.Rows[i]["pinyin"] =a.Rows[0]["Name"];
                //}

                return new WebApiResult<string>() { success = 1, msg = dt.ToJson() };
            }
            catch (Exception ex)
            {
                return new WebApiResult<string>() { success = 0, msg = "错误： " + ex.ToString() };
            }
        }

        /// <summary>
        /// 查询城市-拼音信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public WebApiResult<string> Any(GetCityNameRequest request)
        {
            Angels.Application.Data.MySqlHelper mysqlHelper = new Angels.Application.Data.MySqlHelper(Config.GetValue("DataMySql"));
            //String ConnectionString = String.Format("server={0};user id={1};password={2};database={3};CharSet=utf8", "localhost", "root", "123456", "webmap");
            //mysqlHelper = new Angels.Application.Data.MySqlHelper(ConnectionString);
            try
            {
                DataTable dt = mysqlHelper.ExecuteDataTable("SELECT Name,pinyin FROM cityinform");

                return new WebApiResult<string>() { success = 1, msg = dt.ToJson() };
            }
            catch (Exception ex)
            {
                return new WebApiResult<string>() { success = 0, msg = "错误： " + ex.ToString() };
            }
        }

        /// <summary>
        /// 删除城市ID、拼音信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public WebApiResult<string> Any(DeleteCityInformRequest request)
        {            
            //string[] Ids = request.Ids;
            try
            {
                //    foreach (string Id in Ids)
                //    {
                //        if (Id == "0") continue;
                //        Angels.Application.Data.MySqlHelper mysqlHelper = new Angels.Application.Data.MySqlHelper(Config.GetValue("DataMySql"));
                //        //String ConnectionString = String.Format("server={0};user id={1};password={2};database={3};CharSet=utf8", "localhost", "root", "123456", "webmap");
                //        //mysqlHelper = new Angels.Application.Data.MySqlHelper(ConnectionString); 

                //        if (request.type == 0)
                //        {
                //            int a = mysqlHelper.ExecuteNonQuery("DELETE FROM baidu_cityid WHERE baiduId=@Id", new MySqlParameter(@"Id", int.Parse(Id)));
                //        }
                //        else if (request.type == 1)
                //        {
                //            int a = mysqlHelper.ExecuteNonQuery("DELETE FROM gaode_cityid WHERE gaodeId=@Id", new MySqlParameter(@"Id", int.Parse(Id)));
                //        }
                //        else if (request.type == 2)
                //        {
                //            int a = mysqlHelper.ExecuteNonQuery("DELETE FROM mapbar_cityname WHERE City=@Id", new MySqlParameter(@"Id", Id));//城市中文名
                //        }
                //        else
                //            return new WebApiResult<string>() { success = 0, msg = "输入参数错误" };

                //    }

                foreach(string Name in request.Names)
                {
                    Angels.Application.Data.MySqlHelper mysqlHelper = new Angels.Application.Data.MySqlHelper(Config.GetValue("DataMySql"));
                    //String ConnectionString = String.Format("server={0};user id={1};password={2};database={3};CharSet=utf8", "localhost", "root", "123456", "webmap");
                    //mysqlHelper = new Angels.Application.Data.MySqlHelper(ConnectionString); 

                    int a = mysqlHelper.ExecuteNonQuery("DELETE FROM cityinform WHERE Name=@Name", new MySqlParameter(@"Name", Name));
                }

                return new WebApiResult<string>() { success = 1, msg = "删除成功" };
            }
            catch (Exception ex)
            {
                return new WebApiResult<string>() { success = 0, msg = "错误： " + ex.ToString() };
            }

        }

        public string Any(Hello request)
        {
            return "Hello";
        }

    }
}
