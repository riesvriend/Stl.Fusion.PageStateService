using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Stl.DependencyInjection;

namespace Templates.Blazor2.UI.Services
{
    [Service(Lifetime = ServiceLifetime.Scoped)]
    public class SimpleTodoListStateService
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
    }
}
