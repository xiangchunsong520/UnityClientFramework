#include "aac_converter.h"
#include <stdio.h>

int main()
{
    int id;
    unsigned long frameSamples, maxOutputBytes;
    id = StartEncode(44100, 1, &frameSamples, &maxOutputBytes);
    id = StartEncode(44100, 1, &frameSamples, &maxOutputBytes);
    printf("id\t%d\n", id);
    printf("frameSamples\t%d\n", frameSamples);
    printf("maxOutputBytes\t%d\n", maxOutputBytes);
    return 0;
}