using Microsoft.Extensions.Configuration;
using System.IO;

namespace ExamCare
{
    internal static class Startup
    {
        public static IConfiguration Configuration { get; private set; }

        public static void Init()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }
    }
}