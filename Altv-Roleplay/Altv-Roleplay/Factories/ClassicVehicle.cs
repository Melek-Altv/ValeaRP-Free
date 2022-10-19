using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using System;

namespace Altv_Roleplay.Factories
{
    public class ClassicVehicle : Vehicle
    {
        public int VehicleId { get; set; }
        public bool Trunkstate { get; set; } = false;
        public bool Hoodstate { get; set; } = false;
        public bool inWash { get; set; } = false;


        public ClassicVehicle(ICore server, IntPtr nativePointer, ushort id) : base(server, nativePointer, id)
        {
        }

        public ClassicVehicle(ICore server, uint model, Position position, Rotation rotation) : base(server, model, position, rotation)
        {
        }
    }
}
