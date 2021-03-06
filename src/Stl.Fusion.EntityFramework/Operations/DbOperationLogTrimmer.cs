using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Stl.Fusion.EntityFramework.Operations
{
    public class DbOperationLogTrimmer<TDbContext> : DbWakeSleepProcessBase<TDbContext>
        where TDbContext : DbContext
    {
        public class Options
        {
            public TimeSpan CheckInterval { get; set; } = TimeSpan.FromMinutes(5);
            public TimeSpan MaxOperationAge { get; set; } = TimeSpan.FromMinutes(6);
            public int BatchSize { get; set; } = 1000;
            public LogLevel LogLevel { get; set; } = LogLevel.Information;
        }

        protected IDbOperationLog<TDbContext> DbOperationLog { get; }
        protected TimeSpan CheckInterval { get; }
        protected TimeSpan MaxCommitAge { get; }
        protected int BatchSize { get; }
        protected int LastTrimCount { get; set; }
        protected Random Random { get; }
        protected LogLevel LogLevel { get; }

        public DbOperationLogTrimmer(Options? options, IServiceProvider services)
            : base(services)
        {
            options ??= new();
            LogLevel = options.LogLevel;

            CheckInterval = options.CheckInterval;
            MaxCommitAge = options.MaxOperationAge;
            BatchSize = options.BatchSize;
            DbOperationLog = services.GetRequiredService<IDbOperationLog<TDbContext>>();
            Random = new Random();
        }

        protected override async Task WakeUp(CancellationToken cancellationToken)
        {
            var minCommitTime = (Clock.Now - MaxCommitAge).ToDateTime();
            LastTrimCount = await DbOperationLog
                .Trim(minCommitTime, BatchSize, cancellationToken)
                .ConfigureAwait(false);

            var logEnabled = LogLevel != LogLevel.None && Log.IsEnabled(LogLevel);
            if (LastTrimCount > 0 && logEnabled)
                Log.Log(LogLevel, "Trimmed {Count} operations", LastTrimCount);
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
