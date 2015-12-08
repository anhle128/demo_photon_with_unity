using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameActor : MonoBehaviour
{

    public static List<GameActor> listPlayer = new List<GameActor>();
    public static List<GameActor> listCreep = new List<GameActor>();

    public int x;
    public int y;

    public IActor actor;

    public void SetActor(IActor actor) 
    {
        if(actor.index != null)
        {
            this.x = actor.index.x;
            this.y = actor.index.y;
        }

       this.actor = actor;
       if (actor.actorType == (byte)ActorType.Player)
       {
           listPlayer.Add(this);
       }
       else 
       {
           listCreep.Add(this);
       }
    }

    public void Destruct() 
    {
        if (actor.actorType == (byte)ActorType.Player)
        {
            listPlayer.Remove(this);
        }
        else
        {
            listCreep.Remove(this);
        }
        Destroy(this.gameObject);
    }
}
