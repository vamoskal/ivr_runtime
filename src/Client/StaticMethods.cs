
namespace Client
{
    public static class StaticMethods{
        public static void WriteHelloWorld(){
            Console.WriteLine("Hello, World!");
            PlatformReference.LogExternal(42);
        }
    }
}