using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PlayerPanelUI))]
public class PlayerPanelManager : MonoBehaviourPun
{
    [Header("패널 플레이어 데이터")]
    [SerializeField] private PlayerData playerData;

    [Header("참조 스크립트")]
    [SerializeField] private PlayerPanelUI ui;
    [SerializeField] private Confirm confirm;

    // 방 정보
    private Photon.Realtime.Player _joinPlayer;
    public Photon.Realtime.Player JoinPlayer
    {
        get { return _joinPlayer; }
    }

    private bool _isReady;
    public bool IsReady
    {
        get { return _isReady; }
        set
        {
            _isReady = value;

            ui.SetReadyPanel(value);
        }
    }

    // 패널 정보
    private bool _isClosed;
    public bool IsClosed
    {
        get { return _isClosed; }
        set
        {
            _isClosed = value;

            ui.SetActiveCloseMark(value);
        }
    }

    public bool IsExist
    {
        get { return _joinPlayer != null; }
    }

    /***************************************************************
    * [ 패널 입장 ]
    * 
    * 방 입장 후 패널 설정
    ***************************************************************/

    public void OnJoinPlayer(Photon.Realtime.Player player)
    {
        // 모든 플레이어에게 해당 패널 설정
        photonView.RPC(nameof(SetPanelInfo), RpcTarget.All, player);
    }

    [PunRPC]
    public void SetPanelInfo(Photon.Realtime.Player player)
    {
        _joinPlayer = player;

        if (player.IsLocal)
        {
            ui.SetLocalMark(true);

            // 플레이어 데이터 설정
            LocalPlayerData.Instance.InitPlayerData(playerData);

            // 직업명 설정
            UpdateClass();
        }

        // UI 설정
        SetActivePlayer();
        UpdateAdminPanel();
    }

    public void SetActivePlayer()
    {
        if (IsExist && IsClosed == false)
        {
            if (PhotonNetwork.IsMasterClient && !JoinPlayer.IsLocal)
            {
                // 방장은 해당 플레이어 조작 메뉴 포함 전부 활성화
                ui.SetActiveCharacter(true);
                ui.SetActivePlayerMenuBtn(true);
            }
            else
            {
                // 방장 외엔 플레이터 정보 활성화
                ui.SetActiveCharacter(true);
                ui.SetActivePlayerMenuBtn(false);
            }
        }
    }

    /***************************************************************
    * [ 패널 퇴장 ]
    * 
    * 방 퇴장 후 패널 설정
    ***************************************************************/

    public void OnExitPlayer()
    {
        // 해당 패널을 모든 플레이어에 대해 초기화
        photonView.RPC(nameof(ResetPanel), RpcTarget.All);
    }

    [PunRPC]
    private void ResetPanel()
    {
        _joinPlayer = null;
        _isReady = false;

        // UI 초기화
        ui.SetActiveCharacter(false);
        ui.SetActivePlayerMenuBtn(false);
        ui.SetAdminPanel(false);
        ui.SetReadyPanel(false);
        ui.SetClassName("");
    }

    /***************************************************************
    * [ 방장 설정 ]
    * 
    * 방장을 나타내는 패널 설정
    ***************************************************************/

    public void UpdateAdminPanel()
    {
        photonView.RPC(nameof(SetThisAdminPanel), RpcTarget.MasterClient);
    }

    [PunRPC]
    private void SetThisAdminPanel()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(SetThisAdminPanel), RpcTarget.Others);
        }

        if (JoinPlayer.IsMasterClient)
        {
            _isReady = false; // 준비 해제

            // 방장의 패널일 경우 방장 마크 활성화
            ui.SetReadyPanel(false);
            ui.SetAdminPanel(true);
        }
        else
        {
            _isReady = false; // 준비 해제

            // 방장의 패널이 아닌 경우 방장 마크 비화성화
            ui.SetAdminPanel(false);
        }
    }

    /***************************************************************
    * [ 직업 할당 ]
    * 
    * 해당 패널의 플레이어의 직업 할당
    ***************************************************************/

    public void UpdateClass()
    {
        ClassData classData = LocalPlayerData.Instance.Class;

        // 직업 변경 사실을 방장에게 알림
        photonView.RPC(nameof(SetPlayerClass), RpcTarget.MasterClient, classData.ID);
    }

    [PunRPC]
    private void SetPlayerClass(int id)
    {
        // 모든 사람들에게 해당 패널 플레이어의 직업 적용
        photonView.RPC(nameof(SetClass), RpcTarget.All, id);
    }

    [PunRPC]
    private void SetClass(int id)
    {
        ClassData classData = ClassResource.Instance.GetClass(id);

        playerData.PlayerClass = classData;
        ui.SetClassName(classData.Name);
    }

    /***************************************************************
    * [ 유저 메뉴 ]
    * 
    * 해당 패널의 유저 강퇴 및 방장 양도
    ***************************************************************/

    public void ActiveDelegateConfirm()
    {
        // Confirm 띄우기
        string msg = "해당 유저에게 방장을 양도하시겠습니까?";

        confirm.OnActive(msg, OnDelegateAdmin);

        // 기존 플레이어 메뉴 닫기
        ui.TogglePlayerMenu();
    }

    private void OnDelegateAdmin()
    {
        // 사람이 존재하는 패널이고, 넘기는 사람이 방장일 경우 작동
        if (IsExist && PhotonNetwork.IsMasterClient)
        {
            // 해당 패널 주인에게 방장 권한을 넘기기
            PhotonNetwork.SetMasterClient(JoinPlayer);
        }
    }

    public void ActiveKickConfirm()
    {
        // Confirm 띄우기
        string msg = "해당 유저를 강제 추방하시겠습니까?";

        confirm.OnActive(msg, OnKickedPlayer);

        // 기존 플레이어 메뉴 닫기
        ui.TogglePlayerMenu();
    }

    private void OnKickedPlayer()
    {
        if (IsExist && PhotonNetwork.IsMasterClient)
        {
            // 플레이어에게 퇴장 상태를 부여
            Hashtable hashtable = new Hashtable() { { "IsKicked", true } };

            JoinPlayer.SetCustomProperties(hashtable);
        }
    }

    /***************************************************************
    * [ 게임 준비 ]
    * 
    * 게임 준비 패널 설정
    ***************************************************************/

    public void SetReadyState(bool isReady)
    {
        photonView.RPC(nameof(SetReadyThisPanel), RpcTarget.MasterClient, isReady);
    }

    [PunRPC]
    private void SetReadyThisPanel(bool isReady)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(SetReadyThisPanel), RpcTarget.Others, isReady);
        }

        // 준비 상태 설정
        IsReady = isReady;
    }

    /***************************************************************
    * [ 데이터 동기화 ]
    * 
    * 패널 상태 동기화
    ***************************************************************/

    public void SendRoomData(Photon.Realtime.Player sendPlayer, Photon.Realtime.Player panelPlayer)
    {
        // 본인의 패널이 아닌 플레이어가 존재하는 패널만 동기화
        if (IsExist)
        {
            int classID = (playerData.PlayerClass != null) ? playerData.PlayerClass.ID : -1;

            photonView.RPC(nameof(ReceiveRoomData), sendPlayer, panelPlayer, classID);
        }

        // 방 닫힘 여부 동기화
        photonView.RPC(nameof(ReceiveClosedData), sendPlayer, IsClosed);
        photonView.RPC(nameof(ReceiveReadyData), sendPlayer, IsReady);
    }

    public void UpdateRoomData()
    {
        photonView.RPC(nameof(ReceiveClosedData), RpcTarget.Others, IsClosed);
    }

    [PunRPC]
    private void ReceiveRoomData(Photon.Realtime.Player player, int classID)
    {
        if (classID >= 0)
        {
            SetPanelInfo(player);
            SetClass(classID);
        }
    }

    [PunRPC]
    private void ReceiveClosedData(bool isClosed)
    {
        IsClosed = isClosed;
    }

    [PunRPC]
    private void ReceiveReadyData(bool isReady)
    {
        IsReady = isReady;
    }
}