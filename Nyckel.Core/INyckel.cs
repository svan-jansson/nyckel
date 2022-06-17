using Nyckel.Core.ValueObjects;
using OneOf.Monads;

namespace Nyckel.Core;

public interface INyckel
{
    public Option<Value> Set(Key key, Value value);
    public Option<Value> Get(Key key);
    public Option<Value> Delete(Key key);
    public INyckel Map(Func<Key, Value, bool> mapContinue);
    public int Count();
    public Dictionary<string, string> Info();
}
