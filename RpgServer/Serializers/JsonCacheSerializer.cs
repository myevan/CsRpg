using RpgServer.Interfaces;
using System.Text;
using System.Text.Json;

namespace RpgServer.Serializers
{
    public class JsonCacheSerializer : ICacheSerializer
    {
        public T? Load<T>(byte[] bin)
        {
            var str = Encoding.UTF8.GetString(bin);
            return JsonSerializer.Deserialize<T>(str);
        }

        public byte[] Dump<T>(T model)
        {
            var str = JsonSerializer.Serialize(model);
            var bin = Encoding.UTF8.GetBytes(str);
            return bin;
        }
    }
}
