#include <mono-wasi/driver.h>

__attribute__((import_name("log_external")))
void log_external(char* message, int message_len);

__attribute__((import_name("prompt_and_fill")))
int prompt_and_fill(char* prompt, int prompt_len, void* buf);

MonoString* prompt_and_fill_mono(MonoString* prompt){
    char* prompt_utf8 = mono_wasm_string_get_utf8(prompt);
    char* buffer = malloc(0);
    int result_len = prompt_and_fill(prompt_utf8, strlen(prompt_utf8), buffer);
    MonoString* response = mono_wasm_string_from_js(buffer);
    free(prompt_utf8);
    return response;
}

void log_external_mono(MonoString* message){
    char* message_utf8 = mono_wasm_string_get_utf8(message);
    return log_external(message_utf8, strlen(message_utf8));
}

void bundled_files_attach_internal_calls() {
    mono_add_internal_call("Client.PlatformReference::LogExternal", log_external_mono);
    mono_add_internal_call("Client.PlatformReference::PromptAndFill", prompt_and_fill_mono);
}
