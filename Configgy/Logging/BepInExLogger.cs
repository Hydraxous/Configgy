using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Configgy.Logging
{
    internal class BepInExLogger : Configgy.Logging.ILogger
    {
        private BepInEx.Logging.ManualLogSource _logger;

        public BepInExLogger(BepInEx.Logging.ManualLogSource logger)
        {
            _logger = logger;
        }

        public void Log(object obj)
        {
            _logger.LogInfo(obj);
        }

        public void LogError(object obj)
        {
            _logger.LogError(obj);
        }

        public void LogWarning(object obj)
        {
            _logger.LogWarning(obj);
        }

        public void LogException(Exception e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Exception: " + e.Message);
            sb.AppendLine("StackTrace: " + e.StackTrace);
            _logger.LogError(sb.ToString());
            Debug.LogException(e); // Log to Unity as well.
        }
    }
}
