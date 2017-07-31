///////////////////////////////////////////////////////
//NSTCPFramework
//版本：1.0.0.1
//////////////////////////////////////////////////////
using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.IO;
using System.Reflection;

namespace GVMetting.Core.Net.Sockets
{
    /// <summary> 
    /// 唯一的标志一个Session,辅助Session对象在Hash表中完成特定功能 
    /// </summary> 
    public class SessionId
    {
        /// <summary> 
        /// 与Session对象的Socket对象的Handle值相同,必须用这个值来初始化它 
        /// </summary> 
        private int _id;

        /// <summary> 
        /// 返回ID值 
        /// </summary> 
        public int ID
        {
            get
            {
                return _id;
            }
        }

        /// <summary> 
        /// 构造函数 
        /// </summary> 
        /// <param name="id">Socket的Handle值</param> 
        public SessionId(int id)
        {
            _id = id;
        }

        /// <summary> 
        /// 重载.为了符合Hashtable键值特征 
        /// </summary> 
        /// <param name="obj"></param> 
        /// <returns></returns> 
        public override bool Equals(object obj)
        {
            if (obj != null)
            {
                SessionId right = (SessionId)obj;

                return _id == right._id;
            }
            else if (this == null)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary> 
        /// 重载.为了符合Hashtable键值特征 
        /// </summary> 
        /// <returns></returns> 
        public override int GetHashCode()
        {
            return _id;
        }

        /// <summary> 
        /// 重载,为了方便显示输出 
        /// </summary> 
        /// <returns></returns> 
        public override string ToString()
        {
            return _id.ToString();
        }

    }

}
