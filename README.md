# ivr_runtime

## Required tools

- WebAssembly Toolkit for VSCode (Extension)
- https://github.com/webassembly/wabt
 
## Ho To build

### Client

requireds dotnet sdk 7+

download 'wabt' tools here https://github.com/webassembly/wabt and put into folder '<root>/external/bin'

```
cd <root>\src\Client
dotnet build
```

### Server 
```
cd <root>\src\Server
dotnet run
```
Use Postman collections provided

### AssemblyScript example
https://www.assemblyscript.org/getting-started.html#setting-up-a-new-project 
In case of error below, update node.js
```
"SyntaxError: missing ) after argument list"
```