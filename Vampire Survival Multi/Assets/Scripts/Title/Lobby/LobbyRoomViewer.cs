using System;
using System.Collections.Generic;
using Photon.Realtime;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class LobbyRoomViewer : MonoBehaviour
{
    [Title("구성 컴포넌트")]
    [SerializeField] private TMP_InputField searchField;
    [SerializeField] private Transform roomContentRoot;

    [Title("방 정보 UI 프리팹")]
    [SerializeField] private LobbyRoomEntry roomEntryPrefab;

    private readonly List<LobbyRoomEntry> entryPool = new();

    public void RenderRooms(List<RoomInfo> rooms, Action<string, bool> enterHandler)
    {
        while (rooms.Count > entryPool.Count)
        {
            var entry = Instantiate(roomEntryPrefab, roomContentRoot);
            entryPool.Add(entry);
        }

        for (int i = 0; i < entryPool.Count; i++)
        {
            // 방 숫자만큼 활성화
            if (i < rooms.Count)
            {
                entryPool[i].SetRoomInfo(rooms[i], enterHandler);
            }
            else
            {
                // 나머지 방은 파괴 대신 비활성화로 대기
                entryPool[i].gameObject.SetActive(false);
            }
        }
    }

    public string GetSearchKeyword()
    {
        return searchField.text;
    }

    public void ClearSearchField()
    {
        searchField.text = "";
    }
}