using ArenaBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;

namespace Waves
{
    public class WavesBehavior : ArenaGameBehavior
    {
        public int waveCounter = 380;
        public bool ended = true;

        private WavesGameSession session;
        private int spawnTimer = 500;
        private Dictionary<Creature, int> markedForDissolve = new Dictionary<Creature, int>();
        private int cycleExtension = 0;
        public WavesBehavior(ArenaGameSession gameSession) : base(gameSession)
        {
            if (gameSession is WavesGameSession)
            {
                session = (WavesGameSession)gameSession;
            } else
            {
                this.Destroy();
            }
        }

        public override void Update()
        {
            foreach (AbstractCreature abstractCreature in gameSession.Players)
            {
                if (abstractCreature != null && abstractCreature.realizedCreature != null && abstractCreature.realizedCreature is Player) 
                {
                    Player player = abstractCreature.realizedCreature as Player;
                    for (int i = 0; i < player.grasps.Length; i++)
                    {
                        if (player.grasps[i] != null)
                        {
                            if (player.grasps[i].grabbed is Fly)
                            {
                                Fly fly = player.grasps[0].grabbed as Fly;
                                if (fly != null)
                                {
                                    player.ReleaseGrasp(i);
                                    fly.Die();
                                    player.ObjectEaten(fly);
                                    fly.RemoveFromRoom();
                                    AbstractSpear abstractSpear = new AbstractSpear(this.room.world, null, player.abstractCreature.pos, this.room.game.GetNewID(), false);
                                    room.abstractRoom.AddEntity(abstractSpear);
                                    abstractSpear.RealizeInRoom();
                                    room.PlaySound(SoundID.Fly_Caught, player.firstChunk.pos, 3, 1);
                                    room.PlaySound(SoundID.Slugcat_Pick_Up_Spear, player.firstChunk.pos, 8, 1);
                                    if (player.FreeHand() != -1)
                                    {
                                        player.SlugcatGrab(abstractSpear.realizedObject, player.FreeHand());
                                    }
                                    
                                }
                            }
                        }
                    }
                }
            }
            if (cycleExtension > 0)
            {
                game.world.rainCycle.cycleLength += 2;
                cycleExtension--;
            }
            if (this.gameSession.counter < 20)
            {
                return;
            }
            if (base.room.ReadyForPlayer && waveCounter > 0 && ended)
            {
                this.waveCounter--;
            }
            if (base.room.readyForAI && spawnTimer > 0 && !ended)
            {
                spawnTimer--;
            }

            if (markedForDissolve.Keys.Any())
            {
                foreach (Creature creature1 in new List<Creature>(markedForDissolve.Keys))
                {
                    if (creature1 == null || creature1.slatedForDeletetion)
                    {
                        markedForDissolve.Remove(creature1);
                        continue;
                    }
                    markedForDissolve[creature1]++;
                    if (markedForDissolve[creature1] > 880)
                    {
                        foreach (var bodychunk in creature1.bodyChunks)
                        {
                            //Bubble bubble = new Bubble(bodychunk.pos, Custom.RNV() * UnityEngine.Random.value * 2, false, false);
                            //bubble.mode = Bubble.Mode.Growing;
                            //bubble.growthSpeed = 0.04f;
                            //room.AddObject(bubble);
                            room.AddObject(new Spark(bodychunk.pos, Custom.RNV() * UnityEngine.Random.value * 10f, new Color(255, 116, 56), null, 30, 120));
                        }
                    }
                    if (markedForDissolve[creature1] > 900)
                    {
                        markedForDissolve.Remove(creature1);
                        creature1.Destroy();
                    }
                }
            }

            if (waveCounter == 0)
            {
                this.gameSession.PlayHUDSound(SoundID.HUD_Karma_Reinforce_Contract);
                session.NextRound();
                base.game.cameras[0].hud.textPrompt.AddMessage("Wave: " + session.round, 20, 100, true, false);
                Debug.Log("Started next wave: " + session.round);
                spawnTimer = 500;
                ended = false;
                waveCounter = 280;
            }
        }

        public void TryEndRound()
        {
            if (!session.remaining.Any() && !ended && spawnTimer <= 0)
            {
                waveCounter = 380;
                ended = true;
                gameSession.PlayHUDSound(SoundID.MENU_Endgame_Meter_Fullfilled);
                ExtendCycle(600);
                //game.world.rainCycle.cycleLength += 1000;
                //base.game.cameras[0].hud.textPrompt.AddMessage(Waves.EndOfWaveMessage(session.round), 20, 100, false, false);
            }
        }

        public void MarkForDissolve(Creature creature)
        {
            if (!markedForDissolve.ContainsKey(creature) && creature.dead)
            {
                markedForDissolve.Add(creature, 0);
            }
        }

        public void ExtendCycle(int amount)
        {
            cycleExtension += amount;
        }
    }
}
