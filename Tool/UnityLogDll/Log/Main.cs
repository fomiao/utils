using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace G
{
    /// <summary>
    /// 可以创建多个日志模块,不同日志模块可以单独开关和设置打印等级，[default]是默认模块日志
    /// unity需要定义DEBUG_LOG，DEBUG_WARNING才会打印对应普通和警告日志，主要是为了打包时取消日志格式化的开销
    /// </summary>
    public static class Logger
    {
        public static LoggerModule instance = new LoggerModule("[default]", LoggerModule.ShowLog.All);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="replaceLogHandler"> 表示是否替换掉原生日志，打包一般需要替换，可以少写一份文件 </param>
        /// <param name="logAsync"> 写文件时是否使用异步方式 </param>
        /// <param name="recordTime"> 日志是否带上时间</param>
        /// <param name="folder">文件保存目录</param>
        public static void Init(bool replaceLogHandler, bool logAsync,bool recordTime, string logFolder)
        {
            ILogFile.Init(replaceLogHandler, logAsync, recordTime, logFolder);
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

        [Conditional("LOG_DEBUG")]
        public void Log(string format)
        {
            if ((ShowLog.Log & showLog) != 0)
                Log(LogType.Log, format);
        }
        [Conditional("LOG_DEBUG")]
        public void LogFormat(string format, params object[] message)
        {
            if((ShowLog.Log & showLog) != 0)
                Log(LogType.Log, format, message);
        }
        [Conditional("LOG_WARNING")]
        public void LogWarning(string format)
        {
            if ((ShowLog.Warning & showLog) != 0)
                Log(LogType.Warning, format);
        }
        [Conditional("LOG_WARNING")]
        public void LogWarningFormat(string format, params object[] message)
        {
            if ((ShowLog.Warning & showLog) != 0)
                Log(LogType.Warning, format, message);
        }
        public void LogError(string format)
        {
            if ((ShowLog.Error & showLog) != 0)
                Log(LogType.Error, format);
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
                if(Application.isEditor)
                    LogHandler.unityLogHandler.LogException(exception, null);
            }
        }

        void Log(LogType logType,string format,params object[] argst)
        {
            var text = ILogFile.LogText(logType, moduleName, format, argst);
            if (Application.isEditor)
                LogHandler.unityLogHandler.LogFormat(logType, null, text);
        }

    }
}
