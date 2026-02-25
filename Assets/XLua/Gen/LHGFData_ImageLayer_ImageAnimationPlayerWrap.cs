#if USE_UNI_LUA
using LuaAPI = UniLua.Lua;
using RealStatePtr = UniLua.ILuaState;
using LuaCSFunction = UniLua.CSharpFunctionDelegate;
#else
using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;
#endif

using XLua;
using System.Collections.Generic;


namespace XLua.CSObjectWrap
{
    using Utils = XLua.Utils;
    public class LHGFDataImageLayerImageAnimationPlayerWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(LHGFData.ImageLayer.ImageAnimationPlayer);
			Utils.BeginObjectRegister(type, L, translator, 0, 1, 4, 4);
			
			Utils.RegisterFunc(L, Utils.METHOD_IDX, "Play", _m_Play);
			
			
			Utils.RegisterFunc(L, Utils.GETTER_IDX, "Commnad", _g_get_Commnad);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "Config", _g_get_Config);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "G", _g_get_G);
            Utils.RegisterFunc(L, Utils.GETTER_IDX, "sequence", _g_get_sequence);
            
			Utils.RegisterFunc(L, Utils.SETTER_IDX, "Commnad", _s_set_Commnad);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "Config", _s_set_Config);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "G", _s_set_G);
            Utils.RegisterFunc(L, Utils.SETTER_IDX, "sequence", _s_set_sequence);
            
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 1, 0, 0);
			
			
            
			
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            
			try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
				if(LuaAPI.lua_gettop(L) == 4 && (LuaAPI.lua_isnil(L, 2) || LuaAPI.lua_type(L, 2) == LuaTypes.LUA_TSTRING) && translator.Assignable<System.Collections.Generic.Dictionary<string, string>>(L, 3) && translator.Assignable<UnityEngine.GameObject>(L, 4))
				{
					string _commnad = LuaAPI.lua_tostring(L, 2);
					System.Collections.Generic.Dictionary<string, string> _config = (System.Collections.Generic.Dictionary<string, string>)translator.GetObject(L, 3, typeof(System.Collections.Generic.Dictionary<string, string>));
					UnityEngine.GameObject _g = (UnityEngine.GameObject)translator.GetObject(L, 4, typeof(UnityEngine.GameObject));
					
					var gen_ret = new LHGFData.ImageLayer.ImageAnimationPlayer(_commnad, _config, _g);
					translator.Push(L, gen_ret);
                    
					return 1;
				}
				
			}
			catch(System.Exception gen_e) {
				return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
			}
            return LuaAPI.luaL_error(L, "invalid arguments to LHGFData.ImageLayer.ImageAnimationPlayer constructor!");
            
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_Play(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
                LHGFData.ImageLayer.ImageAnimationPlayer gen_to_be_invoked = (LHGFData.ImageLayer.ImageAnimationPlayer)translator.FastGetCSObj(L, 1);
            
            
                
                {
                    
                    gen_to_be_invoked.Play(  );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_Commnad(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                LHGFData.ImageLayer.ImageAnimationPlayer gen_to_be_invoked = (LHGFData.ImageLayer.ImageAnimationPlayer)translator.FastGetCSObj(L, 1);
                LuaAPI.lua_pushstring(L, gen_to_be_invoked.Commnad);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_Config(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                LHGFData.ImageLayer.ImageAnimationPlayer gen_to_be_invoked = (LHGFData.ImageLayer.ImageAnimationPlayer)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.Config);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_G(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                LHGFData.ImageLayer.ImageAnimationPlayer gen_to_be_invoked = (LHGFData.ImageLayer.ImageAnimationPlayer)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.G);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _g_get_sequence(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                LHGFData.ImageLayer.ImageAnimationPlayer gen_to_be_invoked = (LHGFData.ImageLayer.ImageAnimationPlayer)translator.FastGetCSObj(L, 1);
                translator.Push(L, gen_to_be_invoked.sequence);
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 1;
        }
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_Commnad(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                LHGFData.ImageLayer.ImageAnimationPlayer gen_to_be_invoked = (LHGFData.ImageLayer.ImageAnimationPlayer)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.Commnad = LuaAPI.lua_tostring(L, 2);
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_Config(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                LHGFData.ImageLayer.ImageAnimationPlayer gen_to_be_invoked = (LHGFData.ImageLayer.ImageAnimationPlayer)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.Config = (System.Collections.Generic.Dictionary<string, string>)translator.GetObject(L, 2, typeof(System.Collections.Generic.Dictionary<string, string>));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_G(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                LHGFData.ImageLayer.ImageAnimationPlayer gen_to_be_invoked = (LHGFData.ImageLayer.ImageAnimationPlayer)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.G = (UnityEngine.GameObject)translator.GetObject(L, 2, typeof(UnityEngine.GameObject));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _s_set_sequence(RealStatePtr L)
        {
		    try {
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			
                LHGFData.ImageLayer.ImageAnimationPlayer gen_to_be_invoked = (LHGFData.ImageLayer.ImageAnimationPlayer)translator.FastGetCSObj(L, 1);
                gen_to_be_invoked.sequence = (DG.Tweening.Sequence)translator.GetObject(L, 2, typeof(DG.Tweening.Sequence));
            
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            return 0;
        }
        
		
		
		
		
    }
}
