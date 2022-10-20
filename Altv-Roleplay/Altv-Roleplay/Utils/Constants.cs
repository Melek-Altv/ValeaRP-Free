using AltV.Net.Data;

namespace Altv_Roleplay.Utils
{
    public static class Constants
    {
        public static class DatabaseConfig
        {
            public static string Host = "localhost";
            public static string User = "root";
            public static string Password = "";
            public static string Port = "3306";
            public static string Database = "ValeaRP-a";
        }

        public static class Positions
        {

            //Staatlich
            public static readonly Position IdentityCardApply = new Position(-578.3077f,-217.16043f,38.159668f); //Personalausweis
            public static readonly Position TownhallHouseSelector = new Position(-584.04395f,-207.87692f,38.159668f); //Einwohnermeldeamt
            public static readonly Position Jobcenter_Position = new Position(-572.61096f,-199.38461f,38.159668f); //Jobcenter

            //Zulassungsstelle
            public static readonly Position VehicleLicensing_Position = new Position(337.54243f,-1562.6241f, 30f); //Zulassungsstelle
            public static readonly Position VehicleLicensing_VehPosition = new Position(345.15164f,-1562.6241f,28.4f); //Zulassungsstele Fzg Pos

            //Spawn
            public static readonly Position SpawnPos_Airport = new Position(-1128.5406f,-2783.2615f,27.695923f);
            public static readonly Rotation SpawnRot_Airport = new Rotation(0,0,149.295166015625f);

            public static readonly Position SpawnPos_Beach = new Position(-1483.6483f,-1484.611f,2.5897217f);
            public static readonly Rotation SpawnRot_Beach = new Rotation(0,0,1.3852693f);

            public static readonly Position SpawnPos_SandyShores = new Position(1533.5868f,3629.6177f,34.57068f);
            public static readonly Rotation SpawnRot_SandyShores = new Rotation(0,0,-0.54421294f);

            public static readonly Position SpawnPos_PaletoBay = new Position(-158.67693f,6390.8438f,31.470337f);
            public static readonly Rotation SpawnRot_PaletoBay = new Rotation(0,0,2.572643f);

            // MÜLLMANN
            public static readonly Position Minijob_Müllmann_StartPos = new Position(-617.0723266601562f,-1622.7850341796875f,33.010528564453125f);
            public static readonly Position Minijob_Müllmann_VehOutPos = new Position(-591.8637f,-1586.2814f,25.977295f);
            public static readonly Rotation Minijob_Müllmann_VehOutRot = new Rotation(0,0,1.453125f);

            // BUSFAHRER
            public static readonly Position Minijob_Busdriver_StartPos = new Position(454.12713623046875f,-600.075927734375f,28.578372955322266f);
            public static readonly Position Minijob_Busdriver_VehOutPos = new Position(466.33847f,-579.0725f,27.729614f);
            public static readonly Rotation Minijob_Busdriver_VehOutRot = new Rotation(0,0,3.046875f);

            //LKW Fahrer
            public static readonly Position Minijob_lkw_StartPos = new Position(94.6945f, -2676.356f, 5.993408f);
            public static readonly Position Minijob_lkw_VehOutPos = new Position(115.26594f, -2631.4944f, 6.128174f);
            public static readonly Rotation Minijob_lkw_VehOutRot = new Rotation(0.004653592f, -0.0024550834f, 2.9804506f);
            public static readonly Position Minijob_lkw_VehOutPos2 = new Position(102.87033f, -2634.7253f, 6.2630615f);
            public static readonly Rotation Minijob_lkw_VehOutRot2 = new Rotation(0.0007397744f, 0.0006381165f, 2.9943037f);

            //Hotel
            public static readonly Position Hotel_Apartment_ExitPos = new Position(266.08685302734375f,-1007.5635986328125f,-101.00853729248047f);
            public static readonly Position Hotel_Apartment_StoragePos = new Position(265.9728698730469f,-999.4517211914062f,-99.00858306884766f);

            //Knast
            public static readonly Position Arrest_Position = new Position(-559.411f,-132.75165f,33.744995f);

            //Schönheitschirurgie
            public static readonly Position Surgery = new Position(-459.178f, -326.12308f, 34.48645f);
            public static readonly Rotation Surgery_Rotation = new Position(0, 0, 0.0f);

            //Farming
            public static readonly Position ProcessTest = new Position(-252.05f,-971.736f,31.21f);

            //Schlüsseldienst
            public static readonly Position Schluesseldienst_Pos = new Position(168.47473f,-1797.178f,29.212402f);
            public static readonly Position Schluesseldienst_Ped = new Position(170.01758f,-1799.578f,29.313599f);
            public static readonly float Schluesseldienst_PedRot = -40.02634048461914f;

            //ACLS TuneVehPosition
            public static readonly Position AutoClubLosSantos_TuneVehPosition = new Position(-197.53847f,-1335.1912f,30.189697f); //TuneVehPosition +35f

            // Aservatenkammer
            public static readonly Position Aservatenkammer = new Position(-552.4352f,-118.87912f,33.744995f);

            //NameChange
            public static readonly Position NameChange = new Position(-516.9231f, -210.87033f, 38.159668f);
            public static readonly Rotation NameChange_Rot = new Position(0, 0, 122.80f);

            //Sell Vehicle
            public static readonly Position SellVehicle = new Position((float)-441.7846, (float)-1696.6022, 18);
            public static readonly Position SellVehicle2 = new Position((float)2424.699, (float)3129.9429, 48);

            //Einreise NPC
            public static Position EinreiseNPC = new Position(-1131.112f, -2823.402f, 27.443237f);
            public static Rotation EinreiseNPC_Rotation = new Position(0.0f, 0.0f, 0.0f);
        }
    }
}
