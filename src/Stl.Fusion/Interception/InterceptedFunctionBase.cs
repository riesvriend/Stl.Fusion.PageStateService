using System;
using System.Threading.Tasks;
using Stl.Async;

namespace Stl.Fusion.Interception
{
    public abstract class InterceptedFunctionBase<TOut> : FunctionBase<InterceptedInput, TOut>
    {
        public InterceptedMethodDescriptor Method { get; }
        protected ComputedOptions Options { get; }

        protected InterceptedFunctionBase(InterceptedMethodDescriptor method, IServiceProvider services)
            : base(services)
        {
            Method = method;
            Options = method.Options;
        }

        public override string ToString()
        {
            var mi = Method.MethodInfo;
            return $"Intercepted:{mi.DeclaringType!.Name}.{mi.Name}";
        }

        // Protected methods

        protected static void SetReturnValue(InterceptedInput input, Result<TOut> output)
        {
            if (input.Method.ReturnsValueTask)
                input.Invocation.ReturnValue =
                    // ReSharper disable once HeapView.BoxingAllocation
                    output.IsValue(out var v)
                        ? ValueTaskEx.FromResult(v)
                        : ValueTaskEx.FromException<TOut>(output.Error!);
            else
                input.Invocation.ReturnValue =
                    output.IsValue(out var v)
                        ? Task.FromResult(v)
                        : Task.FromException<TOut>(output.Error!);
        }
    }
}
