using Photon.Pun;
using UnityEngine;

public class PlayerManagePanel : MonoBehaviour
{
    [SerializeField] private PhotonView photonView;

    private int targetNumber;

    public void Open(int playerNumber)
    {
        targetNumber = playerNumber;

        gameObject.SetActive(true);
    }

    public void Close()
    {
        targetNumber = -1;
        gameObject.SetActive(false);
    }

    public void OnClickTrensferMasterButton()
    {
        // 플레이어가 방에 존재하는 경우에만 실행
        if (TryGetPlayer(targetNumber, out var player))
        {
            PhotonNetwork.SetMasterClient(player);
        }

        // 조작 패널 닫기
        Close();
    }

    public void OnClickKickButton()
    {
        // 플레이어가 방에 존재하는 경우에만 실행
        if (TryGetPlayer(targetNumber, out var player))
        {
            photonView.RPC(nameof(RoomSessionReceiver.KickedFromRoom), player);
        }

        // 조작 패널 닫기
        Close();
    }

    private bool TryGetPlayer(int number, out Photon.Realtime.Player player)
    {
        player = null;

        // 방장 외 조작 금지 설정
        if (!PhotonNetwork.IsMasterClient)
        {
            return false;
        }

        player = PhotonNetwork.CurrentRoom?.GetPlayer(number);
        if (player == null)
        {
            Alert.Show("이미 방을 떠난 플레이어 입니다.", "확인");
            return false;
        }

        return true;
    }
}