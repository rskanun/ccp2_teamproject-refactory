using Photon.Pun;

public class RoomSessionReceiver : MonoBehaviourPun
{
    private bool IsValidPlayer(int number) => PhotonNetwork.CurrentRoom.GetPlayer(number) != null;

    [PunRPC]
    public void KickedFromRoom()
    {
        SessionContext.SetExitReason(RoomExitReason.Kicked);
        PhotonNetwork.LeaveRoom();
    }

    [PunRPC]
    public void RequestChangeClass(byte classId, PhotonMessageInfo info)
    {
        // 방장만 처리
        if (!PhotonNetwork.IsMasterClient) return;

        var sender = info.Sender;
        int number = sender.ActorNumber;

        // 플레이어 유효성 확인
        if (!IsValidPlayer(number)) return;

        // 직업 변경
        sender.SetClassID(classId);
    }

    [PunRPC]
    public void RequestToggleReady(PhotonMessageInfo info)
    {
        // 방장만 처리
        if (!PhotonNetwork.IsMasterClient) return;

        var sender = info.Sender;
        int number = sender.ActorNumber;

        // 플레이어 유효성 확인
        if (!IsValidPlayer(number)) return;

        // 리퀘스트 처리
        sender.SetReadyState(!sender.GetReadyState());
    }
}