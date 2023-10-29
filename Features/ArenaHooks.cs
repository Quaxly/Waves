using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;

namespace Waves.Features
{
    internal class ArenaHooks
    {

        private static RainWorldGame rainWorldGame;

        public static void Apply()
        {

            IL.RainWorldGame.ctor += ILRainWorldGameCtor;

            On.RainWorldGame.ctor += OnRainWorldGameCtor;

            On.ArenaBehaviors.ExitManager.ExitsOpen += OnExitsOpen;
            // Debug

        }

        private static bool OnExitsOpen(On.ArenaBehaviors.ExitManager.orig_ExitsOpen orig, ArenaBehaviors.ExitManager self)
        {
            if (self.gameSession.GameTypeSetup.denEntryRule == Waves.DenEntryRule.Never)
            {
                return false;
            }
            return orig(self);
        }

        private static void OnRainWorldGameCtor(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
        {
            // Workaround till I can figure this shit out, might actually be fine this way but idk
            rainWorldGame = self;
            orig(self, manager);
        }

        private static void ILRainWorldGameCtor(ILContext il)
        {
            var c = new ILCursor(il);

            ILLabel postCheck = il.DefineLabel();

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(1),
                x => x.MatchLdfld(typeof(ProcessManager).GetField("arenaSitting")),
                x => x.MatchLdfld(typeof(ArenaSitting).GetField("gameTypeSetup")),
                x => x.MatchLdfld(typeof(ArenaSetup.GameTypeSetup).GetField("gameType")),
                x => x.MatchLdsfld(typeof(ArenaSetup.GameTypeID).GetField("Competitive")),
                x => x.MatchCall(typeof(ExtEnum<ArenaSetup.GameTypeID>).GetMethod("op_Equality")),
                x => x.MatchBrfalse(out postCheck)))
            {
                c.Emit(OpCodes.Ldarg_1);
                c.Emit(OpCodes.Ldarg_0);

                c.EmitDelegate<Action<ProcessManager, RainWorldGame>>((manager, game) =>
                {
                    if (manager.arenaSetup != null && manager.arenaSitting.gameTypeSetup.gameType == Waves.GameTypeID.WavesMode)
                    {
                        rainWorldGame.session = new WavesGameSession(rainWorldGame);
                    }
                });
            }
        }


        #region Debug

        #endregion
    }
}
