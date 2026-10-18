using UnityEngine;
[ExecuteInEditMode]
public class Letterbox : MonoBehaviour
{
    private const float DEFAULT_RATIO = 1980f / 1080f;
    private int m_ScreenWidth;
    private int m_ScreenHeight;
    private const float DEFAULT_SIZE = 540f;
    public float temp_ratio = -1;
    public bool IsFirstSet = true;
    //需要跟踪的对象
    public GameObject MainCanva;
    Camera camera;
    private void Start()
    {
        m_ScreenWidth = Screen.width;
        m_ScreenHeight = Screen.height;
        Debug.Log(m_ScreenWidth);
        camera = GetComponent<Camera>();
        _ResetWindow();
    }
    private void LateUpdate()
    {
        ResetWindow();
    }
    public void ResetWindow()
    {
        var x = MainCanva.transform.position.x;
        var y = MainCanva.transform.position.y;
        transform.position = new Vector2(x, y);
        //float ratio = Screen.width / Screen.height;
        if (m_ScreenWidth != Screen.width || m_ScreenHeight != Screen.height|| IsFirstSet)
        {
            _ResetWindow();
        }
    }
    public void _ResetWindow()
    {
        float ratio = Screen.width / Screen.height;
        m_ScreenWidth = Screen.width;
        float width = Screen.width;
        float height = Screen.height;
        ratio = width / height;
        if (ratio > DEFAULT_RATIO)
        {
            if (camera.orthographicSize != DEFAULT_SIZE)
                camera.orthographicSize = DEFAULT_SIZE;
        }
        else
        {
            camera.orthographicSize = DEFAULT_SIZE * (DEFAULT_RATIO) / (width / height);
        }
        if (IsFirstSet)
        {
            IsFirstSet = false;
        }
    }
}