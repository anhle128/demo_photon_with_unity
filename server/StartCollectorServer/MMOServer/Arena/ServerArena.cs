using ExitGames.Concurrency.Fibers;
using LMLiblary.General;
using LMLiblary.Model;
using MMOServer.Peer;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MMOServer.Arena
{
    public class ServerArena : IDisposable
    {
        public static int numberRegionVerAndHor = 50;
        public static int regionSize = 5;

        public List<IActor> listPlayer = new List<IActor>();
        public List<IActor> listCreep = new List<IActor>();


        public List<ActorPeer> listPeer;

        private long lastAssignedActorID = long.MinValue;
        private System.Random rand = new System.Random();
        public static ServerArena Instance;
        private IFiber executionFiber;

        public void Dispose()
        {
            if (executionFiber != null)
            {
                executionFiber.Dispose();
                executionFiber = null;
            }
        }

        public void Startup()
        {
            executionFiber = new PoolFiber();
            executionFiber.Start();

            listPeer = new List<ActorPeer>();

            InitRegion();

            InitCreep();
        }

        public void ShutDown()
        {
            executionFiber.Dispose();
        }

        public void InitCreep() 
        {
            for (int i = 0; i < 500; i++)
            {
                // find a random position
                double x = rand.Next(-25, 225);
                double y = rand.Next(-25, 225);

                IActor creep = new IActor()
                {
                    actorType = (byte)ActorType.Creep,
                    posX = (float)x,
                    posY = (float)y,
                    actorID = AllocateActorID()
                };
                listCreep.Add(creep);

                Region region = Region.GetRegionFromPosition(creep);
                region.ActorEnter(creep);
            }
        }

        public void InitRegion() 
        {
            Region.arrRegion = new Region[numberRegionVerAndHor, numberRegionVerAndHor];
            for (int x = 0; x < numberRegionVerAndHor; x++)
            {
                for (int y = 0; y < numberRegionVerAndHor; y++)
                {
                    Region region = new Region()
                    {
                        x = x,
                        y = y
                    };
                    Region.arrRegion[x, y] = region;
                }
            }
        }

        public List<Region> GetInterestRegion(Region pointRegion) 
        {
            List<Region> listResult = new List<Region>();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int checkX = pointRegion.x + x;
                    int checkY = pointRegion.y + y;

                    if (checkX >= 0 && checkX < numberRegionVerAndHor && checkY >= 0 && checkY < numberRegionVerAndHor)
                    {
                        listResult.Add(Region.arrRegion[checkX, checkY]);
                    }
                }
            }
            return listResult;
        }

        public void Enter(ActorPeer peer) 
        {
            executionFiber.Enqueue(() =>
            {
                listPeer.Add(peer);
                SpawnPlayer(peer);
            });
        }

        public void SpawnPlayer(ActorPeer peer)
        {
            IActor player = new IActor()
            {
                actorType = (byte)ActorType.Player,
                peerID = peer.playerID,
                actorID = AllocateActorID(),
                posX =10,
                posY =10
            };
            listPlayer.Add(player);

            //#region Test Code
            ////List<MPlayer> listTest = new List<MPlayer>()
            ////{
            ////    new MPlayer() { id = 1,name = "duc anh"},
            ////    new MPlayer() {id = 2, name = "hoang anh"}
            ////};
            //EventData evtData = new EventData()
            //{
            //    Code = (byte)AckEventType.CreateActor,
            //    Parameters = new Dictionary<byte, object>()
            //    {
            //        {(byte)ActorType.Creep, GeneralFunc.Serialize(listCreep)}
            //    }
            //};
            //peer.SendEvent(evtData, new SendParameters());
            //#endregion

            Region startRegion = Region.GetRegionFromPosition(player);
            Console.WriteLine("aaaaaaaaaaaaaaaaaaaaaaa");
            startRegion.ActorEnter(player);

            EventData evtPlayer = new EventData()
            {
                Code = (byte)AckEventType.AssignPlayerID,
                Parameters = new Dictionary<byte, object>() 
                {
                    {0,GeneralFunc.Serialize(player)}
                }
            };
            peer.SendEvent(evtPlayer, new SendParameters());
        }

        public long AllocateActorID()
        {
            return lastAssignedActorID++;
        }

        public void Exit(ActorPeer peer) 
        {
            executionFiber.Enqueue(() =>
            {
                listPeer.Remove(peer);
            });

             IActor player = listPlayer.Where(a => a.peerID == peer.playerID).FirstOrDefault();
             if (player != null)
             {
                lock (listPlayer)
                {
                    listPlayer.Remove(player);
                }
                Region currentRegion = Region.GetRegionFromPosition(player);
                currentRegion.ActorExit(player);
            }
        }

        public void OnOperationRequest(ActorPeer sender, OperationRequest operationRequest, SendParameters sendParameters)
        {
            executionFiber.Enqueue(() =>
            {
                this.ProcessRequest(sender,
                    operationRequest, sendParameters);
            });
        }

        public void ProcessRequest(ActorPeer sender, OperationRequest operationRequest, SendParameters sendParameters)
        {
            if (operationRequest.OperationCode == (byte)AckRequestType.MoveCommand)
            {
                // move command from player
                //long actorID = (long)operationRequest.Parameters[0];
                //float velX = (float)operationRequest.Parameters[1];
                //float velY = (float)operationRequest.Parameters[2];

                Movement movement = GeneralFunc.Deserialize<Movement>(operationRequest.Parameters[(byte)Parameter.Data] as byte[]);

                // find actor
                IActor player = (listPlayer.Find(pl =>
                {
                    return pl.actorID == movement.actorID;
                }));

                Region oldRegion = Region.GetRegionFromPosition(player);

                player.posX = movement.posX;
                player.posY = movement.posY;

                Region newRegion = Region.GetRegionFromPosition(player);

                if(oldRegion != newRegion)
                {
                    oldRegion.ActorExit(player);

                    newRegion.ActorEnter(player);
                }
            }
        }
    }
}
