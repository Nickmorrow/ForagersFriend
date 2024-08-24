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
            modelBuilder.Entity<UserFind>().ToTable("userFinds");
            modelBuilder.Entity<UserFindLocation>().ToTable("UserFindLocation");
            modelBuilder.Entity<UserImage>().ToTable("UserImages");
            modelBuilder.Entity<UserFindsComment>().ToTable("UserFindsComments");
            modelBuilder.Entity<UserFindsCommentXref>().ToTable("UserFindsCommentXref");

            modelBuilder.Entity<User>()
                .HasOne(u => u.UserSecurity)
                .WithOne(us => us.User)
                .HasForeignKey<UserSecurity>(us => us.UssUsrId);

            // One-to-Many: User -> UserFinds
            modelBuilder.Entity<User>()
                .HasMany(u => u.UserFinds)
                .WithOne(uf => uf.User)
                .HasForeignKey(uf => uf.UsfUsrId);

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

            // One-to-Many: UserFind -> UserFindsCommentXref
            modelBuilder.Entity<UserFind>()
                .HasMany(uf => uf.UserFindsCommentXrefs)
                .WithOne(xref => xref.UserFind)
                .HasForeignKey(xref => xref.UcxUsfId);

            // One-to-One: UserFindsCommentXref -> UserFindsComment
            modelBuilder.Entity<UserFindsCommentXref>()
                .HasOne(xref => xref.UserFindsComment)
                .WithOne(usc => usc.UserFindsCommentXref)
                .HasForeignKey<UserFindsCommentXref>(xref => xref.UcxUscId);

            // Many-to-One: UserFindsCommentXref -> User (Commenter)
            modelBuilder.Entity<UserFindsCommentXref>()
                .HasOne(xref => xref.User)
                .WithMany(u => u.UserFindsCommentXrefs)
                .HasForeignKey(xref => xref.UcxUsrId);

            // One-to-One: UserFindsCommentXref -> UserFindsComment
            modelBuilder.Entity<UserFindsCommentXref>()
                .HasOne(xref => xref.UserFindsComment)
                .WithOne(usc => usc.UserFindsCommentXref)
                .HasForeignKey<UserFindsCommentXref>(xref => xref.UcxUscId);
        }
    }
}
