using System;

public enum AckEventType : byte
{
    AssignPlayerID = 0,
    CreateActor = 1,
    DestroyActor = 2,
    UpdateActor = 3,
    ChatMessage = 4
}
