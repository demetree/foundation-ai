using Scheduler.Worker;
using Foundation.Scheduler.Database;
using Foundation.Scheduler.Services;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

// Context
builder.Services.AddDbContext<SchedulerContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Scheduler"));
});

// Services
builder.Services.AddScoped<DonorJourneyCalculator>();

// Hosted Service
builder.Services.AddHostedService<DonorJourneyProcessor>();

var host = builder.Build();
host.Run();
