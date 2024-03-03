using UnityEngine;

namespace ServerBrowser.Util
{
    internal static class GameObjectExtensions
    {
        internal static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
            => gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
    }
}