using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.IO;
using System.Drawing;
using Angels.Application.TicketEntity;
using Angels.Application.TicketEntity.Common.BaiduMapTileDownload;
using Angels.Application.TicketEntity.Request.BMapTileDownload;
using Angels.Common;
using Angels.Application.TicketEntity.Common;
namespace Angels.Application.Service
{
    /// <summary>
    /// 百度瓦片地图个性化下载
    /// </summary>
    public class BMapTileDownloadService
    {
        private readonly static object obj = new object();
        TaskLogEntity log;
        ThreadTaskLogEntity threadlog;
        List<int[]> current_loc;
        int current = 0;
        int current_i = -999999, current_j = -999999;

        CountdownEvent countdown;
        LatLngPoint startcoord, endcoord;
        double[] interval;
        string URLtextBox;
        BaiduMapTileDownloadRequest request;

        List<int[]> error_loc = new List<int[]>();


        //StreamWriter watch;

        #region 百度瓦片地图个性化下载

        /// <summary>
        /// 百度瓦片地图个性化下载(暂时不使用)
        /// </summary>
        /// <returns></returns>
        //public WebApiResult<string> BaiduMapTileDownload(BaiduMapTileDownloadRequest Request)
        //{
        //    var log = new TaskLogEntity() { GUID = Request.GUID, Name = "百度瓦片下载", Type = "瓦片下载", Description = "BaiduMapTileDownload1", Status = "进行中", Parameter = JsonHelper.ToJson(Request) };
        //    //操作日志
        //    new Log<BaiduMapTileDownloadRequest>(log);

        //    try
        //    {
        //        string URLtextBox = GetDownloadUrl(Request.List_TreeNode);
        //        //检查URL地址栏
        //        if (URLtextBox == "")
        //        {
        //            //MessageBox.Show(this, "URL地址为空，请检查！", "提示");
        //            return new WebApiResult<string>() { success = 2, msg = "URL地址为空，请检查！" };
        //        }

        //        //确定要下载的坐标范围信息，起始坐标为左下角，终点坐标为右上角
        //        double startcoord_x = Request.LefttextBox;
        //        double startcoord_y = Request.BottomtextBox;
        //        double endcoord_x = Request.RighttextBox;
        //        double endcoord_y = Request.UptextBox;
        //        LatLngPoint startcoord, endcoord;

        //        //if (startcoord_x > 0 && startcoord_x < endcoord_x && endcoord_x < 180 && startcoord_y > 0 && startcoord_y < endcoord_x && endcoord_y < 90)
        //        {
        //            startcoord = new LatLngPoint(startcoord_y, startcoord_x);
        //            endcoord = new LatLngPoint(endcoord_y, endcoord_x);
        //        }
        //        //else
        //        //{
        //        //    return new WebApiResult<string>() { success = 2, msg = "输入坐标值不正确，请检查！" };
        //        //}

        //        //检查下载层级是否勾选
        //        if (Request.LevelList.Length == 0)
        //        {
        //            return new WebApiResult<string>() { success = 2, msg = "下载地图层级未选择，请检查！" };
        //        }
        //        //获取要下载的地图层级
        //        foreach (int Level in Request.LevelList)
        //        {
        //            //int z = Level;
        //            Tuple<int, int, int, int> BoundTup = GetTileBound(startcoord, endcoord, Level);
        //            for (int i = BoundTup.Item1; i <= BoundTup.Item3; i++)
        //            {
        //                for (int j = BoundTup.Item2; j <= BoundTup.Item4; j++)
        //                {
        //                    try
        //                    {
        //                        String link = String.Format(URLtextBox, Math.Abs(i + j) % 3, i, j, Level);
        //                        String localpath = Request.SavePathText + String.Format("\\{0}\\{1}\\", Level, i);
        //                        String filename = String.Format("{0}.png", j);
        //                        //判断文件是否存在，若存在，直接下载下一个文件
        //                        if (File.Exists(localpath + filename)) continue;
        //                        //下载文件
        //                        String downloadedfile = "";
        //                        do
        //                        {
        //                            downloadedfile = DownloadFile(link, localpath, filename);
        //                            Thread.Sleep(100);
        //                            try
        //                            {
        //                                Image img = Image.FromFile(localpath + filename);
        //                            }
        //                            catch (Exception)
        //                            {
        //                                File.Delete(localpath + filename);
        //                                downloadedfile = "";
        //                            }
        //                        } while (downloadedfile == "");
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        log.Status = "错误";
        //                        log.ErrorMsg = ex.ToString();
        //                        log.ErrorDate = DateTime.Now.ToString();
        //                        new Log<BaiduMapTileDownloadRequest>(log);
        //                        return null;
        //                    }
        //                }
        //            }
        //        }

        //        log.Status = "已完成";
        //        log.CompleteTime = DateTime.Now.ToString();
        //        new Log<BaiduMapTileDownloadRequest>(log);
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Status = "错误";
        //        log.ErrorMsg = ex.ToString();
        //        log.ErrorDate = DateTime.Now.ToString();
        //        new Log<BaiduMapTileDownloadRequest>(log);
        //    }
        //    return null;
        //}

        /// <summary>
        /// 百度瓦片地图个性化下载（当前使用）
        /// </summary>
        /// <returns></returns>
        public WebApiResult<string> BaiduMapTileDownload1(BaiduMapTileDownloadRequest Request)
        {
            //watch = new StreamWriter(new FileStream(System.AppDomain.CurrentDomain.BaseDirectory + "\\App_Data\\ThreadLog.txt", FileMode.Create)); 
            this.countdown = new CountdownEvent(Request.TaskCount);
            this.request = Request;
            
            UpdateLastLoc(Request.GUID);

            log = new TaskLogEntity() { GUID = Request.GUID, Name = Request.TName, Type = "百度瓦片下载", Description = "BaiduMapTileDownload1", Status = "进行中", Parameter = JsonHelper.ToJson(Request), SavePath=request.SavePathText };
            //操作日志
            new Log<BaiduMapTileDownloadRequest>(log);
            try
            {
                if (Request.LayerStr == "sate")
                {
                    this.URLtextBox = "https://ss{0}.bdstatic.com/8bo_dTSlR1gBo1vgoIiO_jowehsv/starpic/?qt=satepc&u=x={1};y={2};z={3};v=009;type=sate";
                }
                else
                {
                    this.URLtextBox = "http://api{0}.map.bdimg.com/customimage/tile?&x={1}&y={2}&z={3}&scale=1&ak=8d6c8b8f3749aed6b1aff3aad6f40e37&" + Request.LayerStr;//GetDownloadUrl(Request.List_TreeNode);
                                                                                                                                                                        //检查URL地址栏http://api{0}.map.bdimg.com/customimage/tile?&x={1}&y={2}&z={3}&scale=1&ak=8d6c8b8f3749aed6b1aff3aad6f40e37&styles=t:background|e:all|v:off,t:poi|e:all|v:off,t:administrative|e:all|v:off
                }
                if (URLtextBox == "")
                {
                    //MessageBox.Show(this, "URL地址为空，请检查！", "提示");
                    return new WebApiResult<string>() { success = 2, msg = "URL地址为空，请检查！" };
                }

                //确定要下载的坐标范围信息，起始坐标为左下角，终点坐标为右上角
                double startcoord_x = Request.LefttextBox;
                double startcoord_y = Request.BottomtextBox;
                double endcoord_x = Request.RighttextBox;
                double endcoord_y = Request.UptextBox;

                //if (startcoord_x > 0 && startcoord_x < endcoord_x && endcoord_x < 180 && startcoord_y > 0 && startcoord_y < endcoord_y && endcoord_y < 90)
                //{
                this.startcoord = new LatLngPoint(startcoord_y, startcoord_x);
                this.endcoord = new LatLngPoint(endcoord_y, endcoord_x);
                //}
                //else
                //{
                //    return new WebApiResult<string>() { success = 2, msg = "输入坐标值不正确，请检查！" };
                //}
                //检查下载层级是否勾选
                if (Request.LevelList.Length == 0)
                {
                    return new WebApiResult<string>() { success = 2, msg = "下载地图层级未选择，请检查！" };
                }

                threadlog = new ThreadTaskLogEntity() { GUID = Request.GUID, TaskLog_GUID = Request.GUID, Status = "进行中", TStatus = 1 ,Total = 0, TName = Request.TName, IsPaused = false, Parameter = JsonHelper.ToJson(Request),URL=Request.URL };
                //计算Total并更新 
                foreach (int Level in Request.LevelList)
                {
                    Tuple<int, int, int, int> BoundTup = GetTileBound(startcoord, endcoord, Level);
                    threadlog.Total += (BoundTup.Item3 - BoundTup.Item1 + 1) * (BoundTup.Item4 - BoundTup.Item2 + 1);
                }
                log.Count = threadlog.Total;
                new Log<BaiduMapTileDownloadRequest>(log);
                new Log<BaiduMapTileDownloadRequest>(threadlog);

                interval = new double[] { (endcoord.Lat - startcoord.Lat) / Request.TaskCount, (endcoord.Lng - startcoord.Lng) / Request.TaskCount };                

                Thread[] t = new Thread[Request.TaskCount];
                for (int num = 0; num < Request.TaskCount; num++)
                {
                    try
                    {
                        t[num] = new Thread(new ParameterizedThreadStart(run))
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
                        new Log<BaiduMapTileDownloadRequest>(threadlog);

                        log.Status = "错误";
                        log.ErrorMsg = ex.ToString();
                        log.ErrorDate = DateTime.Now.ToString();
                        //操作日志
                        new Log<BaiduMapTileDownloadRequest>(log);

                        return new WebApiResult<string>() { success = 0, msg = ex.ToString() };
                    }
                }
                countdown.Wait();

                //watch.Close();
                for (int num = 0; num < Request.TaskCount; num++)
                {
                    t[num].Abort();
                }
                lock (obj)
                {
                    if (!Log<BaiduMapTileDownloadRequest>.GetThreadLogEntity(this.request.GUID).IsPaused)
                    {
                    log.Status = "已完成";
                    log.CompleteTime = DateTime.Now.ToString();
                    log.Current = log.Count;
                    threadlog.Status = "已完成";
                    threadlog.TStatus = 2;
                    threadlog.Current = threadlog.Total;
                    threadlog.Current_loc = List2Str(current_loc);
                    //操作日志
                    new Log<BaiduMapTileDownloadRequest>(threadlog);
                    new Log<BaiduMapTileDownloadRequest>(log);
                    return new WebApiResult<string>() { success = 1, msg = "百度瓦片下载完成！" };
                    }
                }
            }
            catch (Exception ex)
            {
                log.Status = "错误";
                log.ErrorMsg = ex.ToString();
                log.ErrorDate = DateTime.Now.ToString();
                new Log<BaiduMapTileDownloadRequest>(log);
                return new WebApiResult<string>() { success = 0, msg = ex.ToString() };
            }

            return null;
 
        }

        void run(object num)
        {
            //watch.WriteLine("Start: Thread " + num + "  &  " + DateTime.Now);

            var scoord = new LatLngPoint(this.startcoord.Lat, this.startcoord.Lng + interval[1] * (int)num);
            var ecoord = new LatLngPoint(this.endcoord.Lat, this.startcoord.Lng + interval[1] * ((int)num + 1));
            if (ecoord.Lng > endcoord.Lng) ecoord.Lng = endcoord.Lng;

            //获取要下载的地图层级
            for (int l = 0; l < this.request.LevelList.Length; l++)
            {
                int Level = this.request.LevelList[l];
                if (this.current_loc != null)
                {
                    if (Contains(current_loc, Level) >= 0)
                    {
                        var index = Contains(current_loc, Level);
                        current_i = this.current_loc[index][1];
                        current_j = this.current_loc[index][2];
                    }
                }
                //int z = Level;
                Tuple<int, int, int, int> BoundTup = GetTileBound(scoord, ecoord, Level);

                //watch.WriteLine( "Count: Thread " + num + ","+ BoundTup.Item1 + "-" + BoundTup.Item3+","+BoundTup.Item2 +"-"+ BoundTup.Item4+","+ Level + "  &  " + DateTime.Now);

                for (int i = BoundTup.Item1; i <= BoundTup.Item3; i++)
                {
                    if (i < current_i) continue;

                    for (int j = BoundTup.Item2; j <= BoundTup.Item4; j++)
                    {
                        if (i == current_i && j <= current_j) continue;

                        //if (i == 48 && j == 18) Pause(this.request.GUID);
                        if (Log<BaiduMapTileDownloadRequest>.GetThreadLogEntity(this.request.GUID).IsPaused)
                        {

                            threadlog.Current = current;
                            threadlog.Current_loc = List2Str(current_loc);
                            threadlog.Status = "暂停";
                            threadlog.TStatus = 4;
                            threadlog.IsPaused = true;
                            new Log<BaiduMapTileDownloadRequest>(threadlog);

                            log.Status = "未完成";
                            log.Current = current;
                            new Log<BaiduMapTileDownloadRequest>(log);
                            for (int k = 0; k < countdown.CurrentCount; k++)
                                countdown.Signal();
                            return;
                        }
                        try
                        {
                            lock (obj)
                            {
                                current = 0;
                                for (int mm = 0; mm < this.request.LevelList.Length; mm++)
                                {
                                    DirectoryInfo TheFolder = new DirectoryInfo(request.SavePathText + "\\" + this.request.LevelList[l]);
                                    current += GetFileNum(TheFolder);
                                }
                                threadlog.Current = current;
                                new Log<BaiduMapTileDownloadRequest>(threadlog);
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
                            String link = String.Format(URLtextBox, Math.Abs(i + j) % 3, i, j, Level);
                            String localpath = this.request.SavePathText + String.Format("\\{0}\\{1}\\", Level, i);
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
                                catch (Exception)
                                {
                                    File.Delete(localpath + filename);
                                    downloadedfile = "";
                                }
                                c++;
                            } while (downloadedfile == "" && c <= 20 );
                            if (downloadedfile == "")
                                error_loc.Add(new int[3] { Level, i, j });

                            //watch.WriteLine("Running: Thread " + num + "  &  " + "Downloaded: " + i +","+ j + "," + Level  + "  &  " + DateTime.Now);
                        }
                        catch (Exception ex)
                        {
                            log.Status = "错误";
                            log.ErrorMsg = ex.ToString();
                            log.ErrorDate = DateTime.Now.ToString();
                            log.Current = current;
                            new Log<BaiduMapTileDownloadRequest>(log);
                            
                            threadlog.Current = current;
                            threadlog.Current_loc = List2Str(current_loc);
                            threadlog.Status = "错误";
                            threadlog.TStatus = 3;
                            threadlog.IsPaused = true;
                            new Log<BaiduMapTileDownloadRequest>(threadlog);
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
                String link = String.Format(URLtextBox, Math.Abs(i + j) % 3, i, j, Level);
                String localpath = this.request.SavePathText + String.Format("\\{0}\\{1}\\", Level, i);
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
                    new Log<BaiduMapTileDownloadRequest>(log);
                }
            }

            //watch.WriteLine("Finish: Thread " + num + "  &  " + DateTime.Now);

            countdown.Signal();
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

        void UpdateLastLoc(string GUID)
        {
            var entity =  Log<BaiduMapTileDownloadRequest>.GetThreadLogEntity(GUID);
            if (entity == null) return;
            if(entity.Current_loc!="")
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

        #region 百度瓦片地图个性化下载辅助方法

        string GetDownloadUrl(List<TreeNodeRequest> List_TreeNode)
        {
            //百度地图URL地址
            string originlink = "http://api{0}.map.bdimg.com/customimage/tile?&x={1}&y={2}&z={3}&scale=1&ak=8d6c8b8f3749aed6b1aff3aad6f40e37";


            //百度个性化地图样式
            string styles = "&styles=";

            foreach (TreeNodeRequest tn in List_TreeNode)
            {
                if (tn.Level == 0)
                {
                    styles += "t:all|e:all|v:off,";
                }
                else if (tn.Level == 1)
                {
                    if (tn.Text == "地图背景")
                    {
                        styles += "t:background|e:all|v:off,";
                    }
                    else if (tn.Text == "道路")
                    {
                        styles += "t:road|e:all|v:off,";
                    }
                    else if (tn.Text == "兴趣点")
                    {
                        styles += "t:poi|e:all|v:off,";
                    }
                    else  //行政区划
                    {
                        styles += "t:administrative|e:all|v:off,";
                    }
                }
                else if (tn.Level == 2)
                {
                    if (tn.Text == "陆地")
                    {
                        styles += "t:land|e:all|v:off,";
                    }
                    else if (tn.Text == "水系")
                    {
                        styles += "t:water|e:all|v:off,";
                    }
                    else if (tn.Text == "绿地")
                    {
                        styles += "t:green|e:all|v:off,";
                    }
                    else if (tn.Text == "人造区域")
                    {
                        styles += "t:manmade|e:all|v:off,";
                    }
                    else if (tn.Text == "建筑物")
                    {
                        styles += "t:building|e:all|v:off,";
                    }
                    else if (tn.Text == "高速及国道")
                    {
                        styles += "t:highway|e:all|v:off,";
                    }
                    else if (tn.Text == "城市主路")
                    {
                        styles += "t:arterial|e:all|v:off,";
                    }
                    else if (tn.Text == "普通道路")
                    {
                        styles += "t:local|e:all|v:off,";
                    }
                    else if (tn.Text == "铁路")
                    {
                        styles += "t:railway|e:all|v:off,";
                    }
                    else if (tn.Text == "地铁")
                    {
                        styles += "t:subway|e:all|v:off,";
                    }
                    else
                    {
                        if (tn.Text == "几何")
                        {
                            if (tn.Parent.Text == "兴趣点")
                            {
                                styles += "t:poi|e:g|v:off,";
                            }
                            else  //行政区划
                            {
                                styles += "t:administrative|e:g|v:off,";
                            }
                        }
                        else  //文本
                        {
                            if (tn.Parent.Text == "兴趣点")
                            {
                                styles += "t:poi|e:l|v:off,";
                            }
                            else  //行政区划
                            {
                                styles += "t:administrative|e:l|v:off,";
                            }
                        }
                    }
                }
                else if (tn.Level == 3)
                {
                    if (tn.Text == "几何")
                    {
                        if (tn.Parent.Text == "陆地")
                        {
                            styles += "t:land|e:g|v:off,";
                        }
                        else if (tn.Parent.Text == "水系")
                        {
                            styles += "t:water|e:g|v:off,";
                        }
                        else if (tn.Parent.Text == "绿地")
                        {
                            styles += "t:green|e:g|v:off,";
                        }
                        else if (tn.Parent.Text == "人造区域")
                        {
                            styles += "t:manmade|e:g|v:off,";
                        }
                        else if (tn.Parent.Text == "建筑物")
                        {
                            styles += "t:building|e:g|v:off,";
                        }
                        else if (tn.Parent.Text == "高速及国道")
                        {
                            styles += "t:highway|e:g|v:off,";
                        }
                        else if (tn.Parent.Text == "城市主路")
                        {
                            styles += "t:arterial|e:g|v:off,";
                        }
                        else if (tn.Parent.Text == "普通道路")
                        {
                            styles += "t:local|e:g|v:off,";
                        }
                        else if (tn.Parent.Text == "铁路")
                        {
                            styles += "t:railway|e:g|v:off,";
                        }
                        else  //地铁
                        {
                            styles += "t:subway|e:g|v:off,";
                        }
                    }
                    else  //文本
                    {
                        if (tn.Parent.Text == "陆地")
                        {
                            styles += "t:land|e:l|v:off,";
                        }
                        else if (tn.Parent.Text == "水系")
                        {
                            styles += "t:water|e:l|v:off,";
                        }
                        else if (tn.Parent.Text == "绿地")
                        {
                            styles += "t:green|e:l|v:off,";
                        }
                        else if (tn.Parent.Text == "人造区域")
                        {
                            styles += "t:manmade|e:l|v:off,";
                        }
                        else if (tn.Parent.Text == "建筑物")
                        {
                            styles += "t:building|e:l|v:off,";
                        }
                        else if (tn.Parent.Text == "高速及国道")
                        {
                            styles += "t:highway|e:l|v:off,";
                        }
                        else if (tn.Parent.Text == "城市主路")
                        {
                            styles += "t:arterial|e:l|v:off,";
                        }
                        else if (tn.Parent.Text == "普通道路")
                        {
                            styles += "t:local|e:l|v:off,";
                        }
                        else if (tn.Parent.Text == "铁路")
                        {
                            styles += "t:railway|e:l|v:off,";
                        }
                        else  //地铁
                        {
                            styles += "t:subway|e:l|v:off,";
                        }
                    }
                }
            }
            //去掉字符在最后面的逗号
            styles = styles.Substring(0, styles.Length - 1);
            //URL拼接展示
            return originlink + styles;
        }

        /// <summary>
        /// 根据网络地址下载瓦片地图
        /// </summary>
        /// <param name="url">瓦片地图网络地址</param>
        /// <param name="savepath">瓦片地图保存路径</param>
        /// <param name="filename">瓦片地图文件名</param>
        /// <returns></returns>
        string DownloadFile(String url, String savepath, String filename)
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

        /// <summary>
        /// 计算瓦片地图的行列号范围
        /// </summary>
        /// <param name="startcoord">左下角经纬度</param>
        /// <param name="endcoord">右上角经纬度</param>
        /// <param name="z">地图层级</param>
        /// <returns></returns>
        public Tuple<int, int, int, int> GetTileBound(LatLngPoint startcoord, LatLngPoint endcoord, int z)
        {
            PointF StartMCPoint = CoordTransferHelper.LatLng2Mercator(startcoord);
            Point StartTilePoint = GetTileCoord(StartMCPoint, z);
            PointF EndPoint = CoordTransferHelper.LatLng2Mercator(endcoord);
            Point EndTilePoint = GetTileCoord(EndPoint, z);

            return new Tuple<int, int, int, int>(StartTilePoint.X, StartTilePoint.Y, EndTilePoint.X, EndTilePoint.Y);
        }

        /// <summary>
        /// 坐标所在瓦片行列号
        /// </summary>
        /// <param name="pt">经纬度坐标</param>
        /// <param name="z">地图层级</param>
        /// <returns></returns>
        public Point GetTileCoord(PointF pt, int z)
        {
            double pixelPointX = pt.X / Math.Pow(2, (18 - z));
            double pixelPointY = pt.Y / Math.Pow(2, (18 - z));
            int BMapTileCoordX = (int)Math.Floor(pixelPointX / 256);
            int BMapTileCoordY = (int)Math.Floor(pixelPointY / 256);

            return new Point(BMapTileCoordX, BMapTileCoordY);
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


        #endregion

        #endregion
    }
}
