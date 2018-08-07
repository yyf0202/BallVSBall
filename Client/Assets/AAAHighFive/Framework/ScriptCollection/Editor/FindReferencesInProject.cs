using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace HighFive
{
    public class FindReferencesInProject
    {
        static private string GetRelativeAssetsPath(string path)
        {
            return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
        }

        [MenuItem("HighFive/Asset/Find References", false, 10)]
        public static void Find()
        {
            EditorSettings.serializationMode = SerializationMode.ForceText;
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!string.IsNullOrEmpty(path))
            {
                string guid = AssetDatabase.AssetPathToGUID(path);
                ArrayList withoutExtensions = new ArrayList() { ".prefab", ".unity", ".mat", ".asset" };
                string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
                    .Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
                int startIndex = 0;
                Debug.Log("Begin to Match");
                EditorApplication.update = delegate ()
                {
                    string file = files[startIndex];

                    bool isCancel = EditorUtility.DisplayCancelableProgressBar("Matching", file, (float)startIndex / (float)files.Length);

                    if (Regex.IsMatch(File.ReadAllText(file), guid))
                    {
                        Debug.Log(file, AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(GetRelativeAssetsPath(file)));
                    }

                    startIndex++;
                    if (isCancel || startIndex >= files.Length)
                    {
                        EditorUtility.ClearProgressBar();
                        EditorApplication.update = null;
                        startIndex = 0;
                        Debug.Log("Matching end");
                    }
                };
            }
        }
    }
}