using UnityEngine;
using System.Collections;

public class LogCapturer : MonoBehaviour 
{
    public static LogCapturer Instance;

    public UITextList textList;
    public UIPanel panel;

    public static void Show()
    {
        Instance.panel.alpha = 1f;
    }

    public static void Hide()
    {
        Instance.panel.alpha = 0f;
    }

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        Application.logMessageReceived += OnLogMessageReceived;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= OnLogMessageReceived;
    }

    void OnLogMessageReceived (string condition, string stackTrace, LogType type)
    {
        string color = "";
        string stackTraceMessage = "";
        switch (type)
        {
            case LogType.Log:
                color = "ffffff";
                break;

            case LogType.Warning:
                color = "ffff00";
                break;

            default:
                color = "ff0000";
                stackTraceMessage = stackTrace;
                break;
        }

        textList.Add(string.Format("[{0}]{1}\n{2}[-]",color, condition, stackTraceMessage));
    }
}
