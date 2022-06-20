using Microsoft.AspNetCore.Builder;

namespace Nyckel.Api
{
    public static class ApplicationExtensions
    {
        public static IApplicationBuilder UseNyckelApi(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<ApiMiddleware>();

            return builder;
        }
    }
}