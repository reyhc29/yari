using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Yari;

[assembly: InternalsVisibleTo("Yari.MySql")]
[assembly: InternalsVisibleTo("Yari.SqlServer")]
namespace Yari
{
    internal abstract class DBActionExecuter
    {
        protected string connectionString;

        protected DBActionExecuter(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected internal ILogger<DBActionExecuter> logger;

        internal abstract JObject Execute(ActionDescriptor actionDescriptor);
    }
}
