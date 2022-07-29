// The entry file of your WebAssembly module.

let x = 42;
let result = 0;

@external("env", "put_value")
declare function put_value(): i32;

export function get_result(): i32 {
  return result;
}

export function _start():void{
  let n = put_value();
  result = n * x;
}