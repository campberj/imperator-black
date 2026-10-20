using Microsoft.EntityFrameworkCore;

namespace Imperator.Api.Data;

public sealed class ImperatorDb(DbContextOptions<ImperatorDb> options) : DbContext(options)
{
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CampaignMembership> CampaignMemberships => Set<CampaignMembership>();
    public DbSet<CampaignEvent> CampaignEvents => Set<CampaignEvent>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Player>(e =>
        {
            e.Property(p => p.DisplayName).HasMaxLength(100);
        });

        b.Entity<Campaign>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(200);
        });

        b.Entity<CampaignMembership>(e =>
        {
            e.HasKey(m => new { m.CampaignId, m.PlayerId });
            e.Property(m => m.Role).HasConversion<string>().HasMaxLength(20);
            e.HasOne(m => m.Campaign).WithMany(c => c.Members).HasForeignKey(m => m.CampaignId);
            e.HasOne(m => m.Player).WithMany(p => p.Memberships).HasForeignKey(m => m.PlayerId);
        });

        b.Entity<CampaignEvent>(e =>
        {
            e.HasIndex(x => new { x.CampaignId, x.Sequence }).IsUnique();
            e.Property(x => x.Type).HasMaxLength(100);
            e.Property(x => x.Payload).HasColumnType("jsonb");
            e.HasOne(x => x.Campaign).WithMany(c => c.Events).HasForeignKey(x => x.CampaignId);
            e.HasOne(x => x.Creator).WithMany().HasForeignKey(x => x.CreatedBy).OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        GuardAppendOnly();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        GuardAppendOnly();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void GuardAppendOnly()
    {
        if (ChangeTracker.Entries<CampaignEvent>().Any(e => e.State is EntityState.Modified or EntityState.Deleted))
            throw new InvalidOperationException("Campaign events are append-only; record a correcting event instead.");
    }
}
