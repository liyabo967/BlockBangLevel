using UnityEngine;
using UnityGameFramework.Runtime;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _isQuitting = false;   // 防止退出时重建实例

    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance)
                {
                    return _instance;
                }

                // 场景中已存在则直接使用
                _instance = FindAnyObjectByType<T>();
                if (_instance)
                {
                    return _instance;
                }
                
                if (_isQuitting)
                {
                    Log.Warning($"[MonoSingleton] {typeof(T).Name} 已销毁（应用退出中），返回 null");
                    return null;
                }

                // 自动创建
                var go = new GameObject($"[Singleton] {typeof(T).Name}");
                _instance = go.AddComponent<T>();
                DontDestroyOnLoad(go);

                Debug.Log($"[MonoSingleton] 自动创建 {typeof(T).Name}");
                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        lock (_lock)
        {
            if (_instance == null)
            {
                _instance = (T)this;
                DontDestroyOnLoad(gameObject);
                _isQuitting = false;
                OnSingletonAwake();
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[MonoSingleton] 检测到重复的 {typeof(T).Name}，销毁多余实例");
                Destroy(gameObject);
            }
        }
    }

    /// <summary>替代 Awake，子类在此做初始化（无需调用 base.Awake）</summary>
    protected virtual void OnSingletonAwake() { }

    protected virtual void OnApplicationQuit()
    {
        _isQuitting = true;
        _instance = null;
    }

    protected virtual void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }
}