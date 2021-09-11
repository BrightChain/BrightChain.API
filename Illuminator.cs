namespace BrightChain.API
{
    /// <summary>
    /// The original "public class Program", renamed for cheekiness.
    /// </summary>
    public class Illuminator
    {
        public const string Title = "BrightChain";
        public const string Slug = "BrightChain: The Revolution(ary) Network";
        public const bool RegistrationEnabled = false;

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }
    }
}
