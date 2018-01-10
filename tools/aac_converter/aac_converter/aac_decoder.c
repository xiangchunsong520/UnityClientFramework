#include "aac_decoder.h"

#include <stdio.h>
#include <stdlib.h>
#include <string.h>

NeAACDecHandle start_decode(unsigned char* framebuff, int framesize, int* samplerate)
{
    int channels;
    NeAACDecHandle decoder;
    NeAACDecConfigurationPtr config;

    decoder = NeAACDecOpen();

    if (decoder)
    {
        config = NeAACDecGetCurrentConfiguration(decoder);
        config->defObjectType = LC;
        config->outputFormat = FAAD_FMT_16BIT;
        config->defSampleRate = 44100;
        config->dontUpSampleImplicitSBR = 1;
        NeAACDecSetConfiguration(decoder, config);

        if (NeAACDecInit(decoder, framebuff, framesize, samplerate, &channels) < 0)
        {
            NeAACDecClose(decoder);
            return NULL;
        }
    }
    else
    {
        return NULL;
    }

    return decoder;
}

void end_decode(NeAACDecHandle hdecoder)
{
    NeAACDecClose(hdecoder);
}

int get_one_ADTS_frame(unsigned char* buffer, int buf_size, unsigned char* data ,int* data_size)  
{  
    int size = 0;  

    if(!buffer || !data || !data_size )  
    {  
        return -1;  
    }  

    while(1)  
    {  
        if(buf_size  < 7 )  
        {  
            return -1;  
        }  

        if((buffer[0] == 0xff) && ((buffer[1] & 0xf0) == 0xf0) )  
        {  
            size |= ((buffer[3] & 0x03) <<11);     //high 2 bit  
            size |= buffer[4]<<3;                //middle 8 bit  
            size |= ((buffer[5] & 0xe0)>>5);        //low 3bit  
            break;  
        }  
        --buf_size;  
        ++buffer;  
    }  

    if(buf_size < size)  
    {  
        return -1;  
    }  

    memcpy(data, buffer, size);  
    *data_size = size;  

    return 0;  
} 

int check_is_ADTS_frame(unsigned char* buffer, int buf_size)
{
    int size = 0;  

    if(!buffer)  
    {  
        return -1;  
    }  

    if(buf_size  < 7 )  
    {  
        return -1;  
    }  

    if((buffer[0] == 0xff) && ((buffer[1] & 0xf0) == 0xf0) )  
    {  
        size |= ((buffer[3] & 0x03) <<11);     //high 2 bit  
        size |= buffer[4]<<3;                //middle 8 bit  
        size |= ((buffer[5] & 0xe0)>>5);        //low 3bit  
    }  

    if(buf_size != size)  
    {  
        return -1;  
    }  

    return 0;
}

int decode_one_ADTS_frame(NeAACDecHandle hdecoder, unsigned char* inputbuffer, int inputsize, unsigned char* outputbuffer, int* outputsize)
{
    unsigned char* samplebuffer;
    NeAACDecFrameInfo frameInfo;
    unsigned char* outtemp;

    *outputsize = 0;

    if (!hdecoder || !inputbuffer || !outputbuffer || !outputsize)
    {
        return -1;
    }

    outtemp = outputbuffer;

    if (check_is_ADTS_frame(inputbuffer, inputsize) != 0)
    {
        return -1;
    }

    samplebuffer = (unsigned char*)NeAACDecDecode(hdecoder, &frameInfo, inputbuffer, inputsize);
    if (frameInfo.error > 0)
    {
        return -1;
    }
    else if (frameInfo.samples > 0)
    {
        int i;

        *outputsize = frameInfo.samples;
        for (i = 0; i < frameInfo.samples; ++i)
        {
            *outtemp = *samplebuffer;
            ++outtemp;
            samplebuffer = i % 2 == 0 ? samplebuffer + 1 : samplebuffer + 3;
        }
    }

    return 0;
}

int decode_all_buffer(unsigned char* inputbuffer, int inputsize, unsigned char* outputbuffer, int* outputsize)
{
    NeAACDecHandle decoder;
    unsigned char* intemp;
    unsigned char* outtemp;
    unsigned char* frameBuffer;
    int framebufsize;
    int pcmsize;
    int samplerate;
    int leftsize = inputsize;

    *outputsize = 0;

    if (!inputbuffer || !outputbuffer || !outputsize)
    {
        return -1;
    }

    intemp = inputbuffer;
    outtemp = outputbuffer;
    frameBuffer = (unsigned char*)malloc(sizeof(unsigned char)*4096);
    if (!frameBuffer)
    {
        return -1;
    }

    if (get_one_ADTS_frame(intemp, leftsize, frameBuffer, &framebufsize) != 0)
    {
        return -1;
    }

    decoder = start_decode(frameBuffer, framebufsize, &samplerate);
    if (!decoder)
    {
        free(frameBuffer);
        return -1;
    }

    do
    {

        pcmsize = 0;
        decode_one_ADTS_frame(decoder, frameBuffer, framebufsize, outtemp, &pcmsize);

        *outputsize += pcmsize;
        outtemp += pcmsize;

        intemp += framebufsize;
        leftsize -= framebufsize;
    } while (get_one_ADTS_frame(intemp, leftsize, frameBuffer, &framebufsize) == 0);

    end_decode(decoder);

    free(frameBuffer);

    return 0;
}