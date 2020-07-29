using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CloudMusicDotNet.Commons.Interfaces
{
    /// <summary>
    /// 消息通知服务
    /// </summary>
    public interface IMsgService
    {
        /// <summary>
        /// 评论消息
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userId">用户id</param>
        /// <returns></returns>
        Task<string> Comments(string data, string userId);

        /// <summary>
        /// @我
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<string> Forwards(string data);

        /// <summary>
        /// 通知消息
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<string> Notices(string data);

        /// <summary>
        /// 私信消息
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<string> Private(string data);

        /// <summary>
        /// 私信内容(登陆后调用此接口 , 可获取私信内容)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<string> PrivateHistory(string data);

    }
}
