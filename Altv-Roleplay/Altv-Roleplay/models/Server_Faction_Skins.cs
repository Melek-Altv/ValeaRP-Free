using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Altv_Roleplay.models
{
    public partial class Server_faction_skins
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public int faction { get; set; }
        public int gender { get; set; }
        public string clothesName { get; set; }
        public int rank { get; set; }
        public int torso { get; set; }
        public int hat { get; set; }
        public int hattex { get; set; }
        public int glasses { get; set; }
        public int glassestex { get; set; }
        public int top { get; set; }
        public int toptex { get; set; }
        public int undershirt { get; set; }
        public int undershirttex { get; set; }
        public int leg { get; set; }
        public int legtex { get; set; }
        public int shose { get; set; }
        public int shosetex { get; set; }
        public int decal { get; set; }
        public int decaltex { get; set; }
    }
}