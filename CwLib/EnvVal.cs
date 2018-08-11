using System;

namespace CwLib
{
    /// <summary>
    /// 
    /// </summary>
    public class EnvVal
    {
        EnvironmentVariableTarget _target;

        public EnvVal(EnvironmentVariableTarget target)
        {
            _target = target;
        }

        public string this [string key]
        {
            get
            {
                return Environment.GetEnvironmentVariable(key, _target);
            }
            set
            {
                Environment.SetEnvironmentVariable(key, value, _target);
            }
        }
    }
}
