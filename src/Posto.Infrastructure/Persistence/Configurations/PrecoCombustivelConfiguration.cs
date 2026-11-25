using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Posto.Domain.Precos;
using PostoEntity = Posto.Domain.Postos.Posto;

namespace Posto.Infrastructure.Persistence.Configurations;

internal sealed class PrecoCombustivelConfiguration : IEntityTypeConfiguration<PrecoCombustivel>
{
    public void Configure(EntityTypeBuilder<PrecoCombustivel> builder)
    {
        builder.ToTable("precos_combustivel", tabela => tabela
            .HasCheckConstraint("ck_precos_combustivel_valor_positivo", "valor > 0"));
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.HasOne<PostoEntity>()
            .WithMany()
            .HasForeignKey(p => p.PostoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Enums como texto: o banco fica legível e reordenar o enum não corrompe dados.
        builder.Property(p => p.Combustivel).HasConversion<string>().HasMaxLength(30);
        builder.Property(p => p.Fonte).HasConversion<string>().HasMaxLength(30);

        builder.Property(p => p.Valor).HasPrecision(8, 3);
        builder.Ignore(p => p.Unidade);

        // Garante no banco a mesma regra de idempotência do importador.
        builder.HasIndex(p => new { p.PostoId, p.Combustivel, p.DataColeta, p.Fonte }).IsUnique();

        // Consulta típica: preço mais recente de um combustível na região.
        builder.HasIndex(p => new { p.Combustivel, p.DataColeta });
    }
}
