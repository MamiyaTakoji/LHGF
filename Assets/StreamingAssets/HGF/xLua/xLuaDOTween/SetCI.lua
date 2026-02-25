--这个lua文件用于实现立绘的切换
function Set(_self)
    local Dictionary_String_String = CS.System.Collections.Generic.Dictionary(CS.System.String, CS.System.String)
    local dict = Dictionary_String_String()
    local Config = _self.Config
    dict:Add("Time", "1")
    for k,v in pairs(Config) do
        local success, Value = Config:TryGetValue(k)
        if success then
            dict:set_Item(k, Value)
        end
    end
    _self.Config = dict
end

function OnPlay(_self)
    local config = _self.Config
    --local Sprite = CS.LHGFData.ImageLayer.
    --LoadTextureByIO("F:/Unity/LHGFReBulided/Assets/StreamingAssets/HGF/Texture2D/Portrait/Illustrater/Exciting.png")
    --复制一份一模一样的游戏对象作为游戏对象的子对象
    local parent = _self.G:GetComponent(typeof(CS.UnityEngine.Transform))
    local TempG = CS.UnityEngine.Object.Instantiate(_self.G,parent)
    TempG:GetComponent(typeof(CS.UnityEngine.Transform)).position = 
    _self.G:GetComponent(typeof(CS.UnityEngine.Transform)).position
    local FromDic = ImageCILayer.dataLayer.characterData.CharacterID2CharacterInfo
    local characterId = config:get_Item("CharacterID")
    local _From = FromDic:get_Item(characterId)
    local From = _From.From
    local _ResourcePath = ImageCILayer.dataLayer.characterData.CharacterInfo.dataDict:get_Item(From)
    local ResourcePath = _ResourcePath:get_Item("ResourcesPath")
    local CIName = config:get_Item("ImageCharacterIllustration")
    local CIPath = CS.System.IO.Path.Combine(CS.LHGFData.Utils.ResoucePaths.PortraitPath, ResourcePath, CIName)
    local Sprite = CS.LHGFData.ImageLayer.LoadTextureByIO(CIPath)
    local Image = ImageCILayer.ImageCILayerControler.CharacterImageDic:get_Item(characterId)
    Image:GetComponent(typeof(CS.UnityEngine.UI.Image)).sprite = Sprite
    --CS.UnityEngine.Debug.Log(CIPath)
    local sequence = CS.DG.Tweening.DOTween.Sequence()
    local graphic = TempG:GetComponent(typeof(CS.UnityEngine.UI.MaskableGraphic))
    local Time = CS.TypeTransform.string2float(config:get_Item("Time"))
    local animation =  graphic:DOFade(0, Time)
    CS.SequenceExtensions.Append(sequence, animation)
    --CS.UnityEngine.Debug.Log(sequence)
    --CS.UnityEngine.Object.Destory(TempG)
    sequence:OnComplete(
    function()
             CS.UnityEngine.Object.Destroy(TempG)
         end
    )
    -- sequence.onComplete = sequence.onComplete +
    -- (
    --     function()
    --         CS.UnityEngine.Object.Destory(TempG)
    --     end
    -- )
    _self.sequence = sequence
end

function OnLoad(_self)
local config = _self.Config
    --local Sprite = CS.LHGFData.ImageLayer.
    --LoadTextureByIO("F:/Unity/LHGFReBulided/Assets/StreamingAssets/HGF/Texture2D/Portrait/Illustrater/Exciting.png")
    --复制一份一模一样的游戏对象作为游戏对象的子对象
    --local parent = _self.G:GetComponent(typeof(CS.UnityEngine.Transform))
    --local TempG = CS.UnityEngine.Object.Instantiate(_self.G,parent)
    --TempG:GetComponent(typeof(CS.UnityEngine.Transform)).position = 
    --_self.G:GetComponent(typeof(CS.UnityEngine.Transform)).position
    local FromDic = ImageCILayer.dataLayer.characterData.CharacterID2CharacterInfo
    local characterId = config:get_Item("CharacterID")
    local _From = FromDic:get_Item(characterId)
    local From = _From.From
    local _ResourcePath = ImageCILayer.dataLayer.characterData.CharacterInfo.dataDict:get_Item(From)
    local ResourcePath = _ResourcePath:get_Item("ResourcesPath")
    local CIName = config:get_Item("ImageCharacterIllustration")
    local CIPath = CS.System.IO.Path.Combine(CS.LHGFData.Utils.ResoucePaths.PortraitPath, ResourcePath, CIName)
    local Sprite = CS.LHGFData.ImageLayer.LoadTextureByIO(CIPath)
    local Image = ImageCILayer.ImageCILayerControler.CharacterImageDic:get_Item(characterId)
    Image:GetComponent(typeof(CS.UnityEngine.UI.Image)).sprite = Sprite
    --CS.UnityEngine.Debug.Log(CIPath)
    --local sequence = CS.DG.Tweening.DOTween.Sequence()
    --local graphic = TempG:GetComponent(typeof(CS.UnityEngine.UI.MaskableGraphic))
    --local Time = CS.TypeTransform.string2float(config:get_Item("Time"))
    --local animation =  graphic:DOFade(0, Time)
    --CS.SequenceExtensions.Append(sequence, animation)
    --CS.UnityEngine.Debug.Log(sequence)
    --CS.UnityEngine.Object.Destory(TempG)
    --sequence:OnComplete(
    --function()
    --         CS.UnityEngine.Object.Destory(TempG)
    --     end
    --)
    -- sequence.onComplete = sequence.onComplete +
    -- (
    --     function()
    --         CS.UnityEngine.Object.Destory(TempG)
    --     end
    -- )
    --_self.sequence = sequence
end

Set(self)
local Config = self.Config
local success, Value = Config:TryGetValue("Mode")
if success then
    if  Value == "OnPlay" then
        OnPlay(self)
    elseif Value == "OnLoad" then
        OnLoad(self)
    end
end