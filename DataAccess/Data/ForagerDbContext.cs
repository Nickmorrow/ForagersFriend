using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Data
{
    public class ForagerDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserSecurity> UserSecurities { get; set; }
        public DbSet<UserMessage> UserMessages { get; set; }
        public DbSet<UserFind> UserFinds { get; set; }
        public DbSet<UserFindLocation> UserFindLocations { get; set; }
        public DbSet<UserImage> UserImages { get; set; }
        public DbSet<UserFindsComment> UserFindsComments { get; set; }
        public DbSet<UserFindsCommentXref> UserFindsCommentXrefs { get; set; }
        public ForagerDbContext(DbContextOptions<ForagerDbContext> options)
            : base(options)
        {
        }
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseLazyLoadingProxies(); // Enable lazy loading
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<UserSecurity>().ToTable("UserSecurity");
            modelBuilder.Entity<UserMessage>().ToTable("UserMessages");
            modelBuilder.Entity<UserFind>().ToTable("finds");
            modelBuilder.Entity<UserFindLocation>().ToTable("UserFindLocation");
            modelBuilder.Entity<UserImage>().ToTable("UserImages");
            modelBuilder.Entity<UserFindsComment>().ToTable("UserFindsComments");
            modelBuilder.Entity<UserFindsCommentXref>().ToTable("findsCommentXref");

            modelBuilder.Entity<User>()
                .HasOne(u => u.UserSecurity)
                .WithOne(us => us.User)
                .HasForeignKey<UserSecurity>(us => us.UssUsrId);

            // One-to-Many: User -> UserFinds
            modelBuilder.Entity<User>()
                .HasMany(u => u.UserFinds)
                .WithOne(uf => uf.User)
                .HasForeignKey(uf => uf.UsfUsrId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.UserImage)
                .WithOne(ui => ui.User)
                .HasForeignKey<UserImage>(ui => ui.UsiUsrId);

            // One-to-One: UserFind -> UserFindLocation
            modelBuilder.Entity<UserFind>()
                .HasOne(uf => uf.UserFindLocation)
                .WithOne(ufl => ufl.UserFind)
                .HasForeignKey<UserFindLocation>(ufl => ufl.UslUsfId);

            // One-to-Many: UserFind -> UserImages
            modelBuilder.Entity<UserFind>()
                .HasMany(uf => uf.UserImages)
                .WithOne(ui => ui.UserFind)
                .HasForeignKey(ui => ui.UsiUsfId);

            // One-to-Many: UserFind -> findsCommentXref
            modelBuilder.Entity<UserFind>()
                .HasMany(uf => uf.UserFindsCommentXrefs)
                .WithOne(xref => xref.UserFind)
                .HasForeignKey(xref => xref.UcxUsfId);

            // One-to-One: findsCommentXref -> UserFindsComment
            modelBuilder.Entity<UserFindsCommentXref>()
                .HasOne(xref => xref.UserFindsComment)
                .WithOne(usc => usc.UserFindsCommentXref)
                .HasForeignKey<UserFindsCommentXref>(xref => xref.UcxUscId);

            // Many-to-One: findsCommentXref -> User (Commenter)
            modelBuilder.Entity<UserFindsCommentXref>()
                .HasOne(xref => xref.User)
                .WithMany(u => u.UserFindsCommentXrefs)
                .HasForeignKey(xref => xref.UcxUsrId);

            // One-to-One: findsCommentXref -> UserFindsComment
            modelBuilder.Entity<UserFindsCommentXref>()
                .HasOne(xref => xref.UserFindsComment)
                .WithOne(usc => usc.UserFindsCommentXref)
                .HasForeignKey<UserFindsCommentXref>(xref => xref.UcxUscId);
        }
    }
}
