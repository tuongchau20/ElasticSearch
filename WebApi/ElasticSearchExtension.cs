using Nest;

public static class ElasticSearchExtension
{
    public static void AddElasticSearch(this IServiceCollection services, IConfiguration configuration)
    {
        var baseUrl = configuration["ElasticSettings:baseUrl"];
        var index = configuration["ElasticSettings:defaultIndex"];
        var settings = new ConnectionSettings(new Uri(baseUrl ?? "")).PrettyJson().CertificateFingerprint("28c0fac51835694784ff25005c48b7c6f8d0f63a6853f345c9c53a5f09ab7bf2").BasicAuthentication("elastic","bmCoh=*ixVVbSSdmyFTP").DefaultIndex(index);
        settings.EnableApiVersioningHeader();
        AddDefaultMappings(settings);
        var client = new ElasticClient(settings);
        services.AddSingleton<IElasticClient>(client);
        CreateIndex(client, index);
    }
    private static void AddDefaultMappings(ConnectionSettings settings)
    {
        settings.DefaultMappingFor<CountryModel>(m => m.Ignore(p => p.Languages).Ignore(p => p.Cca2));
    }
    private static void CreateIndex(IElasticClient client, string indexName)
    {
        var createIndexResponse = client.Indices.Create(indexName, index => index.Map<CountryModel>(x => x.AutoMap()));
    }
}
