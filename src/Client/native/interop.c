#include <mono-wasi/driver.h>
#include <string.h>
#include <stdio.h>
#include <assert.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/object.h>


__attribute__((import_name("log_external")))
void log_external(char* message, int message_len);

__attribute__((import_name("prompt_and_fill")))
int prompt_and_fill(char* prompt, int prompt_len, void* buf);

void log(char* message){
    log_external(message, strlen(message));
}

void log_exception(MonoObject* object){
    MonoMethod* toString = lookup_dotnet_method("System.Private.CoreLib.dll", "System", "Object", "ToString", -1);
    void* method_params[] = { };
    MonoObject* exception = NULL;

    MonoString* text = mono_wasm_invoke_method(toString, object, method_params, &exception);
    assert(!exception);

    char* text_utf8 = mono_wasm_string_get_utf8(text);
    log(text_utf8);
    free(text_utf8);
}

__attribute__((export_name("call_dialog")))
void call_dialog(int dialog_id) {
    MonoMethod* method = lookup_dotnet_method("Client.dll", "Client", "PlatformReference", "CallDialog", -1);
    assert(method);

    void* method_params[] = { &dialog_id };
    MonoObject* exception = NULL;
    mono_wasm_invoke_method(method, NULL, method_params, &exception);
    if(exception != NULL){
        log_exception(exception);
    } 
    assert(!exception);
    free(method_params);
}

MonoDomain* mono_get_root_domain(void);

MonoString* prompt_and_fill_mono(MonoString* prompt){
    char* prompt_utf8 = mono_wasm_string_get_utf8(prompt);
    char* buffer = malloc(0);
    int result_len = prompt_and_fill(prompt_utf8, strlen(prompt_utf8), buffer);
    MonoString* response = mono_string_new_len(mono_get_root_domain(), buffer, result_len);
    free(prompt_utf8);
    free(buffer);
    return response;
}

void log_external_mono(MonoString* message){
    char* message_utf8 = mono_wasm_string_get_utf8(message);
    log_external(message_utf8, strlen(message_utf8));
    free(message_utf8);
}

void bundled_files_attach_internal_calls() {
    mono_add_internal_call("Client.PlatformReference::LogExternal", log_external_mono);
    mono_add_internal_call("Client.PlatformReference::PromptAndFill", prompt_and_fill_mono);
}
