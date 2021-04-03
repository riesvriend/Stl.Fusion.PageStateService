using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stl.Async;
using Stl.Fusion;
using Stl.Fusion.Authentication;
using Stl.Fusion.Extensions;
using Templates.Blazor2.Abstractions;

namespace Templates.Blazor2.UI.Services
{
    /// <summary>
    /// Service embedding State and action methods for the todopage
    /// </summary>
    [ComputeService(
        typeof(ITodoPageStateService),
        // https://docs.microsoft.com/en-us/aspnet/core/blazor/state-management?view=aspnetcore-5.0&pivots=webassembly
        // ServiceLifetime.Scoped so that each Blazor circuit/session gets to own its own instance
        // TODO: On webassembly this needs to map to Singleton
        // BUG: Despite the lifetime of 'Scoped' the service behaves as if its transient: clicking the 'Load More' button 
        //  shows that the _state is reset
        Lifetime = ServiceLifetime.Scoped)]
    public class TodoPageStateService : ITodoPageStateService
    {
        private readonly ITodoService _todoService;

        private TodoPageState _state;

        public TodoPageStateService(ITodoService todoService)
        {
            _todoService = todoService;
            _state = new TodoPageState();

            Debug.WriteLine($"TodoPageStateService recreated. This should only happen once per session/circuit...");
        }

        /// <summary>
        /// Returns newly computed state
        /// </summary>
        public virtual async Task<TodoPageState> Get(Session session, CancellationToken cancellationToken = default)
        {
            var currentPageSize = _state.PageRef.Count;
            var currentItemsPlusOne = _state.PageRef with { Count = currentPageSize + 1 };
            var items = await _todoService.List(session, currentItemsPlusOne, cancellationToken);
            var hasMore = items.Length > currentPageSize;
            if (hasMore)
                items = items[0..currentPageSize];
            _state = _state with { Items = items, HasMore = hasMore, LastStateUpdateTimeUtc = DateTime.UtcNow };
            return _state;
        }

        /// <summary>
        /// Updates the state and requeries
        /// </summary>
        public virtual Task LoadMore(LoadMoreCommand command, CancellationToken cancellationToken = default)
        {
            var session = command.Session;

            if (Computed.IsInvalidating()) {
                Get(session, cancellationToken: default).Ignore();
                return Task.CompletedTask;
            }
            
            // For now Get() will ingore the next record marker and always fetch a single page from the start
            // so we just increase the page size and refetch it entirely.
            _state = _state with { PageRef = _state.PageRef with { Count = _state.PageRef.Count * 2 } };

            return Task.CompletedTask;
        }
    }
}
