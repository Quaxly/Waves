using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waves.Features
{
    internal class CreatureHooks
    {
        public static void Apply()
        {
            On.Creature.Die += OnDie;

            On.Player.CanMaulCreature += OnCanMaulCreature;
            On.Player.CanEatMeat += OnCanEatMeat;
        }

        private static bool OnCanEatMeat(On.Player.orig_CanEatMeat orig, Player self, Creature crit)
        {
            if (self.room.game.session is WavesGameSession session)
            {
                Player player = crit as Player;
                if (player != null)
                {
                    return false;
                }
            }
            return orig(self, crit);
        }

        private static bool OnCanMaulCreature(On.Player.orig_CanMaulCreature orig, Player self, Creature crit)
        {
            bool flag = true;
            if (self.room.game.session is WavesGameSession session)
            {
                Player player = crit as Player;
                if (player != null && (player.isNPC || !session.GameTypeSetup.spearsHitPlayers))
                {
                    return false;
                }
            }
            return orig(self, crit);
        }

        private static void OnDie(On.Creature.orig_Die orig, Creature self)
        {
            orig(self);
            if (self.room.game.session is WavesGameSession)
            {
                WavesGameSession session = self.room.game.session as WavesGameSession;
                if (session != null)
                {
                    session.OnCreatureDeath(self);
                }
            }
        }
    }
}
