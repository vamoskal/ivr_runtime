#include <mono-wasi/driver.h>

__attribute__((import_name("log_external")))
void log_external(int magic_number);

void bundled_files_attach_internal_calls() {
    mono_add_internal_call("Client.PlatformReference::LogExternal", log_external);
}
