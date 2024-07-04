using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class dynamicLog : MonoBehaviour
{
    // Start is called before the first frame update
    public Text text;
    public ScrollRect scrollRect;

    void Awake()
    {
        Application.logMessageReceived += HandleLogMessage;
        string[] _textLines = new string[256];
        for (int i = 0; i < _textLines.Length; i++) {
            _textLines[i] = " ";
        }
        text.text = string.Join("\n", _textLines, 0, _textLines.Length - 1);
        Debug.Log("running at " + Application.dataPath);
    }
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void HandleLogMessage(string logMessage, string stackTrace, LogType logType)
    {
        string[] _textLines = text.text.Split('\n');
        string[] _logLines = logMessage.Split('\n');
        scrollRect.verticalNormalizedPosition = 0f;
        text.text = string.Join("\n", _textLines, _logLines.Length, _textLines.Length - _logLines.Length);
        text.text += "\n" + logMessage;
    }
}
