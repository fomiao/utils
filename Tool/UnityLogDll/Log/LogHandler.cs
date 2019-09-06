using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace G
{
    //替换掉原生日志
    class LogHandler : ILogHandler
    {
        static LogHandler s_instance;
        public static ILogHandler unityLogHandler = Debug.unityLogger.logHandler;
        public static void Init()
        {
            if (s_instance == null)
                s_instance = new LogHandler();
        }
        const int FLG_CNT = 5;
        bool[] m_logStackTrace = new bool[FLG_CNT];
        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            ILogFile.LogText(logType,null,format, args);
        }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            ILogFile.LogText(LogType.Exception, null, exception.Message);
        }
    }

}
