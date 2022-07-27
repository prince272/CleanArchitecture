using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Helpers;
using CleanArchitecture.Infrastructure.Data;

namespace CleanArchitecture.Infrastructure.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var ss = new Uri("https://example.com");
            var ssss = ExpressionHelper.GetPropertyName(() => ss.AbsolutePath);
            Assert.Equal("ss.AbsolutePath", ssss);
        }
    }
}