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
    local GameObject = _self.G
    local dict = _self.Config
    local Time = CS.TypeTransform.string2float(dict:get_Item("Time"))
    local graphic = GameObject:GetComponent(typeof(CS.UnityEngine.UI.MaskableGraphic))
    local sequence = CS.DG.Tweening.DOTween.Sequence()
    local animation =  graphic:DOFade(0, Time)
    CS.SequenceExtensions.Append(sequence, animation)
    _self.sequence = sequence
end

function OnLoad(_self)
    local GameObject = _self.G
    local graphic = GameObject:GetComponent(typeof(CS.UnityEngine.UI.MaskableGraphic))
    local color = graphic.color
    color.a = 0
    graphic.color = color
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