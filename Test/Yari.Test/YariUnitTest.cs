using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    }
}
