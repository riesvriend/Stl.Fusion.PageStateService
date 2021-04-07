using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Stl.CommandR;
using Stl.CommandR.Configuration;
using Stl.Fusion;
using Stl.Fusion.Authentication;
using Stl.Fusion.Blazor;
using Stl.Fusion.Extensions;
using Stl.Text;

namespace Templates.Blazor2.UI.Services
{
    /// <summary>
    /// Live state for the todopage.razor
    /// </summary>
    public interface ITodoPageStateService
    {
        // Queries
        [ComputeMethod]
        Task<TodoPageStateResponse> Get(Session session, CancellationToken cancellationToken = default);

        [ComputeMethod]
        Task<TodoListStateResponse> GetTodoList(TodoListStateRequest request, Session session, CancellationToken cancellationToken = default);

        [CommandHandler]
        Task LoadMore(LoadMoreCommand command, CancellationToken cancellationToken = default);
    }

    public record LoadMoreCommand(string Path, Session Session) : ISessionCommand<Unit>
    {
        public LoadMoreCommand() : this(Path: "",  Session.Null) { }
    }

    /// <summary>
    /// Immutable for optimized razor re-rendering
    /// </summary>
    public record TodoPageStateResponse(
        TodoListStateResponse List1,
        TodoListStateResponse List2,
        // The last fetch timestamp
        DateTime LastStateUpdateTimeUtc)
    {
        public TodoPageStateResponse()
            : this(List1: new TodoListStateResponse(), List2: new TodoListStateResponse(), LastStateUpdateTimeUtc: new DateTime(1999, 12, 31)) { }
    }
}
