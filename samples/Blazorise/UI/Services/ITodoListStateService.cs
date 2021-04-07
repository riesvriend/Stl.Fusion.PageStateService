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
using Templates.Blazor2.Abstractions;

namespace Templates.Blazor2.UI.Services
{
    public class ITodoListStateService
    {
    }

    /// <summary>
    /// Immutable for optimized razor re-rendering
    /// </summary>
    public record TodoListStateRequest(
        // The filter criteria set by the user
        TodoListFilter Filter,
        PageRef<string> PageRef)
    {
        public TodoListStateRequest()
            : this(Filter: new TodoListFilter(), PageRef: 5)
        { }
    }

    public record TodoListStateResponse(
        // Cleaned up request (next record marker updated)
        TodoListStateRequest Request,
        // The currently visible todo items
        Todo[] Items,
        //// The next record marker and page size
        //string NextRecordMarker,
        // If a next record is available
        bool HasMore)
    {
        public TodoListStateResponse()
            : this(Request: new TodoListStateRequest(), Items: Array.Empty<Todo>(),/* NextRecordMarker: string.Empty,*/ HasMore: false)
        { }
    }


    public enum CompletionStateEnum { Any, Todo, Done }

    public record TodoListFilter(string FilterText, CompletionStateEnum CompletionState)
    {
        public TodoListFilter() : this(FilterText: string.Empty, CompletionStateEnum.Any) { }
    }
}
