using System;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Logging;
using Waves.Features;

#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace Waves;

[BepInPlugin("waves", "Waves", "1.0.0")]
public class Waves : BaseUnityPlugin
{
    new internal static ManualLogSource Logger;

    private WavesOptions Options;

    public static int lastRound;
    public static int topRound;

    public Waves()
    {
        try
        {
            Logger = base.Logger;
            Options = new WavesOptions(this, Logger);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }
    private void OnEnable()
    {
        On.RainWorld.OnModsInit += RainWorldOnOnModsInit;
    }

    private bool IsInit;
    private void RainWorldOnOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        try
        {
            if (IsInit) return;

            RegisterEnums();

            On.RainWorldGame.ShutDownProcess += RainWorldGameOnShutDownProcess;
            On.GameSession.ctor += GameSessionOnctor;

            MenuHooks.Apply();
            ArenaHooks.Apply();
            CreatureHooks.Apply();

            MachineConnector.SetRegisteredOI("waves", Options);
            IsInit = true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }

    private void RegisterEnums()
    {
        Waves.GameTypeID.RegisterValues();
    }

    private void RainWorldGameOnShutDownProcess(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self)
    {
        orig(self);
        ClearMemory();
    }
    private void GameSessionOnctor(On.GameSession.orig_ctor orig, GameSession self, RainWorldGame game)
    {
        orig(self, game);
        ClearMemory();
    }

    #region Quick Injects

    #endregion

    #region Helper Methods

    private void ClearMemory()
    {
        //If you have any collections (lists, dictionaries, etc.)
        //Clear them here to prevent a memory leak
        //YourList.Clear();
    }

    static bool WavesModeComparison()
    {
        var rw = UnityEngine.Object.FindObjectOfType<RainWorld>();
        return rw.processManager.arenaSetup.currentGameType == Waves.GameTypeID.WavesMode;
    }

    static Func<bool> CreateWavesModeComparisonDelegate()
    {
        return new Func<bool>(WavesModeComparison);
    }

    internal static string EndOfWaveMessage(int round)
    {
        return ""; // TODO, implement this
    }

    #endregion

    #region Enums

    public class GameTypeID
    {
        public static void RegisterValues()
        {
            Waves.GameTypeID.WavesMode = new ArenaSetup.GameTypeID("Waves", true);
        }

        public static void UnregisterValues()
        {
            if (Waves.GameTypeID.WavesMode != null)
            {
                Waves.GameTypeID.WavesMode.Unregister();
            }
        }

        public static ArenaSetup.GameTypeID WavesMode;

    }

    public class DenEntryRule
    {
        public static void RegisterValues()
        {
            Waves.DenEntryRule.Never = new ArenaSetup.GameTypeSetup.DenEntryRule("Waves", true);
        }

        public static void UnregisterValues()
        {
            if (Waves.DenEntryRule.Never != null)
            {
                Waves.DenEntryRule.Never.Unregister();
            }
        }

        public static ArenaSetup.GameTypeSetup.DenEntryRule Never;

    }

    #endregion
}
