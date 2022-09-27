
using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
