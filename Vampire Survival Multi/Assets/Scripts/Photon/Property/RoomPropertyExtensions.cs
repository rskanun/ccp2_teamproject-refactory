using ExitGames.Client.Photon;
using Photon.Realtime;

public static class RoomPropertyExtensions
{
    // Room Property Keys
    private const string SLOT_STATE = "ss";

    // Room Property Keys In Lobby
    private const string ROOM_ID = "id";
    private const string ROOM_NAME = "rn";
    private const string ROOM_TYPE = "rt";
    private const string PASSWORD_HASH = "ph";

    /// <summary>
    /// Photon 마스터 서버에 노출할 키 배열
    /// </summary>
    public static readonly string[] LobbyProps = new string[4]
    {
        ROOM_ID, ROOM_NAME, ROOM_TYPE, PASSWORD_HASH
    };

    #region Builder 
    /// <summary>
    /// 룸 생성 시 사용되는 프로퍼티 빌더
    /// </summary>
    public readonly ref struct RoomPropertyBuilder
    {
        private readonly Hashtable table;

        public RoomPropertyBuilder(Hashtable table) => this.table = table;

        public RoomPropertyBuilder SetID(string id)
        {
            table[ROOM_ID] = id;
            return this;
        }

        public RoomPropertyBuilder SetName(string name)
        {
            table[ROOM_NAME] = name;
            return this;
        }

        public RoomPropertyBuilder SetRoomType(RoomType type)
        {
            table[ROOM_TYPE] = (byte)type;
            return this;
        }

        public RoomPropertyBuilder SetPasswordHash(string password)
        {
            table[PASSWORD_HASH] = password;
            return this;
        }

        public static implicit operator Hashtable(RoomPropertyBuilder builder) => builder.table;
    }

    /// <summary>
    /// 빌더 생성 팩토리 메서드
    /// </summary>
    public static RoomPropertyBuilder CreateBuilder(Hashtable target = null)
    {
        return new RoomPropertyBuilder(target ?? new Hashtable());
    }
    #endregion

    #region Setter
    public static void SetSlotState(this Room room, byte state)
    {
        var props = new Hashtable { { SLOT_STATE, state } };
        room.SetCustomProperties(props);
    }
    #endregion

    #region Getter
    public static byte GetSlotState(this RoomInfo room)
    {
        return room.CustomProperties.TryGetValue(SLOT_STATE, out var val) && val is byte state
            ? state
            : (byte)0;
    }

    public static string GetID(this RoomInfo room)
    {
        return room.CustomProperties.TryGetValue(ROOM_ID, out var val) && val is string id
            ? id
            : string.Empty;
    }

    public static string GetName(this RoomInfo room)
    {
        return room.CustomProperties.TryGetValue(ROOM_NAME, out var val) && val is string name
            ? name
            : string.Empty;
    }

    public static RoomType GetRoomType(this RoomInfo room)
    {
        return room.CustomProperties.TryGetValue(ROOM_TYPE, out var val) && val is byte type
            ? (RoomType)type
            : RoomType.Public;
    }

    public static string GetPasswordHash(this RoomInfo room)
    {
        return room.CustomProperties.TryGetValue(PASSWORD_HASH, out var val) && val is string hash
            ? hash
            : string.Empty;
    }
    #endregion
}