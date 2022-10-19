using AltV.Net;
using Altv_Roleplay.models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Altv_Roleplay.Model
{
    class ServerAllVehicles
    {
        public static List<Server_All_Vehicles> ServerAllVehicles_ = new List<Server_All_Vehicles>();

        public static int GetVehicleTaxes(long hash)
        {
            int tax = 0;
            if (hash <= 0) return tax;
            var veh = ServerAllVehicles_.FirstOrDefault(x => x.hash == hash);
            if(veh != null)
            {
                tax = veh.tax;
            }
            return tax;
        }

        public static int GetVehicleMaxFuel(long hash)
        {
            int maxFuel = 0;
            if (hash <= 0) return maxFuel;
            var veh = ServerAllVehicles_.FirstOrDefault(x => x.hash == hash);
            if(veh != null)
            {
                maxFuel = veh.maxFuel;
            }
            return maxFuel;
        }

        public static string GetVehicleNameOnHash(long hash)
        {
            string vehName = "undefined";
            if (hash == 0) return vehName;
            var vehs = ServerAllVehicles_.FirstOrDefault(x => x.hash == hash);
            if(vehs != null)
            {
                vehName = vehs.name;
            }
            return vehName;
        }

        public static int GetVehicleClass(long hash)
        {
            try
            {
                if (hash <= 0) return 0;
                var vehs = ServerAllVehicles_.FirstOrDefault(x => x.hash == hash);
                if(vehs != null)
                {
                    return vehs.vehClass;
                }
            }
            catch (Exception e)
            {
                Alt.Log($"{e}");
            }
            return 0;
        }

        public static bool ExistVehicleName(string vehicleName)
        {
            var vehicle = ServerAllVehicles_.FirstOrDefault(p => p.name == vehicleName);

            if (vehicle != null)
            {
                return true;
            }
            return false;
        }

        public static long GetVehicleHashOnName(string vehicleName)
        {
            var vehicle = ServerAllVehicles_.FirstOrDefault(p => p.name == vehicleName);

            if (vehicle != null)
            {
                return vehicle.hash;
            }
            return 0;
        }

        public static string GetVehicleManufactur(long hash)
        {
            var vehicle = ServerAllVehicles_.FirstOrDefault(p => p.hash == hash);

            if (vehicle != null)
            {
                return vehicle.manufactor;
            }
            return "";
        }

        public static string GetVehicleCategory(long hash)
        {
            var vehicle = ServerAllVehicles_.FirstOrDefault(p => p.hash == hash);

            if (vehicle != null)
            {
                return vehicle.category;
            }
            return "";
        }

        public static int GetVehicleTrunkCapacity(long hash)
        {
            var vehicle = ServerAllVehicles_.FirstOrDefault(p => p.hash == hash);

            if (vehicle != null)
            {
                return vehicle.trunkCapacity;
            }
            return 0;
        }

        public static string GetVehicleFuelType(long hash)
        {
            var vehicle = ServerAllVehicles_.FirstOrDefault(p => p.hash == hash);

            if (vehicle != null)
            {
                return vehicle.fuelType;
            }
            return "";
        }

        public static int GetVehicleSeats(long hash)
        {
            var vehicle = ServerAllVehicles_.FirstOrDefault(p => p.hash == hash);

            if (vehicle != null)
            {
                return vehicle.seats;
            }
            return 0;
        }

        public static int GetVehiclePrice(long hash)
        {
            var vehicle = ServerAllVehicles_.FirstOrDefault(p => p.hash == hash);

            if (vehicle != null)
            {
                return vehicle.price;
            }
            return 0;
        }
    }
}
