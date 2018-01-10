#include "aac_encoder.h"

#include <stdio.h>
#include <stdlib.h>
#include <string.h>

faacEncHandle start_encode(int samplerate, unsigned int channels, int* sampleperframe, int* maxoutputbytes)
{
    faacEncHandle hEncoder = faacEncOpen(samplerate, channels,  sampleperframe, maxoutputbytes);

    if (hEncoder)
    {
        faacEncConfigurationPtr config = faacEncGetCurrentConfiguration(hEncoder);
        config->allowMidside = 1;
        config->aacObjectType = LOW;
        config->mpegVersion = MPEG4;
        config->outputFormat = 1;
        config->useTns = 1;
        config->useLfe = 0;
        config->quantqual = 40;
        config->bandWidth = 0;
        config->shortctl = SHORTCTL_NORMAL;

        if (!faacEncSetConfiguration(hEncoder, config))
        {
            faacEncClose(hEncoder);
            return NULL;
        }
        
        return hEncoder;
    }

    return NULL;
}

void end_encode(faacEncHandle hencoder)
{
    faacEncClose(hencoder);
}

int get_pcm_buffer(unsigned char* inputbuffer, int inputsize, int* pcmbuf, int* pcmsize)
{
    unsigned int i;
    *pcmsize = inputsize > 0 ? inputsize / 2 : 0;
    for (i = 0; i < *pcmsize; ++i)
    {
        int s = ((signed short *)inputbuffer)[i];
        pcmbuf[i] = s << 8;
    }
    return 0;
}

int encode_one_frame(faacEncHandle hencoder, unsigned char* inputbuffer, int inputsize, int maxoutputbytes, int leftsize , unsigned char* outputbuffer, int* outputsize)
{
    int writed;
    int* pcmbuf;
    int pcmsize;

    *outputsize = 0;

    if (!hencoder)
    {
        return -1;
    }

    pcmbuf = (int *)malloc(inputsize/2*sizeof(int));
    if (!pcmbuf)
    {
        return -1;
    }

    do 
    {
        get_pcm_buffer(inputbuffer, inputsize, pcmbuf, &pcmsize);
        inputsize = 0;
        writed = faacEncEncode(hencoder, pcmbuf, pcmsize, outputbuffer, maxoutputbytes);

        outputbuffer += writed;
        *outputsize += writed;
    } while (leftsize <= 0 && writed);

    free(pcmbuf);

    return 0;
}

int encode_all_buffer(int samplerate, unsigned int channels, unsigned char* inputbuffer, int inputsize, unsigned char* outputbuffer, int* outputsize)
{
    unsigned char* intemp;
    unsigned char* outtemp;
    int sampleperframe;
    int maxoutputbytes;
    int curentsize;
    int writesize;
    int leftsize = inputsize;

    faacEncHandle hEncoder = start_encode(samplerate, channels, &sampleperframe, &maxoutputbytes);

    *outputsize = 0;

    if (!hEncoder)
    {
        return -1;
    }

    intemp = inputbuffer;
    outtemp = outputbuffer;

    while (leftsize)
    {
        curentsize = leftsize > sampleperframe * 2 ? sampleperframe * 2 : leftsize;
        leftsize -= curentsize;

        encode_one_frame(hEncoder, intemp, curentsize, maxoutputbytes, leftsize, outtemp, &writesize);

        intemp += curentsize;

        *outputsize += writesize;
        outtemp += writesize;
    }

    end_encode(hEncoder);

    return 0;
}