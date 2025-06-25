using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Img_CharactersShower : MonoBehaviour
{
    // Start is called before the first frame update
    public Dictionary<string,GameObject>characterImgs = new Dictionary<string,GameObject>();
    public Tweener InitCharacter(string ResoucerePath,string characterId,string animationMessage)
    {
        if (characterImgs.ContainsKey(characterId) == true)
        {
            return null;
        }
        var _GameObj = Resources.Load<GameObject>("HGF/Img-Character");
        Sprite character_sprite = Utils.LoadTextureByIO(ResoucerePath);
        _GameObj.GetComponent<Image>().sprite = character_sprite;
        _GameObj.GetComponent<Img_Character_AnimationControler>().Img.sprite = character_sprite;
        //_CameObj.GetComponent<Img_Character_AnimationControler>().Animate_StartOrOutside = animationMessage;
        var G = Instantiate(_GameObj,transform);
        G.GetComponent<ResizeImage>()._ResizeImage();
        G.name = characterId;
        characterImgs.Add(characterId, G);
        Debug.Log(characterId);
        var T = G.GetComponent<Img_Character_AnimationControler>().HandleInOrOutsideMessgae(animationMessage);
        return T;
    }
    public void InitCharacterOnLoad(string ResoucerePath, string characterId, string animationMessage)
    {
        if (characterImgs.ContainsKey(characterId) == true)
        {
            return;
        }
        var _GameObj = Resources.Load<GameObject>("HGF/Img-Character");
        Sprite character_sprite = Utils.LoadTextureByIO(ResoucerePath);
        _GameObj.GetComponent<Image>().sprite = character_sprite;
        _GameObj.GetComponent<Img_Character_AnimationControler>().Img.sprite = character_sprite;
        //_CameObj.GetComponent<Img_Character_AnimationControler>().Animate_StartOrOutside = animationMessage;
        var G = Instantiate(_GameObj, transform);
        G.GetComponent<ResizeImage>()._ResizeImage();
        G.name = characterId;
        characterImgs.Add(characterId, G);
        Debug.Log(characterId);
        G.GetComponent<Img_Character_AnimationControler>().HandleInOrOutsideMessgaeOnLoad(animationMessage);
    }
    public void SetPortrait(string ResoucerePath, string characterId)
    {
        Sprite character_sprite = Utils.LoadTextureByIO(ResoucerePath);
        characterImgs[characterId].GetComponent<Img_Character_AnimationControler>().Img.sprite = character_sprite;
    }
}
