using Nyckel.Core;

namespace Nyckel.Web
{
    public static class Config
    {
        public static int Port(string fallback = "9997")
            => Convert.ToInt32(Environment.GetEnvironmentVariable("PORT") ?? fallback);

        public static Type Backend(string fallback = "InMemory") 
            => (Environment.GetEnvironmentVariable("BACKEND") ?? fallback) switch
                {
                    "InMemory" => typeof(InMemory),
                    _ => throw new ArgumentException("Invalid backend")
                };
    }
}
