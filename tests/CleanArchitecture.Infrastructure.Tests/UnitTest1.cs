using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Helpers;
using CleanArchitecture.Infrastructure.Data;
using System.ComponentModel;

namespace CleanArchitecture.Infrastructure.Tests
{
    public class UnitTest1
    {
        [Fact]
        public object Test1()
        {

            throw new ArgumentNullException(nameof(MyClass));
            var _retry = true;
            var _queued = true;


            if (!_retry && !_queued)
            {

            }
            else
            {
                return 1;
            }

        }
    }

    public class MyClass
    {
        public MyClass(string name)
        {

        }
    }
}