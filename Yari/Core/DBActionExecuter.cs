using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Yari;

[assembly: InternalsVisibleTo("Yari.MySql")]
[assembly: InternalsVisibleTo("Yari.SqlServer")]
[assembly: InternalsVisibleTo("Yari.Test")]
namespace Yari
{
    public abstract class DBActionExecuter
    {
        protected string connectionString;

        protected DBActionExecuter(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected internal Action<LogLevel, string, Exception> onLog;

        internal abstract JObject Execute(ActionDescriptor actionDescriptor);
    }
}
