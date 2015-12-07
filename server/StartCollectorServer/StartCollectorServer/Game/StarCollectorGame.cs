using ExitGames.Concurrency.Fibers;
using LMLiblary.General;
using LMLiblary.Model;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class StarCollectorGame : IDisposable
{

    public List<Star> listStar = new List<Star>();
    public List<Player> listPlayer = new List<Player>();
    private long lastAssignedActorID = long.MinValue;
    private System.Random rand = new System.Random();

    public static StarCollectorGame Instance;
    public List<StarCollectorPeer> listPeer;
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

        listPeer = new List<StarCollectorPeer>();


        InitRound();

        // schedule Simulate 10 times per second, or once every 100 milliseconds
        executionFiber.ScheduleOnInterval
        (
            action : delegate() 
            {
                Simulate(0.1f);
            },
            firstInMs: 0, 
            regularInMs: 100
        );
    }

    public void ShutDown() 
    {
        executionFiber.Dispose();
    }

    public void PeerJoined(StarCollectorPeer peer)
    {
        executionFiber.Enqueue( () => 
        {
            listPeer.Add(peer);
            SpawnPlayer(peer);
        });

        #region Test code
        //List<MPlayer> listTest = new List<MPlayer>()
        //{
        //    new MPlayer()
        //    {
        //        name = "ducanh",
        //        id =1
        //    },
        //    new MPlayer()
        //    {
        //        name = "hoanganh",
        //        id = 2
        //    }
        //};
        //EventData evtDataPlayer = new EventData()
        //{
        //    Code = (byte)AckEventType.TestData,
        //    Parameters = new Dictionary<byte, object> 
        //    {
        //        {(byte)ActorType.Player,GeneralFunc.Serialize(listTest)}
        //    }
        //};
        //peer.SendEvent(evtDataPlayer, new SendParameters()); 
        #endregion

        // send player CreateActor events for players and stars which are in interest



        //foreach (var p in listPlayer)
        //{
        //    EventData evt = new EventData() 
        //    { 
        //        Code = (byte)AckEventType.CreateActor,
        //        Parameters = new Dictionary<byte, object>() 
        //        {
        //            {0, p.actorType},
        //            {1, p.actorID},
        //            {2, p.posX},
        //            {3, p.posY},
        //            {4, (p.owner as StarCollectorPeer).playerID}
        //        }
        //    };
            
        //}
        //peer.SendEvent(evt, new SendParameters());
        //EventData evtDataPlayer = new EventData()
        //{
        //    Code = (byte)AckEventType.CreateActor,
        //    Parameters = new Dictionary<byte, object> 
        //    {
        //        {(byte)ActorType.Player,listPlayer}
        //    }
        //};
        //peer.SendEvent(evtDataPlayer, new SendParameters());
        //peer.SendEvent()

        foreach (var s in listStar)
        {
            EventData evt = new EventData()
            {
                Code = (byte)AckEventType.CreateActor,
                Parameters = new Dictionary<byte, object>() 
                {
                    {0, s.actorType},
                    {1, s.actorID},
                    {2, s.posX},
                    {3, s.posY}
                }
            };
            peer.SendEvent(evt, new SendParameters());
        }

        //EventData evtDataStar = new EventData()
        //{
        //    Code = (byte)AckEventType.CreateActor,
        //    Parameters = new Dictionary<byte, object> 
        //    {
        //        {(byte)ActorType.Star,listStar}
        //    }
        //};
        //peer.SendEvent(evtDataStar, new SendParameters());

    }

    public void PeerLeft(StarCollectorPeer peer)
    {
        executionFiber.Enqueue(() =>
        {
            listPeer.Remove(peer);
        });

        Player player = listPlayer.Where(a => a.owner == peer).FirstOrDefault();
        if (player != null) 
        {
            EventData evt = new EventData() 
            {
                Code = (byte)AckEventType.DestroyActor,
                Parameters = new Dictionary<byte, object>() 
            };
            evt.Parameters[0] = player.actorID;
            BroadcastEvent(evt, new SendParameters());

            listPlayer.Remove(player);
        }

    }

    public void OnOperationRequest(StarCollectorPeer sender, OperationRequest operationRequest, SendParameters sendParameters)
    {
        executionFiber.Enqueue(() =>
        {
            this.ProcessMessage(sender,
                operationRequest, sendParameters);
        });
    }

    public void BroadcastEvent(IEventData evt, SendParameters param)
    {
        foreach (StarCollectorPeer peer in listPeer)
        {
            peer.SendEvent(evt, param);
        }
    }

    public void ProcessMessage(StarCollectorPeer sender, OperationRequest operationRequest, SendParameters sendParameters)
    {
        if(operationRequest.OperationCode == (byte)AckRequestType.MoveCommand)
        {
            // move command from player
            long actorID = (long)operationRequest.Parameters[0];
            float velX = (float)operationRequest.Parameters[1];
            float velY = (float)operationRequest.Parameters[2];

            // find actor
            Player player = (listPlayer.Find(pl =>
            {
                return pl.actorID == actorID;
            }));

            player.velocityX = velX;
            player.velocityY = velY;

            //List<Player> listOtherPlayer = 
        }
    }

    public long AllocateActorID()
    {
        return lastAssignedActorID++;
    }

    public void SpawnStar()
    {
        // find a random position
        double x = rand.NextDouble();
        double y = rand.NextDouble();
        // map to the range -50, +50
        x -= 0.5f;
        x *= 100f; // 0.5 * 100 = 50
        y -= 0.5f;
        y *= 100f;
        Star star = new Star() 
        { 
            posX = (float)x, 
            posY = (float)y, 
            actorID = AllocateActorID() 
        };
        listStar.Add(star);

        EventData evt = new EventData()
        {
            Code = (byte)AckEventType.CreateActor,
            Parameters = new Dictionary<byte, object>() 
            {
                {0, star.actorType},
                {1 , star.actorID},
                {2, star.posX},
                {3, star.posY}
            }
        };

        BroadcastEvent(evt, new SendParameters());
    }

    public void SpawnPlayer(StarCollectorPeer peer)
    {
        Player player = new Player() 
        { 
            owner = peer, 
            actorID = AllocateActorID() 
        };
        listPlayer.Add(player);

        //EventData evt = new EventData()
        //{
        //    Code = (byte)AckEventType.CreateActor,
        //    Parameters = new Dictionary<byte, object>() 
        //    {
        //        {0, player.actorType},
        //        {1 , player.actorID},
        //        {2, player.posX},
        //        {3, player.posY},
        //        {4, peer.playerID}
        //    }
        //};

        //BroadcastEvent(evt, new SendParameters());
    }

    public void Simulate(float timeStep) 
    {
        Star[] arrStar = listStar.ToArray();

        foreach (var player in listPlayer)
        {
            player.Simulate( timeStep );

            // broadcast move event
            EventData moveEvt = new EventData()
            {
                Code = (byte)AckEventType.UpdateActor,
                Parameters = new Dictionary<byte, object>()
                {
                    {0, player.actorID},
                    {1, player.posX},
                    {1, player.posY}
                }
            };
            BroadcastEvent(moveEvt, new SendParameters());

            foreach (var star in listStar)
            {
                if (star.DetectCollision(player))
                {
                    StarPickedUp(star, player);
                }
            }
        }
    }

    public void InitRound()
    {
        // reset players
        foreach (Player player in listPlayer)
        {
            player.posX = 0f;
            player.posY = 0f;
            player.velocityX = 0f;
            player.velocityY = 0f;
            player.score = 0;
        }
        // spawn new stars
        for (int i = 0; i < 100; i++)
        {
            SpawnStar();
        }
    }

    public void StarPickedUp(Star star, Player taker) 
    {
        listStar.Remove(star);

        EventData evt = new EventData()
        {
            Code = (byte)AckEventType.DestroyActor,
            Parameters = new Dictionary<byte, object>()
            {
                {0, star.actorID}
            }
        };
        BroadcastEvent(evt, new SendParameters());

        taker.score++;

        if(listStar.Count == 0)
        {
            //the round is over
            Player winner = 
            (
                from p in listPlayer
                orderby taker.score descending
                select p
            ).First();

            EventData evnChat = new EventData()
            {
                Code = (byte)AckEventType.ChatMessage,
                Parameters = new Dictionary<byte, object>() 
                {
                    {0, string.Format("Player {0} wins the round with {1} stars!", ( winner.owner as StarCollectorPeer).playerID, winner.score)}
                }
            };
            BroadcastEvent(evnChat, new SendParameters());

            //reset round
            InitRound();
        }
    }
}
