using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace G
{
    /// <summary>
    /// 默认日志，使用的是模块default
    /// </summary>
    public static class Logger
    {
        public static LoggerModule instance = new LoggerModule("[default]", LoggerModule.ShowLog.All);
        public static void Init(bool replaceLogHandler, bool logAsync,bool recordTime, string folder)
        {
            ILogFile.Init(replaceLogHandler, logAsync, recordTime, folder);
        }
    }

    public class LoggerModule
    {
        public enum ShowLog
        {
            None = 0,
            Log = 1,    
            Warning = 2,    
            Error = 4,
            Exception = 8,      
            All = 15            //所有日志都显示
        }
        string moduleName;
        ShowLog showLog;
        public LoggerModule(string moduleName, ShowLog showLog)
        {
            this.moduleName = moduleName;
            this.showLog = showLog;
        }

        public void SetShowLog(ShowLog showLog)
        {
            this.showLog = showLog;
        }

        [Conditional("COM_DEBUG")]
        public void Log(string format)
        {
            LogFormat(format);
        }
        [Conditional("COM_DEBUG")]
        public void LogFormat(string format, params object[] message)
        {
            if((ShowLog.Log & showLog) != 0)
                Log(LogType.Log, format, message);
        }
        [Conditional("COM_DEBUG")]
        public void LogWarning(string format)
        {
             LogWarningFormat(format);
        }
        [Conditional("COM_DEBUG")]
        public void LogWarningFormat(string format, params object[] message)
        {
            if ((ShowLog.Warning & showLog) != 0)
                Log(LogType.Warning, format, message);
        }
        public void LogError(string format)
        {
            LogErrorFormat(format);
        }
        public void LogErrorFormat(string format, params object[] message)
        {
            if ((ShowLog.Error & showLog) != 0)
                Log(LogType.Error, format, message);
        }
        public void LogException(Exception exception)
        {
            if ((ShowLog.Exception & showLog) != 0)
            {
                ILogFile.LogText(LogType.Exception, moduleName, exception.Message);
#if COM_DEBUG
                if(Application.isEditor)
                    LogHandler.unityLogHandler.LogException(exception, null);
#endif
            }
        }

        void Log(LogType logType,string format,params object[] argst)
        {
            var text = ILogFile.LogText(logType, moduleName, format, argst);
#if COM_DEBUG
            if (Application.isEditor)
                LogHandler.unityLogHandler.LogFormat(logType, null, text);
#endif
        }

    }
}
