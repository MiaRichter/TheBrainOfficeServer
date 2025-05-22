using System.Data;
using Npgsql;
using Dapper;

namespace TheBrainOfficeServer.Services;

public abstract class AppDbService
{
    private readonly IDbConnection _dbConn;

    protected AppDbService(string connection)
    {
        _dbConn = new NpgsqlConnection(connection);
    }

    public T? GetScalar<T>(string sql, object? param = null)
    {
        try
        {
            _dbConn.Open();
            return _dbConn.Query<T>(sql, param).FirstOrDefault();
        }
        finally
        {
            if (_dbConn.State == ConnectionState.Open)
                _dbConn.Close();
        }
    }

    public List<T> GetList<T>(string sql, object? param = null)
    {
        try
        {
            _dbConn.Open();
            return _dbConn.Query<T>(sql, param).ToList();
        }
        finally
        {
            if (_dbConn.State == ConnectionState.Open)
                _dbConn.Close();
        }
    }

    public List<Dictionary<string, string>> GetList(string sql)
    {
        var res = new List<Dictionary<string, string>>();

        try
        {
            _dbConn.Open();
            using var reader = _dbConn.ExecuteReader(sql);
            while (reader.Read())
            {
                var row = new Dictionary<string, string>();
                for (var i = 0; i < reader.FieldCount; i++)
                    row.Add(
                        reader.GetName(i).ToLower(),
                        reader.GetValue(i)?.ToString() ?? string.Empty
                    );
                res.Add(row);
            }

            return res;
        }
        finally
        {
            if (_dbConn.State == ConnectionState.Open)
                _dbConn.Close();
        }
    }

    public bool Execute(string sql, object? param = null)
    {
        try
        {
            _dbConn.Open();
            _dbConn.Execute(sql, param);
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            if (_dbConn.State == ConnectionState.Open)
                _dbConn.Close();
        }
    }

    public IDbConnection GetConnection()
    {
        return _dbConn;
    }
}