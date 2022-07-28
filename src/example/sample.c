#include <stdio.h>

long int result;
volatile long int x = 42L;

__attribute__((import_name("put_value")))
int put_value();

__attribute__((export_name("get_result")))
long int get_result(){
    return result;
}


int main(void) {
    int n = put_value();
    result = n * x;
    printf("the answer is %ld\n", result);
    return 0;
}