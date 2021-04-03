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

namespace Templates.Blazor2.Abstractions
{
	/// <summary>
	/// Live state for the todopage.razor
	/// </summary>
	public interface ITodoPageStateService
	{
		// Queries
		[ComputeMethod]
		Task<TodoPageState> Get(Session session, CancellationToken cancellationToken = default);

		[CommandHandler]
		Task LoadMore(LoadMoreCommand command, CancellationToken cancellationToken = default);
	}

	public record LoadMoreCommand(Session Session) : ISessionCommand<Unit>
	{
		public LoadMoreCommand() : this(Session.Null) { }
	}

	/// <summary>
	/// Immutable for optimized razor re-rendering
	/// </summary>
	public record TodoPageState(
		// The currently visible todo items
		Todo[] Items,
		// The next record marker and page size
		PageRef<string> PageRef,
		// If a next record is available
		bool HasMore,
		// The last fetch timestamp
		DateTime LastStateUpdateTimeUtc)
	{
		public TodoPageState() 
			: this(Items: Array.Empty<Todo>(), PageRef: 5, HasMore: false,
				  LastStateUpdateTimeUtc: new DateTime(1999, 12, 31)) { }
	}
}
