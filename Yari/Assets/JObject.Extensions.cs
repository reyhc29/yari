using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Yari
{
    public static class JObjectExtensions
    {
        public static bool HasProperty(this JObject jObject, string propertyName)
        {
            return (jObject.Property(propertyName) != null);
        }

        public static void AddValue(this JObject jObject, object value)
        {
            jObject.Add(value);
        }
        
        public static void AddProperty(this JObject jObject, string propertyName, object value)
        {
            jObject.Add(propertyName, JToken.FromObject(value));
        }

        public static void AddArray(this JObject jObject, object[] array)
        {
            jObject.Add(JArray.FromObject(array));
        }

        public static void AddPropertyArray(this JObject jObject, string propertyName, object[] array)
        {
            jObject.Add(propertyName, JArray.FromObject(array));
        }

        public static object GetPropertyObjectValue(this JObject jObject, string propertyName)
        {
            dynamic result;

            if (jObject.Property(propertyName).Value is JArray)
            {
                result = jObject.Property(propertyName).Value;
            }
            else
            {
                result = ((JValue)((JProperty)jObject.Property(propertyName)).Value).Value;
            }

            return result;
        }

        public static T GetTypedPropertyValue<T>(this JObject jObject, string propertyName)
        {
            return jObject.Property(propertyName).Value.ToObject<T>();
        }
    }
}
