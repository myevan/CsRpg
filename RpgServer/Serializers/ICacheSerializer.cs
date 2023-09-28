namespace RpgServer.Serializers
{
    public interface ICacheSerializer
    {
        T? Load<T>(byte[] bytes);
        byte[] Dump<T>(T model);
    }
}
