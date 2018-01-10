#ifndef _AAC_ENCODER_H_
#define _AAC_ENCODER_H_

#include "include/faac.h"

#ifdef __cplusplus
extern "C" {
#endif
faacEncHandle start_encode(int samplerate, unsigned int channels, int* sampleperframe, int* maxoutputbytes);

void end_encode(faacEncHandle hencoder);

int encode_one_frame(faacEncHandle hencoder, unsigned char* inputbuffer, int inputsize, int maxoutputbytes, int leftsize , unsigned char* outputbuffer, int* outputsize);

int encode_all_buffer(int samplerate, unsigned int channels, unsigned char* inputbuffer, int inputsize, unsigned char* outputbuffer, int* outputsize);

#ifdef __cplusplus
}
#endif
#endif//_AAC_ENCODER_H_