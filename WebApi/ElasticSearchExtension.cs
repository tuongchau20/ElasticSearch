using Nest;

public static class ElasticSearchExtension
{
    public static void AddElasticSearch<T>(this IServiceCollection services, IConfiguration configuration) where T : class
    {
        var baseUrl = configuration["ElasticSettings:baseUrl"];
        var index = configuration["ElasticSettings:defaultIndex"];
        var settings = new ConnectionSettings(new Uri(baseUrl ?? "")).PrettyJson().CertificateFingerprint("0b9fc114299b7820b1747a88415efe62b8bd42d75e5401c18a3bcb4a33345c2a").BasicAuthentication("elastic", "LafljCs+QP*ru9lzGWNU").DefaultIndex(index);
        settings.EnableApiVersioningHeader();
        AddDefaultMappings<T>(settings);
        var client = new ElasticClient(settings);
        services.AddSingleton<IElasticClient>(client);
        CreateIndex<T>(client, index);
        EnsureIndexExists<T>(client, index);
    }
    private static void AddDefaultMappings<T>(ConnectionSettings settings) where T : class
    {
        settings.DefaultMappingFor<T>(_ => new ClrTypeMapping<T>());
    }

    private static void CreateIndex<T>(IElasticClient client, string indexName) where T : class
    {
        var createIndexResponse = client.Indices.Create(indexName, index => index.Map<T>(x => x.AutoMap()));
    }
    private static void EnsureIndexExists<T>(IElasticClient client, string indexName) where T : class
    {
        var existsResponse = client.Indices.Exists(indexName);

        if (!existsResponse.Exists)
        {
            var createIndexResponse = client.Indices.Create(indexName, c => c
                .Map<T>(m => m.AutoMap()));

            if (!createIndexResponse.IsValid)
            {
                Console.WriteLine($"Error creating index: {createIndexResponse.DebugInformation}");
            }
        }
        else
        {
            // Update the index mapping
            var response = client.Map<T>(m => m
                .Index(indexName)
                .AutoMap());

            if (!response.IsValid)
            {
                Console.WriteLine($"Error updating index mapping: {response.DebugInformation}");
            }
        }
    }
}

