using System;

namespace FactoryFactory.Resolution
{
    /// <summary>
    ///  Implements the service caching component of lifecycle management.
    /// </summary>
    public class ServiceCacheResolver : IResolver
    {
        private readonly IResolver _innerResolver;
        private readonly ILifecycle _lifecycle;
        private readonly object _key;

        public ServiceCacheResolver(IServiceDefinition definition, IResolver innerResolver, object key)
        {
            _innerResolver = innerResolver;
            _lifecycle = definition.Lifecycle;
            _key = key;
        }

        public bool CanResolve => true;

        public bool Conditional => false;

        public int Priority => _innerResolver.Priority;

        public Type Type => _innerResolver.Type;

        public bool IsConditionMet(ServiceRequest request) => true;

        public object GetService(ServiceRequest request)
        {
            var cache = _lifecycle.GetCache(request);
            if (cache != null) {
                var service = cache.Retrieve(_key);
                if (service == null) {
                    service = _innerResolver.GetService(request);
                    cache.Store(_key, service);
                }

                return service;
            }
            else {
                return _innerResolver.GetService(request);
            }
        }

        public override string ToString() => $"ServiceCacheResolver for {Type}";
    }
}
