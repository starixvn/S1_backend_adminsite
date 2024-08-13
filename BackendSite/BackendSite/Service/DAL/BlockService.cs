using System.Data;

using BackendSite.Extensions;
using BackendSite.Service.Library;
using BackendSite.Service.Model;
using BackendSite.Service.Model.enums;

using Dapper;

using Newtonsoft.Json;

namespace BackendSite.Service.DAL
{
    public class BlockService
    {
        private readonly IConfiguration _configuration;
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;
        private readonly MySqlClient _ipTableClient;
        private readonly CustInfoService _custInfoService;
        public BlockService(IServiceProvider provider)
        {
            _configuration = provider.GetRequiredService<IConfiguration>();
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
            _ipTableClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.IP);
            _custInfoService = provider.GetRequiredService<CustInfoService>();
        }

        public async Task<IEnumerable<T>> GetBlockIPList<T>()
        {
            var p = new DynamicParameters();
            p.Add("IP", null);
            return await _mainDbClient.QueryAsync<T>("Admin_BlockIP_Get", p);
        }

        public async Task<BlockIPPageData> GetBlockIPList(string ip, DateTime startDate, DateTime endDate, int pageNumber, int pageSize)
        {
            var p = new DynamicParameters(new
            {
                IP = string.IsNullOrWhiteSpace(ip) ? null : ip,
                StartDate = startDate,
                EndDate = endDate,
                StartIndex = Math.Abs(pageSize) * (Math.Abs(pageNumber) - 1),
                PageSize = pageSize,
            });

            p.Add("TotalPages", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("RowCount", dbType: DbType.Int32, direction: ParameterDirection.Output);

            var result = await _reportDbClient.QueryAsync<BlockIP>("Admin_BlockIPList_Get", p);
            var blockIpPageData = new BlockIPPageData()
            {
                BlockIP = result,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = p.Get<int?>("TotalPages") ?? 0,
                RowCount = p.Get<int?>("RowCount") ?? 0
            };

            return blockIpPageData;
        }

        public int SetBlockIP(ParamsWithLog<SetBlockIPRequest> input)
        {
            var result = 0;
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            var para = new DynamicParameters(new
            {
                IP = input.After.IP
            });

            #region propsLog

            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.SetBlockIP);
            logParam.Add("_ActionType", (int)ActionType.Create);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", null, DbType.String);
            logParam.Add("_AfterDetailInfo", input.After.ToLog());
            logParam.Add("_Operator", input.Operator);

            #endregion

            var data = _reportDbClient.Query<BlockIP>("Admin_BlockIP_Get", para).ToList();

            if (data.Count == 0)
            {
                var p = new DynamicParameters(new
                {
                    IP = input.After.IP,
                    Memo = input.After.Memo,
                    Operator = input.Operator
                });
                result = _mainDbClient.Execute("Admin_BlockIP_Set", trans, p);
                if (result > 0)
                {
                    var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, logParam) > 0;
                    if (!logResult)
                    {
                        _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                        return 0;
                    }
                }
                else
                {
                    _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                    return result;
                }
            }
            else
            {
                return result;
            }
            _mainDbClient.CommitAndDisposeConnection(conn, trans);
            return result;
        }

        public int DeleteBlockIP(ParamsWithLog<BlockIPRequest> input)
        {
            var result = 0;
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            var p = new DynamicParameters(new
            {
                IP = input.Before.IP
            });

            #region propsLog

            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.DeleteBlockIP);
            logParam.Add("_ActionType", (int)ActionType.Delete);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", input.Before.ToLog());
            logParam.Add("_AfterDetailInfo", null, DbType.String);
            logParam.Add("_Operator", input.Operator);

            #endregion

            result = _mainDbClient.Execute("Admin_BlockIP_Del", trans, p);

            if (result > 0)
            {
                var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, logParam) > 0;
                if (!logResult)
                {
                    _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                    return 0;
                }
            }
            else
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                return result;
            }
            _mainDbClient.CommitAndDisposeConnection(conn, trans);
            return result;
        }

        public IEnumerable<BlockBankAcc> GetCustBlockBankAcc(DateTime startDate, DateTime endDate, int blockStatus, string accountNum, IEnumerable<int> currencyId, string userName)
        {
            var jsonSelectStatus = string.Empty;
            if (blockStatus == 0)
            {
                var tempList = new List<int>() { (int)BlockAccountStatus.Lock, (int)BlockAccountStatus.UnLock };
                jsonSelectStatus = JsonConvert.SerializeObject(tempList);
            }
            else if (blockStatus == 1)
            {
                var tempList = new List<int>() { (int)BlockAccountStatus.Lock };
                jsonSelectStatus = JsonConvert.SerializeObject(tempList);
            }
            else
            {
                var tempList = new List<int>() { (int)BlockAccountStatus.UnLock };
                jsonSelectStatus = JsonConvert.SerializeObject(tempList);
            }

            var p = new DynamicParameters(new
            {
                StartDate = startDate,
                EndDate = endDate,
                AccountNum = string.IsNullOrWhiteSpace(accountNum) ? null : accountNum,
                UserName = string.IsNullOrWhiteSpace(userName) ? null : userName,
                SelectStatus = jsonSelectStatus,
                SelectCurrency = JsonConvert.SerializeObject(currencyId)
            });

            return _reportDbClient.Query<BlockBankAcc>("Admin_BlockBankAcc_Get", p);
        }

        public IEnumerable<BlockList> GetBlockBankAccList(int sysId, string accountNum)
        {
            var p = new DynamicParameters(new
            {
                SysId = sysId,
                AccountNum = accountNum
            });

            return _reportDbClient.Query<BlockList>("Admin_BlockBankAccList_Get", p);
        }

        public IEnumerable<IPUser> GetIPUserList(string ip)
        {
            var p = new DynamicParameters(new
            {
                LoginIp = ip
            });

            return _reportDbClient.Query<IPUser>("Admin_IPUserList_Get", p);
        }

        public async Task<IEnumerable<DeviceUser>> GetIoavationAssociatedList(string deviceId)
        {
            var p = new DynamicParameters(new { _DeviceId = deviceId });
            return await _reportDbClient.QueryAsync<DeviceUser>("Admin_IoavationAssociatedList_Get", p);
        }

        public async Task<IEnumerable<DeviceUser>> GetShieldAssociatedList(string deviceId)
        {
            var p = new DynamicParameters(new { _DeviceId = deviceId });
            return await _reportDbClient.QueryAsync<DeviceUser>("Admin_ShieldAssociatedList_Get", p);
        }
        
        public async Task<IEnumerable<DeviceUser>> GetFPSAssociatedList(string deviceId)
        {
            var p = new DynamicParameters(new { _DeviceId = deviceId });
            return await _reportDbClient.QueryAsync<DeviceUser>("Admin_FPSAssociatedList_Get", p);
        }

        public int BlockAccountSet(ParamsWithLog<BlockAccountSetRequest> input)
        {
            var result = 0;
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            var p = new DynamicParameters(new
            {
                _SysId = input.After.SysId,
                _AccountNum = input.After.AccountNum,
                _Status = (int)input.After.Status,
                _UpdateOperator = input.Operator,
                _Memo = input.After.Memo
            });

            #region propsLog

            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.SetBlockAccount);
            logParam.Add("_ActionType", (int)ActionType.Update);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", input.Before.ToLog());
            logParam.Add("_AfterDetailInfo", input.After.ToLog());
            logParam.Add("_Operator", input.Operator);

            #endregion

            result = _mainDbClient.Execute("Admin_BlockAccount_Set", trans, p);

            if (result > 0)
            {
                var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, logParam) > 0;
                if (!logResult)
                {
                    _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                    return 0;
                }

                var blockBankAccBlockList = GetBlockBankAccList(input.After.SysId, input.After.AccountNum).ToList();

                foreach (var x in blockBankAccBlockList)
                {
                    var parameter = new DynamicParameters(new
                    {
                        CustId = x.CustId,
                        SysId = input.After.SysId,
                        AccountNo = input.After.AccountNum,
                        IFSCCode = x.IFSCCode,
                        Status = input.After.Status == BlockAccountStatus.Lock ? (int)CustBankDetailStatus.Disable : (int)CustBankDetailStatus.BODelete
                    });

                    result = _mainDbClient.Execute("Member_CustBankDetail_Set", trans, parameter);

                    if (result > 0)
                    {
                        var prop = new DynamicParameters(new
                        {
                            CustId = x.CustId,
                            SiteId = x.SiteId,
                            SysId = input.After.SysId,
                            AccountNum = input.After.AccountNum,
                            IFSCCode = string.IsNullOrWhiteSpace(x.IFSCCode) ? "" : x.IFSCCode,
                            AccountName = x.AccountName,
                            Status = input.After.Status == BlockAccountStatus.Lock ? CustBankInfoLogStatus.Lock : CustBankInfoLogStatus.Unlock,
                            Operator = input.Operator
                        });

                        result = _mainDbClient.Execute("Service_CustBankInfoLog_Set_V3", trans, prop);
                        if (result == 0)
                        {
                            _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                            return result;
                        }
                    }
                    else
                    {
                        _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                        return result;
                    }
                }
                _mainDbClient.CommitAndDisposeConnection(conn, trans);
            }
            else
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
            }

            return result;
        }

        public int ReleaseAccount(ParamsWithLog<ReleaseAccountRequest> input)
        {
            var result = 0;
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            var p = new DynamicParameters(new
            {
                CustId = input.Before.CustId,
                SysId = input.Before.SysId,
                AccountNo = input.Before.AccountNum
            });

            #region propsLog

            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.ReleaseAccount);
            logParam.Add("_ActionType", (int)ActionType.Delete);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", input.Before.ToLog());
            logParam.Add("_AfterDetailInfo", null, DbType.String);
            logParam.Add("_Operator", input.Operator);

            #endregion

            result = _mainDbClient.Execute("Admin_CustBankDetail_Del", trans, p);

            if (result > 0)
            {
                var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, logParam) > 0;
                if (!logResult)
                {
                    _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                    return 0;
                }

                var custInfo = _custInfoService.GetCustInfo(input.Before.CustId);

                if (custInfo != null)
                {
                    var prop = new DynamicParameters(new
                    {
                        CustId = input.Before.CustId,
                        SiteId = custInfo.SiteId,
                        SysId = input.Before.SysId,
                        AccountNum = input.Before.AccountNum,
                        IFSCCode = string.IsNullOrWhiteSpace(input.Before.IFSCCode) ? "" : input.Before.IFSCCode,
                        AccountName = input.Before.CustId,
                        Status = CustBankInfoLogStatus.Release,
                        Operator = input.Operator
                    });

                    result = _mainDbClient.Execute("Service_CustBankInfoLog_Set_V3", trans, prop);

                    if (result <= 0)
                    {
                        _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                    }
                }
                else
                {
                    _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                }

            }
            else
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
            }
            _mainDbClient.CommitAndDisposeConnection(conn, trans);
            return result;
        }

        public IEnumerable<BlockBankAcc> GetBlockAccount(DateTime startDate, DateTime endDate, int blockStatus, string accountNum, IEnumerable<int> currencyId)
        {
            var jsonSelectStatus = string.Empty;
            if (blockStatus == 0)
            {
                var tempList = new List<int>() { (int)BlockAccountStatus.Lock, (int)BlockAccountStatus.UnLock };
                jsonSelectStatus = JsonConvert.SerializeObject(tempList);
            }
            else if (blockStatus == 1)
            {
                var tempList = new List<int>() { (int)BlockAccountStatus.Lock };
                jsonSelectStatus = JsonConvert.SerializeObject(tempList);
            }
            else
            {
                var tempList = new List<int>() { (int)BlockAccountStatus.UnLock };
                jsonSelectStatus = JsonConvert.SerializeObject(tempList);
            }

            var p = new DynamicParameters(new
            {
                StartDate = startDate,
                EndDate = endDate,
                AccountNum = string.IsNullOrWhiteSpace(accountNum) ? null : accountNum,
                SelectStatus = jsonSelectStatus,
                SelectCurrency = JsonConvert.SerializeObject(currencyId)
            });

            return _reportDbClient.Query<BlockBankAcc>("Admin_BlockAccount_Get", p);
        }

        public IEnumerable<BlockCryptoWallet> GetBlockCryptoWallet(DateTime startDate, DateTime endDate, int blockStatus, string address, IEnumerable<int> currencyId, string userName, IEnumerable<int> cryptoType, IEnumerable<string> netWork, int siteId)
        {
            var jsonSelectStatus = string.Empty;
            if (blockStatus == 0)
            {
                var tempList = new List<int>() { (int)BlockAccountStatus.Lock, (int)BlockAccountStatus.UnLock };
                jsonSelectStatus = JsonConvert.SerializeObject(tempList);
            }
            else if (blockStatus == 1)
            {
                var tempList = new List<int>() { (int)BlockAccountStatus.Lock };
                jsonSelectStatus = JsonConvert.SerializeObject(tempList);
            }
            else
            {
                var tempList = new List<int>() { (int)BlockAccountStatus.UnLock };
                jsonSelectStatus = JsonConvert.SerializeObject(tempList);
            }

            var p = new DynamicParameters(new
            {
                StartDate = startDate,
                EndDate = endDate,
                Address = string.IsNullOrWhiteSpace(address) ? null : address,
                UserName = string.IsNullOrWhiteSpace(userName) ? null : userName,
                SelectStatus = jsonSelectStatus,
                SelectCurrency = JsonConvert.SerializeObject(currencyId),
                SelectCrypto = JsonConvert.SerializeObject(cryptoType),
                SelectNetWork = JsonConvert.SerializeObject(netWork),
                SiteId = siteId
            });

            return _reportDbClient.Query<BlockCryptoWallet>("Admin_BlockCryptoWallet_Get", p);
        }

        public IEnumerable<BlockCryptoWallet> GetBlockCryptoAddress(DateTime startDate, DateTime endDate, int blockStatus, string address)
        {
            var jsonSelectStatus = string.Empty;
            if (blockStatus == 0)
            {
                var tempList = new List<int>() { (int)BlockAccountStatus.Lock, (int)BlockAccountStatus.UnLock };
                jsonSelectStatus = JsonConvert.SerializeObject(tempList);
            }
            else if (blockStatus == 1)
            {
                var tempList = new List<int>() { (int)BlockAccountStatus.Lock };
                jsonSelectStatus = JsonConvert.SerializeObject(tempList);
            }
            else
            {
                var tempList = new List<int>() { (int)BlockAccountStatus.UnLock };
                jsonSelectStatus = JsonConvert.SerializeObject(tempList);
            }

            var p = new DynamicParameters(new
            {
                StartDate = startDate,
                EndDate = endDate,
                Address = string.IsNullOrWhiteSpace(address) ? null : address,
                SelectStatus = jsonSelectStatus
            });

            return _reportDbClient.Query<BlockCryptoWallet>("Admin_BlockCryptoAddress_Get", p);
        }

        public IEnumerable<BlockCryptoWalletList> GetBlockCryptoWalletList(string address)
        {
            var p = new DynamicParameters(new
            {
                Address = address
            });

            return _reportDbClient.Query<BlockCryptoWalletList>("Admin_BlockCryptoWalletList_Get", p);
        }

        public int BlockCryptoWalletSet(ParamsWithLog<BlockCryptoWalletSetRequest> input)
        {
            var result = 0;
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            var p = new DynamicParameters(new
            {
                _Address = input.After.Address,
                _Status = (int)input.After.Status,
                _UpdateOperator = input.Operator,
                _Memo = input.After.Memo
            });

            #region propsLog

            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.SetBlockCryptoWallet);
            logParam.Add("_ActionType", (int)ActionType.Update);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", input.Before.ToLog());
            logParam.Add("_AfterDetailInfo", input.After.ToLog());
            logParam.Add("_Operator", input.Operator);

            #endregion

            result = _mainDbClient.Execute("Admin_BlockCryptoWallet_Set", trans, p);

            if (result > 0)
            {
                var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, logParam) > 0;
                if (!logResult)
                {
                    _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                    return 0;
                }

                var blockCryptoWalletList = GetBlockCryptoWalletList(input.After.Address).ToList();

                foreach (var x in blockCryptoWalletList)
                {
                    var parameter = new DynamicParameters(new
                    {
                        CustId = x.CustId,
                        Address = input.After.Address,
                        Status = input.After.Status == BlockCryptoWalletStatus.Lock ? (int)CustBankDetailStatus.Disable : (int)CustBankDetailStatus.BODelete
                    });

                    result = _mainDbClient.Execute("Admin_CustCryptoWalletDetail_Set", trans, parameter);

                    if (result > 0)
                    {
                        var prop = new DynamicParameters(new
                        {
                            CustId = x.CustId,
                            SiteId = x.SiteId,
                            AccountName = x.AccountName,
                            Address = input.After.Address,
                            NickName = x.NickName,
                            Network = x.Network,
                            CurrencyId = x.CurrencyId,
                            Status = input.After.Status == BlockCryptoWalletStatus.Lock ? CustUSDTInfoLogStatus.Lock : CustUSDTInfoLogStatus.Unlock,
                            Operator = input.Operator
                        });

                        result = _mainDbClient.Execute("Service_CustUSDTInfoLog_Set_V3", trans, prop);
                        if (result == 0)
                        {
                            _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                            return result;
                        }
                    }
                    else
                    {
                        _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                        return result;
                    }
                }
                _mainDbClient.CommitAndDisposeConnection(conn, trans);
            }
            else
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
            }

            return result;
        }

        public int ReleaseCryptoWallet(ParamsWithLog<ReleaseCryptoWalletRequest> input)
        {
            var result = 0;
            using var conn = _mainDbClient.GetMySqlConnection();
            conn.Open();
            using var trans = conn.BeginTransaction();

            var p = new DynamicParameters(new
            {
                CustId = input.Before.CustId,
                Address = input.Before.Address
            });

            #region propsLog

            var logParam = new DynamicParameters();
            logParam.Add("_SiteId", input.SiteId);
            logParam.Add("_CurrencyId", input.CurrencyId);
            logParam.Add("_Menu", (int)AdminAdjustmentLogMenuEnum.ReleaseCryptoWallet);
            logParam.Add("_ActionType", (int)ActionType.Delete);
            logParam.Add("_RefTransId", string.Empty);
            logParam.Add("_BeforeDetailInfo", input.Before.ToLog());
            logParam.Add("_AfterDetailInfo", null, DbType.String);
            logParam.Add("_Operator", input.Operator);

            #endregion

            var custSameAddress = _reportDbClient.Query<CustCryptoWalletList>("Admin_CustCryptoWalletList_Get", new DynamicParameters(new { CustId = input.Before.CustId }));

            result = _mainDbClient.Execute("Admin_CustCryptoWalletDetail_Del", trans, p);

            if (result > 0)
            {
                var logResult = _mainDbClient.Execute("Admin_AdminAdjustmentInfoLog_Ins", trans, logParam) > 0;
                if (!logResult)
                {
                    _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                    return 0;
                }

                var custInfo = _custInfoService.GetCustInfo(input.Before.CustId);

                if (custInfo != null)
                {
                    foreach (var cryptoWallet in custSameAddress)
                    {
                        var logProp = new DynamicParameters(new
                        {
                            CustId = input.Before.CustId,
                            SiteId = custInfo.SiteId,
                            AccountName = string.Empty,
                            Address = input.Before.Address,
                            NickName = cryptoWallet.NickName,
                            Network = cryptoWallet.Network,
                            CurrencyId = cryptoWallet.CurrencyId,
                            Status = CustUSDTInfoLogStatus.Release,
                            Operator = input.Operator
                        });

                        if (_mainDbClient.Execute("Service_CustUSDTInfoLog_Set_V3", trans, logProp) <= 0)
                        {
                            _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                            return 0;
                        }
                    }
                }
                else
                {
                    _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                    return 0;
                }

            }
            else
            {
                _mainDbClient.RollbackAndDisposeConnection(conn, trans);
                return 0;
            }
            _mainDbClient.CommitAndDisposeConnection(conn, trans);
            return result;
        }
    }
}
