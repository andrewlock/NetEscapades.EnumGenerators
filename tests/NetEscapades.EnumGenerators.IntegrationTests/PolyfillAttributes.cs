#if !NET9_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(
        AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property,
        AllowMultiple = false, Inherited = false)]
    internal sealed class OverloadResolutionPriorityAttribute : Attribute
    {
        public OverloadResolutionPriorityAttribute(int priority) => Priority = priority;
        public int Priority { get; }
    }
}
#endif
