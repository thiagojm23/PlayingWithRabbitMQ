using CryptoService.Abstractions;
using CryptoService.Settings;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace CryptoService.Services.ComunicationServices;

internal sealed class SqliteConnectionFactory(IOptions<NotificationSqliteSettings> settings) : ISqliteConnectionFactory
{
    private readonly NotificationSqliteSettings _settings = settings.Value;

    public async Task<SqliteConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = ResolveConnectionString(_settings.ConnectionString);
        var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static string ResolveConnectionString(string connectionString)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);

        if (!string.IsNullOrWhiteSpace(builder.DataSource) && !Path.IsPathRooted(builder.DataSource))
        {
            builder.DataSource = Path.Combine(AppContext.BaseDirectory, builder.DataSource);
        }

        return builder.ToString();
    }
}
