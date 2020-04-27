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

    // terrain specific tags
    public const byte SEND_TREE_DATA = 50;
    public const byte SEND_GOLD_DATA = 51;
    public const byte SEND_STONE_DATA = 52;
    public const byte SEND_FARM_DATA = 53;
    public const byte SEND_PLAYER_DATA = 58;
    public const byte SEND_WORLD_DATA = 59;
    public const byte DONE_SENDING_TERRAIN = 60;

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

    // ACKs or NACKs received by players during gameplay
    public const byte PLAYER_ACTION_VALIDATION = 120;
}
