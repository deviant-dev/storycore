using System;
using System.Collections.Generic;
using System.Linq;
using StoryCore.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace StoryCore.AssetBuckets {
    public static class AssetBucketWatcher {
        private static bool s_WillSaveAssets;
        private static BaseAssetBucket[] s_Buckets;
        private static string[] s_LastBuckets;

        private static IEnumerable<BaseAssetBucket> Buckets {
            get {
                string[] newBuckets = AssetDatabase.FindAssets($"t:{nameof(BaseAssetBucket)}");

                if (s_LastBuckets == null || !newBuckets.IsEqual(s_LastBuckets)) {
                    s_LastBuckets = newBuckets;
                    s_Buckets = newBuckets
                                .Select(AssetDatabase.GUIDToAssetPath)
                                .Select(AssetDatabase.LoadAssetAtPath<BaseAssetBucket>)
                                .ToArray();
                }

                return s_Buckets;
            }
        }

        [InitializeOnLoadMethod]
        public static void Init() {
            AssetImportTracker.DelayedAssetsChanged -= OnDatabaseChanged;
            AssetImportTracker.DelayedAssetsChanged += OnDatabaseChanged;
        }

        private static HashSet<string> s_ChangedDirectories;

        private static void OnDatabaseChanged(AssetChanges changes) {
            HashSet<string> changedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            changes.Imported.ForEach(f => AddPath(f, changedDirectories));
            changes.Deleted.ForEach(f => AddPath(f, changedDirectories));
            changes.MovedFrom.ForEach(f => AddPath(f, changedDirectories));
            changes.MovedTo.ForEach(f => AddPath(f, changedDirectories));

            Buckets.ForEach(b => FindReferencesIfChanged(b, changes, changedDirectories));
        }

        private static void FindReferencesIfChanged(BaseAssetBucket bucket, AssetChanges changes, HashSet<string> changedDirectories) {
            // Skip if bucket is null.
            if (!bucket) {
                return;
            }

            // Skip if bucket is updated manually.
            if (bucket.ManualUpdate) {
                return;
            }

            // Skip is no source paths are in the changed directories.
            string[] sourcePaths = bucket.EDITOR_Sources.Where(o => o).Select(AssetDatabase.GetAssetPath).ToArray();

            if (changedDirectories != null && !sourcePaths.Any(changedDirectories.Contains)) {
                return;
            }

            // Skip if all imported files already exist in the bucket or can't be added to the bucket.
            if (changes.MovedFrom.Length == 0 && changes.MovedTo.Length == 0 && changes.Deleted.Length == 0 && changes.Imported.Length < 50) {
                if (changes.Imported.All(bucket.EDITOR_HasPathOrCantAdd)) {
                    return;
                }
            }

            FindReferences(bucket, sourcePaths);
        }

        public static void FindReferences(BaseAssetBucket bucket, string[] sourcePaths = null) {
            sourcePaths = sourcePaths ?? bucket.EDITOR_Sources.Where(o => o).Select(AssetDatabase.GetAssetPath).ToArray();

            string filter = bucket.AssetType.IsSubclassOf(typeof(Component)) ? "t:GameObject" : "t:" + bucket.AssetType.Name;

            string[] newPaths = AssetDatabase
                                .FindAssets(filter, sourcePaths)
                                .Select(AssetDatabase.GUIDToAssetPath).Where(p => CanBeType(p, bucket.AssetType))
                                .OrderBy(System.IO.Path.GetFileName)
                                .ToArray();

            HashSet<Object> newObjects = new HashSet<Object>(newPaths.Select(p => AssetDatabase.LoadAssetAtPath(p, bucket.AssetType)).Where(o => o && bucket.EDITOR_CanAdd(o)).OrderBy(o => o.name));

            bucket.EDITOR_Clear();
            newObjects.ForEach(bucket.EDITOR_TryAdd);
            bucket.EDITOR_Sort(AssetGuidSorter);
            EditorUtility.SetDirty(bucket);
            SaveAssetsDelayed();
            Debug.LogFormat(bucket, "<color=#6699cc>AssetBuckets</color>: Updated {0}", bucket.name);
        }

        private static void AddPath(string filePath, HashSet<string> changePaths) {
            string path = GetParentDirectory(filePath);

            while (!path.IsNullOrEmpty() && !changePaths.Contains(path)) {
                changePaths.Add(path);
                path = GetParentDirectory(path);
            }
        }

        private static string GetParentDirectory(string path) {
            int index = path.LastIndexOf('/');

            if (index <= 0) {
                return null;
            }

            return path.Substring(0, index);
        }

        private static void SaveAssetsDelayed() {
            if (s_WillSaveAssets) {
                return;
            }

            s_WillSaveAssets = true;
            EditorApplication.delayCall += SaveAssets;
        }

        private static void SaveAssets() {
            if (s_WillSaveAssets) {
                s_WillSaveAssets = false;
                AssetDatabase.SaveAssets();
            }
        }

        private static bool ContainsAll(IReadOnlyCollection<Object> set, IReadOnlyCollection<Object> list2) {
            return set.Count == list2.Count && list2.All(set.Contains);
        }

        private static bool IsEqual(string[] newPaths, IReadOnlyCollection<Object> objects) {
            if (newPaths.Length != objects.Count) {
                return false;
            }

            string[] currentPaths = objects.Where(o => o).Select(AssetDatabase.GetAssetPath).OrderBy(p => p).ToArray();
            return newPaths.IsEqual(currentPaths, (a, b) => a.Equals(b, StringComparison.OrdinalIgnoreCase));
        }

        private static bool CanBeType(string path, Type testType) {
            Type assetType = AssetDatabase.GetMainAssetTypeAtPath(path);

            if (assetType == null) {
                return false;
            }

            if (assetType == testType || assetType.IsSubclassOf(testType)) {
                return true;
            }

            return assetType == typeof(GameObject) && typeof(Component).IsAssignableFrom(testType);
        }

        private static int AssetGuidSorter(Object a, Object b) {
            if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(a, out string aGuid, out long _) &&
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(b, out string bGuid, out long _)) {
                return string.CompareOrdinal(aGuid, bGuid);
            }

            return string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase);
        }
    }
}