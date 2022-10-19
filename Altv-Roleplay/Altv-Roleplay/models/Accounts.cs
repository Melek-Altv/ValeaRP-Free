using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Altv_Roleplay.models
{
    public partial class Accounts
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int playerid { get; set; }
        public string playerName { get; set; }
        public string Email { get; set; }
        public ulong socialClub { get; set; }
        public ulong hardwareId { get; set; }
        public string password { get; set; }
        public int Online { get; set; }
        public bool whitelisted { get; set; }
        public bool ban { get; set; }
        public string banReason { get; set; }
        public int adminLevel { get; set; }
        public int MaxCharacters { get; set; }

        #region Spam Schutz
        [NotMapped]
        public bool EinreiseisUsed { get; set; } = false;
        [NotMapped]
        public bool EinreiseIsNotifySended { get; set; } = false;
        [NotMapped]
        public DateTime Einreisedate { get; set; }
        #endregion
    }
}
