using System.Reflection;

namespace PaymentGateway.Infrastructure;

public static class CacheHelper<TEntity>
{
    private static readonly PropertyInfo? IdProperty = typeof(TEntity).GetProperty("Id");
    public static string GetPrefix() => $"{typeof(TEntity).Name}";
    public static string GetCacheKey(Guid id) => $"{GetPrefix()}:{id}";
    public static bool TryGetEntityId(TEntity entity, out Guid id)
    {
        id = Guid.Empty;
        var value = IdProperty?.GetValue(entity);
        if (value is not Guid guid || guid == Guid.Empty) return false;
        id = guid;
        
        return true;
    }
}