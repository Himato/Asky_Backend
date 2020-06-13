using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Asky.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, UserRole, string>
    {
        public DbSet<Category> Categories { get; set; }

        public DbSet<Topic> Topics { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Reply> Replies { get; set; }

        public DbSet<Vote> Votes { get; set; }

        public DbSet<CommentVote> CommentVotes { get; set; }

        public DbSet<View> Views { get; set; }

        public DbSet<Bookmark> Bookmarks { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbContextOptions Options { get; set; }

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            Options = options;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            SeedData(builder);

            builder.Entity<ApplicationUser>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd();

            builder.Entity<Category>()
                .HasIndex(p => p.Uri)
                .IsUnique();

            builder.Entity<Topic>()
                .HasIndex(t => t.Uri)
                .IsUnique();

            //
            // Category
            //
            builder.Entity<Category>()
                .HasMany(c => c.Topics)
                .WithOne(t => t.Category)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            //
            // Bookmark
            //
            builder.Entity<Bookmark>()
                .HasOne(b => b.Topic)
                .WithMany()
                .HasForeignKey(b => b.TopicId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Bookmark>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            //
            // Topic
            //
            builder.Entity<Topic>()
                .HasOne(t => t.User)
                .WithMany(u => u.Topics)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Topic>()
                .HasMany(t => t.Comments)
                .WithOne(b => b.Topic)
                .HasForeignKey(c => c.TopicId)
                .OnDelete(DeleteBehavior.NoAction);
            
            //
            // Vote
            //
            builder.Entity<Vote>()
                .HasOne(v => v.User)
                .WithMany(u => u.Votes)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Vote>()
                .HasOne(v => v.Topic)
                .WithMany(u => u.Votes)
                .HasForeignKey(v => v.TopicId)
                .OnDelete(DeleteBehavior.NoAction);

            //
            // Comment
            //
            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Comment>()
                .HasMany(c => c.Replies)
                .WithOne(r => r.Comment)
                .HasForeignKey(r => r.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            //
            // CommentVote
            //
            builder.Entity<CommentVote>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CommentVote>()
                .HasOne(c => c.Comment)
                .WithMany(c => c.Votes)
                .HasForeignKey(v => v.CommentId)
                .OnDelete(DeleteBehavior.Cascade);

            //
            // Reply
            //
            builder.Entity<Reply>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            //
            // View
            //
            builder.Entity<View>()
                .HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            //
            // Notification
            //
            builder.Entity<Notification>()
                .HasOne(n => n.Sender)
                .WithMany()
                .HasForeignKey(n => n.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Notification>()
                .HasOne(n => n.Receiver)
                .WithMany()
                .HasForeignKey(n => n.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Notification>()
                .HasOne(n => n.Topic)
                .WithMany()
                .HasForeignKey(n => n.TopicId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Notification>()
                .HasOne(n => n.Comment)
                .WithMany()
                .HasForeignKey(n => n.CommentId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        protected void SeedData(ModelBuilder builder)
        {
            builder.Entity<UserRole>().HasData(new UserRole(UserRole.Admin));
        }
    }
}
