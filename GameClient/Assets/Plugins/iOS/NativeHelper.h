//
//  NativeHelper.h
//  Unity-iPhone
//
//  Created by longyouMac on 2017/6/19.
//
//

#ifndef NativeHelper_h
#define NativeHelper_h

extern "C"
{
    const char* getVersionCode();
    const char* getIPv6(const char *mHost);
    int getNetworkType();
};

#endif /* NativeHelper_h */
