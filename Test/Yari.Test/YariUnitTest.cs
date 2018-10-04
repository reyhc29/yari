﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Yari.MySql;
using Yari.Test.Models;

namespace Yari.Test
{
    [TestClass]
    public class YariUnitTest
    {
        private ActionManager actionManager
        {
            get
            {
                ActionManager actionManager = new ActionManager();
                actionManager.dbActionExecuter = new MySqlActionExecuter("[yourdbconnectionstring]");

                return actionManager;
            }
        }

        [TestMethod]
        public void TestExecute1()
        {
            ActionDescriptor actionDescriptor = new ActionDescriptor()
            {
                ActionName = "yari_test_2",
                ResultType = ResultType.Array,
            };

            JObject result = actionManager.Execute(actionDescriptor);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestExecute2()
        {
            ActionDescriptor actionDescriptor = new ActionDescriptor()
            {
                ActionName = "yari_test_2",
                ResultType = ResultType.Array,
            };

            ArrayResult<Model1> result = actionManager.Execute<ArrayResult<Model1>>(actionDescriptor);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestExecute3()
        {
            ActionDescriptor actionDescriptor = new ActionDescriptor()
            {
                ActionName = "yari_test_3",
                ResultType = ResultType.Scalar,
            };

            JObject result = actionManager.Execute(actionDescriptor);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestExecute4()
        {
            dynamic param = new
            {
                @params = JObject.FromObject(new
                {
                    age = 1,
                    name = "yari",
                    jobs = new List<string> { "developer", "support", "mathematichian" }
                }).ToString()
            };

            Test4 result = actionManager.ExecuteDBAction<Test4>("yari_test_1", ResultType.MultipleArrays, param);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void TestExecute5()
        {
            dynamic param = new
            {
                age = 1,
                name = "yari",
                birthday = DateTime.Now
            };

            Test5 result = actionManager.ExecuteDBAction<Test5>("yari_test_4", ResultType.Object, param);

            Assert.IsNotNull(result);
        }

        
    }
}