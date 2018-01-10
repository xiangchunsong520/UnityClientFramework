LOCAL_PATH := $(call my-dir)/../../aac_converter

include $(CLEAR_VARS)

LOCAL_MODULE := faac

LOCAL_SRC_FILES:= faac_faad_android_x86/libfaac.a

include $(PREBUILT_STATIC_LIBRARY) 

include $(CLEAR_VARS)

LOCAL_MODULE := faad

LOCAL_SRC_FILES:= faac_faad_android_x86/libfaad.a

include $(PREBUILT_STATIC_LIBRARY) 

include $(CLEAR_VARS)

LOCAL_MODULE:= aac_converter

LOCAL_SRC_FILES:=			\
		aac_decoder.c				\
		aac_encoder.c			\
		aac_converter.c

LOCAL_C_INCLUDES :=				\
	$(LOCAL_PATH)				\

LOCAL_CFLAGS:=		\
	-D_ANDROID_
LOCAL_LDLIBS := -llog -lz
	
LOCAL_STATIC_LIBRARIES:= faac faad
	
include $(BUILD_SHARED_LIBRARY)