using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CwLib.AOP
{
    sealed class AopProxy<T> : DispatchProxy
    {
        T _instance;

        public void SetInstance(T instance)
        {
            _instance = instance;
        }

        protected override object Invoke(MethodInfo targetMethod, object[] args)
        {
            object result = null;

            var aopAction = new AopAction(targetMethod, args);

            var attrs = (IEnumerable<CwAopAttribute>)targetMethod.GetCustomAttributes(typeof(CwAopAttribute));

            foreach (var attr in attrs)
            {
                attr.SetAopAction(ref aopAction);
            }

            var attrsAsc = attrs.OrderBy(x => x.Priority);
            var attrsDesc = attrs.OrderByDescending(x => x.Priority);

            try
            {
                foreach (var attr in attrsAsc)
                {
                    attr.OnBegin();
                }

                if (aopAction.CanExecuteMethod())
                {
                    foreach (var attr in attrsAsc)
                    {
                        attr.OnExecuting();
                    }

                    try
                    {
                        result = targetMethod.Invoke(_instance, args);
                    }
                    catch (Exception e)
                    {
                        foreach (var attr in attrs)
                        {
                            attr.OnExecption(e);
                        }

                        throw;
                    }

                    foreach (var attr in attrsDesc)
                    {
                        attr.OnExecuted();
                    }
                }

                foreach (var attr in attrsDesc)
                {
                    attr.OnResult(ref result);
                }

                return result;
            }
            finally
            {
                foreach (var attr in attrs)
                {
                    attr.Dispose();
                }
            }
        }
    }

}