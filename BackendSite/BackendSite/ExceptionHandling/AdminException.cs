using BackendSite.Service.BLL;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace BackendSite.ExceptionHandling
{
    [Serializable()]
    public class AdminException : Exception, ISerializable
    {

        public ErrorCodeEnum ErrorCode { get; set; }
        public string Lang { get; set; } = "en";
        private string _Message { get; set; } = string.Empty;


        public string Message
        {
            get
            {
                    return _Message;
            }
            set
            {
                _Message = value;
            }
        }

        public AdminException()
        {
        }

        public AdminException(string message)
        : base(message)
        {
        }

        public AdminException(ErrorCodeEnum ErrorCode, 
                              string Message = "", 
                              string Lang = "en")
        {
            this.ErrorCode = ErrorCode;
            this.Lang = Lang;
            this._Message = Message;

        }

        public AdminException(string message, Exception inner)
        : base(message, inner)
        {
        }
    }
}
