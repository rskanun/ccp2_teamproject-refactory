using Sirenix.OdinInspector;
using UnityEngine;

public class TitleMenu : MonoBehaviour
{
    [Title("메뉴 구성요소")]
    [SerializeField] private GameObject roomListWindow;
    [SerializeField] private GameObject searchCodeWindow;
    [SerializeField] private GameObject settingWindown;

    public void OnClickMultiPlay()
    {
        roomListWindow.SetActive(true);
    }

    public void OnClickRoomCode()
    {
        searchCodeWindow.SetActive(true);
    }

    public void OnClickSettings()
    {
        settingWindown.SetActive(true);
    }

    public void OnClickExit()
    {
#if UNITY_EDITOR
        // 에디터 환경 종료
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 인게임 종료
        Application.Quit();
#endif
    }
}