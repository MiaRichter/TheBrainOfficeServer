using System.Collections.Generic;
using TheBrainOfficeServer.Models;
using TheBrainOfficeServer.Services;

namespace TheBrainOfficeServer.Repositories
{
    public class InitializeRepo
    {
        public List<InitializeModel> TestRepo(AppDBService _db)
        {
            List<InitializeModel> result = new();
            string query = @"SELECT * FROM components";
            result = _db.GetList<InitializeModel>(query);

            return result;
        }
    }
}
