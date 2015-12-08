using UnityEngine;
using System.Collections;

public class CheckRegion : MonoBehaviour {

    public int numberRegionVerAndHor = 50;
    public int regionSize = 5;

    public int x;
    public int y;

    public void Load() 
    {
         this.x = GetIndex(this.transform.position.x);
         this.y = GetIndex(this.transform.position.z);
    }

    public int GetIndex(float position)
    {
        int indexResult = (numberRegionVerAndHor / 2) + (int)(position / regionSize);
        return indexResult;
    }
}
