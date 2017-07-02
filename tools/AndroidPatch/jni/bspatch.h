#ifndef _Included_bspatch_
#define _Included_bspatch_

#if _MSC_VER
#define  _DLLExport __declspec (dllexport)
#else
#define  _DLLExport
#endif

#include <jni.h>

#ifdef __cplusplus
extern "C" {
#endif

	_DLLExport int patch(char* oldPath, char* newPath, char* patchPath);

#ifdef __cplusplus
}
#endif
#endif
