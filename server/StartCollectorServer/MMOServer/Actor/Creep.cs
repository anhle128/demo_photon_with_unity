using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMOServer.Actor
{
    public class Creep : IActor
    {
        public Creep() 
        {
            this.actorType = (byte)ActorType.Creep;
        }
    }
}
