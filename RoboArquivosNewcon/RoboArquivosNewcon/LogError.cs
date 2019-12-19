﻿using System;
using System.IO;
using System.Text.RegularExpressions;

namespace RoboArquivosNewcon
{
    public class LogError
    {
        public static void LogErrorMessage(Exception ex) { LogErrorMessage(ex.Message); }

        public static void LogErrorMessage(string message)
        {
            var exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            Regex appPathMatcher = new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");
            var folderLog = appPathMatcher.Match(exePath).Value;
            if (string.IsNullOrEmpty(folderLog))
            {
                folderLog = Directory.GetCurrentDirectory();
            }

            string fileLogName = Path.Combine(folderLog, string.Format("{0}.log", DateTime.Now.ToString("yyyyMMdd")));

            FileStream fileLog;
            if (!File.Exists(fileLogName))
            { fileLog = new FileStream(fileLogName, FileMode.CreateNew); }
            else
            { fileLog = new FileStream(fileLogName, FileMode.Append); }

            using (StreamWriter flog = new StreamWriter(fileLog))
            {
                flog.WriteLine("{0} - {1}", DateTime.Now.ToString("dd/MM/yyyy HH:mm"), message);
            }
        }

    }
}
