﻿using ExitGames.Concurrency.Fibers;
using LMLiblary.General;
using MMOServer.Actor;
using MMOServer.Peer;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MMOServer.Arena
{
    public class ServerArena : IDisposable
    {
        int numberRegionVertical = 50;
        int numberRegionHorizontal = 50;
        int regionHeight = 1;
        int regionWidth = 1;

        public List<Player> listPlayer = new List<Player>();
        public List<Creep> listCreep = new List<Creep>();

        public Region[,] arrRegion;

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
            for (int i = 0; i < 100; i++)
            {
                // find a random position
                double x = rand.NextDouble();
                double y = rand.NextDouble();
                // map to the range -50, +50
                x -= 0.5f;
                x *= 100f; // 0.5 * 100 = 50
                y -= 0.5f;
                y *= 100f;

                Creep creep = new Creep()
                {
                    posX = (float)x,
                    posY = (float)y,
                    actorID = AllocateActorID()
                };
                listCreep.Add(creep);

                Region region = GetRegionFromPosition(creep);
                region.ActorEnter(creep);
            }
        }

        public void InitRegion() 
        {
            arrRegion = new Region[numberRegionHorizontal, numberRegionVertical];
            for (int x = 0; x < numberRegionHorizontal; x++)
            {
                for (int y = 0; y < numberRegionVertical; y++)
                {
                    Region region = new Region()
                    {
                        x = x,
                        y = y
                    };
                    arrRegion[x, y] = region;
                }
            }
        }

        public Region GetRegionFromPosition(IActor actor) 
        {
            int indexX = (int)(actor.posX / regionWidth) + 1;
            if (indexX > numberRegionHorizontal-1)
            {
                indexX = numberRegionVertical - 1;
                actor.posX = indexX * regionWidth;
            }


            int indexY = (int)(actor.posY / regionHeight) + 1;
            if(indexY > numberRegionVertical)
            {
                indexY = numberRegionVertical - 1;
                actor.posY = indexY * regionHeight;
            }

            return arrRegion[indexX, indexY];
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

                    if (checkX >= 0 && checkX < numberRegionHorizontal && checkY >= 0 && checkY < numberRegionVertical)
                    {
                        listResult.Add(arrRegion[checkX, checkY]);
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
            Player player = new Player()
            {
                peerID = peer.playerID,
                actorID = AllocateActorID()
            };
            listPlayer.Add(player);


            Region startRegion = GetRegionFromPosition(player);
            startRegion.ActorEnter(player);

            List<Player> listPlayerInRegion = new List<Player>();
            List<Creep> listCreepInRegion = new List<Creep>();

            List<Region> listInterestRegion = GetInterestRegion(startRegion);
            foreach (var region in listInterestRegion)
            {
                listPlayerInRegion.AddRange(region.listPlayer);
                listCreepInRegion.AddRange(region.listCreep);
            }
            listPlayerInRegion.Remove(player);

            #region Send data creep and other player to this peer
            EventData evtData = new EventData()
            {
                Code = (byte)AckEventType.CreateActor,
                Parameters = new Dictionary<byte, object>()
                {
                    { (byte)ActorType.Player,GeneralFunc.Serialize(listPlayer)},
                    {(byte)ActorType.Creep, GeneralFunc.Serialize(listCreep)}
                }
            };
            peer.SendEvent(evtData, new SendParameters());
            #endregion 

            #region Send player to other peer
            List<Player> listOnePlayer = new List<Player>();
            listOnePlayer.Add(player);

            foreach (var playerInRegion in listPlayerInRegion)
            {
                ActorPeer actorPeer = listPeer.Where(a => a.playerID == playerInRegion.actorID).FirstOrDefault();
                if (actorPeer != null)
                {
                    EventData evtPlayer = new EventData()
                    {
                        Code = (byte)AckEventType.CreateActor,
                        Parameters = new Dictionary<byte, object>()
                        {
                            {(byte)ActorType.Player,GeneralFunc.Serialize(listOnePlayer)}
                        }
                    };
                    actorPeer.SendEvent(evtData, new SendParameters());
                }
            } 
            #endregion
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

             Player player = listPlayer.Where(a => a.peerID == peer.playerID).FirstOrDefault();
             if (player != null)
             {
                lock (listPlayer)
                {
                    listPlayer.Remove(player);
                }
                Region startRegion = GetRegionFromPosition(player);
                startRegion.ActorEnter(player);

                List<Player> listPlayerInRegion = new List<Player>();

                List<Region> listInterestRegion = GetInterestRegion(startRegion);
                foreach (var region in listInterestRegion)
                {
                    listPlayerInRegion.AddRange(region.listPlayer);
                }

                EventData evtData = new EventData()
                {
                    Code = (byte)AckEventType.DestroyActor,
                    Parameters = new Dictionary<byte, object>()
                    {
                        {(byte)ActorType.Player, GeneralFunc.Serialize(player)}
                    }
                };
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
                IActor player = GeneralFunc.Deserialize<IActor>(operationRequest.Parameters[0] as byte[]);
                //// move command from player
                //long actorID = (long)operationRequest.Parameters[0];
                //float velX = (float)operationRequest.Parameters[1];
                //float velY = (float)operationRequest.Parameters[2];

                //// find actor
                //Player player = (listPlayer.Find(pl =>
                //{
                //    return pl.actorID == actorID;
                //}));

                //player.velocityX = velX;
                //player.velocityY = velY;
            }
        }
    }
}