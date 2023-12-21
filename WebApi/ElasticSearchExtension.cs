using Nest;

public static class ElasticSearchExtension
{
    public static void AddElasticSearch<T>(this IServiceCollection services, IConfiguration configuration) where T : class
    {
        var baseUrl = configuration["ElasticSettings:baseUrl"];
        var settings = new ConnectionSettings(new Uri(baseUrl ?? "")).PrettyJson().CertificateFingerprint("0b9fc114299b7820b1747a88415efe62b8bd42d75e5401c18a3bcb4a33345c2a").BasicAuthentication("elastic", "LafljCs+QP*ru9lzGWNU");
        settings.EnableApiVersioningHeader();
        AddDefaultMappings<T>(settings);
        var client = new ElasticClient(settings);
        services.AddSingleton<IElasticClient>(client);
    }

    private static void AddDefaultMappings<T>(ConnectionSettings settings) where T : class
    {
        settings.DefaultMappingFor<T>(_ => new ClrTypeMapping<T>());
    }

  

}

