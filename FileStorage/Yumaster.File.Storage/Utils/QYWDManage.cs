using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using Yumaster.File.Data;

namespace Yumaster.File.Storage
{
    public class QYWDManage
    {
        public void GETUSERINFO(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                msg.Result = UserInfo;
                msg.Result1 = AppSettingB.GetValueByKey("sysname");
                msg.Result2 = AppSettingB.GetValueByKey("qyname");
                msg.Result3 = AppSettingB.GetValueByKey("qyico");
                QyCode qycode = new QyCodeB().GetAllEntities().FirstOrDefault();
                msg.Result4 = qycode;
            }
            catch (Exception ex)
            {
                CommonHelp.WriteLOG(ex.Message.ToString());
            }
        }

        /// <summary>
        /// 获取用户列表,排除管理员
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>

        public void GETUSERS(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            msg.Result = new Auth_UserB().GetEntities(D => D.username != "admin");
        }


        public void DELUSER(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            new Auth_UserB().Delete(D => D.username == P1);
        }


        public void MANGUSER(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            User model = JsonConvert.DeserializeObject<User>(P1);
            if (string.IsNullOrEmpty(model.username))
            {
                msg.ErrorMsg = "用户名称不能为空";
            }
            else
            {
                if (model.ID == 0)
                {
                    User nmodel = new Auth_UserB().GetEntity(d => d.username == model.username);
                    if (nmodel != null)
                    {
                        msg.ErrorMsg = "用户名已存在";
                    }
                }
            }
            if (model.ID == 0)
            {
                model.Space = 0;
                if (P2 == "")
                {
                    P2 = CommonHelp.GetConfig("depad", "yumaster");
                }
                model.pasd = CommonHelp.GetMD5(P2);
                new Auth_UserB().Insert(model);
            }
            else
            {
                new Auth_UserB().Update(model);
            }
        }

        public void TBUSER(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            List<string> ListUSER = P1.SplitTOList(',');
            for (int i = 0; i < ListUSER.Count; i++)
            {
                string strUserName = ListUSER[i].Split('$')[0];
                string strUserRealName = ListUSER[i].Split('$')[1];
                User model = new User();
                User nmodel = new Auth_UserB().GetEntity(d => d.username == strUserName);
                if (nmodel != null)
                {
                    new Auth_UserB().Update(nmodel);
                }
                else
                {
                    model.username = strUserName;
                    model.UserRealName = strUserRealName;
                    model.Space = 0;
                    model.Role = "普通用户";
                    model.pasd = CommonHelp.GetMD5(CommonHelp.GetConfig("depad", "yumaster"));
                    new Auth_UserB().Insert(model);
                }
            }
        }

        /// <summary>
        /// 添加文件夹
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void ADDFLODER(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            Folder_Info Folder = JsonConvert.DeserializeObject<Folder_Info>(P1);
            if (Folder.Name == "")
            {
                Folder.Name = "新建文件夹";
            }
            if (Folder.ID == 0)
            {
                Folder.CRUser = UserInfo.User.username;
                Folder.CRDate = DateTime.Now;
                Folder.ComId = 1;
                Folder.ViewAuthUsers = "0";//默认不在回收站
                new Folder_InfoB().Insert(Folder);
                //更新文件夹路径Code
                Folder.Remark = Folder.Remark + "-" + Folder.ID;
                new Folder_InfoB().Update(Folder);
            }
            else
            {

            }
            msg.Result = Folder;

        }
        public void UPDATENAME(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            string strName = context.Request("Name") ?? "";
            if (strName != "")
            {
                if (P1 == "file")
                {
                    new File_InfoB().ExsSql("UPDATE File_Info SET NAME ='" + strName + "'WHERE ID = " + P2);
                }
                else
                {
                    new File_InfoB().ExsSql("UPDATE Folder_Info SET NAME ='" + strName + "'WHERE ID = " + P2);
                }
            }
        }

        /// <summary>
        /// 移动剪切-粘贴
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void PASTEITEM(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            int PID = int.Parse(P1);
            JArray PASTEITEMS = (JArray)JsonConvert.DeserializeObject(P2);
            string strPASTEtype = context.Request("PASTETYPE");
            if (strPASTEtype == "copy")
            {
                foreach (var item in PASTEITEMS)
                {
                    int itemid = int.Parse(item["ID"].ToString());
                    if (item["type"].ToString() == "file")
                    {
                        File_Info Model = new File_InfoB().GetEntity(d => d.ID == itemid);
                        Model.FolderID = PID;
                        new File_InfoB().Insert(Model);
                    }
                    else
                    {
                        new Folder_InfoB().CopyFloderTree(itemid, PID);

                    }
                }
            }
            else
            {
                foreach (var item in PASTEITEMS)
                {
                    int itemid = int.Parse(item["ID"].ToString());
                    if (item["type"].ToString() == "file")
                    {
                        File_Info Model = new File_InfoB().GetEntity(d => d.ID == itemid);
                        Model.FolderID = PID;
                        new File_InfoB().Update(Model);
                    }
                    else
                    {
                        Folder_Info PModel = new Folder_InfoB().GetEntity(d => d.ID == PID);
                        Folder_Info Model = new Folder_InfoB().GetEntity(d => d.ID == itemid);
                        Model.PFolderID = PID;
                        new Folder_InfoB().Update(Model);

                        //找到所有需要更新得，然后批量更新
                        List<Folder_Info> ALLFolders = new Folder_InfoB().GetEntities(" Remark LIKE '" + Model.Remark + "%'").ToList();
                        string strOldRemark = Model.Remark;
                        string strNewRemark = PModel.Remark + "-" + Model.ID;
                        foreach (Folder_Info folder in ALLFolders)
                        {
                            folder.Remark = folder.Remark.Replace(strOldRemark, strNewRemark);
                        }
                        new Folder_InfoB().Update(ALLFolders);
                        //子文件夹路径修改
                        // new Folder_InfoB().ExsSql("  UPDATE  Folder_Info set Remark= '" + PModel.Remark + "'+SUBSTRING(Remark, CHARINDEX('-" + Model.ID + "-',Remark), 2000) WHERE  Remark LIKE '" + Model.Remark + "-%' ");
                    }
                }
            }
        }
        /// <summary>
        /// 放入回收站
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void RECYCLE(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            JArray DELITEM = (JArray)JsonConvert.DeserializeObject(P1);
            foreach (var item in DELITEM)
            {
                if (item["type"].ToString() == "file")
                {
                    File_Info File = new File_InfoB().GetEntity(d => d.ID.ToString() == item["ID"].ToString());
                    File.IsRecycle = "1";
                    new File_InfoB().Update(File);
                }
                else
                {
                    Folder_Info File = new Folder_InfoB().GetEntity(d => d.ID.ToString() == item["ID"].ToString());
                    File.ViewAuthUsers = "1";
                    new Folder_InfoB().Update(File);
                }
            }
        }

        /// <summary>
        /// 彻底删除文件或文件夹
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void DELFLODER(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            JArray DELITEM = (JArray)JsonConvert.DeserializeObject(P1);
            foreach (var item in DELITEM)
            {
                if (item["type"].ToString() == "file")
                {//删除文件
                    new File_InfoB().Delete(d => d.ID.ToString() == item["ID"].ToString());

                    string strZYID = item["zyid"].ToString();
                    //物理删除
                    DELWJ(context, msg, strZYID, P2, UserInfo);
                    new File_UserAuthB().Delete(d => d.RefID.ToString() == item["ID"].ToString() && d.RefType == "1");
                }
                else
                {//删除目录
                    new Folder_InfoB().DelWDTree(int.Parse(item["ID"].ToString()));
                    new File_UserAuthB().Delete(d => d.RefID.ToString() == item["ID"].ToString() && d.RefType == "0");

                }
            }
        }
        public void ADDFILE(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            QJADDFILE(msg, P1, P2, UserInfo);
        }

        public void QJADDFILE(Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                JArray Files = (JArray)JsonConvert.DeserializeObject(P1);
                var date = DateTime.Now;
                List<File_Info> ListData = new List<File_Info>();
                List<File_Info> ListSameData = new List<File_Info>();//重名文件
                foreach (var item in Files)
                {
                    int index = item["filename"].ToString().LastIndexOf('.');
                    string filename = item["filename"].ToString().Substring(0, index);
                    string md5 = item["md5"].ToString();
                    string zyid = item["zyid"].ToString();
                    File_Info File = new File_InfoB().GetSameFile(filename, item["filename"].ToString().Substring(index + 1).ToLower(), int.Parse(P2));
                    if (File == null)//相同目录下没有重复文件
                    {
                        File_Info newfile = new File_Info();
                        newfile.Name = filename;
                        newfile.FileMD5 = md5.Replace("\"", "").Split(',')[0];
                        newfile.zyid = zyid;
                        newfile.FileSize = item["filesize"].ToString();
                        newfile.FileVersin = 0;
                        newfile.CRDate = date;
                        newfile.CRUser = UserInfo.User.username;
                        newfile.UPDDate = date;
                        newfile.ComId = 1;
                        newfile.UPUser = UserInfo.User.username;
                        newfile.FolderID = int.Parse(P2);
                        newfile.IsRecycle = "0";//默认不在回收站
                        newfile.FileExtendName = item["filename"].ToString().Substring(index + 1).ToLower();
                        if (new List<string>() { "txt", "html", "mp3", "doc", "mp4", "flv", "ogg", "avi", "mov", "rmvb", "mkv", "jpg", "gif", "png", "bmp", "jpeg" }.Contains(newfile.FileExtendName.ToLower()))
                        {
                            newfile.ISYL = "Y";
                        }
                        if (new List<string>() { "pdf", "doc", "docx", "ppt", "pptx", "xls", "xlsx" }.Contains(newfile.FileExtendName.ToLower()))
                        {
                            newfile.ISYL = "Y";
                            newfile.YLUrl = "/ViewV5/AppPage/QYWD/doc.html?zyid=" + newfile.zyid;

                        }
                        if (new List<string>() { "mp4" }.Contains(newfile.FileExtendName.ToLower()))
                        {
                            //结合阿里云转码
                            // string FileUrl = UserInfo.QYinfo.FileServerUrl + "/" + UserInfo.QYinfo.QYCode + "/document/" + newfile.zyid;
                            // AliyunHelp.CopyUrlToOSS(FileUrl, zyid, "mp4");
                        }

                        ListData.Add(newfile);
                    }
                    else
                    {
                        File_Vesion Vseion = new File_Vesion();
                        Vseion.RFileID = File.ID;
                        Vseion.FileSize = File.FileSize;
                        Vseion.CRDate = date;
                        Vseion.CRUser = UserInfo.User.username;
                        new File_VesionB().Insert(Vseion);//加入新版本

                        File.FileVersin = File.FileVersin + 1;
                        File.FileMD5 = md5.Replace("\"", "").Split(',')[0];
                        File.zyid = md5.Split(',').Length == 2 ? md5.Split(',')[1] : md5.Split(',')[0];
                        File.FileSize = item["filesize"].ToString();
                        File.UPDDate = date;
                        File.UPUser = UserInfo.User.username;
                        new File_InfoB().Update(File);//修改新版本
                                                    //修改原版本
                        ListSameData.Add(File);
                    }
                }
                foreach (File_Info item in ListData)
                {
                    new File_InfoB().Insert(item);
                    int filesize = 0;
                    int.TryParse(item.FileSize, out filesize);
                    new File_InfoB().AddSpace(UserInfo.User.username, filesize);
                }
                msg.Result = ListData;
                msg.Result1 = ListSameData;
            }
            catch (Exception ex)
            {
                CommonHelp.WriteLOG("调用上传文件接口出错" + ex.Message.ToString());
            }
        }

        /// <summary>
        /// 获取文件数据
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">文件夹类型</param>
        /// <param name="P2">上级文件夹ID</param>
        /// <param name="UserInfo"></param>
        public void GETLISTDATA(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            //默认找出企业文件夹查看属性为空或者包含当前用户的数据

            string itemtype = context.Request("itemtype") ?? "";//文件或文件夹类型（1:企业文件夹,2:个人文件夹）
            if (itemtype == "1")//企业文件夹
            {
                int FolderID = int.Parse(P1);//
                //默认找出企业文件夹查看属性为空或者包含当前用户的数据
                if (P2 == "Y")//如果是后台查看企业文件夹
                {
                    //msg.Result = new Folder_InfoB().GetEntities(" ComId=" + UserInfo.User.ComId + " AND  PFolderID=" + FolderID);
                    //msg.Result1 = new File_InfoB().GetEntities("ComId=" + UserInfo.User.ComId + " AND  FolderID=" + FolderID);

                    string strSQL = string.Format("SELECT Folder_Info.*,ISNULL(File_UserAuth.AuthUser, '') as AuthUser from Folder_Info LEFT JOIN  File_UserAuth  on Folder_Info.ID= File_UserAuth.RefID  and File_UserAuth.RefType='0' where Folder_Info.ComId='{0}' and Folder_Info.PFolderID='{1}' ", 1, FolderID.ToString());
                    msg.Result = new Folder_InfoB().GetDTByCommand(strSQL);

                    string strSQLFile = string.Format("SELECT File_Info.*,ISNULL(File_UserAuth.AuthUser, '') as AuthUser from File_Info LEFT JOIN  File_UserAuth  on File_Info.ID= File_UserAuth.RefID  and File_UserAuth.RefType='1' where File_Info.ComId='{0}' and File_Info.FolderID='{1}' ", 1, FolderID.ToString());
                    msg.Result1 = new File_InfoB().GetDTByCommand(strSQLFile);
                    return;
                }
                else
                {
                    string strSQL = string.Format("SELECT Folder_Info.*,ISNULL(File_UserAuth.AuthUser, '') as AuthUser from Folder_Info LEFT JOIN  File_UserAuth  on Folder_Info.ID= File_UserAuth.RefID  and File_UserAuth.RefType='0' where Folder_Info.ComId='{0}' and Folder_Info.PFolderID='{1}'   and (  AuthUser is NULL OR  ',' + AuthUser  + ',' like '%,{2},%' )", 1, FolderID.ToString(), UserInfo.User.username);
                    msg.Result = new Folder_InfoB().GetDTByCommand(strSQL);

                    string strSQLFile = string.Format("SELECT File_Info.*,ISNULL(File_UserAuth.AuthUser, '') as AuthUser from File_Info LEFT JOIN  File_UserAuth  on File_Info.ID= File_UserAuth.RefID  and File_UserAuth.RefType='1' where File_Info.ComId='{0}' and File_Info.FolderID='{1}' ", 1, FolderID.ToString());
                    msg.Result1 = new File_InfoB().GetDTByCommand(strSQLFile);
                    return;
                }
            }
            if (itemtype == "2")//个人文件夹
            {
                int FolderID = int.Parse(P1);//
                string strSQL = string.Format("SELECT Folder_Info.*,ISNULL(File_UserAuth.AuthUser, '') as AuthUser from Folder_Info LEFT JOIN  File_UserAuth  on Folder_Info.ID= File_UserAuth.RefID  and File_UserAuth.RefType='0' where Folder_Info.ComId='{0}' and Folder_Info.PFolderID='{1}' and  Folder_Info.CRUser='{2}'", 1, FolderID.ToString(), UserInfo.User.username);
                msg.Result = new Folder_InfoB().GetDTByCommand(strSQL);

                string strSQLFile = string.Format("SELECT File_Info.*,ISNULL(File_UserAuth.AuthUser, '') as AuthUser from File_Info LEFT JOIN  File_UserAuth  on File_Info.ID= File_UserAuth.RefID  and File_UserAuth.RefType='1' where File_Info.ComId='{0}' and File_Info.FolderID='{1}' and  File_Info.CRUser='{2}' ", 1, FolderID.ToString(), UserInfo.User.username);
                msg.Result1 = new File_InfoB().GetDTByCommand(strSQLFile);
                return;
            }
            if (itemtype == "4")//共享文档
            {
                int FolderID = int.Parse(P1);//
                msg.Result = new Folder_InfoB().GetEntities(d => d.PFolderID == FolderID);
                msg.Result1 = new File_InfoB().GetEntities(d => d.FolderID == FolderID);
                return;
            }
            if (itemtype == "3")//我的收藏
            {

                string strWhere = string.Format(SqlHelp.concat("','+CollectUser+','") + "  like '%," + UserInfo.User.username + ",%'");
                string strWhere1 = strWhere;
                if (!string.IsNullOrEmpty(P1))
                {
                    strWhere1 = string.Format("  PFolderID=" + P1);
                }
                string strSQLFLODER = string.Format(@"select * from Folder_Info  where   {0}   order by CRDate desc  ", strWhere1);
                DataTable dtFLODER = new Folder_InfoB().GetDTByCommand(strSQLFLODER);

                string strWhere2 = strWhere;
                if (!string.IsNullOrEmpty(P1))
                {
                    strWhere2 = string.Format("  FolderID=" + P1);
                }
                string strSQLFILE = string.Format(@"select  *  from File_Info  where     {0}   order by CRDate desc  ", strWhere2);
                DataTable dtFILE = new File_InfoB().GetDTByCommand(strSQLFILE);
                msg.Result = dtFLODER;
                msg.Result1 = dtFILE;

                return;
            }
        }

        /// <summary>
        /// 搜索文件
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">文件夹类型</param>
        /// <param name="P2">搜索关键字</param>
        /// <param name="UserInfo"></param>
        public void QUERYFILE(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            msg.Result = new File_InfoB().GetDTByCommand("select File_Info.*,[Folder_Info].FolderType from File_Info left join [Folder_Info] on File_Info.FolderID=[Folder_Info].ID where foldertype='" + P1 + "' " + (P1 == "2" ? "And File_Info.CRUser='" + UserInfo.User.username + "'" : "") + " and( File_Info.Name like '%" + P2 + "%' or File_Info.FileExtendName like '%" + P2 + "%')");
        }

        /// <summary>
        /// 添加外部分享链接
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void ADDSHARECODE(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            string sharepad = DateTime.Now.Minute.ToString() + DateTime.Now.Millisecond.ToString();
            int ID = int.Parse(P1);
            string strType = P2;
            List<File_Share> shareList = new File_ShareB().GetEntities(d => d.RefID == ID && d.CRUser == UserInfo.User.username).ToList();
            File_Share Model = new File_Share();
            if (shareList.Count() > 0)
            {
                if (Model.IsDel != "Y")
                {
                    msg.Result1 = 1;
                }
                Model = shareList.First();
                Model.ShareURL = context.Request("url") + "?ID=" + Model.ID;
                Model.IsDel = "N";
                if (Model.ShareDueDate < DateTime.Now)
                {
                    Model.ShareDueDate = DateTime.Now.AddDays(1);
                }
                new File_ShareB().Update(Model);
                msg.Result = shareList.First();
                return;
            }
            Model.ComId = 0;
            Model.CRDate = DateTime.Now;
            Model.CRUser = UserInfo.User.username;
            Model.CRUserName = UserInfo.User.UserRealName;
            Model.RefID = ID;
            Model.RefType = strType;
            Model.ShareDueDate = DateTime.Now.AddDays(1);//默认当天就过期
            Model.SharePasd = "";
            Model.ShareType = "0";
            Model.ShareURL = "";
            Model.IsDel = "N";
            Model.AuthType = "0";
            new File_ShareB().Insert(Model);
            Model.ShareURL = context.Request("url") + "?ID=" + Model.ID;
            new File_ShareB().Update(Model);
            msg.Result = Model;
        }
        public void GETSHAREINFO(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            int id = 0;
            int.TryParse(P1, out id);
            msg.Result = new File_ShareB().GetEntity(d => d.ID == id);
        }
        /// <summary>
        /// 添加外部分享链接
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void MODIFYJZDATE(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            int ID = int.Parse(P1);
            DateTime newDate = DateTime.Now;
            if (DateTime.TryParse(P2, out newDate))
            {
                File_Share share = new File_ShareB().GetEntity(d => d.ID == ID);
                share.ShareDueDate = newDate;
                new File_ShareB().Update(share);
                msg.Result = share;
            }
            else
            {
                msg.ErrorMsg = "请检查要更新的截止日期格式";
            }

        }
        /// <summary>
        /// 设置密码
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void MODIFYPASSWORD(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            int ID = int.Parse(P1);
            File_Share share = new File_ShareB().GetEntity(d => d.ID == ID);
            share.ShareType = P2;
            string sharecode = GenerateCheckCode(6);
            if (P2 == "0")
            {
                sharecode = "";
            }
            share.SharePasd = sharecode;
            new File_ShareB().Update(share);
            msg.Result = share;


        }
        /// <summary>
        /// 取消分享
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">类型（file:文件,folder:目录）</param>
        /// <param name="P2">ID</param>
        /// <param name="UserInfo"></param>
        public void DELSHARECODE(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            int ID = int.Parse(P1);
            File_Share Model = new File_ShareB().GetEntity(d => d.ID == ID);
            Model.IsDel = "Y";
            new File_ShareB().Update(Model);
        }



        /// <summary>
        /// 收藏目录或文档
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void COLLECTITEM(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            int DataID = int.Parse(P2);
            if (P1 == "file")//
            {
                File_Info Model = new File_InfoB().GetEntity(d => d.ID == DataID);
                if (string.IsNullOrEmpty(Model.CollectUser) || !Model.CollectUser.SplitTOList(',').Contains(UserInfo.User.username))
                {
                    Model.CollectUser = (string.IsNullOrEmpty(Model.CollectUser) ? "" : Model.CollectUser.TrimEnd(',') + ",") + UserInfo.User.username;
                    new File_InfoB().Update(Model);
                }
            }
            else
            {
                Folder_Info Model = new Folder_InfoB().GetEntity(d => d.ID == DataID);
                if (string.IsNullOrEmpty(Model.CollectUser) || !Model.CollectUser.SplitTOList(',').Contains(UserInfo.User.username))
                {
                    Model.CollectUser = (string.IsNullOrEmpty(Model.CollectUser) ? "" : Model.CollectUser.TrimEnd(',') + ",") + UserInfo.User.username;
                    new Folder_InfoB().Update(Model);
                }
            }
        }

        /// <summary>
        /// 取消收藏
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void CANCOLLECTITEM(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            int DataID = int.Parse(P2);

            if (P1 == "file")//
            {
                File_Info Model = new File_InfoB().GetEntity(d => d.ID == DataID);
                if (Model.CollectUser.SplitTOList(',').Contains(UserInfo.User.username))
                {
                    Model.CollectUser = Model.CollectUser.Replace(UserInfo.User.username, ",").Replace(",,", ",").TrimEnd(',');
                    new File_InfoB().Update(Model);
                }
            }
            else
            {
                Folder_Info Model = new Folder_InfoB().GetEntity(d => d.ID == DataID);
                if (Model.CollectUser.SplitTOList(',').Contains(UserInfo.User.username))
                {
                    Model.CollectUser = Model.CollectUser.Replace(UserInfo.User.username, ",").Replace(",,", ",").TrimEnd(',');
                    new Folder_InfoB().Update(Model);
                }
            }
        }

        /// <summary>
        /// 获取文档ITEM
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETWDITEM(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            int ID = int.Parse(P2);
            if (P1 == "file")//
            {
                File_Info Model = new File_InfoB().GetEntity(d => d.ID == ID);
                List<File_Vesion> ListVer = new File_VesionB().GetEntities(D => D.RFileID == Model.ID).ToList();
                msg.Result = Model;
                msg.Result1 = ListVer;

            }
            else
            {
                Folder_Info Model = new Folder_InfoB().GetEntity(d => d.ID == ID);
                msg.Result = Model;
            }
        }

        /// <summary>
        /// 设置权限
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void SETAUTH(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            string strAuthUsers = P1;
            int DataID = int.Parse(P2);
            string RefType = context.Request("REFTYPE") == "file" ? "1" : "0";//默认文件夹
            new File_UserAuthB().Delete(d => d.RefID == DataID && d.RefType == RefType && d.CRUser == UserInfo.User.username);

            File_UserAuth Model = new File_UserAuth();
            Model.ComId = 0;
            Model.CRDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            Model.CRUser = UserInfo.User.username;
            Model.AuthType = "0";//查看权限
            Model.AuthUser = strAuthUsers;
            Model.RefID = DataID;
            Model.RefType = RefType;//文件夹
            new File_UserAuthB().Insert(Model);
        }
        public void CANCELAUTH(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            string RefType = context.Request("REFTYPE") == "file" ? "1" : "0";//默认文件夹
            int DataID = int.Parse(P2);
            new File_UserAuthB().Delete(d => d.RefID == DataID && d.RefType == RefType && d.CRUser == UserInfo.User.username);

        }


        /// <summary>
        ///更具ID获取相应具有内部授权的用户
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETNBSQUSERS(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            string strUserS = "";
            int DataID = int.Parse(P1);
            File_UserAuth MODEL = new File_UserAuthB().GetEntities(d => d.RefID == DataID && d.CRUser == UserInfo.User.username).FirstOrDefault();
            if (MODEL != null)
            {
                strUserS = MODEL.AuthUser;
            }
            msg.Result = strUserS;
        }



        /// <summary>
        /// 获取内部共享来源
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETNBGXLY(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            string strSQL = " SELECT DISTINCT  File_UserAuth.CRUser,user.UserRealName FROM File_UserAuth LEFT JOIN  user ON File_UserAuth.CRUser=user.username  WHERE    " + SqlHelp.concat("','+AuthUser+','") + "   like '%," + UserInfo.User.username + ",%'";
            DataTable dtUserS = new Folder_InfoB().GetDTByCommand(strSQL);
            msg.Result = dtUserS;
        }


        /// <summary>
        /// 获取能够查看的内部人员共享目录
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETNBSHARELIST(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            string strUser = P1;
            string strSQL = "SELECT Folder_Info.* FROM File_UserAuth LEFT JOIN Folder_Info on  File_UserAuth.RefID=Folder_Info.ID WHERE  " + SqlHelp.concat("','+AuthUser+','") + "  like '%," + UserInfo.User.username + ",%' and File_UserAuth.RefType='0'";
            if (strUser != "")
            {
                strSQL = strSQL + "  AND File_UserAuth.CRUser='" + strUser + "'  ";
            }

            string strSQLFile = "SELECT File_Info.* FROM File_UserAuth LEFT JOIN File_Info on  File_UserAuth.RefID=File_Info.ID  WHERE  " + SqlHelp.concat("','+AuthUser+','") + "   like '%," + UserInfo.User.username + ",%' and File_UserAuth.RefType='1'";
            if (strUser != "")
            {
                strSQLFile = strSQLFile + "  AND File_UserAuth.CRUser='" + strUser + "'  ";
            }
            DataTable dtFLODER = new Folder_InfoB().GetDTByCommand(strSQL);
            DataTable dtFile = new File_InfoB().GetDTByCommand(strSQLFile);

            msg.Result = dtFLODER;
            msg.Result1 = dtFile;
        }

        /// <summary>
        /// 向服务器发送压缩目录命令
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">目录ID</param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void COMPRESSFOLDER(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            string strCode = P1;
            Folder_InfoB.FoldFile Mode = new Folder_InfoB.FoldFile();
            Mode.FolderID = -1;
            Mode.Name = "压缩文件";
            Mode.SubFileS = new List<File_Info>();
            Mode.SubFolder = new List<Folder_InfoB.FoldFile>();
            foreach (string item in P1.SplitTOList(','))
            {
                int FileID = int.Parse(item.Split('|')[0].ToString());
                string strType = item.Split('|')[1].ToString();
                if (item.Split('|')[1].ToString() == "file")
                {
                    File_Info file = new File_InfoB().GetEntity(d => d.ID == FileID);
                    file.YLUrl = "";
                    Mode.SubFileS.Add(file);
                }
                else
                {
                    List<Folder_InfoB.FoldFileItem> ListID = new List<Folder_InfoB.FoldFileItem>();
                    Folder_InfoB.FoldFile obj = new Folder_InfoB().GetWDTREE(FileID, ref ListID);
                    Mode.SubFolder.Add(obj);
                }
            }
            IsoDateTimeConverter timeConverter = new IsoDateTimeConverter();
            timeConverter.DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
            string Result = JsonConvert.SerializeObject(Mode, Formatting.Indented, timeConverter).Replace("null", "\"\"");
            string strData = new FileHelp().CompressZip(Result, UserInfo.QyInfo);
            msg.Result = strData;
        }


        #region 获取应用附件列表
        /// <summary>
        /// 获取附件列表
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1">附件Id，多个以逗号隔开</param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETFILESLIST(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {

            int[] fileIds = P1.SplitTOInt(',');
            msg.Result = new File_InfoB().GetEntities(d => fileIds.Contains(d.ID));
        }


        /// <summary>
        /// 获取文件信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETFILEINFO(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            int FileID = int.Parse(P1);//

            File_Info MODEL = new File_InfoB().GetEntity(d => d.ID == FileID);
            msg.Result = MODEL;
            //msg.Result2 = new File_InfoB().GetYLURL(MODEL);
        }
        #endregion

        private int rep = 0;
        private string GenerateCheckCode(int codeCount)
        {
            string str = string.Empty;
            long num2 = DateTime.Now.Ticks + this.rep;
            this.rep++;
            Random random = new Random(((int)(((ulong)num2) & 0xffffffffL)) | ((int)(num2 >> this.rep)));
            for (int i = 0; i < codeCount; i++)
            {
                char ch;
                int num = random.Next();
                if ((num % 2) == 0)
                {
                    ch = (char)(0x30 + ((ushort)(num % 10)));
                }
                else
                {
                    ch = (char)(0x41 + ((ushort)(num % 0x1a)));
                }
                str = str + ch.ToString();
            }
            return str;
        }



        #region 预览页面接口
        /// <summary>
        /// 获取文档资源
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETWDZY(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            Document ff = new DocumentB().GetEntities(p => p.ID == P1).FirstOrDefault();
            if (ff != null)
            {
                msg.Result = ff.FileName;
                msg.Result1 = ff.ylinfo;
            }
            else
            {
                msg.ErrorMsg = "此文件不存在或您没有权限！";
            }
        }

        /// <summary>
        /// 获取页面html(excel)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETHTML(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            string strWDYM = CommonHelp.GetConfig("WDYM");

            File_Info ff = new File_InfoB().GetEntities(p => p.YLCode == P1).FirstOrDefault();
            if (ff != null)
            {
                //定义局部变量
                HttpWebRequest httpWebRequest = null;
                HttpWebResponse httpWebRespones = null;
                Stream stream = null;
                string htmlString = string.Empty;
                string url = strWDYM + ff.YLPath;

                //请求页面
                try
                {
                    httpWebRequest = WebRequest.Create(url + ".html") as HttpWebRequest;
                }
                //处理异常
                catch
                {
                    msg.ErrorMsg = "建立页面请求时发生错误！";
                }
                httpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; .NET CLR 2.0.50727; Maxthon 2.0)";
                //获取服务器的返回信息
                try
                {
                    httpWebRespones = (HttpWebResponse)httpWebRequest.GetResponse();
                    stream = httpWebRespones.GetResponseStream();
                }
                //处理异常
                catch
                {
                    msg.ErrorMsg = "接受服务器返回页面时发生错误！";
                }

                StreamReader streamReader = new StreamReader(stream, System.Text.Encoding.UTF8);
                //读取返回页面
                try
                {
                    htmlString = streamReader.ReadToEnd();
                }
                //处理异常
                catch
                {
                    msg.ErrorMsg = "读取页面数据时发生错误！";
                }
                //释放资源返回结果
                streamReader.Close();
                stream.Close();

                msg.Result = htmlString;
                msg.Result1 = url;

            }
            else
            {
                msg.ErrorMsg = "此文件不存在或您没有权限！";
            }
        }

        #endregion

        #region 判断文件是否转换成功
        public void ISCOV(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            List<File_Info> ListData = JsonConvert.DeserializeObject<List<File_Info>>(P1);
            if (ListData.Count > 0)
            {
                int[] fileId = ListData.Select(d => d.ID).ToList().ListTOString(',').SplitTOInt(',');
                int count = new File_InfoB().GetEntities(d => fileId.Contains(d.ID) && d.FileExtendName == "pdf" && d.YLPath == null).Count();
                if (count == 0)
                {
                    msg.Result = new File_InfoB().GetEntities(d => fileId.Contains(d.ID));
                }
                else
                {
                    msg.ErrorMsg = "转换未完成";
                }
            }
        }
        #endregion


        #region 文档知识库

        public void DELFILE(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            int ID = int.Parse(P1);
            new File_InfoB().Delete(d => d.ID == ID);
        }

        #endregion
        #region 管理员接口

        public void GETSYDATA(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            msg.Result = new QyCodeB().GetAllEntities().Count();
            msg.Result1 = new DocumentB().GetAllEntities().Count();
            msg.Result2 = new DocumentB().GetAllEntities().Sum(d => long.Parse(d.filesize)).ToString();
        }


        /// <summary>
        /// 获取配置信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void GETSETINFO(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            msg.Result = new AppSettingB().GetAllEntities();
        }


        /// <summary>
        /// 保存配置信息
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void SAVESETINFO(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            List<AppSetting> models = JsonConvert.DeserializeObject<List<AppSetting>>(P1);
            new AppSettingB().Update(models);
        }
        public void GETQY(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            msg.Result = new QyCodeB().GetAllEntities();
        }
        public void UPMM(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            User user = UserInfo.User;
            user.pasd = CommonHelp.GetMD5(P2);
            new Auth_UserB().Update(user);
        }


        //重置密码
        public void CZMM(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            User user = new Auth_UserB().GetEntity(d => d.username == P1);
            user.pasd = CommonHelp.GetMD5(CommonHelp.GetConfig("depad", "yumaster"));
            new Auth_UserB().Update(user);
        }



        public void MANGEQY(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            QyCode model = JsonConvert.DeserializeObject<QyCode>(P1);

            if (string.IsNullOrEmpty(model.Name))
            {
                msg.ErrorMsg = "存储空间名称不能为空";
            }
            else
            {
                if (model.ID == 0)
                {
                    QyCode nmodel = new QyCodeB().GetEntity(d => d.Name == model.Name);
                    if (nmodel != null)
                    {
                        msg.ErrorMsg = "存储空间已存在";
                    }
                }
            }
            if (model.ID == 0)
            {
                model.crdate = DateTime.Now;
                model.filecount = 0;
                model.space = "0";
                model.yyspace = "0";
                new QyCodeB().Insert(model);
            }
            else
            {
                new QyCodeB().Update(model);
            }
            msg.Result = model;
        }

        public void DELQY(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            string QYCODE = P1;
            if (!new QyCodeB().Delete(D => D.Code == QYCODE))
            {
                msg.ErrorMsg = "删除失败";
            }
        }

        public void GETFILELIST(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            int page = 0;
            int pagecount = 8;
            int.TryParse(context.Request("p") ?? "1", out page);
            int.TryParse(context.Request("pagecount") ?? "8", out pagecount);//页数
            page = page == 0 ? 1 : page;

            string filename = P1;

            if (!string.IsNullOrEmpty(filename))
            {
                int total = new DocumentB().GetEntities(d => d.FileName.Contains(filename) || d.Qycode.Contains(filename)).Count();
                var files = new DocumentB().GetEntities(d => d.FileName.Contains(filename) || d.Qycode.Contains(filename)).OrderByDescending(d => d.RDate).Take(pagecount * page).Skip(pagecount * (page - 1)).ToList();
                msg.Result = files;
                msg.Result1 = total;
            }
            else
            {
                int total = new DocumentB().GetAllEntities().Count();
                var files = new DocumentB().GetAllEntities().OrderByDescending(d => d.RDate).Take(pagecount * page).Skip(pagecount * (page - 1)).ToList();
                msg.Result = files;
                msg.Result1 = total;

            }
        }


        public void GETLOGLIST(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            int page = 0;
            int pagecount = 8;
            int.TryParse(context.Request("p") ?? "1", out page);
            int.TryParse(context.Request("pagecount") ?? "8", out pagecount);//页数
            page = page == 0 ? 1 : page;

            string logname = P1;

            if (!string.IsNullOrEmpty(logname))
            {
                int total = new UserLogB().GetEntities(d => d.useraction.Contains(logname)).Count();
                var files = new UserLogB().GetEntities(d => d.useraction.Contains(logname)).OrderByDescending(d => d.ID).Take(pagecount * page).Skip(pagecount * (page - 1)).ToList();
                msg.Result = files;
                msg.Result1 = total;
            }
            else
            {
                int total = new UserLogB().GetAllEntities().Count();
                var files = new UserLogB().GetAllEntities().OrderByDescending(d => d.ID).Take(pagecount * page).Skip(pagecount * (page - 1)).ToList();
                msg.Result = files;
                msg.Result1 = total;

            }

        }
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="context"></param>
        /// <param name="msg"></param>
        /// <param name="P1"></param>
        /// <param name="P2"></param>
        /// <param name="UserInfo"></param>
        public void DELWJ(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            string ID = P1;
            Document Model = new DocumentB().GetEntity(D => D.ID == ID);
            string strFile = Model.FullPath;
            if (!new DocumentB().Delete(Model))
            {
                msg.ErrorMsg = "删除失败";
            }
            else
            {
                //存在即删除
                if (System.IO.File.Exists(strFile))
                {
                    System.IO.File.Delete(strFile);
                }
            }
        }
        public void DELRZ(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            new UserLogB().Delete(d => d.ID != 0);
        }
        #endregion

        #region 帮助页面
        public void ADDHMENU(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            HelpData hMenu = JsonConvert.DeserializeObject<HelpData>(P1);
            if (hMenu.ID == 0)
            {
                hMenu.CRDate = DateTime.Now.ToString();
                hMenu.CRUser = UserInfo.User.username;
                hMenu.CRUserName = UserInfo.User.UserRealName;
                new HelpDataB().Insert(hMenu);
            }
            else
            {
                hMenu.CRDate = DateTime.Now.ToString();
                new HelpDataB().Update(hMenu);
            }
            msg.Result = hMenu;
        }

        public void DELGZBGBYID(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            try
            {
                int id = int.Parse(P1);
                hMenu(id);
                new HelpDataB().Delete(d => d.ID == id);

            }
            catch (Exception)
            {
                msg.ErrorMsg = "";
            }
        }

        public void hMenu(long id)
        {
            List<HelpData> hmlist = new HelpDataB().GetEntities(d => d.PID == id).ToList();
            if (hmlist.Count == 0)
                return;
            for (int i = 0; i < hmlist.Count; i++)
            {
                hMenu(hmlist[i].ID);
                new HelpDataB().Delete(d => d.ID == hmlist[i].ID);
            }
        }

        public void GETBZMENU(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            string sql = string.Format("SELECT ID,MenuName,PID,Title,CRDate,CRUserName,MenuChapter FROM HelpData ");

            DataTable dt = new HelpDataB().GetDTByCommand(sql);
            dt.Columns.Add("SubDept", Type.GetType("System.Object"));
            DataTable menu = dt.FilterTable("PID is null OR PID=0 ");
            msg.Result = GetNextWxUser(menu, dt);
        }

        public DataTable GetNextWxUser(DataTable dt, DataTable dtm)
        {
            foreach (DataRow dr in dt.Rows)
            {
                DataTable dtp = dtm.FilterTable(" PID=" + dr["ID"]);
                dr["SubDept"] = GetNextWxUser(dtp, dtm);
            }
            return dt;
        }
        public void GETBZMENUBYID(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            int id = int.Parse(P1);
            HelpData hm = new HelpDataB().GetEntity(d => d.ID == id);
            msg.Result = hm;
            if (hm == null)
                return;
            if (hm.PID != null)
                msg.Result1 = new HelpDataB().GetEntity(d => d.ID == hm.PID);
        }

        public void UPBZ(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            int id = int.Parse(P1);
            HelpData hm = new HelpDataB().GetEntity(d => d.ID == id);
            hm.HelpContent = P2;
            new HelpDataB().Update(hm);
        }

        public void UPFILE(JObject context, Msg_Result msg, string P1, string P2, Auth_UserB.UserInfo UserInfo)
        {
            int id = int.Parse(P1);
            string strFiles = context.Request("files") ?? "";
            HelpData hm = new HelpDataB().GetEntity(d => d.ID == id);
            hm.Files = strFiles;
            new HelpDataB().Update(hm);
        }

        #endregion
    }
}
