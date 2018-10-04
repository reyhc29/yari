using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Yari.Test
{
    [TestClass]
    public class YariServiceUnitTest
    {
        private HttpClient client
        {
            get
            {
                return new HttpClient
                {
                    BaseAddress = new Uri("http://localhost:39123/api/yari/")
                };
            }
        }

        [TestMethod]
        public void TestPostExecute1()
        {
            object data = new 
            {
                    ActionName = "yari_test_1",
	                ResultType = ResultType.MultipleArrays,
	                Params = new
                    {
                        @params = new
                        {
                            age = 1,
                            name = "yari",
                            jobs = new List<string> { "developer", "support", "mathematichian" }
                        }
                    }
            };

            var response = client.PostAsJsonAsync<object>("execute", data);
            response.Wait();
            var result = response.Result.Content.ReadAsAsync<JObject>();
            result.Wait();

            Assert.IsNotNull(result.Result);
        }

        [TestMethod]
        public void TestPostExecute2()
        {
            object data = new 
            {
                ActionName = "yari_test_2",
                ResultType = ResultType.Array,
            };

            var response = client.PostAsJsonAsync<object>("execute", data);
            response.Wait();
            var result = response.Result.Content.ReadAsAsync<JObject>();
            result.Wait();

            Assert.IsNotNull(result.Result);
        }
    }
}
