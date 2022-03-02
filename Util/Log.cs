using System;
using System.Collections.Generic;
using BepInEx.Logging;

namespace Bottleneck.Util
{
    public static class Log
    {
        public static ManualLogSource logger;

        public static void Debug(string message)
        {
#if DEBUG
            logger.LogDebug($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
#endif
        }

        public static void Info(string message)
        {
            logger.LogInfo($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
        }

        public static void Warn(string message)
        {
            logger.LogWarning($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
        }

        public static void LogAndPopupMessage(string message)
        {
            UIRealtimeTip.Popup(message, false);
            logger.LogInfo($"POPUP: '{message}'");
        }
    }
}