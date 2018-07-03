using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use for classes that should only ever be instantiated once. 
/// Used for :
///     Event Manager
///     Scene Manager
/// We do not want events responed to twice or scenes loaded twice.
/// </summary>
/// <typeparam name="T"></typeparam>
public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    protected static T _instance;

    public static T instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (T)FindObjectOfType(typeof(T));
            }
            return _instance;
        }
    }
}