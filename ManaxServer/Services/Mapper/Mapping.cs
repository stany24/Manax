namespace ManaxServer.Services.Mapper;

public class Mapping
{
    private readonly Dictionary<Type, Type> _typeMap = new();

    protected void CreateMap<TSource, TTarget>()
    {
        _typeMap.Add(typeof(TSource), typeof(TTarget));
    }

    public bool Allowed(Type source, Type target)
    {
        return _typeMap.ContainsKey(source) && _typeMap[source] == target;
    }
}