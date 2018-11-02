using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Yari;
using Yari.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace Yari
{
    public class ActionManager
    {
        private Dictionary<string, Func<dynamic, JObject>> executeHandlers;

        private Dictionary<string, Action<dynamic>> beforeExecuteHandlers;

        private Dictionary<string, Func<dynamic, JObject, JObject>> afterExecuteHandlers;

        internal Action<LogLevel, string, Exception> onLog;

        internal DBActionExecuter dbActionExecuter;

        internal ActionManager() 
        {
            beforeExecuteHandlers = new Dictionary<string, Action<dynamic>>();
            executeHandlers = new Dictionary<string, Func<dynamic, JObject>>();
            afterExecuteHandlers = new Dictionary<string, Func<dynamic, JObject, JObject>>();
        }

        public ActionManager(DBActionExecuter dbActionExecuter) : this()
        {
            this.dbActionExecuter = dbActionExecuter;
        }

        /// <summary>
        /// Executes db actions in a way that is more friendly when using the class from c#
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actionName"></param>
        /// <param name="parameters">Make sure all provided parameters have a real type, the mysql driver will attemp to derive the proper db type from the object type</param>
        /// <param name="resultType"></param>
        /// <param name="resultNames"></param>
        /// <returns>Returns a typed result. For complex result type make sure the provided Type has the proper json attributes for deserializing: result, result1, result2 ...</returns>
        public T ExecuteStoredProc<T>(string actionName, ResultType resultType, params dynamic[] parameters)
        {
            return ExecuteStoredProc<T>(actionName, resultType, LogLevel.Information, parameters);
        }
        public T ExecuteStoredProc<T>(string actionName, ResultType resultType, LogLevel logLevel, params dynamic[] parameters)
        {
            ActionDescriptor actionDescriptor = new ActionDescriptor() { ActionName = actionName, Params = parameters, ResultType = resultType, ResultNames = null, ActionType = ActionType.StoredProcedure, LogLevel = logLevel };

            T result = Execute<T>(actionDescriptor);

            return result;
        }


        public void ExecuteStoredProc(string actionName, params dynamic[] parameters)
        {
            ExecuteStoredProc(actionName, LogLevel.Information, parameters);
        }
        public void ExecuteStoredProc(string actionName, LogLevel logLevel, params dynamic[] parameters)
        {
            ActionDescriptor actionDescriptor = new ActionDescriptor() { ActionName = actionName, Params = parameters, ResultType = ResultType.Empty, ResultNames = null, ActionType = ActionType.StoredProcedure, LogLevel = logLevel };

            Execute(actionDescriptor);
        }

        /// <summary>
        /// Executes a db function in similar fashion as ExecuteStoredProc
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actionName"></param>
        /// <param name="resultType"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public T ExecuteFunction<T>(string actionName, ResultType resultType, params dynamic[] parameters)
        {
            return ExecuteFunction<T>(actionName, resultType, LogLevel.Information, parameters);
        }
        public T ExecuteFunction<T>(string actionName, ResultType resultType, LogLevel logLevel, params dynamic[] parameters)
        {
            ActionDescriptor actionDescriptor = new ActionDescriptor() { ActionName = actionName, Params = parameters, ResultType = resultType, ResultNames = null, ActionType = ActionType.Function, LogLevel = logLevel };

            JObject jsonResult = Execute(actionDescriptor);

            T result = jsonResult.GetTypedPropertyValue<T>("result");

            return result;
        }

        /// <summary>
        /// Executes and action based it's ActionDescriptor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actionDescriptor"></param>
        /// <returns>An instance of type T</returns>
        public T Execute<T>(ActionDescriptor actionDescriptor)
        {
            JObject jsonResult = Execute(actionDescriptor);

            T result;
            if (actionDescriptor.ResultType == ResultType.Scalar)
                result = jsonResult.GetTypedPropertyValue<T>("result");
            else
                result = jsonResult.ToObject<T>();

            return result;
        }

        /// <summary>
        /// Executes and action based it's ActionDescriptor
        /// </summary>
        /// <param name="actionDescriptor"></param>
        /// <returns></returns>
        public JObject Execute(ActionDescriptor actionDescriptor)
        {
            JObject result = null;

            if (!actionDescriptor.IsValidated)
                actionDescriptor.Validate();

            if (beforeExecuteHandlers.ContainsKey(actionDescriptor.ActionName))
            {
                beforeExecuteHandlers[actionDescriptor.ActionName].Invoke(actionDescriptor);
            }

            if (actionDescriptor.ActionType == ActionType.Web && executeHandlers.ContainsKey(actionDescriptor.ActionName))
            {
                result = executeHandlers[actionDescriptor.ActionName].Invoke(actionDescriptor);
            }
            else
            {
                result = dbActionExecuter.Execute(actionDescriptor);
            }

            if (afterExecuteHandlers.ContainsKey(actionDescriptor.ActionName))
            {
                result = afterExecuteHandlers[actionDescriptor.ActionName].Invoke(actionDescriptor, result);
            }

            return result;
        }

       

        

        public void RegisterBeforeExecuteHandler(string actionName, Action<dynamic> actionHandler, bool throwIfExist = false)
        {
            if (beforeExecuteHandlers.ContainsKey(actionName))
            {
                if (throwIfExist)
                    throw new ConfigurationException($"BeforeExecute Handler for Action: {actionName} was already registered.");
            }
            else
            {
                beforeExecuteHandlers.Add(actionName, actionHandler);
            }
        }

        public void RegisterAfterExecuteHandler(string actionName, Func<dynamic, JObject, JObject> actionHandler, bool throwIfExist = false)
        {
            if (afterExecuteHandlers.ContainsKey(actionName))
            {
                if (throwIfExist)
                    throw new ConfigurationException($"AfterExecute Handler for Action: {actionName} was already registered.");
            }
            else
            {
                afterExecuteHandlers.Add(actionName, actionHandler);
            }
        }

        public void RegisterExecuteHandler(string actionName, Func<dynamic,JObject> actionHandler, bool throwIfExist = false)
        {
            if (executeHandlers.ContainsKey(actionName))
            {
                if (throwIfExist)
                    throw new ConfigurationException($"Execute Handler for Action: {actionName} was already registered.");
            }
            else
            {
                executeHandlers.Add(actionName, actionHandler);
            }
        }
    }
}
