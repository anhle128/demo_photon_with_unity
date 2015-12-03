using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameActor : MonoBehaviour
{

    public static Dictionary<long, GameActor> dictActor = new Dictionary<long, GameActor>();

    public long actorID;

    public void SetActorID(long actorID) 
    {
        this.actorID = actorID;
        dictActor.Add(this.actorID, this);
    }

    public void Destruct() 
    {
        dictActor.Remove(this.actorID);
        Destroy(this.gameObject);
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
