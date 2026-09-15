using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;



#if UNITY_EDITOR
using UnityEditor;
#endif

public class ClassResource : ScriptableObject
{
    // 저장 파일 위치
    private const string FILE_DIRECTORY = "Assets/Resources/Objects/Player/Class";
    private const string FILE_PATH = "Assets/Resources/Objects/Player/Class/ClassResource.asset";

    private static ClassResource _instance;
    public static ClassResource Instance
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = Resources.Load<ClassResource>("Objects/Player/Class/ClassResource");

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
                _instance = AssetDatabase.LoadAssetAtPath<ClassResource>(FILE_PATH);

                if (_instance == null)
                {
                    _instance = CreateInstance<ClassResource>();
                    AssetDatabase.CreateAsset(_instance, FILE_PATH);
                }
            }
#endif
            return _instance;
        }
    }

    [Title("직업 목록")]
    [SerializeField] private ClassData _defaultClass;
    [SerializeField] private List<ClassData> _classList = new();
    public IReadOnlyList<ClassData> ClassList => _classList;

    public ClassData GetDefaultClass()
    {
        if (_defaultClass != null)
            return _defaultClass;

        // 설정한 초기 직업이 없다면 직업 목록의 첫번째 요소 리턴
        return _classList.FirstOrDefault();
    }

    public ClassData GetClass(int id)
    {
        foreach (var classData in _classList)
        {
            if (classData.ID == id)
            {
                return classData;
            }
        }

        return null;
    }
}