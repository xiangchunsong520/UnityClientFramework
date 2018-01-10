#ifndef _AAC_CONVERTER_H_
#define _AAC_CONVERTER_H_

#if _MSC_VER
#define  _DLLExport __declspec(dllexport)
#else
#define  _DLLExport
#endif

typedef void *Handle;

#ifdef __cplusplus
extern "C" {
#endif
    //////////////////////////////////////////////////////////////////////////
    Handle _DLLExport StartEncode(int samplerate, int channels, int* sampleperframe, int* maxoutputbytes);
    
    void _DLLExport EndEncode(Handle encoder);

    int _DLLExport EncodeOneFrame(Handle encoder, unsigned char* inputbuffer, int inputsize, int maxoutputbytes, int leftsize , unsigned char* outputbuffer, int* outputsize);

    //////////////////////////////////////////////////////////////////////////
    Handle _DLLExport StartDecode(unsigned char* framebuff, int framesize, int* samplerate);

    void _DLLExport EndDecode(Handle decoder);

    int _DLLExport DecodeOneFrame(Handle decoder, unsigned char* inputbuffer, int inputsize, unsigned char* outputbuffer, int* outputsize);
    
#ifdef __cplusplus
    }
#endif

#endif//_AAC_CONVERTER_H_