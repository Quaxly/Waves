using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using Rewired.UI.ControlMapper;

namespace Waves.Features
{
    internal static class WavesSpawner
    {

        private static List<WorldCoordinate> Dens;

        public static List<Creature> SpawnWaveCreatures(RainWorldGame game, ref List<CreatureTemplate.Type> types, ref MultiplayerUnlocks unlocks, bool scanForDens)
        {
            List<Creature> list = new List<Creature>();
            if (types.Count == 0)
            {
                return list;
            }

            AbstractRoom abstractRoom = game.world.GetAbstractRoom(0);
            if (scanForDens || Dens == null)
            {
                Dens = new List<WorldCoordinate>();
                foreach (ShortcutData shortcutData in abstractRoom.realizedRoom.shortcuts)
                {
                    if (shortcutData.shortCutType == ShortcutData.Type.CreatureHole)
                    {
                        Dens.Add(shortcutData.startCoord); // Creatures will be randomized from any den in the room
                    }
                }
            }

            foreach (var type in types)
            {
                if (StaticWorld.GetCreatureTemplate(type).quantified)
                {
                    abstractRoom.AddQuantifiedCreature(-1, type, UnityEngine.Random.Range(7, 11));
                }
                else
                {
                    if (Dens.Any())
                    {
                        int index = UnityEngine.Random.Range(0, Dens.Count);
                        AbstractCreature abstractCreature = CreateAbstractCreature(game.world, type, Dens[index]);
                        abstractCreature.RealizeInRoom();
                        list.Add(abstractCreature.realizedCreature);
                    }
                    else
                    {
                        Debug.Log("No dens were found");
                    }

                }
            }
            return list;
        }
        private static AbstractCreature CreateAbstractCreature(World world, CreatureTemplate.Type critType, WorldCoordinate pos)
        {
            return new AbstractCreature(world, StaticWorld.GetCreatureTemplate(critType), null, pos, world.game.GetNewID());
        }
    }
}

