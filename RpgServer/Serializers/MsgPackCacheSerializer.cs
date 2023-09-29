using MessagePack;

namespace RpgServer.Serializers
{
    public class MsgPackCacheSerializer : ICacheSerializer
    {
        public T? Load<T>(byte[] bin)
        {
            try
            {
                return MessagePackSerializer.Deserialize<T>(bin);
            }
            catch (MessagePack.MessagePackSerializationException exc)
            {
                // TODO: 경고 로그
                return default(T);
            }
        }

        public byte[] Dump<T>(T model)
        {
            var bin = MessagePackSerializer.Serialize(model);
            return bin;
        }
    }
}
