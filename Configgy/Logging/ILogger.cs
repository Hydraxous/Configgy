using System;
using System.Collections.Generic;
using System.Text;

namespace Configgy.Logging
{
    internal interface ILogger
    {
        public void Log(object obj);
        public void LogError(object obj);
        public void LogWarning(object obj);
        public void LogException(Exception e);
    }
}
