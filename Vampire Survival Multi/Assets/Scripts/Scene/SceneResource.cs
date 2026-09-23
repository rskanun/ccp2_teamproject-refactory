using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;


#if UNITY_EDITOR
using UnityEditor;
#endif

public enum SceneType
{
    Booting,
    Title,
    Room,
    Loading,
    InGame,
}

public class SceneResource : ScriptableObject, ISerializationCallbackReceiver
{
    // 저장 파일 위치
    private const string FILE_DIRECTORY = "Assets/Resources/Options";
    private const string FILE_PATH = "Assets/Resources/Options/SceneResource.asset";

    private static SceneResource _instance;
    public static SceneResource Instance
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = Resources.Load<SceneResource>("Options/SceneResource");

            if (_instance == null)
            {
                // Resource 전체 탐색
                var assets = Resources.LoadAll<SceneResource>("");

                if (assets != null && assets.Length > 0)
                {
                    // 가장 처음 발견한 에셋 사용
                    return _instance = assets[0];
                }
            }

#if UNITY_EDITOR
            if (_instance == null)
            {
                // 파일 경로가 없을 경우 폴더 생성
                if (!AssetDatabase.IsValidFolder(FILE_DIRECTORY))
                {
                    string[] folders = FILE_DIRECTORY.Split('/');
                    string currentPath = folders[0];

                    for (int i = 1; i < folders.Length; i++)
                    {
                        if (!AssetDatabase.IsValidFolder(currentPath + "/" + folders[i]))
                        {
                            AssetDatabase.CreateFolder(currentPath, folders[i]);
                        }
                        currentPath += "/" + folders[i];
                    }
                }

                // Resource.Load가 실패했을 경우
                _instance = AssetDatabase.LoadAssetAtPath<SceneResource>(FILE_PATH);
                if (_instance == null)
                {
                    _instance = CreateInstance<SceneResource>();
                    AssetDatabase.CreateAsset(_instance, FILE_PATH);
                }
            }
#endif
            return _instance;
        }
    }

#if UNITY_EDITOR
    [System.Serializable]
    private struct SerializedSceneAssetData
    {
        public SceneType key;
        public SceneAsset value;

        public SerializedSceneAssetData(SceneType key, SceneAsset value)
        {
            this.key = key;
            this.value = value;
        }
    }
#endif

    [System.Serializable]
    private struct SerializedSceneData
    {
        public SceneType key;
        public string value;

        public SerializedSceneData(SceneType key, string value)
        {
            this.key = key;
            this.value = value;
        }
    }

#if UNITY_EDITOR
    [SerializeField]
    private List<SerializedSceneAssetData> usedSceneAssets = new();
#endif
    [SerializeField, HideInInspector]
    private List<SerializedSceneData> usedScenes = new();

    private readonly Dictionary<SceneType, string> usedSceneLookup = new();

    public void OnBeforeSerialize()
    {
#if UNITY_EDITOR
        usedScenes.Clear();

        foreach (var data in usedSceneAssets)
        {
            if (data.value == null) continue;

            usedScenes.Add(new SerializedSceneData(data.key, data.value.name));
        }
#endif
    }
    public void OnAfterDeserialize()
    {
        usedSceneLookup.Clear();
        foreach (var data in usedScenes)
        {
            usedSceneLookup.TryAdd(data.key, data.value);
        }
    }

    public string GetSceneName(SceneType type)
    {
        // Lookup에서 찾아 리턴
        if (usedSceneLookup.TryGetValue(type, out var sceneName))
        {
            return sceneName;
        }

        // Deserialize 과정에서 누락된 경우 리스트에서 찾기
        foreach (var data in usedScenes)
        {
            if (data.key == type)
            {
                usedSceneLookup.Add(type, data.value);
                return data.value;
            }
        }

        return string.Empty;
    }
}