using Microsoft.AspNetCore.Http;
using Nyckel.Core;
using Nyckel.Core.ValueObjects;
using OneOf.Monads;
using System.Net;

namespace Nyckel.Api
{
    internal class ApiMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly INyckel _nyckel;

        public ApiMiddleware(RequestDelegate next, INyckel nyckel)
        {
            _next = next;
            _nyckel = nyckel;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                var methodOption = GetMethod(context);
                var keyOption = GetKey(context);
                var valueOption = await GetValue(context);

                await methodOption
                    .Merge(keyOption)
                    .Merge(valueOption)
                    .DoIfNone(() => { context.Response.StatusCode = (int)HttpStatusCode.BadRequest; })
                    .Bind(group => group.Item1 switch
                        {
                            "GET" => _nyckel.Get(group.Item2),
                            "POST" => _nyckel.Set(group.Item2, group.Item3),
                            "DELETE" => _nyckel.Delete(group.Item2),
                            _ => new None()
                        })
                    .Fold(
                        () =>
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            return Task.CompletedTask;
                        },
                        async some =>
                        {
                            context.Response.ContentType = "text/plain";
                            context.Response.StatusCode = (int)HttpStatusCode.OK;
                            await context.Response.WriteAsync(some.Get<string>());
                        }
                    );

                
            }
            else
            {
                await _next(context);
            }

        }

        private static Option<string> GetMethod(HttpContext context)
        {
            var method = context.Request.Method;

            if (HttpMethods.IsGet(method))
            {
                return HttpMethods.Get;
            }
            else if (HttpMethods.IsPost(method))
            {
                return HttpMethods.Post;
            }
            else if (HttpMethods.IsDelete(method))
            {
                return HttpMethods.Delete;
            }
            else
            {
                return new None();
            }
        }

        private static Option<Key> GetKey(HttpContext context)
        {
            var components = context.Request.Path.ToUriComponent().Split('/');
            if (components.Length != 3) return new None();

            return Try
                .Catching<Key>(() => Key.Create(Uri.UnescapeDataString(components[2])))
                .ToOption();
        }

        private static async Task<Option<Value>> GetValue(HttpContext context)
        {
            var body = string.Empty;
            if (HttpMethods.IsPost(context.Request.Method))
            {
                using (var stream = new StreamReader(context.Request.Body))
                {
                    body = await stream.ReadToEndAsync();
                }
            }

            return Value.Create(body);
        }
    }
}
