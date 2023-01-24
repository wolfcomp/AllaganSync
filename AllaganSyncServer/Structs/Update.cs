using System.Runtime.InteropServices;

namespace AllaganSyncServer.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 0x14)]
    [MessageStructType(StructTypes.LastUpdate)]
    public struct LastUpdate
    {
        [FieldOffset(0x0)] public long Id;
        [FieldOffset(0x8)] public long LastTime;
        [FieldOffset(0x10)] public uint WorldId;
    }

    
}
