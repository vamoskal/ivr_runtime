using System.Runtime.CompilerServices;

namespace Client
{
    public class PlatformReference
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void LogExternal(int n);
    }
}
