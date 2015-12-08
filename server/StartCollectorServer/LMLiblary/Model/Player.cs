using System;
using System.Collections.Generic;

public class Player : IActor
{
    public Player()
    {
        this.actorType = (byte)ActorType.Player;
    }
}