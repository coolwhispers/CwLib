using System;

namespace CwLib
{
    /// <summary>
    /// Environment Variable
    /// </summary>
    public class EnvVar
    {
        EnvironmentVariableTarget _target;

        public EnvVar() : this(EnvironmentVariableTarget.Process) { }

        public EnvVar(EnvironmentVariableTarget target)
        {
            _target = target;
        }

        /// <summary>
        /// Get or set environment variable
        /// </summary>
        /// <value></value>
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
