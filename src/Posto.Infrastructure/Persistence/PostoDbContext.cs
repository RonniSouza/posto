using Microsoft.EntityFrameworkCore;
using Posto.Application.Abstractions;
using Posto.Domain.Precos;
using PostoEntity = Posto.Domain.Postos.Posto;

namespace Posto.Infrastructure.Persistence;

public sealed class PostoDbContext(DbContextOptions<PostoDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<PostoEntity> Postos => Set<PostoEntity>();

    public DbSet<PrecoCombustivel> Precos => Set<PrecoCombustivel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("postgis");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PostoDbContext).Assembly);
    }
}
