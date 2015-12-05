using System;
using System.IO;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.FileProviders;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Autopost.Services;
using Autopost.Options;
using Microsoft.Extensions.Configuration;

namespace Autopost
{
    public class Startup
	{
		public IConfiguration Configuration { get; set; }
		
		public const string User = "alex";
		
		public Startup(IHostingEnvironment env, IApplicationEnvironment appEnv)
		{
			var builder = new ConfigurationBuilder()
				.AddJsonFile("config.json")
				.AddEnvironmentVariables();
				
			Configuration = builder.Build();
		}
		
		public void ConfigureServices(IServiceCollection services)
		{
			services.Configure<FacebookOptions>(Configuration.GetSection("Facebook"));
			
			var keyValueStorePath = Configuration["Data:KeyValueStorePath"];
			services.AddInstance<IKeyValueStore>(new FileBasedKeyValueStore(keyValueStorePath));
			
			services.AddTransient<RssFeedClient, RssFeedClient>();
			services.AddTransient<FeedService, FeedService>();
			services.AddMvc();
		}
		
		public void Configure(IApplicationBuilder app)
		{	
			// TODO: Use grunt tasks to copy into wwwroot
			// For libraries
			app.UseStaticFiles(new StaticFileOptions() {
				FileProvider = new PhysicalFileProvider(Path.Combine(Environment.CurrentDirectory, "lib")),
				RequestPath = new PathString("/assets")
			});
			
			app.UseStaticFiles(); // For wwwroot
			   
			app.UseDeveloperExceptionPage(new ErrorPageOptions {
				
			});
					
			app.UseMvc(routes => {
				routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}