using Microsoft.EntityFrameworkCore;
using wt_lab7_7postgres_efcore_ivanov.Models;

namespace wt_lab7_7postgres_efcore_ivanov.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Artist> Artists => Set<Artist>();
    public DbSet<Track> Tracks => Set<Track>();
}