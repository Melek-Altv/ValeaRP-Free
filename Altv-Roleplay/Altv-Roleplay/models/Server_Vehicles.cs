﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Altv_Roleplay.models
{
    public partial class Server_Vehicles
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }

        public int charid { get; set; }
        public long hash { get; set; }
        public int vehType { get; set; }
        public int faction { get; set; }
        public float fuel { get; set; }
        public float KM { get; set; }
        public bool engineState { get; set; }
        public bool isEngineHealthy { get; set; }
        public bool lockState { get; set; }
        public bool isInGarage { get; set; }
        public int garageId { get; set; }
        public float posX { get; set; }
        public float posY { get; set; }
        public float posZ { get; set; }
        public float rotX { get; set; }
        public float rotY { get; set; }
        public float rotZ { get; set; }
        public string plate { get; set; }
        public DateTime lastUsage { get; set; }
        public DateTime buyDate { get; set; }
        public bool isInHouseGarage { get; set; }
    }
}