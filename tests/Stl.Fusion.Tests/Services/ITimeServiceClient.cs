using System;
using System.Threading;
using System.Threading.Tasks;
using RestEase;
using Stl.Fusion.Client;

namespace Stl.Fusion.Tests.Services
{
    [RestEaseReplicaService(typeof(IClientTimeService), Scope = ServiceScope.ClientServices)]
    [BasePath("time")]
    public interface ITimeServiceClient
    {
        [Get("getTime")]
        Task<DateTime> GetTime(CancellationToken cancellationToken = default);
        [Get("getFormattedTime")]
        Task<string?> GetFormattedTime(string format, CancellationToken cancellationToken = default);
    }

    public interface IClientTimeService : ITimeService { }
}
