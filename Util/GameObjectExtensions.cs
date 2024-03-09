using UnityEngine;

namespace ServerBrowser.Util
{
    internal static class GameObjectExtensions
    {
        internal static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
            => gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();

        internal static bool TryRemoveComponent<T>(this GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            if (component != null)
            {
                Object.Destroy(component);
                return true;
            }
            return false;
        }
    }
}