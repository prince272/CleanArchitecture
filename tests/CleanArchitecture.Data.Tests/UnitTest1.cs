using CleanArchitecture.Core.Entities;

namespace CleanArchitecture.Data.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var dbContextFactory = new AppDbContextFactory();
            var dbContext = dbContextFactory.CreateDbContext(new string[0])!;
            dbContext.Add(new Product() { Name = "Core i9 Laptop" });
            dbContext.SaveChanges();
        }
    }
}