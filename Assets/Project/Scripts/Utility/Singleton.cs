using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    static public T Instance => instance;

    static private T instance = null;


    private void Awake() => instance = this as T;
}
