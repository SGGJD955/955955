using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Net;
using System.Threading;
using System.IO;
using System.Data;
using System.Web;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Angels.Application.TicketEntity;
using Angels.Application.TicketEntity.Request.MapFeatureDownload;
using Angels.Common;
using Angels.Application.TicketEntity.Common;

using OSGeo.OGR;

namespace Angels.Application.Service
{
    /// <summary>
    /// 地图矢量要素下载
    /// </summary>
    public class MapFeatureDownloadService
    {
        private readonly static object obj = new object();
        TaskLogEntity log;
        ThreadTaskLogEntity threadlog;
        List<int[]> current_loc;
        int current = 0;
        CountdownEvent countdown;
        int N;

        XmlDocument xmlDoc = new XmlDocument();
        Layer oLayer;

        string MapFeatureConfig = System.AppDomain.CurrentDomain.BaseDirectory + "XmlConfig\\MapFeature.config";// HttpContext.Current.Server.MapPath("/XmlConfig/MapFeature.config");

        //数据库辅助类
        Angels.Application.Data.MySqlHelper mysqlHelper;

        public MapFeatureDownloadService()
        {
            mysqlHelper = new Angels.Application.Data.MySqlHelper(Config.GetValue("DataMySql"));
            //String ConnectionString = String.Format("server={0};user id={1};password={2};database={3};CharSet=utf8", "localhost", "root", "123456", "webmap");
            //mysqlHelper = new Angels.Application.Data.MySqlHelper(ConnectionString);
        }

        BaiduMapFeatureDownloadResult Result_BFeature;
        /// <summary>
        /// 百度矢量要素下载(数据下载写入shp文件的时候可能出现问题，比如下载网络波动中断到时系统无法识别（解决办法先吧数据下载到数据在写入shp文件）)
        /// </summary>
        /// <param name="Result"></param>
        /// <returns></returns>
        public WebApiResult<string> BaiduDownload(BaiduMapFeatureDownloadResult Result)
        {
            this.countdown = new CountdownEvent(Result.TaskCount);
            this.Result_BFeature = Result;
            this.N = Result.TaskCount;

            UpdateLastLoc<BaiduMapFeatureDownloadResult>(Result.GUID);

            log = new TaskLogEntity() { GUID = Result.GUID, Name = Result.TName, Type = "矢量要素", Description = "BaiduDownload", Status = "进行中", Parameter = JsonHelper.ToJson(Result), SavePath =Result.SavePathText };
            //操作日志
            new Log<BaiduMapFeatureDownloadResult>(log);

            if (Result.LayerName == "道路")
            {
                xmlDoc.Load(MapFeatureConfig);

                ///////////////////////////////////////////////////////////////////////////////
                // GDAL创建shp文件
                // 注册所有的驱动
                Ogr.RegisterAll();
                // GDAL中文路径支持
                OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");
                // 属性表字段中文支持
                OSGeo.GDAL.Gdal.SetConfigOption("SHAPE_ENCODING", "CP936");
                // 设置坐标系存储文件夹gdal-data路径
                OSGeo.GDAL.Gdal.SetConfigOption("GDAL_DATA", System.Windows.Forms.Application.StartupPath + "\\gdal-data");
                Driver oDriver = Ogr.GetDriverByName("ESRI Shapefile");
                if (oDriver == null)
                {
                    return new WebApiResult<string>() { success = 0, msg = "GDAL驱动不可用，请检查！" };
                }
                // 创建数据源  
                DataSource oDS = oDriver.CreateDataSource(Result.SavePathText, null);
                if (oDS == null)
                {
                    return new WebApiResult<string>() { success = 0, msg = "创建数据源失败，请检查！" };
                }
                oLayer = oDS.GetLayerByName(Result.TName);
                if (oLayer == null)
                {
                    // 创建图层，创建一个多线图层，并指定空间参考
                    OSGeo.OSR.SpatialReference oSRS = new OSGeo.OSR.SpatialReference("PROJCS[\"WGS_1984_Pseudo_Mercator\",GEOGCS[\"GCS_WGS_1984\",DATUM[\"D_WGS_1984\",SPHEROID[\"WGS_1984\",6378137.0,298.257223563]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]],PROJECTION[\"Mercator\"],PARAMETER[\"false_easting\",0.0],PARAMETER[\"false_northing\",0.0],PARAMETER[\"central_meridian\",0.0],PARAMETER[\"standard_parallel_1\",0.0],UNIT[\"Meter\",1.0]]");
                    //OSGeo.OSR.SpatialReference oSRS = new OSGeo.OSR.SpatialReference("");
                    //oSRS.SetWellKnownGeogCS("EPSG:4326");
                    oLayer = oDS.CreateLayer(Result.TName, oSRS, wkbGeometryType.wkbMultiLineString, null);
                    if (oLayer == null)
                    {
                        return new WebApiResult<string>() { success = 0, msg = "创建图层失败，请检查！" };
                    }

                    // 创建属性表  
                    // 先创建一个叫Name的字符型属性，字符长度为100
                    FieldDefn oFieldName = new FieldDefn("Name", FieldType.OFTString);
                    oFieldName.SetWidth(100);
                    oLayer.CreateField(oFieldName, 1);
                    // 再创建一个叫Type的字符型属性，字符长度为50  
                    FieldDefn oFieldType = new FieldDefn("Type", FieldType.OFTString);
                    oFieldType.SetWidth(50);
                    oLayer.CreateField(oFieldType, 1);
                    // 再创建一个叫UID的字符型属性，字符长度为50  
                    FieldDefn oFieldUID = new FieldDefn("UID", FieldType.OFTString);
                    oFieldUID.SetWidth(50);
                    oLayer.CreateField(oFieldUID, 1);
                }                

                /////////////////////////////////////////////////////////////////////////////////
                threadlog = new ThreadTaskLogEntity() { GUID = Result.GUID, TaskLog_GUID = Result.GUID, Status = "进行中", TStatus = 1, TName=Result.TName, IsPaused = false, URL = Result.URL, Parameter = JsonHelper.ToJson(Result) };
                //计算Total并更新 
                threadlog.Total = GetList("SELECT KWName FROM mapbar_poi WHERE KWType = '道路'", "KWName").Count;
                new Log<BaiduMapFeatureDownloadResult>(threadlog);
                log.Count = threadlog.Total;
                new Log<BaiduMapFeatureDownloadResult>(log);

                Thread[] t = new Thread[Result.TaskCount];
                for (int num = 0; num < Result.TaskCount; num++)
                {
                    try
                    {
                        t[num] = new Thread(new ParameterizedThreadStart(run_BFeature))
                        {
                            Name = "Thread " + num.ToString()
                        };
                        t[num].Start(num);
                    }
                    catch (Exception ex)
                    {
                        threadlog.Status = "错误";
                        threadlog.ErrorMsg = ex.ToString();
                        threadlog.TStatus = 3;
                        new Log<BaiduMapFeatureDownloadResult>(threadlog);

                        log.Status = "错误";
                        log.ErrorMsg = ex.ToString();
                        log.ErrorDate = DateTime.Now.ToString();
                        //操作日志
                        new Log<BaiduMapFeatureDownloadResult>(log);

                        return new WebApiResult<string>() { success = 0, msg = ex.ToString() };
                    }
                }
                countdown.Wait();
                oLayer.Dispose();
                oDS.Dispose();
                for (int num = 0; num < Result.TaskCount; num++)
                {
                    t[num].Abort();
                }
                lock (obj)
                {
                    if (!Log<BaiduMapFeatureDownloadResult>.GetThreadLogEntity(this.Result_BFeature.GUID).IsPaused)
                    {

                        //配置文件参数信息更新
                        xmlDoc["MapFeature"]["bmap"]["dict_index"].InnerText = "0";
                        xmlDoc.Save(MapFeatureConfig);

                        log.Status = "已完成";
                        log.CompleteTime = DateTime.Now.ToString();
                        log.Current = log.Count;
                        threadlog.Status = "已完成";
                        threadlog.TStatus = 2;
                        threadlog.Current = threadlog.Total;
                        threadlog.Current_loc = List2Str(current_loc);
                        //操作日志
                        new Log<BaiduMapFeatureDownloadResult>(threadlog);
                        new Log<BaiduMapFeatureDownloadResult>(log);
                        return new WebApiResult<string>() { success = 1, msg = "百度地图要素下载完成！" };

                    }
                }
                
            }
            else
            {
                return new WebApiResult<string>() { success = 2, msg = "图层类型不符合" };
            }
            return null;
        }

        void run_BFeature(object num)
        {
            FeatureDefn oDefn = oLayer.GetLayerDefn();

            string city_id = GetList("SELECT baiduId FROM cityinform WHERE Name = " + "'" + this.Result_BFeature.City + "'", "baiduId")[0];
            var roadName = GetList("SELECT KWName FROM mapbar_poi WHERE KWType = '道路' and BaiduId = " + city_id, "KWName");
            int interval = (int)Math.Ceiling(roadName.Count / (double)N);
            int start = 0 + interval * (int)num;
            int end = start + interval;
            if (end > roadName.Count) end = roadName.Count;

            for (int i = start; i < end; i++)
            {
                if (i < current) continue;

                if (Log<BaiduMapFeatureDownloadResult>.GetThreadLogEntity(this.Result_BFeature.GUID).IsPaused)
                {
                    // 更新配置文件参数信息
                    xmlDoc["MapFeature"]["bmap"]["dict_index"].InnerText = current.ToString();
                    xmlDoc.Save(MapFeatureConfig);

                    threadlog.Current = current;
                    threadlog.Current_loc = List2Str(current_loc);
                    threadlog.Status = "暂停";
                    threadlog.TStatus = 4;
                    threadlog.IsPaused = true;
                    new Log<BaiduMapFeatureDownloadResult>(threadlog);

                    log.Status = "未完成";
                    log.Current = current;
                    new Log<BaiduMapFeatureDownloadResult>(log);
                    for (int k = 0; k < countdown.CurrentCount; k++)
                        countdown.Signal();
                    return;
                }

                //Application.DoEvents();

                // 根据道路名称向服务器请求矢量道路信息，并对返回JSON数据进行解析（取前10条记录进行解析）
                try
                {
                    lock (obj)
                    {
                        current++;
                        threadlog.Current = current;
                        new Log<BaiduMapFeatureDownloadResult>(threadlog);
                        if (Contains(current_loc, (int)num) >= 0)
                        {
                            var index = Contains(current_loc, (int)num);
                            current_loc[index][1] = i;
                        }
                        else
                        {
                            if (current_loc == null) current_loc = new List<int[]>();
                            current_loc.Add(new int[2] { (int)num, i });
                        }
                    }

                    //CacheHelper.SetCache(this.Result_BFeature.GUID, log, 7200);
                    // 状态栏更新
                    //CacheHelper.SetCache(this.Result_BFeature.GUID, "{\"MapDescription\":\"百度地图矢量道路下载中...\",\"Count\":\"" + roadName.Count + "\",\"Current\":\"" + current + "\"}", 180);

                    //string city_id = GetList("SELECT baiduId FROM cityinform WHERE Name = " + "'" + this.Result_BFeature.City + "'" , "baiduId")[0];
                    // 构建请求URL
                    //string curUrl = xmlDoc["MapFeature"]["bmap"]["url"].InnerText + "&qt=s&da_src=searchBox.button&wd=" + HttpUtility.UrlEncode(roadName[i].ToString() + "-道路") + "&c=" + xmlDoc["MapFeature"]["bmap"]["city"].InnerText + "&pn=0";
                    string curUrl = xmlDoc["MapFeature"]["bmap"]["url"].InnerText + "&qt=s&da_src=searchBox.button&wd=" + HttpUtility.UrlEncode(roadName[i].ToString() + "-道路") + "&c=" + city_id + "&pn=0";

                    // 向服务器发起Get请求
                    string curResponse = GetHttpResponse(curUrl);
                    // 返回结果字符串序列化为JSON
                    JObject jobject = (JObject)JsonConvert.DeserializeObject(curResponse);
                    // 解析JSON获取道路相关信息
                    // 获取返回记录的数量
                    int totalnum = 0;
                    if (jobject["result"]["total"] == null || jobject["result"]["total"].Type == JTokenType.Array)
                        continue;
                    else
                    {
                        totalnum = Convert.ToInt32(jobject["result"]["total"].ToString());
                        if (totalnum == 0 || jobject["content"] == null) continue;
                    }
                    //contents为搜索请求返回页面POI信息的集合，每个POI为一个content
                    JEnumerable<JToken> contents = jobject["content"].Children();
                    foreach (JToken content in contents)
                    {
                        //判断名称和UID是否正常
                        string name = GetInfotoString("name", content);
                        String uid = GetInfotoString("uid", content);
                        if (name == "" || uid == "") continue;
                        //筛选类别为道路的POI进行详细信息请求，并解析其中坐标串写入shp文件
                        String type = "";
                        JEnumerable<JToken> clas = content["cla"].Children();
                        foreach (JToken cla in clas)
                        {
                            type = type + cla[1].ToString();
                        }
                        if (type.Contains("道路"))
                        {
                            /////////////////////////////////////////////////////
                            Feature oFeature = new Feature(oDefn);
                            oFeature.SetField(0, name);
                            oFeature.SetField(1, type);
                            oFeature.SetField(2, uid);
                            ////////////////////////////////////////////////////
                            
                            // 构建道路矢量信息（坐标串）请求URL
                            string roadURL = xmlDoc["MapFeature"]["bmap"]["url"].InnerText + "&qt=ext&l=18&uid=" + uid + "&c=" + city_id;
                            // 向服务器发起Get请求
                            string roadResponse = GetHttpResponse(roadURL);
                            // 返回结果字符串序列化为JSON
                            JObject roadobject = (JObject)JsonConvert.DeserializeObject(roadResponse);
                            if (roadobject.ContainsKey("content") == false) continue;
                            //坐标串解析，并写入多线
                            string coordinates = roadobject["content"]["geo"].ToString();
                            string roadcoordinates = coordinates.Substring(coordinates.LastIndexOf("|") + 1);
                            string[] roadlist_coordinates = roadcoordinates.Split(';');
                            string strMultiline = "MULTILINESTRING(";
                            for (int j = 0; j < roadlist_coordinates.Length - 1; j++)
                            {
                                string strLine = "(";
                                string[] pointlist_coordinates = roadlist_coordinates[j].Split(',');
                                for (int t = 0; t < pointlist_coordinates.Length; t = t + 2)
                                {
                                    strLine = strLine + pointlist_coordinates[t] + " " + pointlist_coordinates[t + 1] + ",";
                                }
                                strLine = strLine.Substring(0, strLine.LastIndexOf(",")) + ")";
                                strMultiline = strMultiline + strLine + ",";
                            }
                            strMultiline = strMultiline.Substring(0, strMultiline.LastIndexOf(",")) + ")";
                            //将多线设置为要素的空间属性
                            Geometry geom = Geometry.CreateFromWkt(strMultiline);
                            oFeature.SetGeometry(geom);
                            lock (obj)
                            {
                                //将要素写入图层
                                oLayer.CreateFeature(oFeature);
                            }
                            oFeature.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Status = "错误";
                    log.ErrorMsg = ex.ToString();
                    log.ErrorDate = DateTime.Now.ToString();
                    new Log<BaiduMapFeatureDownloadResult>(log);

                    threadlog.Current = current;
                    threadlog.Current_loc = List2Str(current_loc);
                    threadlog.Status = "错误";
                    threadlog.TStatus = 3;
                    threadlog.IsPaused = true;
                    new Log<BaiduMapFeatureDownloadResult>(threadlog);
                    for (int k = 0; k < countdown.CurrentCount; k++)
                        countdown.Signal();
                    return;
                }
            }
            countdown.Signal();
        }

        GaodeMapFeatureDownloadResult Result_AFeature;
        /// <summary>
        /// 高德矢量要素下载
        /// </summary>
        /// <param name="Result"></param>
        /// <returns></returns>
        public WebApiResult<string> GaodeDownload(GaodeMapFeatureDownloadResult Result)
        {
            this.countdown = new CountdownEvent(Result.TaskCount);
            this.Result_AFeature = Result;
            this.N = Result.TaskCount;

            UpdateLastLoc<GaodeMapFeatureDownloadResult>(Result.GUID);

            log = new TaskLogEntity() { GUID = Result.GUID, Name = Result.TName, Type = "矢量要素", Description = "GaodeDownload", Status = "进行中", Parameter = JsonHelper.ToJson(Result), SavePath =Result.SavePathText };
            //操作日志
            new Log<GaodeMapFeatureDownloadResult>(log);

            if (Result.LayerName == "道路")
            {
                xmlDoc.Load(MapFeatureConfig);

                ///////////////////////////////////////////////////////////////////////////////
                // GDAL创建shp文件
                // 注册所有的驱动
                Ogr.RegisterAll();
                // GDAL中文路径支持
                OSGeo.GDAL.Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");
                // 属性表字段中文支持
                OSGeo.GDAL.Gdal.SetConfigOption("SHAPE_ENCODING", "CP936");
                // 设置坐标系存储文件夹gdal-data路径
                OSGeo.GDAL.Gdal.SetConfigOption("GDAL_DATA", System.Windows.Forms.Application.StartupPath + "\\gdal-data");
                Driver oDriver = Ogr.GetDriverByName("ESRI Shapefile");
                if (oDriver == null)
                {
                    return new WebApiResult<string>() { success = 0, msg = "GDAL驱动不可用，请检查！" };
                }
                // 创建数据源  
                DataSource oDS = oDriver.CreateDataSource(Result.SavePathText, null);
                if (oDS == null)
                {
                    return new WebApiResult<string>() { success = 0, msg = "创建数据源失败，请检查！" };
                }
                oLayer = oDS.GetLayerByName(Result.TName);
                if (oLayer == null)
                {
                    // 创建图层，创建一个多线图层，并指定空间参考
                    //第一种方法
                    OSGeo.OSR.SpatialReference oSRS = new OSGeo.OSR.SpatialReference("PROJCS[\"WGS_1984_Pseudo_Mercator\",GEOGCS[\"GCS_WGS_1984\",DATUM[\"D_WGS_1984\",SPHEROID[\"WGS_1984\",6378137.0,298.257223563]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]],PROJECTION[\"Mercator\"],PARAMETER[\"false_easting\",0.0],PARAMETER[\"false_northing\",0.0],PARAMETER[\"central_meridian\",0.0],PARAMETER[\"standard_parallel_1\",0.0],UNIT[\"Meter\",1.0]]");
                    //第二种方法
                    //OSGeo.OSR.SpatialReference oSRS = new OSGeo.OSR.SpatialReference("");
                    //oSRS.SetWellKnownGeogCS("EPSG:4326");

                    oLayer = oDS.CreateLayer(Result.TName, oSRS, wkbGeometryType.wkbMultiLineString, null);
                    if (oLayer == null)
                    {
                        return new WebApiResult<string>() { success = 0, msg = "创建图层失败，请检查！" };
                    }
                    // 创建属性表  
                    // 先创建一个叫Name的字符型属性，字符长度为100
                    FieldDefn oFieldName = new FieldDefn("Name", FieldType.OFTString);
                    oFieldName.SetWidth(100);
                    oLayer.CreateField(oFieldName, 1);
                    // 再创建一个叫FeatureName的字符型属性，字符长度为50  
                    FieldDefn oFieldUID = new FieldDefn("RID", FieldType.OFTString);
                    oFieldUID.SetWidth(50);
                    oLayer.CreateField(oFieldUID, 1);
                }

                /////////////////////////////////////////////////////////////////////////////////

                threadlog = new ThreadTaskLogEntity() { GUID = Result.GUID, TaskLog_GUID = Result.GUID, Status = "进行中",TStatus=1, TName = Result.TName, IsPaused = false, URL = Result.URL, Parameter = JsonHelper.ToJson(Result) };
                //计算Total并更新 
                threadlog.Total = GetList("SELECT KWName FROM mapbar_poi WHERE KWType = '道路'", "KWName").Count;
                new Log<GaodeMapFeatureDownloadResult>(threadlog);
                log.Count = threadlog.Total;
                new Log<GaodeMapFeatureDownloadResult>(log);

                Thread[] t = new Thread[Result.TaskCount];
                for (int num = 0; num < Result.TaskCount; num++)
                {
                    try
                    {
                        t[num] = new Thread(new ParameterizedThreadStart(run_AFeature))
                        {
                            Name = "Thread " + num.ToString()
                        };
                        t[num].Start(num);
                        
                    }
                    catch (Exception ex)
                    {
                        threadlog.Status = "错误";
                        threadlog.TStatus = 3;
                        threadlog.ErrorMsg = ex.ToString();
                        new Log<GaodeMapFeatureDownloadResult>(threadlog);

                        log.Status = "错误";
                        log.ErrorMsg = ex.ToString();
                        log.ErrorDate = DateTime.Now.ToString();
                        //操作日志
                        new Log<BaiduMapFeatureDownloadResult>(log);

                        return new WebApiResult<string>() { success = 0, msg = ex.ToString() };
                    }
                }
                countdown.Wait();
                oLayer.Dispose();
                oDS.Dispose();
                for (int num = 0; num < Result.TaskCount; num++)
                {
                    t[num].Abort();
                }
                lock (obj)
                {
                    if (!Log<GaodeMapFeatureDownloadResult>.GetThreadLogEntity(this.Result_AFeature.GUID).IsPaused)
                    {

                        //配置文件参数信息更新
                        xmlDoc["MapFeature"]["bmap"]["dict_index"].InnerText = "0";
                        xmlDoc.Save(MapFeatureConfig);

                        log.Status = "已完成";
                        log.CompleteTime = DateTime.Now.ToString();
                        log.Current = log.Count;
                        threadlog.Status = "已完成";
                        threadlog.TStatus = 2;
                        threadlog.Current = threadlog.Total;
                        threadlog.Current_loc = List2Str(current_loc);
                        //操作日志
                        new Log<GaodeMapFeatureDownloadResult>(threadlog);
                        new Log<GaodeMapFeatureDownloadResult>(log);
                        return new WebApiResult<string>() { success = 1, msg = "高德地图要素下载完成！" };
                    }
                }
                
            }
            else
            {
                return new WebApiResult<string>() { success = 2, msg = "图层类型不符合" };
            }
            return null;
        }

        void run_AFeature(object num)
        {
            FeatureDefn oDefn = oLayer.GetLayerDefn();

            string city_id = GetList("SELECT baiduId FROM cityinform WHERE Name = " + "'" + this.Result_AFeature.City + "'", "baiduId")[0];
            // 查询webmap数据库，获取mapbar_poi中类型为道路的POI的名字
            List<String> roadName = GetList("SELECT KWName FROM mapbar_poi WHERE KWType = '道路'and BaiduId = " + city_id, "KWName");
            int interval = (int)Math.Ceiling(roadName.Count / (double)N);
            int start = 0 + interval * (int)num;
            int end = start + interval;
            if (end > roadName.Count) end = roadName.Count;

            for (int i = start; i < end; i++)
            {
                if (i < current) continue;
                //if (i == 1) Pause<BaiduMapFeatureDownloadResult>(this.Result_AFeature.GUID);

                if (Log<GaodeMapFeatureDownloadResult>.GetThreadLogEntity(this.Result_AFeature.GUID).IsPaused)
                {
                    // 更新配置文件参数信息
                    xmlDoc["MapFeature"]["bmap"]["dict_index"].InnerText = current.ToString();
                    xmlDoc.Save(MapFeatureConfig);

                    threadlog.Current = current;
                    threadlog.Current_loc = List2Str(current_loc);
                    threadlog.Status = "暂停";
                    threadlog.TStatus = 4;
                    threadlog.IsPaused = true;
                    new Log<GaodeMapFeatureDownloadResult>(threadlog);

                    log.Status = "未完成";
                    log.Current = current;
                    new Log<GaodeMapFeatureDownloadResult>(log);
                    for (int k = 0; k < countdown.CurrentCount; k++)
                        countdown.Signal();
                    return;
                }
                //Application.DoEvents();

                // 根据道路名称向服务器请求矢量道路信息，并对返回JSON数据进行解析（取第一页前30条记录进行解析）
                try
                {
                    lock (obj)
                    {
                        current++;
                        threadlog.Current = current;
                        new Log<GaodeMapFeatureDownloadResult>(threadlog);
                        if (Contains(current_loc, (int)num) >= 0)
                        {
                            var index = Contains(current_loc, (int)num);
                            current_loc[index][1] = i;
                        }
                        else
                        {
                            if (current_loc == null) current_loc = new List<int[]>();
                            current_loc.Add(new int[2] { (int)num, i });
                        }
                    }

                    //CacheHelper.SetCache(this.Result_AFeature.GUID, log, 7200);
                    // 状态栏更新
                    //CacheHelper.SetCache(this.Result_AFeature.GUID, "{\"MapDescription\":\"高德地图矢量道路下载中...\",\"Count\":\"" + roadName.Count + "\",\"Current\":\"" + current + "\"}", 180);
                    
                    city_id = GetList("SELECT gaodeId FROM cityinform WHERE Name = " + "'" + this.Result_AFeature.City + "'", "gaodeId")[0];
                    // 构建请求URL
                    //string curUrl = xmlDoc["MapFeature"]["amap"]["url"].InnerText + "&city=" + xmlDoc["MapFeature"]["amap"]["city"].InnerText + "&keywords=" + HttpUtility.UrlEncode(roadName[i].ToString()) + "&pagenum=1";
                    string curUrl = xmlDoc["MapFeature"]["amap"]["url"].InnerText + "&city=" + city_id + "&keywords=" + HttpUtility.UrlEncode(roadName[i].ToString()) + "&pagenum=1";

                    // 向服务器发起Get请求
                    string curResponse = GetHttpResponse(curUrl);
                    // 返回结果字符串序列化为JSON
                    JObject jobject = (JObject)JsonConvert.DeserializeObject(curResponse);
                    if (jobject["data"].ToString() == "Return Failure!") continue;
                    //当查询次数超过阈值时，需破解极验验证码
                    if (jobject["data"].ToString() == "too fast")
                    {
                        string referer = "";
                        //利用第三方网站（http://jiyandoc.c2567.com/）提供收费接口获取识别结果
                        do
                        {
                            referer = "http://ditu.amap.com/verify/?from=" + HttpUtility.UrlEncode("http://ditu.amap.com/search?query=" + roadName[i].ToString()) + "&channel=newpc&uuid=322a41da-6f8d-4388-b5a7-e1708309cf4f&url=" + curUrl.Substring(20);
                            curResponse = GetHttpResponse(referer);
                            //解析高德地图极验验证码标识
                            String[] strgt = curResponse.Split(new String[] { "//api.geetest.com/get.php?gt=", "&product=embed" }, StringSplitOptions.None);
                            //利用第三方网站构建请求进行识别
                            String cverifyurl = "http://jiyanapi.c2567.com/shibie?user=zeswang&pass=200632590117&return=json&gt=" + strgt[1] + "&referer=http://ditu.amap.com";
                            curResponse = GetHttpResponse(cverifyurl);
                            jobject = (JObject)JsonConvert.DeserializeObject(curResponse);
                        } while (jobject["status"].ToString() == "no");  //判断识别结果，如果识别不成功重复请求识别
                        string posturl = "http://ditu.amap.com/gt?rand=" + rm.NextDouble().ToString();
                        string geetest_challenge = jobject["challenge"].ToString();
                        string geetest_validate = jobject["validate"].ToString();
                        string geetest_seccode = geetest_validate + "|jordan";
                        curResponse = GetHttpResponse(posturl, geetest_challenge, geetest_validate, geetest_seccode, referer);
                        JObject verifyjo = (JObject)JsonConvert.DeserializeObject(curResponse);
                        if (verifyjo["errmsg"].ToString() == "OK")
                        {
                            curResponse = GetHttpResponse(curUrl);
                            jobject = (JObject)JsonConvert.DeserializeObject(curResponse);
                        }
                    }
                    // 解析JSON获取道路相关信息
                    // 获取返回记录的数量
                    int totalnum = Convert.ToInt32(jobject["data"]["total"].ToString());
                    if (totalnum == 0 || jobject["data"]["poi_list"] == null) continue;
                    //解析返回POI信息
                    JEnumerable<JToken> poi_list = jobject["data"]["poi_list"].Children();
                    foreach (JToken poi in poi_list)
                    {
                        string name = GetInfotoString("name", poi);
                        string rid = GetInfotoString("id", poi);
                        string typecode = GetInfotoString("typecode", poi);
                        if (name == "" || rid == "") continue;
                        //判断该POI是否为道路
                        if (typecode == "190301")
                        {
                            //判断该POI中是否含有坐标串信息
                            if (poi["domain_list"][3]["value"] != null)
                            {
                                /////////////////////////////////////////////////////
                                Feature oFeature = new Feature(oDefn);
                                oFeature.SetField(0, name);
                                oFeature.SetField(1, rid);
                                ////////////////////////////////////////////////////

                                string roadcoordinates = poi["domain_list"][3]["value"].ToString();
                                string[] roadlist_coordinates = roadcoordinates.Split('|');
                                string strMultiline = "MULTILINESTRING(";
                                for (int j = 0; j < roadlist_coordinates.Length; j++)
                                {
                                    string strLine = "(";
                                    string[] pointlist_coordinates = roadlist_coordinates[j].Split('_');
                                    for (int t = 0; t < pointlist_coordinates.Length; t++)
                                    {
                                        strLine = strLine + pointlist_coordinates[t].Replace(",", " ") + ",";
                                    }
                                    strLine = strLine.Substring(0, strLine.LastIndexOf(",")) + ")";
                                    strMultiline = strMultiline + strLine + ",";
                                }
                                strMultiline = strMultiline.Substring(0, strMultiline.LastIndexOf(",")) + ")";

                                Geometry geom = Geometry.CreateFromWkt(strMultiline);
                                oFeature.SetGeometry(geom);
                                lock (obj)
                                {
                                    oLayer.CreateFeature(oFeature);
                                }
                                oFeature.Dispose();
                            }
                        }
                    }
                }

                catch (Exception ex)
                {
                    log.Status = "错误";
                    log.ErrorMsg = ex.ToString();
                    log.ErrorDate = DateTime.Now.ToString();
                    new Log<GaodeMapFeatureDownloadResult>(log);

                    threadlog.Current = current;
                    threadlog.Current_loc = List2Str(current_loc);
                    threadlog.Status = "错误";
                    threadlog.TStatus = 3;
                    threadlog.IsPaused = true;
                    new Log<GaodeMapFeatureDownloadResult>(threadlog);
                    for (int k = 0; k < countdown.CurrentCount; k++)
                        countdown.Signal();
                    return;
                }
            }
            countdown.Signal();
        }

        //随机数
        Random rm;

        /// <summary>
        /// 获取网页请求结果
        /// </summary>
        /// <param name="URL">请求网页地址</param>
        /// <returns>返回网页字符串</returns>
        public String GetHttpResponse(String url)
        {
            String restring = "";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727; .NET CLR 3.0.04506.648; .NET CLR 3.5.21022)";
            //设置代理IP 
            //WebProxy proxy = new WebProxy("120.237.91.34", 9797);
            //request.Proxy = proxy;
            HttpWebResponse response = null;
            HttpWebResponse resex = null;
            int requestcount = 0;
            do
            {
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex)
                {
                    resex = (HttpWebResponse)ex.Response;
                    Thread.Sleep(rm.Next(5, 10) * 1000);
                    requestcount++;
                }
            } while (response == null && requestcount < 3);
            if (response == null) response = resex;
            Stream responseStream = response.GetResponseStream();
            StreamReader sr = new StreamReader(responseStream, Encoding.UTF8);
            restring = sr.ReadToEnd();
            responseStream.Close();
            return restring;
        }

        /// <summary>
        /// 提取JSON中key对应的的字符串型value
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="jt">josn</param>
        /// <returns></returns>
        public String GetInfotoString(String key, JToken jt)
        {
            try
            {
                return jt[key].ToString();
            }
            catch
            {
                return DBNull.Value.ToString();
            }
        }


        /// <summary>
        /// 获取数据库中已有信息以List返回
        /// </summary>
        /// <param name="querystring">查询字符串</param>
        /// <param name="colname">列名</param>
        /// <returns></returns>
        List<String> GetList(String querystring, String colname)
        {
            //读取数据库中已有POI分类信息存储于List中
            DataTable existdt = mysqlHelper.ExecuteDataTable(querystring);
            List<String> aList = new List<string>();
            foreach (DataRow row in existdt.Rows)
            {
                aList.Add(row[colname].ToString());
            }

            return aList;
        }

         /// <summary>
        /// 将极验验证码识别结果组合构成post请求URL地址传递给服务器
        /// </summary>
        /// <param name="url"></param>
        /// <param name="geetest_challenge">极验验证码标识</param>
        /// <param name="geetest_validate">识别结果参数</param>
        /// <param name="geetest_seccode">识别结果参数</param>
        /// <param name="referer">跳转url</param>
        /// <param name="uuid">极验验证用户标识（高德地图）</param>
        /// <param name="channel">高德地图平台参数</param>
        /// <returns></returns>
        public String GetHttpResponse(String url, String geetest_challenge, String geetest_validate, String geetest_seccode, String referer, String uuid = "322a41da-6f8d-4388-b5a7-e1708309cf4f", String channel = "newpc")
        {
            String restring = "";
            try
            {
                String poststring = String.Format("geetest_challenge={0}&geetest_validate={1}&geetest_seccode={2}&uuid={3}&channel={4}", geetest_challenge, geetest_validate, HttpUtility.UrlEncode(geetest_seccode), uuid, channel);
                Byte[] postdata = Encoding.UTF8.GetBytes(poststring);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727; .NET CLR 3.0.04506.648; .NET CLR 3.5.21022)";
                //设置代理IP
                //WebProxy proxy = new WebProxy("120.237.91.34", 9797);
                //request.Proxy = proxy;
                request.Method = "POST";
                request.Referer = referer;
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postdata.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(postdata, 0, postdata.Length);

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader sr = new StreamReader(responseStream, Encoding.UTF8);
                restring = sr.ReadToEnd();
                responseStream.Close();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
            }

            Thread.Sleep(rm.Next(1, 3) * 1000);
            return restring;
        }

        int Contains(List<int[]> items, int num)
        {
            if (items == null)
                return -1;
            for (int item = 0; item < items.Count; item++)
            {
                if (items[item][0] == num)
                {
                    return item;
                }
            }
            return -1;
        }

        public string List2Str(List<int[]> li)
        {
            if (li == null)
                return null;
            string res = "";
            foreach (var item in li)
                res = res + "{" + item[0].ToString() + "," + item[1].ToString() + "};";
            res = res.Substring(0, res.Length - 1);
            return res;
        }

        public List<int[]> Str2List(string str)
        {
            if (str == null)
                return null;
            List<int[]> li = new List<int[]>();
            foreach (string item in str.Split(';'))
            {
                var i = item.Split(',');
                var a = int.Parse(i[0].Substring(1, i[0].Length - 1));
                var b = int.Parse(i[1].Substring(0, i[1].Length - 1));
                li.Add(new int[2] { a, b });
            }

            return li;
        }

        void UpdateLastLoc<T>(string GUID)
        {
            var entity = Log<T>.GetThreadLogEntity(GUID);
            if (entity == null) return;
            if (entity.Current_loc != "")
                this.current_loc = Str2List(entity.Current_loc);
            this.current = entity.Current;
        }

        public WebApiResult<string> Pause<T>(string GUID)
        {
            var entity = Log<T>.GetThreadLogEntity(GUID);
            if (entity == null) return new WebApiResult<string> { success = 0, msg = "该任务不存在" };
            if (entity.Status == "已完成") return new WebApiResult<string> { success = 0, msg = "该任务已完成" };
            entity.IsPaused = true;
            new Log<T>(entity);
            return new WebApiResult<string>() { success = 1, msg = "暂停成功" };
        }
    }
}
