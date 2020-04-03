using glTech.Log4netWrapper;
using Nancy.Hosting.Self;
using System;
using System.ServiceProcess;
using Yumaster.File.Data;
using Yumaster.File.Storage.Service;

namespace Yumaster.File.Storage
{
    static class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Logger.Initialize(PathUtil.GetLog4netPath());
            Logger.LogError("开始");
            string strAppType = CommonHelp.GetConfig("apptype", "0");
            if (strAppType == "0")
            {
                try
                {
                    var hostConfiguration = new HostConfiguration
                    {
                        UrlReservations = new UrlReservations() { CreateAutomatically = true }
                    };
                    string strIP = AppSettingB.GetValueByKey("ip");
                    string port = AppSettingB.GetValueByKey("port");
                    string url = string.Format("http://{0}:{1}", strIP, port);
                    var rootPath = AppSettingB.GetValueByKey("path");
                    var nancyHost = new NancyHost(new RestBootstrapper(), hostConfiguration, new Uri(url));
                    nancyHost.Start();
                    System.Console.WriteLine("文件中心服务开启，管理地址：" + url.ToString());
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    Logger.LogError("启动NancyHost失败");
                    Logger.LogError4Exception(ex);
                }
            }
            else
            {
                if (!Environment.UserInteractive)
                {
                    ServiceBase[] ServiceToRun;
                    ServiceToRun = new ServiceBase[]
                        {
                            new FileCenterService()
                        };
                    ServiceBase.Run(ServiceToRun);
                }
                else
                {
                    FileCenterService service = new FileCenterService();
                    System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);  // forces debug to keep VS running while we debug the service  
                }
            }
        }
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e != null && e.ExceptionObject != null)
            {
                Logger.LogError("未捕获异常");
                Logger.LogError4Exception((Exception)e.ExceptionObject);
            }
        }
    }
}
