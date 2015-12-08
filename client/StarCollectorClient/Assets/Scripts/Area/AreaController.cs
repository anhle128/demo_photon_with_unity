using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AreaController : MonoBehaviour {

    #region Singleton
    private static AreaController instance;

    public static AreaController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AreaController>();
            }
            return AreaController.instance;
        }
    }
    
    #endregion

    public static int numberRegionVerAndHor = 50;
    public static int regionSize = 5;

    public GameObject regionPref;

    public GameObject[,] arrRegion;

	// Use this for initialization
	void Start () {
        CreateArea();
	}

    void CreateArea() 
    {
        GameObject[,] listRegion = new GameObject[numberRegionVerAndHor, numberRegionVerAndHor];
        for (int x = 0; x < numberRegionVerAndHor; x++)
        {
            for (int y = 0; y < numberRegionVerAndHor; y++)
            {
                Vector3 regionPosition = new Vector3(-numberRegionVerAndHor/2 + x*regionSize, 0, -numberRegionVerAndHor/2 + y*regionSize);
                GameObject regionCreated = Instantiate(regionPref, regionPosition, regionPref.transform.rotation) as GameObject;
                listRegion[x, y] = regionCreated;
                regionCreated.transform.parent = this.transform;
                regionCreated.transform.localScale = new Vector3(regionSize, regionSize, 0f);
            }
        }
    }
	
	
}
