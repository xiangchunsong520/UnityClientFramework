LOCAL_PATH := $(call my-dir)

include $(CLEAR_VARS)

LOCAL_MODULE:= patch

LOCAL_SRC_FILES:= 		\
		blocksort.c		\
		bzlib.c			\
		compress.c		\
		crctable.c		\
		decompress.c	\
		huffman.c		\
		randtable.c		\
		bspatch.c
		
LOCAL_C_INCLUDES :=		\
		$(JNI_H_INCLUDE)\
		$(LOCAL_PATH)	\
		$(LOCAL_PATH)/bzip2
		
LOCAL_LDLIBS     := -lz -llog  

include $(BUILD_SHARED_LIBRARY)