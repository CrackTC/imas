using UnityEngine;

namespace Imas
{
    class MBSingleton<T> : MonoBehaviour
        where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (this != Instance)
            {
                Destroy(gameObject);
            }
        }
    }
}
