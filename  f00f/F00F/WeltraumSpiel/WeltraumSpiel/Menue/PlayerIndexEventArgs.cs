﻿#region ::::Chancelog::::
/*
 * PlayerIndexEventArgs.cs
 * 
 *
 * @author: Alexander Stoldt
 * @version: 1.0
 */
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
#endregion

namespace WeltraumSpiel
{
    /// <summary>
    /// Custom event argument which includes the index of the player who
    /// triggered the event. This is used by the MenuEntry.Selected event.
    /// </summary>
    class PlayerIndexEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public PlayerIndexEventArgs(PlayerIndex playerIndex)
        {
            this.playerIndex = playerIndex;
        }


        /// <summary>
        /// Gets the index of the player who triggered this event.
        /// </summary>
        public PlayerIndex PlayerIndex
        {
            get { return playerIndex; }
        }

        PlayerIndex playerIndex;
    }
}