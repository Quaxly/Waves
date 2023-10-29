using ArenaBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Waves.Features
{
    internal class RevivePlayers : ArenaGameBehavior
    {
        public RevivePlayers(ArenaGameSession gameSession) : base(gameSession)
        {
        }

        public void ReviveAll()
        {
            foreach (var player in gameSession.Players)
            {
                if (player == null)
                {
                    continue;
                }
                if (player.realizedCreature != null)
                {
                    Player realizedPlayer = player.realizedCreature as Player;
                    if (realizedPlayer != null)
                    {
                        if (realizedPlayer.dead)
                        {
                            realizedPlayer.playerState.alive = true;
                            realizedPlayer.dead = false;
                            realizedPlayer.killTag = null;
                            realizedPlayer.killTagCounter = 0;
                            player.abstractAI?.SetDestination(player.pos);
                        }
                    }
                }
            }
            gameSession.PlayHUDSound(SoundID.UI_Multiplayer_Player_Revive);
        }
    }
}
