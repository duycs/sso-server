using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Infrastructure.Data.Identity
{
    public class AppIdentityDbContext : IdentityDbContext<AppUser>
    {
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Name = Constants.Roles.Staff, NormalizedName = Constants.Roles.Staff.ToUpper() },
                new IdentityRole { Name = Constants.Roles.Admin, NormalizedName = Constants.Roles.Admin.ToUpper() },
                new IdentityRole { Name = Constants.Roles.CSO, NormalizedName = Constants.Roles.CSO.ToUpper() },
                new IdentityRole { Name = Constants.Roles.Editor, NormalizedName = Constants.Roles.Editor.ToUpper() },
                new IdentityRole { Name = Constants.Roles.QC, NormalizedName = Constants.Roles.QC.ToUpper() }
                );
        }
    }
}
