using Entities.Models.BaseTables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Entities.Models.BaseConfiguration;

public class BaseConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseTable
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {






    }

}

