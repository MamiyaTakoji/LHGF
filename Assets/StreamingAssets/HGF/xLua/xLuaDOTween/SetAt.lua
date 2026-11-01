function Set(_self)
    local Dictionary_String_String = CS.System.Collections.Generic.Dictionary(CS.System.String, CS.System.String)
    local dict = Dictionary_String_String()
    local Config = _self.Config
    dict:Add("Time", "1")
    dict:Add("sizeX", "default")
    dict:Add("sizeY", "default")
    dict:Add("posX", "0")
    dict:Add("posY", "default")
    dict:Add("colorR", "1")
    dict:Add("colorG", "1")
    dict:Add("colorB", "1")
    dict:Add("colorA", "1")
    dict:Add("rotation", "0")
    for k,v in pairs(Config) do
        local success, Value = Config:TryGetValue(k)
        if success then
            dict:set_Item(k, Value)
        end
    end
    _self.Config = dict
end
function OnPlay(_self)
    local GameObject = _self.G
    local Rect = GameObject:GetComponent(typeof(CS.UnityEngine.RectTransform))
    local dict = _self.Config
    local Time = CS.TypeTransform.string2float(dict:get_Item("Time"))
    --首先设置图像大小
    local sizeX = 0
    local sizeY = 0
    if dict:get_Item("sizeX") == "default" then
        sizeX = Rect.sizeDelta.x
    else
        sizeX = CS.TypeTransform.string2float(dict:get_Item("sizeX"))
    end

    if dict:get_Item("sizeY") == "default" then
        sizeY = Rect.sizeDelta.y
    else
        sizeY = CS.TypeTransform.string2float(dict:get_Item("sizeY"))
    end
    Rect.sizeDelta = CS.UnityEngine.Vector2(sizeX, sizeY)
    
    local posX = CS.TypeTransform.string2float(dict:get_Item("posX"))
    local posY = 0
    if dict:get_Item("posY") ~= "default" then
        posY = CS.TypeTransform.string2float(dict:get_Item("posY"))
    else
        posY = Rect.rect.height/2
    end
    --设置旋转
    local rotation = CS.TypeTransform.string2float(dict:get_Item("rotation"))
    Rect.localEulerAngles = CS.UnityEngine.Vector3(0, 0, rotation)

    --设置位置
    Rect.anchoredPosition = CS.UnityEngine.Vector2(posX, posY)

    --设置Image颜色
    local graphic = GameObject:GetComponent(typeof(CS.UnityEngine.UI.MaskableGraphic))
    local colorR = CS.TypeTransform.string2float(dict:get_Item("colorR"))
    local colorG = CS.TypeTransform.string2float(dict:get_Item("colorG"))
    local colorB = CS.TypeTransform.string2float(dict:get_Item("colorB"))
    local colorA = CS.TypeTransform.string2float(dict:get_Item("colorA"))
    local FinalColor = CS.UnityEngine.Color(colorR,colorG,colorB,colorA)
    graphic.color = FinalColor
    local Color = graphic.color
    Color.a = 0
    graphic.color = Color
    local sequence = CS.DG.Tweening.DOTween.Sequence()
    local animation =  graphic:DOFade(FinalColor.a, Time)
    CS.SequenceExtensions.Append(sequence, animation)
    _self.sequence = sequence
end

function OnLoad(_self)
    local GameObject = _self.G
    local Rect = GameObject:GetComponent(typeof(CS.UnityEngine.RectTransform))
    CS.UnityEngine.Debug.Log("sizeDelta是：")
    CS.UnityEngine.Debug.Log(Rect.sizeDelta)
    local dict = _self.Config
    local Time = CS.TypeTransform.string2float(dict:get_Item("Time"))
    --首先设置图像大小
    local sizeX = 0
    local sizeY = 0
    if dict:get_Item("sizeX") == "default" then
        sizeX = Rect.sizeDelta.x
    else
        sizeX = CS.TypeTransform.string2float(dict:get_Item("sizeX"))
    end

    if dict:get_Item("sizeY") == "default" then
        sizeY = Rect.sizeDelta.y
    else
        sizeY = CS.TypeTransform.string2float(dict:get_Item("sizeY"))
    end
    Rect.sizeDelta = CS.UnityEngine.Vector2(sizeX, sizeY)
    
    local posX = CS.TypeTransform.string2float(dict:get_Item("posX"))
    local posY = 0
    if dict:get_Item("posY") ~= "default" then
        posY = CS.TypeTransform.string2float(dict:get_Item("posY"))
    else
        posY = Rect.rect.height/2
    end
    --设置旋转
    local rotation = CS.TypeTransform.string2float(dict:get_Item("rotation"))
    Rect.localEulerAngles = CS.UnityEngine.Vector3(0, 0, rotation)

    --设置位置
    Rect.anchoredPosition = CS.UnityEngine.Vector2(posX, posY)

    --设置Image颜色
    local graphic = GameObject:GetComponent(typeof(CS.UnityEngine.UI.MaskableGraphic))
    local colorR = CS.TypeTransform.string2float(dict:get_Item("colorR"))
    local colorG = CS.TypeTransform.string2float(dict:get_Item("colorG"))
    local colorB = CS.TypeTransform.string2float(dict:get_Item("colorB"))
    local colorA = CS.TypeTransform.string2float(dict:get_Item("colorA"))
    local FinalColor = CS.UnityEngine.Color(colorR,colorG,colorB,colorA)
    graphic.color = FinalColor
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