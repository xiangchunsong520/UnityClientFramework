#include "aac_converter.h";
#include "aac_encoder.h";
#include "aac_decoder.h";

//////////////////////////////////////////////////////////////////////////
Handle StartEncode(int samplerate, int channels, int* sampleperframe, int* maxoutputbytes)
{
    return start_encode(samplerate, channels, sampleperframe, maxoutputbytes);
}

void EndEncode(Handle encoder)
{
    end_encode(encoder);
}

int EncodeOneFrame(Handle encoder, unsigned char* inputbuffer, int inputsize, int maxoutputbytes, int leftsize , unsigned char* outputbuffer, int* outputsize)
{
    return encode_one_frame(encoder, inputbuffer, inputsize, maxoutputbytes, leftsize, outputbuffer, outputsize);
}

//////////////////////////////////////////////////////////////////////////
Handle StartDecode(unsigned char* framebuff, int framesize, int* samplerate)
{
    return start_decode(framebuff, framesize, samplerate);
}

void EndDecode(Handle decoder)
{
    end_decode(decoder);
}

int DecodeOneFrame(Handle decoder, unsigned char* inputbuffer, int inputsize, unsigned char* outputbuffer, int* outputsize)
{
    return decode_one_ADTS_frame(decoder, inputbuffer, inputsize, outputbuffer, outputsize);
}