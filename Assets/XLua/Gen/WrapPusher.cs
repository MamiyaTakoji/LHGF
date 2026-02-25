#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

using System;


namespace XLua
{
    public partial class ObjectTranslator
    {
        
        class IniterAdderUnityEngineVector2
        {
            static IniterAdderUnityEngineVector2()
            {
                LuaEnv.AddIniter(Init);
            }
			
			static void Init(LuaEnv luaenv, ObjectTranslator translator)
			{
			
				translator.RegisterPushAndGetAndUpdate<UnityEngine.Vector2>(translator.PushUnityEngineVector2, translator.Get, translator.UpdateUnityEngineVector2);
				translator.RegisterPushAndGetAndUpdate<UnityEngine.Vector3>(translator.PushUnityEngineVector3, translator.Get, translator.UpdateUnityEngineVector3);
				translator.RegisterPushAndGetAndUpdate<UnityEngine.Vector4>(translator.PushUnityEngineVector4, translator.Get, translator.UpdateUnityEngineVector4);
				translator.RegisterPushAndGetAndUpdate<UnityEngine.Color>(translator.PushUnityEngineColor, translator.Get, translator.UpdateUnityEngineColor);
				translator.RegisterPushAndGetAndUpdate<UnityEngine.Quaternion>(translator.PushUnityEngineQuaternion, translator.Get, translator.UpdateUnityEngineQuaternion);
				translator.RegisterPushAndGetAndUpdate<UnityEngine.Ray>(translator.PushUnityEngineRay, translator.Get, translator.UpdateUnityEngineRay);
				translator.RegisterPushAndGetAndUpdate<UnityEngine.Bounds>(translator.PushUnityEngineBounds, translator.Get, translator.UpdateUnityEngineBounds);
				translator.RegisterPushAndGetAndUpdate<UnityEngine.Ray2D>(translator.PushUnityEngineRay2D, translator.Get, translator.UpdateUnityEngineRay2D);
				translator.RegisterPushAndGetAndUpdate<XLuaTest.Pedding>(translator.PushXLuaTestPedding, translator.Get, translator.UpdateXLuaTestPedding);
				translator.RegisterPushAndGetAndUpdate<XLuaTest.MyStruct>(translator.PushXLuaTestMyStruct, translator.Get, translator.UpdateXLuaTestMyStruct);
				translator.RegisterPushAndGetAndUpdate<XLuaTest.PushAsTableStruct>(translator.PushXLuaTestPushAsTableStruct, translator.Get, translator.UpdateXLuaTestPushAsTableStruct);
				translator.RegisterPushAndGetAndUpdate<Tutorial.TestEnum>(translator.PushTutorialTestEnum, translator.Get, translator.UpdateTutorialTestEnum);
				translator.RegisterPushAndGetAndUpdate<XLuaTest.MyEnum>(translator.PushXLuaTestMyEnum, translator.Get, translator.UpdateXLuaTestMyEnum);
				translator.RegisterPushAndGetAndUpdate<Tutorial.DerivedClass.TestEnumInner>(translator.PushTutorialDerivedClassTestEnumInner, translator.Get, translator.UpdateTutorialDerivedClassTestEnumInner);
				translator.RegisterPushAndGetAndUpdate<DG.Tweening.Ease>(translator.PushDGTweeningEase, translator.Get, translator.UpdateDGTweeningEase);
				translator.RegisterPushAndGetAndUpdate<DG.Tweening.PathType>(translator.PushDGTweeningPathType, translator.Get, translator.UpdateDGTweeningPathType);
				translator.RegisterPushAndGetAndUpdate<DG.Tweening.PathMode>(translator.PushDGTweeningPathMode, translator.Get, translator.UpdateDGTweeningPathMode);
				translator.RegisterPushAndGetAndUpdate<DG.Tweening.RotateMode>(translator.PushDGTweeningRotateMode, translator.Get, translator.UpdateDGTweeningRotateMode);
			
			}
        }
        
        static IniterAdderUnityEngineVector2 s_IniterAdderUnityEngineVector2_dumb_obj = new IniterAdderUnityEngineVector2();
        static IniterAdderUnityEngineVector2 IniterAdderUnityEngineVector2_dumb_obj {get{return s_IniterAdderUnityEngineVector2_dumb_obj;}}
        
        
        int UnityEngineVector2_TypeID = -1;
        public void PushUnityEngineVector2(RealStatePtr L, UnityEngine.Vector2 val)
        {
            if (UnityEngineVector2_TypeID == -1)
            {
			    bool is_first;
                UnityEngineVector2_TypeID = getTypeId(L, typeof(UnityEngine.Vector2), out is_first);
				
            }
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 8, UnityEngineVector2_TypeID);
            if (!CopyByValue.Pack(buff, 0, val))
            {
                throw new Exception("pack fail fail for UnityEngine.Vector2 ,value="+val);
            }
			
        }
		
        public void Get(RealStatePtr L, int index, out UnityEngine.Vector2 val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineVector2_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Vector2");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);if (!CopyByValue.UnPack(buff, 0, out val))
                {
                    throw new Exception("unpack fail for UnityEngine.Vector2");
                }
            }
			else if (type ==LuaTypes.LUA_TTABLE)
			{
			    CopyByValue.UnPack(this, L, index, out val);
			}
            else
            {
                val = (UnityEngine.Vector2)objectCasters.GetCaster(typeof(UnityEngine.Vector2))(L, index, null);
            }
        }
		
        public void UpdateUnityEngineVector2(RealStatePtr L, int index, UnityEngine.Vector2 val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineVector2_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Vector2");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  val))
                {
                    throw new Exception("pack fail for UnityEngine.Vector2 ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int UnityEngineVector3_TypeID = -1;
        public void PushUnityEngineVector3(RealStatePtr L, UnityEngine.Vector3 val)
        {
            if (UnityEngineVector3_TypeID == -1)
            {
			    bool is_first;
                UnityEngineVector3_TypeID = getTypeId(L, typeof(UnityEngine.Vector3), out is_first);
				
            }
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 12, UnityEngineVector3_TypeID);
            if (!CopyByValue.Pack(buff, 0, val))
            {
                throw new Exception("pack fail fail for UnityEngine.Vector3 ,value="+val);
            }
			
        }
		
        public void Get(RealStatePtr L, int index, out UnityEngine.Vector3 val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineVector3_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Vector3");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);if (!CopyByValue.UnPack(buff, 0, out val))
                {
                    throw new Exception("unpack fail for UnityEngine.Vector3");
                }
            }
			else if (type ==LuaTypes.LUA_TTABLE)
			{
			    CopyByValue.UnPack(this, L, index, out val);
			}
            else
            {
                val = (UnityEngine.Vector3)objectCasters.GetCaster(typeof(UnityEngine.Vector3))(L, index, null);
            }
        }
		
        public void UpdateUnityEngineVector3(RealStatePtr L, int index, UnityEngine.Vector3 val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineVector3_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Vector3");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  val))
                {
                    throw new Exception("pack fail for UnityEngine.Vector3 ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int UnityEngineVector4_TypeID = -1;
        public void PushUnityEngineVector4(RealStatePtr L, UnityEngine.Vector4 val)
        {
            if (UnityEngineVector4_TypeID == -1)
            {
			    bool is_first;
                UnityEngineVector4_TypeID = getTypeId(L, typeof(UnityEngine.Vector4), out is_first);
				
            }
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 16, UnityEngineVector4_TypeID);
            if (!CopyByValue.Pack(buff, 0, val))
            {
                throw new Exception("pack fail fail for UnityEngine.Vector4 ,value="+val);
            }
			
        }
		
        public void Get(RealStatePtr L, int index, out UnityEngine.Vector4 val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineVector4_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Vector4");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);if (!CopyByValue.UnPack(buff, 0, out val))
                {
                    throw new Exception("unpack fail for UnityEngine.Vector4");
                }
            }
			else if (type ==LuaTypes.LUA_TTABLE)
			{
			    CopyByValue.UnPack(this, L, index, out val);
			}
            else
            {
                val = (UnityEngine.Vector4)objectCasters.GetCaster(typeof(UnityEngine.Vector4))(L, index, null);
            }
        }
		
        public void UpdateUnityEngineVector4(RealStatePtr L, int index, UnityEngine.Vector4 val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineVector4_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Vector4");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  val))
                {
                    throw new Exception("pack fail for UnityEngine.Vector4 ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int UnityEngineColor_TypeID = -1;
        public void PushUnityEngineColor(RealStatePtr L, UnityEngine.Color val)
        {
            if (UnityEngineColor_TypeID == -1)
            {
			    bool is_first;
                UnityEngineColor_TypeID = getTypeId(L, typeof(UnityEngine.Color), out is_first);
				
            }
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 16, UnityEngineColor_TypeID);
            if (!CopyByValue.Pack(buff, 0, val))
            {
                throw new Exception("pack fail fail for UnityEngine.Color ,value="+val);
            }
			
        }
		
        public void Get(RealStatePtr L, int index, out UnityEngine.Color val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineColor_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Color");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);if (!CopyByValue.UnPack(buff, 0, out val))
                {
                    throw new Exception("unpack fail for UnityEngine.Color");
                }
            }
			else if (type ==LuaTypes.LUA_TTABLE)
			{
			    CopyByValue.UnPack(this, L, index, out val);
			}
            else
            {
                val = (UnityEngine.Color)objectCasters.GetCaster(typeof(UnityEngine.Color))(L, index, null);
            }
        }
		
        public void UpdateUnityEngineColor(RealStatePtr L, int index, UnityEngine.Color val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineColor_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Color");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  val))
                {
                    throw new Exception("pack fail for UnityEngine.Color ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int UnityEngineQuaternion_TypeID = -1;
        public void PushUnityEngineQuaternion(RealStatePtr L, UnityEngine.Quaternion val)
        {
            if (UnityEngineQuaternion_TypeID == -1)
            {
			    bool is_first;
                UnityEngineQuaternion_TypeID = getTypeId(L, typeof(UnityEngine.Quaternion), out is_first);
				
            }
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 16, UnityEngineQuaternion_TypeID);
            if (!CopyByValue.Pack(buff, 0, val))
            {
                throw new Exception("pack fail fail for UnityEngine.Quaternion ,value="+val);
            }
			
        }
		
        public void Get(RealStatePtr L, int index, out UnityEngine.Quaternion val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineQuaternion_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Quaternion");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);if (!CopyByValue.UnPack(buff, 0, out val))
                {
                    throw new Exception("unpack fail for UnityEngine.Quaternion");
                }
            }
			else if (type ==LuaTypes.LUA_TTABLE)
			{
			    CopyByValue.UnPack(this, L, index, out val);
			}
            else
            {
                val = (UnityEngine.Quaternion)objectCasters.GetCaster(typeof(UnityEngine.Quaternion))(L, index, null);
            }
        }
		
        public void UpdateUnityEngineQuaternion(RealStatePtr L, int index, UnityEngine.Quaternion val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineQuaternion_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Quaternion");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  val))
                {
                    throw new Exception("pack fail for UnityEngine.Quaternion ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int UnityEngineRay_TypeID = -1;
        public void PushUnityEngineRay(RealStatePtr L, UnityEngine.Ray val)
        {
            if (UnityEngineRay_TypeID == -1)
            {
			    bool is_first;
                UnityEngineRay_TypeID = getTypeId(L, typeof(UnityEngine.Ray), out is_first);
				
            }
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 24, UnityEngineRay_TypeID);
            if (!CopyByValue.Pack(buff, 0, val))
            {
                throw new Exception("pack fail fail for UnityEngine.Ray ,value="+val);
            }
			
        }
		
        public void Get(RealStatePtr L, int index, out UnityEngine.Ray val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineRay_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Ray");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);if (!CopyByValue.UnPack(buff, 0, out val))
                {
                    throw new Exception("unpack fail for UnityEngine.Ray");
                }
            }
			else if (type ==LuaTypes.LUA_TTABLE)
			{
			    CopyByValue.UnPack(this, L, index, out val);
			}
            else
            {
                val = (UnityEngine.Ray)objectCasters.GetCaster(typeof(UnityEngine.Ray))(L, index, null);
            }
        }
		
        public void UpdateUnityEngineRay(RealStatePtr L, int index, UnityEngine.Ray val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineRay_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Ray");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  val))
                {
                    throw new Exception("pack fail for UnityEngine.Ray ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int UnityEngineBounds_TypeID = -1;
        public void PushUnityEngineBounds(RealStatePtr L, UnityEngine.Bounds val)
        {
            if (UnityEngineBounds_TypeID == -1)
            {
			    bool is_first;
                UnityEngineBounds_TypeID = getTypeId(L, typeof(UnityEngine.Bounds), out is_first);
				
            }
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 24, UnityEngineBounds_TypeID);
            if (!CopyByValue.Pack(buff, 0, val))
            {
                throw new Exception("pack fail fail for UnityEngine.Bounds ,value="+val);
            }
			
        }
		
        public void Get(RealStatePtr L, int index, out UnityEngine.Bounds val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineBounds_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Bounds");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);if (!CopyByValue.UnPack(buff, 0, out val))
                {
                    throw new Exception("unpack fail for UnityEngine.Bounds");
                }
            }
			else if (type ==LuaTypes.LUA_TTABLE)
			{
			    CopyByValue.UnPack(this, L, index, out val);
			}
            else
            {
                val = (UnityEngine.Bounds)objectCasters.GetCaster(typeof(UnityEngine.Bounds))(L, index, null);
            }
        }
		
        public void UpdateUnityEngineBounds(RealStatePtr L, int index, UnityEngine.Bounds val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineBounds_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Bounds");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  val))
                {
                    throw new Exception("pack fail for UnityEngine.Bounds ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int UnityEngineRay2D_TypeID = -1;
        public void PushUnityEngineRay2D(RealStatePtr L, UnityEngine.Ray2D val)
        {
            if (UnityEngineRay2D_TypeID == -1)
            {
			    bool is_first;
                UnityEngineRay2D_TypeID = getTypeId(L, typeof(UnityEngine.Ray2D), out is_first);
				
            }
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 16, UnityEngineRay2D_TypeID);
            if (!CopyByValue.Pack(buff, 0, val))
            {
                throw new Exception("pack fail fail for UnityEngine.Ray2D ,value="+val);
            }
			
        }
		
        public void Get(RealStatePtr L, int index, out UnityEngine.Ray2D val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineRay2D_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Ray2D");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);if (!CopyByValue.UnPack(buff, 0, out val))
                {
                    throw new Exception("unpack fail for UnityEngine.Ray2D");
                }
            }
			else if (type ==LuaTypes.LUA_TTABLE)
			{
			    CopyByValue.UnPack(this, L, index, out val);
			}
            else
            {
                val = (UnityEngine.Ray2D)objectCasters.GetCaster(typeof(UnityEngine.Ray2D))(L, index, null);
            }
        }
		
        public void UpdateUnityEngineRay2D(RealStatePtr L, int index, UnityEngine.Ray2D val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != UnityEngineRay2D_TypeID)
				{
				    throw new Exception("invalid userdata for UnityEngine.Ray2D");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  val))
                {
                    throw new Exception("pack fail for UnityEngine.Ray2D ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int XLuaTestPedding_TypeID = -1;
        public void PushXLuaTestPedding(RealStatePtr L, XLuaTest.Pedding val)
        {
            if (XLuaTestPedding_TypeID == -1)
            {
			    bool is_first;
                XLuaTestPedding_TypeID = getTypeId(L, typeof(XLuaTest.Pedding), out is_first);
				
            }
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 1, XLuaTestPedding_TypeID);
            if (!CopyByValue.Pack(buff, 0, val))
            {
                throw new Exception("pack fail fail for XLuaTest.Pedding ,value="+val);
            }
			
        }
		
        public void Get(RealStatePtr L, int index, out XLuaTest.Pedding val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != XLuaTestPedding_TypeID)
				{
				    throw new Exception("invalid userdata for XLuaTest.Pedding");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);if (!CopyByValue.UnPack(buff, 0, out val))
                {
                    throw new Exception("unpack fail for XLuaTest.Pedding");
                }
            }
			else if (type ==LuaTypes.LUA_TTABLE)
			{
			    CopyByValue.UnPack(this, L, index, out val);
			}
            else
            {
                val = (XLuaTest.Pedding)objectCasters.GetCaster(typeof(XLuaTest.Pedding))(L, index, null);
            }
        }
		
        public void UpdateXLuaTestPedding(RealStatePtr L, int index, XLuaTest.Pedding val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != XLuaTestPedding_TypeID)
				{
				    throw new Exception("invalid userdata for XLuaTest.Pedding");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  val))
                {
                    throw new Exception("pack fail for XLuaTest.Pedding ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int XLuaTestMyStruct_TypeID = -1;
        public void PushXLuaTestMyStruct(RealStatePtr L, XLuaTest.MyStruct val)
        {
            if (XLuaTestMyStruct_TypeID == -1)
            {
			    bool is_first;
                XLuaTestMyStruct_TypeID = getTypeId(L, typeof(XLuaTest.MyStruct), out is_first);
				
            }
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 25, XLuaTestMyStruct_TypeID);
            if (!CopyByValue.Pack(buff, 0, val))
            {
                throw new Exception("pack fail fail for XLuaTest.MyStruct ,value="+val);
            }
			
        }
		
        public void Get(RealStatePtr L, int index, out XLuaTest.MyStruct val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != XLuaTestMyStruct_TypeID)
				{
				    throw new Exception("invalid userdata for XLuaTest.MyStruct");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);if (!CopyByValue.UnPack(buff, 0, out val))
                {
                    throw new Exception("unpack fail for XLuaTest.MyStruct");
                }
            }
			else if (type ==LuaTypes.LUA_TTABLE)
			{
			    CopyByValue.UnPack(this, L, index, out val);
			}
            else
            {
                val = (XLuaTest.MyStruct)objectCasters.GetCaster(typeof(XLuaTest.MyStruct))(L, index, null);
            }
        }
		
        public void UpdateXLuaTestMyStruct(RealStatePtr L, int index, XLuaTest.MyStruct val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != XLuaTestMyStruct_TypeID)
				{
				    throw new Exception("invalid userdata for XLuaTest.MyStruct");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  val))
                {
                    throw new Exception("pack fail for XLuaTest.MyStruct ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int XLuaTestPushAsTableStruct_TypeID = -1;
        public void PushXLuaTestPushAsTableStruct(RealStatePtr L, XLuaTest.PushAsTableStruct val)
        {
            if (XLuaTestPushAsTableStruct_TypeID == -1)
            {
			    bool is_first;
                XLuaTestPushAsTableStruct_TypeID = getTypeId(L, typeof(XLuaTest.PushAsTableStruct), out is_first);
				
            }
			
			
			LuaAPI.xlua_pushcstable(L, 2, XLuaTestPushAsTableStruct_TypeID);
			
			LuaAPI.xlua_pushasciistring(L, "x");
			LuaAPI.xlua_pushinteger(L, val.x);
			LuaAPI.lua_rawset(L, -3);
			
			LuaAPI.xlua_pushasciistring(L, "y");
			LuaAPI.xlua_pushinteger(L, val.y);
			LuaAPI.lua_rawset(L, -3);
			
			
        }
		
        public void Get(RealStatePtr L, int index, out XLuaTest.PushAsTableStruct val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != XLuaTestPushAsTableStruct_TypeID)
				{
				    throw new Exception("invalid userdata for XLuaTest.PushAsTableStruct");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);if (!CopyByValue.UnPack(buff, 0, out val))
                {
                    throw new Exception("unpack fail for XLuaTest.PushAsTableStruct");
                }
            }
			else if (type ==LuaTypes.LUA_TTABLE)
			{
			    CopyByValue.UnPack(this, L, index, out val);
			}
            else
            {
                val = (XLuaTest.PushAsTableStruct)objectCasters.GetCaster(typeof(XLuaTest.PushAsTableStruct))(L, index, null);
            }
        }
		
        public void UpdateXLuaTestPushAsTableStruct(RealStatePtr L, int index, XLuaTest.PushAsTableStruct val)
        {
		    
			if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TTABLE)
            {
			    return;
			}
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int TutorialTestEnum_TypeID = -1;
		int TutorialTestEnum_EnumRef = -1;
        
        public void PushTutorialTestEnum(RealStatePtr L, Tutorial.TestEnum val)
        {
            if (TutorialTestEnum_TypeID == -1)
            {
			    bool is_first;
                TutorialTestEnum_TypeID = getTypeId(L, typeof(Tutorial.TestEnum), out is_first);
				
				if (TutorialTestEnum_EnumRef == -1)
				{
				    Utils.LoadCSTable(L, typeof(Tutorial.TestEnum));
				    TutorialTestEnum_EnumRef = LuaAPI.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
				}
				
            }
			
			if (LuaAPI.xlua_tryget_cachedud(L, (int)val, TutorialTestEnum_EnumRef) == 1)
            {
			    return;
			}
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 4, TutorialTestEnum_TypeID);
            if (!CopyByValue.Pack(buff, 0, (int)val))
            {
                throw new Exception("pack fail fail for Tutorial.TestEnum ,value="+val);
            }
			
			LuaAPI.lua_getref(L, TutorialTestEnum_EnumRef);
			LuaAPI.lua_pushvalue(L, -2);
			LuaAPI.xlua_rawseti(L, -2, (int)val);
			LuaAPI.lua_pop(L, 1);
			
        }
		
        public void Get(RealStatePtr L, int index, out Tutorial.TestEnum val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != TutorialTestEnum_TypeID)
				{
				    throw new Exception("invalid userdata for Tutorial.TestEnum");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
				int e;
                if (!CopyByValue.UnPack(buff, 0, out e))
                {
                    throw new Exception("unpack fail for Tutorial.TestEnum");
                }
				val = (Tutorial.TestEnum)e;
                
            }
            else
            {
                val = (Tutorial.TestEnum)objectCasters.GetCaster(typeof(Tutorial.TestEnum))(L, index, null);
            }
        }
		
        public void UpdateTutorialTestEnum(RealStatePtr L, int index, Tutorial.TestEnum val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != TutorialTestEnum_TypeID)
				{
				    throw new Exception("invalid userdata for Tutorial.TestEnum");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  (int)val))
                {
                    throw new Exception("pack fail for Tutorial.TestEnum ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int XLuaTestMyEnum_TypeID = -1;
		int XLuaTestMyEnum_EnumRef = -1;
        
        public void PushXLuaTestMyEnum(RealStatePtr L, XLuaTest.MyEnum val)
        {
            if (XLuaTestMyEnum_TypeID == -1)
            {
			    bool is_first;
                XLuaTestMyEnum_TypeID = getTypeId(L, typeof(XLuaTest.MyEnum), out is_first);
				
				if (XLuaTestMyEnum_EnumRef == -1)
				{
				    Utils.LoadCSTable(L, typeof(XLuaTest.MyEnum));
				    XLuaTestMyEnum_EnumRef = LuaAPI.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
				}
				
            }
			
			if (LuaAPI.xlua_tryget_cachedud(L, (int)val, XLuaTestMyEnum_EnumRef) == 1)
            {
			    return;
			}
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 4, XLuaTestMyEnum_TypeID);
            if (!CopyByValue.Pack(buff, 0, (int)val))
            {
                throw new Exception("pack fail fail for XLuaTest.MyEnum ,value="+val);
            }
			
			LuaAPI.lua_getref(L, XLuaTestMyEnum_EnumRef);
			LuaAPI.lua_pushvalue(L, -2);
			LuaAPI.xlua_rawseti(L, -2, (int)val);
			LuaAPI.lua_pop(L, 1);
			
        }
		
        public void Get(RealStatePtr L, int index, out XLuaTest.MyEnum val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != XLuaTestMyEnum_TypeID)
				{
				    throw new Exception("invalid userdata for XLuaTest.MyEnum");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
				int e;
                if (!CopyByValue.UnPack(buff, 0, out e))
                {
                    throw new Exception("unpack fail for XLuaTest.MyEnum");
                }
				val = (XLuaTest.MyEnum)e;
                
            }
            else
            {
                val = (XLuaTest.MyEnum)objectCasters.GetCaster(typeof(XLuaTest.MyEnum))(L, index, null);
            }
        }
		
        public void UpdateXLuaTestMyEnum(RealStatePtr L, int index, XLuaTest.MyEnum val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != XLuaTestMyEnum_TypeID)
				{
				    throw new Exception("invalid userdata for XLuaTest.MyEnum");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  (int)val))
                {
                    throw new Exception("pack fail for XLuaTest.MyEnum ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int TutorialDerivedClassTestEnumInner_TypeID = -1;
		int TutorialDerivedClassTestEnumInner_EnumRef = -1;
        
        public void PushTutorialDerivedClassTestEnumInner(RealStatePtr L, Tutorial.DerivedClass.TestEnumInner val)
        {
            if (TutorialDerivedClassTestEnumInner_TypeID == -1)
            {
			    bool is_first;
                TutorialDerivedClassTestEnumInner_TypeID = getTypeId(L, typeof(Tutorial.DerivedClass.TestEnumInner), out is_first);
				
				if (TutorialDerivedClassTestEnumInner_EnumRef == -1)
				{
				    Utils.LoadCSTable(L, typeof(Tutorial.DerivedClass.TestEnumInner));
				    TutorialDerivedClassTestEnumInner_EnumRef = LuaAPI.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
				}
				
            }
			
			if (LuaAPI.xlua_tryget_cachedud(L, (int)val, TutorialDerivedClassTestEnumInner_EnumRef) == 1)
            {
			    return;
			}
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 4, TutorialDerivedClassTestEnumInner_TypeID);
            if (!CopyByValue.Pack(buff, 0, (int)val))
            {
                throw new Exception("pack fail fail for Tutorial.DerivedClass.TestEnumInner ,value="+val);
            }
			
			LuaAPI.lua_getref(L, TutorialDerivedClassTestEnumInner_EnumRef);
			LuaAPI.lua_pushvalue(L, -2);
			LuaAPI.xlua_rawseti(L, -2, (int)val);
			LuaAPI.lua_pop(L, 1);
			
        }
		
        public void Get(RealStatePtr L, int index, out Tutorial.DerivedClass.TestEnumInner val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != TutorialDerivedClassTestEnumInner_TypeID)
				{
				    throw new Exception("invalid userdata for Tutorial.DerivedClass.TestEnumInner");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
				int e;
                if (!CopyByValue.UnPack(buff, 0, out e))
                {
                    throw new Exception("unpack fail for Tutorial.DerivedClass.TestEnumInner");
                }
				val = (Tutorial.DerivedClass.TestEnumInner)e;
                
            }
            else
            {
                val = (Tutorial.DerivedClass.TestEnumInner)objectCasters.GetCaster(typeof(Tutorial.DerivedClass.TestEnumInner))(L, index, null);
            }
        }
		
        public void UpdateTutorialDerivedClassTestEnumInner(RealStatePtr L, int index, Tutorial.DerivedClass.TestEnumInner val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != TutorialDerivedClassTestEnumInner_TypeID)
				{
				    throw new Exception("invalid userdata for Tutorial.DerivedClass.TestEnumInner");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  (int)val))
                {
                    throw new Exception("pack fail for Tutorial.DerivedClass.TestEnumInner ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int DGTweeningEase_TypeID = -1;
		int DGTweeningEase_EnumRef = -1;
        
        public void PushDGTweeningEase(RealStatePtr L, DG.Tweening.Ease val)
        {
            if (DGTweeningEase_TypeID == -1)
            {
			    bool is_first;
                DGTweeningEase_TypeID = getTypeId(L, typeof(DG.Tweening.Ease), out is_first);
				
				if (DGTweeningEase_EnumRef == -1)
				{
				    Utils.LoadCSTable(L, typeof(DG.Tweening.Ease));
				    DGTweeningEase_EnumRef = LuaAPI.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
				}
				
            }
			
			if (LuaAPI.xlua_tryget_cachedud(L, (int)val, DGTweeningEase_EnumRef) == 1)
            {
			    return;
			}
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 4, DGTweeningEase_TypeID);
            if (!CopyByValue.Pack(buff, 0, (int)val))
            {
                throw new Exception("pack fail fail for DG.Tweening.Ease ,value="+val);
            }
			
			LuaAPI.lua_getref(L, DGTweeningEase_EnumRef);
			LuaAPI.lua_pushvalue(L, -2);
			LuaAPI.xlua_rawseti(L, -2, (int)val);
			LuaAPI.lua_pop(L, 1);
			
        }
		
        public void Get(RealStatePtr L, int index, out DG.Tweening.Ease val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != DGTweeningEase_TypeID)
				{
				    throw new Exception("invalid userdata for DG.Tweening.Ease");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
				int e;
                if (!CopyByValue.UnPack(buff, 0, out e))
                {
                    throw new Exception("unpack fail for DG.Tweening.Ease");
                }
				val = (DG.Tweening.Ease)e;
                
            }
            else
            {
                val = (DG.Tweening.Ease)objectCasters.GetCaster(typeof(DG.Tweening.Ease))(L, index, null);
            }
        }
		
        public void UpdateDGTweeningEase(RealStatePtr L, int index, DG.Tweening.Ease val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != DGTweeningEase_TypeID)
				{
				    throw new Exception("invalid userdata for DG.Tweening.Ease");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  (int)val))
                {
                    throw new Exception("pack fail for DG.Tweening.Ease ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int DGTweeningPathType_TypeID = -1;
		int DGTweeningPathType_EnumRef = -1;
        
        public void PushDGTweeningPathType(RealStatePtr L, DG.Tweening.PathType val)
        {
            if (DGTweeningPathType_TypeID == -1)
            {
			    bool is_first;
                DGTweeningPathType_TypeID = getTypeId(L, typeof(DG.Tweening.PathType), out is_first);
				
				if (DGTweeningPathType_EnumRef == -1)
				{
				    Utils.LoadCSTable(L, typeof(DG.Tweening.PathType));
				    DGTweeningPathType_EnumRef = LuaAPI.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
				}
				
            }
			
			if (LuaAPI.xlua_tryget_cachedud(L, (int)val, DGTweeningPathType_EnumRef) == 1)
            {
			    return;
			}
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 4, DGTweeningPathType_TypeID);
            if (!CopyByValue.Pack(buff, 0, (int)val))
            {
                throw new Exception("pack fail fail for DG.Tweening.PathType ,value="+val);
            }
			
			LuaAPI.lua_getref(L, DGTweeningPathType_EnumRef);
			LuaAPI.lua_pushvalue(L, -2);
			LuaAPI.xlua_rawseti(L, -2, (int)val);
			LuaAPI.lua_pop(L, 1);
			
        }
		
        public void Get(RealStatePtr L, int index, out DG.Tweening.PathType val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != DGTweeningPathType_TypeID)
				{
				    throw new Exception("invalid userdata for DG.Tweening.PathType");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
				int e;
                if (!CopyByValue.UnPack(buff, 0, out e))
                {
                    throw new Exception("unpack fail for DG.Tweening.PathType");
                }
				val = (DG.Tweening.PathType)e;
                
            }
            else
            {
                val = (DG.Tweening.PathType)objectCasters.GetCaster(typeof(DG.Tweening.PathType))(L, index, null);
            }
        }
		
        public void UpdateDGTweeningPathType(RealStatePtr L, int index, DG.Tweening.PathType val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != DGTweeningPathType_TypeID)
				{
				    throw new Exception("invalid userdata for DG.Tweening.PathType");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  (int)val))
                {
                    throw new Exception("pack fail for DG.Tweening.PathType ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int DGTweeningPathMode_TypeID = -1;
		int DGTweeningPathMode_EnumRef = -1;
        
        public void PushDGTweeningPathMode(RealStatePtr L, DG.Tweening.PathMode val)
        {
            if (DGTweeningPathMode_TypeID == -1)
            {
			    bool is_first;
                DGTweeningPathMode_TypeID = getTypeId(L, typeof(DG.Tweening.PathMode), out is_first);
				
				if (DGTweeningPathMode_EnumRef == -1)
				{
				    Utils.LoadCSTable(L, typeof(DG.Tweening.PathMode));
				    DGTweeningPathMode_EnumRef = LuaAPI.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
				}
				
            }
			
			if (LuaAPI.xlua_tryget_cachedud(L, (int)val, DGTweeningPathMode_EnumRef) == 1)
            {
			    return;
			}
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 4, DGTweeningPathMode_TypeID);
            if (!CopyByValue.Pack(buff, 0, (int)val))
            {
                throw new Exception("pack fail fail for DG.Tweening.PathMode ,value="+val);
            }
			
			LuaAPI.lua_getref(L, DGTweeningPathMode_EnumRef);
			LuaAPI.lua_pushvalue(L, -2);
			LuaAPI.xlua_rawseti(L, -2, (int)val);
			LuaAPI.lua_pop(L, 1);
			
        }
		
        public void Get(RealStatePtr L, int index, out DG.Tweening.PathMode val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != DGTweeningPathMode_TypeID)
				{
				    throw new Exception("invalid userdata for DG.Tweening.PathMode");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
				int e;
                if (!CopyByValue.UnPack(buff, 0, out e))
                {
                    throw new Exception("unpack fail for DG.Tweening.PathMode");
                }
				val = (DG.Tweening.PathMode)e;
                
            }
            else
            {
                val = (DG.Tweening.PathMode)objectCasters.GetCaster(typeof(DG.Tweening.PathMode))(L, index, null);
            }
        }
		
        public void UpdateDGTweeningPathMode(RealStatePtr L, int index, DG.Tweening.PathMode val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != DGTweeningPathMode_TypeID)
				{
				    throw new Exception("invalid userdata for DG.Tweening.PathMode");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  (int)val))
                {
                    throw new Exception("pack fail for DG.Tweening.PathMode ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        int DGTweeningRotateMode_TypeID = -1;
		int DGTweeningRotateMode_EnumRef = -1;
        
        public void PushDGTweeningRotateMode(RealStatePtr L, DG.Tweening.RotateMode val)
        {
            if (DGTweeningRotateMode_TypeID == -1)
            {
			    bool is_first;
                DGTweeningRotateMode_TypeID = getTypeId(L, typeof(DG.Tweening.RotateMode), out is_first);
				
				if (DGTweeningRotateMode_EnumRef == -1)
				{
				    Utils.LoadCSTable(L, typeof(DG.Tweening.RotateMode));
				    DGTweeningRotateMode_EnumRef = LuaAPI.luaL_ref(L, LuaIndexes.LUA_REGISTRYINDEX);
				}
				
            }
			
			if (LuaAPI.xlua_tryget_cachedud(L, (int)val, DGTweeningRotateMode_EnumRef) == 1)
            {
			    return;
			}
			
            IntPtr buff = LuaAPI.xlua_pushstruct(L, 4, DGTweeningRotateMode_TypeID);
            if (!CopyByValue.Pack(buff, 0, (int)val))
            {
                throw new Exception("pack fail fail for DG.Tweening.RotateMode ,value="+val);
            }
			
			LuaAPI.lua_getref(L, DGTweeningRotateMode_EnumRef);
			LuaAPI.lua_pushvalue(L, -2);
			LuaAPI.xlua_rawseti(L, -2, (int)val);
			LuaAPI.lua_pop(L, 1);
			
        }
		
        public void Get(RealStatePtr L, int index, out DG.Tweening.RotateMode val)
        {
		    LuaTypes type = LuaAPI.lua_type(L, index);
            if (type == LuaTypes.LUA_TUSERDATA )
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != DGTweeningRotateMode_TypeID)
				{
				    throw new Exception("invalid userdata for DG.Tweening.RotateMode");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
				int e;
                if (!CopyByValue.UnPack(buff, 0, out e))
                {
                    throw new Exception("unpack fail for DG.Tweening.RotateMode");
                }
				val = (DG.Tweening.RotateMode)e;
                
            }
            else
            {
                val = (DG.Tweening.RotateMode)objectCasters.GetCaster(typeof(DG.Tweening.RotateMode))(L, index, null);
            }
        }
		
        public void UpdateDGTweeningRotateMode(RealStatePtr L, int index, DG.Tweening.RotateMode val)
        {
		    
            if (LuaAPI.lua_type(L, index) == LuaTypes.LUA_TUSERDATA)
            {
			    if (LuaAPI.xlua_gettypeid(L, index) != DGTweeningRotateMode_TypeID)
				{
				    throw new Exception("invalid userdata for DG.Tweening.RotateMode");
				}
				
                IntPtr buff = LuaAPI.lua_touserdata(L, index);
                if (!CopyByValue.Pack(buff, 0,  (int)val))
                {
                    throw new Exception("pack fail for DG.Tweening.RotateMode ,value="+val);
                }
            }
			
            else
            {
                throw new Exception("try to update a data with lua type:" + LuaAPI.lua_type(L, index));
            }
        }
        
        
		// table cast optimze
		
        
    }
	
	public partial class StaticLuaCallbacks
    {
	    internal static bool __tryArrayGet(Type type, RealStatePtr L, ObjectTranslator translator, object obj, int index)
		{
		
			if (type == typeof(UnityEngine.Vector2[]))
			{
			    UnityEngine.Vector2[] array = obj as UnityEngine.Vector2[];
				translator.PushUnityEngineVector2(L, array[index]);
				return true;
			}
			else if (type == typeof(UnityEngine.Vector3[]))
			{
			    UnityEngine.Vector3[] array = obj as UnityEngine.Vector3[];
				translator.PushUnityEngineVector3(L, array[index]);
				return true;
			}
			else if (type == typeof(UnityEngine.Vector4[]))
			{
			    UnityEngine.Vector4[] array = obj as UnityEngine.Vector4[];
				translator.PushUnityEngineVector4(L, array[index]);
				return true;
			}
			else if (type == typeof(UnityEngine.Color[]))
			{
			    UnityEngine.Color[] array = obj as UnityEngine.Color[];
				translator.PushUnityEngineColor(L, array[index]);
				return true;
			}
			else if (type == typeof(UnityEngine.Quaternion[]))
			{
			    UnityEngine.Quaternion[] array = obj as UnityEngine.Quaternion[];
				translator.PushUnityEngineQuaternion(L, array[index]);
				return true;
			}
			else if (type == typeof(UnityEngine.Ray[]))
			{
			    UnityEngine.Ray[] array = obj as UnityEngine.Ray[];
				translator.PushUnityEngineRay(L, array[index]);
				return true;
			}
			else if (type == typeof(UnityEngine.Bounds[]))
			{
			    UnityEngine.Bounds[] array = obj as UnityEngine.Bounds[];
				translator.PushUnityEngineBounds(L, array[index]);
				return true;
			}
			else if (type == typeof(UnityEngine.Ray2D[]))
			{
			    UnityEngine.Ray2D[] array = obj as UnityEngine.Ray2D[];
				translator.PushUnityEngineRay2D(L, array[index]);
				return true;
			}
			else if (type == typeof(XLuaTest.Pedding[]))
			{
			    XLuaTest.Pedding[] array = obj as XLuaTest.Pedding[];
				translator.PushXLuaTestPedding(L, array[index]);
				return true;
			}
			else if (type == typeof(XLuaTest.MyStruct[]))
			{
			    XLuaTest.MyStruct[] array = obj as XLuaTest.MyStruct[];
				translator.PushXLuaTestMyStruct(L, array[index]);
				return true;
			}
			else if (type == typeof(XLuaTest.PushAsTableStruct[]))
			{
			    XLuaTest.PushAsTableStruct[] array = obj as XLuaTest.PushAsTableStruct[];
				translator.PushXLuaTestPushAsTableStruct(L, array[index]);
				return true;
			}
			else if (type == typeof(Tutorial.TestEnum[]))
			{
			    Tutorial.TestEnum[] array = obj as Tutorial.TestEnum[];
				translator.PushTutorialTestEnum(L, array[index]);
				return true;
			}
			else if (type == typeof(XLuaTest.MyEnum[]))
			{
			    XLuaTest.MyEnum[] array = obj as XLuaTest.MyEnum[];
				translator.PushXLuaTestMyEnum(L, array[index]);
				return true;
			}
			else if (type == typeof(Tutorial.DerivedClass.TestEnumInner[]))
			{
			    Tutorial.DerivedClass.TestEnumInner[] array = obj as Tutorial.DerivedClass.TestEnumInner[];
				translator.PushTutorialDerivedClassTestEnumInner(L, array[index]);
				return true;
			}
			else if (type == typeof(DG.Tweening.Ease[]))
			{
			    DG.Tweening.Ease[] array = obj as DG.Tweening.Ease[];
				translator.PushDGTweeningEase(L, array[index]);
				return true;
			}
			else if (type == typeof(DG.Tweening.PathType[]))
			{
			    DG.Tweening.PathType[] array = obj as DG.Tweening.PathType[];
				translator.PushDGTweeningPathType(L, array[index]);
				return true;
			}
			else if (type == typeof(DG.Tweening.PathMode[]))
			{
			    DG.Tweening.PathMode[] array = obj as DG.Tweening.PathMode[];
				translator.PushDGTweeningPathMode(L, array[index]);
				return true;
			}
			else if (type == typeof(DG.Tweening.RotateMode[]))
			{
			    DG.Tweening.RotateMode[] array = obj as DG.Tweening.RotateMode[];
				translator.PushDGTweeningRotateMode(L, array[index]);
				return true;
			}
            return false;
		}
		
		internal static bool __tryArraySet(Type type, RealStatePtr L, ObjectTranslator translator, object obj, int array_idx, int obj_idx)
		{
		
			if (type == typeof(UnityEngine.Vector2[]))
			{
			    UnityEngine.Vector2[] array = obj as UnityEngine.Vector2[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(UnityEngine.Vector3[]))
			{
			    UnityEngine.Vector3[] array = obj as UnityEngine.Vector3[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(UnityEngine.Vector4[]))
			{
			    UnityEngine.Vector4[] array = obj as UnityEngine.Vector4[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(UnityEngine.Color[]))
			{
			    UnityEngine.Color[] array = obj as UnityEngine.Color[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(UnityEngine.Quaternion[]))
			{
			    UnityEngine.Quaternion[] array = obj as UnityEngine.Quaternion[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(UnityEngine.Ray[]))
			{
			    UnityEngine.Ray[] array = obj as UnityEngine.Ray[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(UnityEngine.Bounds[]))
			{
			    UnityEngine.Bounds[] array = obj as UnityEngine.Bounds[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(UnityEngine.Ray2D[]))
			{
			    UnityEngine.Ray2D[] array = obj as UnityEngine.Ray2D[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(XLuaTest.Pedding[]))
			{
			    XLuaTest.Pedding[] array = obj as XLuaTest.Pedding[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(XLuaTest.MyStruct[]))
			{
			    XLuaTest.MyStruct[] array = obj as XLuaTest.MyStruct[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(XLuaTest.PushAsTableStruct[]))
			{
			    XLuaTest.PushAsTableStruct[] array = obj as XLuaTest.PushAsTableStruct[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(Tutorial.TestEnum[]))
			{
			    Tutorial.TestEnum[] array = obj as Tutorial.TestEnum[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(XLuaTest.MyEnum[]))
			{
			    XLuaTest.MyEnum[] array = obj as XLuaTest.MyEnum[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(Tutorial.DerivedClass.TestEnumInner[]))
			{
			    Tutorial.DerivedClass.TestEnumInner[] array = obj as Tutorial.DerivedClass.TestEnumInner[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(DG.Tweening.Ease[]))
			{
			    DG.Tweening.Ease[] array = obj as DG.Tweening.Ease[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(DG.Tweening.PathType[]))
			{
			    DG.Tweening.PathType[] array = obj as DG.Tweening.PathType[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(DG.Tweening.PathMode[]))
			{
			    DG.Tweening.PathMode[] array = obj as DG.Tweening.PathMode[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
			else if (type == typeof(DG.Tweening.RotateMode[]))
			{
			    DG.Tweening.RotateMode[] array = obj as DG.Tweening.RotateMode[];
				translator.Get(L, obj_idx, out array[array_idx]);
				return true;
			}
            return false;
		}
	}
}