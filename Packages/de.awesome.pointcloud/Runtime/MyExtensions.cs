using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Awesome.Pointcloud
{
    public static class MyExtensions
    {
        public static JToken Get(this JObject obj, string path)
        {
            var segments = path.Trim('/').Split('/');
            JToken token = obj;
            foreach(var s in segments)
            {
                token = token[s];
                if (token == null)
                    return JValue.CreateNull();
            }
            return token;
        }

        public static JToken Get(this JToken obj, string path)
        {
            return ((JObject)obj).Get(path);
        }
    }
}