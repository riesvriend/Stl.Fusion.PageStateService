using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Stl.Fusion.EntityFramework.Authentication
{
    public class DbSessionInfoTrimmer<TDbContext> : DbWakeSleepProcessBase<TDbContext>
        where TDbContext : DbContext
    {
        public class Options
        {
            public TimeSpan CheckInterval { get; set; } = TimeSpan.FromHours(1);
            public TimeSpan MaxSessionAge { get; set; } = TimeSpan.FromDays(60);
            public int BatchSize { get; set; } = 1000;
            public LogLevel LogLevel { get; set; } = LogLevel.Information;
        }

        protected IDbSessionInfoRepo<TDbContext> Sessions { get; }
        protected TimeSpan CheckInterval { get; }
        protected TimeSpan MaxSessionAge { get; }
        protected int BatchSize { get; }
        protected int LastTrimCount { get; set; }
        protected Random Random { get; }
        protected LogLevel LogLevel { get; }

        public DbSessionInfoTrimmer(Options? options, IServiceProvider services)
            : base(services)
        {
            options ??= new();
            LogLevel = options.LogLevel;

            CheckInterval = options.CheckInterval;
            MaxSessionAge = options.MaxSessionAge;
            BatchSize = options.BatchSize;
            Sessions = services.GetRequiredService<IDbSessionInfoRepo<TDbContext>>();
            Random = new Random();
        }

        protected override async Task WakeUp(CancellationToken cancellationToken)
        {
            var minLastSeenAt = (Clock.Now - MaxSessionAge).ToDateTime();
            LastTrimCount = await Sessions
                .Trim(minLastSeenAt, BatchSize, cancellationToken)
                .ConfigureAwait(false);

            var logEnabled = LogLevel != LogLevel.None && Log.IsEnabled(LogLevel);
            if (LastTrimCount > 0 && logEnabled)
                Log.Log(LogLevel, "Trimmed {Count} sessions", LastTrimCount);
        }

        protected override Task Sleep(Exception? error, CancellationToken cancellationToken)
        {
            var delay = default(TimeSpan);
            if (error != null)
                delay = TimeSpan.FromMilliseconds(1000 * Random.NextDouble());
            else if (LastTrimCount < BatchSize)
                delay = CheckInterval + TimeSpan.FromMilliseconds(100 * Random.NextDouble());
            return Clock.Delay(delay, cancellationToken);
        }
    }
}
