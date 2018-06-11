using System.Reflection;

namespace CwLib.AOP
{
    public class AopAction
    {
        internal AopAction(MethodInfo targetMethod, object[] args)
        {
            TargetMethod = targetMethod;
            Args = args;
            ExecuteMethod = EcecuteType.All;
        }

        internal bool CanExecuteMethod()
        {
            return (ExecuteMethod & EcecuteType.Method) == EcecuteType.Method;
        }

        public MethodInfo TargetMethod { get; private set; }

        public object[] Args { get; private set; }

        public EcecuteType ExecuteMethod { get; set; }
    }
}