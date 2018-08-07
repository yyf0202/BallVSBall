using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace HighFive
{
    public class Singleton<T> where T : Singleton<T>
    {
        private static T _Instance = null;
        protected Singleton() { }
        public static T Instance
        {
            get
            {
                if (_Instance == null)
                {
                    // get all private ctors
                    ConstructorInfo[] ctors = typeof(T).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                    // get no param ctor from ctors
                    ConstructorInfo ctor = Array.Find(ctors, c => c.GetParameters().Length == 0);
                    if (ctor == null)
                    {
                        throw new Exception("Non-public ctor() not found!");
                    }

                    // call ctor function
                    _Instance = ctor.Invoke(null) as T;
                }

                return _Instance;
            }
        }
    }
}
