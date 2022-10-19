using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Altv_Roleplay.models
{
    public partial class Server_Clothes_Shops_Items
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        public int shopId { get; set; }
        public string clothesName { get; set; }
        public int itemPrice { get; set; }

        [NotMapped]
        public int gender { get; set; }
        [NotMapped]
        public int tex { get; set; }
        [NotMapped]
        public int draw { get; set; }
        [NotMapped]
        public string type { get; set; }
    }
}