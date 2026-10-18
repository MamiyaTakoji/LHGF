using UnityEngine;
using UnityEngine.EventSystems;

public class TEXScroll : MonoBehaviour, IDragHandler, IScrollHandler
{
    public bool constrainX = true;
    public bool clamp = true;

    public void OnDrag(PointerEventData eventData)
    {
        var tex = GetComponent<TEXDraw>();
        if (tex)
        {
            Drag(eventData.delta);
        }
    }

    public void OnScroll(PointerEventData eventData)
    {
        var tex = GetComponent<TEXDraw>();
        if (tex)
        {
            var speed = tex.size * -2;
            var scroll = eventData.scrollDelta * speed;
            if (Input.GetKey(KeyCode.LeftShift)) {
                scroll = new Vector2(-scroll.y, 0);
            }
            Drag(scroll);
        }
    }

    public void Drag(Vector2 delta)
    {
        var tex = GetComponent<TEXDraw>();
        if (tex)
        {
            var sc = tex.scrollArea;
            if (constrainX)
                sc.y += delta.y;
            else
                sc.position += delta;
            if (clamp)
            {
                var canvasRect = tex.orchestrator.outputNativeCanvasSize;
                sc.x = Mathf.Clamp(sc.x, -canvasRect.x + tex.orchestrator.canvasRect.width, 0);
                sc.y = Mathf.Clamp(sc.y, 0, canvasRect.y - tex.orchestrator.canvasRect.height);
            }
            tex.scrollArea = sc;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
