using System.Reflection;

namespace CwLib.AOP
{
 
    public sealed class CwAop
    {
        public static T Create<T>(T instance)
        {
            object proxy = DispatchProxy.Create<T, AopProxy<T>>();

            ((AopProxy<T>)proxy).SetInstance(instance);

            return (T)proxy;
        }
    }
}
