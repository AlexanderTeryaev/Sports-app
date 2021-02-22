using System;
using System.Collections.Generic;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace DataService
{

    public static class ConnectionHelper
    {
        public static string GetConnectionString()
        {
            string serverName = "DEVUPLDB01\\DEV2K5";
            string databaseName = "VisitorsCenter";
            string userID = "devsa";
            string password = "sadev";

            var sqlBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = databaseName,
                PersistSecurityInfo = true,
                MultipleActiveResultSets = true,
                ApplicationName = "EntityFramework",
                UserID = userID,
                Password = password
            };

            return sqlBuilder.ToString();
        }
    }
}
