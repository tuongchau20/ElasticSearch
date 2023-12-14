using Nest;

public static class ElasticSearchExtension
{
    public static void AddElasticSearch(this IServiceCollection services, IConfiguration configuration)
    {
        var baseUrl = configuration["ElasticSettings:baseUrl"];
        var index = configuration["ElasticSettings:defaultIndex"];
        var settings = new ConnectionSettings(new Uri(baseUrl ?? "")).PrettyJson().CertificateFingerprint("0b9fc114299b7820b1747a88415efe62b8bd42d75e5401c18a3bcb4a33345c2a").BasicAuthentication("elastic", "LafljCs+QP*ru9lzGWNU").DefaultIndex(index);
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
