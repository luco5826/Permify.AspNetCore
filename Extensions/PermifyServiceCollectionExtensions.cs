using Microsoft.Extensions.DependencyInjection;
using Permify.AspNetCore.Implementations;
using Permify.AspNetCore.Interfaces;

namespace Permify.AspNetCore.Extensions;

public static class PermifyServiceCollectionExtensions
{
    public static IServiceCollection AddPermify(this IServiceCollection services, Action<PermifyOptions>? setupAction = null)
    {
        PermifyOptions opts = new PermifyOptions();
        setupAction?.Invoke(opts);

        if (opts.Host == null)
            throw new ArgumentException($"{nameof(opts.Host)} cannot be null.");

        services.AddSingleton<IPermifyAuthorizationService>(factory => new PermifyAuthorizationService(opts));
        return services;
    }
}