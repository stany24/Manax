namespace ManaxServer.Services.Mapper;

public interface IMapper
{
    public T Map<T>(object source) where T : new();
    public void Map(object source, object target);
}