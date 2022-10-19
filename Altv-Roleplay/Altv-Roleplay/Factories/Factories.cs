using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using System;

namespace Altv_Roleplay.Factories
{
    public class VehicleFactory : IEntityFactory<IVehicle>
    {
        public IVehicle Create(ICore server, uint model, Position position, Rotation rotation)
        {
            return new ClassicVehicle(server, model, position, rotation);
        }

        public IVehicle Create(ICore server, IntPtr nativePointer, ushort id)
        {
            return new ClassicVehicle(server, nativePointer, id);
        }
    }

    class AccountsFactory : IEntityFactory<IPlayer>
    {
        public IPlayer Create(ICore server, IntPtr playerPointer, ushort id)
        {
            return new ClassicPlayer(server, playerPointer, id);
        }
    }

    public class ColshapeFactory : IBaseObjectFactory<IColShape>
    {
        public IColShape Create(ICore server, IntPtr entityPointer)
        {
            return new ClassicColshape(server, entityPointer);
        }
    }
}
