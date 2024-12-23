using BackendSite.Service.BLL.Infrastructure;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace BackendSite.Service.Library
{
    public class Tools
    {
        private readonly string _backEndKey;

        public Tools(IServiceProvider provider)
        {
            var configBll = provider.GetRequiredService<ConfigBLL>();
            _backEndKey = configBll.GetConfigValue("ApiPaymentGateWayKey");
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="pass"></param>
        /// <returns></returns>
        public string MD5Encrypt(string pass)
        {
            var md5Hasher = new MD5CryptoServiceProvider();
            var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(pass));
            var sBuilder = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
        /// <summary>
        /// 验证用户名 必须是字母和数字的组合
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public bool CheckUserName(string userName)
        {
            var regex = new Regex("^(?!^\\d+$)(?!^[a-zA-Z]+$)[0-9a-zA-Z]{6,18}$", RegexOptions.Singleline);
            var mPwd = regex.Match(userName);
            if (userName.Length < 6 || userName.Length > 18)
                return false;
            return mPwd.Success;
        }
        /// <summary>
        /// 验证用户密码："^[a-zA-Z]\w{5,17}$"正确格式为：以字母开头，长度在6~18之间，只能包含字符、数字和下划线。
        /// </summary>
        /// <param name="userPwd"></param>
        /// <returns></returns>
        public bool CheckUserInputPassword(string userPwd)
        {
            var regex = new Regex("^[A-Za-z0-9]+$", RegexOptions.Singleline);
            var mPwd = regex.Match(userPwd);
            if (userPwd.Length < 6 || userPwd.Length > 18)
                return false;
            return mPwd.Success;
        }

        #region GetClientIP
        /// <summary>
        /// 获取客户端IP
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string GetClientIP(HttpContext context)
        {
            var HeaderNames = new string[] { "HTTP_X_FORWARDED_FOR", "HTTP_INCAP_CLIENT_IP", "HTTP_TRUE_CLIENT_IP", "HTTP_X_REAL_IP", "REMOTE_ADDR", "X-Forwarded-For" };

            foreach (var Header in HeaderNames)
            {

                if (context.Request.Headers.ContainsKey(Header))
                {
                    string sServerVariables = context.Request.Headers[Header];
                    var ip = GetIPAddress(sServerVariables.Split(','));
                    if (!string.IsNullOrEmpty(ip))
                    {
                        return ip;
                    }
                }
            }

            return context.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }

        private string GetIPAddress(string[] IPArry)
        {
            foreach (var ip in IPArry)
            {
                var IPAddress = ip.Trim();
                if (IsIPAddress(IPAddress)
                    && !isInnerIP(IPAddress)) return IPAddress.Trim();
            }

            return null;
        }
        private bool isInnerIP(string ipAddress)
        {
            var isInnerIp = false;
            var ipNum = getIpNum(ipAddress);

            long aBegin = 167772160;// getIpNum("10.0.0.0");
            long aEnd = 184549375;// getIpNum("10.255.255.255");
            long bBegin = 2886729728; //getIpNum("172.16.0.0");
            long bEnd = 2887778303; //getIpNum("172.31.255.255");
            long cBegin = 3232235520; //getIpNum("192.168.0.0");
            long cEnd = 3232301055; //getIpNum("192.168.255.255");

            // Logger.Log("aBegin:" + aBegin + ",aEnd:" + aEnd + ",bBegin:" + bBegin + ",bEnd:" + bEnd + ",cBegin:" + cBegin + ",cEnd:" + cEnd);

            isInnerIp = isInner(ipNum, aBegin, aEnd) || isInner(ipNum, bBegin, bEnd) || isInner(ipNum, cBegin, cEnd) || ipAddress == "127.0.0.1";
            return isInnerIp;
        }
        public long getIpNum(string ipAddress)
        {
            var ip = ipAddress.Split('.');
            long a = Convert.ToInt32(ip[0]);
            long b = Convert.ToInt32(ip[1]);
            long c = Convert.ToInt32(ip[2]);
            long d = Convert.ToInt32(ip[3]);

            var ipNum = a * 256 * 256 * 256 + b * 256 * 256 + c * 256 + d;
            return ipNum;
        }
        private bool IsIPAddress(string str1)
        {
            if (string.IsNullOrEmpty(str1) || str1.Length < 7 || str1.Length > 15) return false;
            const string regformat = @"^\d{1,3}[\.]\d{1,3}[\.]\d{1,3}[\.]\d{1,3}$";

            var regex = new Regex(regformat, RegexOptions.IgnoreCase);
            return regex.IsMatch(str1);

        }
        private bool isInner(long userIp, long begin, long end)
        {
            return (userIp >= begin) && (userIp <= end);
        }
        #endregion
        public string GetFromUrl(HttpContext context, bool isCheckReferer = true)
        {
            var fromUrl = "";
            if (isCheckReferer && context.Request.Headers.ContainsKey("Referer"))
            {
                fromUrl = context.Request.Headers["Referer"].ToString();
            }

            if (string.IsNullOrWhiteSpace(fromUrl))
                fromUrl = context.Request.Host.ToString();

            return fromUrl;
        }

        /// <summary>
        /// 保留两位小数的金额
        /// </summary>
        /// <param name="d"></param>
        /// <param name="decimals"></param>
        /// <returns></returns>
        public decimal Floor(decimal d, int decimals)
        {
            var value = (long)Math.Pow(10.0, decimals);
            return Convert.ToDecimal(Math.Floor(d * (decimal)value) / (decimal)value);
        }
        /// <summary>
        /// 余额
        /// </summary>
        /// <param name="dBalance"></param>
        /// <returns></returns>
        public string FmtBalance(decimal dBalance)
        {
            return Floor(dBalance, 2).ToString("#,##0.#0");
        }

        /// <summary>
        /// 字串轉日期格式&取時區
        /// </summary>
        /// <param name="paramDateTime">參數日期(格式=yyyy-mm-ddThh:MM:ss+TT:TT)</param>
        /// <param name="newTimeZone">希望轉換的時區(TimeSpan)</param>
        /// <param name="newDateTimeOffset">轉換後的日期(DateTimeOffset)</param>
        /// <param name="paramTimeZone">參數的時區(TimeSpan)</param>
        /// <returns></returns>
        public bool StringConvertDateTimeOffset(string paramDateTime,
            out DateTimeOffset newDateTimeOffset,
            out TimeSpan paramTimeZone,
            TimeSpan? newTimeZone = null)
        {
            newDateTimeOffset = DateTimeOffset.MinValue;
            paramTimeZone = TimeSpan.Zero;
            try
            {
                if (string.IsNullOrEmpty(paramDateTime))
                {
                    return false;
                }
                newDateTimeOffset = DateTimeOffset.Parse(paramDateTime);
                paramTimeZone = newDateTimeOffset.Offset;
                newDateTimeOffset = new DateTimeOffset(newDateTimeOffset.ToUniversalTime().DateTime, newTimeZone ?? TimeSpan.Zero);

                return true;
            }
            catch (FormatException ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 電話遮罩
        /// </summary>
        /// <param name="phone">電話</param>
        /// <returns>遮罩處理結果</returns>
        public string MaskPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone))
                return phone;

            return new string('*', phone.Length);
        }

        /// <summary>
        /// EMail遮罩
        /// </summary>
        /// <param name="email">Email</param>
        /// <returns>遮罩處理結果</returns>
        public string MaskEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return email;

            var pattern = @"^[^@]+(?=@)";
            return Regex.Replace(email, pattern, m => new string('*', m.Length));
        }

        /// <summary>
        /// LineUserId遮罩
        /// </summary>
        /// <param name="data">Email</param>
        /// <returns>遮罩處理結果</returns>
        public string MaskLineId(string data)
        {
            if (string.IsNullOrEmpty(data))
                return data;

            var pattern = @"(?<=[\w]{1})[\w-\._\+%]*(?=[\w]{1})";
            return Regex.Replace(data, pattern, m => new string('*', m.Length));
        }

        /// <summary>
        /// BackendApiTokenCheck
        /// </summary>
        /// <returns>後台api token檢查</returns>
        public bool BackendTokenCheck(object para, string token)
        {
            if (token == MD5Encrypt($"{para}{_backEndKey}"))
            {
                return true;
            }

            return false;
        }
    }

}
