function Set(_self)
    local Dictionary_String_String = CS.System.Collections.Generic.Dictionary(CS.System.String, CS.System.String)
    local dict = Dictionary_String_String()
    local Config = _self.Config
    dict:Add("Size", "540")
    dict:Add("posX","0")
    dict:Add("posY","0")
    dict:Add("rotation","0")
    dict:Add("Time","1")
    dict:Add("IsFade","1")
    for k,v in pairs(Config) do
        local success, Value = Config:TryGetValue(k)
        if success then
            dict:set_Item(k, Value)
        end
    end
    _self.Config = dict
end
-- Lua 代码
function CopyRenderTextureTemporary(sourceRT)
    if sourceRT == nil then
        return nil
    end
    
    -- 获取临时RenderTexture
    local tempRT = CS.UnityEngine.RenderTexture.GetTemporary(
        sourceRT.width,
        sourceRT.height,
        sourceRT.depth,
        sourceRT.format
    )
    
    -- 复制内容
    CS.UnityEngine.Graphics.Blit(sourceRT, tempRT)
    
    return tempRT
end

function ReleaseTemporaryRenderTexture(rt)
    if rt then
        CS.UnityEngine.RenderTexture.ReleaseTemporary(rt)
    end
end
function OnPlay(_self)
    local GameObject = _self.G
    local dict = _self.Config
    local posX = CS.TypeTransform.string2float(dict:get_Item("posX"))
    local posY = CS.TypeTransform.string2float(dict:get_Item("posY"))

    --测试一下纹理复制
    local CamerLayerControler = GameObject:GetComponent(typeof(CS.LHGFCameraLayerControler))
    local FreeCanvasCameraRender = CamerLayerControler.FreeCanvasCameraRender
    local Texture2d = FreeCanvasCameraRender:GetComponent(typeof(CS.UnityEngine.UI.RawImage)).texture
    --CS.UnityEngine.Debug.Log(Texture2d)
    local Texture2dCopied =  CopyRenderTextureTemporary(Texture2d)
    --CS.UnityEngine.Debug.Log(Texture2dCopied)
    local TempG = CS.UnityEngine.Object.Instantiate(FreeCanvasCameraRender,
    FreeCanvasCameraRender:GetComponent(typeof(CS.UnityEngine.Transform)))
    TempG:GetComponent(typeof(CS.UnityEngine.UI.RawImage)).texture = Texture2dCopied

    --设置旋转
    local rotation = CS.TypeTransform.string2float(dict:get_Item("rotation"))
    local Transform = GameObject:GetComponent(typeof(CS.UnityEngine.Transform))
    Transform.localEulerAngles = CS.UnityEngine.Vector3(0, 0, rotation)

    --设置位置
    local posZ = Transform.position.z
    Transform.position = CS.UnityEngine.Vector3(posX, posY, posZ)

    --设置相机范围
    local Camera = GameObject:GetComponent(typeof(CS.UnityEngine.Camera))
    local size = CS.TypeTransform.string2float(dict:get_Item("Size"))
    Camera.orthographicSize = size

    --如果需要Fade
    local IsFadeStr = CS.TypeTransform.string2float(dict:get_Item("IsFade"))
    local IsFade = (IsFadeStr == 1)
    local sequence = CS.DG.Tweening.DOTween.Sequence()
    if IsFade then
        local Time = CS.TypeTransform.string2float(dict:get_Item("Time"))
        local animation = TempG:GetComponent(typeof(CS.UnityEngine.UI.RawImage)):DOFade(0, Time)
        CS.SequenceExtensions.Append(sequence, animation)
    end
    sequence:OnComplete(
    function()
            ReleaseTemporaryRenderTexture(Texture2dCopied)
             CS.UnityEngine.Object.Destroy(TempG)
         end
    )
    _self.sequence = sequence
end
function OnLoad(_self)
    local GameObject = _self.G
    local dict = _self.Config
    local posX = CS.TypeTransform.string2float(dict:get_Item("posX"))
    local posY = CS.TypeTransform.string2float(dict:get_Item("posY"))
    --设置旋转
    local rotation = CS.TypeTransform.string2float(dict:get_Item("rotation"))
    local Transform = GameObject:GetComponent(typeof(CS.UnityEngine.Transform))
    Transform.localEulerAngles = CS.UnityEngine.Vector3(0, 0, rotation)

    --设置位置
    local posZ = Transform.position.z
    Transform.position = CS.UnityEngine.Vector3(posX, posY, posZ)

    --设置相机范围
    local Camera = GameObject:GetComponent(typeof(CS.UnityEngine.Camera))
    local size = CS.TypeTransform.string2float(dict:get_Item("Size"))
    Camera.orthographicSize = size

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

