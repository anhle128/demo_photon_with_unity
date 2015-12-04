using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Star : Actor
{
    public Star() 
    {
        this.actorType = (byte)ActorType.Star;
        this.radius = 0.25f;
    }

    public bool DetectCollision(Actor other) 
    {
        // calculate square distance between actors
        float sqrDist = ((this.posX - other.posX) * (this.posX -
        other.posX) + (this.posY - other.posY) * (this.posY -
        other.posY));
        // if the distance is less than the sum of the radii, collision occurs
        if (sqrDist <= (this.radius + other.radius))
        {
            return true;
        }
        return false;
    }
}
