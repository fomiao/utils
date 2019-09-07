using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TestLogFile : MonoBehaviour
{
    public bool recordTime;
    public bool replaceLogHander;
    public bool logBattle;
    public bool logSkill;
    public bool logNormal;

    public bool showLog;
    public bool showWarning;
    public bool showError;

    G.LoggerModule battle = new G.LoggerModule("[battle]", G.LoggerModule.ShowLog.All);
    G.LoggerModule skill = new G.LoggerModule("[skill]", G.LoggerModule.ShowLog.All);
    G.LoggerModule normal = G.Logger.instance;

    // Start is called before the first frame update
    void Start()
    {
        var folder = Directory.GetCurrentDirectory()+ "/Logs";         
        G.Logger.Init(replaceLogHander, true, recordTime, folder);
        InvokeRepeating("Test", 0, 0.5f);
    }

    void Test()
    {
        if(logBattle)
            Print(battle);
        if(logSkill)
            Print(skill);
        if(logNormal)
            Print(normal);
    }
    void Print(G.LoggerModule model)
    {
        int show = 0;
        if (showLog)
            show += (int)G.LoggerModule.ShowLog.Log;
        if (showWarning)
            show += (int)G.LoggerModule.ShowLog.Warning;
        if (showError)
            show += (int)G.LoggerModule.ShowLog.Error;

        model.SetShowLog((G.LoggerModule.ShowLog)show);
        model.Log("log");
        model.LogWarning("warning");
        model.LogError("error");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
