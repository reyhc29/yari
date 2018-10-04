using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yari.Test.Models
{
    public class Test4
    {
        [JsonProperty("result1")]
        public List<Model2> People { get; set; }

        [JsonProperty("result2")]
        public List<Model3> Jobs1 { get; set; }

        [JsonProperty("result3")]
        public List<Model3> Jobs2 { get; set; }
    }
}
