using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Yari.Exceptions;

namespace Yari
{
    public enum ActionType { StoredProc = 0, Function = 1, Web = 2 };

    public enum ResultType { Empty = 0, Scalar = 1, Object = 2, Array = 3, MultipleArrays = 4  }
        
    public class ActionDescriptor
    {
        private JObject actionDescriptor;

        public ActionDescriptor()
        {

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

            if (actionDescriptor == null)
                throw new ConfigurationException("Provided Action Description is not valid: null value");

            if (!actionDescriptor.HasProperty("ActionName"))
                throw new ConfigurationException("Provided Action Description is not valid: Action Name is required.");
            else
                ActionName = actionDescriptor.GetTypedPropertyValue<string>("ActionName");

            if (!actionDescriptor.HasProperty("ActionType"))
                ActionType = ActionType.StoredProc;
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
                Params = actionDescriptor.GetTypedPropertyValue<JObject>("Params");
            }

            IsValidated = true;
        }

        internal bool IsValidated { get; private set; }


        public string ActionName { get; set; }

        public ActionType ActionType { get; set; }

        public JObject Params { get; set; }

        public ResultType ResultType { get; set; }

        public List<string> ResultNames { get; set; }
    }
}
