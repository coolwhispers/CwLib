using System;

namespace CwLib.AOP
{
    [Flags]
    public enum EcecuteType
    {
        None = 0,
        Method = 1,
        All = Method,
    }
}