using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace G
{
    /// <summary>
    /// 日志文件超过一定数量会清理掉旧的
    /// </summary>
    abstract class ILogFile : MonoBehaviour
    {
        static ILogFile instance;
        const int MAX_FILE_CNT = 100;   //超过这个文件就会删除
        protected StreamWriter m_sw;
        protected bool recordTime;      //是否记录时间
        const int FLG_CNT = 5;
        bool[] m_logStackTrace = new bool[FLG_CNT];

        public static void Init(bool replaceLogHandler,bool logAsync,bool recordTime,string folder)
        {
            if (instance != null)
                return;

            var go = new GameObject("LogFile");
            GameObject.DontDestroyOnLoad(go);
            if (logAsync)
                instance = go.AddComponent<LogHandelFileAsyn>();
            else
                instance = go.AddComponent<LogHandleFileSync>();

            instance.InitImpl(folder);
            instance.recordTime = recordTime;
            Application.logMessageReceived += instance.LogCallback;
            //是否替换LogHandler
            if(replaceLogHandler)
                LogHandler.Init();
        }

        public static string LogText(LogType logType,string title,string format, params object[] args)
        {
            if (instance == null)
                return "LogFile==null";
            string stackTrace = null;
            if (instance.m_logStackTrace[(int)logType])
                stackTrace = new System.Diagnostics.StackTrace(true).ToString();
            if (args != null && args.Length > 1)
                format = string.Format(format, args);
            var text = GetLogText(title, format, stackTrace, logType);
            instance.LogImpl(text);
            return text;
        }

        static string GetLogText(string title, string condition, string stackTrace, LogType logType)
        {
            int index = (int)logType;
            if (index > 4)
                return "logType Error" + logType.ToString();

            string timeText = instance.recordTime ? DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") : "";
            return string.Format("{0} {1} {2} \r\n", title ?? "", timeText, condition, stackTrace);
        }

        protected virtual void OnDestroy()
        {
            Application.logMessageReceived -= instance.LogCallback;
        }

        void InitImpl(string folder)
        {
            for (int i = 0; i < FLG_CNT; i++)
                m_logStackTrace[i] = Application.GetStackTraceLogType((LogType)i) != StackTraceLogType.None;

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            ClearTempFile(folder);

            string fileTitle = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".txt";
            m_sw = new StreamWriter(folder + "/" + fileTitle, false);
        }

        void ClearTempFile(string folder)
        {
            var files = Directory.GetFiles(folder, "*.txt");
            Array.Sort(files);
            for (int i = 0; i < files.Length; i++)
            {
                if (files.Length - i > MAX_FILE_CNT)
                    File.Delete(files[i]);
                else
                    break;
            }
        }

        static string[] typeNames = new string[] { "Error:", "Assert:", "Warning:", "Log:", "Exception: " };

        void LogCallback(string condition, string stackTrace, LogType logType)
        {
            LogImpl(GetLogText(null,condition, stackTrace, logType));
        }
        protected abstract void LogImpl(string text);

    }

    class LogHandleFileSync : ILogFile
    {
        protected override void LogImpl(string text)
        {
            m_sw.WriteLine(text);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_sw.Close();
        }
    }

    class LogHandelFileAsyn : ILogFile
    {
        bool m_stop;
        Queue<string> m_queue_cache = new Queue<string>();
        Queue<string> m_queue_loging = new Queue<string>();
        object m_lock = new object();
        Thread m_thread;

        public LogHandelFileAsyn()
        {
            m_thread = new Thread(Run);
            m_thread.Start();
        }

        protected override void LogImpl(string text)
        {
            lock (m_lock)
            {
                m_queue_cache.Enqueue(text);
            }
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_stop = true;
        }

        Queue<string> Swap()
        {
            Queue<string> tempQueue;
            tempQueue = m_queue_cache;
            m_queue_cache = m_queue_loging;
            m_queue_loging = tempQueue;
            return tempQueue;
        }

        void Run()
        {
            while (true)
            {
                if (m_stop)
                    break;
                Queue<string> queueLog;
                lock (m_lock)
                {
                    queueLog = Swap();
                }

                while (queueLog.Count > 0)
                {
                    var curText = queueLog.Dequeue();
                    m_sw.WriteLine(curText);
                    m_sw.Flush();
                }
                Thread.Sleep(10);
            }
            m_sw.Close();
        }
    }
}
