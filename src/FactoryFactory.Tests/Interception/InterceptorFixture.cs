using System;
using FactoryFactory.Tests.Model;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FactoryFactory.Tests.Interception
{
    public class InterceptorFixture
    {
        [Fact]
        public void InterceptedServiceShouldBeIntercepted()
        {
            var interceptor = A.Fake<IInterceptor<IServiceWithoutDependencies>>();
            IServiceWithoutDependencies original = null;
            IServiceWithoutDependencies decorated = null;
            A.CallTo(
                () => interceptor.Intercept(
                    A<ServiceRequest>.Ignored,
                    A<Func<IServiceWithoutDependencies>>.Ignored))
                .ReturnsLazily(
                    (ServiceRequest req, Func<IServiceWithoutDependencies> svc) => {
                        original = svc();
                        decorated = A.Fake<IServiceWithoutDependencies>();
                        return decorated;
                    });
            var module = new Module();
            module.Intercept<IServiceWithoutDependencies>().With(interceptor);
            module.Define<IServiceWithoutDependencies>().As<ServiceWithoutDependencies>()
                .Singleton();
            var container = Configuration.CreateContainer(module);
            var service = container.GetService<IServiceWithoutDependencies>();
            Assert.Same(decorated, service);
            Assert.IsType<ServiceWithoutDependencies>(original);
        }

        [Fact]
        public void SingletonShouldBeInterceptedOnlyOnce()
        {
            var interceptor = A.Fake<IInterceptor<IServiceWithoutDependencies>>();
            A.CallTo(() =>
                interceptor.Intercept(
                    A<ServiceRequest>.Ignored,
                    A<Func<IServiceWithoutDependencies>>.Ignored))
                .ReturnsLazily((ServiceRequest req, Func<IServiceWithoutDependencies> svc) => svc());
            var module = new Module();
            module.Intercept<IServiceWithoutDependencies>().With(interceptor);
            module.Define<IServiceWithoutDependencies>().As<ServiceWithoutDependencies>()
                .Singleton();
            var container = Configuration.CreateContainer(module);
            var service1 = container.GetService<IServiceWithoutDependencies>();
            var service2 = container.GetService<IServiceWithoutDependencies>();
            Assert.Same(service1, service2);
            A.CallTo(() => interceptor.Intercept(
                A<ServiceRequest>.Ignored,
                A<Func<IServiceWithoutDependencies>>.Ignored)).MustHaveHappened(1, Times.Exactly);
        }

        [Fact]
        public void TransientShouldBeInterceptedOncePerInstance()
        {
            var interceptor = A.Fake<IInterceptor<IServiceWithoutDependencies>>();
            A.CallTo(() =>
                    interceptor.Intercept(
                        A<ServiceRequest>.Ignored,
                        A<Func<IServiceWithoutDependencies>>.Ignored))
                .ReturnsLazily((ServiceRequest req, Func<IServiceWithoutDependencies> svc) => svc());
            var module = new Module();
            module.Intercept<IServiceWithoutDependencies>().With(interceptor);
            module.Define<IServiceWithoutDependencies>().As<ServiceWithoutDependencies>()
                .Transient();
            var container = Configuration.CreateContainer(module);
            var service1 = container.GetService<IServiceWithoutDependencies>();
            var service2 = container.GetService<IServiceWithoutDependencies>();
            Assert.NotSame(service1, service2);
            A.CallTo(() => interceptor.Intercept(
                A<ServiceRequest>.Ignored,
                A<Func<IServiceWithoutDependencies>>.Ignored)).MustHaveHappened(2, Times.Exactly);
        }

        [Fact]
        public void LastInterceptorRegisteredShouldBeClosestToTheService()
        {
            var interceptor1 = A.Fake<IInterceptor<IServiceWithoutDependencies>>();
            var interceptor2 = A.Fake<IInterceptor<IServiceWithoutDependencies>>();
            int call1 = 0;
            int call2 = 0;
            int postCall1 = 0;
            int postCall2 = 0;
            int callCount = 0;
            int postCallCount = 0;
            A.CallTo(() =>
                interceptor1.Intercept(
                    A<ServiceRequest>.Ignored,
                    A<Func<IServiceWithoutDependencies>>.Ignored))
                .ReturnsLazily((ServiceRequest req, Func<IServiceWithoutDependencies> svc) => {
                    call1 = ++callCount;
                    var result = svc();
                    postCall1 = ++postCallCount;
                    return result;
                });
            A.CallTo(() =>
                interceptor2.Intercept(
                    A<ServiceRequest>.Ignored,
                    A<Func<IServiceWithoutDependencies>>.Ignored))
                .ReturnsLazily((ServiceRequest req, Func<IServiceWithoutDependencies> svc) => {
                    call2 = ++callCount;
                    var result = svc();
                    postCall2 = ++postCallCount;
                    return result;
                });
            var module = new Module();
            module.Intercept<IServiceWithoutDependencies>().With(interceptor1);
            module.Intercept<IServiceWithoutDependencies>().With(interceptor2);
            module.Define<IServiceWithoutDependencies>().As<ServiceWithoutDependencies>()
                .Transient();
            var container = Configuration.CreateContainer(module);
            var service1 = container.GetService<IServiceWithoutDependencies>();

            Assert.Equal(1, postCall1);
            Assert.Equal(2, postCall2);

            Assert.Equal(2, call1);
            Assert.Equal(1, call2);
        }


        [Fact]
        public void TypeConstraintsOnOpenGenericInterceptorsShouldBeRespected()
        {
            var module = new Module();
            var blueService = A.Fake<IBlueService>();
            var greenService = A.Fake<IGreenService>();

            module.Define(typeof(IInterceptor<>)).As(typeof(InterceptorWithTypeConstraints<>));
            module.Define<IBlueService>().As(blueService);
            module.Define<IGreenService>().As(greenService);
            var container = Configuration.CreateContainer(module);
            var newBlue = container.GetService<IBlueService>();
            var newGreen = container.GetService<IGreenService>();
            Assert.True(newBlue.Intercepted);
            Assert.False(newGreen.Intercepted);
        }
    }
}
