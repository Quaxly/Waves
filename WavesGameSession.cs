using IL.MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waves.Features;

namespace Waves
{
    public class WavesGameSession : ArenaGameSession
    {
        public int round;
        public List<Creature> remaining;

        private WavesBehavior wavesBehavior;
        private RevivePlayers revivePlayers;
        private List<CreatureTemplate.Type> upcoming;

        public WavesGameSession(RainWorldGame game) : base(game)
        {
            round = 0;
            upcoming = new List<CreatureTemplate.Type>();
            AddBehavior(new ArenaBehaviors.RespawnFlies(this));
            wavesBehavior = new WavesBehavior(this);
            AddBehavior(wavesBehavior);
            revivePlayers = new RevivePlayers(this);
            AddBehavior(revivePlayers);
        }

        public override void Initiate()
        {
            base.SpawnPlayers(base.room, null);
            base.Initiate();
            base.AddHUD();
            arenaSitting.gameTypeSetup.denEntryRule = Waves.DenEntryRule.Never;
        }

        public override bool ShouldSessionEnd()
        {
            if (this.arenaSitting.players.Count == 0)
            {
                return this.game.world.rainCycle.TimeUntilRain < -200;
            }
            return (this.initiated && this.thisFrameActivePlayers == 0);
        }

        public void NextRound()
        {
            //this.PlayHUDSound(SoundID.MENU_Passage_Button);
            revivePlayers.ReviveAll();
            upcoming.Clear();
            for (int i = 0; i < (round + 1); i++)
            {
                upcoming.Add(Pool(i));
            }
            //upcoming.Add(CreatureTemplate.Type.BigNeedleWorm);
            foreach (AbstractCreature creature in room.abstractRoom.creatures)
            {
                Creature realized = creature.realizedCreature;
                if (realized != null && realized.dead && (!(realized is Player || realized is Fly)))
                {
                    realized.RemoveFromRoom();
                    creature.slatedForDeletion = true;
                }
            }
            remaining = WavesSpawner.SpawnWaveCreatures(game, ref upcoming, ref arenaSitting.multiplayerUnlocks, round == 0 ? true : false);
            round++;
        }

        public void OnCreatureDeath(Creature creature)
        {
            if (!remaining.Contains(creature))
            {
                return;
            }
            remaining.Remove(creature);
            wavesBehavior.MarkForDissolve(creature);
            wavesBehavior.ExtendCycle(100);
            wavesBehavior.TryEndRound();
        }

        public CreatureTemplate.Type Pool(int seed)
        {
            System.Random random = new System.Random(seed); // Using System.Random to set the seed
            int i = random.Next(0, 10);
            switch (i)
            {
                default: return CreatureTemplate.Type.PinkLizard;
                case 1: case 2: case 3: return CreatureTemplate.Type.GreenLizard;
                case 5: case 6: return CreatureTemplate.Type.BlueLizard;
                case 10: return CreatureTemplate.Type.CyanLizard;
            }
        }
    }
}
