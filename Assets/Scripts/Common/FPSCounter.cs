using UnityEngine;

public class FPSCounter : MonoBehaviour
{
    [SerializeField]
    private int _fontSize = 32;
    [SerializeField]
    private Color _normalColor = Color.white;
    [SerializeField]
    private Color _lowColor = Color.red;
    [SerializeField]
    private float _lowFps = 30.0f;

    private float _deltaTime = 0.0f;

    private void Update()
    {
        _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        float msec = _deltaTime / Time.timeScale * 1000.0f;
        float fps = Time.timeScale / _deltaTime;

        string text = string.Format("{0:0.00} ms ({1:0.00} fps)", msec, fps);

        Rect rect = new()
        {
            x = Screen.width
        };

        GUIStyle style = new()
        {
            alignment = TextAnchor.UpperRight,
            fontSize = _fontSize
        };
        style.normal.textColor = fps >= _lowFps ? _normalColor : _lowColor;

        GUI.Label(rect, text, style);
    }
}