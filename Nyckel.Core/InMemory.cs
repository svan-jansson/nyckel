using Microsoft.Extensions.Caching.Memory;
using Nyckel.Core.ValueObjects;
using OneOf.Monads;
using System.Collections;
using System.Reflection;

namespace Nyckel.Core;

public class InMemory : INyckel, IDisposable
{
    private readonly MemoryCache _cache;

    public InMemory()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
    }

    public Option<Value> Get(Key key) =>
        Try.Catching(() =>
        {
            var value = _cache.Get(key.Get());
            if (value == null)
            {
                throw new KeyNotFoundException($"Key not found: {key}");
            }

            return Value.Create(value);
        }).ToOption();

    public Option<Value> Set(Key key, Value value)
        => Try.Catching(() =>
        {
            var internalKey = key.Get();
            var internalValue = value.Get();

            _cache.Set(internalKey, internalValue, NeverExpire());


            return Value.Create(internalValue);
        }).ToOption();

    public Option<Value> Delete(Key key)
        => Try.Catching(() =>
        {
            var internalKey = key.Get();

            var value = _cache.Get(internalKey);

            if (value == null)
            {
                throw new KeyNotFoundException();
            }

            _cache.Remove(internalKey);

            return Value.Create(value);
        })
        .ToOption();

    public INyckel Map(Func<Key, Value, bool> mapContinue)
        => Try.Catching<INyckel>(() =>
        {
            var field = typeof(MemoryCache).GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
            var collection = field.GetValue(_cache) as ICollection;
            var items = new List<string>();

            if (collection != null)
            {
                foreach (var item in collection)
                {
                    var entry = item.GetType().GetProperty("Value").GetValue(item) as ICacheEntry;

                    if (!mapContinue(Key.Create(entry.Key.ToString()), Value.Create(entry.Value)))
                    {
                        break;
                    }
                }
            }

            return this;
        }).DefaultWith(_ => this);

    public int Count() => _cache.Count;

    public Dictionary<string, string> Info() => new Dictionary<string, string>();

    public void Dispose()
    {
        _cache.Dispose();
    }

    private static MemoryCacheEntryOptions NeverExpire() 
        => new MemoryCacheEntryOptions { Priority = CacheItemPriority.NeverRemove };

}
