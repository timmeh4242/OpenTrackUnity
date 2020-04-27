using UnityEngine;
using UnityEngine.UI;

public class DebugGyroText : MonoBehaviour
{
    public GyroBehaviour GyroBehaviour;

    public Text RawText;
    public Text CalibratedText;

    private void Reset()
    {
        if(GyroBehaviour == null && !Application.isPlaying)
        { GyroBehaviour = FindObjectOfType<GyroBehaviour>(); }
    }

    void Update()
    {
        RawText.text = $"R: X:{GyroBehaviour.RawEulers.x.ToString("F2")}, Y:{GyroBehaviour.RawEulers.y.ToString("F2")}, Z:{GyroBehaviour.RawEulers.z.ToString("F2")}";
        CalibratedText.text = $"C: X:{GyroBehaviour.CalibratedEulers.x.ToString("F2")}, Y:{GyroBehaviour.CalibratedEulers.y.ToString("F2")}, Z:{GyroBehaviour.CalibratedEulers.z.ToString("F2")}";
    }
}
