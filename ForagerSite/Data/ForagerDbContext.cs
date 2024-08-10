using ForagerSite.Models;
using Microsoft.EntityFrameworkCore;

namespace ForagerSite.Data
{
    public class ForagerDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserSecurity> UserSecurities { get; set; }
        public DbSet<UserMessage> UserMessages { get; set; }
        public DbSet<UserMessageXref> UserMessageXrefs { get; set; }
        public DbSet<UserFind> UserFinds { get; set; }
        public DbSet<UserFindLocation> UserFindLocations { get; set; }
        public DbSet<UserImage> UserImages { get; set; }
        public DbSet<UserFindsComment> UserFindsComments { get; set; }
        public DbSet<UserFindsCommentXref> UserFindsCommentXrefs { get; set; }

        public ForagerDbContext(DbContextOptions<ForagerDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<UserSecurity>().ToTable("UserSecurity");
            modelBuilder.Entity<UserMessage>().ToTable("UserMessages");
            modelBuilder.Entity<UserMessageXref>().ToTable("UserMessagesXref");
            modelBuilder.Entity<UserFind>().ToTable("UserFinds");
            modelBuilder.Entity<UserFindLocation>().ToTable("UserFindLocation");
            modelBuilder.Entity<UserImage>().ToTable("UserImages");
            modelBuilder.Entity<UserFindsComment>().ToTable("UserFindsComments");
            modelBuilder.Entity<UserFindsCommentXref>().ToTable("UserFindsCommentXref");
        }
    }
}
