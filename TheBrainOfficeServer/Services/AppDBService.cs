using System.Data;
using Npgsql;
using Dapper;

namespace TheBrainOfficeServer.Services
{
    public class AppDbService
    {
        private readonly string _connectionString;

        public AppDbService(string connection)
        {
            _connectionString = connection;
        }

        private NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public async Task<T> GetScalarAsync<T>(string sql, object param = null)
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            return await conn.QueryFirstOrDefaultAsync<T>(sql, param);
        }

        public async Task<List<T>> GetListAsync<T>(string sql, object param = null)
        {
            await using var conn = CreateConnection();
            await conn.OpenAsync();
            var list = await conn.QueryAsync<T>(sql, param);
            return list.ToList();
        }

        public async Task<List<Dictionary<string, string>>> GetListAsync(string sql)
        {
            var res = new List<Dictionary<string, string>>();

            await using var conn = CreateConnection();
            await conn.OpenAsync();

            await using var reader = await conn.ExecuteReaderAsync(sql);
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row.Add(reader.GetName(i).ToLower(), reader.GetValue(i)?.ToString() ?? string.Empty);
                }
                res.Add(row);
            }
            return res;
        }

        public async Task<bool> ExecuteAsync(string sql, object param = null)
        {
            try
            {
                await using var conn = CreateConnection();
                await conn.OpenAsync();
                var affectedRows = await conn.ExecuteAsync(sql, param);
                return affectedRows > 0;
            }
            catch (Exception)
            {
                // Логирование ошибки — рекомендую добавить
                return false;
            }
        }
    }
}
