using System;
using System.Collections.Generic;

public class Creep : IActor
{
    public Creep()
    {
        this.actorType = (byte)ActorType.Creep;
    }
}
