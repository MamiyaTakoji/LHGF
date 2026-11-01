using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TEXDraw)), ExecuteAlways]
[AddComponentMenu("TEXDraw/TEXEmbed UI", 5)]
public class TEXEmbed : MonoBehaviour
{
    public enum EmbedMethod
    {
        byIndex = 0,
        byObjectName = 1,
    }
    public List<RectTransform> objects = new List<RectTransform>();
    public EmbedMethod embedMethod = EmbedMethod.byIndex;
    protected TEXDraw target;

    protected virtual void OnEnable()
    {
        target = GetComponent<TEXDraw>();
        if (target != null)
            target.hook += UpdateTexDraw;
    }

    protected virtual void OnDisable()
    {
        if (target != null)
            target.hook -= UpdateTexDraw;
    }



    protected void UpdateTexDraw(TexDrawLib.DirtyLevel dirty)
    {
        if (dirty < TexDrawLib.DirtyLevel.Render) return;
        var links = target.orchestrator.rendererState.vertexLinks;
        switch (embedMethod)
        {
            case EmbedMethod.byIndex:
                ArrangeByIndex(links);
                break;
            case EmbedMethod.byObjectName:
                ArrangeByObject(links);
                break;
        }
    }

    protected void ArrangeByIndex(List<(string key, Rect area)> links)
    {
        for (int i = 0; i < objects.Count; i++)
        {
            if (!objects[i]) continue;
            if (i < links.Count)
            {
                if (!objects[i].gameObject.activeSelf)
                    objects[i].gameObject.SetActive(true);
                objects[i].anchoredPosition = links[i].area.center;
                objects[i].sizeDelta = links[i].area.size;
            } 
            else
            {
                if (objects[i].gameObject.activeSelf)
                    objects[i].gameObject.SetActive(false);
            }
        }
    }

    protected void ArrangeByObject(List<(string key, Rect area)> links)
    {
        for (int i = 0; i < objects.Count; i++)
        {
            if (!objects[i]) continue;
            var j = links.FindIndex(x => x.key == objects[i].gameObject.name);
            if (j >= 0)
            {
                if (!objects[i].gameObject.activeSelf)
                    objects[i].gameObject.SetActive(true);
                objects[i].anchoredPosition = links[j].area.center;
                objects[i].sizeDelta = links[j].area.size;
            }
            else
            {
                if (objects[i].gameObject.activeSelf)
                    objects[i].gameObject.SetActive(false);
            }
        }
    }

}
