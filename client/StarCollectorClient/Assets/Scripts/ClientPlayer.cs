using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClientPlayer : MonoBehaviour 
{
    public float moveSpeed = 5f;

    private OwnerInfo ownerInfo;
    private GameActor actorInfo;
    public bool isMine = false;

    private Vector3 lastReceivedMove;

    private float timeOfLastMoveCmd = 0f;

	// Use this for initialization
	void Start () 
    {
        timeOfLastMoveCmd = Time.time;

        lastReceivedMove = transform.position;

        ownerInfo = GetComponent<OwnerInfo>();
        actorInfo = GetComponent<GameActor>();
        isMine = (ownerInfo.OwnerID == StarCollectorClient.playerID);

        Camera.main.transform.parent = this.transform;
        Camera.main.transform.localPosition = new Vector3(0f, 14f, 0f);
    }
	
	// Update is called once per frame
	void Update ()
    {
	    if(isMine)
        {
            // get movement direction
            float mX = Input.GetAxis("Horizontal") * moveSpeed;
            float mY = Input.GetAxis("Vertical") * moveSpeed;

            this.transform.position += (new Vector3(mX, 0f, mY))*Time.deltaTime;

            if (Time.time >= timeOfLastMoveCmd + 0.1f && lastReceivedMove != this.transform.position)
            {
                lastReceivedMove = this.transform.position;
                timeOfLastMoveCmd = Time.time;

                // send move command to server every 0.1 seconds
                Dictionary<byte, object> requestDict = new Dictionary<byte, object>();

                Movement moveMent = new Movement();
                moveMent.actorID = actorInfo.actor.actorID;
                moveMent.posX = this.transform.position.x;
                moveMent.posY = this.transform.position.z;

                requestDict.Add((byte)Parameter.Data,LMLiblary.General.GeneralFunc.Serialize(moveMent));

                StarCollectorClient.connection.OpCustom((byte)AckRequestType.MoveCommand, requestDict, false);
                this.GetComponent<CheckRegion>().Load();
            }

            //transform.position = Vector3.Lerp(transform.position,lastReceivedMove, Time.deltaTime * 20f);
        }
	}

    public void UpdatePosition(Vector3 newPos)
    {
        lastReceivedMove = newPos;
    }
}
