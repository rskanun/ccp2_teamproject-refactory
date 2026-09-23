using Photon.Pun;

public class TitleSessionManager : MonoBehaviourPunCallbacks
{
    public override void OnLeftRoom()
    {
        var reason = SessionContext.LastExitResson;
        SessionContext.ClearReason();

        if (reason == RoomExitReason.Kicked)
        {
            Alert.Show("방장에 의해 방에서 추방되었습니다.", "확인");
            return;
        }

        if (reason == RoomExitReason.Timeout)
        {
            Alert.Show("네트워크 지연으로 방에서 퇴장되었습니다.", "확인");
            return;
        }
    }
}