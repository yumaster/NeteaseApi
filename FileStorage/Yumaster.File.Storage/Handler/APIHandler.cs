using glTech.Log4netWrapper;
using Nancy;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Yumaster.File.Data;

namespace Yumaster.File.Storage.Handler
{
    public class APIHandler:NancyModule
    {
        public APIHandler() : base()
        {
            Msg_Result msg = new Msg_Result() { Action = "", ErrorMsg = "" };
            Auth_UserB.UserInfo userInfo = new Auth_UserB.UserInfo();
            Before += ctx =>
             {
                 try
                 {
                     //Logger.LogInfo("请求地址" + ctx.Request.Path);
                     if (ctx.Request.Path == "/adminapi/login")
                     {
                         return ctx.Response;
                     }
                     else
                         if (ctx.Request.Path == "/adminapi/getuser")
                     {
                         return ctx.Response;
                     }
                     else
                         if (ctx.Request.Path == "/")
                     {
                         return Response.AsRedirect("/Web/Login.html");
                     }
                     else
                         if (ctx.Request.Path.StartsWith("/adminapi/dfile"))
                     {
                         return ctx.Response;
                     }
                     else
                         if (ctx.Request.Path.StartsWith("/adminapi/ExeActionPub"))
                     {
                         return ctx.Response;
                     }
                     else
                     {
                         string strUser = "";
                         string strpasd = "";
                         string strDay = "";
                         string filecode = "";
                         List<User> users = new List<User>();
                         if (ctx.Request.Cookies.ContainsKey("user"))
                         {
                             strUser = ctx.Request.Cookies["user"];
                         }
                         if (ctx.Request.Cookies.ContainsKey("filecode"))
                         {
                             filecode = ctx.Request.Cookies["filecode"];
                         }
                         if (string.IsNullOrEmpty(filecode))
                         {
                             filecode = ctx.Request.Query["filecode"];
                         }
                         if (string.IsNullOrEmpty(filecode))
                         {
                             filecode = ctx.Request.Form["filecode"];
                         }
                         if (!string.IsNullOrEmpty(filecode))
                         {
                             strUser = new CacheHelp().Get(filecode);
                             users = new Auth_UserB().GetEntities(d => d.username == strUser).ToList();
                             new CacheHelp().Set(filecode, strUser);

                         }
                         if (users.Count() != 1)
                         {
                             msg.ErrorMsg = "NOSESSIONCODE";
                             return Response.AsText(JsonConvert.SerializeObject(msg), "text/html; charset=utf-8");
                         }
                         else
                         {
                             userInfo.User = users[0];
                         }
                         return ctx.Response;
                     }
                 }
                 catch (Exception ex)
                 {
                     msg.ErrorMsg = ex.Message.ToString();
                     return Response.AsText(JsonConvert.SerializeObject(msg), "text/html; charset=utf-8");
                 }
             };
            Get["/"] = p =>
            {
                return Response.AsRedirect("/Web/Login.html");
            };
            ///获取文件
            Get["/adminapi/dfile/{fileid}"] = p =>
            {
                int fileId = 0;
                int.TryParse(p.fileid.Value.Split(',')[0], out fileId);
                File_Info file = new File_InfoB().GetEntity(d => d.ID == fileId);
                string strzyid = p.fileid.Value;
                if (file != null)
                {
                    strzyid = file.zyid;
                }

                string width = Context.Request.Query["width"].Value ?? "";
                string height = Context.Request.Query["height"].Value ?? "";
                QyCode qy = new QyCodeB().GetAllEntities().FirstOrDefault();
                string filename = Context.Request.Url.SiteBase + "/" + qy.Code + "/document/" + strzyid;
                if (width + height != "")
                {
                    filename = Context.Request.Url.SiteBase + "/" + qy.Code + "/document/image/" + strzyid + (width + height != "" ? ("/" + width + "/" + height) : "");
                }
                return Response.AsRedirect(filename);

            };
            //登录接口
            Post["/adminapi/login"] = p =>
            {
                string strUser = Context.Request.Form["user"];
                string strpasd = Context.Request.Form["pasd"];
                var users = new Auth_UserB().GetEntities(d => d.username == strUser && d.pasd == CommonHelp.GetMD5(strpasd));
                if (users.Count() == 1)
                {
                    msg.Result = "Y";
                    string strToken = CommonHelp.GenerateToken(strUser);
                    msg.Result2 = strToken;
                    new CacheHelp().Set(strToken, strUser);

                }
                else
                {
                    msg.Result = "N";
                }
                //JsonConvert.SerializeObject(Model).Replace("null", "\"\"");
                return Response.AsJson(msg);
            };
            Post["/adminapi/getuser"] = p =>
            {
                string strUser = Context.Request.Form["user"];
                string Secret = Context.Request.Form["Secret"];
                string strlotusSecret = CommonHelp.GetConfig("lotusSecret", "yumaster.net");
                Logger.LogError("用户" + strUser + strlotusSecret + "获取Code" + Secret);
                if (Secret == strlotusSecret)
                {
                    var users = new Auth_UserB().GetEntities(d => d.username == strUser);
                    if (users.Count() == 1)
                    {
                        string strToken = CommonHelp.GenerateToken(strUser);
                        msg.Result = strToken;
                        new CacheHelp().Set(strToken, strUser);
                    }
                    else
                    {
                        msg.ErrorMsg = "找不到用户信息";
                        return Response.AsText(JsonConvert.SerializeObject(msg), "text/html; charset=utf-8");
                    }
                }
                else
                {
                    msg.ErrorMsg = "Secret不匹配";
                    return Response.AsText(JsonConvert.SerializeObject(msg), "text/html; charset=utf-8");
                }

                //JsonConvert.SerializeObject(Model).Replace("null", "\"\"");
                return Response.AsJson(msg);
            };

            //普通用户接口
            Post["/adminapi/ExeAction/{action}"] = p =>
            {
                JObject JsonData = new JObject();

                foreach (string item in Context.Request.Form.Keys)
                {
                    JsonData.Add(item, Context.Request.Form[item].Value);
                }
                string PostData = "";
                string P1 = JsonData["P1"] == null ? "" : JsonData["P1"].ToString();
                string P2 = JsonData["P2"] == null ? "" : JsonData["P2"].ToString();

                string strAction = p.action;


                QyCode qy = new QyCodeB().GetAllEntities().FirstOrDefault();
                Auth_Qy QYinfo = new Auth_Qy();
                QYinfo.FileServerUrl = Context.Request.Url.SiteBase;
                QYinfo.QyCode = qy.Code;
                userInfo.QyInfo = QYinfo;
                //Dictionary<string, string> results3 = JsonConvert.DeserializeObject<Dictionary<string, string>>(PostData.ToString());
                var function = Activator.CreateInstance(typeof(QYWDManage)) as QYWDManage;
                var method = function.GetType().GetMethod(strAction.ToUpper());
                method.Invoke(function, new object[] { JsonData, msg, P1, P2, userInfo });

                IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
                timeConverter.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
                string Result = JsonConvert.SerializeObject(msg, Formatting.Indented, timeConverter).Replace("null", "\"\"");
                return Response.AsText(Result, "text/html; charset=utf-8");
            };

            //管理用户接口
            Post["/adminapi/ExeActionAuth/{action}"] = p =>
            {
                JObject JsonData = new JObject();

                foreach (string item in Context.Request.Form.Keys)
                {
                    JsonData.Add(item, Context.Request.Form[item].Value);
                }
                string PostData = "";
                string P1 = JsonData["P1"] == null ? "" : JsonData["P1"].ToString();
                string P2 = JsonData["P2"] == null ? "" : JsonData["P2"].ToString();
                string strAction = p.action;


                QyCode qy = new QyCodeB().GetAllEntities().FirstOrDefault();
                Auth_Qy QYinfo = new Auth_Qy();
                QYinfo.FileServerUrl = Context.Request.Url.SiteBase;
                QYinfo.QyCode = qy.Code;
                userInfo.QyInfo = QYinfo;
                //Dictionary<string, string> results3 = JsonConvert.DeserializeObject<Dictionary<string, string>>(PostData.ToString());
                var function = Activator.CreateInstance(typeof(QYWDManage)) as QYWDManage;
                var method = function.GetType().GetMethod(strAction.ToUpper());
                method.Invoke(function, new object[] { JsonData, msg, P1, P2, userInfo });

                IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
                timeConverter.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
                string Result = JsonConvert.SerializeObject(msg, Formatting.Indented, timeConverter).Replace("null", "\"\"");
                return Response.AsText(Result, "text/html; charset=utf-8");
            };

            //外部接口
            Post["/adminapi/ExeActionPub/{action}"] = p =>
            {
                JObject JsonData = new JObject();

                foreach (string item in Context.Request.Form.Keys)
                {
                    JsonData.Add(item, Context.Request.Form[item].Value);
                }
                string PostData = "";
                string P1 = JsonData["P1"] == null ? "" : JsonData["P1"].ToString();
                string P2 = JsonData["P2"] == null ? "" : JsonData["P2"].ToString();
                string strAction = p.action;


                QyCode qy = new QyCodeB().GetAllEntities().FirstOrDefault();
                Auth_Qy QYinfo = new Auth_Qy();
                QYinfo.FileServerUrl = Context.Request.Url.SiteBase;
                QYinfo.QyCode = qy.Code;
                userInfo.QyInfo = QYinfo;
                //Dictionary<string, string> results3 = JsonConvert.DeserializeObject<Dictionary<string, string>>(PostData.ToString());
                var function = Activator.CreateInstance(typeof(PubManage)) as PubManage;
                var method = function.GetType().GetMethod(strAction.ToUpper());
                method.Invoke(function, new object[] { JsonData, msg, P1, P2, userInfo });

                IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
                timeConverter.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
                string Result = JsonConvert.SerializeObject(msg, Formatting.Indented, timeConverter).Replace("null", "\"\"");
                return Response.AsText(Result, "text/html; charset=utf-8");
            };

            After += ctx =>
            {
                msg.Action = Context.Request.Path;
                //添加日志
            };
        }
    }
}
