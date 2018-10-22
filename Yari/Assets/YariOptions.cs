using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yari
{
    public class YariOptions
    {
        internal DBActionExecuter dbActionExecuter;

        public Action<LogLevel,string, Exception> OnLog { internal get; set; }
    }
}
