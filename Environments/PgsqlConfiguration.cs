namespace talk2me_dotnet_api.Environments;

public class PgsqlConfiguration
{
    [ConfigurationKeyName("CONNECTION_STRING")]
    public string ConnectionString { get; set; } = null!;
}