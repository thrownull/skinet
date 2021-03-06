using API.Extensions;
using API.Middleware;
using AutoMapper;
using Infrastructure.Data;
using Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace API
{
	public class Startup
	{
		public IConfiguration Configuration { get; }

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddAutoMapper(typeof(Startup));

			services.AddControllers();

			services.AddDbContext<StoreContext>(options => options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

			services.AddDbContext<AppIdentityDbContext>(options => options.UseSqlite(Configuration.GetConnectionString("IdentityConnection")));

			services.AddSingleton<IConnectionMultiplexer>(options => ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(Configuration.GetConnectionString("Redis"), true)));

			services.AddApplicationServices();

			services.AddIdentityServices(Configuration);

			services.AddSwaggerDocumentation();

			services.AddCors(options => options.AddPolicy("CorsPolicy", policy => policy.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200")));
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseMiddleware<ExceptionMiddleware>();

			app.UseStatusCodePagesWithReExecute("/errors/{0}");

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseStaticFiles();

			app.UseCors("CorsPolicy");

			app.UseAuthentication();

			app.UseAuthorization();

			app.UseSwaggerDocumention();

			app.UseEndpoints(endpoints => endpoints.MapControllers());
		}
	}
}
