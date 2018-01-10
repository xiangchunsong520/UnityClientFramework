#include <stdio.h>
#include <stdlib.h>

#include "aac_decoder.h"

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
    unsigned long pcmsize;
    int frame;
    NeAACDecHandle hdecode;
    unsigned long samplerate;
    unsigned char channel;

    unsigned char* OutAllbuffer;

    pFile = fopen("e:/out.aac", "rb");
    outFile = fopen("e:/test_out.wav", "wb");
    out2File = fopen("e:/test_out_2.wav", "wb");
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

    OutAllbuffer = (unsigned char*)malloc(sizeof(unsigned char)*1024*1024*20);
   if (decode_all_buffer(buffer, lSize, OutAllbuffer, &pcmsize)==0)
   {
       fwrite(OutAllbuffer, 1, pcmsize, out2File);
       fflush(out2File);
   }

    frame = 0;
    printf("input size : %d \n", lSize);
    frameBuffer = (unsigned char*)malloc(sizeof(unsigned char)*4096);
    if (get_one_ADTS_frame(buffer, lSize, frameBuffer, &framebufsize) != 0)
    {
        return 1;
    }

    hdecode = start_decode(frameBuffer, framebufsize, &samplerate);
    if (!hdecode)
    {
        return 1;
    }

    printf("samplerate : %d\n", samplerate);

    pcmBuffer = (unsigned char*)malloc(sizeof(unsigned char)*4096);
    do
    {
        pcmsize = 0;
        decode_one_ADTS_frame(hdecode, frameBuffer, framebufsize, pcmBuffer, &pcmsize);
        if (!pcmsize)
            printf("%d decode from aac size : %d to pcm size : %d\n", frame, framebufsize, pcmsize);

        fwrite(pcmBuffer, 1, pcmsize, outFile);
        fflush(outFile);

        buffer += framebufsize;
        lSize -= framebufsize;

        ++frame;
    } while (get_one_ADTS_frame(buffer, lSize, frameBuffer, &framebufsize) == 0);
	
	end_decode(hdecode);

    printf("frame : %d\n", frame);

    free(inbuffer);
    free(frameBuffer);

    fclose(pFile);
    fclose(outFile);
    fclose(out2File);

    system("pause");
    return 0;
}