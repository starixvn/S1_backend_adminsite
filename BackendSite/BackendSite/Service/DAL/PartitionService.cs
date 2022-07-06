using BackendSite.Extensions;
using BackendSite.Service.Library;
using BackendSite.Service.Model.enums;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendSite.Service.DAL
{
    public class PartitionService
    {
        private readonly MySqlClient _mainDbClient;
        private readonly MySqlClient _reportDbClient;

        public PartitionService(IServiceProvider provider)
        {
            _mainDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Main);
            _reportDbClient = provider.GetServices<MySqlClient>().First(p => p.DataBase == DataBase.Report);
        }

        public List<TResult> Query<TResult>(DateTimeOffset StartDate, DateTimeOffset endDate, string spName, object parameters, DataBase dataBase = DataBase.Report)
        {
            var tasks = new List<Task<IEnumerable<TResult>>>();
            foreach (var partitionKey in StartDate.GetPartitionRange(endDate))
            {
                var p = new DynamicParameters(parameters);
                p.Add("YYYYMMDD", partitionKey);
                var task = dataBase switch
                {
                    DataBase.Report => _reportDbClient.QueryAsync<TResult>(spName, p),
                    DataBase.Main => _mainDbClient.QueryAsync<TResult>(spName, p),
                    _ => throw new NotImplementedException(),
                };
                tasks.Add(task);
            }

            Task.WhenAll(tasks);

            var result = new List<TResult>();
            foreach (var item in tasks)
            {
                result.AddRange(item.Result);
            }
            return result;
        }
    }
}
