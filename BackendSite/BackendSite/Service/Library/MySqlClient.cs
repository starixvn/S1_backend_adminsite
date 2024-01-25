using System.Data;
using System.Diagnostics;

using BackendSite.Service.Model.enums;

using Dapper;

using Newtonsoft.Json;

using MySql.Data.MySqlClient;

using Serilog.Context;

namespace BackendSite.Service.Library
{
    public sealed class MySqlClient
    {
        public readonly DataBase DataBase;

        private readonly string _connectionString;

        private readonly string _backupConnectionString;

        private readonly ILogger _logger;

        public MySqlClient(DataBase dataBase, ILoggerFactory factory, string connectionString, string backupConnectionString = null)
        {
            DataBase = dataBase;
            _logger = factory.CreateLogger("MySqlClient");
            _connectionString = connectionString;
            _backupConnectionString = backupConnectionString;
        }

        private string HealthCheck()
        {
            if (_backupConnectionString == null)
            {
                return _connectionString;
            }

            try
            {
                using var connection = new MySqlConnection(_connectionString);
                var check = connection.QueryFirst<bool>("Service_HealthCheck", commandType: CommandType.StoredProcedure);
                return check ? _connectionString : _backupConnectionString;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "HealthCheck Exception");
                return _backupConnectionString;
            }
        }

        public MySqlConnection GetMySqlConnection()
        {
            var conn = HealthCheck();
            return new MySqlConnection(conn);
        }

        #region Query

        public async Task<T> QueryFirstAsync<T>(string name, object parameters)
        {
            T result;
            var sw = Stopwatch.StartNew();

            try
            {
                var conn = HealthCheck();
                await using var connection = new MySqlConnection(conn);
                var command = new CommandDefinition(name, parameters, // 使用 CommandDefinition 不讓 Dapper 用預設值建立
                    commandType: CommandType.StoredProcedure,
                    flags: CommandFlags.Buffered);

                result = await connection.QueryFirstOrDefaultAsync<T>(command);
                await connection.CloseAsync();
                await connection.DisposeAsync();
            }
            catch (Exception e)
            {
                await Task.Run(() =>
                {
                    _logger.LogError(exception: e, message: name);
                    return Task.CompletedTask;
                });
                throw;
            }

            LogContext.PushProperty("ElapsedMilliseconds", sw.Elapsed.TotalMilliseconds);
            _logger.LogDebug($"{name} {sw.Elapsed:c}");
            return result;
        }

        public async Task<T> QueryFirstWithNoCacheAsync<T>(string name, object parameters)
        {
            T result;
            var sw = Stopwatch.StartNew();

            try
            {
                var conn = HealthCheck();
                await using var connection = new MySqlConnection(conn);
                var command = new CommandDefinition(name, parameters, // 使用 CommandDefinition 不讓 Dapper 用預設值建立
                    commandType: CommandType.StoredProcedure,
                    flags: CommandFlags.NoCache);

                result = await connection.QueryFirstOrDefaultAsync<T>(command);
                await connection.CloseAsync();
                await connection.DisposeAsync();
            }
            catch (Exception e)
            {
                await Task.Run(() =>
                {
                    _logger.LogError(exception: e, message: name);
                    return Task.CompletedTask;
                });
                throw;
            }

            LogContext.PushProperty("ElapsedMilliseconds", sw.Elapsed.TotalMilliseconds);
            _logger.LogDebug($"{name} {sw.Elapsed:c}");
            return result;
        }

        public T QueryFirst<T>(string name, object parameters, int? commandTimeout = null)
        {
            T result;
            var sw = Stopwatch.StartNew();
            try
            {
                var conn = HealthCheck();
                using var connection = new MySqlConnection(conn);
                var command = new CommandDefinition(name, parameters, // 使用 CommandDefinition 不讓 Dapper 用預設值建立
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: commandTimeout,
                    flags: CommandFlags.Buffered);

                result = connection.QueryFirstOrDefault<T>(command);
                connection.Close();
                connection.Dispose();
            }
            catch (Exception e)
            {
                _logger.LogError(e, name);
                throw;
            }

            LogContext.PushProperty("ElapsedMilliseconds", sw.Elapsed.TotalMilliseconds);
            _logger.LogDebug($"{name} {sw.Elapsed:c}");
            return result;
        }

        public IEnumerable<T> Query<T>(string sql, object parameters = null)
        {
            IEnumerable<T> result;
            var sw = Stopwatch.StartNew();

            try
            {
                var conn = HealthCheck();
                using var connection = new MySqlConnection(conn);
                var command = new CommandDefinition(sql, parameters, // 使用 CommandDefinition 不讓 Dapper 用預設值建立
                    commandType: CommandType.StoredProcedure,
                    flags: CommandFlags.Buffered);

                result = connection.Query<T>(command);
                connection.Close();
                connection.Dispose();
            }
            catch (Exception e)
            {
                _logger.LogError(e, sql);
                throw;
            }

            LogContext.PushProperty("ElapsedMilliseconds", sw.Elapsed.TotalMilliseconds);
            _logger.LogDebug($"{sql} {sw.Elapsed:c}");
            return result.ToList();
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string name, object parameters = null)
        {
            IEnumerable<T> result;
            var sw = Stopwatch.StartNew();

            try
            {
                var conn = HealthCheck();
                await using var connection = new MySqlConnection(conn);
                result = await connection.QueryAsync<T>(name, parameters, commandType: CommandType.StoredProcedure);
                await connection.CloseAsync();
                await connection.DisposeAsync();
            }
            catch (Exception e)
            {
                await Task.Run(() =>
                {
                    _logger.LogError(exception: e, message: name);
                    return Task.CompletedTask;
                });
                throw;
            }

            LogContext.PushProperty("ElapsedMilliseconds", sw.Elapsed.TotalMilliseconds);
            _logger.LogDebug($"{name} {sw.Elapsed:c}");
            return result;
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string name, SqlMapper.IDynamicParameters parameters, int? commandTimeout = null)
        {
            IEnumerable<T> result;
            var sw = Stopwatch.StartNew();
            try
            {
                var conn = HealthCheck();
                await using var connection = new MySqlConnection(conn);
                result = await connection.QueryAsync<T>(name, parameters, commandType: CommandType.StoredProcedure, commandTimeout: commandTimeout);
            }
            catch (Exception e)
            {
                _logger.LogError(e, name);
                throw;
            }

            LogContext.PushProperty("ElapsedMilliseconds", sw.Elapsed.TotalMilliseconds);
            _logger.LogDebug($"{name} {sw.Elapsed:c}");
            return result;
        }

        #endregion

        #region RetryQuery

        public async Task<T> RetryQueryFirstAsync<T>(string name, object parameters)
        {
            T result;
            var sw = Stopwatch.StartNew();

            try
            {
                var conn = HealthCheck();
                _logger.LogInformation($"DB {conn.Substring(0, 30)} | {name} | {JsonConvert.SerializeObject(parameters)}");
                using var connection = new MySqlConnection(conn);
                result = await connection.QueryFirstOrDefaultAsync<T>(name, param: parameters, commandType: CommandType.StoredProcedure);
            }
            catch (MySqlException sqlEx)
            {
                var conn = HealthCheck();
                _logger.LogError(sqlEx, $"DBRetry {conn.Substring(0, 30)} | {name} | {JsonConvert.SerializeObject(parameters)}");
                using var connection = new MySqlConnection(conn);
                result = await connection.QueryFirstOrDefaultAsync<T>(name, param: parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{name} | {JsonConvert.SerializeObject(parameters)}");
                throw;
            }

            LogContext.PushProperty("ElapsedMilliseconds", sw.Elapsed.TotalMilliseconds);
            _logger.LogInformation($"{name} {sw.Elapsed:c}");
            return result;
        }

        public IEnumerable<T> RetryQuery<T>(string sql, object parameters)
        {
            IEnumerable<T> result;
            var sw = Stopwatch.StartNew();

            try
            {
                var conn = HealthCheck();
                _logger.LogInformation($"DB {conn.Substring(0, 30)} | {sql} | {JsonConvert.SerializeObject(parameters)}");
                using var connection = new MySqlConnection(conn);
                result = connection.Query<T>(sql, param: parameters, commandType: CommandType.StoredProcedure);
            }
            catch (MySqlException sqlEx)
            {
                var conn = HealthCheck();
                _logger.LogError(sqlEx, $"DBRetry {conn.Substring(0, 30)} | {sql} | {JsonConvert.SerializeObject(parameters)}");
                using var connection = new MySqlConnection(conn);
                result = connection.Query<T>(sql, param: parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{sql} | {JsonConvert.SerializeObject(parameters)}");
                throw;
            }

            LogContext.PushProperty("ElapsedMilliseconds", sw.Elapsed.TotalMilliseconds);
            _logger.LogInformation($"{sql} {sw.Elapsed:c}");
            return result;
        }

        public async Task<IEnumerable<T>> RetryQueryAsync<T>(string name, object parameters)
        {
            IEnumerable<T> result;
            var sw = Stopwatch.StartNew();
            try
            {
                var conn = HealthCheck();
                _logger.LogInformation($"DB {conn.Substring(0, 30)} | {name} | {JsonConvert.SerializeObject(parameters)}");
                await using var connection = new MySqlConnection(conn);
                result = await connection.QueryAsync<T>(name, parameters, commandType: CommandType.StoredProcedure);
            }
            catch (MySqlException sqlEx)
            {
                var conn = HealthCheck();
                _logger.LogError(sqlEx, $"DBRetry {conn.Substring(0, 30)} | {name} | {JsonConvert.SerializeObject(parameters)}");
                using var connection = new MySqlConnection(conn);
                result = await connection.QueryAsync<T>(name, param: parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{name} | {JsonConvert.SerializeObject(parameters)}");
                throw;
            }

            LogContext.PushProperty("ElapsedMilliseconds", sw.Elapsed.TotalMilliseconds);
            _logger.LogInformation($"{name} {sw.Elapsed:c}");
            return result;
        }

        #endregion

        #region Execute

        public async Task<int> ExecuteAsync(string name, IDbTransaction transaction, object parameters = null)
        {
            var sw = Stopwatch.StartNew();
            int result;
            try
            {
                result = await transaction.Connection.ExecuteAsync(name, parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception e)
            {
                await Task.Run(() =>
                {
                    _logger.LogError(exception: e, message: name);
                    return Task.CompletedTask;
                });
                throw;
            }

            LogContext.PushProperty("ElapsedMilliseconds", sw.Elapsed.TotalMilliseconds);
            _logger.LogDebug($"{name} {sw.Elapsed:c}");
            return result;
        }

        public async Task<int> ExecuteAsync(string name, object parameters)
        {
            int result;
            var sw = Stopwatch.StartNew();
            try
            {
                var conn = HealthCheck();
                await using var connection = new MySqlConnection(conn);
                result = await connection.ExecuteAsync(name, parameters, commandType: CommandType.StoredProcedure);
                await connection.CloseAsync();
                await connection.DisposeAsync();
            }
            catch (Exception e)
            {
                await Task.Run(() =>
                {
                    _logger.LogError(exception: e, message: name);
                    return Task.CompletedTask;
                });
                throw;
            }

            LogContext.PushProperty("ElapsedMilliseconds", sw.Elapsed.TotalMilliseconds);
            _logger.LogDebug($"{name} {sw.Elapsed:c}");
            return result;
        }

        public T ExecuteScalar<T>(string name, object parameters)
        {
            T result;
            var sw = Stopwatch.StartNew();
            try
            {
                var conn = HealthCheck();
                using var connection = new MySqlConnection(conn);
                result = connection.ExecuteScalar<T>(name, parameters, commandType: CommandType.StoredProcedure);
                connection.Close();
                connection.Dispose();
            }
            catch (Exception e)
            {
                _logger.LogError(e, name);
                throw;
            }

            LogContext.PushProperty("ElapsedMilliseconds", sw.Elapsed.TotalMilliseconds);
            _logger.LogDebug($"{name} {sw.Elapsed:c}");
            return result;
        }

        public int Execute(string name, object parameters, int? commandTimeout = null)
        {
            var result = 0;
            var sw = Stopwatch.StartNew();
            try
            {
                var conn = HealthCheck();
                using var connection = new MySqlConnection(conn);
                result = connection.Execute(name, parameters, commandType: CommandType.StoredProcedure, commandTimeout: commandTimeout);
            }
            catch (Exception e)
            {
                _logger.LogError(e, name);
                throw;
            }

            LogContext.PushProperty("ElapsedMilliseconds", sw.Elapsed.TotalMilliseconds);
            _logger.LogDebug($"{name} {sw.Elapsed:c}");
            return result;
        }

        public int Execute(string name, IDbTransaction transaction, object parameters = null)
        {
            var sw = Stopwatch.StartNew();
            int result;
            try
            {
                result = transaction.Connection.Execute(name, parameters, commandType: CommandType.StoredProcedure);
            }
            catch (Exception e)
            {
                _logger.LogError(e, name);
                throw;
            }

            LogContext.PushProperty("elapsed", sw.Elapsed.TotalMilliseconds);
            _logger.LogDebug($"{name} {sw.Elapsed:c}");
            return result;
        }

        public async Task<T> ExecuteScalarAsync<T>(string name, object parameters)
        {
            T result;
            var sw = Stopwatch.StartNew();
            try
            {
                var conn = HealthCheck();
                await using var connection = new MySqlConnection(conn);
                result = await connection.ExecuteScalarAsync<T>(name, parameters, commandType: CommandType.StoredProcedure);
                await connection.CloseAsync();
                await connection.DisposeAsync();
            }
            catch (Exception e)
            {
                await Task.Run(() =>
                {
                    _logger.LogError(exception: e, message: name);
                    return Task.CompletedTask;
                });
                throw;
            }

            LogContext.PushProperty("ElapsedMilliseconds", sw.Elapsed.TotalMilliseconds);
            _logger.LogDebug($"{name} {sw.Elapsed:c}");
            return result;
        }

        #endregion

        #region Commit/Rollback

        public void CommitAndDisposeConnection(MySqlConnection conn, IDbTransaction trans)
        {
            trans.Commit();
            trans.Dispose();
            conn.Close();
            conn.Dispose();
        }

        public void RollbackAndDisposeConnection(MySqlConnection conn, IDbTransaction trans)
        {
            trans.Rollback();
            trans.Dispose();
            conn.Close();
            conn.Dispose();
        }

        public async Task CommitAndDisposeConnectionAsync(MySqlConnection conn, MySqlTransaction trans)
        {
            await trans.CommitAsync();
            await trans.DisposeAsync();
            await conn.CloseAsync();
            await conn.DisposeAsync();
        }

        public async Task RollbackAndDisposeConnectionAsync(MySqlConnection conn, MySqlTransaction trans)
        {
            await trans.RollbackAsync();
            await trans.DisposeAsync();
            await conn.CloseAsync();
            await conn.DisposeAsync();
        }

        #endregion
    }
}
