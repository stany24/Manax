using System.Reflection;
using ManaxLibrary.Logging;

namespace ManaxServer.Services.Mapper;

public class ManaxMapper(Mapping mapping) : IMapper
{
    public T Map<T>(object source) where T : new()
    {
        Type sourceType = source.GetType();
        bool allowed = mapping.Allowed(sourceType, typeof(T));
        if (!allowed)
        {
            Logger.LogError($"Cannot map from {typeof(T)} to {sourceType}", new NotImplementedException(),
                Environment.StackTrace);
            throw new NotImplementedException();
        }

        T foo = new();
        foreach (PropertyInfo prop in sourceType.GetProperties())
        {
            PropertyInfo? targetProp = typeof(T).GetProperty(prop.Name);
            if (targetProp == null || !targetProp.CanWrite) continue;
            object? value = prop.GetValue(source);
            targetProp.SetValue(foo, value);
        }

        return foo;
    }

    public void Map(object source, object target)
    {
        Type sourceType = source.GetType();
        Type targetType = target.GetType();
        bool allowed = mapping.Allowed(sourceType, targetType);
        if (!allowed)
        {
            Logger.LogError($"Cannot map from {sourceType} to {targetType}", new NotImplementedException(),
                Environment.StackTrace);
            throw new NotImplementedException();
        }

        foreach (PropertyInfo prop in sourceType.GetProperties())
        {
            PropertyInfo? targetProp = targetType.GetProperty(prop.Name);
            if (targetProp == null || !targetProp.CanWrite) continue;
            object? value = prop.GetValue(source);
            targetProp.SetValue(target, value);
        }
    }
}