using System.Collections.Generic;
using UnityEngine;

//It's okay to remove this if you wanted to
namespace TexDrawLib
{
    //This one is used for getting a List class
    public static class ListPool<T>
    {
        // Object pool to avoid allocations.
        private static readonly ObjectPool<List<T>> s_ListPool = new ObjectPool<List<T>>();

        private static readonly bool m_IsImplementIFlusable = typeof(IFlushable).IsAssignableFrom(typeof(T));

        /// Get a new list instance
        /// Replacement for new List<T>()
        public static List<T> Get()
        {
            //if(typeof(T) == typeof(Box))
            //   Debug.LogWarning("POP " + Time.frameCount);

            return s_ListPool.Get();
        }

        public static List<T> Get(IEnumerable<T> list)
        {
            //if(typeof(T) == typeof(Box))
            //   Debug.LogWarning("POP " + Time.frameCount);

            var l = s_ListPool.Get();
            l.AddRange(list);
            return l;
        }

        /// Releasing this list with its children if possible
        public static void FlushAndRelease(List<T> toRelease)
        {
            if (m_IsImplementIFlusable && toRelease.Count > 0)
            {
                for (int i = 0; i < toRelease.Count; i++)
                {
                    ((IFlushable)toRelease[i]).Flush();
                }
            }
            Release(toRelease);
        }

        /// Releasing this list without flushing the childs
        /// used if reference child is still used somewhere
        public static void Release(List<T> toRelease)
        {
            toRelease.Clear();
            //if(typeof(T) == typeof(Box))
            //    Debug.Log("PUSH " + Time.frameCount);
            s_ListPool.Release(toRelease);
        }
    }

    public static class ObjPool<T> where T : class, IFlushable, new()
    {
        // Object pool to avoid allocations.
        private static readonly ObjectPool<T> s_ObjPool = new ObjectPool<T>();

        public static T Get()
        {
            T obj = s_ObjPool.Get();
            obj.IsFlushed = false;
            return obj;
        }

        public static void Release(T toRelease)
        {
            if (!toRelease.IsFlushed)
            {
                toRelease.IsFlushed = true;
                s_ObjPool.Release(toRelease);
            }
        }
    }

    //Interface to get a class to be flushable (flush means to be released to the main class stack
    //when it's unused, later if code need a new instance, the main stack will give this class back
    //instead of creating a new instance (which later introducing Memory Garbages)).
    public interface IFlushable
    {
        bool IsFlushed { get; set; }

        void Flush();
    }

    /* Example of Implementation: (Copy snippet code below as Template)

	using TexDrawLib;
	public class SomeClass : IFlushable
	{
		/// This class is replacement for New()
		public static SomeClass Get()
		{
			var obj = ObjPool<SomeClass>.Get();
			// Set up some stuff here
			return obj;
		}

		/// used Internally, check whether it's already released or not
		/// Public for convenience, you shouldn't set this manually
		public bool IsFlushed { get; set; }

		// Call this in your code if this class is in no longer use
		public void Flush()
		{
			// Reset additional stuff, properties, variables, all have to be set to it's default value.
			// then you can ...
			ObjPool<SomeClass>.Release(this);
		}
	}

	*/
}
