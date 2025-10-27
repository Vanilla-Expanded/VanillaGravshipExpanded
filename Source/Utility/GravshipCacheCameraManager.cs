using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded
{
    [StaticConstructorOnStartup]
    public static class GravshipCacheCameraManager
    {
        private static Camera gravshipCacheCameraInt;

        public static Camera GravshipCacheCamera => gravshipCacheCameraInt;

        static GravshipCacheCameraManager()
        {
            gravshipCacheCameraInt = CreateGravshipCacheCamera();
        }

        private static Camera CreateGravshipCacheCamera()
        {
            GameObject gameObject = new GameObject("GravshipCacheCamera", typeof(Camera));
            gameObject.SetActive(false);
            Object.DontDestroyOnLoad(gameObject);
            Camera component = gameObject.GetComponent<Camera>();
            component.transform.position = new Vector3(0f, 1000f, 0f);
            component.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            component.orthographic = true;
            component.cullingMask = 0;
            component.orthographicSize = 1f;
            component.clearFlags = CameraClearFlags.Color;
            component.backgroundColor = new Color(0f, 0f, 0f, 0f);
            component.useOcclusionCulling = false;
            component.renderingPath = RenderingPath.Forward;
            return component;
        }
    }
}