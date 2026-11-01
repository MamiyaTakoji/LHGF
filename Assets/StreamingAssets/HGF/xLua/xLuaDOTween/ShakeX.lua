function Set(_self)
    local Dictionary_String_String = CS.System.Collections.Generic.Dictionary(CS.System.String, CS.System.String)
    local dict = Dictionary_String_String()
    local Config = _self.Config
    dict:Add("Time", "1")
    dict:Add("ShakeTime","10")
    dict:Add("Delta","50")
    for k,v in pairs(Config) do
        local success, Value = Config:TryGetValue(k)
        if success then
            dict:set_Item(k, Value)
        end
    end
    _self.Config = dict
end
function OnPlay(_self)
    local dict = _self.Config
    local Time = CS.TypeTransform.string2float(dict:get_Item("Time"))
    local ShakeTime = CS.TypeTransform.string2float(dict:get_Item("ShakeTime"))
    local Delta = CS.TypeTransform.string2float(dict:get_Item("Delta"))
    local Transform = _self.G:GetComponent(typeof(CS.UnityEngine.Transform))
    local posy =Transform.position.y 
    local animation = CS.DG.Tweening.DOTween.To
    (
        function () return 0 end,
        function (_t)
            local f = Time/ShakeTime
            local t = _t % f 
            local pos = Transform.position
            pos.y = posy + math.sin(t/f*2*math.pi)*Delta
            Transform.position = pos
            --CS.UnityEngine.Debug.Log(pos.y)
        end,
        Time,
        Time
    )
    local sequence = CS.DG.Tweening.DOTween.Sequence()
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