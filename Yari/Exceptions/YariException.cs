using System;
using System.Collections.Generic;
using System.Text;

namespace Yari.Exceptions
{
    public class YariException : Exception
    {
        public YariException(string message) : base(message)
        {

        }

        public YariException(string message, Exception exp) : base(message, exp)
        {

        }
    }
}
