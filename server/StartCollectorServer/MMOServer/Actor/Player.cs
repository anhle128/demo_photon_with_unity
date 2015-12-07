using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMOServer.Actor
{
    public class Player : IActor
    {
        public Player() 
        {
            this.actorType = (byte)ActorType.Player;
        }
    }
}
