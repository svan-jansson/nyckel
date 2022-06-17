using Nyckel.Core;
using Nyckel.Core.ValueObjects;
using OneOf.Monads;

namespace Nyckel.Web
{
    public class NyckelObservabilityDecorator : INyckel
    {
        private readonly INyckel _implementation;
        private readonly ILogger<INyckel> _logger;

        public NyckelObservabilityDecorator(INyckel implementation, ILogger<INyckel> logger)
        {
            _implementation = implementation;
            _logger = logger;
        }

        public int Count() => _implementation.Count();
        public Dictionary<string, string> Info() => _implementation.Info();

        public INyckel Map(Func<Key, Value, bool> mapContinue)
            => _implementation.Map(mapContinue);

        public Option<Value> Delete(Key key)
            => _implementation.Delete(key)
                    .Do(_ => _logger.LogInformation($"Deleted value for key: {key}"))
                    .DoIfNone(() => _logger.LogError($"Could not delete value for key: {key}"));

        public Option<Value> Get(Key key)
            => _implementation.Get(key)
                        .Do(_ => _logger.LogInformation($"Returned value for key: {key}"))
                        .DoIfNone(() => _logger.LogError($"Could not return value for key: {key}"));

        public Option<Value> Set(Key key, Value value)
            => _implementation.Set(key, value)
                        .Do(_ => _logger.LogInformation($"Added value for key: {key}"))
                        .DoIfNone(() => _logger.LogError($"Could not add value for key: {key}"));

    }
}
