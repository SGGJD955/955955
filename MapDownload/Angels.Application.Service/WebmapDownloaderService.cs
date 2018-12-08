using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Data;
using System.Net;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using Angels.Application.TicketEntity;
using Angels.Application.TicketEntity.Request.WebmapDownloader;
using Angels.Application.TicketEntity.Common.WebmapDownloader;
using Angels.Common;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using System.Drawing;
using Angels.Application.TicketEntity.Common;

namespace Angels.Application.Service
{
    /// <summary>
    /// 网络地图下载
    /// </summary>
    public class WebmapDownloaderService
    {
        #region 固定变量配置信息

        //图吧URL信息
        //String CityName = "wuhan";
        //String CityName = "enshi";
        //String CityName = "beijing";
        String RootURL = "http://poi.mapbar.com/";
        //百度地图URL信息(POI)
        String BaiduURL = "http://map.baidu.com/?newmap=1&reqflag=pcmap&biz=1&from=webmap&da_par=direct&pcevaname=pc4.1&qt=s&da_src=searchBox.button";
        //String c = "218";
        //String c = "373";
        //String c = "131";
        //高德地图URL信息(POI)
        String AmapVerifyURL = "http://ditu.amap.com/verify/?from=";
        String AmapURL = "http://ditu.amap.com/service/poiInfo?query_type=TQUERY&pagesize=30&qii=true&cluster_state=5&need_utd=true&utd_sceneid=1000&div=PC1000&addr_poi_merge=true&is_classify=true";
        //String city = "420100";
        //配置参数信息
        //String MapDownloadConfig = "../../../Config/MapDownloadConfig.xml";
        
        #endregion

        //数据库连接信息
        String ConnectionString = Config.GetValue("DataMySql");
        //String ConnectionString = String.Format("server={0};user id={1};password={2};database={3};CharSet=utf8", "localhost", "root", "123456", "webmap");
        Angels.Application.Data.MySqlHelper mysqlHelper;
        Random rm;
        //深度学习参数
        public static IntPtr classifierHandle;
        String prototxt = System.AppDomain.CurrentDomain.BaseDirectory + "Resource\\deploy.prototxt";// System.Windows.Forms.Application.StartupPath + "\\Resource\\deploy.prototxt"; //"D:\\深度神经网络\\CC深度学习V2.2\\examples\\验证码-简单\\deploy.prototxt";
        String caffemodel = System.AppDomain.CurrentDomain.BaseDirectory + "Resource\\_iter_504.caffemodel";// System.Windows.Forms.Application.StartupPath + "\\Resource\\_iter_504.caffemodel";  //"D:\\深度神经网络\\CC深度学习V2.2\\examples\\验证码-简单\\models\\_iter_504.caffemodel";
        //正则表达式定义
        Match mat;
        Regex regex;
        MatchCollection collection;

        private readonly static object obj = new object();
        TaskLogEntity log;
        ThreadTaskLogEntity threadlog;
        List<int[]> current_loc;
        int current = 0;
        int current_i = -999999, current_j = -999999;
        CountdownEvent countdown;
        int N;
        int TaskCount = 1;
        
        List<int[]> error_loc = new List<int[]>();

        List<String> classList;
        public WebmapDownloaderService()
        {
            rm = new Random();

            mysqlHelper = new Angels.Application.Data.MySqlHelper(ConnectionString);
        }

        Mapbar_POIRequest request_mapbarPOI;
        /// <summary>
        /// 图吧POI
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        public WebApiResult<string> Mapbar_POI_MenuItem(Mapbar_POIRequest Request)
        {
            this.countdown = new CountdownEvent(TaskCount);
            this.request_mapbarPOI = Request;
            this.N = TaskCount;
            UpdateLastLoc<Mapbar_POIRequest>(Request.GUID);
            
            log = new TaskLogEntity() { GUID = Request.GUID, Name = Request.TName, Type = "POI", Description = "Mapbar_POI_MenuItem", Status = "进行中", Parameter = JsonHelper.ToJson(Request) };
            threadlog = new ThreadTaskLogEntity() { GUID = Request.GUID, TaskLog_GUID = Request.GUID, Status = "进行中", TStatus = 1, Total = 0, TName = Request.TName, IsPaused = false, Parameter = JsonHelper.ToJson(Request), URL = Request.URL };
            new Log<Mapbar_POIRequest>(threadlog);
            new Log<Mapbar_POIRequest>(log);

            //获取大类信息存储到mapbar_poiclass
            classList = GetPOIClass(this.request_mapbarPOI);
            //计算Total并更新 
            threadlog.Total = classList.Count;
            log.Count = threadlog.Total;
            new Log<Mapbar_POIRequest>(threadlog);            
            new Log<Mapbar_POIRequest>(log);

            try
            {
                Thread[] t = new Thread[TaskCount];
                for (int num = 0; num < t.Length; num++)
                {
                    try
                    {
                        t[num] = new Thread(new ParameterizedThreadStart(run_mapbarPOI))
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
                        new Log<Mapbar_POIRequest>(threadlog);
                    }
                }
                countdown.Wait();
                for (int num = 0; num < t.Length; num++)
                {
                    t[num].Abort();
                }
                lock (obj)
                {
                    if (!Log<Mapbar_POIRequest>.GetThreadLogEntity(this.request_mapbarPOI.GUID).IsPaused)
                    {
                        log.Status = "已完成";
                        log.CompleteTime = DateTime.Now.ToString();
                        log.Current = log.Count;
                        threadlog.Status = "已完成";
                        threadlog.TStatus = 2;
                        threadlog.Current = threadlog.Total;
                        threadlog.Current_loc = List2Str(current_loc);
                        //操作日志
                        new Log<Mapbar_POIRequest>(threadlog);
                        new Log<Mapbar_POIRequest>(log);
                    }
                }
            }
            catch (Exception ex)
            {
                //log = (LogEntity)CacheHelper.GetCache(log.GUID);
                log.Status = "错误";
                log.ErrorMsg = ex.ToString();
                log.ErrorDate = DateTime.Now.ToString();
                new Log<Mapbar_POIRequest>(log);
            }

            return null;
        }

        public void run_mapbarPOI(object num)
        {
            int city_id = int.Parse(GetList("SELECT baiduId FROM cityinform WHERE Name = " + "'" + this.request_mapbarPOI.CityName_zh + "'", "baiduId")[0]);

            //获取大类信息存储到mapbar_poiclass
            //List<String> classList = GetPOIClass(this.request_mapbarPOI);
            //获取mapbar_poi中已有的关键字信息
            List<String> kwList = GetList("select KWName,KWType from mapbar_poi WHERE BaiduId = " + city_id, "KWName");

            int interval = (int)Math.Ceiling(classList.Count / (double)N);
            int start = 0 + interval * (int)num;
            int end = start + interval;
            if (end > classList.Count) end = classList.Count;

            int ord;
            for (ord = start; ord < end; ord++)
            {

                try
                {
                    int i = 0;
                    int pageNum = 1;
                    int currentPage = 0;

                    if (this.current_loc != null)
                    {
                        if (Contains(current_loc, ord) >= 0)
                        {
                            var index = Contains(current_loc, ord);
                            currentPage = current_loc[index][1];
                        }
                    }

                    do
                    {
                        if (i < currentPage)
                        {
                            i++;
                            continue;
                        }

                        if (Log<Mapbar_POIRequest>.GetThreadLogEntity(this.request_mapbarPOI.GUID).IsPaused)
                        {
                            threadlog.Current = current;
                            threadlog.Current_loc = List2Str(current_loc);
                            threadlog.Status = "暂停";
                            threadlog.TStatus = 4;
                            threadlog.IsPaused = true;
                            new Log<Mapbar_POIRequest>(threadlog);
                            log.Status = "未完成";
                            log.Current = current;
                            new Log<Mapbar_POIRequest>(log);

                            countdown.Signal();
                            return;
                        }

                        //请求数据;
                        String httpurl = classList[ord].Substring(classList[ord].LastIndexOf(",")+ 1);
                        httpurl ="http://poi.mapbar.com/" + this.request_mapbarPOI.CityName + '/' + httpurl;
                        String pagecontent = GetHttpResponse(httpurl.Substring(0, httpurl.Length - 1) + "_" + (i + 1) + "/");
                        //判断是否触发网站验证码
                        if (pagecontent.Contains("错误"))
                        {
                            pagecontent = VerifyCrack(this.request_mapbarPOI, pagecontent, classList[ord].Substring(0, classList[ord].Length - 1) + "_" + (i + 1) + "/");
                        }
                        if(pagecontent=="failed")
                        {
                            threadlog.Current = current;
                            threadlog.Current_loc = List2Str(current_loc);
                            threadlog.Status = "暂停";
                            threadlog.TStatus = 4;
                            threadlog.IsPaused = true;
                            new Log<Mapbar_POIRequest>(threadlog);
                            log.Status = "未完成";
                            log.Current = current;
                            new Log<Mapbar_POIRequest>(log);

                            countdown.Signal();
                            return;
                        }
                        //获取页面为当前分类首页时，解析页面获取页码信息
                        if (i == 0)
                        {
                            //获取当前分类POI的页数
                            mat = Regex.Match(pagecontent, "var pageNum = '.+'");
                            String[] strNum = mat.ToString().Split(new String[] { "'", "'" }, StringSplitOptions.RemoveEmptyEntries);
                            pageNum = Convert.ToInt32(strNum[1]);

                            //状态栏信息输出
                            //CacheHelper.SetCache(Request.GUID + "1", "{\"POICount\":" + classList.Count + ",\"Current\":" + (classList.IndexOf(classList[ord]) + 1) + ",\"Count\":" + pageNum + "}", 180);//String.Format("POI共有{0}类，当前为{1}类，共{2}页", classList.Count, classList.IndexOf(classList[ord]) + 1, pageNum)
                        }
                        
                        //创建DataTable用于存储数据
                        DataTable poidt = new DataTable();
                        poidt.TableName = "mapbar_poi";
                        //poidt.TableName = "mapbar_poi_enshi";//-------------------------------更换poi_class表名
                        //poidt.TableName = "mapbar_poi_bj";
                        poidt.Columns.Add("KWName", typeof(String));
                        poidt.Columns.Add("KWType", typeof(String));
                        poidt.Columns.Add("KWAddress", typeof(String));
                        poidt.Columns.Add("KWLatitude", typeof(String));
                        poidt.Columns.Add("KWLongitude", typeof(String));
                        poidt.Columns.Add("KWUpdatetime", typeof(DateTime));
                        poidt.Columns.Add("BaiduId", typeof(int));

                        //分割字符串，并利用正则表达式生成字符串集合
                        string[] strArr = pagecontent.Split(new string[] { "<div class=\"sortC\">", "<div class=\"sortPage cl\" id=\"pageDiv\"" }, StringSplitOptions.None);
                        regex = new Regex("<a.*</a>");
                        collection = regex.Matches(strArr[1]);                                           

                        lock (obj)
                        {
                            if (Contains(current_loc, ord) >= 0)
                            {
                                var index = Contains(current_loc, ord);
                                current_loc[index][1] = i;
                            }
                            else
                            {
                                if (current_loc == null) current_loc = new List<int[]>();
                                current_loc.Add(new int[3] { ord, i, collection.Count });
                            }
                        }

                        for (int j = 0; j < collection.Count; j++)
                        {

                            //Application.DoEvents();

                            String matchstring = collection[j].ToString();
                            //获取字符串中的name，判断是否存在于关键字列表中，若存在，直接跳过，不存在则进行采集存储
                            String[] nameArr = matchstring.Split(new String[] { ">", "</a>" }, StringSplitOptions.RemoveEmptyEntries);
                            if (kwList.Contains(nameArr[1])) continue;
                            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            //请求图吧POI详细信息页面的代码，疑似有内存泄漏，长时间运行会导致程序自动退出
                            /*
                            //获取字符串中的href，用于请求POI详情页面
                            String[] hrefArr = matchstring.Split(new String[] { "href=\"", "\">" }, StringSplitOptions.RemoveEmptyEntries);
                            String detailstring = GetHttpResponse(hrefArr[1]);
                            //判断是否触发网站验证码
                            if (detailstring.Contains("<title>错误</title>"))
                            {
                                detailstring = VerifyCrack(detailstring, hrefArr[1]);
                                Thread.Sleep(rm.Next(1, 3) * 1000);
                                Debug.WriteLine(detailstring);
                            }
                            //判断请求页面是否存在
                            if (detailstring.Contains("http://img.mapbar.com/web/index/js/404source.js") || detailstring.Contains("<title>图吧 商户认证</title>")) continue;
                            //利用HtmlAgilityPack解析HTML文档
                            HtmlAgilityPack.HtmlDocument htmldoc = new HtmlAgilityPack.HtmlDocument();
                            htmldoc.LoadHtml(detailstring);
                            //POI经纬度信息
                            String metacontent = htmldoc.DocumentNode.SelectSingleNode(@"//meta[@name='location']").Attributes["content"].Value.Trim();
                            String coord = metacontent.Substring(metacontent.LastIndexOf("=") + 1);
                            String[] latlon = coord.Split(new String[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                            String KWLatitude = latlon[1];
                            String KWLongitude = latlon[0];
                            //POI名字信息
                            String KWName = htmldoc.DocumentNode.SelectSingleNode(@"//div[@class='POILeftA']/h1[@id='poiName']").InnerText.Trim();
                            //POI地址信息
                            String KWAddress = htmldoc.DocumentNode.SelectSingleNode(@"//div[@class='infoPhoto']/ul[@class='POI_ulA']/li[2]").InnerText.Trim();
                            KWAddress = KWAddress.Substring(KWAddress.LastIndexOf("：") + 1);
                            KWAddress = KWAddress.Replace("\t", String.Empty).Replace("\r", String.Empty).Replace("\n", String.Empty).Replace(" ", String.Empty);
                            //POI类别信息
                            String KWType = htmldoc.DocumentNode.SelectSingleNode(@"//div[@class='infoPhoto']/ul[@class='POI_ulA']/li[last()]").InnerText.Trim();
                            KWType = KWType.Substring(KWType.LastIndexOf("：") + 1);
                            */
                            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
                            //POI名字信息
                            String KWName = nameArr[1].ToString();
                            //POI地址信息
                            String KWAddress = "";
                            //POI经纬度信息
                            String KWLatitude = "0.0";
                            String KWLongitude = "0.0";
                            //POI类别信息
                            String KWType = classList[ord].Substring(0, classList[ord].LastIndexOf(","));

                            
                            kwList.Add(KWName);
                            poidt.Rows.Add(KWName, KWType, KWAddress, KWLatitude, KWLongitude, DateTime.Now, city_id);
                        }
                        if (poidt.Rows.Count > 0)
                            //将当前页面POI数据插入数据库
                            InsertData(ConnectionString, poidt, log.GUID);
                        //强制回收内存
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        i++;
                    } while (i < pageNum);

                    lock (obj)
                    {
                        //current++;//classlist
                        //int city_id = int.Parse(GetList("SELECT baiduId FROM cityinform WHERE Name = " + "'" + this.request_mapbarPOI.CityName_zh + "'", "baiduId")[0]);
                        current = GetList("SELECT DISTINCT KWType FROM mapbar_poi WHERE BaiduId = " + city_id , "KWType").Count();
                        threadlog = Log<Mapbar_POIRequest>.GetThreadLogEntity(this.request_mapbarPOI.GUID);
                        threadlog.Current = current;
                        new Log<Mapbar_POIRequest>(threadlog);
                    }
                }
                catch (Exception ex)
                {
                    log.Status = "错误";
                    log.ErrorMsg = ex.ToString();
                    log.ErrorDate = DateTime.Now.ToString();
                    new Log<Mapbar_POIRequest>(log);

                    threadlog.Current = current;
                    threadlog.Current_loc = List2Str(current_loc);
                    threadlog.Status = "错误";
                    threadlog.TStatus = 3;
                    threadlog.IsPaused = true;
                    new Log<Mapbar_POIRequest>(threadlog);
                    for (int k = 0; k < countdown.CurrentCount; k++)
                        countdown.Signal();
                    return;
                }
            }
            countdown.Signal();
        }

        Baidumap_POIRequest request_baiduPOI;
        /// <summary>
        /// 百度POI
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        public WebApiResult<string> Baidumap_POI_MenuItem(Baidumap_POIRequest Request)
        {
            this.countdown = new CountdownEvent(Request.TaskCount);
            this.request_baiduPOI = Request;
            this.N = Request.TaskCount;
            UpdateLastLoc<Baidumap_POIRequest>(Request.GUID);

            int city_id = int.Parse(GetList("SELECT baiduId FROM cityinform WHERE Name = " + "'" + this.request_baiduPOI.City + "'", "baiduId")[0]);
            //获取大类信息存储到mapbar_poiclass
            List<String> kwList = GetList("select KWName,KWType from mapbar_poi WHERE BaiduId = " + city_id, "KWName");
            log = new TaskLogEntity() { GUID = Request.GUID, Name = Request.TName, Type = "POI", Description = "Baidumap_POI_MenuItem", Status = "进行中", Parameter = JsonHelper.ToJson(Request) };
            threadlog = new ThreadTaskLogEntity(){GUID =Request.GUID,TaskLog_GUID = Request.GUID,Status = "进行中",TStatus=1, Total = 0, TName = Request.TName, IsPaused = false, Parameter = JsonHelper.ToJson(Request), URL = Request.URL };
            //计算Total并更新 
            threadlog.Total = kwList.Count;
            log.Count = threadlog.Total;
            new Log<Baidumap_POIRequest>(threadlog);
            new Log<Baidumap_POIRequest>(log);

            try
            {
                Thread[] t = new Thread[Request.TaskCount];
                for (int num = 0; num < Request.TaskCount; num++)
                {
                    try
                    {
                        t[num] = new Thread(new ParameterizedThreadStart(run_baiduPOI))
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
                        new Log<Baidumap_POIRequest>(threadlog);
                    }
                }
                countdown.Wait();
                for (int num = 0; num < Request.TaskCount; num++)
                {
                    t[num].Abort();
                }
                lock (obj)
                {
                    if (!Log<Baidumap_POIRequest>.GetThreadLogEntity(this.request_baiduPOI.GUID).IsPaused)
                    {
                        log.Status = "已完成";
                        log.Current = log.Count;
                        log.CompleteTime = DateTime.Now.ToString();
                        threadlog.Status = "已完成";
                        threadlog.TStatus = 2;
                        threadlog.Current = threadlog.Total;
                        threadlog.Current_loc = List2Str(current_loc);
                        //操作日志
                        new Log<Baidumap_POIRequest>(threadlog);
                        new Log<Baidumap_POIRequest>(log);
                    }
                }               
            }
            catch (Exception ex)
            {
                //操作日志
                log.Status = "错误";
                log.ErrorDate = DateTime.Now.ToString();
                log.ErrorMsg = ex.ToString();
                new Log<Baidumap_POIRequest>(log);
            }

            return null; 

        }

        public void run_baiduPOI(object num)
        {            
            int cityid = int.Parse(GetList("SELECT baiduId FROM cityinform WHERE Name = " + "'" + this.request_baiduPOI.City + "'", "baiduId")[0]);
            //获取mapbar_poi中已有的关键字信息
            List<String> kwList = GetList("select KWName,KWType from mapbar_poi WHERE BaiduId = " + cityid, "KWName");
            
            //获取baidumap_poi中已有的POI列表
            List<String> bmpoilist = GetList("select name,addr from baidumap_poi", "name", "addr");

            int interval = (int)Math.Ceiling(kwList.Count / (double)N);
            int start = 0 + interval * (int)num;
            int end = start + interval;
            if (end > kwList.Count) end = kwList.Count;
            
            //遍历关键字字典，基于关键字搜索百度地图POI
            for (int i = start; i < end; i++)
            {

                try
                {
                    int pn = 0;
                    int totalnum = 0;
                    int currentPage = 0;

                    if (this.current_loc != null)
                    {
                        if (Contains(current_loc, i) >= 0)
                        {
                            var index = Contains(current_loc, i);
                            currentPage = current_loc[index][1];
                        }
                    }

                    do
                    {
                        if (pn < currentPage)
                        {
                            pn = pn + 1;
                            continue;
                        }

                        if (Log<Baidumap_POIRequest>.GetThreadLogEntity(this.request_baiduPOI.GUID).IsPaused)
                        {
                            threadlog.Current = current;
                            threadlog.Current_loc = List2Str(current_loc);
                            threadlog.Status = "暂停";
                            threadlog.TStatus = 4;
                            threadlog.IsPaused = true;
                            new Log<Baidumap_POIRequest>(threadlog);
                            log.Status = "未完成";
                            log.Current = current;
                            new Log<Baidumap_POIRequest>(log);

                            countdown.Signal();
                            return;
                        }

                        string city_id = GetList("SELECT baiduId FROM cityinform WHERE Name = " + "'" + this.request_baiduPOI.City + "'", "baiduId")[0];
                        //基于关键字搜索百度地图POI,并将返回结果序列化为JSON
                        String curl = BaiduURL + "&wd=" + kwList[i].ToString() + "&c=" + city_id + "&pn=" + pn.ToString();
                        String curpage = GetHttpResponse(curl);
                        //百度高德设置时间间隔
                        //Thread.Sleep(rm.Next(1, 3) * 1000);
                        JObject jobject;
                        try
                        {
                            jobject = (JObject)JsonConvert.DeserializeObject(curpage);
                        }
                        catch (Exception ex)
                        {
                            //状态栏信息输出
                            //CacheHelper.SetCache(Request.GUID + "2", "{\"Count\":" + Math.Ceiling(totalnum / 10.0) + ",:\"Current\"" + (pn + 1) + "}", 180);//String.Format("搜索结果共{0}页，当前为{1}页", Math.Ceiling(totalnum / 10.0), pn + 1)

                            //Application.DoEvents();
                            pn = pn + 1;

                            //操作日志
                            log.Status = "错误";
                            log.ErrorDate = DateTime.Now.ToString();
                            log.ErrorMsg = ex.ToString();
                            new Log<Baidumap_POIRequest>(log);
                            continue;
                        }
                        //获取返回记录的数量
                        if (jobject["result"]["total"] == null)
                            break;
                        else if (jobject["result"]["total"].Type == JTokenType.Array)
                            break;
                        else
                            totalnum = Convert.ToInt32(jobject["result"]["total"].ToString());
                        if (totalnum == 0 || jobject["content"] == null) break;

                        //状态栏信息输出
                        //CacheHelper.SetCache(Request.GUID + "2", "{\"Count\":" + Math.Ceiling(totalnum / 10.0) + ",:\"Current\"" + (pn + 1) + "}", 180);//String.Format("搜索结果共{0}页，当前为{1}页", Math.Ceiling(totalnum / 10.0), pn + 1)
                        //Application.DoEvents();
                        lock (obj)
                        {
                            if (Contains(current_loc, i) >= 0)
                            {
                                var index = Contains(current_loc, i);
                                current_loc[index][1] = pn;
                            }
                            else
                            {
                                if (current_loc == null) current_loc = new List<int[]>();
                                current_loc.Add(new int[3] { i, pn, totalnum });
                            }
                        }

                        JEnumerable<JToken> contents = jobject["content"].Children();
                        foreach (JToken child in contents)
                        {
                            //获取百度POI的名字和地址，判断是否存已在于baidumap_poi中，若存在，直接跳过，不存在则进行采集存储
                            String nameaddr = GetInfotoString("name", child) + "," + GetInfotoString("addr", child);
                            if (bmpoilist.Contains(nameaddr) || child["cla"] == null) continue;
                            bmpoilist.Add(nameaddr);

                            List<MySqlParameter> listParam = new List<MySqlParameter>();
                            listParam.Add(new MySqlParameter(@"addr", GetInfotoString("addr", child)));
                            listParam.Add(new MySqlParameter(@"address_norm", GetInfotoString("address_norm", child)));
                            listParam.Add(new MySqlParameter(@"area_name", GetInfotoString("area_name", child)));
                            listParam.Add(new MySqlParameter(@"name", GetInfotoString("name", child)));
                            listParam.Add(new MySqlParameter(@"uid", GetInfotoString("uid", child)));
                            if (child["cla"].HasValues == true)
                                listParam.Add(new MySqlParameter(@"cla", child["cla"][0][1].ToString()));
                            else
                                listParam.Add(new MySqlParameter(@"cla", ""));

                            //坐标转换，需要用到百度API的key，每天限10万次
                            listParam.Add(new MySqlParameter(@"mx", GetInfotoString("x", child)));
                            listParam.Add(new MySqlParameter(@"my", GetInfotoString("y", child)));
                            List<float> coordinate = ConvertCoor(GetInfotoInt("x", child), GetInfotoInt("y", child));
                            listParam.Add(new MySqlParameter(@"x", coordinate[0]));
                            listParam.Add(new MySqlParameter(@"y", coordinate[1]));
                            //插入数据库
                            String sqlcommand = "insert into baidumap_poi(uid, name, addr, address_norm, area_name, cla, x, y) values(@uid, @name, @addr, @address_norm, @area_name, @cla, @x, @y)";
                            //String sqlcommand = "insert into baidumap_poi_enshi(uid, name, addr, address_norm, area_name, cla, mx, my, x, y) values(@uid, @name, @addr, @address_norm, @area_name, @cla, @mx, @my, @x, @y)";
                            //String sqlcommand = "insert into baidumap_poi_bj(uid, name, addr, address_norm, area_name, cla, mx, my, x, y) values(@uid, @name, @addr, @address_norm, @area_name, @cla, @mx, @my, @x, @y)";
                            mysqlHelper.ExecuteNonQuery(sqlcommand, listParam.ToArray());
                        }
                        pn = pn + 1;
                    } while (pn < Math.Ceiling(totalnum / 10.0));

                    lock (obj)
                    {
                        current++;//classlist
                        threadlog.Current = current;
                        new Log<Baidumap_POIRequest>(threadlog);
                    }
                }
                catch (Exception ex)
                {
                    log.Status = "错误";
                    log.ErrorMsg = ex.ToString();
                    log.ErrorDate = DateTime.Now.ToString();
                    new Log<Baidumap_POIRequest>(log);

                    threadlog.Current = current;
                    threadlog.Current_loc = List2Str(current_loc);
                    threadlog.Status = "错误";
                    threadlog.TStatus = 3;
                    threadlog.IsPaused = true;
                    new Log<Baidumap_POIRequest>(threadlog);
                    for (int k = 0; k < countdown.CurrentCount; k++)
                        countdown.Signal();
                    return;
                }
            }
            countdown.Signal();
        }


        LatLngPoint startcoord, endcoord;
        Baidumap_TileRequest request_BTile;
        /// <summary>
        /// 百度道路瓦片
        /// </summary>
        /// <returns></returns>
        public WebApiResult<string> Baidumap_Tile_MenuItem(Baidumap_TileRequest Request)
        {
            this.countdown = new CountdownEvent(Request.TaskCount);
            this.request_BTile = Request;
            this.N = Request.TaskCount;

            UpdateLastLoc< Baidumap_TileRequest>(Request.GUID);

            log = new TaskLogEntity() { GUID = Request.GUID, Name = Request.TName, Type = "百度瓦片下载", Description = "Baidumap_Tile_MenuItem", Status = "进行中", Parameter = JsonHelper.ToJson(Request), SavePath = Request.SavePathText };
            //操作日志
            new Log<Baidumap_TileRequest>(log);
            try
            {

                //武汉地区坐标范围信息
                //LatLngPoint startcoord = new LatLngPoint(29.972313, 113.707018);
                //LatLngPoint endcoord = new LatLngPoint(31.367044, 115.087233);
                //恩施地区坐标范围信息
                //LatLngPoint startcoord = new LatLngPoint(29.120116, 108.374308);
                //LatLngPoint endcoord = new LatLngPoint(31.402847, 110.649141);
                ////北京地区坐标范围信息
                //startcoord = new LatLngPoint(Request.startcoord.Lat, Request.startcoord.Lng);
                //endcoord = new LatLngPoint(Request.endcoord.Lat, Request.endcoord.Lng);

                startcoord = new LatLngPoint(29.120116, 108.374308);
                endcoord = new LatLngPoint(31.402847, 110.649141);
                int z = 11;
                Tuple<int, int, int, int> BoundTup = GetTileBound(startcoord, endcoord, z);

                threadlog = new ThreadTaskLogEntity() { GUID = Request.GUID, TaskLog_GUID = Request.GUID, Status = "进行中", TStatus=1, Total = 0, TName = Request.TName, IsPaused = false, Parameter = JsonHelper.ToJson(Request), URL = Request.URL };
                //计算Total并更新 
                threadlog.Total += (BoundTup.Item3 - BoundTup.Item1 + 1) * (BoundTup.Item4 - BoundTup.Item2 + 1);
                new Log<Baidumap_TileRequest>(threadlog);
                log.Count = threadlog.Total;
                new Log<Baidumap_TileRequest>(log);

                //构建下载链接，循环请求获取瓦片地图
                //String link = "http://online{0}.map.bdimg.com/onlinelabel/?qt=tile&x={1}&y={2}&z={3}&styles=pl&udt=20171031&scaler=1&p=0";
                //String originlink = "http://api{0}.map.bdimg.com/customimage/tile?&x={1}&y={2}&z={3}&udt=20171031&scale=1&ak=8d6c8b8f3749aed6b1aff3aad6f40e37&styles=t%3Abackground%7Ce%3Aall%7Cv%3Aoff%2Ct%3Aroad%7Ce%3Ag%7Cv%3Aon%7Cc%3A%23ffffff%2Ct%3Aroad%7Ce%3Al%7Cv%3Aoff%2Ct%3Apoi%7Ce%3Aall%7Cv%3Aoff%2Ct%3Aadministrative%7Ce%3Aall%7Cv%3Aoff";
                //恩施URLhttp://api{0}.map.bdimg.com/customimage/tile?&x={1}&y={2}&z={3}&scale=1&ak=8d6c8b8f3749aed6b1aff3aad6f40e37
                //String originlink = "http://api{0}.map.bdimg.com/customimage/tile?&x={1}&y={2}&z={3}&udt=20171214&scale=1&ak=8d6c8b8f3749aed6b1aff3aad6f40e37&styles=t%3Abackground%7Ce%3Aall%7Cv%3Aoff%2Ct%3Aroad%7Ce%3Ag%7Cv%3Aon%7Cc%3A%23ffffff%2Ct%3Aroad%7Ce%3Al%7Cv%3Aoff%2Ct%3Apoi%7Ce%3Aall%7Cv%3Aoff%2Ct%3Aadministrative%7Ce%3Aall%7Cv%3Aoff";
                //北京URL---------高速公路及国道
                //String originlink = "http://api{0}.map.bdimg.com/customimage/tile?&x={1}&y={2}&z={3}&udt=20180403&scale=1&ak=8d6c8b8f3749aed6b1aff3aad6f40e37&styles=t:administrative|e:all|v:off,t:poi|e:all|v:off,t:background|e:all|v:off,t:subway|e:all|v:off,t:railway|e:all|v:off,t:local|e:all|v:off,t:arterial|e:all|v:off,t:highway|e:l|v:off";
                //北京URL---------城市主路
                //String originlink = "http://api{0}.map.bdimg.com/customimage/tile?&x={1}&y={2}&z={3}&udt=20180403&scale=1&ak=8d6c8b8f3749aed6b1aff3aad6f40e37&styles=t:administrative|e:all|v:off,t:poi|e:all|v:off,t:background|e:all|v:off,t:subway|e:all|v:off,t:railway|e:all|v:off,t:local|e:all|v:off,t:arterial|e:l|v:off,t:highway|e:all|v:off";
                //北京URL---------普通道路
                //String originlink = "http://api{0}.map.bdimg.com/customimage/tile?&x={1}&y={2}&z={3}&udt=20180403&scale=1&ak=8d6c8b8f3749aed6b1aff3aad6f40e37&styles=t:administrative|e:all|v:off,t:poi|e:all|v:off,t:background|e:all|v:off,t:subway|e:all|v:off,t:railway|e:all|v:off,t:local|e:l|v:off,t:arterial|e:all|v:off,t:highway|e:all|v:off";
                //北京URL---------铁路
                //String originlink = "http://api{0}.map.bdimg.com/customimage/tile?&x={1}&y={2}&z={3}&udt=20180403&scale=1&ak=8d6c8b8f3749aed6b1aff3aad6f40e37&styles=t:administrative|e:all|v:off,t:poi|e:all|v:off,t:background|e:all|v:off,t:subway|e:all|v:off,t:railway|e:l|v:off,t:local|e:all|v:off,t:arterial|e:all|v:off,t:highway|e:all|v:off";
                //北京URL---------地铁
                //String originlink = "http://api{0}.map.bdimg.com/customimage/tile?&x={1}&y={2}&z={3}&udt=20180403&scale=1&ak=8d6c8b8f3749aed6b1aff3aad6f40e37&styles=t:administrative|e:all|v:off,t:poi|e:all|v:off,t:background|e:all|v:off,t:subway|e:l|v:off,t:railway|e:all|v:off,t:local|e:all|v:off,t:arterial|e:all|v:off,t:highway|e:all|v:off";
                //北京URL---------建筑物
                //String originlink = "http://api{0}.map.bdimg.com/customimage/tile?&x={1}&y={2}&z={3}&udt=20180403&scale=1&ak=8d6c8b8f3749aed6b1aff3aad6f40e37&styles=t:administrative|e:all|v:off,t:poi|e:all|v:off,t:road|e:all|v:off,t:manmade|e:all|v:off,t:green|e:all|v:off,t:water|e:all|v:off,t:land|e:all|v:off";
                //北京URL---------绿地
                //String originlink = "http://api{0}.map.bdimg.com/customimage/tile?&x={1}&y={2}&z={3}&udt=20180403&scale=1&ak=8d6c8b8f3749aed6b1aff3aad6f40e37&styles=t:administrative|e:all|v:off,t:poi|e:all|v:off,t:road|e:all|v:off,t:manmade|e:all|v:off,t:building|e:all|v:off,t:water|e:all|v:off,t:land|e:all|v:off";
                //北京URL---------水系
                //String originlink = "http://api{0}.map.bdimg.com/customimage/tile?&x={1}&y={2}&z={3}&udt=20180403&scale=1&ak=8d6c8b8f3749aed6b1aff3aad6f40e37&styles=t:administrative|e:all|v:off,t:poi|e:all|v:off,t:road|e:all|v:off,t:manmade|e:all|v:off,t:building|e:all|v:off,t:green|e:all|v:off,t:land|e:all|v:off";
                //北京URL---------人造区域
                //String originlink = "http://api{0}.map.bdimg.com/customimage/tile?&x={1}&y={2}&z={3}&udt=20180403&scale=1&ak=8d6c8b8f3749aed6b1aff3aad6f40e37&styles=t:administrative|e:all|v:off,t:poi|e:all|v:off,t:road|e:all|v:off,t:building|e:all|v:off,t:green|e:all|v:off,t:water|e:all|v:off,t:land|e:all|v:off,t:manmade|e:l|v:off";
                //北京URL---------东城区所有要素
                //String originlink = "http://api{0}.map.bdimg.com/customimage/tile?&x={1}&y={2}&z={3}&udt=20180403&scale=1&ak=8d6c8b8f3749aed6b1aff3aad6f40e37&customid=undefined";
                //String savedir = "E:\\BaiduMap\\Beijing\\DongCheng\\ALL\\tiles";

                Thread[] t = new Thread[Request.TaskCount];
                for (int num = 0; num < Request.TaskCount; num++)
                {
                    try
                    {
                        t[num] = new Thread(new ParameterizedThreadStart(run_BTile))
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
                        new Log<Baidumap_TileRequest>(threadlog);

                        log.Status = "错误";
                        log.ErrorMsg = ex.ToString();
                        log.ErrorDate = DateTime.Now.ToString();
                        //操作日志
                        new Log<Baidumap_TileRequest>(log);

                        return new WebApiResult<string>() { success = 0, msg = ex.ToString() };
                    }
                }
                countdown.Wait();
                for (int num = 0; num < Request.TaskCount; num++)
                {
                    t[num].Abort();
                }
                lock (obj)
                {
                    if (!Log<Baidumap_TileRequest>.GetThreadLogEntity(this.request_BTile.GUID).IsPaused)
                    {
                            log.Status = "已完成";
                            log.CompleteTime = DateTime.Now.ToString();
                            log.Current = log.Count;
                            log.ErrorMsg = "";
                            threadlog.Status = "已完成";
                            threadlog.TStatus = 2;
                            threadlog.Current = threadlog.Total;
                            threadlog.Current_loc = List2Str(current_loc);
                            //操作日志
                            new Log<Baidumap_TileRequest>(threadlog);
                            new Log<Baidumap_TileRequest>(log);
                           return new WebApiResult<string>() { success = 1, msg = "百度道路瓦片下载完成！" };
                    }
                }
            }
            catch (Exception ex)
            {
                log.Status = "错误";
                log.ErrorMsg = ex.ToString();
                log.ErrorDate = DateTime.Now.ToString();
                //操作日志
                new Log<Baidumap_TileRequest>(log);

                return new WebApiResult<string>() { success = 0, msg = ex.ToString() };
            }
            return null;
        }

        public void run_BTile(object num)
        {
            string originlink = "http://api{0}.map.bdimg.com/customimage/tile?&x={1}&y={2}&z={3}&scale=1&ak=8d6c8b8f3749aed6b1aff3aad6f40e37&";
            double[] interval = new double[] { (endcoord.Lat - startcoord.Lat) / N, (endcoord.Lng - startcoord.Lng) / N };
            var scoord = new LatLngPoint(this.startcoord.Lat, this.startcoord.Lng + interval[1] * (int)num);
            var ecoord = new LatLngPoint(this.endcoord.Lat, this.startcoord.Lng + interval[1] * ((int)num + 1));
            if (ecoord.Lng > endcoord.Lng) ecoord.Lng = endcoord.Lng;

            if (this.current_loc != null)
            {
                if (Contains(current_loc, 11) >= 0)
                {
                    var index = Contains(current_loc, 11);
                    current_i = this.current_loc[index][1];
                    current_j = this.current_loc[index][2];
                }
            }

            Tuple<int, int, int, int> BoundTup = GetTileBound(scoord, ecoord, 11);

            for (int i = BoundTup.Item1; i <= BoundTup.Item3; i++)
            {
                if (i < current_i) continue;

                for (int j = BoundTup.Item2; j <= BoundTup.Item4; j++)
                {
                    if (i == current_i && j <= current_j) continue;

                    if (Log<Baidumap_TileRequest>.GetThreadLogEntity(this.request_BTile.GUID).IsPaused)
                    {

                        threadlog.Current = current;
                        threadlog.Current_loc = List2Str(current_loc);
                        threadlog.Status = "暂停";
                        threadlog.TStatus = 4;
                        threadlog.IsPaused = true;
                        new Log<Baidumap_TileRequest>(threadlog);

                        log.Status = "未完成";
                        log.Current = current;
                        new Log<Baidumap_TileRequest>(log);

                        countdown.Signal();
                        return;
                    }

                    try
                    {
                        lock (obj)
                        {
                            current++;
                            threadlog.Current = current;
                            new Log<Baidumap_TileRequest>(threadlog);
                            if (Contains(current_loc, 11) >= 0)
                            {
                                var index = Contains(current_loc, 11);
                                current_loc[index][1] = i;
                                current_loc[index][2] = j;
                            }
                            else
                            {
                                if (current_loc == null) current_loc = new List<int[]>();
                                current_loc.Add(new int[3] { 11, i, j });
                            }
                        }
                        String link = String.Format(originlink, Math.Abs(i + j) % 3, i, j, 11);
                        String localpath = this.request_BTile.SavePathText + String.Format("\\{0}\\{1}\\", 11, i);
                        String filename = String.Format("{0}.png", j);
                        //判断文件是否存在，若存在，直接下载下一个文件
                        if (File.Exists(localpath + filename)) continue;
                        //下载文件
                        String downloadedfile = "";
                        int c = 0;
                        do
                        {
                            downloadedfile = DownloadFile(link, localpath, filename);
                            Thread.Sleep(100);
                            try
                            {
                                Image img = Image.FromFile(localpath + filename);
                            }
                            catch (Exception ex)
                            {
                                File.Delete(localpath + filename);
                                downloadedfile = "";

                                log.Status = "错误";
                                log.ErrorMsg = localpath + filename + "下载失败： "+ ex.ToString();
                                log.ErrorDate = DateTime.Now.ToString();
                                //操作日志
                                new Log<Baidumap_TileRequest>(log);
                            }
                            c++;
                        } while (downloadedfile == "" && c <= 20);
                        if (downloadedfile == "")
                            error_loc.Add(new int[3] { 11, i, j });

                    }
                    catch (Exception ex)
                    {
                        log.Status = "错误";
                        log.ErrorMsg = ex.ToString();
                        log.ErrorDate = DateTime.Now.ToString();
                        log.Current = current;
                        //操作日志
                        new Log<Baidumap_TileRequest>(log);

                        threadlog.Current = current;
                        threadlog.Current_loc = List2Str(current_loc);
                        threadlog.Status = "错误";
                        threadlog.TStatus = 3;
                        threadlog.IsPaused = true;
                        new Log<Baidumap_TileRequest>(threadlog);
                        for (int k = 0; k < countdown.CurrentCount; k++)
                            countdown.Signal();
                        return;
                    }
                }
            }

            for (int l = 0; l < error_loc.Count; l++)
            {
                string error_msg="";
                int Level = error_loc[l][0];
                int i = error_loc[l][1];
                int j = error_loc[l][2];
                String link = String.Format(originlink, Math.Abs(i + j) % 3, i, j, 11);
                String localpath = this.request_BTile.SavePathText + String.Format("\\{0}\\{1}\\", 11, i);
                String filename = String.Format("{0}.png", j);
                //下载文件
                String downloadedfile = "";
                int c = 0;
                do
                {
                    downloadedfile = DownloadFile(link, localpath, filename);
                    Thread.Sleep(100);
                    try
                    {
                        Image img = Image.FromFile(localpath + filename);
                    }
                    catch (Exception ex)
                    {
                        File.Delete(localpath + filename);
                        downloadedfile = "";
                        error_msg = localpath + filename + "下载失败： " + ex.ToString();
                    }
                    c++;
                } while (downloadedfile == "" && c <= 20);
                if (downloadedfile == "")
                {
                    log.Status = "错误";
                    log.ErrorMsg += " "+error_msg+";";
                    log.ErrorDate = DateTime.Now.ToString();
                    new Log<Baidumap_TileRequest>(log);
                }
            }
            countdown.Signal();
        }
        

        Baidumap_BuildingTileRequest request_BBuildingTile;
        /// <summary>
        /// 百度建筑瓦片
        /// </summary>
        /// <returns></returns>
        public WebApiResult<string> Baidumap_BuildingTile_MenuItem(Baidumap_BuildingTileRequest Request)
        {

            this.countdown = new CountdownEvent(Request.TaskCount);
            this.request_BBuildingTile = Request;
            this.N = Request.TaskCount;

            UpdateLastLoc<Baidumap_BuildingTileRequest>(Request.GUID);

            log = new TaskLogEntity() { GUID = Request.GUID, Name = Request.TName, Type = "百度瓦片下载", Description = "Baidumap_BuildingTile_MenuItem", Status = "进行中", Parameter = JsonHelper.ToJson(Request), SavePath = Request.SavePathText };
            //操作日志
            new Log<Baidumap_BuildingTileRequest>(log);
                        
            var restul = new WebApiResult<string>() { success = 0, msg = "百度建筑瓦片下载失败或未完成！" };

            try
            {
                //武汉地区坐标范围信息
                //LatLngPoint startcoord = new LatLngPoint(29.972313, 113.707018);
                //LatLngPoint endcoord = new LatLngPoint(31.367044, 115.087233);
                ////恩施地区坐标范围信息
                //LatLngPoint startcoord = new LatLngPoint(Request.startcoord.Lat, Request.startcoord.Lng);
                //LatLngPoint endcoord = new LatLngPoint(Request.endcoord.Lat, Request.endcoord.Lng);

                startcoord = new LatLngPoint(29.972313, 113.707018);
                endcoord = new LatLngPoint(30.367044, 114.087233);
                int z = 17;
                Tuple<int, int, int, int> BoundTup = GetTileBound(startcoord, endcoord, z);
                
                threadlog = new ThreadTaskLogEntity() { GUID = Request.GUID, TaskLog_GUID = Request.GUID, Status = "进行中", TStatus=1, Total = 0, TName = Request.TName, IsPaused = false, Parameter = JsonHelper.ToJson(Request), URL = Request.URL };
                //计算Total并更新 
                threadlog.Total += (BoundTup.Item3 - BoundTup.Item1 + 1) * (BoundTup.Item4 - BoundTup.Item2 + 1);
                new Log<Baidumap_BuildingTileRequest>(threadlog);
                log.Count = threadlog.Total;
                new Log<Baidumap_BuildingTileRequest>(log);

                //构建下载链接，循环请求获取瓦片地图
                //String link = "http://online{0}.map.bdimg.com/onlinelabel/?qt=tile&x={1}&y={2}&z={3}&styles=pl&udt=20171031&scaler=1&p=0";
                //String originlink = "http://api{0}.map.bdimg.com/customimage/tile?&x={1}&y={2}&z={3}&udt=20171224&scale=1&ak=8d6c8b8f3749aed6b1aff3aad6f40e37&styles=t%3Aadministrative%7Ce%3Aall%7Cv%3Aoff%2Ct%3Apoi%7Ce%3Aall%7Cv%3Aoff%2Ct%3Aroad%7Ce%3Aall%7Cv%3Aoff%2Ct%3Aland%7Ce%3Aall%7Cv%3Aoff%2Ct%3Awater%7Ce%3Aall%7Cv%3Aoff%2Ct%3Agreen%7Ce%3Aall%7Cv%3Aoff%2Ct%3Amanmade%7Ce%3Aall%7Cv%3Aoff%2Ct%3Abuilding%7Ce%3Aall%7Cv%3Aon%7Cc%3A%23ffffffff";
                //String savedir = "E:\\BaiduMap20171224\\Building\\tiles";
                //恩施URL
                //String originlink = "http://api{0}.map.bdimg.com/customimage/tile?&x={1}&y={2}&z={3}&udt=20171214&scale=1&ak=8d6c8b8f3749aed6b1aff3aad6f40e37&styles=t%3Aadministrative%7Ce%3Aall%7Cv%3Aoff%2Ct%3Apoi%7Ce%3Aall%7Cv%3Aoff%2Ct%3Aroad%7Ce%3Aall%7Cv%3Aoff%2Ct%3Amanmade%7Ce%3Aall%7Cv%3Aoff%2Ct%3Abuilding%7Ce%3Aall%7Cv%3Aoff";
                //String savedir = "E:\\BaiduMap20171214\\GreenWater\\tiles";

                Thread[] t = new Thread[Request.TaskCount];
                for (int num = 0; num < Request.TaskCount; num++)
                {
                    try
                    {
                        t[num] = new Thread(new ParameterizedThreadStart(run_BBuildingTile))
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
                        new Log<Baidumap_BuildingTileRequest>(threadlog);

                        log.Status = "错误";
                        log.ErrorMsg = ex.ToString();
                        log.ErrorDate = DateTime.Now.ToString();
                        //操作日志
                        new Log<Baidumap_BuildingTileRequest>(log);
                        
                        restul = new WebApiResult<string>() { success = 0, msg = ex.ToString() };
                    }
                }
                countdown.Wait();
                for (int num = 0; num < Request.TaskCount; num++)
                {
                    t[num].Abort();
                }
                lock (obj)
                {
                    if (!Log<Baidumap_BuildingTileRequest>.GetThreadLogEntity(this.request_BTile.GUID).IsPaused)
                    {
                            log.Status = "已完成";
                            log.CompleteTime = DateTime.Now.ToString();
                            log.Current = log.Count;
                            log.ErrorMsg = "";
                            threadlog.Status = "已完成";
                            threadlog.TStatus = 2;
                            threadlog.Current = threadlog.Total;
                            threadlog.Current_loc = List2Str(current_loc);
                            //操作日志
                            new Log<Baidumap_BuildingTileRequest>(threadlog);
                            new Log<Baidumap_BuildingTileRequest>(log);

                            restul = new WebApiResult<string>() { success = 1, msg = "百度建筑瓦片下载完成！" };
                        
                    }
                }

                
            }
            catch (Exception ex)
            {
                new Log<Baidumap_BuildingTileRequest>(new TicketEntity.Common.TaskLogEntity() { GUID = Request.GUID, Name = "百度建筑瓦片", Type = "错误日志", Description = "Baidumap_BuildingTile_MenuItem", Status = "错误", ErrorMsg = ex.ToString(), ErrorDate = DateTime.Now.ToString() });
                restul = new WebApiResult<string>() { success = 0, msg = ex.ToString() };
            }

            return restul; 
        }

        void run_BBuildingTile(object num)
        {
            double[] interval = new double[] { (endcoord.Lat - startcoord.Lat) / N, (endcoord.Lng - startcoord.Lng) / N };
            var scoord = new LatLngPoint(this.startcoord.Lat, this.startcoord.Lng + interval[1] * (int)num);
            var ecoord = new LatLngPoint(this.endcoord.Lat, this.startcoord.Lng + interval[1] * ((int)num + 1));
            if (ecoord.Lng > endcoord.Lng) ecoord.Lng = endcoord.Lng;

            if (this.current_loc != null)
            {
                if (Contains(current_loc, 17) >= 0)
                {
                    var index = Contains(current_loc, 17);
                    current_i = this.current_loc[index][1];
                    current_j = this.current_loc[index][2];
                }
            }

            Tuple<int, int, int, int> BoundTup = GetTileBound(scoord, ecoord, 17);

            for (int i = BoundTup.Item1; i <= BoundTup.Item3; i++)
            {
                if (i < current_i) continue;

                for (int j = BoundTup.Item2; j <= BoundTup.Item4; j++)
                {
                    if (i == current_i && j <= current_j) continue;

                    if (Log<Baidumap_BuildingTileRequest>.GetThreadLogEntity(this.request_BBuildingTile.GUID).IsPaused)
                    {

                        threadlog.Current = current;
                        threadlog.Current_loc = List2Str(current_loc);
                        threadlog.Status = "暂停";
                        threadlog.TStatus = 4;
                        threadlog.IsPaused = true;
                        new Log<Baidumap_BuildingTileRequest>(threadlog);

                        log.Status = "未完成";
                        log.Current = current;
                        new Log<Baidumap_BuildingTileRequest>(log);
                        for (int k = 0; k < countdown.CurrentCount; k++)
                            countdown.Signal();
                        return;
                    }

                    try
                    {
                        lock (obj)
                        {
                            current++;
                            threadlog.Current = current;
                            new Log<Baidumap_BuildingTileRequest>(threadlog);
                            if (Contains(current_loc, 17) >= 0)
                            {
                                var index = Contains(current_loc, 17);
                                current_loc[index][1] = i;
                                current_loc[index][2] = j;
                            }
                            else
                            {
                                if (current_loc == null) current_loc = new List<int[]>();
                                current_loc.Add(new int[3] { 17, i, j });
                            }
                        }

                        //i = 23805;
                        //j = 6870;
                        String link = String.Format(this.request_BBuildingTile.originlink, Math.Abs(i + j) % 3, i, j, 17);
                        String localpath = this.request_BBuildingTile.SavePathText + String.Format("\\{0}\\{1}\\", 17, i);
                        String filename = String.Format("{0}.png", j);
                        //判断文件是否存在，若存在，直接下载下一个文件
                        if (File.Exists(localpath + filename)) continue;
                        //下载文件
                        String downloadedfile = "";
                        int c = 0;
                        do
                        {
                            downloadedfile = DownloadFile(link, localpath, filename);
                            Thread.Sleep(100);
                            try
                            {
                                Image img = Image.FromFile(localpath + filename);
                            }
                            catch (Exception ex)
                            {
                                File.Delete(localpath + filename);
                                downloadedfile = "";

                                log.Status = "错误";
                                log.ErrorMsg = localpath + filename + "下载失败： " + ex.ToString();
                                log.ErrorDate = DateTime.Now.ToString();
                                //操作日志
                                new Log<Baidumap_BuildingTileRequest>(log);
                            }
                            c++;
                        } while (downloadedfile == "" && c<= 20 );
                        if (downloadedfile == "")
                            error_loc.Add(new int[3] { 17, i, j });

                    }
                    catch (Exception ex)
                    {
                        new Log<Baidumap_BuildingTileRequest>(new TicketEntity.Common.TaskLogEntity() { GUID = this.request_BBuildingTile.GUID, Name = "百度建筑瓦片", Type = "错误日志", Description = "Baidumap_BuildingTile_MenuItem", Status = "错误", ErrorMsg = ex.ToString(), ErrorDate = DateTime.Now.ToString() });

                        threadlog.Current = current;
                        threadlog.Current_loc = List2Str(current_loc);
                        threadlog.Status = "错误";
                        threadlog.TStatus = 3;
                        threadlog.IsPaused = true;
                        new Log<Baidumap_BuildingTileRequest>(threadlog);
                        log.Current = current;
                        new Log<Baidumap_BuildingTileRequest>(log);
                        for (int k = 0; k < countdown.CurrentCount; k++)
                            countdown.Signal();
                        return;
                    }
                }
            }

            for (int l = 0; l < error_loc.Count; l++)
            {
                string error_msg="";
                int Level = error_loc[l][0];
                int i = error_loc[l][1];
                int j = error_loc[l][2];
                String link = String.Format(this.request_BBuildingTile.originlink, Math.Abs(i + j) % 3, i, j, 17);
                String localpath = this.request_BBuildingTile.SavePathText + String.Format("\\{0}\\{1}\\", 17, i);
                String filename = String.Format("{0}.png", j);
                //下载文件
                String downloadedfile = "";
                int c = 0;
                do
                {
                    downloadedfile = DownloadFile(link, localpath, filename);
                    Thread.Sleep(100);
                    try
                    {
                        Image img = Image.FromFile(localpath + filename);
                    }
                    catch (Exception ex)
                    {
                        File.Delete(localpath + filename);
                        downloadedfile = "";
                        error_msg = localpath + filename + "下载失败： " + ex.ToString();
                    }
                    c++;
                } while (downloadedfile == "" && c <= 20);
                if (downloadedfile == "")
                {
                    log.Status = "错误";
                    log.ErrorMsg += " "+error_msg+";";
                    log.ErrorDate = DateTime.Now.ToString();
                    new Log<Baidumap_BuildingTileRequest>(log);
                }
            }

            countdown.Signal();
        }

        Amap_POIRequest request_AmapPOI;
        /// <summary>
        /// 高德POI
        /// </summary>
        /// <returns></returns>
        public WebApiResult<string> Amap_POI_MenuItem(Amap_POIRequest Request)
        {
            this.countdown = new CountdownEvent(Request.TaskCount);
            this.request_AmapPOI = Request;
            this.N = Request.TaskCount;

            UpdateLastLoc<Amap_POIRequest>(Request.GUID);

            int cityid = int.Parse(GetList("SELECT baiduId FROM cityinform WHERE Name = " + "'" + this.request_AmapPOI.City + "'", "baiduId")[0]);
            //获取大类信息存储到mapbar_poiclass
            List<String> kwList = GetList("select KWName,KWType from mapbar_poi WHERE BaiduId = " + cityid, "KWName");
            log = new TaskLogEntity() { GUID = Request.GUID, Name = Request.TName, Type = "POI", Description = "Amap_POI_MenuItem", Status = "进行中", Parameter = JsonHelper.ToJson(Request) };
            threadlog = new ThreadTaskLogEntity(){ GUID= Request.GUID,TaskLog_GUID = Request.GUID,Status = "进行中", TStatus=1, TName = Request.TName, Total = 0,IsPaused = false, Parameter = JsonHelper.ToJson(Request), URL = Request.URL };
            //计算Total并更新 
            threadlog.Total = kwList.Count;
            log.Count = threadlog.Total;
            new Log<Amap_POIRequest>(threadlog);
            new Log<Amap_POIRequest>(log);
            
            try
            {
                Thread[] t = new Thread[Request.TaskCount];
                for (int num = 0; num < Request.TaskCount; num++)
                {
                    try
                    {
                        t[num] = new Thread(new ParameterizedThreadStart(run_AmapPOI))
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
                        new Log<Amap_POIRequest>(threadlog);
                    }
                }
                countdown.Wait();
                for (int num = 0; num < Request.TaskCount; num++)
                {
                    t[num].Abort();
                }
                lock (obj)
                {
                    if (!Log<Amap_POIRequest>.GetThreadLogEntity(this.request_AmapPOI.GUID).IsPaused)
                    {
                        log.Status = "已完成";
                        log.CompleteTime = DateTime.Now.ToString();
                        log.Current = log.Count;
                        threadlog.Status = "已完成";
                        threadlog.TStatus = 2;
                        threadlog.Current = threadlog.Total;
                        threadlog.Current_loc = List2Str(current_loc);
                        //操作日志
                        new Log<Amap_POIRequest>(threadlog);
                        new Log<Amap_POIRequest>(log);
                    }
                }
            }
            catch (Exception ex)
            {
                //操作日志
                log.Status = "错误";
                log.ErrorDate = DateTime.Now.ToString();
                log.ErrorMsg = ex.ToString();
                new Log<Amap_POIRequest>(log);
            }

            return null;
        }

        public void run_AmapPOI(object num)
        {
            int cityid = int.Parse(GetList("SELECT baiduId FROM cityinform WHERE Name = " + "'" + this.request_AmapPOI.City + "'", "baiduId")[0]);
            //获取mapbar_poi中已有的关键字信息
            List<String> kwList = GetList("select KWName,KWType from mapbar_poi WHERE BaiduId = " + cityid, "KWName");
            //获取amap_poi中已有的POI列表
            List<String> ampoilist = GetList("select id,version from amap_poi", "id", "version");
            //amap字段
            String[] strArr = { "rate", "tel", "typecode", "areacode", "cityname", "longitude", "address", "id", "name", "adcode", "newtype", "disp_name", "latitude" };

            int interval = (int)Math.Ceiling(kwList.Count / (double)N);
            int start = 0 + interval * (int)num;
            int end = start + interval;
            if (end > kwList.Count) end = kwList.Count;

            //遍历关键字字典，基于关键字搜索高德地图POI
            for (int i = start; i < end; i++)
            {
                try
                {
                    int pagenum = 1;
                    int totalnum = 0;
                    int currentPage = 0;

                    if (this.current_loc != null)
                    {
                        if (Contains(current_loc, i) >= 0)
                        {
                            var index = Contains(current_loc, i);
                            currentPage = current_loc[index][1];
                        }
                    }

                    do
                    {
                        if (pagenum < currentPage)
                        {
                            pagenum = pagenum + 1;
                            continue;
                        }

                        if (Log<Amap_POIRequest>.GetThreadLogEntity(this.request_AmapPOI.GUID).IsPaused)
                        {
                            threadlog.Current = current;
                            threadlog.Current_loc = List2Str(current_loc);
                            threadlog.Status = "暂停";
                            threadlog.TStatus = 4;
                            threadlog.IsPaused = true;
                            new Log<Amap_POIRequest>(threadlog);
                            log.Status = "未完成";
                            log.Current = current;
                            new Log<Amap_POIRequest>(log);

                            countdown.Signal();
                            return;
                        }


                        string city_id = GetList("SELECT gaodeId FROM cityinform WHERE Name = " + "'" + this.request_AmapPOI.City + "'", "gaodeId")[0];
                        //基于关键字搜索高德地图POI,并将返回结果序列化为JSON
                        String curl = AmapURL + "&city=" + city_id + "&keywords=" + HttpUtility.UrlEncode(kwList[i]) + "&pagenum=" + pagenum.ToString();
                        String curpage = GetHttpResponse(curl);
                        JObject jobject = (JObject)JsonConvert.DeserializeObject(curpage);

                        //当查询次数超过阈值时，需破解极验验证码
                        if (jobject["data"].ToString() == "too fast")
                        {
                            String referer = "";
                            do
                            {
                                referer = AmapVerifyURL + HttpUtility.UrlEncode("http://ditu.amap.com/search?query=" + kwList[i]) + "&channel=newpc&uuid=322a41da-6f8d-4388-b5a7-e1708309cf4f&url=" + curl.Substring(20);
                                curpage = GetHttpResponse(referer);
                                String[] strgt = curpage.Split(new String[] { "//api.geetest.com/get.php?gt=", "&product=embed" }, StringSplitOptions.None);
                                String cverifyurl = "http://jiyanapi.c2567.com/shibie?user=zeswang&pass=200632590117&return=json&gt=" + strgt[1] + "&referer=http://ditu.amap.com";
                                curpage = GetHttpResponse(cverifyurl);
                                jobject = (JObject)JsonConvert.DeserializeObject(curpage);
                            } while (jobject["status"].ToString() == "no");
                            String posturl = "http://ditu.amap.com/gt?rand=" + rm.NextDouble().ToString();
                            String geetest_challenge = jobject["challenge"].ToString();
                            String geetest_validate = jobject["validate"].ToString();
                            String geetest_seccode = geetest_validate + "|jordan";
                            curpage = GetHttpResponse(posturl, geetest_challenge, geetest_validate, geetest_seccode, referer);
                            JObject verifyjo = (JObject)JsonConvert.DeserializeObject(curpage);
                            if (verifyjo["errmsg"].ToString() == "OK")
                            {
                                curpage = GetHttpResponse(curl);
                                jobject = (JObject)JsonConvert.DeserializeObject(curpage);
                            }
                        }

                        if (Convert.ToInt32(jobject["status"].ToString()) == 2) break;
                        ///获取返回记录的数量
                        totalnum = Convert.ToInt32(jobject["data"]["total"].ToString());
                        if (totalnum == 0 || jobject["data"]["poi_list"] == null) break;
                        

                        lock (obj)
                        {
                            if (Contains(current_loc, i) >= 0)
                            {
                                var index = Contains(current_loc, i);
                                current_loc[index][1] = pagenum;
                            }
                            else
                            {
                                if (current_loc == null) current_loc = new List<int[]>();
                                current_loc.Add(new int[3] { i, pagenum, totalnum });
                            }
                        }

                        String version = jobject["data"]["version"].ToString();
                        JEnumerable<JToken> poi_list = jobject["data"]["poi_list"].Children();
                        foreach (JToken poi in poi_list)
                        {
                            //获取高德POI的id和version，判断是否存已在于amap_poi中，若存在，直接跳过，不存在则进行采集存储
                            String idversion = GetInfotoString("id", poi) + "," + version;
                            if (ampoilist.Contains(idversion)) continue;
                            ampoilist.Add(idversion);
                            //解析JSON，获取数据库字段值
                            List<MySqlParameter> listParam = new List<MySqlParameter>();
                            foreach (String s in strArr)
                            {
                                listParam.Add(new MySqlParameter(@s, GetInfotoString(s, poi)));
                            }
                            listParam.Add(new MySqlParameter("version", version));
                            //插入数据库
                            String sqlcommand = "insert into amap_poi(rate, tel, typecode, areacode, cityname, longitude, address, id, name, adcode, newtype, disp_name, latitude, version) values(@rate, @tel, @typecode, @areacode, @cityname, @longitude, @address, @id, @name, @adcode, @newtype, @disp_name, @latitude, @version)";
                            mysqlHelper.ExecuteNonQuery(sqlcommand, listParam.ToArray());
                        }
                        pagenum = pagenum + 1;
                    } while (pagenum <= Math.Ceiling(totalnum / 30.0));

                    lock(obj)
                    {
                        current++;//classlist
                        threadlog.Current = current;
                        new Log<Amap_POIRequest>(threadlog);
                    }
                }
                catch (Exception ex)
                {
                    log.Status = "错误";
                    log.ErrorMsg = ex.ToString();
                    log.ErrorDate = DateTime.Now.ToString();
                    new Log<Amap_POIRequest>(log);

                    threadlog.Current = current;
                    threadlog.Current_loc = List2Str(current_loc);
                    threadlog.Status = "错误";
                    threadlog.TStatus = 3;
                    threadlog.IsPaused = true;
                    new Log<Amap_POIRequest>(threadlog);
                    for (int k = 0; k < countdown.CurrentCount; k++)
                        countdown.Signal();
                    return;
                }
            }
            countdown.Signal();
        }


        Amap_TileRequest request_ATile;
        /// <summary>
        /// 高德瓦片
        /// </summary>
        /// <returns></returns>
        public WebApiResult<string> Amap_Tile_MenuItem(Amap_TileRequest Request)
        {
            this.countdown = new CountdownEvent(Request.TaskCount);
            this.request_ATile = Request;
            this.N = Request.TaskCount;

            UpdateLastLoc<Amap_TileRequest>(Request.GUID);

            log = new TaskLogEntity() { GUID = Request.GUID, Name = Request.TName, Type = "高德瓦片下载", Description = "Amap_Tile_MenuItem", Status = "进行中", Parameter = JsonHelper.ToJson(Request), SavePath = Request.SavePathText };
            //操作日志
            new Log<Amap_TileRequest>(log);
            
            try
            {
                //确定要下载的坐标范围信息，起始坐标为左下角，终点坐标为右上角
                double startcoord_x = Request.LefttextBox;
                double startcoord_y = Request.BottomtextBox;
                double endcoord_x = Request.RighttextBox;
                double endcoord_y = Request.UptextBox;

                if (startcoord_x > 0 && startcoord_x < endcoord_x && endcoord_x < 180 && startcoord_y > 0 && startcoord_y < endcoord_y && endcoord_y < 90)
                {
                    this.startcoord = new LatLngPoint(startcoord_y, startcoord_x);
                    this.endcoord = new LatLngPoint(endcoord_y, endcoord_x);
                }
                else
                {
                    return new WebApiResult<string>() { success = 2, msg = "输入坐标值不正确，请检查！" };
                }

                //startcoord = new LatLngPoint(29.120116, 108.374308);
                //endcoord = new LatLngPoint(31.402847, 110.649141);

                //武汉地区坐标范围信息
                //LatLngPoint startcoord = new LatLngPoint(Request.startcoord.Lat, Request.startcoord.Lng);
                //LatLngPoint endcoord = new LatLngPoint(Request.endcoord.Lat, Request.endcoord.Lng);

                threadlog = new ThreadTaskLogEntity() { GUID = Request.GUID, TaskLog_GUID = Request.GUID, Status = "进行中", TStatus=1, Total = 0, TName = Request.TName, IsPaused = false, Parameter = JsonHelper.ToJson(Request), URL = Request.URL };
                //计算Total并更新 
                foreach (int Level in Request.LevelList)
                {
                    Point StartPoint = CoordTransferHelper.LatLng2TileXY(startcoord, Level);
                    Point EndPoint = CoordTransferHelper.LatLng2TileXY(endcoord, Level);
                    threadlog.Total += (EndPoint.X - StartPoint.X + 1) * (StartPoint.Y - EndPoint.Y + 1);
                }
                new Log<Amap_TileRequest>(threadlog);
                log.Count = threadlog.Total;
                new Log<Amap_TileRequest>(log);

                Thread[] t = new Thread[Request.TaskCount];
                for (int num = 0; num < Request.TaskCount; num++)
                {
                    try
                    {
                        t[num] = new Thread(new ParameterizedThreadStart(run_ATile))
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
                        new Log<Amap_TileRequest>(threadlog);

                        log.Status = "错误";
                        log.ErrorMsg = ex.ToString();
                        log.ErrorDate = DateTime.Now.ToString();
                        new Log<Amap_TileRequest>(log);

                        return new WebApiResult<string>() { success = 0, msg = ex.ToString() };
                    }
                }
                countdown.Wait();
                for (int num = 0; num < Request.TaskCount; num++)
                {
                    t[num].Abort();
                }
                lock (obj)
                {
                    if (!Log<Amap_TileRequest>.GetThreadLogEntity(this.request_ATile.GUID).IsPaused)
                    {
                            log.Status = "已完成";
                            log.CompleteTime = DateTime.Now.ToString();
                            log.Current = log.Count;
                            log.ErrorMsg = "";
                            threadlog.Status = "已完成";
                            threadlog.TStatus = 2;
                            threadlog.Current = threadlog.Total;
                            threadlog.Current_loc = List2Str(current_loc);
                            //操作日志
                            new Log<Amap_TileRequest>(threadlog);
                            new Log<Amap_TileRequest>(log);
                            return new WebApiResult<string>() { success = 1, msg = "高德瓦片下载完成！" };
                    }                
                }
            }
            catch (Exception ex)
            {
                //操作日志
                log.Status = "错误";
                log.ErrorDate = DateTime.Now.ToString();
                log.ErrorMsg = ex.ToString();
                new Log<Amap_TileRequest>(log);
                return new WebApiResult<string>() { success = 0, msg = ex.ToString() };
            }

            return null; 
        }

        void run_ATile(object num)
        {
            //构建下载链接，循环请求获取瓦片地图
            //String originlink = "http://wprd0{0}.is.autonavi.com/appmaptile?lang=zh_cn&size=1&style=7&x={1}&y={2}&z={3}&scl=1&ltype=2";
            String originlink = "http://wprd0{0}.is.autonavi.com/appmaptile?lang=zh_cn&size=1&x={1}&y={2}&z={3}&scl=1" + this.request_ATile.LayerStr;
            //String savedir = "E:\\AMap20171110\\tiles";
            double[] interval = new double[] { (endcoord.Lat - startcoord.Lat) / N, (endcoord.Lng - startcoord.Lng) / N };
            var scoord = new LatLngPoint(this.startcoord.Lat, this.startcoord.Lng + interval[1] * (int)num);
            var ecoord = new LatLngPoint(this.endcoord.Lat, this.startcoord.Lng + interval[1] * ((int)num + 1));
            if (ecoord.Lng > endcoord.Lng) ecoord.Lng = endcoord.Lng;

            //获取要下载的地图层级
            for (int l = 0; l < this.request_ATile.LevelList.Length; l++)
            {
                int Level = this.request_ATile.LevelList[l];

                if (this.current_loc != null)
                {
                    if (Contains(current_loc, Level) >= 0)
                    {
                        var index = Contains(current_loc, Level);
                        current_i = this.current_loc[index][1];
                        current_j = this.current_loc[index][2];
                    }
                }

                Point StartPoint = CoordTransferHelper.LatLng2TileXY(startcoord, Level);
                Point EndPoint = CoordTransferHelper.LatLng2TileXY(endcoord, Level);

                for (int i = StartPoint.X; i <= EndPoint.X; i++)
                {
                    if (i < current_i) continue;

                    for (int j = EndPoint.Y; j <= StartPoint.Y; j++)
                    {
                        if (i == current_i && j <= current_j) continue;
                        //if (i == 104993 && j == 53521) Pause<Amap_TileRequest>(this.request_ATile.GUID);

                        if (Log<Amap_TileRequest>.GetThreadLogEntity(this.request_ATile.GUID).IsPaused)
                        {

                            threadlog.Current = current;
                            threadlog.Current_loc = List2Str(current_loc);
                            threadlog.Status = "暂停";
                            threadlog.TStatus = 4;
                            threadlog.IsPaused = true;
                            new Log<Amap_TileRequest>(threadlog);

                            log.Status = "未完成";
                            log.Current = current;
                            new Log<Amap_TileRequest>(log);
                            for (int k = 0; k < countdown.CurrentCount; k++)
                                countdown.Signal();
                            return;
                        }

                        try
                        {
                            lock (obj)
                            {
                                current = 0;
                                for (int mm = 0; mm < this.request_ATile.LevelList.Length; mm++)
                                {
                                    DirectoryInfo TheFolder = new DirectoryInfo(request_ATile.SavePathText + "\\" + this.request_ATile.LevelList[l]);
                                    current += GetFileNum(TheFolder);
                                }
                                threadlog.Current = current;
                                new Log<Amap_TileRequest>(threadlog);
                                if (Contains(current_loc, Level) >= 0)
                                {
                                    var index = Contains(current_loc, Level);
                                    current_loc[index][1] = i;
                                    current_loc[index][2] = j;
                                }
                                else
                                {
                                    if (current_loc == null) current_loc = new List<int[]>();
                                    current_loc.Add(new int[3] { Level, i, j });
                                }
                            }

                            String link = String.Format(originlink, rm.Next(1, 4), i, j, Level);
                            String localpath = this.request_ATile.SavePathText + String.Format("\\{0}\\{1}\\", Level, i);
                            String filename = String.Format("{0}.png", j);
                            //判断文件是否存在，若存在，直接下载下一个文件
                            if (File.Exists(localpath + filename))  continue;
                            
                            //下载文件
                            String downloadedfile = "";
                            int c = 0;
                            do
                            {
                                downloadedfile = DownloadFile(link, localpath, filename);
                                Thread.Sleep(100);
                                try
                                {
                                    Image img = Image.FromFile(localpath + filename);
                                }
                                catch (Exception ex)
                                {
                                    File.Delete(localpath + filename);
                                    downloadedfile = "";

                                    log.Status = "错误";
                                    log.ErrorMsg = localpath + filename + "下载失败： " + ex.ToString();
                                    log.ErrorDate = DateTime.Now.ToString();
                                    //操作日志
                                    new Log<Amap_TileRequest>(log);
                                }
                                c++;
                            } while (downloadedfile == "" && c <= 20);
                            if (downloadedfile == "")
                                error_loc.Add(new int[3] { Level, i, j });

                        }
                        catch (Exception ex)
                        {
                            //操作日志
                            log.Status = "错误";
                            log.ErrorDate = DateTime.Now.ToString();
                            log.ErrorMsg = ex.ToString();
                            log.Current = current;
                            new Log<Amap_TileRequest>(log);

                            threadlog.Current = current;
                            threadlog.Current_loc = List2Str(current_loc);
                            threadlog.Status = "错误";
                            threadlog.TStatus = 3;
                            threadlog.IsPaused = true;
                            new Log<Amap_TileRequest>(threadlog);
                            for (int k = 0; k < countdown.CurrentCount; k++)
                                countdown.Signal();
                            return;
                        }
                    }

                }
            }

            for (int l = 0; l < error_loc.Count; l++)
            {
                string error_msg="";
                int Level = error_loc[l][0];
                int i = error_loc[l][1];
                int j = error_loc[l][2];
                String link = String.Format(originlink, rm.Next(1, 4), i, j, Level);
                String localpath = this.request_ATile.SavePathText + String.Format("\\{0}\\{1}\\", Level, i);
                String filename = String.Format("{0}.png", j);
                //下载文件
                String downloadedfile = "";
                int c = 0;
                do
                {
                    downloadedfile = DownloadFile(link, localpath, filename);
                    Thread.Sleep(100);
                    try
                    {
                        Image img = Image.FromFile(localpath + filename);
                    }
                    catch (Exception ex)
                    {
                        File.Delete(localpath + filename);
                        downloadedfile = "";
                        error_msg = localpath + filename + "下载失败： " + ex.ToString();
                    }
                    c++;
                } while (downloadedfile == "" && c <= 20);
                if (downloadedfile == "")
                {
                    log.Status = "错误";
                    log.ErrorMsg += " "+error_msg+";";
                    log.ErrorDate = DateTime.Now.ToString();
                    new Log<Amap_TileRequest>(log);
                }
            }

            countdown.Signal();
        }


        /// <summary>
        /// 获取进度
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        //public WebApiResult<string> GetScheduleData(ScheduleDataRequest Request)
        //{
        //    var data = CacheHelper.GetCache(Request.Key);

        //    if (data != null)
        //    {
        //        return new WebApiResult<string>() { success = 1, msg = "获取成功", results = JsonHelper.ToJson(data) };
        //    }
        //    else
        //    {
        //       // var log = new Log<ScheduleDataRequest>();
        //        var entity = Log<ScheduleDataRequest>.GetLogEntity(Request.Key);
        //        if (entity != null)
        //        {
        //            return new WebApiResult<string>() { success = 1, msg = "获取成功", results = JsonHelper.ToJson(entity) };
        //        }
        //        else
        //        {
        //            return new WebApiResult<string>() { success = 0, msg = "" };
        //        }
               
        //    }          
        //}

        /// <summary>
        /// 设置任务暂停
        /// </summary>
        /// <param name="Request"></param>
        /// <returns></returns>
        //public WebApiResult<string> SetStop(TaskStop Request)
        //{
        //    var data = CacheHelper.GetCache(Request.GUID);
        //    if (data != null)
        //    {
        //        var log = (TaskLogEntity)data;
        //        log.IsStart = false;
        //        CacheHelper.SetCache(Request.GUID, log);

        //        return new WebApiResult<string>() { success = 1, msg = "设置成功" };
        //    }
        //    else
        //    {
        //        return new WebApiResult<string> { success = 0, msg = "该任务已完成或不存在" };
        //    }
        //}


        /// <summary>
        /// 向数据表中插入数据
        /// </summary>
        /// <param name="connectionstring">数据库连接字符串</param>
        /// <param name="datatable">待插入数据</param>
        public void InsertData(String connectionstring, DataTable datatable, string ScheduleKey)
        {
            if (datatable.Rows.Count != 0)
            {

                int insertCount = Angels.Application.Data.MySqlHelper.BulkInsert(connectionstring, datatable);
                if (insertCount > 0)
                    CacheHelper.SetCache(ScheduleKey + "2", "{\"Count\":" + insertCount.ToString() + ",\"Msg\":\"\"}", 180);//"成功插入" + insertCount.ToString() + "条记录"
                else
                    CacheHelper.SetCache(ScheduleKey + "2", "{\"Count\":0,\"Msg\":\"插入数据失败，请检查程序！\"}", 180); //"插入数据失败，请检查程序！"
            }
        }

        public String VerifyCrack(Mapbar_POIRequest Request, String pagecontent, String requesturl)
        {
            String verifyresult = "";
            int i = 2000;
            do
            {
                //验证码验证链接
                String[] verify = pagecontent.Split(new String[] { "ipValidateObj.src = \"", "\" + value" }, StringSplitOptions.RemoveEmptyEntries);
                //下载验证码图片保存为二进制
                String[] codename = pagecontent.Split(new String[] { "\"/services/code/getCode?s=", "\";" }, StringSplitOptions.RemoveEmptyEntries);
                String codeURL = "http://poi.mapbar.com/services/code/getCode?s=" + codename[1];
                WebClient wc = new WebClient();
                byte[] img = null;
                try
                {
                    img = wc.DownloadData(codeURL);
                }
                catch (WebException ex)
                {
                    Thread.Sleep(3000);
                    continue;
                }
                //string aa = ;
                //基于Caffe识别验证码
                classifierHandle = CC.createClassifier(prototxt, caffemodel, 1, null, 0, null, -1); 
                CC.forward(classifierHandle, img, img.Length);
                String verifycode = GetCaffeResult(classifierHandle, 4);
                //向服务器发送验证信息
                String verifyurl = verify[1] + verifycode + "&" + (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
                verifyresult = GetHttpResponse(verifyurl);
                //重新请求数据
                pagecontent = GetHttpResponse(requesturl);
                i--;
                if (i <= 0) break;
            } while (verifyresult.Contains("window.location.reload();") == false || pagecontent.Contains("错误"));
            if (i <= 0) return "failed";
            return pagecontent;
        }

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
                    //throw ex;
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
        /// 获取Caffe的验证码识别结果
        /// </summary>
        /// <param name="classifierHandle">分类器句柄</param>
        /// <param name="lossnum">输出结果类别</param>
        /// <returns></returns>
        public String GetCaffeResult(IntPtr classifierHandle, int lossnum)
        {
            String res = "";
            for (int i = 1; i <= lossnum; i++)
            {
                String blob_name = String.Format("loss{0}", i);
                IntPtr blob = CC.getBlobData(classifierHandle, blob_name);
                //int[] tempbuf = new int[4];
                //CC.getBlobDims(blob, tempbuf);

                float[] buf = new float[CC.getBlobLength(blob)];
                CC.cpyBlobData(buf, blob);
                CC.releaseBlobData(blob);

                res = res + ArgMax(buf);
            }
            return res;
        }

        /// <summary>
        /// 获取识别结果中概率最大的类别
        /// </summary>
        /// <param name="buf">识别结果</param>
        /// <returns></returns>
        public String ArgMax(float[] buf)
        {
            String s = "";
            float maxvalue = -1.0f;
            for (int i = 0; i < buf.Length; i++)
            {
                if (buf[i] > maxvalue)
                {
                    maxvalue = buf[i];
                    s = i.ToString();
                }
            }
            return s;
        }

        /// <summary>
        /// 获取数据库中已有信息以List返回
        /// </summary>
        /// <param name="querystring">查询字符串</param>
        /// <param name="colname">列名</param>
        /// <returns></returns>
        public List<String> GetList(String querystring, String colname)
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
        /// 获取POI分类信息存储于数据库中
        /// </summary>
        public List<String> GetPOIClass(Mapbar_POIRequest Request)
        {
            //状态栏信息输出
            //CacheHelper.SetCache("toolStripStatusLabel1", "当前正在抓取POI类别信息...");

            //List<String> classList = GetList("select * from mapbar_poiclass", "ClassURL");
            //List<String> classList = GetList("select * from mapbar_poiclass_enshi", "ClassURL");//-------------------------------更换poi_class表名
            List<String> classList = GetList("select * from mapbar_poiclass", "ClassName", "ClassURL");
            try
            {
                //创建DataTable用于存储数据
                DataTable dt = new DataTable();
                //dt.TableName = "mapbar_poiclass";
                //dt.TableName = "mapbar_poiclass_enshi";//-------------------------------更换poi_class表名
                dt.TableName = "mapbar_poiclass";
                dt.Columns.Add("ClassName", typeof(String));
                dt.Columns.Add("ClassURL", typeof(String));
                dt.Columns.Add("UpdateTime", typeof(DateTime));

                //请求网页，并将返回结果转换成字符串
                String restring = GetHttpResponse(RootURL + Request.CityName);
                //String restring = GetHttpResponse("http://poi.mapbar.com/123");
                //判断是否触发网站验证码
                if (restring.Contains("错误"))
                {
                    restring = VerifyCrack(Request, restring, RootURL + Request.CityName);
                }
                //分割字符串，并利用正则表达式生成自字符串集合
                String[] strArr = restring.Split(new String[] { "<div class=\"sort cl\">", "<dl class=\"other\">" }, StringSplitOptions.None);
                regex = new Regex("<a.*</a>");
                collection = regex.Matches(strArr[1]);

                for (int i = 0; i < collection.Count; i++)
                {
                    String astring = collection[i].ToString();
                    String[] classArr = astring.Split(new String[] { ">", "</a>" }, StringSplitOptions.RemoveEmptyEntries);
                    String[] hrefArr = astring.Split(new String[] { "href=\"", "\" title" }, StringSplitOptions.RemoveEmptyEntries);
                    if (classList.Contains(classArr[1] + "," + hrefArr[1].Substring(hrefArr[1].Length - 4))) continue;
                    classList.Add(classArr[1] + "," + hrefArr[1].Substring(hrefArr[1].Length-4));

                    dt.Rows.Add(classArr[1], hrefArr[1].Substring(hrefArr[1].Length - 4), DateTime.Now);
                }
                if (dt.Rows.Count > 0)
                    //将POI大类信息插入数据库
                    InsertData(ConnectionString, dt, Request.GUID);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return classList;
        }

        public List<String> GetList(String querystring, String colname1, String colname2)
        {
            //读取数据库中已有POI分类信息存储于List中
            DataTable existdt = mysqlHelper.ExecuteDataTable(querystring);
            List<String> aList = new List<string>();
            foreach (DataRow row in existdt.Rows)
            {
                aList.Add(row[colname1].ToString() + "," + row[colname2].ToString());
            }

            return aList;
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
        /// 提取JSON中key对应的的数值型value
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="jt">json</param>
        /// <returns></returns>
        public int GetInfotoInt(String key, JToken jt)
        {
            try
            {
                return Convert.ToInt32(jt[key].ToString());
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 百度墨卡托坐标转百度09坐标
        /// </summary>
        /// <param name="x">x坐标</param>
        /// <param name="y">y坐标</param>
        /// <returns></returns>
        public List<float> ConvertCoor(int x, int y)
        {
            List<float> coorlist = new List<float>();
            String coorurl = "http://api.map.baidu.com/geoconv/v1/?coords=" + x / (100.0) + "," + y / (100.0) + "&from=6&to=5&ak=GevTfxGlAWxzIzTobk7PGX1eu2YF0RMl";
            try
            {
                String rescoor = GetHttpResponse(coorurl);
                JObject jocoor = (JObject)JsonConvert.DeserializeObject(rescoor);
                coorlist.Add(float.Parse(jocoor["result"][0]["x"].ToString()));
                coorlist.Add(float.Parse(jocoor["result"][0]["y"].ToString()));
            }
            catch
            {
                coorlist.Add(0.0f);
                coorlist.Add(0.0f);
            }
            return coorlist;
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

        public Tuple<int, int, int, int> GetTileBound(LatLngPoint startcoord, LatLngPoint endcoord, int z)
        {
            PointF StartMCPoint = CoordTransferHelper.LatLng2Mercator(startcoord);
            Point StartTilePoint = GetTileCoord(StartMCPoint, z);
            PointF EndPoint = CoordTransferHelper.LatLng2Mercator(endcoord);
            Point EndTilePoint = GetTileCoord(EndPoint, z);

            return new Tuple<int, int, int, int>(StartTilePoint.X, StartTilePoint.Y, EndTilePoint.X, EndTilePoint.Y);
        }

        public Point GetTileCoord(PointF pt, int z)
        {
            double pixelPointX = pt.X / Math.Pow(2, (18 - z));
            double pixelPointY = pt.Y / Math.Pow(2, (18 - z));
            int BMapTileCoordX = (int)Math.Floor(pixelPointX / 256);
            int BMapTileCoordY = (int)Math.Floor(pixelPointY / 256);

            return new Point(BMapTileCoordX, BMapTileCoordY);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="savepath"></param>
        /// <param name="filename"></param>
        /// <returns>异常返回空字符，否则返回文件路径</returns>
        public string DownloadFile(String url, String savepath, String filename)
        {
            try
            {
                //判断保存路径文件夹是否存在，不存在则创建
                if (Directory.Exists(savepath) == false)
                    Directory.CreateDirectory(savepath);

                WebClient wc = new WebClient();
                wc.DownloadFile(url, savepath + filename);

                //下载成功，返回文件名
                return filename;
            }
            catch (Exception)
            {
                //下载异常，返回空值
                return "";
            }
        }

        int Contains(List<int[]> items, int Level)
        {
            if (items == null)
                return -1;
            for (int item = 0; item < items.Count; item++)
            {
                if (items[item][0] == Level)
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
                res = res + "{" + item[0].ToString() + "," + item[1].ToString() + "," + item[2].ToString() + "};";
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
                var b = int.Parse(i[2].Substring(0, i[2].Length - 1));
                li.Add(new int[3] { a, int.Parse(i[1]), b });
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
            if(entity.Status=="已完成") return new WebApiResult<string> { success = 0, msg = "该任务已完成" };
            entity.IsPaused = true;
            new Log<T>(entity);
            return new WebApiResult<string>() { success = 1, msg = "暂停成功" };
        }

        int GetFileNum(DirectoryInfo TheFolder)
        {
            int Num = 0;
            if (Directory.Exists(TheFolder.FullName) == false)
                return Num;
            FileInfo[] file_list = TheFolder.GetFiles();
            Num = file_list.Length;

            DirectoryInfo[] folder_list = TheFolder.GetDirectories();
            foreach (DirectoryInfo NextFolder in folder_list)
            {
                int child_file_list = GetFileNum(NextFolder);
                Num += child_file_list;
            }

            return Num;
        }
    }
}
