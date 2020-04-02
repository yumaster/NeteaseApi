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
    public class Auth_Qy
    {
        public string QyCode { get; set; }
        public string FileServerUrl { get; set; }


    }

}
