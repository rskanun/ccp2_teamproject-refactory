using System;
using ExitGames.Client.Photon;
using Photon.Realtime;

public static class RoomPropertyExtensions
{
    private const string ROOM_ID = "id";
    private const string ROOM_NAME = "rn";
    private const string ROOM_TYPE = "rt";
    private const string PASSWORD_HASH = "ph";

    public static readonly string[] LobbyProps = new string[4]
    {
        ROOM_ID, ROOM_NAME, ROOM_TYPE, PASSWORD_HASH
    };

    public static Hashtable SetID(this Hashtable props, string id)
    {
        props[ROOM_ID] = id;
        return props;
    }

    public static Hashtable SetName(this Hashtable props, string name)
    {
        props[ROOM_NAME] = name;
        return props;
    }

    public static Hashtable SetType(this Hashtable props, RoomType type)
    {
        props[ROOM_TYPE] = (byte)type;
        return props;
    }

    public static Hashtable SetPasswordHash(this Hashtable props, string password)
    {
        props[PASSWORD_HASH] = password;
        return props;
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

    public static RoomType GetType(this RoomInfo room)
    {
        return room.CustomProperties.TryGetValue(ROOM_TYPE, out var val)
            ? (RoomType)Convert.ToByte(val)
            : RoomType.Public;
    }

    public static string GetPasswordHash(this RoomInfo room)
    {
        return room.CustomProperties.TryGetValue(PASSWORD_HASH, out var val) && val is string hash
            ? hash
            : string.Empty;
    }
}