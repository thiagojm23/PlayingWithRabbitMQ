using Microsoft.Data.Sqlite;

namespace CryptoService.Abstractions;

internal interface ISqliteConnectionFactory
{
    Task<SqliteConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
