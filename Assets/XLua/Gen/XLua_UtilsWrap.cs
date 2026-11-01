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
    public class XLuaUtilsWrap 
    {
        public static void __Register(RealStatePtr L)
        {
			ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
			System.Type type = typeof(XLua.Utils);
			Utils.BeginObjectRegister(type, L, translator, 0, 0, 0, 0);
			
			
			
			
			
			
			Utils.EndObjectRegister(type, L, translator, null, null,
			    null, null, null);

		    Utils.BeginClassRegister(type, L, __CreateInstance, 34, 0, 0);
			Utils.RegisterFunc(L, Utils.CLS_IDX, "LoadField", _m_LoadField_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetMainState", _m_GetMainState_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "GetAllTypes", _m_GetAllTypes_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "loadUpvalue", _m_loadUpvalue_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "RegisterEnumType", _m_RegisterEnumType_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "MakePrivateAccessible", _m_MakePrivateAccessible_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "ReflectionWrap", _m_ReflectionWrap_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "BeginObjectRegister", _m_BeginObjectRegister_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "EndObjectRegister", _m_EndObjectRegister_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "RegisterFunc", _m_RegisterFunc_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "RegisterLazyFunc", _m_RegisterLazyFunc_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "RegisterObject", _m_RegisterObject_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "BeginClassRegister", _m_BeginClassRegister_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "EndClassRegister", _m_EndClassRegister_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "LoadCSTable", _m_LoadCSTable_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "SetCSTable", _m_SetCSTable_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "IsParamsMatch", _m_IsParamsMatch_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "IsSupportedMethod", _m_IsSupportedMethod_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "MakeGenericMethodWithConstraints", _m_MakeGenericMethodWithConstraints_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "IsStaticPInvokeCSFunction", _m_IsStaticPInvokeCSFunction_xlua_st_);
            Utils.RegisterFunc(L, Utils.CLS_IDX, "IsPublic", _m_IsPublic_xlua_st_);
            
			
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "OBJ_META_IDX", XLua.Utils.OBJ_META_IDX);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "METHOD_IDX", XLua.Utils.METHOD_IDX);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "GETTER_IDX", XLua.Utils.GETTER_IDX);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "SETTER_IDX", XLua.Utils.SETTER_IDX);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "CLS_IDX", XLua.Utils.CLS_IDX);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "CLS_META_IDX", XLua.Utils.CLS_META_IDX);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "CLS_GETTER_IDX", XLua.Utils.CLS_GETTER_IDX);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "CLS_SETTER_IDX", XLua.Utils.CLS_SETTER_IDX);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "LuaIndexsFieldName", XLua.Utils.LuaIndexsFieldName);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "LuaNewIndexsFieldName", XLua.Utils.LuaNewIndexsFieldName);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "LuaClassIndexsFieldName", XLua.Utils.LuaClassIndexsFieldName);
            Utils.RegisterObject(L, translator, Utils.CLS_IDX, "LuaClassNewIndexsFieldName", XLua.Utils.LuaClassNewIndexsFieldName);
            
			
			
			
			Utils.EndClassRegister(type, L, translator);
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int __CreateInstance(RealStatePtr L)
        {
            return LuaAPI.luaL_error(L, "XLua.Utils does not have a constructor!");
        }
        
		
        
		
        
        
        
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_LoadField_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    System.IntPtr _L = LuaAPI.lua_touserdata(L, 1);
                    int _idx = LuaAPI.xlua_tointeger(L, 2);
                    string _field_name = LuaAPI.lua_tostring(L, 3);
                    
                        var gen_ret = XLua.Utils.LoadField( _L, _idx, _field_name );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetMainState_xlua_st_(RealStatePtr L)
        {
		    try {
            
            
            
                
                {
                    System.IntPtr _L = LuaAPI.lua_touserdata(L, 1);
                    
                        var gen_ret = XLua.Utils.GetMainState( _L );
                        LuaAPI.lua_pushlightuserdata(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_GetAllTypes_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 1&& LuaTypes.LUA_TBOOLEAN == LuaAPI.lua_type(L, 1)) 
                {
                    bool _exclude_generic_definition = LuaAPI.lua_toboolean(L, 1);
                    
                        var gen_ret = XLua.Utils.GetAllTypes( _exclude_generic_definition );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                if(gen_param_count == 0) 
                {
                    
                        var gen_ret = XLua.Utils.GetAllTypes(  );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to XLua.Utils.GetAllTypes!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_loadUpvalue_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    System.IntPtr _L = LuaAPI.lua_touserdata(L, 1);
                    System.Type _type = (System.Type)translator.GetObject(L, 2, typeof(System.Type));
                    string _metafunc = LuaAPI.lua_tostring(L, 3);
                    int _index = LuaAPI.xlua_tointeger(L, 4);
                    
                    XLua.Utils.loadUpvalue( _L, _type, _metafunc, _index );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RegisterEnumType_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    System.IntPtr _L = LuaAPI.lua_touserdata(L, 1);
                    System.Type _type = (System.Type)translator.GetObject(L, 2, typeof(System.Type));
                    
                    XLua.Utils.RegisterEnumType( _L, _type );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_MakePrivateAccessible_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    System.IntPtr _L = LuaAPI.lua_touserdata(L, 1);
                    System.Type _type = (System.Type)translator.GetObject(L, 2, typeof(System.Type));
                    
                    XLua.Utils.MakePrivateAccessible( _L, _type );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_ReflectionWrap_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    System.IntPtr _L = LuaAPI.lua_touserdata(L, 1);
                    System.Type _type = (System.Type)translator.GetObject(L, 2, typeof(System.Type));
                    bool _privateAccessible = LuaAPI.lua_toboolean(L, 3);
                    
                    XLua.Utils.ReflectionWrap( _L, _type, _privateAccessible );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_BeginObjectRegister_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
			    int gen_param_count = LuaAPI.lua_gettop(L);
            
                if(gen_param_count == 8&& translator.Assignable<System.Type>(L, 1)&& LuaTypes.LUA_TLIGHTUSERDATA == LuaAPI.lua_type(L, 2)&& translator.Assignable<XLua.ObjectTranslator>(L, 3)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 4)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 5)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 6)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 7)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 8)) 
                {
                    System.Type _type = (System.Type)translator.GetObject(L, 1, typeof(System.Type));
                    System.IntPtr _L = LuaAPI.lua_touserdata(L, 2);
                    XLua.ObjectTranslator _translator = (XLua.ObjectTranslator)translator.GetObject(L, 3, typeof(XLua.ObjectTranslator));
                    int _meta_count = LuaAPI.xlua_tointeger(L, 4);
                    int _method_count = LuaAPI.xlua_tointeger(L, 5);
                    int _getter_count = LuaAPI.xlua_tointeger(L, 6);
                    int _setter_count = LuaAPI.xlua_tointeger(L, 7);
                    int _type_id = LuaAPI.xlua_tointeger(L, 8);
                    
                    XLua.Utils.BeginObjectRegister( _type, _L, _translator, _meta_count, _method_count, _getter_count, _setter_count, _type_id );
                    
                    
                    
                    return 0;
                }
                if(gen_param_count == 7&& translator.Assignable<System.Type>(L, 1)&& LuaTypes.LUA_TLIGHTUSERDATA == LuaAPI.lua_type(L, 2)&& translator.Assignable<XLua.ObjectTranslator>(L, 3)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 4)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 5)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 6)&& LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 7)) 
                {
                    System.Type _type = (System.Type)translator.GetObject(L, 1, typeof(System.Type));
                    System.IntPtr _L = LuaAPI.lua_touserdata(L, 2);
                    XLua.ObjectTranslator _translator = (XLua.ObjectTranslator)translator.GetObject(L, 3, typeof(XLua.ObjectTranslator));
                    int _meta_count = LuaAPI.xlua_tointeger(L, 4);
                    int _method_count = LuaAPI.xlua_tointeger(L, 5);
                    int _getter_count = LuaAPI.xlua_tointeger(L, 6);
                    int _setter_count = LuaAPI.xlua_tointeger(L, 7);
                    
                    XLua.Utils.BeginObjectRegister( _type, _L, _translator, _meta_count, _method_count, _getter_count, _setter_count );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
            return LuaAPI.luaL_error(L, "invalid arguments to XLua.Utils.BeginObjectRegister!");
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_EndObjectRegister_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    System.Type _type = (System.Type)translator.GetObject(L, 1, typeof(System.Type));
                    System.IntPtr _L = LuaAPI.lua_touserdata(L, 2);
                    XLua.ObjectTranslator _translator = (XLua.ObjectTranslator)translator.GetObject(L, 3, typeof(XLua.ObjectTranslator));
                    XLua.LuaDLL.lua_CSFunction _csIndexer = translator.GetDelegate<XLua.LuaDLL.lua_CSFunction>(L, 4);
                    XLua.LuaDLL.lua_CSFunction _csNewIndexer = translator.GetDelegate<XLua.LuaDLL.lua_CSFunction>(L, 5);
                    System.Type _base_type = (System.Type)translator.GetObject(L, 6, typeof(System.Type));
                    XLua.LuaDLL.lua_CSFunction _arrayIndexer = translator.GetDelegate<XLua.LuaDLL.lua_CSFunction>(L, 7);
                    XLua.LuaDLL.lua_CSFunction _arrayNewIndexer = translator.GetDelegate<XLua.LuaDLL.lua_CSFunction>(L, 8);
                    
                    XLua.Utils.EndObjectRegister( _type, _L, _translator, _csIndexer, _csNewIndexer, _base_type, _arrayIndexer, _arrayNewIndexer );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RegisterFunc_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    System.IntPtr _L = LuaAPI.lua_touserdata(L, 1);
                    int _idx = LuaAPI.xlua_tointeger(L, 2);
                    string _name = LuaAPI.lua_tostring(L, 3);
                    XLua.LuaDLL.lua_CSFunction _func = translator.GetDelegate<XLua.LuaDLL.lua_CSFunction>(L, 4);
                    
                    XLua.Utils.RegisterFunc( _L, _idx, _name, _func );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RegisterLazyFunc_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    System.IntPtr _L = LuaAPI.lua_touserdata(L, 1);
                    int _idx = LuaAPI.xlua_tointeger(L, 2);
                    string _name = LuaAPI.lua_tostring(L, 3);
                    System.Type _type = (System.Type)translator.GetObject(L, 4, typeof(System.Type));
                    XLua.LazyMemberTypes _memberType;translator.Get(L, 5, out _memberType);
                    bool _isStatic = LuaAPI.lua_toboolean(L, 6);
                    
                    XLua.Utils.RegisterLazyFunc( _L, _idx, _name, _type, _memberType, _isStatic );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_RegisterObject_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    System.IntPtr _L = LuaAPI.lua_touserdata(L, 1);
                    XLua.ObjectTranslator _translator = (XLua.ObjectTranslator)translator.GetObject(L, 2, typeof(XLua.ObjectTranslator));
                    int _idx = LuaAPI.xlua_tointeger(L, 3);
                    string _name = LuaAPI.lua_tostring(L, 4);
                    object _obj = translator.GetObject(L, 5, typeof(object));
                    
                    XLua.Utils.RegisterObject( _L, _translator, _idx, _name, _obj );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_BeginClassRegister_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    System.Type _type = (System.Type)translator.GetObject(L, 1, typeof(System.Type));
                    System.IntPtr _L = LuaAPI.lua_touserdata(L, 2);
                    XLua.LuaDLL.lua_CSFunction _creator = translator.GetDelegate<XLua.LuaDLL.lua_CSFunction>(L, 3);
                    int _class_field_count = LuaAPI.xlua_tointeger(L, 4);
                    int _static_getter_count = LuaAPI.xlua_tointeger(L, 5);
                    int _static_setter_count = LuaAPI.xlua_tointeger(L, 6);
                    
                    XLua.Utils.BeginClassRegister( _type, _L, _creator, _class_field_count, _static_getter_count, _static_setter_count );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_EndClassRegister_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    System.Type _type = (System.Type)translator.GetObject(L, 1, typeof(System.Type));
                    System.IntPtr _L = LuaAPI.lua_touserdata(L, 2);
                    XLua.ObjectTranslator _translator = (XLua.ObjectTranslator)translator.GetObject(L, 3, typeof(XLua.ObjectTranslator));
                    
                    XLua.Utils.EndClassRegister( _type, _L, _translator );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_LoadCSTable_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    System.IntPtr _L = LuaAPI.lua_touserdata(L, 1);
                    System.Type _type = (System.Type)translator.GetObject(L, 2, typeof(System.Type));
                    
                    XLua.Utils.LoadCSTable( _L, _type );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_SetCSTable_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    System.IntPtr _L = LuaAPI.lua_touserdata(L, 1);
                    System.Type _type = (System.Type)translator.GetObject(L, 2, typeof(System.Type));
                    int _cls_table = LuaAPI.xlua_tointeger(L, 3);
                    
                    XLua.Utils.SetCSTable( _L, _type, _cls_table );
                    
                    
                    
                    return 0;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_IsParamsMatch_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    System.Reflection.MethodInfo _delegateMethod = (System.Reflection.MethodInfo)translator.GetObject(L, 1, typeof(System.Reflection.MethodInfo));
                    System.Reflection.MethodInfo _bridgeMethod = (System.Reflection.MethodInfo)translator.GetObject(L, 2, typeof(System.Reflection.MethodInfo));
                    
                        var gen_ret = XLua.Utils.IsParamsMatch( _delegateMethod, _bridgeMethod );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_IsSupportedMethod_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    System.Reflection.MethodInfo _method = (System.Reflection.MethodInfo)translator.GetObject(L, 1, typeof(System.Reflection.MethodInfo));
                    
                        var gen_ret = XLua.Utils.IsSupportedMethod( _method );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_MakeGenericMethodWithConstraints_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    System.Reflection.MethodInfo _method = (System.Reflection.MethodInfo)translator.GetObject(L, 1, typeof(System.Reflection.MethodInfo));
                    
                        var gen_ret = XLua.Utils.MakeGenericMethodWithConstraints( _method );
                        translator.Push(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_IsStaticPInvokeCSFunction_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    XLua.LuaDLL.lua_CSFunction _csFunction = translator.GetDelegate<XLua.LuaDLL.lua_CSFunction>(L, 1);
                    
                        var gen_ret = XLua.Utils.IsStaticPInvokeCSFunction( _csFunction );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        [MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
        static int _m_IsPublic_xlua_st_(RealStatePtr L)
        {
		    try {
            
                ObjectTranslator translator = ObjectTranslatorPool.Instance.Find(L);
            
            
            
                
                {
                    System.Type _type = (System.Type)translator.GetObject(L, 1, typeof(System.Type));
                    
                        var gen_ret = XLua.Utils.IsPublic( _type );
                        LuaAPI.lua_pushboolean(L, gen_ret);
                    
                    
                    
                    return 1;
                }
                
            } catch(System.Exception gen_e) {
                return LuaAPI.luaL_error(L, "c# exception:" + gen_e);
            }
            
        }
        
        
        
        
        
        
		
		
		
		
    }
}
