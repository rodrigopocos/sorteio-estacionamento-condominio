using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SorteioEstacionamento.Infrastructure.Persistence;

public sealed class SorteioDbContextFactory : IDesignTimeDbContextFactory<SorteioDbContext>
{
    public SorteioDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SorteioDbContext>()
            .UseSqlite("Data Source=sorteio-dev.db")
            .Options;
        return new SorteioDbContext(options);
    }
}
