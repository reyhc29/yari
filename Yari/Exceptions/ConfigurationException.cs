using System;
using System.Collections.Generic;
using System.Text;

namespace Yari.Exceptions
{
    public class ConfigurationException : YariException
    {
        public ConfigurationException(string msg) : base(msg)
        {

        }

        public ConfigurationException(Exception exp) : base("Invalid Configuration", exp)
        {

        }
    }
}
