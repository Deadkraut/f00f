#region ::::Changelog::::
/*
 *GameOverMenuScreen.cs
 * 
 *
 * @author: Alexander Stoldt
 * @version: 1.0
 */
#endregion
#region Using Statements
using Microsoft.Xna.Framework;
using WeltraumSpiel.MenueManager; // Dient dazu das MenuEntry hier auch geladen wird
#endregion

namespace WeltraumSpiel
{
    class GameOverMenuScreen : MenuScreen
    {

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameOverMenuScreen()
            : base("GameOver")
        {
            // Create our menu entries.
            MenuEntry newGameMenuEntry = new MenuEntry("Neustarten");
            MenuEntry quitGameMenuEntry = new MenuEntry("Beenden");

            // Hook up menu event handlers.
            newGameMenuEntry.Selected += newGameMenuEntrySelected;
            quitGameMenuEntry.Selected += QuitGameMenuEntrySelected;

            // Add entries to the menu.
            MenuEntries.Add(newGameMenuEntry);
            MenuEntries.Add(quitGameMenuEntry);
        }
        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Quit Game menu entry is selected.
        /// </summary>
        void QuitGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            const string message = "Bist du sicher das du das Spiel beendet willst?";

            MessageBoxScreen confirmQuitMessageBox = new MessageBoxScreen(message);

            confirmQuitMessageBox.Accepted += ConfirmQuitMessageBoxAccepted;

            ScreenManager.AddScreen(confirmQuitMessageBox, ControllingPlayer);
        }


        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to quit" message box. This uses the loading screen to
        /// transition from the game back to the main menu screen.
        /// </summary>
        void ConfirmQuitMessageBoxAccepted(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(),
                                                           new MainMenuScreen());
        }

        void newGameMenuEntrySelected(object sender, PlayerIndexEventArgs e)
        {
            const string message = "Bist du sicher das du nochmal Spielen willst?";

            MessageBoxScreen newGameMessageBox = new MessageBoxScreen(message);
            newGameMessageBox.Accepted += NewGameMessageBox;
            ScreenManager.AddScreen(newGameMessageBox, ControllingPlayer);
        }

        void NewGameMessageBox(object sender, PlayerIndexEventArgs e)
        {
            LoadingScreen.Load(ScreenManager, true, e.PlayerIndex,
                              new GameplayScreen());
        }


        #endregion
    }
}
