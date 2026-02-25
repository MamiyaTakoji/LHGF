using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class LHGFVideoAnimationLayerControler : MonoBehaviour
{
    public Dictionary<string, LHGFVideoAnimationPlayer> videoAnimationPlayers = new() { };
    public GameObject videoAnimationPlayer;
}
