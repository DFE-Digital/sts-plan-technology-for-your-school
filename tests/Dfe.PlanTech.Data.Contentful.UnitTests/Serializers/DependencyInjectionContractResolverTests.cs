using System.Reflection;
using Dfe.PlanTech.Data.Contentful.Serializers;
using Newtonsoft.Json.Serialization;
using NSubstitute;

namespace Dfe.PlanTech.Data.Contentful.UnitTests.Serializers
{
    public class DependencyInjectionContractResolverTests
    {
        [Fact]
        public void CreateObjectContract_ModelFound()
        {
            var serviceProvider = Substitute.For<IServiceProvider>();
            var objectType = typeof(DummyModel);

            serviceProvider.GetService(objectType).Returns(new DummyModel());

            var resolver = new DependencyInjectionContractResolver(serviceProvider);

            var contract = InvokeProtectedCreateObjectContract(resolver, objectType);

            Assert.NotNull(contract);
        }

        [Fact]
        public void CreateObjectContract_ModelNotFound()
        {
            var serviceProvider = Substitute.For<IServiceProvider>();
            var objectType = typeof(DummyModel);

            serviceProvider.GetService(objectType).Returns(null);

            var resolver = new DependencyInjectionContractResolver(serviceProvider);

            var contract = InvokeProtectedCreateObjectContract(resolver, objectType);

            Assert.NotNull(contract);
        }


        private static JsonObjectContract? InvokeProtectedCreateObjectContract(DependencyInjectionContractResolver resolver, Type objectType)
        {
            var methodInfo = typeof(DependencyInjectionContractResolver).GetMethod(
                "CreateObjectContract",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new[] { typeof(Type) },
                null
            );

            if (methodInfo != null)
            {
                return (JsonObjectContract)methodInfo.Invoke(resolver, new object[] { objectType })!;
            }

            throw new MissingMethodException("CreateObjectContract method not found.");
        }
    }

    public class DummyModel
    { }
}
