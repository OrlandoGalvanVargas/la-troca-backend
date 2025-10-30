using LaTroca.Application.Interfaces;
using LaTroca.Infrastructure.Services;
using LaTroca.Moderacion.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LaTroca.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddHttpClient<IImageModerationService, HuggingFaceImagenModerationService>();
            services.AddHttpClient<ITextModerationService, HuggingFaceTextModerationService>();

            return services;
        }
    }
}
