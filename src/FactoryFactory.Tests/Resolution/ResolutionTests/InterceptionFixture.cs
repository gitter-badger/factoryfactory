using FactoryFactory.Tests.Model;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FactoryFactory.Tests.Resolution.ResolutionTests
{
    public class InterceptionFixture
    {
        [Fact]
        public void DecorationCanReplaceService()
        {
            var container = Configuration.CreateContainer(module => {
                module.Define<IServiceWithoutDependencies>()
                    .As<ServiceWithoutDependencies>();
                module.Intercept<IServiceWithoutDependencies>()
                    .By((req, svc) => new AlternateServiceWithoutDependencies());
            });

            var service = container.GetService<IServiceWithoutDependencies>();
            Assert.IsType<AlternateServiceWithoutDependencies>(service);
        }
    }
}
