using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class LocalPlayerData : ScriptableObject
{
    // 저장 파일 위치
    private const string FILE_DIRECTORY = "Assets/Resources/Objects/Player/Data";
    private const string FILE_PATH = "Assets/Resources/Objects/Player/Data/LocalPlayerData.asset";

    private static LocalPlayerData _instance;
    public static LocalPlayerData Instance
    {
        get
        {
            if (_instance != null) return _instance;

            _instance = Resources.Load<LocalPlayerData>("Objects/Player/Data/LocalPlayerData");

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
                _instance = AssetDatabase.LoadAssetAtPath<LocalPlayerData>(FILE_PATH);

                if (_instance == null)
                {
                    _instance = CreateInstance<LocalPlayerData>();
                    AssetDatabase.CreateAsset(_instance, FILE_PATH);
                }
            }
#endif
            return _instance;
        }
    }

    [Header("로컬 플레이어 정보")]
    [ReadOnly]
    [SerializeField]
    private PlayerData _playerData;
    public PlayerData PlayerData
    {
        private set { _playerData = value; }
        get { return _playerData; }
    }

    private ClassData cashedClassData;
    public ClassData Class
    {
        get
        {
            // 아직 플레이어 정보가 할당되지 않은 경우
            if (PlayerData == null)
            {
                if (cashedClassData == null)
                {
                    cashedClassData = ClassResource.Instance.GetDefaultClass();

                    SetClass(cashedClassData);
                }

                return cashedClassData;
            }
            else
            {
                // 플레이어 데이터가 생겼는데, 캐시 메모리에 직업 정보가 남은 경우
                if (cashedClassData != null)
                {
                    // 플레이어 데이터에 직업 정보 옮김
                    PlayerData.PlayerClass = cashedClassData;

                    // 캐시 초기화
                    cashedClassData = null;
                }

                // 플레이어 정보는 있지만, 직업이 정해지지 않은 경우
                if (PlayerData.PlayerClass == null)
                {
                    ClassData initClass = ClassResource.Instance.GetDefaultClass();

                    SetClass(initClass);
                }
            }

            return PlayerData.PlayerClass;
        }
    }

    [SerializeField]
    private bool _isDead;
    public bool IsDead
    {
        set { _isDead = value; }
        get { return _isDead; }
    }

    [Header("이벤트")]
    [SerializeField] private GameEvent classEvent;
    [SerializeField] private GameEvent posEvent;

    public void InitPlayerData(PlayerData localPlayerData)
    {
        PlayerData = localPlayerData;

        // 해당 플레이어 데이터에 기본 스텟 적용
        PlayerData.InitData(Class);
    }

    public void SetClass(ClassData classData)
    {
        if (PlayerData != null) PlayerData.PlayerClass = classData;
        else cashedClassData = classData;

        // 이벤트 알림
        classEvent.NotifyUpdate();
    }

    public void MoveToPos(Vector2 pos)
    {
        PlayerData.Position = pos;

        // 위치 이동 이벤트
        posEvent.NotifyUpdate();
    }
}