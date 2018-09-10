using System;
using System.Collections.Generic;
using System.Text;
using Yari.Exceptions;

namespace Yari.MySql
{
    public static class YariOptionsExtensions
    {
        public static YariOptions UserMySql(this YariOptions options, string connectionString)
        {
            if (String.IsNullOrWhiteSpace(connectionString))
                throw new ConfigurationException($"Invalid ConnectionString provided: {connectionString}.");

            options.dbActionExecuter = new MySqlActionExecuter(connectionString);

            return options;
        }
    }
}
