/*-
 * Copyright 2003-2005 Colin Percival
 * All rights reserved
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted providing that the following conditions 
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED.  IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
 * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING
 * IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

#if 0
__FBSDID("$FreeBSD: src/usr.bin/bsdiff/bspatch/bspatch.c,v 1.1 2005/08/06 01:59:06 cperciva Exp $");
#endif

#include <bzlib.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>
//#include <err.h>
#include <unistd.h>
#include <fcntl.h>

#include "bspatch.h"

static off_t offtin(u_char *buf)
{
	off_t y;

	y=buf[7]&0x7F;
	y=y*256;y+=buf[6];
	y=y*256;y+=buf[5];
	y=y*256;y+=buf[4];
	y=y*256;y+=buf[3];
	y=y*256;y+=buf[2];
	y=y*256;y+=buf[1];
	y=y*256;y+=buf[0];

	if(buf[7]&0x80) y=-y;

	return y;
}

int bspatch(int argc,char * argv[])
{
	FILE * f, * cpf, * dpf, * epf;
	BZFILE * cpfbz2, * dpfbz2, * epfbz2;
	int cbz2err, dbz2err, ebz2err;
	int fd;
	ssize_t oldsize,newsize;
	ssize_t bzctrllen,bzdatalen;
	u_char header[32],buf[8];
	off_t oldpos,newpos;
	off_t ctrl[3];
	off_t lenread;
	off_t i;

    FILE *oldFile, *newFile;
    u_char *buffer1, *buffer2;
    int cpSize, leftSize;
    int bufferSize = 1024 * 1024;

	if(argc!=4) return 1;

	/* Open patch file */
	if ((f = fopen(argv[3], "r")) == NULL)
		return 2;

	/*
	File format:
		0	8	"BSDIFF40"
		8	8	X
		16	8	Y
		24	8	sizeof(newfile)
		32	X	bzip2(control block)
		32+X	Y	bzip2(diff block)
		32+X+Y	???	bzip2(extra block)
	with control block a set of triples (x,y,z) meaning "add x bytes
	from oldfile to x bytes from the diff block; copy y bytes from the
	extra block; seek forwards in oldfile by z bytes".
	*/

	/* Read header */
	if (fread(header, 1, 32, f) < 32) {
		if (feof(f))
			return 3;
		return 4;
	}

	/* Check for appropriate magic */
	if (memcmp(header, "BSDIFF40", 8) != 0)
		return 5;

	/* Read lengths from header */
	bzctrllen=offtin(header+8);
	bzdatalen=offtin(header+16);
	newsize=offtin(header+24);
	if((bzctrllen<0) || (bzdatalen<0) || (newsize<0))
		return 6;

	/* Close patch file and re-open it via libbzip2 at the right places */
	if (fclose(f))
		return 7;
	if ((cpf = fopen(argv[3], "r")) == NULL)
		return 8;
	if (fseeko(cpf, 32, SEEK_SET))
		return 9;
	if ((cpfbz2 = BZ2_bzReadOpen(&cbz2err, cpf, 0, 0, NULL, 0)) == NULL)
		return 10;
	if ((dpf = fopen(argv[3], "r")) == NULL)
		return 11;
	if (fseeko(dpf, 32 + bzctrllen, SEEK_SET))
		return 12;
	if ((dpfbz2 = BZ2_bzReadOpen(&dbz2err, dpf, 0, 0, NULL, 0)) == NULL)
		return 13;
	if ((epf = fopen(argv[3], "r")) == NULL)
		return 14;
	if (fseeko(epf, 32 + bzctrllen + bzdatalen, SEEK_SET))
		return 15;
	if ((epfbz2 = BZ2_bzReadOpen(&ebz2err, epf, 0, 0, NULL, 0)) == NULL)
		return 16;

    if ((oldFile = fopen(argv[1], "rb")) == NULL)
        return 17;
    fseek(oldFile, 0, SEEK_END);
    oldsize = ftell(oldFile);

    if ((newFile = fopen(argv[2], "wb")) == NULL)
        return 18;

    if ((buffer1 = (u_char*)malloc(bufferSize)) == NULL) return 19;
    if ((buffer2 = (u_char*)malloc(bufferSize)) == NULL) return 20;

	oldpos=0;newpos=0;
	while(newpos<newsize) {
		/* Read control data */
		for(i=0;i<=2;i++) {
			lenread = BZ2_bzRead(&cbz2err, cpfbz2, buf, 8);
			if ((lenread < 8) || ((cbz2err != BZ_OK) &&
			    (cbz2err != BZ_STREAM_END)))
				return 21;
			ctrl[i]=offtin(buf);
		};

		/* Sanity-check */
		if(newpos+ctrl[0]>newsize)
			return 22;

        leftSize = ctrl[0];
		/* Read diff string */
        while (leftSize > 0)
        {
            cpSize = leftSize < bufferSize ? leftSize : bufferSize;
		    lenread = BZ2_bzRead(&dbz2err, dpfbz2, buffer1, cpSize);
		    if ((lenread < cpSize) ||
		        ((dbz2err != BZ_OK) && (dbz2err != BZ_STREAM_END)))
			    return 23;

            fseek(oldFile, oldpos, SEEK_SET);
            fread(buffer2, sizeof(u_char), cpSize, oldFile);
		    /* Add old data to diff string */
		    for(i=0;i<cpSize;i++)
			    if((oldpos+i>=0) && (oldpos+i<oldsize))
				    buffer1[i]+=buffer2[i];

            fseek(newFile, newpos, SEEK_SET);
            fwrite(buffer1, sizeof(u_char), cpSize, newFile);
		    /* Adjust pointers */
		    newpos+=cpSize;
		    oldpos+=cpSize;

            leftSize -= cpSize;
        }

		/* Sanity-check */
		if(newpos+ctrl[1]>newsize)
			return 24;

		/* Read extra string */
        leftSize = ctrl[1];
        while (leftSize > 0)
        {
            cpSize = leftSize < bufferSize ? leftSize : bufferSize;
		    lenread = BZ2_bzRead(&ebz2err, epfbz2, buffer1, cpSize);
		    if ((lenread < cpSize) ||
		        ((ebz2err != BZ_OK) && (ebz2err != BZ_STREAM_END)))
			    return 25;

            fseek(newFile, newpos, SEEK_SET);
            fwrite(buffer1, sizeof(u_char), cpSize, newFile);
		    /* Adjust pointers */
			newpos+=cpSize;

            leftSize -= cpSize;
        }
		
        oldpos+=ctrl[2];
    };

	/* Clean up the bzip2 reads */
	BZ2_bzReadClose(&cbz2err, cpfbz2);
	BZ2_bzReadClose(&dbz2err, dpfbz2);
	BZ2_bzReadClose(&ebz2err, epfbz2);
	if (fclose(cpf) || fclose(dpf) || fclose(epf))
		return 26;
    if (fclose(oldFile))
        return 27;
    if (fclose(newFile))
        return 28;
	
    free(buffer1);
    free(buffer2);

	return 0;
}

_DLLExport int patch(char* oldPath, char* newPath, char* patchPath)
{
	int argc = 4;
	char * argv[argc];
	argv[0] = "bspatch";
	argv[1] = oldPath;
	argv[2] = newPath;
	argv[3] = patchPath;

	int ret = bspatch(argc, argv);
	return ret;
}
