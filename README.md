Proof of concept extracting private Blazor component into a Circuit/Session bound Compute service powered by [Stl.Fusion](https://github.com/servicetitan/Stl.Fusion).

See the updates to sample project [Blazorise](https://github.com/riesvriend/Stl.Fusion.PageStateService/tree/master/samples/Blazorise). 

Goals
* End-to-end Real Time, from database to multiple hosts and multiple Blazor clients; thanks to Fusion.
* The PageState fusion service serves up an immutable object than can efficiently be rendered through fusion LiveComponents, plus child components that check for reference equality of the child nodes from the state passed to them
* Isolation of UI state and update logic into an live/observable model. 
* Efficient rendering without the need to implement Blazor's [ShouldComponentRender](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/rendering?view=aspnetcore-5.0) nor to strategically call [StateHasChanged](https://docs.microsoft.com/en-us/aspnet/core/blazor/components/rendering?view=aspnetcore-5.0).
* This translates to good scalability of complex user interfaces. All components can be simple, 'Pure'; free of complex code and side-effects or inter-component rendering dependencies. All logic is testable and delegated away into the Actions in the stores.
* Testing the stores is a good substitute for testing the actual Blazor components and much easier.
  
Done:
* created interface and DTOs for the todopage state: [ITodoPageStateService and TodoPageState](https://github.com/riesvriend/Stl.Fusion.PageStateService/blob/master/samples/Blazorise/UI/Services/ITodoPageStateService.cs) and [TodoPageStore](https://github.com/riesvriend/FusionAndCortex/blob/master/samples/Blazorise/UI/Stores/TodoPageStore.cs) 
* [TodoPageStateService](https://github.com/riesvriend/Stl.Fusion.PageStateService/blob/master/samples/Blazorise/UI/Services/TodoPageStateService.cs) provides a live instance to the todopage component. It also manipulates the state through Commands, such as LoadMore.
* TodoPageState contains the client-side state for the todo page, such as current page size and page marker. It 
  also depends on the TodoService for live requerying of the currently visible todo-items
* Started extracted the logic from the [todopage.razor](https://github.com/riesvriend/Stl.Fusion.PageStateService/blob/master/samples/Blazorise/UI/Pages/TodoPage.razor), making it a render-only component


 Issues and Todos:
 1. The state behaves as  ServiceLifetime.Transient and is reset after each request, even though it is marked as ServiceLifetime.Scoped [here](https://github.com/riesvriend/Stl.Fusion.PageStateService/blob/21fb94ccf5fd39b6bfe361217607a1dc9b630922/samples/Blazorise/UI/Services/TodoPageStateService.cs#L24) as per [MS Docs](https://docs.microsoft.com/en-us/aspnet/core/blazor/state-management?view=aspnetcore-5.0&pivots=webassembly). Due to this bug the PageSize state is lost on every request and loading more pages does not work.
 2. Finish extracting all logic and commands in todopage.razor and todoitem.razor
 2. Testers still to be implemented.
 
