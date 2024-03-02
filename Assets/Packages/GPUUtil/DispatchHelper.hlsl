#ifndef CS_DISPATCH_HELPER_HLSL
#define CS_DISPATCH_HELPER_HLSL

uint3 _NumThreads;
uint3 _NumGroups;

#define RETURN_IF_INVALID(TID) if (any(TID >= _NumThreads)) return;

#endif /* CS_DISPATCH_HELPER_HLSL */