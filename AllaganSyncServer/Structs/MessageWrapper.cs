using System.Reflection;
using System.Runtime.InteropServices;

namespace AllaganSyncServer.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 0x18)]
    public struct MessageWrapper
    {
        [FieldOffset(0x0)] public StructTypes Type;
        [FieldOffset(0x4)] public StructTypes ResponseType;
        [FieldOffset(0x8)] public int Counter;
        [FieldOffset(0xC)] public int Checksum;
        [FieldOffset(0x10)] public long Time;
    }

    public enum StructTypes : uint
    {
        Null = 0,
        Init = 1,
        LastUpdate = 2,
        UpdateMessage = 3
    }

    [AttributeUsage(AttributeTargets.Struct)]
    public class MessageStructType : Attribute
    {
        public StructTypes Type;
        public MessageStructType(StructTypes type)
        {
            Type = type;
        }
    }

    public static class MessageWrapperExtensions
    {
        public static byte[] ToBytes(this MessageWrapper message)
        {
            var size = Marshal.SizeOf(message);
            var arr = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(message, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        public static MessageWrapper ToMessageWrapper(this byte[] bytes)
        {
            var size = Marshal.SizeOf(typeof(MessageWrapper));
            var ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(bytes, 0, ptr, size);

            var message = (MessageWrapper)(Marshal.PtrToStructure(ptr, typeof(MessageWrapper)) ?? new MessageWrapper());
            Marshal.FreeHGlobal(ptr);

            return message;
        }

        public static object GetSubStruct(this MessageWrapper wrapper, byte[] bytes)
        {
            var type = Assembly.GetExecutingAssembly().GetTypes().First(t => ((MessageStructType?)Attribute.GetCustomAttribute(t, typeof(MessageStructType)))?.Type == wrapper.Type);
            var size = Marshal.SizeOf(type);
            var ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(bytes, 0x18, ptr, size);

            var stuct = Marshal.PtrToStructure(ptr, type)!;
            Marshal.FreeHGlobal(ptr);
            return stuct;
        }

        public static Tuple<byte[], StructTypes> ToBytes<T>(this T @struct) where T : struct
        {
            var type = ((MessageStructType?)Attribute.GetCustomAttribute(@struct.GetType(), typeof(MessageStructType)))!.Type;
            var size = Marshal.SizeOf(@struct);
            var arr = new byte[size];
            var ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(@struct, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return Tuple.Create(arr, type);
        }
    }
}
