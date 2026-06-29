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
        public DbSet<AIConversation> AIConversations { get; set; }
        public DbSet<Itinerary> Itineraries { get; set; }
        public DbSet<ItineraryDay> ItineraryDays { get; set; }
        public DbSet<ItineraryActivity> ItineraryActivities { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }
        public DbSet<RecognizedLocation> RecognizedLocations { get; set; }
        public DbSet<ItineraryFavorite> ItineraryFavorites { get; set; }
        public DbSet<ReviewVerification> ReviewVerifications { get; set; }
        public DbSet<VoiceCommand> VoiceCommands { get; set; }

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

            // AI Conversation relationships
            builder.Entity<AIConversation>()
                .HasOne(ac => ac.User)
                .WithMany()
                .HasForeignKey(ac => ac.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Itinerary relationships
            builder.Entity<Itinerary>()
                .HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ItineraryDay>()
                .HasOne(d => d.Itinerary)
                .WithMany(i => i.Days)
                .HasForeignKey(d => d.ItineraryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ItineraryActivity>()
                .HasOne(a => a.Day)
                .WithMany(d => d.Activities)
                .HasForeignKey(a => a.ItineraryDayId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ItineraryActivity>()
                .HasOne(a => a.Destination)
                .WithMany()
                .HasForeignKey(a => a.DestinationId)
                .OnDelete(DeleteBehavior.SetNull);

            // User Preferences - composite unique index
            builder.Entity<UserPreference>()
                .HasIndex(p => new { p.UserId, p.PreferenceType, p.PreferenceValue })
                .IsUnique();

            builder.Entity<UserPreference>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Recognized Location
            builder.Entity<RecognizedLocation>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RecognizedLocation>()
                .HasOne(r => r.RecognizedDestination)
                .WithMany()
                .HasForeignKey(r => r.RecognizedDestinationId)
                .OnDelete(DeleteBehavior.SetNull);

            // Itinerary Favorites
            builder.Entity<ItineraryFavorite>()
                .HasKey(f => new { f.ItineraryId, f.UserId });

            builder.Entity<ItineraryFavorite>()
                .HasOne(f => f.Itinerary)
                .WithMany(i => i.FavoritedByUsers)
                .HasForeignKey(f => f.ItineraryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ItineraryFavorite>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Review Verification
            builder.Entity<ReviewVerification>()
                .HasOne(rv => rv.Review)
                .WithMany()
                .HasForeignKey(rv => rv.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            // Voice Commands
            builder.Entity<VoiceCommand>()
                .HasOne(vc => vc.User)
                .WithMany()
                .HasForeignKey(vc => vc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

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
