using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using Angels.Application.TicketEntity;
using Angels.Application.TicketEntity.Request.ImageMosaic;
using OSGeo.GDAL;
using Angels.Common;
using Angels.Application.TicketEntity.Common;
using Angels.Application.TicketEntity.Common.BaiduMapTileDownload;
using System.Threading;

namespace Angels.Application.Service
{
    public class ImageMosaicService
    {
        //private readonly static object obj = new object();
        TaskLogEntity log;
        ThreadTaskLogEntity threadlog;
        //List<int[]> current_loc;
        int current = 0;
        //CountdownEvent countdown;
        //int N;

        BaiduTileSplicingResult Result_baidu;
        /// <summary>
        /// 百度瓦片地图拼接
        /// </summary>
        /// <param name="Result"></param>
        /// <returns></returns>
        public WebApiResult<string> BaiduTileSplicing(BaiduTileSplicingResult Result)
        {
            this.Result_baidu = Result;
            UpdateLastLoc<BaiduTileSplicingResult>(Result.GUID);

            log = new TaskLogEntity() { GUID = Result.GUID, Name = Result.TName, Type = "瓦片拼接", Description = "BaiduTileSplicing", Status = "进行中", Parameter = JsonHelper.ToJson(Result), SavePath = Result.SavePath };
            //操作日志
            new Log<BaiduTileSplicingResult>(log);
            threadlog = new ThreadTaskLogEntity() { GUID = this.Result_baidu.GUID, TaskLog_GUID = this.Result_baidu.GUID, Status = "进行中", Current= current, TStatus = 1, TName = Result_baidu.TName, IsPaused = false, URL = Result_baidu.URL, Parameter = JsonHelper.ToJson(Result_baidu) };
            new Log<BaiduTileSplicingResult>(threadlog);

            try
            {
                //监测瓦片地图拼接耗时
                //Stopwatch stopwatch = new Stopwatch();
                //stopwatch.Start();

                //百度地图瓦片相关信息获取，文件路径必须选取到z级（缩放层级）文件夹
                String FilePath = Result.FilePath;
                DirectoryInfo TheFolder = new DirectoryInfo(FilePath);
                DirectoryInfo[] folder_list = TheFolder.GetDirectories();
                ////百度瓦片地图拼接
                String savepath = Result.SavePath;
                var restuls = ParallelWriteImage(folder_list, savepath, 0, Result.crstype, Result.Size);

                if (restuls != null)
                {
                    log.Status = "已完成";
                    log.CompleteTime = DateTime.Now.ToString();
                    log.Current = log.Count;

                    threadlog.TStatus = 2;
                    threadlog.Status = "已完成";
                    threadlog.Current = threadlog.Total;
                    //操作日志
                    new Log<BaiduTileSplicingResult>(threadlog);
                    new Log<BaiduTileSplicingResult>(log);                    

                    return restuls;
                }
                //stopwatch.Stop();

            }
            catch (Exception ex)
            {

                threadlog.Status = "错误";
                threadlog.ErrorMsg = ex.ToString();
                threadlog.TStatus = 3;
                new Log<BaiduTileSplicingResult>(threadlog);

                log.Status = "错误";
                log.ErrorMsg = ex.ToString();
                log.ErrorDate = DateTime.Now.ToString();
                new Log<BaiduTileSplicingResult>(log);

                return new WebApiResult<string>() { success = 0, msg = ex.ToString() };
            }
            return null;
        }

        GaodeTileSplicingResult Result_gaode;
        /// <summary>
        /// 高德瓦片地图拼接
        /// </summary>
        /// <param name="Result"></param>
        /// <returns></returns>
        public WebApiResult<string> GaodeTileSplicing(GaodeTileSplicingResult Result)
        {
            this.Result_gaode = Result;
            UpdateLastLoc<GaodeTileSplicingResult>(Result.GUID);

            log = new TaskLogEntity() { GUID = Result.GUID, Name = Result.TName, Type = "瓦片拼接", Description = "GaodeTileSplicing", Status = "进行中", Parameter = JsonHelper.ToJson(Result), SavePath = Result.SavePath };
            //操作日志
            new Log<GaodeTileSplicingResult>(log);

            threadlog = new ThreadTaskLogEntity() { GUID = this.Result_gaode.GUID, TaskLog_GUID = this.Result_gaode.GUID, Status = "进行中", TStatus = 1, TName = Result_gaode.TName, IsPaused = false, URL = Result_gaode.URL, Parameter = JsonHelper.ToJson(Result_gaode) };
            new Log<BaiduTileSplicingResult>(threadlog);

            try
            {               
                //耗时统计
                //Stopwatch stopwatch = new Stopwatch();
                //stopwatch.Start();

                //高德地图瓦片相关信息获取，文件路径必须选取到z级（缩放层级）文件夹
                String FilePath = Result.FilePath;
                DirectoryInfo TheFolder = new DirectoryInfo(FilePath);
                DirectoryInfo[] folder_list = TheFolder.GetDirectories();
                ////整个瓦片地图范围，用于创建拼接大影像
                //FileInfo[] file_list = GetFileList(TheFolder);
                //List<Point> TilePoints = GetTileCoor(file_list);
                //Tuple<int, int, int, int> TileBoundary = GetBoundary(TilePoints);

                ////高德瓦片地图拼接
                String savepath = Result.SavePath;
                var restuls = ParallelWriteImage(folder_list, savepath, 1,"", Result.Size);

                if (restuls != null)
                {
                    log.Status = "已完成";
                    log.CompleteTime = DateTime.Now.ToString();
                    log.Current = log.Count;

                    threadlog.TStatus = 2;
                    threadlog.Status = "已完成";
                    threadlog.Current = threadlog.Total;
                    //操作日志
                    new Log<GaodeTileSplicingResult>(threadlog);
                    new Log<GaodeTileSplicingResult>(log);

                    string dir = FilePath;
                    FileDelete(dir);

                    return restuls;
                }               
                //stopwatch.Stop();
                
            }
            catch (Exception ex)
            {

                threadlog.Status = "错误";
                threadlog.ErrorMsg = ex.ToString();
                threadlog.TStatus = 3;
                new Log<GaodeTileSplicingResult>(threadlog);

                log.Status = "错误";
                log.ErrorMsg = ex.ToString();
                log.ErrorDate = DateTime.Now.ToString();
                new Log<GaodeTileSplicingResult>(log);
                
                return new WebApiResult<string>() { success = 0, msg = ex.ToString() };
            }
            return null;
        }
        
        /// <summary>
        /// 高德瓦片地图为索引图，需要先转化为RGB图像，并进行二值化
        /// </summary>
        /// <param name="path">路径（E:\\AMap20171110\\tiles\\17）</param>
        /// <returns></returns>
        public WebApiResult<string> GaodeErzhihua(GaodeErzhihuaRequest Result)
        {          
            try
            {
                //高德地图瓦片相关信息获取
                String FilePath = Result.FilePath;// "E:\\AMap20171110\\tiles\\17";
                //String FilePath = "F:\\000";
                DirectoryInfo TheFolder = new DirectoryInfo(FilePath);
                FileInfo[] file_list = GetFileList(TheFolder);
                //List<Point> TilePoints = GetTileCoor(file_list);

                for (int i = 0; i < file_list.Length; i++)
                {
                    string level = file_list[i].DirectoryName.Substring(0, file_list[i].DirectoryName.LastIndexOf("\\"));
                    level = level.Substring(level.LastIndexOf("\\"));
                    String savedir = Result.FilePath + "$erzhihua" + file_list[i].DirectoryName.Substring(file_list[i].DirectoryName.LastIndexOf("\\"));
                    if (!Directory.Exists(savedir))
                    {
                        Directory.CreateDirectory(savedir);
                    }
                    String savepath = savedir + "\\" + file_list[i].Name.Substring(0, file_list[i].Name.LastIndexOf(".") + 1) + "jpg";
                    if (File.Exists(savepath))
                    {
                        continue;
                        //File.Delete(savepath);
                    }

                    Bitmap amtile = (Bitmap)Bitmap.FromFile(file_list[i].FullName);
                    //如果原图片是索引像素格式之类的，则需要转换
                    if (IsPixelFormatIndexed(amtile.PixelFormat) || amtile.PixelFormat == PixelFormat.Format32bppArgb)
                    {
                        using (Bitmap bmp = new Bitmap(amtile.Width, amtile.Height, PixelFormat.Format32bppArgb))
                        {
                            using (Graphics g = Graphics.FromImage(bmp))
                            {
                                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                                g.DrawImage(amtile, 0, 0);
                            }
                            amtile = (Bitmap)bmp.Clone();
                        }
                    }
                    if (Result.IsRoad==true)
                    {
                        //二值化
                        for (int m = 0; m < amtile.Height; m++)
                        {
                            for (int n = 0; n < amtile.Width; n++)
                            {
                                Color color = amtile.GetPixel(m, n);
                                Color newcolor;
                                if (color == Color.FromArgb(252, 249, 242))//高德地图北京颜色
                                {
                                    newcolor = Color.FromArgb(0, 0, 0);
                                }
                                else
                                {
                                    newcolor = Color.FromArgb(255, 255, 255);
                                }
                                amtile.SetPixel(m, n, newcolor);
                            }
                        }
                    }
                    
                    amtile.Save(savepath, ImageFormat.Jpeg);
                }

                return new WebApiResult<string>() { success = 1, msg = "高德瓦片二值化完成！" };
            }
            catch (Exception ex)
            {
                return new WebApiResult<string>() { success = 0, msg = ex.ToString() };
            }
        }

        /// <summary>  
        /// 判断图片的PixelFormat 是否在 引发异常的 PixelFormat 之中  
        /// </summary>  
        /// <param name="imgPixelFormat">原图片的PixelFormat</param>  
        /// <returns></returns>  
        static bool IsPixelFormatIndexed(PixelFormat imgPixelFormat)
        {
            foreach (PixelFormat pf in indexedPixelFormats)
            {
                if (pf.Equals(imgPixelFormat)) return true;
            }

            return false;
        }

        /// <summary>  
        /// 会产生graphics异常的PixelFormat  
        /// </summary>
        static PixelFormat[] indexedPixelFormats = {
            PixelFormat.Undefined,
            PixelFormat.DontCare,
            PixelFormat.Format16bppArgb1555,
            PixelFormat.Format1bppIndexed,
            PixelFormat.Format4bppIndexed,
            PixelFormat.Format8bppIndexed
        };

        #region 瓦片地图拼接辅助方法

        /// <summary>
        /// 瓦片拼接范围计算函数
        /// </summary>
        /// <param name="file">文件列表</param>
        /// <param name="maxSize">拼接后瓦片最大尺寸</param>
        /// <param name="res">各拼接块瓦片文件列表</param>
        public void deSplice(FileInfo[] file, int maxSize, ref List<List<FileInfo>> res)
        {
            List<Point> Points = GetTileCoor(file);
            Tuple<int, int, int, int> Boundary = GetBoundary(Points);
            int X = Boundary.Item3 - Boundary.Item1 + 1;
            int Y = Boundary.Item4 - Boundary.Item2 + 1;
            if (X * Y <= maxSize)
            {
                res.Add(file.ToList());
            }
            else
            {
                FileInfo[] file1 = { }, file2 = { };
                if (X >= Y)
                {
                    file1 = file.Take((X / 2) * Y).ToList().ToArray();
                    file2 = file.Skip((X / 2) * Y).ToList().ToArray();
                }
                else
                {
                    for (int i = 0; i < file.Length; i = i + Y)
                    {
                        file1 = JoinArray(file1, file.Take(Y / 2 + i).Skip(i).ToList().ToArray());
                        file2 = JoinArray(file2, file.Take(i + Y).Skip(i + Y / 2).ToList().ToArray());
                    }
                }
                deSplice(file1, maxSize, ref res);
                deSplice(file2, maxSize, ref res);
            }
        }

        /// <summary>
        /// 瓦片地图拼接函数
        /// 百度地图和高德地图瓦片地图组织形式不一样，用maptype加以区别
        /// </summary>
        /// <param name="folder_list">文件夹列表</param>
        /// <param name="savepath">保存路径</param>
        /// <param name="maptype">0为百度地图，1为高德地图</param>
        WebApiResult<string> ParallelWriteImage(DirectoryInfo[] folder_list, String savepath, int maptype, string crstype, int maxSize)
        {
            FileInfo[] file = new FileInfo[] { };
            foreach (var folder in folder_list)
            {
                FileInfo[] a = GetFileList(folder);
                file = JoinArray(file, a);
            }

            List<List<FileInfo>> file_list_blocks = new List<List<FileInfo>>();
            deSplice(file, maxSize, ref file_list_blocks);
            var count = file_list_blocks.Count;

            //计算Total并更新 
            threadlog.Total = file_list_blocks.Count;
            new Log<BaiduTileSplicingResult>(threadlog);
            log.Count = threadlog.Total;
            new Log<BaiduTileSplicingResult>(log);

            //利用GDAL创建结果影像，并写入瓦片数据
            Gdal.AllRegister(); //驱动注册
            Gdal.SetConfigOption("GDAL_FILENAME_IS_UTF8", "YES");    //中文路径支持
            Driver pDriver = Gdal.GetDriverByName("GTIFF");

            int num = 0;
            //分块拼接影像
            foreach (var file_list_block in file_list_blocks)
            {
                if (num < current)
                {
                    num++;
                    continue;
                }
                //if (num == 6) Pause<BaiduTileSplicingResult>(this.Result_gaode.GUID);

                if (maptype == 0)
                {
                    if (Log<BaiduTileSplicingResult>.GetThreadLogEntity(this.Result_baidu.GUID).IsPaused)
                    {
                        threadlog.Current = current;
                        threadlog.Status = "暂停";
                        threadlog.TStatus = 4;
                        threadlog.IsPaused = true;
                        new Log<BaiduTileSplicingResult>(threadlog);

                        log.Status = "未完成";
                        log.Current = current;
                        new Log<BaiduTileSplicingResult>(log);
                        return null;
                    }
                }
                else
                {
                    if (Log<BaiduTileSplicingResult>.GetThreadLogEntity(this.Result_gaode.GUID).IsPaused)
                    {
                        threadlog.Current = current;
                        threadlog.Status = "暂停";
                        threadlog.TStatus = 4;
                        threadlog.IsPaused = true;
                        new Log<BaiduTileSplicingResult>(threadlog);

                        log.Status = "未完成";
                        log.Current = current;
                        new Log<BaiduTileSplicingResult>(log);
                        return null;
                    }
                }

                FileInfo[] file_list_array = file_list_block.ToArray<FileInfo>();
                if (file_list_array.Length > 1)
                {
                    //获取分块的坐标信息和范围信息
                    List<Point> TilePoints = GetTileCoor(file_list_array);
                    Tuple<int, int, int, int> TileBoundary = GetBoundary(TilePoints);

                    //定义存储文件名
                    string level = file_list_block[0].DirectoryName.Substring(0, file_list_block[0].DirectoryName.LastIndexOf("\\")).Split('$')[0];
                    level = level.Substring(level.LastIndexOf("\\"));
                    string savedir = savepath + level;

                    //判断存储路径是否存在
                    if (!Directory.Exists(savedir))
                    {
                        Directory.CreateDirectory(savedir);
                    }
                    string dstPath = savedir + '\\' + file_list_block.First<FileInfo>().DirectoryName.Split('\\').Last() + "-" + file_list_block.First<FileInfo>().Name.Split('.').First() + "_" + file_list_block.Last<FileInfo>().DirectoryName.Split('\\').Last() + "-" + file_list_block.Last<FileInfo>().Name.Split('.').First() + ".tif";
                    if (File.Exists(dstPath)) continue;
                    string tfwPath = savedir + '\\' + file_list_block.First<FileInfo>().DirectoryName.Split('\\').Last() + "-" + file_list_block.First<FileInfo>().Name.Split('.').First() + "_" + file_list_block.Last<FileInfo>().DirectoryName.Split('\\').Last() + "-" + file_list_block.Last<FileInfo>().Name.Split('.').First() + ".tfw";
                    if (File.Exists(tfwPath)) continue;

                    //计算图像的宽度和高度，定义图像的Dataset
                    int bufWidth = (TileBoundary.Item3 - TileBoundary.Item1 + 1) * 256;
                    int bufHeight = (TileBoundary.Item4 - TileBoundary.Item2 + 1) * 256;
                    Dataset dstDataset = pDriver.Create(dstPath, bufWidth, bufHeight, 3, DataType.GDT_Byte, new String[] { "BIGTIFF=IF_NEEDED" });

                    //生成.tfw文件
                    StreamWriter writer = new StreamWriter(new FileStream(tfwPath,FileMode.Create,FileAccess.Write) ,Encoding.ASCII);
                    if (maptype == 0)
                    {
                        int z = int.Parse(level.Substring(1));
                        //计算瓦片地图左上角墨卡托坐标
                        double mcx = TileBoundary.Item1 * 256 * Math.Pow(2, (18 - z));
                        double mcy = (TileBoundary.Item4 + 1) * 256 * Math.Pow(2, (18 - z));
                        double sizex = 256 * Math.Pow(2, (18 - z));
                        double sizey = sizex;

                        if (crstype == "BD09")
                        {
                            var F = CoordTransferHelper.Mercator2LatLng(new PointF((float)mcx, (float)mcy));
                            mcx = F.Lat;
                            mcy = F.Lng;
                            double r1 = (TileBoundary.Item3 + 1) * 256 * Math.Pow(2, (18 - z));
                            double r2 = TileBoundary.Item2 * 256 * Math.Pow(2, (18 - z));
                            F = CoordTransferHelper.Mercator2LatLng(new PointF((float)r1, (float)r2));
                            sizex = Math.Abs(mcx - F.Lat) / (TileBoundary.Item3 - TileBoundary.Item1 + 1);
                            sizey = Math.Abs(mcy - F.Lng) / (TileBoundary.Item4 - TileBoundary.Item2 + 1);
                        }
						
                        writer.WriteLine(sizex);
                        writer.WriteLine(0.000000);
                        writer.WriteLine(0.000000);
                        writer.WriteLine(-1 * sizey);
                        writer.WriteLine(mcx);
                        writer.WriteLine(mcy);
                    }
                    if (maptype == 1)
                    {
                        int z = int.Parse(level.Substring(1));
                        Point TileP = new Point(TileBoundary.Item1, TileBoundary.Item2);
                        Point PixelP = new Point (0,0);
                        var P = CoordTransferHelper.PixelXY2LatLng(TileP, PixelP, z);
                        double mcx = P.Lat;
                        double mcy = P.Lng;
                        
                        P = CoordTransferHelper.PixelXY2LatLng(new Point(TileBoundary.Item3, TileBoundary.Item4), new Point(256,256) , z);
                        double sizex = Math.Abs(mcx - P.Lat) / (TileBoundary.Item3 - TileBoundary.Item1 + 1);
                        double sizey = Math.Abs(mcy - P.Lng) / (TileBoundary.Item4 - TileBoundary.Item2 + 1);

                        writer.WriteLine(sizex);
                        writer.WriteLine(0.000000);
                        writer.WriteLine(0.000000);
                        writer.WriteLine(sizey);
                        writer.WriteLine(mcx);
                        writer.WriteLine(mcy);
                    }
                    writer.Close();

                    //遍历分块列表中所有影像，读取瓦片图像信息写入分块影像
                    for (int i = 0; i < file_list_array.Length; i++)
                    {
                        //打开瓦片地图，获取瓦片地图的宽度、高度、波段、位数信息
                        Dataset img = Gdal.Open(file_list_array[i].FullName, Access.GA_ReadOnly);
                        int Width = img.RasterXSize;
                        int Height = img.RasterYSize;
                        int BandNum = img.RasterCount;
                        int Depth = Gdal.GetDataTypeSize(img.GetRasterBand(1).DataType);

                        //读取瓦片地图的数据信息
                        Byte[] buf = new Byte[Width * Height * BandNum];
                        CPLErr err = img.ReadRaster(0, 0, Width, Height, buf, Width, Height, BandNum, null, 0, 0, 0);
                        if (err == CPLErr.CE_Failure)
                        {
                            string message = string.Format("{0}图像读取失败！", file_list_array[i].Name);

                            return new WebApiResult<string>() { success = 1, msg = message };
                        }

                        //获取瓦片地图在拼接影像中的起始坐标信息
                        int OrgX = -9999, OrgY = -9999;
                        if (maptype == 0)
                        {
                            OrgX = (TilePoints[i].X - TileBoundary.Item1) * 256;
                            OrgY = (TileBoundary.Item4 - TilePoints[i].Y) * 256;
                        }
                        else if (maptype == 1)
                        {
                            OrgX = (TilePoints[i].X - TileBoundary.Item1) * 256;
                            OrgY = (TilePoints[i].Y - TileBoundary.Item2) * 256;
                        }
                        if (OrgX == -9999 || OrgY == -9999) return new WebApiResult<string>() { success = 1, msg = "坐标错误" }; ;
                        err = dstDataset.WriteRaster(OrgX, OrgY, Width, Height, buf, Width, Height, BandNum, null, 0, 0, 0);
                        if (err == CPLErr.CE_Failure)
                        {
                            string message = string.Format("结果图像{0}输出数据失败！", file_list_block.First<FileInfo>().Name + "_" + file_list_block.Last<FileInfo>().Name + ".tif");

                            return new WebApiResult<string>() { success = 1, msg = message };
                        }
                        img.Dispose();
                    }
                    dstDataset.Dispose();
                }
                else
                {
                    //定义存储文件名
                    string level = file_list_block[0].DirectoryName.Substring(0, file_list_block[0].DirectoryName.LastIndexOf("\\")).Split('_')[0];
                    level = level.Substring(level.LastIndexOf("\\"));
                    string savedir = savepath + level;

                    //判断存储路径是否存在
                    if (!Directory.Exists(savedir))
                    {
                        Directory.CreateDirectory(savedir);
                    }
                    string dstPath = savedir + '\\' + file_list_block.First<FileInfo>().DirectoryName.Split('\\').Last() + "-" + file_list_block.First<FileInfo>().Name.Split('.').First() + "_" + file_list_block.Last<FileInfo>().DirectoryName.Split('\\').Last() + "-" + file_list_block.Last<FileInfo>().Name.Split('.').First() + ".tif";
                    if (File.Exists(dstPath)) continue;

                    File.Copy(file_list_array[0].FullName.ToString(), dstPath);
                }

                current++;
                num++;
                if (maptype == 0)
                {
                    threadlog = Log<BaiduTileSplicingResult>.GetThreadLogEntity(this.Result_baidu.GUID);
                }
                else
                {
                    threadlog = Log<BaiduTileSplicingResult>.GetThreadLogEntity(this.Result_gaode.GUID);
                }
                threadlog.Current = current;
                new Log<BaiduTileSplicingResult>(threadlog);                
            }
            pDriver.Deregister();
            Gdal.GDALDestroyDriverManager();

            return new WebApiResult<string>() { success = 1, msg = "瓦片拼接完成！" };
        }

        /// <summary>
        /// 递归遍历文件夹，获取文件夹中所有瓦片地图
        /// </summary>
        /// <param name="TheFolder"></param>
        /// <returns></returns>
        FileInfo[] GetFileList(DirectoryInfo TheFolder)
        {
            FileInfo[] file_list = TheFolder.GetFiles();
            DirectoryInfo[] folder_list = TheFolder.GetDirectories();

            foreach (DirectoryInfo NextFolder in folder_list)
            {
                FileInfo[] child_file_list = GetFileList(NextFolder);
                file_list = JoinArray(file_list, child_file_list);
            }

            return file_list;
        }

        FileInfo[] JoinArray(FileInfo[] First, FileInfo[] Second)
        {
            FileInfo[] result = new FileInfo[First.Length + Second.Length];
            First.CopyTo(result, 0);
            Second.CopyTo(result, First.Length);
            return result;
        }

        /// <summary>
        /// 获取瓦片地图的范围
        /// </summary>
        /// <param name="FileList"></param>
        /// <returns></returns>
        List<Point> GetTileCoor(FileInfo[] FileList)
        {
            List<Point> TilePoint = new List<Point>();
            foreach (FileInfo file in FileList)
            {
                int x = Convert.ToInt32(file.DirectoryName.Substring(file.DirectoryName.LastIndexOf("\\") + 1));
                int y = Convert.ToInt32(file.Name.Substring(0, file.Name.IndexOf(".")));
                TilePoint.Add(new Point(x, y));
            }
            return TilePoint;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="TilePoint"></param>
        /// <returns></returns>
        Tuple<int, int, int, int> GetBoundary(List<Point> TilePoint)
        {
            Point minPoint = new Point(1000000000, 1000000000);
            Point maxPoint = new Point(0, 0);
            foreach (Point pt in TilePoint)
            {
                if (pt.X < minPoint.X)
                {
                    minPoint.X = pt.X;
                }
                else if (pt.X > maxPoint.X)
                {
                    maxPoint.X = pt.X;
                }
                if (pt.Y < minPoint.Y)
                {
                    minPoint.Y = pt.Y;
                }
                else if (pt.Y > maxPoint.Y)
                {
                    maxPoint.Y = pt.Y;
                }
            }
            return new Tuple<int, int, int, int>(minPoint.X, minPoint.Y, maxPoint.X, maxPoint.Y);
        }

        #endregion

        void UpdateLastLoc<T>(string GUID)
        {
            var entity = Log<T>.GetThreadLogEntity(GUID);
            if (entity == null) return;
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

        /// 清空指定的文件夹，同时删除文件夹
        /// </summary>
        /// <param name="dir"></param>
        public static void FileDelete(string dir)
        {
            try
            {
                foreach (string d in Directory.GetFileSystemEntries(dir))
                {
                    if (File.Exists(d))
                    {
                        FileInfo fi = new FileInfo(d);
                        if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                            fi.Attributes = FileAttributes.Normal;
                        File.Delete(d);//直接删除其中的文件  
                    }
                    else
                    {
                        DeleteFolder(d);////递归删除子文件夹
                        Directory.Delete(d);
                    }
                }

                Directory.Delete(dir);
            }
            catch (Exception ex)
            {
            }

        }

        /// 清空指定的文件夹，但不删除文件夹
        /// </summary>
        /// <param name="dir"></param>
        public static void DeleteFolder(string dir)
        {
            foreach (string d in Directory.GetFileSystemEntries(dir))
            {
                if (File.Exists(d))
                {
                    FileInfo fi = new FileInfo(d);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        fi.Attributes = FileAttributes.Normal;
                    File.Delete(d);//直接删除其中的文件  
                }
                else
                {
                    DirectoryInfo d1 = new DirectoryInfo(d);
                    if (d1.GetFiles().Length != 0)
                    {
                        DeleteFolder(d1.FullName);////递归删除子文件夹
                    }
                    Directory.Delete(d);
                }
            }
        }

    }
}
