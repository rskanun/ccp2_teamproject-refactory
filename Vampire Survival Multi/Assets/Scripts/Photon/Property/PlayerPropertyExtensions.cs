using ExitGames.Client.Photon;

public static class PlayerPropertyExtensions
{
    private const string SLOT_NUM = "sn";
    private const string CLASS_ID = "ci";
    private const string PLAYER_STATE = "ps";
    private const string LOADING_COMPLETED = "lc";

    #region Builder
    public readonly ref struct PlayerPropertyBuilder
    {
        private readonly Hashtable table;

        public PlayerPropertyBuilder(Hashtable table) => this.table = table;

        public PlayerPropertyBuilder SetSlotNumber(byte index)
        {
            table[SLOT_NUM] = index;
            return this;
        }

        public PlayerPropertyBuilder SetClass(byte id)
        {
            table[CLASS_ID] = id;
            return this;
        }

        public PlayerPropertyBuilder SetReadyState(bool isReady)
        {
            table[PLAYER_STATE] = isReady;
            return this;
        }

        public PlayerPropertyBuilder SetLoadingState(bool isCompleted)
        {
            table[LOADING_COMPLETED] = isCompleted;
            return this;
        }

        public static implicit operator Hashtable(PlayerPropertyBuilder builder) => builder.table;
    }

    public static PlayerPropertyBuilder CreateBuilder(Hashtable target = null)
    {
        return new PlayerPropertyBuilder(target ?? new Hashtable());
    }
    #endregion

    #region Setter
    public static void SetClassID(this Photon.Realtime.Player player, byte id)
    {
        var props = new Hashtable { { CLASS_ID, id } };
        player.SetCustomProperties(props);
    }

    public static void SetReadyState(this Photon.Realtime.Player player, bool isReady)
    {
        var props = new Hashtable { { PLAYER_STATE, isReady } };
        player.SetCustomProperties(props);
    }

    public static void SetLoadingState(this Photon.Realtime.Player player, bool isCompleted)
    {
        var props = new Hashtable { { LOADING_COMPLETED, isCompleted } };
        player.SetCustomProperties(props);
    }
    #endregion

    #region Getter
    public static byte GetSlotNumber(this Photon.Realtime.Player player)
    {
        return player.CustomProperties.TryGetValue(SLOT_NUM, out var val) && val is byte index
            ? index
            : byte.MaxValue;
    }

    public static byte GetClassID(this Photon.Realtime.Player player)
    {
        return player.CustomProperties.TryGetValue(CLASS_ID, out var val) && val is byte id
            ? id
            : byte.MaxValue;
    }

    public static bool GetReadyState(this Photon.Realtime.Player player)
    {
        return player.CustomProperties.TryGetValue(PLAYER_STATE, out var val) && val is bool isReady
            ? isReady
            : false;
    }

    public static bool GetLoadingState(this Photon.Realtime.Player player)
    {
        return player.CustomProperties.TryGetValue(LOADING_COMPLETED, out var val) && val is bool isCompleted
            ? isCompleted
            : false;
    }
    #endregion
}