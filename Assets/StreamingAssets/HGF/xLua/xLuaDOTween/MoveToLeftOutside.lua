function Set(_self)
    --设置posX，posX应为父对象的width的一半减去立绘宽度的1/2
    local GameObjcetRect = _self.G:GetComponent(typeof(CS.UnityEngine.RectTransform))
    local Parent = GameObjcetRect.parent
    local ParentRect = Parent:GetComponent(typeof(CS.UnityEngine.RectTransform))
    local ParentWidth = ParentRect.rect.width/2
    local ImageWidth = GameObjcetRect.rect.width/2
    local posX = -2*(ParentWidth+ImageWidth)
    local poXStr = tostring(posX)
    local Dictionary_String_String = CS.System.Collections.Generic.Dictionary(CS.System.String, CS.System.String)
    local dict = Dictionary_String_String()
    local Config = _self.Config
    Config:set_Item("posX", poXStr)
    for k,v in pairs(Config) do
        local success, Value = Config:TryGetValue(k)
        if success then
            dict:set_Item(k, Value)
        end
    end
    _self.Config = dict
end

function OnPlay(_self)
    local DotweenAnimationCommandDic = ImageCILayer.DOTweenLuaCommandDic
    local LuaCommand = DotweenAnimationCommandDic:get_Item("MoveTo")
    local animationPlayer = CS.LHGFData.ImageLayer.ImageAnimationPlayer(LuaCommand, _self.Config, _self.G)
    --CS.UnityEngine.Debug.Log(animationPlayer)
    animationPlayer:Play()
    _self.sequence = animationPlayer.sequence
end

function OnLoad(_self)
    local DotweenAnimationCommandDic = ImageCILayer.DOTweenLuaCommandDic
    local LuaCommand = DotweenAnimationCommandDic:get_Item("MoveTo")
    local animationPlayer = CS.LHGFData.ImageLayer.ImageAnimationPlayer(LuaCommand, _self.Config, _self.G)
    animationPlayer:Play()
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
function Set(_self)
    --设置posX，posX应为父对象的width的一半减去立绘宽度的1/2
    local GameObjcetRect = _self.G:GetComponent(typeof(CS.UnityEngine.RectTransform))
    local Parent = GameObjcetRect.parent
    local ParentRect = Parent:GetComponent(typeof(CS.UnityEngine.RectTransform))
    local ParentWidth = ParentRect.rect.width/2
    local ImageWidth = GameObjcetRect.rect.width/2
    local posX = ParentWidth-ImageWidth
    local poXStr = tostring(posX)
    local Dictionary_String_String = CS.System.Collections.Generic.Dictionary(CS.System.String, CS.System.String)
    local dict = Dictionary_String_String()
    local Config = _self.Config
    Config:set_Item("posX", poXStr)
    for k,v in pairs(Config) do
        local success, Value = Config:TryGetValue(k)
        if success then
            dict:set_Item(k, Value)
        end
    end
    _self.Config = dict
end

function OnPlay(_self)
    local DotweenAnimationCommandDic = ImageCILayer.DOTweenLuaCommandDic
    local LuaCommand = DotweenAnimationCommandDic:get_Item("MoveTo")
    local animationPlayer = CS.LHGFData.ImageLayer.ImageAnimationPlayer(LuaCommand, _self.Config, _self.G)
    --CS.UnityEngine.Debug.Log(animationPlayer)
    animationPlayer:Play()
    _self.sequence = animationPlayer.sequence
end

function OnLoad(_self)
    local DotweenAnimationCommandDic = ImageCILayer.DOTweenLuaCommandDic
    local LuaCommand = DotweenAnimationCommandDic:get_Item("MoveTo")
    local animationPlayer = CS.LHGFData.ImageLayer.ImageAnimationPlayer(LuaCommand, _self.Config, _self.G)
    animationPlayer:Play()
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