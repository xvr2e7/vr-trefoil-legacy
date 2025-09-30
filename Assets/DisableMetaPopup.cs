using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DisableMetaPopup : MonoBehaviour
{
    private const string TelemetryEnabledKey = "OVRTelemetry.TelemetryEnabled";

    [ContextMenu("DisablePopup")]
    public void DisablePopup()
    {
        #if UNITY_EDITOR
        EditorPrefs.SetBool(TelemetryEnabledKey, false);
        Debug.Log("Telemetry popup has been disabled.");
        #endif
    }
}