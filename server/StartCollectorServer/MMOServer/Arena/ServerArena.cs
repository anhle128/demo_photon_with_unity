using ExitGames.Concurrency.Fibers;
using LMLiblary.General;
using MMOServer.Peer;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MMOServer.Arena
{
    public class ServerArena : IDisposable
    {
        public static int NumberRegionVerAndHor = 50;
        public static int RegionSize = 5;

        public List<IActor> ListPlayer = new List<IActor>();
        public List<IActor> ListCreep = new List<IActor>();


        public List<ActorPeer> ListPeer;

        private long _lastAssignedActorId = long.MinValue;
        private readonly System.Random rand = new System.Random();
        public static ServerArena Instance;
        private IFiber _executionFiber;

        public void Dispose()
        {
            if (_executionFiber != null)
            {
                _executionFiber.Dispose();
                _executionFiber = null;
            }
        }

        public void Startup()
        {
            _executionFiber = new PoolFiber();
            _executionFiber.Start();

            ListPeer = new List<ActorPeer>();

            InitRegion();

            InitCreep();
        }

        public void ShutDown()
        {
            _executionFiber.Dispose();
        }

        public void InitCreep()
        {
            for (int i = 0; i < 500; i++)
            {
                // find a random position
                double x = rand.Next(-24, 220);
                double y = rand.Next(-24, 220);

                IActor creep = new IActor()
                {
                    actorType = (byte)ActorType.Creep,
                    posX = (float)x,
                    posY = (float)y,
                    actorID = AllocateActorId()
                };
                ListCreep.Add(creep);

                Region region = Region.GetRegionFromPosition(creep);
                region.ActorEnter(creep);
            }
        }

        public void InitRegion()
        {
            Region.arrRegion = new Region[NumberRegionVerAndHor, NumberRegionVerAndHor];
            for (int x = 0; x < NumberRegionVerAndHor; x++)
            {
                for (int y = 0; y < NumberRegionVerAndHor; y++)
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

                    if (checkX >= 0 && checkX < NumberRegionVerAndHor && checkY >= 0 && checkY < NumberRegionVerAndHor)
                    {
                        listResult.Add(Region.arrRegion[checkX, checkY]);
                    }
                }
            }
            return listResult;
        }

        public void Enter(ActorPeer peer)
        {
            _executionFiber.Enqueue(() =>
            {
                ListPeer.Add(peer);
                SpawnPlayer(peer);
            });
        }

        public void SpawnPlayer(ActorPeer peer)
        {
            IActor player = new IActor()
            {
                actorType = (byte)ActorType.Player,
                peerID = peer.playerID,
                actorID = AllocateActorId(),
                posX = 10,
                posY = 10
            };
            ListPlayer.Add(player);

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

        public long AllocateActorId()
        {
            return _lastAssignedActorId++;
        }

        public void Exit(ActorPeer peer)
        {
            _executionFiber.Enqueue(() =>
            {
                ListPeer.Remove(peer);
            });

            IActor player = ListPlayer.FirstOrDefault(a => a.peerID == peer.playerID);
            if (player != null)
            {
                lock (ListPlayer)
                {
                    ListPlayer.Remove(player);
                }
                Region currentRegion = Region.GetRegionFromPosition(player);
                currentRegion.ActorExit(player);
            }
        }

        public void OnOperationRequest(ActorPeer sender, OperationRequest operationRequest, SendParameters sendParameters)
        {
            _executionFiber.Enqueue(() =>
            {
                this.ProcessRequest(sender,
                    operationRequest, sendParameters);
            });
        }

        public void ProcessRequest(ActorPeer sender, OperationRequest operationRequest, SendParameters sendParameters)
        {
            if (operationRequest.OperationCode == (byte)AckRequestType.MoveCommand)
            {
                Movement movement = GeneralFunc.Deserialize<Movement>(operationRequest.Parameters[(byte)Parameter.Data] as byte[]);

                // find actor
                IActor player = (ListPlayer.Find(pl => pl.actorID == movement.actorID));

                Region oldRegion = Region.GetRegionFromPosition(player);

                player.posX = movement.posX;
                player.posY = movement.posY;

                Region newRegion = Region.GetRegionFromPosition(player);

                if (oldRegion != newRegion)
                {
                    oldRegion.ActorExit(player);

                    newRegion.ActorEnter(player);
                }
            }
        }
    }
}
