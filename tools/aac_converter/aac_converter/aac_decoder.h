#ifndef _AAC_DECODER_H_
#define _AAC_DECODER_H_

#include "include/neaacdec.h"

#ifdef __cplusplus
extern "C" {
#endif
NeAACDecHandle start_decode(unsigned char* framebuff, int framesize, int* samplerate);

void end_decode(NeAACDecHandle hdecoder);

int get_one_ADTS_frame(unsigned char* buffer, int buf_size, unsigned char* data ,int* data_size);

int check_is_ADTS_frame(unsigned char* buffer, int buf_size);

int decode_one_ADTS_frame(NeAACDecHandle hdecoder, unsigned char* inputbuffer, int inputsize, unsigned char* outputbuffer, int* outputsize);

int decode_all_buffer(unsigned char* inputbuffer, int inputsize, unsigned char* outputbuffer, int* outputsize);

#ifdef __cplusplus
}
#endif
#endif//_AAC_DECODER_H_