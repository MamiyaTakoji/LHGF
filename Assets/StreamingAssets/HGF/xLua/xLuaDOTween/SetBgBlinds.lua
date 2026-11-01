function Set(_self)
    local Dictionary_String_String = CS.System.Collections.Generic.Dictionary(CS.System.String, CS.System.String)
    local dict = Dictionary_String_String()
    local Config = _self.Config
    dict:Add("Time", "1.5")
    dict:Add("BlindNum","30")
    dict:Add("N","20")
    dict:Add("colorR", "1")
    dict:Add("colorG", "1")
    dict:Add("colorB", "1")
    dict:Add("colorA", "1")
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
    local parent = _self.G:GetComponent(typeof(CS.UnityEngine.Transform))
    local TempG = CS.UnityEngine.Object.Instantiate(_self.G,parent)
    TempG:GetComponent(typeof(CS.UnityEngine.Transform)).position = 
    _self.G:GetComponent(typeof(CS.UnityEngine.Transform)).position
    TempG:AddComponent(typeof(CS.UnityEngine.UI.HorizontalLayoutGroup))
    local H = TempG:GetComponent(typeof(CS.UnityEngine.UI.HorizontalLayoutGroup))
    H.childControlHeight = true
    H.childControlWidth = true
    local sequence1 = CS.DG.Tweening.DOTween.Sequence()
    local Bs = {}
    local BlindNum = config:get_Item("BlindNum")
    for i = 1, BlindNum, 1 do
        local B = CS.UnityEngine.GameObject()
        B.transform.parent = TempG:GetComponent(typeof(CS.UnityEngine.Transform))
        B:AddComponent(typeof(CS.UnityEngine.UI.Image))
        local blindImage = B:GetComponent(typeof(CS.UnityEngine.UI.Image))
        blindImage.color = CS.UnityEngine.Color(0, 0, 0, 1)
        local pos = B:GetComponent(typeof(CS.UnityEngine.RectTransform)).anchoredPosition3D
        pos.z = 0
        B:GetComponent(typeof(CS.UnityEngine.RectTransform)).anchoredPosition3D = pos
        B:GetComponent(typeof(CS.UnityEngine.RectTransform)).localEulerAngles = CS.UnityEngine.Vector3(0,90,0)
        Bs[i] = B
    end
    local Time = CS.TypeTransform.string2float(config:get_Item("Time"))
    local N = CS.TypeTransform.string2float(config:get_Item("N"))
    --设置百叶窗旋转
    for i = 1, BlindNum, 1 do
         --Wait就是啥也不做
         local WaitTime = Time/BlindNum*(i-1)
         local RotateTime = Time/BlindNum*N
         local Wait =  Bs[i]:GetComponent(typeof(CS.UnityEngine.RectTransform)):DOLocalRotate(CS.UnityEngine.Vector3(0,90,0),WaitTime)
         local Rotate = Bs[i]:GetComponent(typeof(CS.UnityEngine.RectTransform)):DOLocalRotate(CS.UnityEngine.Vector3(0,0,0),RotateTime)
         local _sequence = CS.DG.Tweening.DOTween.Sequence()
         CS.SequenceExtensions.Append(_sequence,Wait)
         CS.SequenceExtensions.Append(_sequence,Rotate)
         CS.SequenceExtensions.Join(sequence1, _sequence)
    end
    local TempGgraphic = TempG:GetComponent(typeof(CS.UnityEngine.UI.MaskableGraphic))
    local graphic = _self.G:GetComponent(typeof(CS.UnityEngine.UI.MaskableGraphic))
    local colorR = CS.TypeTransform.string2float(config:get_Item("colorR"))
    local colorG = CS.TypeTransform.string2float(config:get_Item("colorG"))
    local colorB = CS.TypeTransform.string2float(config:get_Item("colorB"))
    local colorA = CS.TypeTransform.string2float(config:get_Item("colorA"))
    local TargetColor = CS.UnityEngine.Color(colorR,colorG,colorB,colorA)
    graphic.color = TargetColor
    local FadeImaeg = TempGgraphic:DOFade(0,0.01)
    CS.SequenceExtensions.Append(sequence1, FadeImaeg)
    --更换背景
    local ResourcePath = config:get_Item("BgImage")
    local BgPath = CS.System.IO.Path.Combine(CS.LHGFData.Utils.ResoucePaths.BackgroundPath, ResourcePath)
    local Sprite = CS.LHGFData.ImageLayer.LoadTextureByIO(BgPath)
    _self.G:GetComponent(typeof(CS.UnityEngine.UI.Image)).sprite = Sprite

    --再次旋转百叶窗
    local sequence2 = CS.DG.Tweening.DOTween.Sequence()
    for i = BlindNum, 1,-1 do
         --Wait就是啥也不做
         local WaitTime = Time/BlindNum*(BlindNum-i)
         local RotateTime = Time/BlindNum*N
         local Wait =  Bs[i]:GetComponent(typeof(CS.UnityEngine.RectTransform)):DOLocalRotate(CS.UnityEngine.Vector3(0,0,0),WaitTime)
         local Rotate = Bs[i]:GetComponent(typeof(CS.UnityEngine.RectTransform)):DOLocalRotate(CS.UnityEngine.Vector3(0,90,0),RotateTime)
         local _sequence = CS.DG.Tweening.DOTween.Sequence()
         CS.SequenceExtensions.Append(_sequence,Wait)
         CS.SequenceExtensions.Append(_sequence,Rotate)
         CS.SequenceExtensions.Join(sequence2, _sequence)
    end
    CS.SequenceExtensions.Append(sequence1,sequence2)
    sequence1:OnComplete(
    function()
             CS.UnityEngine.Object.Destroy(TempG)
         end
    )

    _self.sequence = sequence1
end

function OnLoad(_self)
    local config = _self.Config
    local ResourcePath = config:get_Item("BgImage")
    local BgPath = CS.System.IO.Path.Combine(CS.LHGFData.Utils.ResoucePaths.BackgroundPath, ResourcePath)
    local Sprite = CS.LHGFData.ImageLayer.LoadTextureByIO(BgPath)
    _self.G:GetComponent(typeof(CS.UnityEngine.UI.Image)).sprite = Sprite
    local graphic = _self.G:GetComponent(typeof(CS.UnityEngine.UI.MaskableGraphic))
    local colorR = CS.TypeTransform.string2float(config:get_Item("colorR"))
    local colorG = CS.TypeTransform.string2float(config:get_Item("colorG"))
    local colorB = CS.TypeTransform.string2float(config:get_Item("colorB"))
    local colorA = CS.TypeTransform.string2float(config:get_Item("colorA"))
    local TargetColor = CS.UnityEngine.Color(colorR,colorG,colorB,colorA)
    graphic.color = TargetColor
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