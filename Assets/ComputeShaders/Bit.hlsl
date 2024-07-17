#ifndef CS_BIT_HLSL
#define CS_BIT_HLSL

#define SET_BIT(target, shift) target = target | (1<<shift)
#define CLEAR_BIT(target, shift) target = target & (~(1<<shift))

#define SET_VALUE(target, val, mask, shift) target = (target & ~mask) | ((val << shift) & mask);
#define GET_VALUE(val, mask, shift) (val & mask) >> shift


#endif /* CS_BIT_HLSL */