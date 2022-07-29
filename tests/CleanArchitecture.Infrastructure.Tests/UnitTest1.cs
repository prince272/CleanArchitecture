using CleanArchitecture.Core.Entities;
using CleanArchitecture.Core.Helpers;
using CleanArchitecture.Infrastructure.Data;
using System.ComponentModel;

namespace CleanArchitecture.Infrastructure.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {

            throw new InvalidOperationException($"Value '{ContactType.PhoneNumber}' of type '{nameof(ContactType)}' is not supported.");
        }

        public enum Type
        {
            A1,
            B2,
            C3
        }
    }
}