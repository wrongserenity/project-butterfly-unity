using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoggableBase : MonoBehaviour
{
    [SerializeField] bool isLogging = false;

    protected void TryLog(string message, LogType logType = LogType.Log)
    {
        if (!isLogging)
            return;

        switch (logType)
        {
            case LogType.Log:
                Debug.Log(message);
                break;
            case LogType.Error:
                Debug.LogError(message);
                break;
            case LogType.Warning:
                Debug.LogWarning(message);
                break;
            case LogType.Assert:
                Debug.LogAssertion(message);
                break;
            default:
                Debug.LogWarningFormat(this, "Cannot log \"{0}\" because of unsupported log type \"{1}\" ", 
                                                message, logType.ToString());
                break;
        }
        
    }
}
