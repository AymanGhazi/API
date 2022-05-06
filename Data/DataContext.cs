using System;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace API.Data
{
    public class DataContext : IdentityDbContext<AppUser,
     AppRole, int, IdentityUserClaim<int>, AppUserRole,
    IdentityUserLogin<int>, IdentityRoleClaim<int>,
     IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions options) : base(options)
        {

        }
        public DbSet<UserLike> Likes { get; set; }
        public DbSet<message> Messages { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<Photo> Photos { get; set; }

        protected override void OnModelCreating(ModelBuilder Builder)
        {
            base.OnModelCreating(Builder);

            Builder.Entity<AppUser>()
            .HasMany(ur => ur.Roles)
            .WithOne(u => u.user)
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();

            Builder.Entity<AppRole>()
          .HasMany(ur => ur.UserRoles)
          .WithOne(r => r.Role)
          .HasForeignKey(ur => ur.RoleId)
          .IsRequired();

            Builder.Entity<UserLike>()
            .HasKey(k => new { k.SourceUserID, k.LikedUserID });

            Builder.Entity<UserLike>()
            .HasOne(s => s.sourceUser)
            .WithMany(s => s.UsersILike)
            .HasForeignKey(s => s.SourceUserID)
            .OnDelete(DeleteBehavior.Cascade);

            Builder.Entity<UserLike>()
            .HasOne(s => s.LikedUser)
            .WithMany(s => s.ILikedByUsers)
            .HasForeignKey(s => s.LikedUserID)
            .OnDelete(DeleteBehavior.Cascade);
            //with sql use no action

            Builder.Entity<message>()
            .HasOne(u => u.Recipient)
            .WithMany(R => R.MessagesReceived)
            .OnDelete(DeleteBehavior.Restrict);

            Builder.Entity<message>()
           .HasOne(u => u.Sender)
           .WithMany(R => R.MessagesSent)
           .OnDelete(DeleteBehavior.Restrict);

            //return Only Approved Photos
            Builder.Entity<Photo>().HasQueryFilter(p => p.IsApproved);

            Builder.ApplyUtcDateTimeConverter();
        }
    }

    public static class UtcDateAnnotation
    {
        private const String IsUtcAnnotation = "IsUtc";
        private static readonly ValueConverter<DateTime, DateTime> UtcConverter =
          new ValueConverter<DateTime, DateTime>(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        private static readonly ValueConverter<DateTime?, DateTime?> UtcNullableConverter =
          new ValueConverter<DateTime?, DateTime?>(v => v, v => v == null ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc));

        public static PropertyBuilder<TProperty> IsUtc<TProperty>(this PropertyBuilder<TProperty> builder, Boolean isUtc = true) =>
          builder.HasAnnotation(IsUtcAnnotation, isUtc);

        public static Boolean IsUtc(this IMutableProperty property) =>
          ((Boolean?)property.FindAnnotation(IsUtcAnnotation)?.Value) ?? true;

        /// <summary>
        /// Make sure this is called after configuring all your entities.
        /// </summary>
        public static void ApplyUtcDateTimeConverter(this ModelBuilder builder)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (!property.IsUtc())
                    {
                        continue;
                    }

                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(UtcConverter);
                    }

                    if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(UtcNullableConverter);
                    }
                }
            }
        }
    }
}