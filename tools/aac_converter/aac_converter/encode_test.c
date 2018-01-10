#include "aac_encoder.h"

#include <stdio.h>
#include <stdlib.h>

int main()
{
    long lSize;
    FILE * pFile;
    FILE * outFile;
    FILE * out2File;
    unsigned char* buffer;
    unsigned char* inbuffer;
    unsigned char* frameBuffer;
    int framebufsize;
    unsigned char* pcmBuffer;
    unsigned long outputsize;
    int frame;
    unsigned long samplerate;
    unsigned char channel;

    unsigned char* OutAllbuffer;

    pFile = fopen("e:/test_out.wav", "rb");
    outFile = fopen("e:/test_out.aac", "wb");
    out2File = fopen("e:/test_out_2.aac", "wb");
    if (!pFile || !outFile || !out2File)
    {
        return 1;
    }

    fseek(pFile, 0, SEEK_END);
    lSize = ftell(pFile);
    rewind(pFile);

    buffer = (unsigned char*)malloc(sizeof(unsigned char)*lSize);
    if (buffer == NULL)
    {
        fclose(pFile);
        return 1;
    }

    if (fread(buffer, 1, lSize, pFile) != lSize)
    {
        fclose(pFile);
        free(buffer);
        return 1;
    }
    inbuffer = buffer;
    printf("input size : %d \n", lSize);

    OutAllbuffer = (unsigned char*)malloc(sizeof(unsigned char)*lSize);
    if (encode_all_buffer(44100, 1, buffer, lSize, OutAllbuffer, &outputsize) == 0)
    {
        fwrite(OutAllbuffer, 1, outputsize, out2File);
        fflush(out2File);
    }

    fclose(pFile);
    fclose(outFile);
    fclose(out2File);

    system("pause");
    return 0;
}