using Nancy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        }
    }
}
