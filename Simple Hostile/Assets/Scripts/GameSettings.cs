using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Kawaiisun.SimpleHostile
{
    public enum GameMode
    {
        FFA = 0,
        TDM = 1
    }

    public class GameSettings : MonoBehaviour
    {
        public static GameMode GameMode = GameMode.FFA;
        public static bool IsAwayTeam = false;
    }
}