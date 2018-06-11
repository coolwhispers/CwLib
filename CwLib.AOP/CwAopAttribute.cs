using System;

namespace CwLib.AOP
{
    public abstract class CwAopAttribute : Attribute, IDisposable
    {
        private int _priority = int.MaxValue;

        public int Priority
        {
            get { return _priority; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("Priority value cannot be less than 0");
                }

                _priority = value;
            }
        }

        protected AopAction AopAction;

        internal void SetAopAction(ref AopAction aopAction)
        {
            AopAction = aopAction;
        }

        public virtual void OnBegin()
        {
        }

        public virtual void OnExecuting()
        {
        }

        public virtual void OnExecuted()
        {
        }

        public virtual void OnResult(ref object result)
        {
        }

        public virtual void OnExecption(Exception e)
        {
        }

        public virtual void Dispose()
        {
        }
    }
}