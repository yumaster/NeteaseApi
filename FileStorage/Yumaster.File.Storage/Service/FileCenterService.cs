﻿using glTech.Log4netWrapper;
using Nancy.Hosting.Self;
using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using Yumaster.File.Data;

namespace Yumaster.File.Storage.Service
{
    public partial class FileCenterService : ServiceBase
    {
        private Thread _thread;
        private bool _isStop;
        public FileCenterService()
        {
            InitializeComponent();
            try
            {
                var nancyPost = AppSettingB.GetValueByKey("post");
                var rootPath = AppSettingB.GetValueByKey("path");
                string ip = AppSettingB.GetValueByKey("ip");
                StartNancyHost(rootPath, ip, nancyPost);
            }
            catch (Exception ex)
            {
                Logger.LogError4Exception(ex);
            }
        }
        protected override void OnStart(string[] args)
        {
            try
            {
                _isStop = false;
                _thread = new Thread(ThreadMain)
                {
                    Name = "REST Finder"
                };

                _thread.Start();
            }
            catch (Exception ex)
            {
                Logger.LogError4Exception(ex);
            }
        }
        /// <summary>
        /// 启动Nancy服务
        /// </summary>
        private void StartNancyHost(string rootPath, string IP, string portNO = "9100")
        {
            try
            {
                Logger.LogInfo("启动NancyHost开始");

                var hostConfiguration = new HostConfiguration
                {
                    UrlReservations = new UrlReservations() { CreateAutomatically = true }
                };
                string url = string.Format("http://localhost:{0}", portNO);

                var nancyHost = new NancyHost(new RestBootstrapper(), hostConfiguration, new Uri(url));
                nancyHost.Start();
            }
            catch (Exception ex)
            {
                Logger.LogError("启动NancyHost失败.");
                Logger.LogError4Exception(ex);
            }
        }


        private void StartNancyHostHttps(string rootPath, int portNO = 8080)
        {
            try
            {
                var hostConfiguration = new HostConfiguration
                {
                    UrlReservations = new UrlReservations() { CreateAutomatically = true }
                };

                //StartBat(PathUtil.GetCertPath());
                string args = string.Format("http add sslcert ipport=0.0.0.0:{0} certhash=0460ee52d52f7ac2a26fa9b126b34f2a36da61e7 appid={{dbe03eec-c167-4381-ae63-7f04c60cb6c8}} clientcertnegotiation=enable", portNO);
                StartBat("netsh", args);

                string url = string.Format("https://localhost:{0}", portNO);

                var nancyHost = new NancyHost(new RestBootstrapper(), hostConfiguration, new Uri(url));
                nancyHost.Start();
                System.Console.WriteLine("地址" + url);
            }
            catch (Exception ex)
            {
                Logger.LogError("启动NancyHost失败.");
                Logger.LogError4Exception(ex);
            }
        }

        private void StartBat(string fileName, string arguments = "")
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = fileName;
            p.StartInfo.Arguments = arguments;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
        }

        private void ThreadMain()
        {
            while (!_isStop)
            {
                try
                {
                    Console.WriteLine(PathUtil.GetLog4netPath());

                }
                catch (Exception ex)
                {
                    Logger.LogError4Exception(ex);
                }

                Thread.Sleep(1000);
            }
        }
        protected override void OnStop()
        {
            _isStop = true;
            _thread.Join();
        }
    }
}