using CleanArchitecture.Core.Utilities;

namespace CleanArchitecture.Infrastructure.Tests
{
    public class Class1
    {
        [Fact]
        public void Do()
        {
            var paymentIdValue = Algorithm.GenerateMD5("1");
            var paymentId = StringCompressor.DecompressString(paymentIdValue).To<long>();

        }
    }
}
