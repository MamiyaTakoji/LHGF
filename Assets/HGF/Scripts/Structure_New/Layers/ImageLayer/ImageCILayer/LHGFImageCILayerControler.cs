using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHGFData;
public class LHGFImageCILayerControler : MonoBehaviour
{
    public ImageCILayer imageCILayer;
    public GameObject CharacterImagePerfab;
    public Dictionary<string, GameObject> CharacterImageDic = new() { };
}
