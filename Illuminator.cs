namespace BrightChain.API
{
    public class Illuminator
    {
        public const string Title = "BrightChain";
        public const string Slug = "BrightChain: The Revolution(ary) Network";

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
