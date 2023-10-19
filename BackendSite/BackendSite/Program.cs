using BackendSite;
using BackendSite.Extensions;
using Elastic.Apm.SerilogEnricher;

using Serilog;
using Serilog.Exceptions;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddRemoteJsonConfig(builder.Configuration.GetSection("ConfigService").Get<string[]>());

var excludesMessage = builder.Configuration.GetSection("ExcludesMessage").Get<string[]>();
var excludesApiEndpoints = builder.Configuration.GetSection("ExcludesApiEndpoint").Get<string[]>();
builder.Host.UseSerilog((context, Configuration) =>
{
    Configuration
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.WithElasticApmCorrelationInfo()
        .Enrich.FromLogContext()
        .Filter.ByExcluding(c =>
            c.Properties.Any(p =>
                excludesApiEndpoints.Any(item => p.Value.ToString().Contains(item))
                || excludesMessage.Any(item => c.MessageTemplate.ToString().Contains(item))))
        .Enrich.WithExceptionDetails();
});

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

//GC無法全部回收，改用其他方式重啟服務
//builder.Services.AddHostedService<GarbageCollectCheckWorker>();

var app = builder.Build();
startup.Configure(app, builder.Environment);

var minWorkers = 64;
var minIOCPs = 4;
ThreadPool.SetMinThreads(minWorkers, minIOCPs);

app.Run();
