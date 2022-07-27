
namespace Client
{
    public static class StaticMethods
    {
        public static string PromptAndFill(string prompt){
            try{
                var response = PlatformReference.PromptAndFill(prompt);
                PlatformReference.LogExternal($"Received response: '{response}'");
                return response;
            }catch(Exception e){
                PlatformReference.LogExternal($"Received exception: {e}");
                throw;
            }
        }
    }
}