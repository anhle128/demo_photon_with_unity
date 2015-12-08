using UnityEngine;
using System.Collections;

public class CheckRegion : MonoBehaviour {

    public int numberRegionVerAndHor = 50;
    public int regionSize = 5;

    public int x;
    public int y;

    public void Load() 
    {
         this.x = AreaController.Instance.GetIndex(this.transform.position.x);
         this.y = AreaController.Instance.GetIndex(this.transform.position.z);
    }
}
