using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Yari
{
    internal class Any
    {
        public dynamic Instance { get; private set; }

        public Any()
        {
            Instance = new ExpandoObject();
        }

        public Any(dynamic any)
        {
            Instance = any;
        }

        /// <summary>
        /// Returns true if the instance supports adding properties and methods dynamically
        /// </summary>
        public bool IsReallyDynamic
        {
            get
            {
                return (Instance is ExpandoObject);
            }
        }

        public bool HasProperty(string name)
        {
            if (Instance is ExpandoObject)
                return ((IDictionary<string, object>)Instance).ContainsKey(name);

            return Instance.GetType().GetProperty(name) != null;
        }

        public dynamic GetProperty(string name)
        {
            if (Instance is ExpandoObject)
            {
                if (((IDictionary<string, object>)Instance).ContainsKey(name))
                    return ((IDictionary<string, object>)Instance)[name];
                else
                    return null;
            }
            else
                return null;
        }

        public void AddProperty(string name, object value)
        {
            if (Instance is ExpandoObject)
                ((IDictionary<string, object>)Instance).Add(name, value);
        }

        public void AddMethod(Action methodBody, string methodName)
        {
            if (Instance is ExpandoObject)
                this.AddProperty(methodName, methodBody);
        }
    }
}
