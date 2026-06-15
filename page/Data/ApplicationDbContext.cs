using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using page.Models;

namespace page.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Destination> Destinations { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<SavedDestination> SavedDestinations { get; set; }
        public DbSet<VisitedDestination> VisitedDestinations { get; set; }
        public DbSet<UserRegionStamp> UserRegionStamps { get; set; }

        public DbSet<TravelStory> TravelStories { get; set; }
        public DbSet<StoryUpvote> StoryUpvotes { get; set; }
        public DbSet<StoryComment> StoryComments { get; set; }
        public DbSet<DestinationImage> DestinationImages { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }
        public DbSet<DestinationComment> DestinationComments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<SavedDestination>()
                .HasKey(sd => new { sd.UserId, sd.DestinationId });
                
            builder.Entity<SavedDestination>()
                .HasOne(sd => sd.User)
                .WithMany(u => u.SavedDestinations)
                .HasForeignKey(sd => sd.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Entity<SavedDestination>()
                .HasOne(sd => sd.Destination)
                .WithMany(d => d.SavedByUsers)
                .HasForeignKey(sd => sd.DestinationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<VisitedDestination>()
                .HasKey(v => new { v.UserId, v.DestinationId });

            builder.Entity<VisitedDestination>()
                .HasOne(v => v.User)
                .WithMany(u => u.VisitedDestinations)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<VisitedDestination>()
                .HasOne(v => v.Destination)
                .WithMany()
                .HasForeignKey(v => v.DestinationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserRegionStamp>()
                .HasIndex(s => new { s.UserId, s.Region })
                .IsUnique();

            builder.Entity<StoryUpvote>()
                .HasKey(su => new { su.UserId, su.StoryId });

            builder.Entity<StoryUpvote>()
                .HasOne(su => su.Story)
                .WithMany(s => s.Upvotes)
                .HasForeignKey(su => su.StoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StoryUpvote>()
                .HasOne(su => su.User)
                .WithMany()
                .HasForeignKey(su => su.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<StoryComment>()
                .HasOne(sc => sc.Story)
                .WithMany(s => s.Comments)
                .HasForeignKey(sc => sc.StoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StoryComment>()
                .HasOne(sc => sc.User)
                .WithMany()
                .HasForeignKey(sc => sc.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<DestinationImage>()
                .HasOne(di => di.Destination)
                .WithMany(d => d.Images)
                .HasForeignKey(di => di.DestinationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserNotification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DestinationComment>()
                .HasOne(dc => dc.Destination)
                .WithMany(d => d.Comments)
                .HasForeignKey(dc => dc.DestinationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DestinationComment>()
                .HasOne(dc => dc.User)
                .WithMany()
                .HasForeignKey(dc => dc.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Destination>()
                .HasOne(d => d.SubmittedByUser)
                .WithMany()
                .HasForeignKey(d => d.SubmittedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Destination>()
                .Property(d => d.EstimatedCost)
                .HasColumnType("decimal(18,2)");
        }
    }
}
