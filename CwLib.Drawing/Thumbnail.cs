using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;

namespace CwLib.Drawing
{
    /// <summary>
    /// 縮圖
    /// </summary>
    public class Thumbnail
    {
        private static readonly Dictionary<string, object> Locklist = new Dictionary<string, object>();
        private static readonly object ThumbnailLock = new object();

        /// <summary>
        /// 縮圖
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="maxPix">The maximum pix.</param>
        /// <param name="isCenter"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static string Start(string path, int maxPix = 320, bool isCenter = false, int height = 0)
        {
            var fileName = Path.GetFileName(path);
            var filePath = path.Remove(path.Length - (string.IsNullOrEmpty(fileName) ? 0 : fileName.Length));

            var fileOnlyName = Path.GetFileNameWithoutExtension(path);
            var ext = Path.GetExtension(fileName);
            var fileExtension = string.IsNullOrEmpty(ext) ? string.Empty : ext.ToLower();

            var thumbnailFileName = (!isCenter) ?
                $"{fileOnlyName}.{maxPix}.thumbnail{fileExtension}" : (height > 0) ?
                $"{fileOnlyName}.{maxPix}x{height}.center.thumbnail{fileExtension}" : $"{fileOnlyName}.{maxPix}x{maxPix}.center.thumbnail{fileExtension}";
            var thumbnailFilePath = filePath + thumbnailFileName;

            object thumbnailLock = null;
            lock (ThumbnailLock)
            {
                if (!Locklist.ContainsKey(thumbnailFilePath))
                {
                    Locklist.Add(thumbnailFilePath, new object());
                }

                thumbnailLock = Locklist[thumbnailFilePath];
            }

            lock (thumbnailLock)
            {
                if (!File.Exists(thumbnailFilePath))
                {
                    if (!isCenter)
                    {
                        ThumbnailImage.SaveThumbPic(path, maxPix, thumbnailFilePath);
                    }
                    else
                    {
                        ThumbnailImage.SaveCenterThumbPic(path, maxPix, thumbnailFilePath, height);
                    }
                }
            }

            return thumbnailFileName;
        }


        /// <summary>
        /// 縮圖
        /// </summary>
        public class ThumbnailImage
        {
            #region  取得圖片等比例縮圖後的寬和高像素
            /// <summary>
            ///  寬高誰較長就縮誰  - 計算方法
            /// </summary>
            /// <param name="image">System.Drawing.Image 的物件</param>
            /// <param name="maxPx">寬或高超過多少像素就要縮圖</param>
            /// <returns>回傳int陣列，索引0為縮圖後的寬度、索引1為縮圖後的高度</returns>
            private static int[] GetThumbPic_WidthAndHeight(System.Drawing.Image image, int maxPx)
            {

                int fixWidth = 0;

                int fixHeight = 0;

                if (image.Width > maxPx || image.Height > maxPx)
                //如果圖片的寬大於最大值或高大於最大值就往下執行 
                {

                    if (image.Width >= image.Height)
                    //圖片的寬大於圖片的高 
                    {

                        fixHeight = Convert.ToInt32((Convert.ToDouble(maxPx) / Convert.ToDouble(image.Width)) * Convert.ToDouble(image.Height));
                        //設定修改後的圖高 
                        fixWidth = maxPx;
                    }
                    else
                    {

                        fixWidth = Convert.ToInt32((Convert.ToDouble(maxPx) / Convert.ToDouble(image.Height)) * Convert.ToDouble(image.Width));
                        //設定修改後的圖寬 
                        fixHeight = maxPx;

                    }

                }
                else
                {//圖片沒有超過設定值，不執行縮圖 

                    fixHeight = image.Height;

                    fixWidth = image.Width;

                }

                int[] fixWidthAndfixHeight = { fixWidth, fixHeight };



                return fixWidthAndfixHeight;
            }


            /// <summary>
            /// 寬度維持maxWidth，高度等比例縮放   - 計算方法
            /// </summary>
            /// <param name="image"></param>
            /// <param name="maxWidth"></param>
            /// <returns></returns>
            private static int[] GetThumbPic_Width(System.Drawing.Image image, int maxWidth)
            {
                //要回傳的結果
                int fixWidth = 0;
                int fixHeight = 0;

                if (image.Width > maxWidth)
                //如果圖片的寬大於最大值
                {


                    //等比例的圖高
                    fixHeight = Convert.ToInt32((Convert.ToDouble(maxWidth) / Convert.ToDouble(image.Width)) * Convert.ToDouble(image.Height));
                    //設定修改後的圖寬 
                    fixWidth = maxWidth;

                }
                else
                {//圖片寬沒有超過設定值，不執行縮圖 

                    fixHeight = image.Height;

                    fixWidth = image.Width;

                }

                int[] fixWidthAndfixHeight = { fixWidth, fixHeight };



                return fixWidthAndfixHeight;


            }

            /// <summary>
            /// 高度維持maxHeight，寬度等比例縮放  - 計算方法
            /// </summary>
            /// <param name="image"></param>
            /// <param name="maxHeight"></param>
            /// <returns></returns>
            private static int[] GetThumbPic_Height(System.Drawing.Image image, int maxHeight)
            {
                //要回傳的值
                int fixWidth = 0;
                int fixHeight = 0;

                if (image.Height > maxHeight)
                //如果圖片的高大於最大值
                {
                    //等比例的寬
                    fixWidth = Convert.ToInt32((Convert.ToDouble(maxHeight) / Convert.ToDouble(image.Height)) * Convert.ToDouble(image.Width));
                    //圖高固定 
                    fixHeight = maxHeight;

                }
                else
                {//圖片的高沒有超過設定值

                    fixHeight = image.Height;

                    fixWidth = image.Width;

                }

                int[] fixWidthAndfixHeight = { fixWidth, fixHeight };



                return fixWidthAndfixHeight;
            }


            /// <summary>
            /// 寬高誰較短就放大誰  - 計算方法
            /// </summary>
            /// <param name="image"></param>
            /// <param name="maxPx"></param>
            /// <param name="height"></param>
            /// <returns></returns>
            private static int[] GetEnlargePic_WidthAndHeight(System.Drawing.Image image, int maxPx, int height = 0)
            {
                int fixWidth = 0;

                int fixHeight = 0;

                if (height > maxPx)
                {
                    maxPx = height;
                }

                if (image.Width < maxPx || image.Height < maxPx)
                //如果圖片的寬小於最大值或高小於最大值就往下執行 
                {

                    if (image.Width >= image.Height)
                    //圖片的寬大於圖片的高 
                    {
                        //設定修改後的圖寬
                        fixWidth = Convert.ToInt32((Convert.ToDouble(maxPx) / Convert.ToDouble(image.Height)) * Convert.ToDouble(image.Width));
                        //設定的圖高調至最大值
                        fixHeight = maxPx;
                    }
                    else
                    {
                        //設定修改後的圖高 
                        fixHeight = Convert.ToInt32((Convert.ToDouble(maxPx) / Convert.ToDouble(image.Width)) * Convert.ToDouble(image.Height));
                        //設定的圖寬調至最大值
                        fixWidth = maxPx;
                    }

                }
                else
                {//圖片沒有超過設定值，不執行縮圖 

                    fixHeight = image.Height;

                    fixWidth = image.Width;

                }

                int[] fixWidthAndfixHeight = { fixWidth, fixHeight };

                return fixWidthAndfixHeight;

            }

            #endregion

            #region 產生縮圖並儲存
            /// <summary>
            /// 產生縮圖並儲存 寬高誰較長就縮誰
            /// </summary>
            /// <param name="srcImagePath">來源圖片的路徑</param>
            /// <param name="maxPix">超過多少像素就要等比例縮圖</param>
            /// <param name="saveThumbFilePath">縮圖的儲存檔案路徑</param>
            public static void SaveThumbPic(string srcImagePath, int maxPix, string saveThumbFilePath)
            {
                //讀取原始圖片
                using (FileStream fs = new FileStream(srcImagePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    //取得原始圖片
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(fs);

                    //圖片寬高
                    int ImgWidth = bitmap.Width;
                    int ImgHeight = bitmap.Height;
                    // 計算維持比例的縮圖大小
                    int[] thumbnailScaleWidth = GetThumbPic_WidthAndHeight(bitmap, maxPix);
                    int AfterImgWidth = thumbnailScaleWidth[0];
                    int AfterImgHeight = thumbnailScaleWidth[1];

                    // 產生縮圖
                    using (var bmp = new Bitmap(AfterImgWidth, AfterImgHeight))
                    {
                        using (var gr = Graphics.FromImage(bmp))
                        {

                            gr.CompositingQuality = CompositingQuality.HighSpeed;
                            gr.SmoothingMode = SmoothingMode.HighSpeed;
                            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            gr.DrawImage(bitmap, new Rectangle(0, 0, AfterImgWidth, AfterImgHeight), 0, 0, ImgWidth, ImgHeight, GraphicsUnit.Pixel);

                            bmp.Save(saveThumbFilePath);
                        }
                    }

                }
            }

            /// <summary>
            /// 產生縮圖並儲存 寬度維持maxpix，高度等比例
            /// </summary>
            /// <param name="srcImagePath">來源圖片的路徑</param>
            /// <param name="widthMaxPix">超過多少像素就要等比例縮圖</param>
            /// <param name="saveThumbFilePath">縮圖的儲存檔案路徑</param>
            public static void SaveThumbPicWidth(string srcImagePath, int widthMaxPix, string saveThumbFilePath)
            {
                //讀取原始圖片
                using (FileStream fs = new FileStream(srcImagePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    //取得原始圖片
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(fs);

                    //圖片寬高
                    int ImgWidth = bitmap.Width;
                    int ImgHeight = bitmap.Height;
                    // 計算維持比例的縮圖大小
                    int[] thumbnailScaleWidth = GetThumbPic_Width(bitmap, widthMaxPix);
                    int AfterImgWidth = thumbnailScaleWidth[0];
                    int AfterImgHeight = thumbnailScaleWidth[1];

                    // 產生縮圖
                    using (var bmp = new Bitmap(AfterImgWidth, AfterImgHeight))
                    {
                        using (var gr = Graphics.FromImage(bmp))
                        {

                            gr.CompositingQuality = CompositingQuality.HighSpeed;
                            gr.SmoothingMode = SmoothingMode.HighSpeed;
                            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            gr.DrawImage(bitmap, new Rectangle(0, 0, AfterImgWidth, AfterImgHeight), 0, 0, ImgWidth, ImgHeight, GraphicsUnit.Pixel);
                            bmp.Save(saveThumbFilePath);
                        }
                    }

                }
            }

            /// <summary>
            /// 產生縮圖並儲存 高度維持maxPix，寬度等比例
            /// </summary>
            /// <param name="srcImagePath">來源圖片的路徑</param>
            /// <param name="heightMaxPix">超過多少像素就要等比例縮圖</param>
            /// <param name="saveThumbFilePath">縮圖的儲存檔案路徑</param>
            public static void SaveThumbPicHeight(string srcImagePath, int heightMaxPix, string saveThumbFilePath)
            {
                //讀取原始圖片
                using (FileStream fs = new FileStream(srcImagePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    //取得原始圖片
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(fs);

                    //圖片寬高
                    int ImgWidth = bitmap.Width;
                    int ImgHeight = bitmap.Height;
                    // 計算維持比例的縮圖大小
                    int[] thumbnailScaleWidth = GetThumbPic_Height(bitmap, heightMaxPix);
                    int AfterImgWidth = thumbnailScaleWidth[0];
                    int AfterImgHeight = thumbnailScaleWidth[1];

                    // 產生縮圖
                    using (var bmp = new Bitmap(AfterImgWidth, AfterImgHeight))
                    {
                        using (var gr = Graphics.FromImage(bmp))
                        {

                            gr.CompositingQuality = CompositingQuality.HighSpeed;
                            gr.SmoothingMode = SmoothingMode.HighSpeed;
                            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            gr.DrawImage(bitmap, new Rectangle(0, 0, AfterImgWidth, AfterImgHeight), 0, 0, ImgWidth, ImgHeight, GraphicsUnit.Pixel);
                            bmp.Save(saveThumbFilePath);
                        }
                    }

                }
            }

            /// <summary>
            /// 產生縮圖裁切中心並儲存 高度height決定裁切寬度
            /// </summary>
            /// <param name="srcImagePath"></param>
            /// <param name="maxPix"></param>
            /// <param name="saveThumbFilePath"></param>
            /// <param name="height"></param>
            public static void SaveCenterThumbPic(string srcImagePath, int maxPix, string saveThumbFilePath, int height)
            {
                //讀取原始圖片
                using (FileStream fs = new FileStream(srcImagePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    //取得原始圖片
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(fs);

                    //圖片寬高
                    int ImgWidth = bitmap.Width;
                    int ImgHeight = bitmap.Height;

                    // 計算維持比例的縮圖大小
                    int[] thumbnailScaleWidth = GetEnlargePic_WidthAndHeight(bitmap, maxPix, height);
                    int AfterImgWidth = thumbnailScaleWidth[0];
                    int AfterImgHeight = thumbnailScaleWidth[1];

                    // 產生縮圖
                    using (var bmp = new Bitmap(AfterImgWidth, AfterImgHeight))
                    {
                        using (var gr = Graphics.FromImage(bmp))
                        {
                            gr.CompositingQuality = CompositingQuality.HighSpeed;
                            gr.SmoothingMode = SmoothingMode.HighSpeed;
                            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            gr.DrawImage(bitmap, new Rectangle(0, 0, AfterImgWidth, AfterImgHeight), 0, 0, ImgWidth, ImgHeight, GraphicsUnit.Pixel);

                            //bmp.Save(saveThumbFilePath);
                            #region 影像裁切(裁切中心 預設maxPix)

                            int cropHeight = (height > 0) ? height : maxPix;

                            //建立新的影像
                            System.Drawing.Image cropImage = new Bitmap(maxPix, cropHeight) as System.Drawing.Image;
                            //準備繪製新的影像
                            Graphics graphics2 = Graphics.FromImage(cropImage);
                            //設定裁切範圍
                            Rectangle cropRect = new Rectangle(AfterImgWidth / 2 - (maxPix / 2), AfterImgHeight / 2 - (cropHeight / 2), maxPix, cropHeight);
                            //於座標(0,0)開始繪製裁切影像
                            graphics2.DrawImage(bmp, 0, 0, cropRect, GraphicsUnit.Point);
                            graphics2.Dispose();
                            //儲存新的影像
                            cropImage.Save(saveThumbFilePath);
                            #endregion

                        }
                    }





                }

            }

            #endregion



            /**/
            /// <param name="fileName">图像名</param>
            /// <param name="quality">品质</param>
            public static void SaveImage(string fileName, int quality)
            {

                Bitmap myBitmap = new Bitmap(fileName);


                System.Drawing.Imaging.EncoderParameters myEncoderParameters =
                    new System.Drawing.Imaging.EncoderParameters(1);

                System.Drawing.Imaging.EncoderParameter myEncoderParameter =
                    new System.Drawing.Imaging.EncoderParameter(
                    System.Drawing.Imaging.Encoder.Quality, quality);

                myEncoderParameters.Param[0] = myEncoderParameter;


                System.Drawing.Imaging.ImageCodecInfo myImageCodecInfo;
                myImageCodecInfo = GetEncoderInfo("image/jpeg");


                string ext = myImageCodecInfo.FilenameExtension.Split(';')[0];
                ext = System.IO.Path.GetExtension(ext).ToLower();

                string saveName = System.IO.Path.ChangeExtension(fileName, ext);

                //保存
                myBitmap.Save(saveName, myImageCodecInfo, myEncoderParameters);
            }

            //获取MimeType
            private static System.Drawing.Imaging.ImageCodecInfo
                GetEncoderInfo(string mineType)
            {

                System.Drawing.Imaging.ImageCodecInfo[] myEncoders =
                    System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();

                foreach (System.Drawing.Imaging.ImageCodecInfo myEncoder in myEncoders)
                {
                    if (myEncoder.MimeType == mineType)
                    {
                        return myEncoder;
                    }
                }
                return null;
            }

        }
    }
}