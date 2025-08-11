using System.Reflection;

namespace AspNet.MinimalApi.BlogWithFront.Common;

public static class EndpointExtensions
{
    public static void MapEndpoints(this IEndpointRouteBuilder app, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetExecutingAssembly();
        var endpointTypes = assembly.GetTypes()
            .Where(t => typeof(IEndpoint).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
            .ToList();

        foreach (var type in endpointTypes)
        {
            if (Activator.CreateInstance(type) is IEndpoint endpoint)
            {
                endpoint.MapEndpoint(app);
            }
        }
    }
}

