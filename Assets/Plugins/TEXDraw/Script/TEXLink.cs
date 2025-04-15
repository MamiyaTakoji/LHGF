using UnityEngine;
#if TEXDRAW_INPUTSYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(TEXDraw))]
[AddComponentMenu("TEXDraw/TEXLink UI", 4)]
public class TEXLink : TEXLinkBase
{
   protected override int SamplePointerStatus(int linkIdx)
   {
       Vector2 o;
       if (linkIdx >= m_Orchestrator.rendererState.vertexLinks.Count)
           return 0;

       for (int i = 0; i < input_PressPos.Count; i++)
       {
           var screenPos = input_PressPos[i];

           if (RectTransformUtility.ScreenPointToLocalPointInRectangle
                   ((RectTransform)transform, screenPos, triggerCamera, out o))
           {
               if (m_Orchestrator.rendererState.IsPointHitLink(linkIdx, o, interactionPadding))
                   return 2;
           }
       }

#if TEXDRAW_INPUTSYSTEM
        if (Mouse.current != null && RectTransformUtility.ScreenPointToLocalPointInRectangle
        ((RectTransform)transform, input_HoverPos, triggerCamera, out o))
        {
            if (m_Orchestrator.rendererState.IsPointHitLink(linkIdx, o, interactionPadding))
                return 1;
        }
#else
        if (Input.mousePresent && RectTransformUtility.ScreenPointToLocalPointInRectangle
                    ((RectTransform)transform, input_HoverPos, triggerCamera, out o))
        {
            if (m_Orchestrator.rendererState.IsPointHitLink(linkIdx, o, interactionPadding))
                return 1;
        }
#endif
        return 0;
   }

   protected override void OnEnable()
   {
       base.OnEnable();
       target = GetComponent<TEXDraw>();
       var tex = (TEXDraw)target;
       triggerCamera = tex.canvas.worldCamera;
   }

   protected override void Update()
   {
       if (((TEXDraw)target).raycastTarget)
           base.Update();
   }
    public void OnDrawGizmosSelected()
    {
        if (!isActiveAndEnabled)
            return;
        var dc = Target.orchestrator;
        if (dc == null)
            return;
        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;
        var p = interactionPadding;
        for (int i = 0; i < dc.rendererState.vertexLinks.Count; i++)
        {
            Rect r = dc.rendererState.vertexLinks[i].area;
            r = new Rect(r.x - p.x, r.y - p.y, r.width + p.x * 2, r.height + p.y * 2);
            Gizmos.DrawWireCube((r.center), (r.size));
        }
    }
}
