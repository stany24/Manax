using System.Reflection;
using ManaxLibrary.Logging;
using System.Collections;

namespace ManaxServer.Services.Mapper;

public class ManaxMapper(Mapping mapping) : IMapper
{
    public T Map<T>(object source) where T : new()
    {
        Type sourceType = source.GetType();
        bool allowed = mapping.Allowed(sourceType, typeof(T));
        if (!allowed)
        {
            Logger.LogError($"Cannot map from {typeof(T)} to {sourceType}", new NotImplementedException());
            throw new NotImplementedException();
        }

        T target = new();
        MapProperties(source, target, sourceType, typeof(T));
        return target;
    }

    public void Map(object source, object target)
    {
        Type sourceType = source.GetType();
        Type targetType = target.GetType();
        bool allowed = mapping.Allowed(sourceType, targetType);
        if (!allowed)
        {
            Logger.LogError($"Cannot map from {sourceType} to {targetType}", new NotImplementedException());
            throw new NotImplementedException();
        }

        MapProperties(source, target, sourceType, targetType);
    }

    private void MapProperties(object source, object target, Type sourceType, Type targetType)
    {
        foreach (PropertyInfo prop in sourceType.GetProperties())
        {
            PropertyInfo? targetProp = targetType.GetProperty(prop.Name);
            if (targetProp == null || !targetProp.CanWrite) continue;
            object? value = prop.GetValue(source);

            if (value == null)
            {
                targetProp.SetValue(target, null);
                continue;
            }

            if (IsEnumerable(prop.PropertyType) && IsEnumerable(targetProp.PropertyType))
            {
                Type? sourceElementType = GetEnumerableElementType(prop.PropertyType);
                Type? targetElementType = GetEnumerableElementType(targetProp.PropertyType);

                if (sourceElementType != null && targetElementType != null && sourceElementType != targetElementType)
                {
                    object? mappedCollection = MapEnumerable(value, targetElementType, targetProp.PropertyType);
                    targetProp.SetValue(target, mappedCollection);
                    continue;
                }
            }

            targetProp.SetValue(target, value);
        }
    }
    
    private static bool IsEnumerable(Type type)
    {
        return type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type);
    }

    private static Type? GetEnumerableElementType(Type type)
    {
        if (!type.IsGenericType) return type.IsArray ? type.GetElementType() : null;
        Type[] genericArgs = type.GetGenericArguments();
        if (genericArgs.Length == 1)
            return genericArgs[0];

        return type.IsArray ? type.GetElementType() : null;
    }

    private object? MapEnumerable(object sourceEnumerable, Type targetElementType, Type targetCollectionType)
    {
        if (sourceEnumerable is not IEnumerable enumerable) return null;

        object? targetCollection;

        if (targetCollectionType.IsGenericType && targetCollectionType.GetGenericTypeDefinition() == typeof(List<>))
        {
            targetCollection = Activator.CreateInstance(targetCollectionType);
        }
        else
        {
            Type listType = typeof(List<>).MakeGenericType(targetElementType);
            targetCollection = Activator.CreateInstance(listType);
        }

        if (targetCollection is not IList targetList) return null;

        foreach (object? item in enumerable)
        {
            if (item == null)
            {
                targetList.Add(null);
                continue;
            }

            try
            {
                object? targetItem = Activator.CreateInstance(targetElementType);
                if (targetItem != null)
                {
                    Map(item, targetItem);
                    targetList.Add(targetItem);
                }
                else
                {
                    targetList.Add(item);
                }
            }
            catch
            {
                targetList.Add(item);
            }
        }

        return targetCollection;
    }
}