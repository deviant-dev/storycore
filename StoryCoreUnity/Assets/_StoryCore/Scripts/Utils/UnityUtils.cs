using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace StoryCore.Utils {
    public static class UnityUtils {
        /// <summary>
        ///     Determines how to destroy a game object based on whether or not the game is running and we are in editor mode.
        /// </summary>
        /// <param name="obj">The game object or component of the game object to destroy.</param>
        public static void DestroyObject(Object obj) {
            DestroyObject(obj, 0);
        }

        /// <summary>
        ///     Determines how to destroy a game object based on whether or not the game is running and we are in editor mode.
        /// </summary>
        /// <param name="obj">The game object or component of the game object to destroy.</param>
        /// <param name="life">An optional delay in seconds before the object is destroyed. (Only valid during runtime.)</param>
        public static void DestroyObject(Object obj, float life) {
            GameObject go = GetGameObject(obj);

            if (!go) {
                return;
            }

            if (Application.isPlaying) {
                Object.Destroy(go, life);
            } else {
                Object.DestroyImmediate(go);
            }
        }

        /// <summary>
        ///     Determines how to destroy a component based on whether or not the game is running and we are in editor mode.
        /// </summary>
        /// <param name="component">The component to destroy.</param>
        public static void DestroyComponent(Component component) {
            if (!component) {
                return;
            }

            if (Application.isPlaying) {
                Object.Destroy(component);
            } else {
                Object.DestroyImmediate(component);
            }
        }

        public static void Quit() {
#if UNITY_EDITOR
            if (Application.isEditor) {
                UnityEditor.EditorApplication.ExitPlaymode();
                return;
            }
#endif
            Application.Quit();
        }

        /// <summary>
        ///     Gets a list of child types reflected from the given type.
        /// </summary>
        /// <typeparam name="T">The type to gather child types from.</typeparam>
        /// <returns>The list of child types.</returns>
        public static List<Type> GetChildTypes<T>() {
            List<Type> derivedTypes = new List<Type>();

            // For each assembly in this app domain
            foreach (Assembly assem in AppDomain.CurrentDomain.GetAssemblies()) {
                // For each type in the assembly
                foreach (Type type in assem.GetTypes()) {
                    // If this is derived from T and instantiable
                    if (typeof(T).IsAssignableFrom(type) && type != typeof(T) && !type.IsAbstract) {
                        // Addit to the derived type list.
                        derivedTypes.Add(type);
                    }
                }
            }

            return derivedTypes;
        }

        /// <summary>
        ///     Gets the relative path between one path and another.
        /// </summary>
        /// <param name="startPath">Starting path.</param>
        /// <param name="targetPath">Target path.</param>
        /// <returns>The relative path between the two supplied paths.</returns>
        public static string GetRelativePath(string startPath, string targetPath) {
            if (targetPath.IsNullOrEmpty() || startPath.IsNullOrEmpty()) {
                return null;
            }

            if (Directory.Exists(startPath)) {
                return GetRelativePath(new DirectoryInfo(startPath), new FileInfo(targetPath));
            }

            return GetRelativePath(new FileInfo(startPath), new FileInfo(targetPath));
        }

        /// <summary>
        ///     Gets the relative path between two files.
        /// </summary>
        /// <param name="start">Starting file.</param>
        /// <param name="target">Target file.</param>
        /// <returns>The relative path between the two supplied files.</returns>
        public static string GetRelativePath(FileSystemInfo start, FileSystemInfo target) {
            Uri uri2;

            Uri uri1 = new Uri(target.FullName);

            if (start is FileInfo) {
                uri2 = new Uri(start.FullName);
            } else {
                string folderName = start.FullName;

                if (!folderName.EndsWith(Path.DirectorySeparatorChar.ToString())) {
                    folderName += Path.DirectorySeparatorChar;
                }

                uri2 = new Uri(folderName);
            }

            Uri relativeUri = uri2.MakeRelativeUri(uri1);

            return Uri.UnescapeDataString(NormalizePath(relativeUri.ToString()));
        }

        private static string NormalizePath(string path) {
            return path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        }

        /// <summary>
        ///     Creates all necessary folders for the given path.
        /// </summary>
        /// <param name="path">Path that includes potentially missing folders.</param>
        public static void CreateFoldersFor(string path) {
            if (path.IsNullOrEmpty()) {
                Debug.LogWarning("Can't make a directory for an empty path.");
                return;
            }

            string folder = Path.GetDirectoryName(path);

            if (folder.IsNullOrEmpty() || Directory.Exists(folder)) {
                return;
            }

            Directory.CreateDirectory(folder);
        }

        /// <summary>
        ///     Gets the game object regardless of what type of thing it is.
        /// </summary>
        /// <param name="thing">The thing to check for a game object.</param>
        /// <returns>The game object of the thing.</returns>
        public static GameObject GetGameObject(Object thing) {
            Transform t = thing as Transform;
            if (t) {
                return t.gameObject;
            }
            Component c = thing as Component;
            if (c) {
                return c.gameObject;
            }
            GameObject go = thing as GameObject;
            if (go) {
                return go;
            }
            return null;
        }

        /// <summary>
        ///     Gets the transform regardless of what type of thing it is.
        /// </summary>
        /// <param name="thing">The thing to check for a transform.</param>
        /// <returns>The transform of the thing.</returns>
        public static Transform GetTransform(Object thing) {
            Transform t = thing as Transform;
            if (t) {
                return t;
            }
            Component c = thing as Component;
            if (c) {
                return c.transform;
            }
            GameObject go = thing as GameObject;
            if (go) {
                return go.transform;
            }
            return null;
        }

        public static Transform FindRecursive(Transform node, string searchName) {
            if (node.name == searchName) {
                return node;
            }

            for (int i = 0; i < node.childCount; i++) {
                Transform foundInChild = FindRecursive(node.GetChild(i), searchName);
                if (foundInChild != null) {
                    return foundInChild;
                }
            }

            return null;
        }

        public static string GetPath(GameObject root, Component target) {
            return GetPath(root.transform, target.transform);
        }

        private static string GetPath(Transform root, Transform target) {
            Transform t = target;
            string path = t.FullName(FullName.Parts.UniqueName);

            while (t.parent && t != root) {
                t = t.parent;
                path = String.Format("{0}/{1}", t.FullName(FullName.Parts.UniqueName), path);
            }

            if (root && t != root) {
                throw new Exception("Couldn't find " + target.name + " under " + root.name);
            }

            return path;
        }

        public static Dictionary<T, Transform> Orphan<T>(IEnumerable<T> things) where T : Object {
            Dictionary<T, Transform> orphans = new Dictionary<T, Transform>();
            foreach (T thing in things) {
                Transform t = GetTransform(thing);

                if (!t) {
                    continue;
                }

                orphans[thing] = t.parent;
                t.SetParent(null);
            }

            return orphans;
        }

        public static void Readopt<T>(Dictionary<T, Transform> orphans) where T : Object {
            List<KeyValuePair<T, Transform>> active = orphans.Where(kvp => kvp.Key && kvp.Value).ToList();

            foreach (KeyValuePair<T, Transform> kvp in active) {
                Transform t = GetTransform(kvp.Key);
                kvp.Value.SetParent(t);
            }
        }

        public static Camera Camera {
            get {
                if (s_Camera == null) {
                    s_Camera = Camera.allCameras.FirstOrDefault(c => c.enabled);

                    if (!s_Camera) {
                        s_Camera = Camera.main;
                    } else if (!s_Camera) {
                        s_Camera = Camera.current;
                    } else if (!s_Camera) {
                        s_Camera = Camera.allCameras.FirstOrDefault();
                    }
                }

                return s_Camera;
            }
            set {
                if (!value) {
                    return;
                }

                s_Camera = value;
                s_CameraTransform = value.transform;
            }
        }
        private static Camera s_Camera;

        public static Transform CameraTransform {
            get {
                if (s_CameraTransform == null) {
                    Camera camera = Camera;
                    if (camera != null) {
                        s_CameraTransform = camera.transform;
                    }
                }
                return s_CameraTransform;
            }
        }
        private static Transform s_CameraTransform;

        public static void SetUtilCamera(Camera c) {
            s_Camera = c;
            s_CameraTransform = c.transform;
        }

        public static bool OnCamera(Vector3 worldPosition) {
            Vector3 viewPos = Camera.WorldToViewportPoint(worldPosition);
            return viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0;
        }

        public static Vector3 GetCenter(params Transform[] components) {
            return GetBoundsOfChildren(components).center;
        }

        private static Bounds GetBoundsOfChildren(params Transform[] components) {
            bool foundBounds = false;
            Bounds bounds = new Bounds();

            components.ForEach(c => c.GetComponentsInChildren<Renderer>().ForEach(r => {
                if (!foundBounds) {
                    foundBounds = true;
                    bounds = r.bounds;
                } else {
                    bounds.Encapsulate(r.bounds);
                }
            }));

            components.ForEach(c => c.GetComponentsInChildren<Collider>().ForEach(r => {
                if (!foundBounds) {
                    foundBounds = true;
                    bounds = r.bounds;
                } else {
                    bounds.Encapsulate(r.bounds);
                }
            }));

            if (foundBounds) {
                return bounds;
            }

            components.ForEach(c => {
                Bounds b = new Bounds(c.transform.position, Vector3.one);
                if (!foundBounds) {
                    foundBounds = true;
                    bounds = b;
                } else {
                    bounds.Encapsulate(b);
                }
            });

            return bounds;
        }

        public enum BoundsType {
            All,
            Collider,
            Renderer
        }

        public static Bounds GetBounds(Transform transform, BoundsType type = BoundsType.All, Collider excluded = null) {
            Bounds bounds = new Bounds {size = Vector3.zero};
            if (!transform) {
                return bounds;
            }

            // Save initial rotation.
            Quaternion rotation = transform.localRotation;
            transform.localRotation = Quaternion.identity;
            bounds.center = transform.position;

            if (type == BoundsType.All || type == BoundsType.Collider) {
                Collider[] colliders = transform.GetComponentsInChildren<Collider>();
                foreach (Collider c in colliders) {
                    if (c != excluded && !c.isTrigger) {
                        bounds = CombineBounds(bounds, c.bounds);
                    }
                }
            }

            if (type == BoundsType.All || type == BoundsType.Renderer) {
                Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers) {
                    bounds = CombineBounds(bounds, renderer.bounds);
                }
            }

            // Restore initial rotation.
            transform.localRotation = rotation;

            return bounds;
        }

        private static Bounds CombineBounds(Bounds bounds, Bounds subBounds) {
            bounds.max = new Vector3(Mathf.Max(subBounds.max.x, bounds.max.x), Mathf.Max(subBounds.max.y, bounds.max.y),
                                     Mathf.Max(subBounds.max.z, bounds.max.z));
            bounds.min = new Vector3(Mathf.Min(subBounds.min.x, bounds.min.x), Mathf.Min(subBounds.min.y, bounds.min.y),
                                     Mathf.Min(subBounds.min.z, bounds.min.z));
            return bounds;
        }

        public static void WaitUntil(Func<bool> test, Action action, MonoBehaviour target) {
            target.StartCoroutine(WaitUntil(test, action));
        }

        private static IEnumerator WaitUntil(Func<bool> test, Action action) {
            if (action == null) {
                Debug.LogError("No action found.");
                yield break;
            }

            yield return new WaitUntil(test);

            action();
        }

        public static void WaitWhile(Func<bool> test, Action action, MonoBehaviour target) {
            target.StartCoroutine(WaitWhile(test, action));
        }

        private static IEnumerator WaitWhile(Func<bool> test, Action action) {
            if (action == null) {
                Debug.LogError("No action found.");
                yield break;
            }

            yield return new WaitWhile(test);

            action();
        }

        public static List<GameObject> GetRootObjects() {
            List<GameObject> gos = new List<GameObject>();
            for (int i = 0; i < SceneManager.sceneCount; i++) {
                Scene scene = SceneManager.GetSceneAt(i);
                gos.AddRange(scene.GetRootGameObjects());
            }
            return gos;
        }

        public static T GetOrInstantiate<T>(ref T existing, T prefab) where T : Object {
            return GetOrSet(ref existing, () => Object.Instantiate(prefab));
        }

        public static T GetOrInstantiate<T>(ref T existing) where T : ScriptableObject {
            return GetOrSet(ref existing, ScriptableObject.CreateInstance<T>);
        }

        public static T GetOrSet<T>(ref T field, Func<T> assignFunc) {
            if (field == null || field.Equals(null)) {
                field = assignFunc();
            }

            return field;
        }

        public static int Mod(int a, int b) {
            return (a%b + b)%b;
        }
    }
}