--MoveTo方法把物体从初始位置移动到给定位置
function Set(_self)
    local Dictionary_String_String = CS.System.Collections.Generic.Dictionary(CS.System.String, CS.System.String)
    local dict = Dictionary_String_String()
    local Config = _self.Config
    dict:Add("Time", "1")
    dict:Add("posX", "300")
    dict:Add("posY", "default")
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
    --设置目标位置
    local posX = CS.TypeTransform.string2float(dict:get_Item("posX"))
    local posY = 0
    if dict:get_Item("posY") ~= "default" then
        posY = CS.TypeTransform.string2float(dict:get_Item("posY"))
    else
        posY = Rect.rect.height/2
    end
    local targetPos =  CS.UnityEngine.Vector2(posX, posY)

    local sequence = CS.DG.Tweening.DOTween.Sequence()
    local animation =  Rect:DOAnchorPos(targetPos, Time)
    CS.SequenceExtensions.Append(sequence, animation)
    _self.sequence = sequence
end

function OnLoad(_self)
    local GameObject = _self.G
    local Rect = GameObject:GetComponent(typeof(CS.UnityEngine.RectTransform))
    local dict = _self.Config
    --local Time = CS.TypeTransform.string2float(dict:get_Item("Time"))
    --设置目标位置
    local posX = CS.TypeTransform.string2float(dict:get_Item("posX"))
    local posY = 0
    if dict:get_Item("posY") ~= "default" then
        posY = CS.TypeTransform.string2float(dict:get_Item("posY"))
    else
        posY = Rect.rect.height/2
    end
    local targetPos =  CS.UnityEngine.Vector2(posX, posY)
    Rect.anchoredPosition = targetPos
end

Set(self)
local Config = self.Config
local success, Value = Config:TryGetValue("Mode")
if success then
    if  Value == "OnPlay" then
        CS.UnityEngine.Debug.Log("执行OnPlay")
        OnPlay(self)
    elseif Value == "OnLoad" then
        OnLoad(self)
    end
end