using System;
using System.Collections.Generic;

namespace FocLauncher
{
    public class LauncherServiceProvider : IServiceProvider
    {
        private static LauncherServiceProvider _instance;

        private readonly IDictionary<Type, object> _services;

        public static LauncherServiceProvider Instance => _instance ??= new LauncherServiceProvider();

        private LauncherServiceProvider()
        {
            _services = new Dictionary<Type, object>();
        }

        public void RegisterService(object service)
        {
            RegisterService(service, service.GetType());
        }

        public void RegisterService(object service, Type serviceType, bool overrideService = false)
        {
            if (!_services.ContainsKey(serviceType))
                _services.Add(serviceType, service);
            else
            {
                if (!overrideService)
                    throw new InvalidOperationException("The service already exists");
                _services[serviceType] = service;
            }
        }

        public T GetService<T>() where T : class
        {
            return GetService(typeof(T)) as T;
        }

        public object GetService(Type serviceType)
        {
            return !_services.ContainsKey(serviceType) ? null : _services[serviceType];
        }
    }
}
