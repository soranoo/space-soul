using UnityEngine;

public abstract class SingletonBase<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    /// <summary>
    /// Global access to the singleton instance.
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<T>();

                if (_instance == null)
                {
                    // If no instance exists, create instance
                    GameObject singletonObject = new GameObject(typeof(T).Name);
                    _instance = singletonObject.AddComponent<T>();
                }
            }

            return _instance;
        }
    }

    /// <summary>
    /// Enforces single instance.
    /// </summary>
    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            // If another instance exists, destroy this one
            Destroy(gameObject);
            return;
        }

        _instance = this as T;
    }
}
