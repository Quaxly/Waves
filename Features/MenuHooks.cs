using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using UnityEngine;
using Menu;
using System.Text.RegularExpressions;

namespace Waves.Features
{
    internal static class MenuHooks
    {

        public static void Apply()
        {
            IL.Menu.MultiplayerMenu.InitiateGameTypeSpecificButtons += ILGameTypeSpecific;
            //IL.Menu.MultiplayerMenu.Update += ILUpdate;

            On.Menu.MultiplayerMenu.InitiateGameTypeSpecificButtons += OnGameTypeSpecific;
            On.Menu.MultiplayerMenu.ClearGameTypeSpecificButtons += OnClearGameType;
            On.Menu.MultiplayerMenu.Singal += OnSignal;
            On.Menu.MultiplayerMenu.Update += OnUpdate;

            On.Menu.MultiplayerResults.ctor += OnMultiplayerResults;

            On.Menu.ArenaSettingsInterface.ctor += OnArenaSettingsInterface;

            On.Menu.InfoWindow.ctor += OnInfoWindow;

            On.Menu.FinalResultbox.ctor += OnFinalResults;
        }

        private static void OnClearGameType(On.Menu.MultiplayerMenu.orig_ClearGameTypeSpecificButtons orig, MultiplayerMenu self)
        {
            orig(self);
        }

        private static void OnInfoWindow(On.Menu.InfoWindow.orig_ctor orig, InfoWindow self, Menu.Menu menu, MenuObject owner, Vector2 pos)
        {
            orig(self, menu, owner, pos);
            string text = "";
            if ((menu as MultiplayerMenu).GetGameTypeSetup.gameType == Waves.GameTypeID.WavesMode)
            {
                text = Regex.Replace(menu.Translate("Fight off waves of creatures that<LINE>progressively gets more difficult and survive as long as you can.<LINE>Grabbing batflies will turn them into spears.<LINE>You can also revive dead players and everyone gets revived at the end<LINE>of the round as long as one player is alive.<LINE>This mode is very challenging solo so a team is recommended."), "<LINE>", "\r\n");
                string[] array = Regex.Split(text, "\r\n");
                int num = 0;
                for (int i = 0; i < array.Length; i++)
                {
                    num = Math.Max(num, array[i].Length);
                }
                if (InGameTranslator.LanguageID.UsesLargeFont(menu.CurrLang))
                {
                    num *= 2;
                }
                self.goalSize = new Vector2((float)num * 10.5f, (float)array.Length * 30f) + new Vector2(20f, 20f);
                self.label = new MenuLabel(menu, self, text, new Vector2(20f, 20f), self.goalSize, true, null);
                self.label.label.alignment = FLabelAlignment.Left;
                self.subObjects.Add(self.label);
            }
        }

        private static void OnArenaSettingsInterface(On.Menu.ArenaSettingsInterface.orig_ctor orig, Menu.ArenaSettingsInterface self, Menu.Menu menu, Menu.MenuObject owner)
        {
            orig(self, menu, owner);
            Vector2 a = new Vector2(826.01f, 140.01f);
            float num = 340f;
            bool flag = menu.CurrLang != InGameTranslator.LanguageID.English && menu.CurrLang != InGameTranslator.LanguageID.Korean && menu.CurrLang != InGameTranslator.LanguageID.Chinese;
            if (self.GetGameTypeSetup.gameType == Waves.GameTypeID.WavesMode)
            {
                self.spearsHitCheckbox = new CheckBox(menu, self, self, a + new Vector2(0f, 220f), 120f, menu.Translate("Spears Hit:"), "SPEARSHIT", false);
                self.subObjects.Add(self.spearsHitCheckbox);
                self.evilAICheckBox = new CheckBox(menu, self, self, a + new Vector2(num - 24f, 220f), InGameTranslator.LanguageID.UsesLargeFont(menu.CurrLang) ? 140f : 120f, menu.Translate("Aggressive AI:"), "EVILAI", false);
                self.subObjects.Add(self.evilAICheckBox);

                self.rainTimer = new MultipleChoiceArray(menu, self, self, a + new Vector2(0f, 100f), menu.Translate("Rain Timer:"), "SESSIONLENGTH", InGameTranslator.LanguageID.UsesLargeFont(menu.CurrLang) ? 125f : 120f, num, 6, false, menu.CurrLang == InGameTranslator.LanguageID.French || menu.CurrLang == InGameTranslator.LanguageID.Spanish || menu.CurrLang == InGameTranslator.LanguageID.Portuguese);
                self.subObjects.Add(self.rainTimer);
            }
        }

        private static void OnFinalResults(On.Menu.FinalResultbox.orig_ctor orig, Menu.FinalResultbox self, Menu.MultiplayerResults resultPage, Menu.MenuObject owner, ArenaSitting.ArenaPlayer player, int index)
        {
            orig(self, resultPage, owner, player, index);
        }

        private static void OnMultiplayerResults(On.Menu.MultiplayerResults.orig_ctor orig, Menu.MultiplayerResults self, ProcessManager manager)
        {
            orig(self, manager);
            if (self.ArenaSitting.gameTypeSetup.gameType == Waves.GameTypeID.WavesMode)
            {
                self.headingLabel.text = self.Translate("YOU MADE IT TO WAVE " + Waves.lastRound);
            }
        }

        private static void OnUpdate(On.Menu.MultiplayerMenu.orig_Update orig, Menu.MultiplayerMenu self)
        {
            orig(self);
            if (self.currentGameType == Waves.GameTypeID.WavesMode)
            {
                self.APBLPulse = RWCustom.Custom.LerpAndTick(self.APBLPulse, 0f, 0.04f, 0.025f);
                self.playButton.buttonBehav.greyedOut = false;
            }
        }

        private static void OnSignal(On.Menu.MultiplayerMenu.orig_Singal orig, Menu.MultiplayerMenu self, Menu.MenuObject sender, string message)
        {
            orig(self, sender, message);

            // IL hooks were acting weird

            if (ModManager.MSC && self.currentGameType == Waves.GameTypeID.WavesMode)
            {
                for (int i = 0; i < self.playerClassButtons.Length; i++)
                {
                    if (message == "CLASSCHANGE" + i.ToString())
                    {
                        self.GetArenaSetup.playerClass[i] = self.NextClass(self.GetArenaSetup.playerClass[i]);
                        self.playerClassButtons[i].menuLabel.text = self.Translate(SlugcatStats.getSlugcatName(self.GetArenaSetup.playerClass[i]));
                        self.playerJoinButtons[i].portrait.fileName = self.ArenaImage(self.GetArenaSetup.playerClass[i], i);
                        self.playerJoinButtons[i].portrait.LoadFile();
                        self.playerJoinButtons[i].portrait.sprite.SetElementByName(self.playerJoinButtons[i].portrait.fileName);
                        self.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
                    }
                }
            }
        }

        private static void ILSignal(ILContext il)
        {
            var c = new ILCursor(il);

            ILLabel postCheck = null;

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchCall(typeof(Menu.MultiplayerMenu).GetMethod("get_currentGameType")),
                x => x.MatchLdsfld(typeof(ArenaSetup.GameTypeID).GetField("Competitive")),
                x => x.MatchCall(typeof(ExtEnum<ArenaSetup.GameTypeID>).GetMethod("op_Equality")),
                x => x.MatchBrtrue(out postCheck)))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<Menu.MultiplayerMenu, bool>>((menu) =>
                {
                    return menu.currentGameType == ArenaSetup.GameTypeID.Sandbox ||
                    menu.currentGameType == Waves.GameTypeID.WavesMode;
                });
                c.Emit(OpCodes.Brfalse, postCheck);
            }

        }

        private static void ILUpdate(ILContext il)
        {
            var c = new ILCursor(il);

            ILLabel postCheck = null;

            if (c.TryGotoNext(MoveType.Before,
                x => x.MatchLdarg(0),
                x => x.MatchCall(typeof(Menu.MultiplayerMenu).GetMethod("get_currentGameType")),
                x => x.MatchLdsfld(typeof(ArenaSetup.GameTypeID).GetField("Sandbox")),
                x => x.MatchCall(typeof(ExtEnum<ArenaSetup.GameTypeID>).GetMethod("op_Equality")),
                x => x.MatchBrfalse(out postCheck)))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<Menu.MultiplayerMenu, bool>>((menu) =>
                {
                    return menu.currentGameType == ArenaSetup.GameTypeID.Sandbox || menu.currentGameType == Waves.GameTypeID.WavesMode;
                });
                c.Emit(OpCodes.Brfalse, postCheck);
            }
        }

        private static void OnGameTypeSpecific(On.Menu.MultiplayerMenu.orig_InitiateGameTypeSpecificButtons orig, Menu.MultiplayerMenu self)
        {
            orig(self);

            // Covers everything that was skipped over

            if (self.currentGameType == Waves.GameTypeID.WavesMode)
            {
                // Level Selector

                if (self.levelSelector != null)
                {
                    self.levelSelector.RemoveSprites();
                    self.pages[0].subObjects.Remove(self.levelSelector);
                }
                self.levelSelector = new Menu.LevelSelector(self, self.pages[0], self.currentGameType == Waves.GameTypeID.WavesMode);
                self.pages[0].subObjects.Add(self.levelSelector);

                // Illustrations

                self.scene.AddIllustration(new Menu.MenuIllustration(self, self.scene, string.Empty, "WavesShadow", new Vector2(-2.99f, 265.01f), true, false));
                self.scene.AddIllustration(new Menu.MenuIllustration(self, self.scene, string.Empty, "WavesTitle", new Vector2(-2.99f, 265.01f), true, false));
                self.scene.flatIllustrations[self.scene.flatIllustrations.Count - 1].sprite.shader = self.manager.rainWorld.Shaders["MenuText"];

                // First Button Bindings

                //self.MutualVerticalButtonBind(self.playButton, self.arenaSettingsInterface.wildlifeArray.buttons[self.arenaSettingsInterface.wildlifeArray.buttons.Length - 1]);

                // Last Button Bindings

                self.MutualVerticalButtonBind(self.backButton, self.levelSelector.allLevelsList.scrollDownButton);
                for (int num19 = 0; num19 < self.playerJoinButtons.Length; num19++)
                {
                    self.MutualVerticalButtonBind(self.playerJoinButtons[num19], self.infoButton);
                }
                self.MutualVerticalButtonBind(self.levelSelector.allLevelsList.scrollUpButton, self.prevButton);
                self.arenaSettingsInterface = new Menu.ArenaSettingsInterface(self, self.pages[0]);
                self.pages[0].subObjects.Add(self.arenaSettingsInterface);
                /*
                if (ModManager.MSC)
                {
                    self.MutualVerticalButtonBind(self.arenaSettingsInterface.evilAICheckBox, self.playerClassButtons[3]);
                }
                else
                {
                    self.MutualVerticalButtonBind(self.arenaSettingsInterface.evilAICheckBox, self.playerJoinButtons[3]);
                }*/
            }
        }

        private static void ILGameTypeSpecific(ILContext il)
        {
            var c = new ILCursor(il);

            ILLabel postCheck = null;

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchCall(typeof(Menu.MultiplayerMenu).GetMethod("get_currentGameType")),
                x => x.MatchLdsfld(typeof(ArenaSetup.GameTypeID).GetField("Sandbox")),
                x => x.MatchCall(typeof(ExtEnum<ArenaSetup.GameTypeID>).GetMethod("op_Equality")),
                x => x.MatchBrtrue(out postCheck)))

            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<Menu.MultiplayerMenu, bool>>((menu) =>
                {
                    Waves.Logger.LogDebug("Check menu: " + menu.ToString());
                    Waves.Logger.LogDebug("Check gameType: " + menu.currentGameType.ToString());
                    return menu.currentGameType == ArenaSetup.GameTypeID.Sandbox ||
                    menu.currentGameType == ArenaSetup.GameTypeID.Competitive ||
                    menu.currentGameType == Waves.GameTypeID.WavesMode;
                });
                c.Emit(OpCodes.Brtrue, postCheck);
            }
            else
            {
                Waves.Logger.LogFatal("Fatal error while attempting IL hook: ILGameTypeSpecific");
            }
        }

    }
}
