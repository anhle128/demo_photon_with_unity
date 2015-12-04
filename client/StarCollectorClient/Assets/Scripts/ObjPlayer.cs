using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjPlayer : MonoBehaviour 
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

            if (Time.time >= timeOfLastMoveCmd + 0.1f)
            {

                Debug.Log("vao day");
                timeOfLastMoveCmd = Time.time;

                // send move command to server every 0.1 seconds
                Dictionary<byte, object> moveParams = new Dictionary<byte, object>();
                moveParams[0] = actorInfo.actorID;
                moveParams[1] = mX;
                moveParams[2] = mY;

                StarCollectorClient.connection.OpCustom((byte)AckRequestType.MoveCommand, moveParams, false);
            }

            //transform.position = Vector3.Lerp(transform.position,lastReceivedMove, Time.deltaTime * 20f);
        }
	}

    public void UpdatePosition(Vector3 newPos)
    {
        lastReceivedMove = newPos;
    }
}
