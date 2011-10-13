using System;
using System.Collections.Generic;

namespace Client
{
    public class GameServiceContainer : IServiceProvider
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public void AddService(Type type, object provider)
        {
            Asserter.AssertIsNotNull(type, "type");
            Asserter.AssertIsNotNull(provider, "provider");
            Asserter.Assert(_services.ContainsKey(type), "Service is already present.");
            Asserter.Assert(
                !type.IsAssignableFrom(provider.GetType()),
                string.Format("Service type {0} must be assignable from type {1}.", provider.GetType().FullName, type.GetType().FullName));

            _services.Add(type, provider);
        }

        public void RemoveService(Type type)
        {
            Asserter.AssertIsNotNull(type, "type");
            _services.Remove(type);
        }

        public object GetService(Type type)
        {
            Asserter.AssertIsNotNull(type, "type");

            if (_services.ContainsKey(type))
                return _services[type];

            return null;
        }
    }
}