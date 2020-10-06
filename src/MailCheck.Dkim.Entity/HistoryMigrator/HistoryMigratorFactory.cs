using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Implementations;
using MailCheck.Common.Environment;
using Microsoft.Extensions.DependencyInjection;

namespace MailCheck.Dkim.Entity.HistoryMigrator
{
    public class HistoryMigratorFactory
    {
        public static IHistoryMigrator Create(string connectionString)
        {
            return new ServiceCollection()
                .AddTransient<IHistoryMigrator, HistoryMigrator>()
                .AddTransient<IHistoryDao, HistoryDao>()
                .AddEnvironment()
                .AddTransient<IConnectionInfo>(_ => new StringConnectionInfo(connectionString))
                .BuildServiceProvider()
                .GetRequiredService<IHistoryMigrator>();
        }
    }
}