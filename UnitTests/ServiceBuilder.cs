using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace UnitTests
{
    public static class ServiceBuilder
    {
        public static ServiceBuilder<TService> For<TService>() where TService : class => new ServiceBuilder<TService>();
    }

    public class ServiceBuilder<TService>
        where TService : class
    {
        private readonly MethodInfo _buildMockMethod;

        private readonly ServiceCollection _services;
        private readonly HashSet<Type> _dependencies;

        public ServiceBuilder()
        {
            _services = new();
            _dependencies = new();

            _buildMockMethod = GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(m => m.Name == nameof(BuildMockObject) && m.IsGenericMethodDefinition);

            _services.AddSingleton<TService>();
            CollectServiceDependencies(typeof(TService));
        }

        public ServiceBuilder<TService> AddDependency<TDependency>(TDependency instance)
            where TDependency : class
        {
            _services.AddSingleton(instance);
            return this;
        }

        public ServiceBuilder<TService> AddDependency<TDependency, TImplementation>()
            where TDependency : class
            where TImplementation : class, TDependency
        {
            _services.AddSingleton<TDependency, TImplementation>();
            CollectServiceDependencies(typeof(TImplementation));
            return this;
        }

        public TService Build()
        {
            RegisterUnmetDependencies();
            var serviceProvider = BuildServiceProvider();
            return serviceProvider.GetRequiredService<TService>();
        }

        private void CollectServiceDependencies(Type type)
        {
            var ctor = type.GetConstructors().Single();
            foreach (var parameter in ctor.GetParameters())
            {
                _dependencies.Add(parameter.ParameterType);
            }
        }

        private void RegisterUnmetDependencies()
        {
            foreach (var dependency in _dependencies)
            {
                if (_services.Any(descriptor => descriptor.ServiceType == dependency))
                {
                    continue;
                }

                var mockObject = BuildMockObject(dependency);
                _services.AddSingleton(dependency, mockObject);
            }
        }

        private object BuildMockObject(Type type)
        {
            return _buildMockMethod.MakeGenericMethod(type).Invoke(this, Array.Empty<object>());
        }

        private T BuildMockObject<T>() where T : class
        {
            var mock = new Mock<T>();
            return mock.Object;
        }

        private IServiceProvider BuildServiceProvider()
        {
            var options = new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true,
            };

            return _services.BuildServiceProvider(options);
        }
    }
}