#ifndef _CS_DISPATCHERHELPER_HLSL_
#define _CS_DISPATCHERHELPER_HLSL_

uint3 _NumThreads;

#define RETURN_IF_INVALID(TID) if (any(TID >= _NumThreads)) return;


#endif /* _CS_DISPATCHERHELPER_HLSL_ */
