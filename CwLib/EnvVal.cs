using System;

namespace CwLib
{
    /// <summary>
    /// Load or set Environment Var
    /// </summary>
    public class EnvVar
    {
        EnvironmentVariableTarget _target;

        public EnvVar(EnvironmentVariableTarget target)
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
