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
                var method = GetMethod(context);
                var key = GetKey(context);
                var value = await GetValue(context);

                if (method.IsNone() || key.IsNone() || value.IsNone())
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
                else
                {
                    method
                        .Bind(method => method switch
                        {
                            "GET" => _nyckel.Get(key.Value()),
                            "POST" => _nyckel.Set(key.Value(), value.Value()),
                            "DELETE" => _nyckel.Delete(key.Value()),
                            _ => new None()
                        })
                        .Switch(
                            _ =>
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            },
                            async some =>
                            {
                                context.Response.ContentType = "text/plain";
                                context.Response.StatusCode = (int)HttpStatusCode.OK;
                                await context.Response.WriteAsync(some.Value.Get<string>());
                            }
                        );
                }
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
