using CleanArchitecture.Core.Utilities;
using CleanArchitecture.Infrastructure.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Reflection;

namespace CleanArchitecture.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<User, Role, long, IdentityUserClaim<long>, UserRole, IdentityUserLogin<long>, IdentityRoleClaim<long>, IdentityUserToken<long>>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Rename the ASP.NET Identity models.
            builder.Entity<User>().ToTable("User");
            builder.Entity<Role>().ToTable("Role");
            builder.Entity<UserRole>().ToTable("UserRole");
            builder.Entity<IdentityUserClaim<long>>().ToTable("UserClaim");
            builder.Entity<IdentityUserLogin<long>>().ToTable("UserLogin");
            builder.Entity<IdentityUserToken<long>>().ToTable("UserToken");
            builder.Entity<IdentityRoleClaim<long>>().ToTable("RoleClaim");

            builder.ApplyEntitiesFromAssembly(typeof(IEntity).Assembly);
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            builder.AddJsonProperties();
        }

        // Entity Framework Core - setting the decimal precision and scale to all decimal properties [duplicate]
        // source: https://stackoverflow.com/questions/43277154/entity-framework-core-setting-the-decimal-precision-and-scale-to-all-decimal-p
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);

            configurationBuilder.Properties<decimal>().HavePrecision(18, 6);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            ValidateEntities();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            ValidateEntities();
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public void ValidateEntities()
        {
            var modifiedEntries = ChangeTracker.Entries().Where(ee => ee.State == EntityState.Added || ee.State == EntityState.Modified).ToArray();

            foreach (var modifiedEntry in modifiedEntries)
            {
                var entity = modifiedEntry.Entity;
                var validatorType = GetPossibleValidatorType(entity);

                if (validatorType != null)
                {
                    var validator = (IValidator)Activator.CreateInstance(validatorType)!;
                    var result = validator.Validate(new ValidationContext<object>(entity));
                    if (!result.IsValid) throw new ValidationException(result.Errors);
                }
            }
        }

        static Type[] ValidatorTypes { get; set; }

        static AppDbContext()
        {
            ValidatorTypes = Assembly.GetExecutingAssembly().GetConstructibleTypes()
          .Where(t => TypeHelper.IsAssignableToGenericType(t, typeof(AbstractValidator<>))).ToArray();
        }

        private static Type? GetPossibleValidatorType(object entity)
        {
            var validatorType = ValidatorTypes.SingleOrDefault(t => t.BaseType != null && t.BaseType.GetGenericArguments()[0] == entity.GetType());
            return validatorType;
        }
    }
}