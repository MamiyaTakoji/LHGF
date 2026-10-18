using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LHGFData.BlackboardLayer;
public class LHGFBlackboardLayerControler : MonoBehaviour
{
    // Start is called before the first frame update
    public LHGFBlackboardControler Blackboard;
    public void OpenBlackboard(BlackboardData data)
    {
        Blackboard.gameObject.SetActive(true);
        //设置一下黑板的位置和大小
        LHGFData.Utils.SetSingleAnchorPoint(Blackboard.GetComponent<RectTransform>(), data.Pos);
        LHGFData.Utils.SetRelativeSize(Blackboard.GetComponent<RectTransform>(), data.Size);
    }
    public void CloseBlackboard()
    {
        Blackboard.gameObject.SetActive(false);
    }
    public Tweener AddContent(BlackboardData data, bool IsOnLoad)
    {
        return Blackboard.AddTextContent(data, IsOnLoad);
    }
    public void WithdrawContent(List<BlackboardData> datas, bool IsClear)
    {
        Blackboard.WithdrawTextContent(datas, IsClear);
    }
    public void Clear()
    {
        Blackboard.Clear();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
