using Newtonsoft.Json;

namespace ForagerSite.Utilities
{
    public static class VmUtilities
    {

        public static T Copy<T>(T obj)
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };

            var serializedObject = JsonConvert.SerializeObject(obj, settings);
            return JsonConvert.DeserializeObject<T>(serializedObject);
        }
    }
}
