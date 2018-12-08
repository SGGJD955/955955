using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Xml.Serialization;

using Angels.Application.TicketEntity;
using Angels.Application.TicketEntity.Common.BuildVRT;
using Angels.Application.TicketEntity.Request.BuildVRT;
using Angels.Application.TicketEntity.Common;
using Angels.Common;
namespace Angels.Application.Service
{
    public class BuildVRTService
    {
        #region 生成VRT

        public WebApiResult<string> BuildVRTByTile(BuildVRTByTileRequest Request)
        {
            var log = new TaskLogEntity() { GUID = Request.GUID, Name = "生成VRT", Type = "VRT", Description = "BuildVRTByTile", Status = "进行中", Parameter = JsonHelper.ToJson(Request) };
            //操作日志
            new Log<BuildVRTByTileRequest>(log);
            try
            {
                //遍历文件夹，获取瓦片地图地址信息
                DirectoryInfo TheFolder = new DirectoryInfo(Request.defaultfolderpath);
                //DirectoryInfo[] folder_list = TheFolder.GetDirectories();
                //瓦片地图格式为png，索引转RGB图格式为jpg，兼容两种图像格式
                FileInfo[] file_list = GetFileList(TheFolder, "(*.jpg|*.png)");

                //创建VRT文件
                //string dstPath = textBox1.Text + "\\Wuhan_17.vrt";
                string dstPath = Request.defaultfolderpath + "\\Dongcheng_all_19.vrt";
                BuildVRT(file_list, dstPath, 0, Request);

                log.Status = "已完成";
                log.CompleteTime = DateTime.Now.ToString();
                //操作日志
                new Log<BuildVRTByTileRequest>(log);
            }
            catch (Exception ex)
            {
                log.Status = "错误";
                log.ErrorMsg = ex.ToString();
                log.ErrorDate = DateTime.Now.ToString();
                new Log<BuildVRTByTileRequest>(log);
            }

            return null;
        }

        #region 生成VRT辅助方法

        /// <summary>
        /// 创建VRT文件
        /// </summary>
        /// <param name="file_list">遍历文件夹得到的瓦片地图列表</param>
        /// <param name="dstPath">保存路径</param>
        /// <param name="maptype">0为百度地图，1为高德地图</param>
        /// <param name="Request"></param>
        void BuildVRT(FileInfo[] file_list, string dstPath, int maptype, BuildVRTByTileRequest Request)
        {
            int z = 19;

            //获取瓦片地图的范围
            List<Point> TilePoints = GetTileCoor(file_list);
            Tuple<int, int, int, int> TileBoundary = GetBoundary(TilePoints);
            //计算VRT的宽度和高度
            int VRTWidth = (TileBoundary.Item3 - TileBoundary.Item1 + 1) * 256;
            int VRTHeight = (TileBoundary.Item4 - TileBoundary.Item2 + 1) * 256;
            //计算瓦片地图左上角墨卡托坐标
            int mcx = (int)(TileBoundary.Item1 * 256 * Math.Pow(2, (18 - z)));
            int mcy = (int)((TileBoundary.Item4 + 1) * 256 * Math.Pow(2, (18 - z)));
            //PointF startpoint = new PointF(mcx, mcy);

            //基于VRT生成类构建VRT文件
            VRTDataset wuhan_vrt = new VRTDataset();
            //设置VRTDataset属性
            wuhan_vrt.rasterXSize = VRTWidth.ToString();
            wuhan_vrt.rasterYSize = VRTHeight.ToString();
            //设置VRTDataset子节点
            wuhan_vrt.ItemsElementName = new ItemsChoiceType5[5] { ItemsChoiceType5.SRS, ItemsChoiceType5.GeoTransform, ItemsChoiceType5.VRTRasterBand, ItemsChoiceType5.VRTRasterBand, ItemsChoiceType5.VRTRasterBand };
            wuhan_vrt.Items = new object[5];
            wuhan_vrt.Items[0] = "PROJCS[\"WGS_1984_Pseudo_Mercator\",GEOGCS[\"GCS_WGS_1984\",DATUM[\"D_WGS_1984\",SPHEROID[\"WGS_1984\",6378137.0,298.257223563]],PRIMEM[\"Greenwich\",0.0],UNIT[\"Degree\",0.0174532925199433]],PROJECTION[\"Mercator\"],PARAMETER[\"false_easting\",0.0],PARAMETER[\"false_northing\",0.0],PARAMETER[\"central_meridian\",0.0],PARAMETER[\"standard_parallel_1\",0.0],UNIT[\"Meter\",1.0]]";
            wuhan_vrt.Items[1] = mcx.ToString() + ", " + Math.Pow(2, (18 - z)).ToString() + ", 0, " + mcy.ToString() + ", 0, -" + Math.Pow(2, (18 - z)).ToString();


            //红色波段
            VRTRasterBandType VRTRasterBand_Red = new VRTRasterBandType();
            VRTRasterBand_Red.dataType = DataTypeType.Byte;
            VRTRasterBand_Red.band = 1;
            VRTRasterBand_Red.ItemsElementName = new ItemsChoiceType4[file_list.Length + 1];
            VRTRasterBand_Red.ItemsElementName[0] = ItemsChoiceType4.ColorInterp;
            VRTRasterBand_Red.Items = new object[file_list.Length + 1];
            VRTRasterBand_Red.Items[0] = ColorInterpType.Red;
            //绿色波段
            VRTRasterBandType VRTRasterBand_Green = new VRTRasterBandType();
            VRTRasterBand_Green.dataType = DataTypeType.Byte;
            VRTRasterBand_Green.band = 2;
            VRTRasterBand_Green.ItemsElementName = new ItemsChoiceType4[file_list.Length + 1];
            VRTRasterBand_Green.ItemsElementName[0] = ItemsChoiceType4.ColorInterp;
            VRTRasterBand_Green.Items = new object[file_list.Length + 1];
            VRTRasterBand_Green.Items[0] = ColorInterpType.Green;
            //蓝色波段
            VRTRasterBandType VRTRasterBand_Blue = new VRTRasterBandType();
            VRTRasterBand_Blue.dataType = DataTypeType.Byte;
            VRTRasterBand_Blue.band = 3;
            VRTRasterBand_Blue.ItemsElementName = new ItemsChoiceType4[file_list.Length + 1];
            VRTRasterBand_Blue.ItemsElementName[0] = ItemsChoiceType4.ColorInterp;
            VRTRasterBand_Blue.Items = new object[file_list.Length + 1];
            VRTRasterBand_Blue.Items[0] = ColorInterpType.Blue;
            //循环遍历瓦片地图列表，获取瓦片地图信息写入VRT
            for (int i = 0; i < file_list.Length; i++)
            {
                //红色波段
                SimpleSourceType SimpleSource_Red = new SimpleSourceType();
                SimpleSource_Red.ItemsElementName = new ItemsChoiceType1[5] { ItemsChoiceType1.SourceFilename, ItemsChoiceType1.SourceBand, ItemsChoiceType1.SourceProperties, ItemsChoiceType1.SrcRect, ItemsChoiceType1.DstRect };
                SimpleSource_Red.Items = new object[5];
                SimpleSource_Red.Items[1] = "1";
                //绿色波段
                SimpleSourceType SimpleSource_Green = new SimpleSourceType();
                SimpleSource_Green.ItemsElementName = new ItemsChoiceType1[5] { ItemsChoiceType1.SourceFilename, ItemsChoiceType1.SourceBand, ItemsChoiceType1.SourceProperties, ItemsChoiceType1.SrcRect, ItemsChoiceType1.DstRect };
                SimpleSource_Green.Items = new object[5];
                SimpleSource_Green.Items[1] = "2";
                //蓝色波段
                SimpleSourceType SimpleSource_Blue = new SimpleSourceType();
                SimpleSource_Blue.ItemsElementName = new ItemsChoiceType1[5] { ItemsChoiceType1.SourceFilename, ItemsChoiceType1.SourceBand, ItemsChoiceType1.SourceProperties, ItemsChoiceType1.SrcRect, ItemsChoiceType1.DstRect };
                SimpleSource_Blue.Items = new object[5];
                SimpleSource_Blue.Items[1] = "3";

                //SimpleSource共同属性构建
                //1. SourceFilename
                SourceFilenameType SourceFilename = new SourceFilenameType();
                SourceFilename.relativeToVRT = ZeroOrOne.Item1;
                SourceFilename.Value = file_list[i].FullName.Replace(Request.defaultfolderpath + "\\", "").Replace("\\", "/");
                SimpleSource_Red.Items[0] = SourceFilename;
                SimpleSource_Green.Items[0] = SourceFilename;
                SimpleSource_Blue.Items[0] = SourceFilename;
                //3. SourceProperties
                SourcePropertiesType SourceProperties = new SourcePropertiesType();
                SourceProperties.RasterXSize = "256";
                SourceProperties.RasterYSize = "256";
                SourceProperties.DataType = DataTypeType.Byte;
                SourceProperties.BlockXSize = "128";
                SourceProperties.BlockYSize = "128";
                SimpleSource_Red.Items[2] = SourceProperties;
                SimpleSource_Green.Items[2] = SourceProperties;
                SimpleSource_Blue.Items[2] = SourceProperties;
                //4. SrcRect
                RectType SrcRect = new RectType();
                SrcRect.xOff = 0.0;
                SrcRect.yOff = 0.0;
                SrcRect.xSize = 256.0;
                SrcRect.ySize = 256.0;
                SimpleSource_Red.Items[3] = SrcRect;
                SimpleSource_Green.Items[3] = SrcRect;
                SimpleSource_Blue.Items[3] = SrcRect;
                //5. DstRect
                RectType DstRect = new RectType();
                ////根据maptype类型计算瓦片地图的偏移坐标
                int xOff = -9999, yOff = -9999;
                if (maptype == 0)
                {
                    xOff = (TilePoints[i].X - TileBoundary.Item1) * 256;
                    yOff = (TileBoundary.Item4 - TilePoints[i].Y) * 256;
                }
                else if (maptype == 1)
                {
                    xOff = (TilePoints[i].X - TileBoundary.Item1) * 256;
                    yOff = (TilePoints[i].Y - TileBoundary.Item2) * 256;
                }
                if (xOff == -9999 || yOff == -9999) return;
                ////
                DstRect.xOff = (double)xOff;
                DstRect.yOff = (double)yOff;
                DstRect.xSize = 256.0;
                DstRect.ySize = 256.0;
                SimpleSource_Red.Items[4] = DstRect;
                SimpleSource_Green.Items[4] = DstRect;
                SimpleSource_Blue.Items[4] = DstRect;

                VRTRasterBand_Red.ItemsElementName[i + 1] = ItemsChoiceType4.SimpleSource;
                VRTRasterBand_Green.ItemsElementName[i + 1] = ItemsChoiceType4.SimpleSource;
                VRTRasterBand_Blue.ItemsElementName[i + 1] = ItemsChoiceType4.SimpleSource;

                VRTRasterBand_Red.Items[i + 1] = SimpleSource_Red;
                VRTRasterBand_Green.Items[i + 1] = SimpleSource_Green;
                VRTRasterBand_Blue.Items[i + 1] = SimpleSource_Blue;
            }
            wuhan_vrt.Items[2] = VRTRasterBand_Red;
            wuhan_vrt.Items[3] = VRTRasterBand_Green;
            wuhan_vrt.Items[4] = VRTRasterBand_Blue;
            //VRT文件输出保存
            var serializer = new XmlSerializer(typeof(VRTDataset));
            using (var stream = new StreamWriter(dstPath))
            {
                try
                {
                    serializer.Serialize(stream, wuhan_vrt);
                }
                catch (Exception ex)
                {

                    throw ex;
                }
            }
        }

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

        #endregion

        #region 索引图转RGB图，生存VRT时需要读取瓦片地图RBG图层信息

        public WebApiResult<string> IndexToRGB(IndexToRGBRequest Request)
        {
            var log = new TaskLogEntity() { GUID = Request.GUID, Name = "索引图转RGB图", Type = "VRT", Description = "BuildVRTByTile", Status = "进行中", Parameter = JsonHelper.ToJson(Request) };
            //操作日志
            new Log<IndexToRGBRequest>(log);

            try
            {
                //遍历文件夹，获取瓦片地图地址信息
                DirectoryInfo TheFolder = new DirectoryInfo(Request.path);
                //DirectoryInfo[] folder_list = TheFolder.GetDirectories();
                FileInfo[] file_list = GetFileList(TheFolder, "*.png");

                //如果瓦片地图为索引色图片，则将其转换为RGB图像
                for (int i = 0; i < file_list.Length; i++)
                {
                    Bitmap mtile = (Bitmap)Bitmap.FromFile(file_list[i].FullName);
                    //如果原图片是索引像素格式之列的，则需要转换
                    if (IsPixelFormatIndexed(mtile.PixelFormat) || mtile.PixelFormat == PixelFormat.Format32bppArgb)
                    {
                        using (Bitmap bmp = new Bitmap(mtile.Width, mtile.Height, PixelFormat.Format32bppArgb))
                        {
                            using (Graphics g = Graphics.FromImage(bmp))
                            {
                                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                                g.DrawImage(mtile, 0, 0);
                            }
                            mtile = (Bitmap)bmp.Clone();
                        }
                    }
                    String savedir = "E" + file_list[i].DirectoryName.Substring(1);
                    if (!Directory.Exists(savedir))
                    {
                        Directory.CreateDirectory(savedir);
                    }
                    String savepath = "E" + file_list[i].FullName.Substring(0, file_list[i].FullName.LastIndexOf(".") + 1).Substring(1) + "jpg";
                    if (File.Exists(savepath))
                    {
                        File.Delete(savepath);
                    }
                    mtile.Save(savepath, ImageFormat.Jpeg);
                }

                log.Status = "已完成";
                log.CompleteTime = DateTime.Now.ToString();
                //操作日志
                new Log<IndexToRGBRequest>(log);
            }
            catch (Exception ex)
            {
                log.Status = "错误";
                log.ErrorMsg = ex.ToString();
                log.ErrorDate = DateTime.Now.ToString();
                new Log<IndexToRGBRequest>(log);
            }

            return null;
        }

        #region 索引转换RGB图辅助方法

        FileInfo[] GetFileList(DirectoryInfo TheFolder, String FileFormat)
        {
            FileInfo[] file_list = TheFolder.GetFiles(FileFormat);
            DirectoryInfo[] folder_list = TheFolder.GetDirectories();

            foreach (DirectoryInfo NextFolder in folder_list)
            {
                FileInfo[] child_file_list = GetFileList(NextFolder, FileFormat);
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
        /// 判断图片的PixelFormat 是否在 引发异常的 PixelFormat 之中  
        /// </summary>  
        /// <param name="imgPixelFormat">原图片的PixelFormat</param>  
        /// <returns></returns>  
        bool IsPixelFormatIndexed(PixelFormat imgPixelFormat)
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
        PixelFormat[] indexedPixelFormats = {
            PixelFormat.Undefined,
            PixelFormat.DontCare,
            PixelFormat.Format16bppArgb1555,
            PixelFormat.Format1bppIndexed,
            PixelFormat.Format4bppIndexed,
            PixelFormat.Format8bppIndexed
        };

        #endregion

        #endregion
    }
}
