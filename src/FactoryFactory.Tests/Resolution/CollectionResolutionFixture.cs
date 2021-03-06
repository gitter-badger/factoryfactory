using System.Collections.Generic;
using FactoryFactory.Tests.Model;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FactoryFactory.Tests.Resolution
{
    public class CollectionResolutionFixture
    {
        private IContainer _container;

        public CollectionResolutionFixture()
        {
            _container = Configuration.CreateContainer(mod => {
                mod.Define<IServiceWithoutDependencies>().As<ServiceWithoutDependencies>();
                mod.Define<IServiceWithoutDependencies>().As<AlternateServiceWithoutDependencies>();
            });
        }

        [Fact]
        public void CanResolveEnumerableAsCollection()
        {
            var collection = _container.GetService<ICollection<IServiceWithoutDependencies>>();
            Assert.Equal(2, collection.Count);
        }

        [Fact]
        public void CanResolveEnumerableAsReadOnlyCollection()
        {
            var collection = _container.GetService<IReadOnlyCollection<IServiceWithoutDependencies>>();
            Assert.Equal(2, collection.Count);
        }

        [Fact]
        public void CanResolveEnumerableAsIList()
        {
            var collection = _container.GetService<IList<IServiceWithoutDependencies>>();
            Assert.Equal(2, collection.Count);
        }

        [Fact]
        public void CanResolveEnumerableAsList()
        {
            var collection = _container.GetService<List<IServiceWithoutDependencies>>();
            Assert.Equal(2, collection.Count);
        }
    }
}
