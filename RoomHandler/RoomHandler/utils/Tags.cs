public class Tags {
    public const byte CREATE_ROOM = 1;
    public const byte RECEIVE_PLAYER_NAME = 2;
    public const byte SEND_ROOM_IDENTIFIER = 3;

    public const byte SEND_ROOMS_LIST = 4;
    public const byte REQUEST_ROOMS_LIST = 5;
    public const byte JOIN_ROOM = 6;
    public const byte ROOM_NOT_AVAILABLE = 7;
    public const byte CAN_JOIN_ROOM = 8;
    public const byte LEAVE_ROOM = 9;
    public const byte START_GAME = 10;
    public const byte CAN_START_GAME = 11;
    public const byte KICK_PLAYER_FROM_LOBBY = 12;
    public const byte GET_PLAYER_CIVILIZATION = 13;
    public const byte READY_TO_START = 14;

    // terrain specific tags
    public const byte SEND_TREE_DATA = 50;
    public const byte SEND_GOLD_DATA = 51;
    public const byte SEND_STONE_DATA = 52;
    public const byte SEND_FARM_DATA = 53;
    public const byte SEND_PLAYER_DATA = 58;
    public const byte SEND_WORLD_DATA = 59;
    public const byte DONE_SENDING_TERRAIN = 60;
    public const byte DONE_INIT_WORLD = 61;
    public const byte RESOURCE_EXHAUST = 62;

    // player specific actions performed during gameplay
    public const byte PLAYER_MOVE = 100;
    public const byte PLAYER_ROTATE = 101;
    public const byte PLAYER_ATTACK = 102;
    public const byte PLAYER_SPAWN_UNIT = 103;
    public const byte PLAYER_UNIT_DEATH = 104;
    public const byte PLAYER_STOP_UNIT = 105;
    public const byte PLAYER_BUILD = 106;
    public const byte PLAYER_GATHER_RESOURCE = 107;
    public const byte PLAYER_TECHNOLOGY_UPGRADE = 108;
    public const byte PLAYER_BUILDING_DISCOVER = 109;
    public const byte PLAYER_UNIT_HP_UPGRADE = 110;
    public const byte PLAYER_SEND_PROJECTILE = 111;
    public const byte PLAYER_TAKE_DAMAGE = 112;
    public const byte MIXED_MESSAGE = 113; // signal a package composed from smaller packages
    public const byte PLAYER_VILLAGER_WALK = 114;
    public const byte PLAYER_VILLAGER_GATHER = 115;
    public const byte PLAYER_IDENTIFY_UNIT = 116;
    public const byte PLAYER_SEND_WAYPOINT = 117;

    // ACKs or NACKs received by players during gameplay
    public const byte PLAYER_ACTION_VALIDATION = 120;
}
