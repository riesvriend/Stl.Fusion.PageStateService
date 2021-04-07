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

        private static TodoPageStateResponse _state = null!;

        public TodoPageStateService(ITodoService todoService)
        {
            _todoService = todoService;

            if (_state == null)
                _state = new TodoPageStateResponse();

            Debug.WriteLine($"TodoPageStateService recreated. This should only happen once per session/circuit...");
        }

        /// <summary>
        /// Returns newly computed state
        /// </summary>
        public virtual async Task<TodoListStateResponse> GetTodoList(
            TodoListStateRequest request, Session session, CancellationToken cancellationToken = default)
        {
            var pageRef = request.PageRef;
            var currentPageSize = pageRef.Count;
            var currentItemsPlusOne = pageRef with { Count = currentPageSize + 1 };
            // todo: implement filtering
            var items = await _todoService.List(session, currentItemsPlusOne, cancellationToken);
            var hasMore = items.Length > currentPageSize;
            var nextPageRef = pageRef;
            if (hasMore) {
                items = items[0..currentPageSize];
            }
            if (items.Length > 0)
                nextPageRef = pageRef with { AfterKey = items[items.Length].Id };

            var response = new TodoListStateResponse(
                Request: request with { PageRef = nextPageRef },
                Items: items,
                HasMore: hasMore);
            
            return response;
        }

        /// <summary>
        /// Fetches more records in a todo-list
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

            // _state = _state with { PageRef = _state.PageRef with { Count = _state.PageRef.Count * 2 } };

            return Task.CompletedTask;
        }

        public virtual async Task<TodoPageStateResponse> Get(Session session, CancellationToken cancellationToken = default)
        {
            var list1 = await GetTodoList(_state.List1.Request, session, cancellationToken);
            var list2 = await GetTodoList(_state.List2.Request, session, cancellationToken);
            var response = new TodoPageStateResponse(list1, list2, LastStateUpdateTimeUtc: DateTime.UtcNow);
            return response;
        }

    }
}
