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
}
