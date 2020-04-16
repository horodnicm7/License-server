using System;
using System.Collections.Generic;
using System.Text;

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
