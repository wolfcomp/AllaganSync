using System.Runtime.InteropServices;

namespace AllaganSyncServer.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 0x4C)]
    [MessageStructType(StructTypes.Init)]
    public unsafe struct Init
    {
        [FieldOffset(0x0)] public long Id;
        [FieldOffset(0x8)] public uint WorldId;
        [FieldOffset(0xC)] public fixed byte Name[64];
    }
}
