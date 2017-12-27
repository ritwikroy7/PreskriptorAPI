using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using System.IO;
using System.Net;
using Newtonsoft.Json.Serialization;
using PreskriptorAPI.DataAccess;
using PreskriptorAPI.PDFGenerator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace PreskriptorAPI
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            // Due To https://github.com/dotnet/corefx/issues/8768
            // Temporary fix here https://github.com/StackExchange/StackExchange.Redis/issues/463
            //var dns_Redis_Task = Dns.GetHostAddressesAsync("pub-redis-10931.us-west-2-1.1.ec2.garantiadata.com");
            var dns_Redis_Task = Dns.GetHostAddressesAsync("redis-10273.c17.us-east-1-4.ec2.cloud.redislabs.com");
            var addresses = dns_Redis_Task.Result;
            //var connect_Redis = string.Join(",", addresses.Select(x => x.MapToIPv4().ToString() + ":" + "10931"));
            var connect_Redis = string.Join(",", addresses.Select(x => x.MapToIPv4().ToString() + ":" + "10273"));

            // Add framework services.
            services.AddDistributedRedisCache(options =>
            {
                options.InstanceName = "PreskriptorRedis";
                //options.Configuration = "pub-redis-10931.us-west-2-1.1.ec2.garantiadata.com:10931";
                options.Configuration = connect_Redis;
            });
            services.AddCors(options => 
            {
                options.AddPolicy("PreskriptorCORSPolicy", builder =>
                {
                    builder.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory("PreskriptorCORSPolicy"));
            });
            services.AddMvc(config =>
            {
                var PreskriptorAuthPolicy = new AuthorizationPolicyBuilder()
                                .RequireAuthenticatedUser()
                                .Build();
                config.Filters.Add(new AuthorizeFilter(PreskriptorAuthPolicy));
            });
            //services.AddMvc();
            //services.AddMvc()
                    //.AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1.0", new Info { Title = "PreskriptorAPI", Version = "v1.0" });
                var basePath = System.AppContext.BaseDirectory;
                var xmlPath = Path.Combine(basePath, "PreskriptorAPI.xml"); 
                c.IncludeXmlComments(xmlPath);
            });
            services.AddTransient<IDrugsDataAccess, DrugsDataAccess>();
            services.AddTransient<ITestsDataAccess, TestsDataAccess>();
            services.AddTransient<ILetterheadsDataAccess, LetterheadsDataAccess>();
            services.AddTransient<IPatientsDataAccess, PatientsDataAccess>();
            services.AddTransient<IPrescriptionsDataAccess, PrescriptionsDataAccess>();
            services.AddTransient<IPrescriptionPDFGenerator, PrescriptionPDFGenerator>();            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.AddFile("Logs/Preskriptor-{Date}.txt", isJson: true);
            var options = new JwtBearerOptions
            {
                Audience = Configuration["Auth0:ApiIdentifier"],
                Authority = $"https://{Configuration["Auth0:Domain"]}/"
            };
            app.UseJwtBearerAuthentication(options);
            app.UseCors("PreskriptorCORSPolicy");
            app.UseMvc();            
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "PreskriptorAPI v1.0");
            });
        }
    }
}
