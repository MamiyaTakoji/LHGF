--什么都不做，返回空动画，用于测试
function Set(_self)
end
function OnPlay(_self)
        local sequence = CS.DG.Tweening.DOTween.Sequence()
        _self.sequence = sequence
end
function OnLoad(_self)
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