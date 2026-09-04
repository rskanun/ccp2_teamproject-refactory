using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MultiRoomUI : MonoBehaviour
{
    [Header("참조 오브젝트")]
    [SerializeField] private GameObject roomContainer;
    [SerializeField] private GameObject roomList;
    [SerializeField] private GameObject createPanel;
    [SerializeField] private TMP_InputField searchField;

    [Header("생성될 방 프리팹")]
    [SerializeField] private GameObject roomPrefab;

    // 방 오브젝트 목록
    private List<GameObject> roomObjList = new List<GameObject>();

    /***************************************************************
    * [ 방 목록 ]
    * 
    * 방 목록에 관한 UI 함수
    ***************************************************************/

    public void SetActiveRoomList(bool isActive)
    {
        roomList.SetActive(isActive);
    }

    public void AddRoomObj(RoomInfo info, Action<string, string> listener)
    {
        // 방 정보를 표시하는 오브젝트 소환
        var roomObj = Instantiate(roomPrefab, roomContainer.transform);
        var room = roomObj.GetComponent<RoomEntry>();
        room.SetRoomInfo(info, listener);

        // 해당 오브젝트를 목록에 저장
        roomObjList.Add(roomObj);
    }

    public void RemoveAllRooms()
    {
        foreach (GameObject roomObj in roomObjList)
        {
            // 방 오브젝트 파괴
            Destroy(roomObj);
        }

        // 리스트 초기화
        roomObjList.Clear();
    }

    public string GetSearchKeyword()
    {
        return searchField.text;
    }

    public void ClearSearchField()
    {
        searchField.text = "";
    }

    /***************************************************************
    * [ 방 생성 ]
    * 
    * 방 생성 패널 활성화 여부 설정
    ***************************************************************/

    public void SetCreatePanel(bool isActive)
    {
        createPanel.SetActive(isActive);
    }
}