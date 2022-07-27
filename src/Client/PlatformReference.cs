using System.Runtime.CompilerServices;

namespace Client
{
    public class PlatformReference
    {
        // Imports
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void LogExternal(string message);

        
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string PromptAndFill(string prompt);

        // Exports
        public unsafe static void CallDialog(int dialogId)
        {
            try{
                var product = PromptAndFill($"[{dialogId}] Hello, what do you want to order?");
                PlatformReference.LogExternal($"Received response: '{product}'");
                switch (product)
                {
                    case "pizza":
                        var pizzaType = PromptAndFill($"[{dialogId}]What kind of pizza would you like?");
                        PlatformReference.LogExternal($"Received response: '{pizzaType}'");
                        switch(pizzaType){
                            case "salami":
                            case "cheese":
                                LogExternal($"[{dialogId}] Order created for '{product}', type: '{pizzaType}'");
                                break;
                            default: LogExternal($"[{dialogId}] Unknown pizzaType: '{pizzaType}'"); break;
                        }
                        break;
                    default: LogExternal($"[{dialogId}] Unknown product: '{product}'"); break;
                }
            }catch(Exception ex){
                LogExternal($"[{dialogId}] Exception'{ex}'");
            }
        }
    }
}
