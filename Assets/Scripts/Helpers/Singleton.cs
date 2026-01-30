using System;
using UnityEngine;

namespace Helpers
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (!_instance) _instance = FindObjectOfType<T>();
                if (!_instance) Debug.LogError("Singleton of type : " + typeof(T).Name + " not found on scene");

                return _instance;
            }
        }

        protected void InitializeSingleton(bool persistent = true)
        {
            if (_instance == null)
            {
                _instance = (T)Convert.ChangeType(this, typeof(T));
                if (persistent) DontDestroyOnLoad(_instance);
            }
            else
            {
                Debug.LogWarning($"Another instance of Singleton<{typeof(T).Name}> detected on GO {name}. Destroyed", gameObject);
                Destroy(this);
            }
        }
    }
}