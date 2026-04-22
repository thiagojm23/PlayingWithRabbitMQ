namespace CryptoService.Settings;

internal sealed class NotificationSqliteSettings
{
    public const string SectionName = "Notifications:Sqlite";

    public string ConnectionString { get; set; } = "Data Source=notifications.db;Mode=ReadWriteCreate;Cache=Private;Pooling=True";
}
