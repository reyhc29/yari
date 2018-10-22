using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Yari.Exceptions;

namespace Yari
{
    public enum ActionType { StoredProcedure = 0, Function = 1, Web = 2 };

    public enum ResultType { Empty = 0, Scalar = 1, Object = 2, Array = 3, MultipleArrays = 4  }
        
    public class ActionDescriptor
    {
        private JObject actionDescriptor;
        private bool defaultConstructorUsed = false;

        internal bool IsValidated { get; private set; } = false;

        public ActionDescriptor()
        {
            defaultConstructorUsed = true;
        }

        public ActionDescriptor(string jsonStringActionDescriptor) 
        {
            actionDescriptor = JObject.Parse(jsonStringActionDescriptor);

            Validate();
        }

        public ActionDescriptor(JObject jsonActionDescriptor) 
        {
            actionDescriptor = jsonActionDescriptor;

            Validate();
        }

        public void Validate()
        {
            IsValidated = false;

            if (defaultConstructorUsed)
            {
                if (String.IsNullOrWhiteSpace(ActionName))
                    throw new ConfigurationException("Provided ActionDescription object requires the ActionName property.");
            }
            else
            {
                if (!actionDescriptor.HasProperty("ActionName"))
                    throw new ConfigurationException("Provided ActionDescription object requires the ActionName property.");
                else
                    ActionName = actionDescriptor.GetTypedPropertyValue<string>("ActionName");

                if (!actionDescriptor.HasProperty("ActionType"))
                    ActionType = ActionType.StoredProcedure;
                else
                    ActionType = Enum.Parse<ActionType>(actionDescriptor.GetTypedPropertyValue<string>("ActionType"), true);

                if (!actionDescriptor.HasProperty("ResultType"))
                    ResultType = ResultType.Empty;
                else
                {
                    ResultType = Enum.Parse<ResultType>(actionDescriptor.GetTypedPropertyValue<string>("ResultType"), true);

                    if (ResultType == ResultType.MultipleArrays && actionDescriptor.HasProperty("ResultNames"))
                        ResultNames = new List<string>(actionDescriptor.GetTypedPropertyValue<string[]>("ResultNames"));
                    else
                        ResultNames = null;
                }

                if (!actionDescriptor.HasProperty("Params"))
                {
                    Params = null;
                }
                else
                {
                    Params = actionDescriptor.GetTypedPropertyValue<IEnumerable<dynamic>>("Params");
                }

                if (!actionDescriptor.HasProperty("LogLevel"))
                {
                    LogLevel = LogLevel.Information;
                }
            }

            IsValidated = true;
        }

        public string ActionName { get; set; }

        public ActionType ActionType { get; set; }

        /// <summary>
        /// This can either be a object or an array. When an array is sent, then parameters will be sent in that same order to the stored proc or function. 
        /// If an object is sent, then only one parameter will be sent to the stored proc or funtion as a json value
        /// </summary>
        public IEnumerable<dynamic> Params { get; set; }

        public ResultType ResultType { get; set; }

        public IEnumerable<string> ResultNames { get; set; }

        public LogLevel LogLevel { get; set; }
    }
}
