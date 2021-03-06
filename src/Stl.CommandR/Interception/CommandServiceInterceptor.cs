using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Stl.CommandR.Configuration;
using Stl.CommandR.Internal;
using Stl.Interception.Interceptors;

namespace Stl.CommandR.Interception
{
    public class CommandServiceInterceptor : InterceptorBase
    {
        public new class Options : InterceptorBase.Options
        { }

        protected ICommander Commander { get; }

        public CommandServiceInterceptor(
            Options options,
            IServiceProvider services,
            ILoggerFactory? loggerFactory = null)
            : base(options, services, loggerFactory)
            => Commander = services.GetRequiredService<ICommander>();

        protected override Action<IInvocation> CreateHandler<T>(
            IInvocation initialInvocation, MethodDef methodDef)
            => invocation => {
                if (!(invocation.Proxy is ICommandService)) {
                    invocation.Proceed();
                    return;
                }
                var command = (ICommand) invocation.Arguments[0];
                var cancellationToken1 = (CancellationToken) invocation.Arguments[^1];
                var context = CommandContext.Current;
                if (ReferenceEquals(command, context?.UntypedCommand))
                    invocation.Proceed();
                else {
                    invocation.ReturnValue = methodDef.IsAsyncVoidMethod
                        ? Commander.Call(command, false, cancellationToken1)
                        : Commander.Call((ICommand<T>) command, false, cancellationToken1);
                }
            };

        protected override MethodDef? CreateMethodDef(MethodInfo methodInfo, IInvocation initialInvocation)
        {
            var methodDef = new CommandHandlerMethodDef(this, methodInfo);
            return methodDef.IsValid ? methodDef : null;
        }

        protected override void ValidateTypeInternal(Type type)
        {
            if (typeof(ICommandHandler).IsAssignableFrom(type))
                throw Errors.OnlyInterceptedCommandHandlersAllowed(type);
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Instance | BindingFlags.Static
                | BindingFlags.FlattenHierarchy;
            foreach (var method in type.GetMethods(bindingFlags)) {
                var attr = MethodCommandHandler.GetAttribute(method);
                if (attr == null)
                    continue;

                var methodDef = new CommandHandlerMethodDef(this, method);
                var attributeName = nameof(CommandHandlerAttribute).Replace(nameof(Attribute), "");
                if (!methodDef.IsValid) // attr.IsEnabled == false
                    Log.Log(ValidationLogLevel,
                        "- {Method}: has [{Attribute}(false)]", method.ToString(), attributeName);
                else
                    Log.Log(ValidationLogLevel,
                        "+ {Method}: [{Attribute}(" +
                        "Priority = {Priority}" +
                        ")]", method.ToString(), attributeName, attr.Priority);
            }
        }
    }
}
