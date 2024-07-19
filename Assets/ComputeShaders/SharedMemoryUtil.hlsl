#ifndef CS_SHARED_MEMORY_UTIL_HLSL
#define CS_SHARED_MEMORY_UTIL_HLSL

#define NUM_THREADS_PER_GROUP_DIMENSION 8

static const int3 num_threads_per_group_dimension = NUM_THREADS_PER_GROUP_DIMENSION;
static const int num_threads_per_group = num_threads_per_group_dimension.x * num_threads_per_group_dimension.y * num_threads_per_group_dimension.z;
static const int group_shared_memory_size = num_threads_per_group;

inline int3 CellIndexToSmIndex(int3 cell_index, int3 group_id)
{
    return cell_index - group_id * num_threads_per_group_dimension;
}

inline int3 GroupThreadIDToSmIndex(int3 group_thread_id)
{
    return group_thread_id;
}

inline uint SmIndexToSmID(int3 sm_index)
{
    return sm_index.x + sm_index.y * num_threads_per_group_dimension.x + sm_index.z * num_threads_per_group_dimension.x * num_threads_per_group_dimension.y;
}

inline uint GroupThreadIDToSmId(int3 group_thread_id)
{
    return SmIndexToSmID(GroupThreadIDToSmIndex(group_thread_id));
}

#endif /* CS_SHARED_MEMORY_UTIL_HLSL */