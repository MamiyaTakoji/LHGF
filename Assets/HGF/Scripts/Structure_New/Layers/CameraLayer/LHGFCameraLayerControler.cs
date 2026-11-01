using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LHGFCameraLayerControler : MonoBehaviour
{
    public Camera camera;
    public GameObject FreeCanvasCameraRender;
    public Vector3 defaultPos = new Vector3(0, 0, 0);
    public float defaultSize = 540;
    public float defaultRotation = 0;
    void Start()
    {
        defaultPos.z = transform.position.z;
        if (camera == null)
        {
            camera = GetComponent<Camera>();
        }
    }
    public void Reset()
    {
        transform.position = defaultPos;
        transform.localEulerAngles = new Vector3(0, 0, defaultRotation);
        camera = GetComponent<Camera>();
        camera.orthographicSize = defaultSize;
    }
    // Update is called once per frame
    void Update()
    {

    }
}
