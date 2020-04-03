using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yumaster.File.Storage
{
    public class PathUtil
    {
        private const string CERT_PATH = "cert.bat";
        internal static string GetLog4netPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "log4netService.config");
        }
        internal static string GetAppConfigPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "App.config");
        }
        internal static string GetDatabasePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "", "FileCenter.db");
        }
        internal static string GetCertPath()
        {
            return Combine(CERT_PATH);
        }
        private static string Combine(params string[] args)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine(args));
        }
    }
}
