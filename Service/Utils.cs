using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public static class Utils
    {
        public static ByteArrayContent ConvertForPost<T>(T test)
        {
            var myContent = JsonConvert.SerializeObject(test);
            var buffer = Encoding.UTF8.GetBytes(myContent);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return byteContent;
        }

        //var settings = new JsonSerializerSettings {
        //    ContractResolver = new DefaultContractResolver {
        //        NamingStrategy = new SnakeCaseNamingStrategy { ProcessDictionaryKeys = true }
        //    },
        //    Formatting = Formatting.Indented
        //};

        //public static void SetObjectInSession(this ISession session, string key, object value) {
        //    session.SetString(key, JsonConvert.SerializeObject(value));
        //}

        //public static T GetCustomObjectFromSession<T>(this ISession session, string key) {
        //    var value = session.GetString(key);
        //    return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        //}
    }
}
