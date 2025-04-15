using System;
using System.Linq;
using System.Reflection;

public static class ReflectionUtility
{

    static readonly object[] emptyArgs = { };

    const BindingFlags Instance = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
    const BindingFlags InstanceNoFlat = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
    const BindingFlags Static = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

    public static object ICall(object obj, string methodName)
    {
        var dyn = GetMethod(obj.GetType(), methodName, Instance, emptyArgs);
        return dyn.Invoke(obj, emptyArgs);
    }

    public static object ICall(object obj, string methodName, params object[] args)
    {
        var dyn = GetMethod(obj.GetType(), methodName, Instance, args);
        return dyn.Invoke(obj, args);
    }

    public static T ICall<T>(object obj, string methodName, params object[] args)
    {
        var dyn = GetMethod(obj.GetType(), methodName, Instance, args);
        return (T)dyn.Invoke(obj, args);
    }

    public static object ICallOverride(object obj, string methodName, params object[] args)
    {
        var dyn = GetMethod(obj.GetType(), methodName, InstanceNoFlat, args);
        return dyn.Invoke(obj, args);
    }

    public static object IGetField(object obj, string memberName)
    {
        var dyn = obj.GetType().GetField(memberName, Instance);
        return dyn.GetValue(obj);
    }

    public static T IGetField<T>(object obj, string memberName)
    {
        var dyn = obj.GetType().GetField(memberName, Instance);
        return (T)dyn.GetValue(obj);
    }

    public static void ISetField(object obj, string memberName, object value)
    {
        var dyn = obj.GetType().GetField(memberName, Instance);
        dyn.SetValue(obj, value);
    }


    public static T IGetProp<T>(object obj, string memberName)
    {
        var dyn = obj.GetType().GetMethod("get_" + memberName, Instance);
        return (T)dyn.Invoke(obj, emptyArgs);
    }

    public static void ISetProp(object obj, string memberName, object value)
    {
        var dyn = obj.GetType().GetMethod("set_" + memberName, Instance);
        dyn.Invoke(obj, new object[] { value });
    }


    /// ---------------------------STATIC---------------------------------------

    public static Type GetTypeFrom(Assembly assembly, string typeName)
    {
        return assembly.GetTypes().First(x => x.Name == typeName);
    }

    public static object INewCall(Type type, params object[] args)
    {
        var dyns = type.GetConstructors();
        var dyn = dyns.First(x => x.GetParameters().Length == args.Length);
        return dyn.Invoke(null, args);
    }

    public static object IGetStaticField(Type type, string memberName)
    {
        var dyn = type.GetField(memberName, Static);
        return dyn.GetValue(null);
    }

    public static void ISetStaticField(Type type, string memberName, object value)
    {
        var dyn = type.GetField(memberName, Static);
        dyn.SetValue(null, value);
    }

    public static object IStaticCall(Type type, string methodName)
    {
        var dyn = GetMethod(type, methodName, Static, emptyArgs);
        return dyn.Invoke(null, emptyArgs);
    }

    public static object IStaticCall(Type type, string methodName, params object[] args)
    {
        var dyn = GetMethod(type, methodName, Static, args);
        return dyn.Invoke(null, args);
    }

    private static MethodInfo GetMethod(Type type, string methodName, BindingFlags flags, object[] args)
    {
        try
        {
            return type.GetMethod(methodName, flags);
        }
        catch (AmbiguousMatchException)
        {
            return type.GetMethod(methodName, flags, null, args.Select(x => x.GetType()).ToArray(), null);
        }
    }
}