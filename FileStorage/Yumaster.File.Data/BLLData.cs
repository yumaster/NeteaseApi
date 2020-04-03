using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yumaster.File.Data
{
    public class DocumentB : BaseEFDao<Document>
    {

    }
    public class QyCodeB : BaseEFDao<QyCode>
    {

    }
    public class AppSettingB : BaseEFDao<AppSetting>
    {
        public static string GetValueByKey(string strKey, string strDefault = "")
        {
            string strValue = "";
            var keys = new AppSettingB().GetEntities(d => d.Key == strKey).ToList();
            if (keys.Count() > 0)
            {
                strValue = keys[0].Value;
            }
            else
            {
                strValue = strDefault;
            }

            return strValue;
        }
    }
    public class Auth_UserB : BaseEFDao<User>
    {
        /// <summary>
        /// 判断是否具有管理权限
        /// </summary>
        /// <param name="strUser"></param>
        /// <param name="strpasd"></param>
        /// <returns></returns>
        public bool isAuth(string strUser, string strpasd)
        {
            var users = new Auth_UserB().GetEntities(d => d.username == strUser && d.pasd == strpasd);
            return users.Count() == 1;
        }
        public class UserInfo
        {
            public User User;
            public string UserRoleCode;
            public QyCode QyCode;
            public Auth_Qy QyInfo;

        }


        public UserInfo GetUserInfo(string strUser, string strpasd)
        {
            UserInfo UserInfo = new UserInfo(); ;
            var users = new Auth_UserB().GetEntities(d => d.username == strUser && d.pasd == strpasd).ToList();
            if (users.Count() > 0)
            {
                UserInfo.User = users[0];
                UserInfo.QyCode = new QyCodeB().GetAllEntities().FirstOrDefault();
            }
            return UserInfo;
        }
    }
    public class UserLogB : BaseEFDao<UserLog>
    { }
    public class Auth_Qy
    {
        public string QyCode { get; set; }
        public string FileServerUrl { get; set; }
    }
    #region 文档管理模块
    public class Folder_InfoB : BaseEFDao<Folder_Info>
    {
        public FoldFile GetWDTREE(int FolderID, ref List<FoldFileItem> ListID, string strUserName = "")
        {
            List<Folder_Info> ListAll = new Folder_InfoB().GetAllEntities().ToList();
            Folder_Info Folder = new Folder_InfoB().GetEntity(d => d.ID == FolderID);
            Folder_InfoB.FoldFile Model = new Folder_InfoB.FoldFile();
            Model.Name = Folder.Name;
            Model.FolderID = Folder.ID;
            Model.CRUser = Folder.CRUser;
            Model.PFolderID = Folder.PFolderID.Value;
            ListID.Add(new FoldFileItem() { ID = Folder.ID, Type = "folder" });
            if (strUserName != "")
            {
                Model.SubFileS = new File_InfoB().GetEntities(d => d.FolderID == Folder.ID && d.CRUser == strUserName).ToList();
            }
            else
            {
                Model.SubFileS = new File_InfoB().GetEntities(d => d.FolderID == Folder.ID).ToList();
            }
            foreach (var item in Model.SubFileS)
            {
                ListID.Add(new FoldFileItem() { ID = item.ID, Type = "file" });

            }
            Model.SubFolder = new Folder_InfoB().GETNEXTFLODER(Folder.ID, ListAll, ref ListID, strUserName);
            return Model;
        }

        private List<FoldFile> GETNEXTFLODER(int FolderID, List<Folder_Info> ListAll, ref List<FoldFileItem> ListID, string strUserName = "")
        {
            List<FoldFile> ListData = new List<FoldFile>();
            var list = ListAll.Where(d => d.PFolderID == FolderID);
            if (strUserName != "")
            {
                list = list.Where(d => d.CRUser == strUserName);
            }
            foreach (var item in list)
            {
                FoldFile FolderNew = new FoldFile();
                FolderNew.FolderID = item.ID;
                FolderNew.Name = item.Name;
                FolderNew.CRUser = item.CRUser;
                FolderNew.PFolderID = item.PFolderID.Value;
                if (strUserName != "")
                {
                    FolderNew.SubFileS = new File_InfoB().GetEntities(d => d.FolderID == item.ID && d.CRUser == strUserName).ToList();
                }
                else
                {
                    FolderNew.SubFileS = new File_InfoB().GetEntities(d => d.FolderID == item.ID).ToList();
                }
                foreach (var SubFile in FolderNew.SubFileS)
                {
                    ListID.Add(new FoldFileItem() { ID = SubFile.ID, Type = "file" });
                }
                FolderNew.SubFolder = GETNEXTFLODER(item.ID, ListAll, ref ListID, strUserName);
                ListData.Add(FolderNew);
                ListID.Add(new FoldFileItem() { ID = item.ID, Type = "folder" });
            }
            return ListData;

        }



        /// <summary>
        /// 获取指定文件夹下得所有文件夹
        /// </summary>
        /// <param name="FolderID"></param>
        /// <returns></returns>
        public List<Folder_Info> GetChiFolder(int FolderID)
        {
            string strQuery = FolderID.ToString() + "-";
            return new Folder_InfoB().GetEntities(d => d.Remark.Contains(strQuery)).ToList();
        }

        /// <summary>
        /// 复制树状结构
        /// </summary>
        /// <param name="FloderID"></param>
        /// <param name="PID"></param>
        public void CopyFloderTree(int FloderID, int PID)
        {
            List<FoldFileItem> ListID = new List<FoldFileItem>();
            FoldFile Model = new Folder_InfoB().GetWDTREE(FloderID, ref ListID);
            Folder_Info Folder = new Folder_InfoB().GetEntity(d => d.ID == Model.FolderID);
            Folder.PFolderID = PID;
            new Folder_InfoB().Insert(Folder);

            //更新文件夹路径Code
            Folder_Info PFolder = new Folder_InfoB().GetEntity(d => d.ID == PID);
            Folder.Remark = PFolder.Remark + "-" + Folder.ID;
            new Folder_InfoB().Update(Folder);

            foreach (File_Info file in Model.SubFileS)
            {
                file.FolderID = Folder.ID;
                new File_InfoB().Insert(file);
            }
            GreateWDTree(Model.SubFolder, Folder.ID);
        }

        /// <summary>
        /// 根据父ID创建树装结构文档
        /// </summary>
        /// <param name="ListFoldFile"></param>
        private void GreateWDTree(List<FoldFile> ListFoldFile, int newfolderid)
        {

            foreach (FoldFile item in ListFoldFile)
            {

                Folder_Info PModel = new Folder_InfoB().GetEntity(d => d.ID == item.FolderID);
                PModel.PFolderID = newfolderid;
                new Folder_InfoB().Insert(PModel);

                //更新文件夹路径Code
                Folder_Info PFolder = new Folder_InfoB().GetEntity(d => d.ID == newfolderid);
                PModel.Remark = PFolder.Remark + "-" + PModel.ID;
                new Folder_InfoB().Update(PModel);

                foreach (File_Info file in item.SubFileS)
                {
                    file.FolderID = PModel.ID;
                    new File_InfoB().Insert(file);
                }

                GreateWDTree(item.SubFolder, PModel.ID);
            }
        }
        public void DelWDTree(int FolderID)
        {
            List<FoldFileItem> ListID = new List<FoldFileItem>();
            new Folder_InfoB().GetWDTREE(FolderID, ref ListID);
            foreach (FoldFileItem listitem in ListID)
            {
                if (listitem.Type == "file")
                {
                    new File_InfoB().Delete(d => d.ID == listitem.ID);
                }
                else
                {
                    new Folder_InfoB().Delete(d => d.ID == listitem.ID);
                }
            }
        }
        public class FoldFile
        {
            public int FolderID { get; set; }
            public string Name { get; set; }
            public string CRUser { get; set; }
            public int PFolderID { get; set; }
            public string Remark { get; set; }

            public List<FoldFile> SubFolder { get; set; }
            public List<File_Info> SubFileS { get; set; }

        }
        public class FoldFileItem
        {
            public int ID { get; set; }
            public string Type { get; set; }
        }
    }
    public class File_InfoB : BaseEFDao<File_Info>
    {
        public void AddVersion(File_Info oldmodel, string strMD5, string strSIZE)
        {
            File_Vesion Vseion = new File_Vesion();
            Vseion.FileMD5 = oldmodel.FileMD5;
            Vseion.RFileID = oldmodel.ID;
            new File_VesionB().Insert(Vseion);
            //添加新版本

            oldmodel.FileVersin = oldmodel.FileVersin + 1;
            oldmodel.FileMD5 = strMD5;
            oldmodel.FileSize = strSIZE;
            new File_InfoB().Update(oldmodel);
            //修改原版本

        }

        public List<File_Info> getDT(string strWhere, int page, int pagecount, ref int total)
        {
            var dt = new File_InfoB().Db.Queryable<File_Info>().Where(strWhere).OrderBy(it => it.CRDate, OrderByType.Desc).ToPageList(page, pagecount, ref total);
            return dt;

        }

        /// <summary>
        /// 判断同一目录下是否有相同文件(不判断应用文件夹)
        /// </summary>
        /// <param name="strMD5"></param>
        /// <param name="strFileName"></param>
        /// <param name="FolderID"></param>
        /// <returns></returns>
        public File_Info GetSameFile(string strFileName, string strkzname, int FolderID)
        {
            int[] folders = { 1, 2, 3 };
            if (!folders.Contains(FolderID))
            {
                return new File_InfoB().GetEntities(d => d.Name == strFileName && d.FileExtendName == strkzname && d.FolderID == FolderID).FirstOrDefault();
            }
            return null;

        }


        /// <summary>
        /// 更新企业空间占用
        /// </summary>
        /// <param name="FileSize"></param>
        /// <returns></returns>
        public int AddSpace(string strUserName, int FileSize)
        {
            User qymodel = new Auth_UserB().GetEntity(d => d.username == strUserName);
            if (qymodel != null)
            {
                qymodel.Space = qymodel.Space + FileSize;
            }
            new Auth_UserB().Update(qymodel);
            return int.Parse(qymodel.Space.ToString());
        }
    }
    public class File_DownhistoryB : BaseEFDao<File_Downhistory> { }
    public class File_ShareB : BaseEFDao<File_Share> { }
    public class File_UserAuthB : BaseEFDao<File_UserAuth> { }
    public class File_UserTagB : BaseEFDao<File_UserTag> { }
    public class File_VesionB : BaseEFDao<File_Vesion> { }
    public class HelpDataB : BaseEFDao<HelpDataB> { }
    #endregion
}
