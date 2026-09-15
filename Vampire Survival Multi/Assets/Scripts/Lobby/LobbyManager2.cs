using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager2 : MonoBehaviourPunCallbacks
{
    [Header("참조 스크립트")]
    [SerializeField] private LobbyUI ui;

    [Header("플레이어 패널 스크립트")]
    [SerializeField]
    private List<PlayerPanelManager> playerPanels;
    private PlayerPanelManager myPanelManager;

    // 해당 방의 호스트
    private Photon.Realtime.Player masterClient;

    // 방 정보
    private Room room;
    private int readyToStartPlayer = 0;
    private int setPlayerDataCount = 0;

    /***************************************************************
    * [ 방 입장 ]
    * 
    * 방 입장 시 이벤트 설정
    ***************************************************************/

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        room = PhotonNetwork.CurrentRoom;

        // 방장 설정
        masterClient = PhotonNetwork.MasterClient;

        OnEnterRoom();
    }

    private void OnEnterRoom()
    {
        // 로비 기본 UI 처리
        InitLobbyUI();

        if (masterClient.IsLocal)
        {
            // 방장의 경우 비활성화 패널 설정
            SetClosedPanel(room.MaxPlayers);

            // 첫 번째 패널에 자신 할당
            SetPlayerPanel(masterClient);
        }
        else
        {
            // 방장으로부터 패널 정보 동기화 받기
            photonView.RPC(nameof(SynchroPanel), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
        }
    }

    private void SetPlayerPanel(Photon.Realtime.Player newPlayer)
    {
        // 빈 패널 찾기
        int index = GetEmptyPanelIndex();
        PlayerPanelManager manager = playerPanels[index];

        // 새로 들어온 플레이어에게 할당
        photonView.RPC(nameof(SetMyPanelManager), newPlayer, index);

        // 모든 플레이어에게 해당 패널 설정
        manager.OnJoinPlayer(newPlayer);
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        if (masterClient.IsLocal)
        {
            // 새 플레이어 패널 설정
            SetPlayerPanel(newPlayer);

            // 시작 여부 업데이트
            UpdateStartActive();
        }
    }

    /***************************************************************
    * [ 방 퇴장 ]
    * 
    * 방 퇴장 시 이벤트 설정
    ***************************************************************/

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (masterClient.IsLocal)
        {
            RemovePlayer(otherPlayer);

            // 시작 여부 업데이트
            UpdateStartActive();
        }
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.AutomaticallySyncScene = false;

        // 방을 나갔으면 타이틀로 돌아가기
        SceneManager.LoadScene("TitleScene");
    }

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        // 새로운 방장은 이전 방장의 권한을 받아 UI 수정
        if (newMasterClient.IsLocal)
        {
            if (IsPlayerInRoom(masterClient) == false)
            {
                // 이미 나간 방장일 경우 패널에서 삭제
                RemovePlayer(masterClient);
            }

            // 방장 넘겨받기
            masterClient = newMasterClient;

            // 방장 전용 UI로 변경
            ReloadUI();

            // 시작 여부 업데이트
            UpdateStartActive();
        }
        else
        {
            Photon.Realtime.Player formerAdmin = masterClient;

            // 새 방장 설정
            masterClient = newMasterClient;

            if (formerAdmin.IsLocal)
            {
                // 이전 방장인 경우 UI 새로 불러오기
                ReloadUI();
            }
        }
    }

    private bool IsPlayerInRoom(Photon.Realtime.Player checkPlayer)
    {
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            if (player == checkPlayer)
                return true;
        }

        return false;
    }

    private void RemovePlayer(Photon.Realtime.Player removePlayer)
    {
        foreach (PlayerPanelManager manager in playerPanels)
        {
            Photon.Realtime.Player player = manager.JoinPlayer;

            if (player == removePlayer)
            {
                manager.OnExitPlayer();
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
    {
        if (targetPlayer.IsLocal)
        {
            // 본인에게 강퇴 상태가 있을 경우 방 나가기
            bool isKicked = (changedProps["IsKicked"] != null) ?
                (bool)changedProps["IsKicked"] : false;

            if (isKicked)
            {
                // 강퇴 상태 비활성화
                Hashtable hashtable = new Hashtable() { { "IsKicked", false } };

                PhotonNetwork.SetPlayerCustomProperties(hashtable);
                PhotonNetwork.LeaveRoom(); // 방 나가기
            }
        }
    }

    /***************************************************************
    * [ 방 설정 ]
    * 
    * 방 설정 변경에 따른 UI 변화
    ***************************************************************/

    public void UpdateRoomSetting()
    {
        UpdateMaxPlayer();
    }

    private void UpdateMaxPlayer()
    {
        int maxPlayer = room.MaxPlayers;

        SetClosedPanel(maxPlayer);

        // 변경 내역 전체 알림
        SynchroPanelClosed();
    }

    /***************************************************************
    * [ 서버 통신 ]
    * 
    * 리슨 서버 형식(방장이 서버 역할)의 통신 구축
    ***************************************************************/

    [PunRPC]
    private void SetMyPanelManager(int index)
    {
        // 패널 할당 및 설정
        PlayerPanelManager manager = playerPanels[index];

        myPanelManager = manager;
    }

    [PunRPC]
    private void SynchroPanel(Photon.Realtime.Player newPlayer)
    {
        // 모든 패널 동기화
        foreach (PlayerPanelManager manager in playerPanels)
        {
            Photon.Realtime.Player panelPlayer = manager.JoinPlayer;

            manager.SendRoomData(newPlayer, panelPlayer);
        }
    }

    private void SetActivePlayerMenu()
    {
        foreach (PlayerPanelManager manager in playerPanels)
        {
            manager.SetActivePlayer();
        }
    }

    private void SynchroPanelClosed()
    {
        // 모든 패널 닫힘 상태 동기화
        foreach (PlayerPanelManager manager in playerPanels)
        {
            manager.UpdateRoomData();
        }
    }

    /***************************************************************
    * [ UI 설정 ]
    * 
    * 상황에 따른 UI 설정
    ***************************************************************/

    private void InitLobbyUI()
    {
        // 로비 기본 UI 설정
        ui.SetRoomCode(room.Name);
        UpdateSettingButton();
        UpdateReadyOrStartButton();

        // 시작 여부 업데이트
        UpdateStartActive();
    }

    private void SetClosedPanel(int maxPlayer)
    {
        // 인원수만큼의 패널만 활성화
        int openPanel = playerPanels.Count;
        for (int i = playerPanels.Count - 1; i >= 0; i--)
        {
            // 뒤에서부터 패널 비활성화
            PlayerPanelManager panel = playerPanels[i];

            if (panel.IsExist == false && openPanel > maxPlayer)
            {
                playerPanels[i].IsClosed = true;

                openPanel--;
            }
            else
            {
                playerPanels[i].IsClosed = false;
            }
        }
    }

    private void ReloadUI()
    {
        // 플레이어의 권한에 맞는 UI 다시 적용
        SetActivePlayerMenu();
        UpdateSettingButton();
        UpdateReadyOrStartButton();
        myPanelManager.UpdateAdminPanel();
    }

    private void UpdateReadyOrStartButton()
    {
        // 게임준비 버튼 및 시작 버튼 활성화
        if (masterClient.IsLocal) ui.ChangeToStartButton();
        else ui.ChangeToReadyButton();
    }

    private void UpdateSettingButton()
    {
        if (masterClient.IsLocal) ui.SetActiveSettingButton(true);
        else ui.SetActiveSettingButton(false);
    }

    private int GetEmptyPanelIndex()
    {
        // 가장 가까운 빈 패널 찾기
        for (int i = 0; i <= playerPanels.Count; i++)
        {
            PlayerPanelManager manager = playerPanels[i];

            if (manager.IsExist == false)
            {
                return i;
            }
        }

        return -1;
    }

    [PunRPC]
    private void UpdateStartActive()
    {
        if (GetStartableState())
        {
            // 모든 플레이어가 준비상태일 경우 시작 버튼 활성화
            ui.SetUsableStartButton(true);
        }
        else
        {
            // 모든 플레이어가 준비상태가 아닐 경우 비활성화
            ui.SetUsableStartButton(false);
        }
    }

    private bool GetStartableState()
    {
        foreach (PlayerPanelManager manager in playerPanels)
        {
            if (manager.IsExist && manager.IsReady == false)
            {
                if (manager.JoinPlayer.IsMasterClient == false)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /***************************************************************
    * [ 직업 변경 ]
    * 
    * 사용자의 직업이 변경되었을 시 이벤트
    ***************************************************************/

    public void OnChangedPlayerClass()
    {
        myPanelManager?.UpdateClass();
    }

    /***************************************************************
    * [ 게임 시작 ]
    * 
    * 게임 준비와 시작
    ***************************************************************/

    public void OnReady()
    {
        // 현재의 반대 상태 적용
        bool isReady = !myPanelManager.IsReady;

        // 준비 상태 변경
        myPanelManager.SetReadyState(isReady);

        // 캐릭터 선택 버튼 상태 변경
        ui.SetActiveChrSelectButton(!isReady);

        // 게임 시작 상태 변경
        photonView.RPC(nameof(UpdateStartActive), masterClient);
    }

    public void OnStart()
    {
        StartCoroutine(StartGameCoroutine());
    }

    private System.Collections.IEnumerator StartGameCoroutine()
    {
        for (int i = 0; i < playerPanels.Count; i++)
        {
            PlayerPanelManager manager = playerPanels[i];
            PlayerData playerData = PlayerResource.Instance.PlayerDatas[i];

            if (manager.IsExist)
            {
                // 해당 자리의 플레이어 참가 설정
                photonView.RPC(nameof(SetPlayerIsPlaying), RpcTarget.All, i, manager.JoinPlayer);

                // 플레이어 데이터 할당
                photonView.RPC(nameof(InitClassData), manager.JoinPlayer, i);
            }
            else
            {
                // 나머지 플레이어 참가 해제
                photonView.RPC(nameof(SetPlayerIsPlaying), RpcTarget.All, i, null);
            }
        }

        // RPC 호출이 완료된 후에 실행
        yield return new WaitUntil(() => AllRPCsFinished());

        // 모든 플레이어가 데이터를 전송 받았으면 게임 시작
        SceneLoadManager.LoadLevel("InGame");
    }

    private bool AllRPCsFinished()
    {
        return readyToStartPlayer >= room.PlayerCount;
    }

    [PunRPC]
    private void SetPlayerIsPlaying(int index, Photon.Realtime.Player joinPlayer)
    {
        PlayerData playerData = PlayerResource.Instance.PlayerDatas[index];

        playerData.Player = joinPlayer;

        if (joinPlayer == null)
        {
            // 완료된 데이터 셋팅
            AddCompletedRPC();
        }
    }

    [PunRPC]
    private void InitClassData(int index)
    {
        PlayerData playerData = PlayerResource.Instance.PlayerDatas[index];

        LocalPlayerData localPlayer = LocalPlayerData.Instance;
        localPlayer.InitPlayerData(playerData);

        photonView.RPC(nameof(AsyncClassData), RpcTarget.Others, localPlayer.Class.ID, index);

        // 완료된 데이터 셋팅에 추가
        AddCompletedRPC();
    }

    [PunRPC]
    private void AsyncClassData(int classID, int playerIndex)
    {
        ClassData classData = ClassResource.Instance.GetClass(classID);
        PlayerData playerData = PlayerResource.Instance.PlayerDatas[playerIndex];

        playerData.InitData(classData);

        // 완료된 데이터 셋팅에 추가
        AddCompletedRPC();
    }

    private void AddCompletedRPC()
    {
        setPlayerDataCount++;

        if (setPlayerDataCount >= playerPanels.Count)
        {
            photonView.RPC(nameof(CompletedAsync), RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    private void CompletedAsync()
    {
        readyToStartPlayer++;
    }

    /***************************************************************
    * [ 서버 연결 ]
    * 
    * 서버 연결이 끊겼을 경우의 조치
    ***************************************************************/

    public override void OnDisconnected(DisconnectCause cause)
    {
        // 게임을 나가서 생긴 끊김이 아닐 경우
        if (cause != DisconnectCause.ApplicationQuit)
        {
            // 오류 출력 패널 활성화
            ui.SetDisconnectedConfirm(true);
        }
    }

    public void OnClickToTitle()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnClickExitGame()
    {
        Application.Quit();
    }
}