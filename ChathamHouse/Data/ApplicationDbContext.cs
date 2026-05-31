using ChathamHouse.Dto;
using ChathamHouse.Migrations;
using ChathamHouse.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace ChathamHouse.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
         : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Pay> Payments { get; set; }
        public DbSet<Follow> Follow { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<SavedPost> SavedPosts { get; set; }
        public DbSet<LiveStream> LiveStreams { get; set; }
        public DbSet<StreamComment> StreamComments { get; set; }
        public DbSet<StreamLike> StreamLikes { get; set; }
        public DbSet<StreamViewer> StreamViewers { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Follow relationships
            builder.Entity<Follow>()
                .HasOne(f => f.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(f => f.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Follow>()
                .HasOne(f => f.Following)
                .WithMany(u => u.Followers)
                .HasForeignKey(f => f.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed roles
            builder.Entity<IdentityRole>().HasData(new IdentityRole
            {
                Id = "1",
                Name = "Admin",
                NormalizedName = "ADMIN",
            },
            new IdentityRole
            {
                Id = "2",
                Name = "User",
                NormalizedName = "USER",
            });

            // Configure SavedPost
            builder.Entity<SavedPost>()
                .HasOne(sp => sp.Post)
                .WithMany(p => p.SavedByUsers)
                .HasForeignKey(sp => sp.PostId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<SavedPost>()
                .HasOne(sp => sp.User)
                .WithMany()
                .HasForeignKey(sp => sp.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Like
            builder.Entity<Like>()
                .HasOne(l => l.Post)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure Comment
            builder.Entity<Comment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure LiveStream
            builder.Entity<LiveStream>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.StreamKey).IsUnique();
                entity.Property(e => e.StartedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.ViewerCount).HasDefaultValue(0);
                entity.Property(e => e.TotalLikes).HasDefaultValue(0);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.LiveStreams)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure StreamComment
            builder.Entity<StreamComment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Stream)
                    .WithMany(s => s.Comments)
                    .HasForeignKey(e => e.StreamId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.StreamComments)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure StreamLike
            builder.Entity<StreamLike>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.StreamId, e.UserId }).IsUnique();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Stream)
                    .WithMany(s => s.Likes)
                    .HasForeignKey(e => e.StreamId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.StreamLikes)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure StreamViewer
            builder.Entity<StreamViewer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.StreamId, e.UserId }).IsUnique();
                entity.Property(e => e.JoinedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(e => e.Stream)
                    .WithMany(s => s.Viewers)
                    .HasForeignKey(e => e.StreamId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.User)
                    .WithMany(u => u.StreamViewers)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure Post entity
            builder.Entity<Post>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.UserId);
                entity.HasIndex(p => p.IsPaid);

                // Configure relationship with Payment
                entity.HasOne(p => p.payment)
                    .WithOne(p => p.Post)
                    .HasForeignKey<Post>(p => p.PaymentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Payment entity
            builder.Entity<Pay>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.Reference).IsUnique();
                entity.HasIndex(p => p.UserId);

                // Configure decimal precision
                entity.Property(p => p.Amount).HasPrecision(18, 2);

                // Configure relationship with User
                entity.HasOne(p => p.User)
                    .WithMany(u => u.payment)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Configure relationship with Post
                entity.HasOne(p => p.Post)
                    .WithOne(p => p.payment)
                    .HasForeignKey<Pay>(p => p.PostId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Rating relationships - FIXED SYNTAX
            
        }
    }
}