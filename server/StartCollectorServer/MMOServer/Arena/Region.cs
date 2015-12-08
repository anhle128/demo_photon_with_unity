using LMLiblary.General;
using MMOServer.Peer;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MMOServer.Arena
{
    public class Region
    {
        public int x;
        public int y;
        public List<IActor> listPlayer = new List<IActor>();
        public List<IActor> listCreep = new List<IActor>();

        public static Region[,] arrRegion;

        public void ActorEnter(IActor actor) 
        {
            if(actor.actorType == (byte)ActorType.Player)
            {
                lock (listPlayer)
                {
                    listPlayer.Add(actor);
                }

                ActorPeer peer = ServerArena.Instance.listPeer.Where(a => a.playerID == actor.peerID).First();

                SendNewInfoToPeer(peer, actor);
                SendNewPeerToTheOthers(actor, GetListPlayerInterest());
            }
            else 
            {
                lock (listCreep)
                {
                    listCreep.Add(actor);
                }
            }
        }

        public void ActorExit(IActor actor) 
        {
            if (actor.actorType == (byte)ActorType.Player)
            {
                lock (listPlayer)
                {
                    listPlayer.Remove(actor);
                }

                List<IActor> listPlayerInterest = GetListPlayerInterest();
                DeletePeerToOther(actor, listPlayerInterest);
            }
            else
            {
                lock (listCreep)
                {
                    listCreep.Remove(actor);
                }
            }
        }

        private List<IActor> GetListPlayerInterest()
        {
            List<IActor> listPlayerInterest = new List<IActor>();
            List<Region> listInterestRegion = GetInterestRegion();
            foreach (var region in listInterestRegion)
            {
                listPlayerInterest.AddRange(region.listPlayer);
            }
            return listPlayerInterest;
        }

        private void SendNewInfoToPeer(ActorPeer peer, IActor player)
        {
            List<IActor> listPlayerInRegion = new List<IActor>();
            List<IActor> listCreepInRegion = new List<IActor>();

            List<Region> listInterestRegion = GetInterestRegion();
            foreach (var region in listInterestRegion)
            {
                listPlayerInRegion.AddRange(region.listPlayer);
                listCreepInRegion.AddRange(region.listCreep);
            }
            listPlayerInRegion.Remove(player);

            InterestRegions dataInterest = GetDataInterestRegion(listInterestRegion);
            dataInterest.playerPosition = new MPosition()
            {
                x = player.posX,
                y = player.posY
            };


            EventData evtDataCrrepAndOtherPlayers = new EventData()
            {
                Code = (byte)AckEventType.CreateActor,
                Parameters = new Dictionary<byte, object>()
                {
                    {(byte)Parameter.Player,GeneralFunc.Serialize(listPlayerInRegion)},
                    {(byte)Parameter.Creep, GeneralFunc.Serialize(listCreepInRegion)},
                    {(byte)Parameter.Regions,GeneralFunc.Serialize(dataInterest)}
                }
            };
            peer.SendEvent(evtDataCrrepAndOtherPlayers, new SendParameters());
        }

        private void SendNewPeerToTheOthers(IActor player, List<IActor> listPlayerInRegion)
        {
            List<IActor> listOnePlayer = new List<IActor>();
            listOnePlayer.Add(player);

            foreach (var playerInRegion in listPlayerInRegion)
            {
                ActorPeer actorPeer = ServerArena.Instance.listPeer.Where(a => a.playerID == playerInRegion.actorID).FirstOrDefault();
                if (actorPeer != null)
                {
                    EventData evtOtherPlayer = new EventData()
                    {
                        Code = (byte)AckEventType.CreateActor,
                        Parameters = new Dictionary<byte, object>()
                        {
                            {(byte)ActorType.Player,GeneralFunc.Serialize(listOnePlayer)}
                        }
                    };
                    actorPeer.SendEvent(evtOtherPlayer, new SendParameters());
                }
            }
        }

        private void DeletePeerToOther(IActor player, List<IActor> listPlayerInRegion) 
        {
            foreach (var playerInRegion in listPlayerInRegion)
            {
                ActorPeer actorPeer = ServerArena.Instance.listPeer.Where(a => a.playerID == playerInRegion.actorID).FirstOrDefault();
                if (actorPeer != null)
                {
                    EventData evtOtherPlayer = new EventData()
                    {
                        Code = (byte)AckEventType.DestroyActor,
                        Parameters = new Dictionary<byte, object>()
                        {
                            {(byte)Parameter.Data,GeneralFunc.Serialize(player)}
                        }
                    };
                    actorPeer.SendEvent(evtOtherPlayer, new SendParameters());
                }
            }
        }

        public List<Region> GetInterestRegion()
        {
            List<Region> listResult = new List<Region>();
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int checkX = this.x + x;
                    int checkY = this.y + y;

                    if (checkX >= 0 && checkX < ServerArena.numberRegionVerAndHor && checkY >= 0 && checkY < ServerArena.numberRegionVerAndHor)
                    {
                        listResult.Add(Region.arrRegion[checkX, checkY]);
                    }
                }
            }
            return listResult;
        }

        public static Region GetRegionFromPosition(IActor actor)
        {
            int indexX = GetIndex(actor.posX);
            int indexY = GetIndex(actor.posY);
            return Region.arrRegion[indexX, indexY];
        }

        private static int GetIndex(float position)
        {
            int indexResult = (ServerArena.numberRegionVerAndHor / 2) + (int)(position / ServerArena.regionSize);
            return indexResult;
        }

        private InterestRegions GetDataInterestRegion(List<Region> listInterestRegion) 
        {
            InterestRegions dataInterest = new InterestRegions() { listRegion = new List<MPosition>() };

            foreach (var region in listInterestRegion)
            {
                MPosition data = new MPosition() { x = region.x, y = region.y };
                dataInterest.listRegion.Add(data);
            }

            return dataInterest;
        }
    }
}
