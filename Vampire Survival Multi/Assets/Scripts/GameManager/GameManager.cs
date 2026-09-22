using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("참조 스크립트")]
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private Confirm resultConfirm;

    [Header("플레이어블 캐릭터")]
    [SerializeField] private List<GameObject> playableChrList;

    [Header("이벤트")]
    [SerializeField] private GameEvent reviveEvent;

    // 플레이어 리소스
    private List<PlayerData> playerDatas;

    // 참조 데이터
    private WaveData waveData;
    private GameData gameData;
    private ExpResource expResource;

    // 준비 여부
    private int readyToStartPlayer = 0;

    private void Awake()
    {
        waveData = WaveData.Instance;
        gameData = GameData.Instance;
        expResource = ExpResource.Instance;

        // Init Player Resource
        PlayerResource resource = PlayerResource.Instance;

        playerDatas = resource.PlayerDatas;

        // Init Photon
        PhotonNetwork.MinimalTimeScaleToDispatchInFixedUpdate = 0;
    }

    private void Start()
    {
        // 게임 데이터 초기화
        InitGameData();

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(OnStartCoroutine());
        }
    }

    private IEnumerator OnStartCoroutine()
    {
        yield return new WaitUntil(() => AllPlayerReady());

        // 게임 시작
        GameStart();
    }

    private bool AllPlayerReady()
    {
        return readyToStartPlayer >= PhotonNetwork.CurrentRoom.PlayerCount;
    }

    private void InitGameData()
    {
        // 플레이어 데이터 초기화
        InitPlayer();

        // 아이템 데이터 초기화
        ItemResource.Instance.InitItemData();

        // 보상 초기화
        RewardResource.Instance.InitRewardData();

        // 경험치 데이터 설정
        expResource.SetWaveLevel(waveData.WaveLevel);

        // 준비 완료 알림
        OnReadyToStart();
    }

    private void OnReadyToStart()
    {
        photonView.RPC(nameof(AddReadyPlayer), RpcTarget.MasterClient);
    }

    [PunRPC]
    private void AddReadyPlayer()
    {
        readyToStartPlayer++;
    }

    private void GameStart()
    {
        // 웨이브 시작
        photonView.RPC(nameof(WaveStart), RpcTarget.All);
    }

    [PunRPC]
    private void WaveStart()
    {
        waveData.WaveStart();
    }

    /***************************************************************
    * [ 플레이어 세팅 ]
    * 
    * 게임 시작 시 플레이어에 관한 세팅
    ***************************************************************/

    private void InitPlayer()
    {
        GameData gameData = GameData.Instance;

        for (int i = 0; i < playerDatas.Count; i++)
        {
            PlayerData playerData = playerDatas[i];
            if (playerData.IsPlaying)
            {
                playableChrList[i].SetActive(true);

                // 플레이어 오브젝트 목록에 추가
                gameData.AddPlayableChr(playableChrList[i]);
            }
        }

        gameData.InitData();
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // 플레이어 상태 체크
            UpdatePlayersStatus();

            // 웨이브 상태 진행
            UpdateWaveStatus();
        }
    }

    /***************************************************************
    * [ 웨이브 진행 ]
    * 
    * 웨이브 진행에 따른 게임 진행 설정
    ***************************************************************/

    private void UpdateWaveStatus()
    {
        if (waveData.IsRunning)
        {
            // 남은 시간이 없거나, 모든 몬스터를 죽였을 경우
            if (waveData.RemainTime <= 0 || waveData.MobCount <= 0)
            {
                // 최종 웨이브가 아니고, 시간이 남은 경우
                if (waveData.RemainTime > 0 && waveData.IsLastWave == false)
                {
                    // 클리어 보상
                    photonView.RPC(nameof(OnWaveClear), RpcTarget.All);
                }

                // 다음 웨이브 진행
                photonView.RPC(nameof(NextWave), RpcTarget.All);

                // 만약 웨이브가 종료된 경우
                if (waveData.IsRunning == false)
                {
                    // 게임 종료
                    photonView.RPC(nameof(OnGameClear), RpcTarget.All);
                }
            }
            else
            {
                float time = Time.deltaTime;

                waveData.RemainTime -= time;

                float prevTime = (float)System.Math.Round(waveData.RemainTime + time, 1);
                float curTime = (float)System.Math.Round(waveData.RemainTime, 1);

                if (prevTime != curTime)
                {
                    photonView.RPC(nameof(UpdateWaveTime), RpcTarget.Others, curTime);
                }
            }
        }
    }
    [PunRPC]
    private void NextWave()
    {
        waveData.NextWave();
        expResource.SetWaveLevel(waveData.WaveLevel);

        // 다음 웨이브로 넘어갈 시 모든 플레이어 부활
        ReviveAllPlayer();
    }

    [PunRPC]
    private void UpdateWaveTime(float time)
    {
        waveData.RemainTime = time;
    }

    [PunRPC]
    private void OnWaveClear()
    {
        Debug.Log("Wave Clear");
    }

    private void ReviveAllPlayer()
    {
        foreach (GameObject deadPlayer in gameData.DeadPlayerList)
        {
            deadPlayer.SetActive(true);
        }

        if (LocalPlayerData.Instance.IsDead)
        {
            // 본인이 죽었었으면 부활
            LocalPlayerData.Instance.IsDead = false;

            // Notify Revive Event
            reviveEvent.NotifyUpdate();
        }

        gameData.ReviveAllPlayer();
    }

    private void UpdatePlayersStatus()
    {
        if (gameData.IsAllDead && waveData.IsRunning)
        {
            photonView.RPC(nameof(OnGameOver), RpcTarget.All);
        }
    }

    [PunRPC]
    private void OnGameOver()
    {
        waveData.WaveStop();

        Time.timeScale = 0.0f;
        resultConfirm.OnActive("Game Over...", () =>
        {
            Time.timeScale = 1.0f;

            PhotonNetwork.LeaveRoom();
        });
    }

    [PunRPC]
    private void OnGameClear()
    {
        // 웨이브를 최종 클리어 했을 시
        Time.timeScale = 0.0f;
        resultConfirm.OnActive("Game Clear!!", () =>
        {
            Time.timeScale = 1.0f;

            PhotonNetwork.LeaveRoom();
        });
    }

    /***************************************************************
    * [ 연결 끊김 ]
    * 
    * 연결이 끊겼을 때의 대처 방안
    ***************************************************************/

    public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
    {
        Time.timeScale = 0.0f;
        resultConfirm.OnActive("호스트의 서버 연결이 끊겼습니다!!", () =>
        {
            PhotonNetwork.LeaveRoom();
        });
    }

    /***************************************************************
    * [ 게임 퇴장 ]
    * 
    * 특정 이유로 게임에서 퇴장될 때 처리될 함수
    ***************************************************************/

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("TitleScene");
    }
}