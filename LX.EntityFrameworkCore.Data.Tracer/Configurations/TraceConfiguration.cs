using LX.EntityFrameworkCore.Data.Tracer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LX.EntityFrameworkCore.Data.Tracer.Configurations;

public class TypeConfiguration : IEntityTypeConfiguration<Trace>
{
    public void Configure(EntityTypeBuilder<Trace> builder)
    {
        builder
            .ToTable(nameof(Trace));
    }
}
