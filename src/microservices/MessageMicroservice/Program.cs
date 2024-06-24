using Autofac.Extensions.DependencyInjection;
using Autofac;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using IdentityMicroservice.Repository;
using Middleware;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace MessageMicroservice;

public static class Program
{ 
    private static IConfigurationBuilder ConfigurationBuilder { get; set; }

    static void Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config
                    .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                    .AddJsonFile("appsettings.json", true, true)
                    .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true, true)
                    .AddEnvironmentVariables();

                if (hostingContext.HostingEnvironment.EnvironmentName == "Development")
                {
                    config.AddJsonFile("appsettings.Local.json", true, true);
                }

                ConfigurationBuilder = config;
            })
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureContainer<ContainerBuilder>((container) =>
            {
                var config = ConfigurationBuilder.Build();
                
                container.Register(x =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
                    optionsBuilder.UseNpgsql(config.GetConnectionString("defaultConnection"),
                        b => b.MigrationsAssembly(System.Reflection.Assembly.GetEntryAssembly().FullName));
                    return new UserDbContext(optionsBuilder.Options, x.Resolve<IEncryptor>());
                }).InstancePerLifetimeScope();

                container.RegisterType<UserService>().InstancePerLifetimeScope();
                container.RegisterType<TokenService>().InstancePerLifetimeScope();
                container.RegisterType<Encryptor>().As<IEncryptor>().InstancePerLifetimeScope();
                container.RegisterType<AuthenticationService>().As<IAuthenticationService>().InstancePerLifetimeScope();
                container.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();
                container.RegisterType<MessageRepository>().As<IMessageRepository>().InstancePerLifetimeScope();

                container.Register(context =>
                    new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>()).CreateMapper())
                    .As<IMapper>()
                    .SingleInstance();

            })
            .UseSerilog((_, config) =>
            {
                config
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .WriteTo.Console();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });

            builder.Build().Run();
    }
}

