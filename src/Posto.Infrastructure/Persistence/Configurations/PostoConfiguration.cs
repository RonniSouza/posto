using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Posto.Domain.Postos;
using PostoEntity = Posto.Domain.Postos.Posto;

namespace Posto.Infrastructure.Persistence.Configurations;

internal sealed class PostoConfiguration : IEntityTypeConfiguration<PostoEntity>
{
    public void Configure(EntityTypeBuilder<PostoEntity> builder)
    {
        builder.ToTable("postos");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        // Value object guardado como texto; o índice único garante um posto por CNPJ.
        builder.Property(p => p.Cnpj)
            .HasConversion(cnpj => cnpj.Valor, valor => Cnpj.Criar(valor).Value)
            .HasMaxLength(14)
            .IsRequired();
        builder.HasIndex(p => p.Cnpj).IsUnique();

        builder.Property(p => p.RazaoSocial).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Bandeira).HasMaxLength(100);

        // Complex type: as colunas do endereço ficam na própria tabela postos (endereco_logradouro, ...).
        builder.ComplexProperty(p => p.Endereco, endereco =>
        {
            endereco.Property(e => e.Logradouro).HasMaxLength(200).IsRequired();
            endereco.Property(e => e.Numero).HasMaxLength(20);
            endereco.Property(e => e.Complemento).HasMaxLength(100);
            endereco.Property(e => e.Bairro).HasMaxLength(100);
            endereco.Property(e => e.Cep).HasMaxLength(8).IsFixedLength();
            endereco.Property(e => e.Municipio).HasMaxLength(100).IsRequired();
            endereco.Property(e => e.Uf).HasMaxLength(2).IsFixedLength().IsRequired();
        });

        // geography (e não geometry): distâncias calculadas em metros sobre o globo, sem projeção.
        builder.Property(p => p.Localizacao).HasColumnType($"geography (point, {PostoEntity.Srid})");
        builder.HasIndex(p => p.Localizacao).HasMethod("gist");
    }
}
