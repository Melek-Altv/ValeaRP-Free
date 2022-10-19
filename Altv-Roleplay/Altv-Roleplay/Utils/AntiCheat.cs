namespace Altv_Roleplay.Utils
{
    public class AntiCheat
    {
        public enum forbiddenWeapons : uint
        {
            CompactGrenadeLauncher = 125959754,
            VintagePistol = 137902532,
            HeavySniperMkII = 177293209,
            HeavySniper = 205991906,
            SweeperShotgun = 317205821,
            PipeWrench = 419712736,
            StickyBomb = 741814745,
            StoneHatchet = 940833800,
            AussaultRifleMkII = 961495388,
            Minigun = 1119849093,
            UnholyHellbringer = 1198256469,
            FlareGun = 1198879012,
            GrenadeLauncherSmoke = 1305664598,
            PumpShotgunMkII = 1432025498,
            HomingLauncher = 1672152130,
            MarksmanRifleMkII = 1785463520,
            Railgun = 1834241177,
            SMGMkII = 2024373456,
            CombatMG = 2144741730,
            Crowbar = 2227010557,
            BullpupRifleMkII = 2228681469,
            SNSPistolMkII = 2285322324,
            Flashlight = 2343591895,
            Grenade = 2481070269,
            SpecialCarbineMkII = 2526821735,
            DoubleActionRevolver = 2548703416,
            MG = 2634544996,
            BullpupShotgun = 2640438543,
            GrenadeLauncher = 2726580491,
            Musket = 2828843422,
            ProximityMines = 2874559379,
            UpnAtomizer = 2939590305,
            RPG = 2982836145,
            Widowmaker = 3056410471,
            PipeBombs = 3125143736,
            SNSPistol = 3218215474,
            HeavyRevolver = 3249783761,
            MarksmanRifle = 3342088282,
            HeavyRevolverMkII = 3415619887,
            MachinePistol = 3675956304,
            MarksmanPistol = 3696079510,
            AssaultShotgun = 3800352039,
            DoubleBarrelShotgun = 4019527611,
            BrokenBottle = 4192643659,
            CarbineRifleMkII = 4208062921,
        }
        public static class AnticheatConfig
        {
            public static bool teleport = false;
            public static bool autoheal = true;
            public static string anticheatName = "Anticheat";
            public static string version = "V.2.0";
            public const int TELEPORT_KICK_FOOT = 40;
            public const int TELEPORT_KICK_VEHICLE = 50;
            public const int TELEPORT_KICK_FLYVEHICLE = 20;
            public const bool death = true;
        }
    }
}