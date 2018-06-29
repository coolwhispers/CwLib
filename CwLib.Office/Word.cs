using System;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Office.Interop.Word;

namespace CwLib.Office
{
    public class Word : OfficeApplication
    {
        Application _app = null;
        Document _doc = null;

        string _tempPath;
        public Word(Stream stream)
        {
            _tempPath = System.IO.Path.GetTempFileName();

            using(var fileStream = File.Create(_tempPath))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }

            _doc = _app.Documents.Open(_tempPath, Type.Missing, true);

            _app.Visible = false; //不顯示Word視窗
        }

        /// <summary>
        /// 列印Word文件
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="printerName">Print Device Name, default use active printer</param>
        public void Print(Stream stream, string printerName = "")
        {
            if (string.IsNullOrWhiteSpace(printerName))
            {
                _doc.PrintOut();
            }
            else
            {
                string tempPrint = _app.ActivePrinter;
                _app.ActivePrinter = printerName;
                _doc.PrintOut();
                _app.ActivePrinter = tempPrint;
            }
        }

        public void Close()
        {
            if (_app != null)
            {
                _doc.Close();
                _app.Quit();

                System.Runtime.InteropServices.Marshal.ReleaseComObject(_doc);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(_app);

                _doc = null;
                _app = null;
            }

            if (!string.IsNullOrWhiteSpace(_tempPath) && File.Exists(_tempPath))
            {
                File.Delete(_tempPath);
            }
        }

        public void Dispose()
        {
            Close();
        }

    }
}
