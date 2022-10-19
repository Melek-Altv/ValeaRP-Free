using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Altv_Roleplay.models
{
    public partial class Server_Doors
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        public string name { get; set; }
        public float posX { get; set; }
        public float posY { get; set; }
        public float posZ { get; set; }
        public ulong hash { get; set; }
        public bool state { get; set; }
        public string doorKey { get; set; }
        public string doorKey2 { get; set; }
        public string type { get; set; }
        public float lockPosX { get; set; }
        public float lockPosY { get; set; }
        public float lockPosZ { get; set; }
        public string pairs { get; set; }
    }
}