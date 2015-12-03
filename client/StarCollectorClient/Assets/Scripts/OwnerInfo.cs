using UnityEngine;
using System.Collections;

public class OwnerInfo : MonoBehaviour 
{
    public long OwnerID;

    public bool IsMine 
    {
        get 
        {
            return OwnerID == StarCollectorClient.playerID;
        }
    }

    public void SetOwnerID(long ownerID) 
    {
        this.OwnerID = ownerID;
    }
}
