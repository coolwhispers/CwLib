using System;
using System.IO;
using System.Linq.Expressions;

namespace CwLib.IO
{
    public abstract class FileSave
    {

        /// <summary>
        /// 如果檔名重複則重新命名
        /// </summary>
        /// <param name="fileName">檔案名稱</param>
        /// <param name="expression">檢查條件式</param>
        /// <returns></returns>
        public static string IsFileExistThenRename(string fileName, Expression<Func<string, bool>> expression)
        {
            var resultName = fileName;

            var fileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(fileName);
            var ext = System.IO.Path.GetExtension(fileName);

            var count = 0;

            var func = expression.Compile();
            while (func.Invoke(resultName))
            {
                resultName = fileNameWithoutExtension + "_" + count + ext;
            }

            return resultName;
        }
    }


    public class StorageFileSave : FileSave
    {
        /// <summary>
        /// 目標資料夾路徑
        /// </summary>
        protected string Path;

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="path">目標資料夾</param>
        public StorageFileSave(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                Path = string.Empty;
            }
            else
            {
                path = path.Replace("/", "\\");
                if (!path.EndsWith("\\"))
                {
                    path += "\\";
                }
                Path = path;
            }
        }

        /// <summary>
        /// 儲存資料流
        /// </summary>
        /// <param name="stream">資料流(FileStream會自行抓取名稱，否則隨機名稱)</param>
        /// <returns></returns>
        public string Save(Stream stream)
        {
            if (stream is FileStream)
            {
                return Save(stream, System.IO.Path.GetFileName(((FileStream)stream).Name));
            }

            return Save(stream, Guid.NewGuid().ToString("D").ToUpper());
        }

        /// <summary>
        /// 儲存資料流
        /// </summary>
        /// <param name="stream">資料流</param>
        /// <param name="fileName">檔案名稱</param>
        /// <returns></returns>
        public string Save(Stream stream, string fileName)
        {
            var filePath = Path + IsFileExistThenRename(fileName, x => System.IO.File.Exists(Path + x));

            using (var fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }

            return filePath;
        }

    }
}