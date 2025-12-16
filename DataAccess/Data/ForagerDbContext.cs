using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Data
{
    public class ForagerDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<UserSecurity> UserSecurities { get; set; }
        public DbSet<UserMessage> UserMessages { get; set; } = default!;
        public DbSet<UserThreadState> UserThreadStates { get; set; } = default!;
        public DbSet<UserMessageThread> UserMessageThreads { get; set; } = default!;
        public DbSet<UserFind> UserFinds { get; set; }
        public DbSet<UserFindLocation> UserFindLocations { get; set; }
        public DbSet<UserImage> UserImages { get; set; }
        public DbSet<UserFindsComment> UserFindsComments { get; set; }
        public DbSet<UserFindsCommentXref> UserFindsCommentXrefs { get; set; }
        public DbSet<UserVote> UserVotes { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; } = default!;
        public DbSet<UserRelationship> UserRelationships { get; set; } = default!;
        public DbSet<Notification> Notifications { get; set; } = default!;


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
            modelBuilder.Entity<UserMessageThread>().ToTable("UserMessageThreads");
            modelBuilder.Entity<UserThreadState>().ToTable("UserThreadStates");
            modelBuilder.Entity<UserFind>().ToTable("UserFinds");
            modelBuilder.Entity<UserFindLocation>().ToTable("UserFindLocation");
            modelBuilder.Entity<UserImage>().ToTable("UserImages");
            modelBuilder.Entity<UserFindsComment>().ToTable("UserFindsComments");
            modelBuilder.Entity<UserFindsCommentXref>().ToTable("UserFindsCommentXref");
            modelBuilder.Entity<UserVote>().ToTable("UserVotes");
            modelBuilder.Entity<FriendRequest>().ToTable("FriendRequest");
            modelBuilder.Entity<UserRelationship>().ToTable("UserRelationship");
            modelBuilder.Entity<Notification>().ToTable("Notification");

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
                .HasMany(u => u.UserImages)
                .WithOne(ui => ui.User)
                .HasForeignKey(ui => ui.UsiUsrId);


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

            // One-to-One: findsCommentXref -> findsComment
            modelBuilder.Entity<UserFindsCommentXref>()
                .HasOne(xref => xref.UserFindsComment)
                .WithOne(usc => usc.UserFindsCommentXref)
                .HasForeignKey<UserFindsCommentXref>(xref => xref.UcxUscId);

            // Many-to-One: findsCommentXref -> User (Commenter)
            modelBuilder.Entity<UserFindsCommentXref>()
                .HasOne(xref => xref.User)
                .WithMany(u => u.UserFindsCommentXrefs)
                .HasForeignKey(xref => xref.UcxUsrId)
                .OnDelete(DeleteBehavior.Restrict);

            // Self-referencing relationship for replies
            modelBuilder.Entity<UserFindsComment>()
                .HasOne(usc => usc.ParentComment)
                .WithMany(usc => usc.Replies)
                .HasForeignKey(usc => usc.UscParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserVote>()
                .HasOne(v => v.User)
                .WithMany(u => u.UserVotes)
                .HasForeignKey(v => v.UsvUsrId)
                .OnDelete(DeleteBehavior.Cascade);

            // Find → votes (1-many, optional)
            modelBuilder.Entity<UserVote>()
                .HasOne(v => v.UserFind)
                .WithMany(f => f.UserVotes)
                .HasForeignKey(v => v.UsvUsfId)
                .OnDelete(DeleteBehavior.Restrict); 

            // Comment → votes (1-many, optional)
            modelBuilder.Entity<UserVote>()
                .HasOne(v => v.UserFindsComment)
                .WithMany(c => c.UserVotes)
                .HasForeignKey(v => v.UsvUscId)
                .OnDelete(DeleteBehavior.Restrict);

            //Friend Request
            modelBuilder.Entity<FriendRequest>(entity =>
            {
                entity.HasKey(x => x.FrqId);

                entity.HasOne(x => x.RequesterUser)
                      .WithMany()
                      .HasForeignKey(x => x.FrqRequesterUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.AddresseeUser)
                      .WithMany()
                      .HasForeignKey(x => x.FrqAddresseeUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(x => x.FrqStatus).HasConversion<int>();

                // Prevent duplicate pending requests in the same direction
                entity.HasIndex(x => new { x.FrqRequesterUserId, x.FrqAddresseeUserId })
                      .IsUnique();
            });

            // UserRelationship
            modelBuilder.Entity<UserRelationship>(entity =>
            {
                entity.HasKey(x => x.UrlId);

                entity.HasOne(x => x.UserA)
                      .WithMany()
                      .HasForeignKey(x => x.UrlUserAId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.UserB)
                      .WithMany()
                      .HasForeignKey(x => x.UrlUserBId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.ActionUser)
                      .WithMany()
                      .HasForeignKey(x => x.UrlActionUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(x => x.UrlStatus).HasConversion<int>();

                // Ensure only one relationship row per pair
                entity.HasIndex(x => new { x.UrlUserAId, x.UrlUserBId })
                      .IsUnique();
                // Enforce canonical ordering: UrlUserAId must be < UrlUserBId
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_UserRelationship_UserA_LessThan_UserB",
                    "[UrlUserAId] < [UrlUserBId]"
                ));

            });

            //Notification
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(x => x.NotId);

                entity.HasOne(x => x.User)
                      .WithMany()
                      .HasForeignKey(x => x.NotUserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.ActorUser)
                      .WithMany()
                      .HasForeignKey(x => x.NotActorUserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(x => x.NotType)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(x => x.NotEntityType)
                      .HasMaxLength(50);

                entity.Property(x => x.NotMessage)
                      .HasMaxLength(500);

                entity.HasIndex(x => new { x.NotUserId, x.NotIsRead });
            });
           
            // UserMessage / Inbox

            // UserMessageThread (one thread per pair of users)
            modelBuilder.Entity<UserMessageThread>(entity =>
            {
                entity.ToTable("UserMessageThreads");

                entity.HasKey(t => t.UmtId);

                // IMPORTANT: prevent SQL Server "multiple cascade paths"
                // Two FKs to Users can't both be CASCADE, so we use Restrict.
                entity.HasOne(t => t.UserA)
                    .WithMany()
                    .HasForeignKey(t => t.UmtUserAId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.UserB)
                    .WithMany()
                    .HasForeignKey(t => t.UmtUserBId)
                    .OnDelete(DeleteBehavior.Restrict);

                // One thread has many messages
                entity.HasMany(t => t.Messages)
                    .WithOne(m => m.Thread)
                    .HasForeignKey(m => m.UsmThreadId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Ensure you can’t have duplicate threads for the same pair
                entity.HasIndex(t => new { t.UmtUserAId, t.UmtUserBId })
                    .IsUnique();
            });


            // UserThreadState (per-user state for a thread: unread counts, last read, archived, etc.)
            modelBuilder.Entity<UserThreadState>(entity =>
            {
                entity.ToTable("UserThreadStates");

                entity.HasKey(s => s.UtsId);

                // One row per (Thread, User)
                entity.HasIndex(s => new { s.UtsThreadId, s.UtsUserId })
                    .IsUnique();

                entity.HasOne(s => s.Thread)
                    .WithMany()
                    .HasForeignKey(s => s.UtsThreadId)
                    .OnDelete(DeleteBehavior.Cascade);

                // If a user gets deleted, you can safely delete their thread-state rows
                entity.HasOne(s => s.User)
                    .WithMany()
                    .HasForeignKey(s => s.UtsUserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            // UserMessage (the individual messages inside a thread)
            modelBuilder.Entity<UserMessage>(entity =>
            {
                entity.ToTable("UserMessages");

                entity.HasKey(m => m.UsmId);

                // Message belongs to one thread
                entity.HasOne(m => m.Thread)
                    .WithMany(t => t.Messages)
                    .HasForeignKey(m => m.UsmThreadId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Sender and Recipient both point to Users.
                // Restrict avoids multiple cascade paths on SQL Server.
                entity.HasOne(m => m.Sender)
                    .WithMany()
                    .HasForeignKey(m => m.UsmSenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.Recipient)
                    .WithMany()
                    .HasForeignKey(m => m.UsmRecipientId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Optional reply-to / parent message (in same table)
                entity.HasOne(m => m.ParentMessage)
                    .WithMany()
                    .HasForeignKey(m => m.UsmParentMessageId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Property(m => m.UsmSubject)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(m => m.UsmStatus)
                    .HasMaxLength(20)
                    .IsRequired();
            });





        }
    }
}
