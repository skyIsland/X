﻿using System;
using System.Collections.Generic;
using System.Linq;
using NewLife.Net;
using NewLife.Security;

namespace NewLife.Remoting
{
    /// <summary>Api会话</summary>
    [Api(null)]
    public class ApiSession : IApi
    {
        #region 属性
        /// <summary>会话</summary>
        public IApiSession Session { get; set; }
        #endregion

        #region 异常处理
        /// <summary>抛出异常</summary>
        /// <param name="errCode"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        protected ApiException Error(Int32 errCode, String msg) { return new ApiException(errCode, msg); }
        #endregion

        #region 登录
        /// <summary>收到登录请求</summary>
        /// <param name="user">用户名</param>
        /// <param name="pass">密码</param>
        /// <returns></returns>
        [Api("Login")]
        protected virtual Object OnLogin(String user, String pass)
        {
            if (user.IsNullOrEmpty()) throw Error(3, "用户名不能为空");

            WriteLog("登录 {0}/{1}", user, pass);

            // 注册与登录
            var rs = CheckLogin(user, pass);

            // 可能是注册
            var dic = rs.ToDictionary();
            if (dic.ContainsKey(nameof(user))) user = dic[nameof(user)] + "";
            if (dic.ContainsKey(nameof(pass))) pass = dic[nameof(pass)] + "";

            // 用户名保存到会话
            Session["Name"] = user;

            // 生成密钥
            if (!dic.ContainsKey("Key")) dic["Key"] = GenerateKey().ToHex();

            return dic;
        }

        /// <summary>检查登录，返回要发给客户端的对象</summary>
        /// <param name="user"></param>
        /// <param name="pass"></param>
        /// <returns></returns>
        protected virtual Object CheckLogin(String user, String pass) { return null; }

        /// <summary>生成密钥，可继承并加密返回密钥</summary>
        /// <returns></returns>
        protected virtual Byte[] GenerateKey()
        {
            // 随机密钥
            var key = Rand.NextBytes(8);
            Session["Key"] = key;

            WriteLog("生成密钥 {0}", key.ToHex());

            return key;
        }
        #endregion

        #region 心跳
        /// <summary>心跳</summary>
        /// <returns></returns>
        [Api("Ping")]
        protected virtual Object OnPing()
        {
            WriteLog("心跳 ");

            var dic = ControllerContext.Current.Parameters;
            // 返回服务器时间
            dic["ServerTime"] = DateTime.Now;

            return dic;
        }
        #endregion

        #region 辅助
        private String _prefix;

        /// <summary>写日志</summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void WriteLog(String format, params Object[] args)
        {
            var ns = Session as NetSession;
            if (_prefix == null)
            {
                var type = GetType();
                _prefix = "{0}[{1}] ".F(type.GetDisplayName() ?? type.Name.TrimEnd("Session"), ns.ID);
                ns.LogPrefix = _prefix;
            }

            ns.WriteLog(Session["Name"] + " " + format, args);
        }
        #endregion
    }
}