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

    public TileRegion regionPref;

    public TileRegion[,] arrRegion;

    private string nameRegion = "region";
    private string nameInterestRegion = "InterestRegion";

    // Use this for initialization
    void Start () {
        CreateArea();
        //Test();
	}

    void CreateArea() 
    {
        arrRegion = new TileRegion[numberRegionVerAndHor, numberRegionVerAndHor];
        for (int x = 0; x < numberRegionVerAndHor; x++)
        {
            for (int y = 0; y < numberRegionVerAndHor; y++)
            {
                Vector3 regionPosition = new Vector3(-numberRegionVerAndHor/2 + x*regionSize, 0, -numberRegionVerAndHor/2 + y*regionSize);
                TileRegion regionCreated = Instantiate(regionPref, regionPosition, regionPref.transform.rotation) as TileRegion;
                regionCreated.x = x;
                regionCreated.y = y;
                arrRegion[x, y] = regionCreated;
                regionCreated.transform.parent = this.transform;
                regionCreated.transform.localScale = new Vector3(regionSize, regionSize, 0f);
                regionCreated.renderer.material.color = Color.white;
            }
        }
    }
	
	public void ShowInterestRegion(List<MPosition> listPosion)
    {
        ClearInterestRegion();
        foreach (var position in listPosion)
        {
            arrRegion[(int)position.x, (int)position.y].name = nameInterestRegion;
            arrRegion[(int)position.x,(int)position.y].renderer.material.color = Color.red;
        }
    }

    void ClearInterestRegion()
    {
        for (int x = 0; x < numberRegionVerAndHor; x++)
        {
            for (int y = 0; y < numberRegionVerAndHor; y++)
            {
                arrRegion[x, y].name = nameRegion;
                arrRegion[x, y].renderer.material.color = Color.white;
            }
        }
    }

    void Test()
    {
        System.Random rand = new System.Random();
        for (int i = 0; i < 100; i++)
        {
            // find a random position
            double x = rand.Next(-25, 225);
            double y = rand.Next(-25, 225);

            GetRegionFromPosition(x, y);
        }
    }

    public TileRegion GetRegionFromPosition(double posX, double posY)
    {
        int indexX = GetIndex(posX);
        int indexY = GetIndex(posY);
        return arrRegion[indexX, indexY];
    }

    public int GetIndex(double position)
    {
        //int indexResult = (ServerArena.numberRegionVerAndHor / 2) + (int)(position / ServerArena.regionSize);

        int indexResult = (int)((position + (numberRegionVerAndHor / 2)) / regionSize);
        return indexResult;
    }
}
