namespace BrightChain.API
{
    using System.Reflection;
    using BrightChain.API.Data;
    using BrightChain.API.Extensions;
    using BrightChain.API.Helpers;
    using BrightChain.API.Services;
    using BrightChain.Engine.Services;
    using LettuceEncrypt;
    using MediatR;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity.UI.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class Startup
    {

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Startup.Environment = env;
        }

        public IConfiguration Configuration { get; }

        public static IWebHostEnvironment Environment { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddPersistence(this.Configuration);
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddDatabaseDeveloperPageExceptionFilter();
            services.AddSingleton<WeatherForecastService>();
            services.AddSingleton<BrightBlockService>();
            services.AddMediatR(Assembly.GetExecutingAssembly());

            if (Environment.IsProduction())
            {
                services.AddLettuceEncrypt();
                services.AddSingleton<ICertificateSource, LettuceEncryptSource>();
            }

            #region API Versioning
            // Add API Versioning to the Project
            services.AddApiVersioning(setupAction: config =>
            {
                // Specify the default API Version as 1.0
                config.DefaultApiVersion = new ApiVersion(1, 0);
                // If the client hasn't specified the API version in the request, use the default API version number 
                config.AssumeDefaultVersionWhenUnspecified = true;
                // Advertise the API versions supported for the particular endpoint
                config.ReportApiVersions = true;
            });
            #endregion

            // requires
            // using Microsoft.AspNetCore.Identity.UI.Services;
            // using WebPWrecover.Services;
            services.AddTransient<IEmailSender, BrightChainEmailSender>();
            services.Configure<AuthMessageSenderOptions>(this.Configuration);
            services.AddApplicationInsightsTelemetry();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            CancellationToken c = default;
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");

                if (!Illuminator.RegistrationEnabled)
                {
                    endpoints.MapGet("/Identity/Account/Register", context => Task.Factory.StartNew(
                        action: () => context.Response.Redirect("/Identity/Account/Login", false, true),
                        cancellationToken: c,
                        creationOptions: TaskCreationOptions.None,
                        scheduler: TaskScheduler.Default));

                    endpoints.MapPost("/Identity/Account/Register", context => Task.Factory.StartNew(
                        action: () => context.Response.Redirect("/Identity/Account/Login", false, true),
                        cancellationToken: c,
                        creationOptions: TaskCreationOptions.None,
                        scheduler: TaskScheduler.Default));
                }
            });
        }
    }
}
