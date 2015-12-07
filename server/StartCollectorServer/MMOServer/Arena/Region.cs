using System;
using System.Collections.Generic;
using System.Linq;
using MMOServer.Actor;

namespace MMOServer.Arena
{
    public class Region
    {
        public int x;
        public int y;
        public List<Player> listPlayer = new List<Player>();
        public List<Creep> listCreep = new List<Creep>();

        public void ActorEnter(IActor actor) 
        {
           
            if(actor.actorType == (byte)ActorType.Player)
            {
                lock (listPlayer)
                {
                    listPlayer.Add(actor as Player);
                }
            }
            else 
            {
                lock (listCreep)
                {
                    listCreep.Add(actor as Creep);
                }
            }
        }

        public void ActorExit(IActor actor) 
        {
            if (actor.actorType == (byte)ActorType.Player)
            {
                lock (listPlayer)
                {
                    listPlayer.Remove(actor as Player);
                }
            }
            else
            {
                lock (listCreep)
                {
                    listCreep.Remove(actor as Creep);
                }
            }
        }
    }
}
