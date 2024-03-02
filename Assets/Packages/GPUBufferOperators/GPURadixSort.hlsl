#ifndef CS_GPU_RADIX_SORT_HLSL
#define CS_GPU_RADIX_SORT_HLSL

//#pragma kernel RadixSortLocal
//#pragma kernel GlobalShuffle

// default define
#ifndef DATA_TYPE
#define DATA_TYPE uint2  // input data struct
#endif
#ifndef GET_KEY
#define GET_KEY(s) s.x   // how to get the key-values used for sorting
#endif

#define NUM_GROUP_THREADS 128

StructuredBuffer<DATA_TYPE> data_in_buffer;
RWStructuredBuffer<DATA_TYPE> data_out_buffer;

RWStructuredBuffer<uint> first_index_buffer;
RWStructuredBuffer<uint> group_sum_buffer;
StructuredBuffer<uint> global_prefix_sum_buffer;

int num_elements;
int num_groups;
int group_offset;
int bit_shift;

static const int num_elements_per_group = NUM_GROUP_THREADS;
static const int log_num_elements_per_group = log2(num_elements_per_group);
static const int num_elements_per_group_1 = num_elements_per_group - 1;

static const int n_way = 4;
static const int n_way_1 = n_way - 1;
static const int4 n_way_arr = int4(0, 1, 2, 3);

static const int s_data_len = num_elements_per_group;
static const int s_scan_len = num_elements_per_group;
static const int s_Pd_len = n_way;

groupshared DATA_TYPE s_data[s_data_len];
groupshared uint4 s_scan[s_scan_len];
groupshared uint s_Pd[s_Pd_len];

/**
 * \brief sort input data locally and output first-index / sums of each 2bit key-value within groups
 */
[numthreads(NUM_GROUP_THREADS, 1, 1)]
void RadixSortLocal(int group_thread_id : SV_GroupThreadID, int group_id : SV_GroupID)
{
    group_id += group_offset;
    int global_id = num_elements_per_group * group_id + group_thread_id;

    // extract 2 bits
    DATA_TYPE data;
    int key_2_bit = n_way_1;
    if (global_id < num_elements)
    {
        data = data_in_buffer[global_id];
        key_2_bit = ((GET_KEY(data)) >> bit_shift) & n_way_1;
    }

    // build scan data
    s_scan[group_thread_id] = (key_2_bit == n_way_arr);
    GroupMemoryBarrierWithGroupSync();

    // Hillis-Steele scan
    [unroll(log_num_elements_per_group)]
    for (int offset = 1;; offset <<= 1)
    {
        uint4 sum = s_scan[group_thread_id];
        int partner = group_thread_id - offset;
        if (partner >= 0)
        {
            sum += s_scan[partner];
        }
        GroupMemoryBarrierWithGroupSync();
        s_scan[group_thread_id] = sum;
        GroupMemoryBarrierWithGroupSync();
    }

    // calculate first index of each 2bit key-value
    uint4 total = s_scan[num_elements_per_group_1];
    uint4 first_index;
    uint run_sum = 0;
    [unroll(n_way)]
    for (int i = 0;; ++i)
    {
        first_index[i] = run_sum;
        run_sum += total[i];
    }

    if (group_thread_id < n_way)
    {
        // copy sums of each 2bit key-value to global memory
        group_sum_buffer[group_thread_id * num_groups + group_id] = total[group_thread_id];
        // copy first index of each 2bit key-value to global memory
        first_index_buffer[group_thread_id + n_way * group_id] = first_index[group_thread_id];
    }

    // sort the input data locally
    int new_id = first_index[key_2_bit];
    if (group_thread_id > 0)
    {
        new_id += s_scan[group_thread_id - 1][key_2_bit];
    }
    s_data[new_id] = data;

    GroupMemoryBarrierWithGroupSync();

    // copy sorted input data to global memory
    if (global_id < num_elements)
    {
        data_out_buffer[global_id] = s_data[group_thread_id];
    }
}

/**
 * \brief copy input data to final position in global memory
 */
[numthreads(NUM_GROUP_THREADS, 1, 1)]
void GlobalShuffle(int group_thread_id : SV_GroupThreadID, int group_id : SV_GroupID)
{
    group_id += group_offset;
    int global_id = num_elements_per_group * group_id + group_thread_id;

    if (group_thread_id < n_way)
    {
        // set global destination of each 2bit key-value
        s_Pd[group_thread_id] = global_prefix_sum_buffer[group_thread_id * num_groups + group_id];
        // subtract the first index of each 2bit key-value
        // to make it easier to calculate the final destination of individual data
        s_Pd[group_thread_id] -= first_index_buffer[group_thread_id + n_way * group_id];
    }

    GroupMemoryBarrierWithGroupSync();

    if (global_id < num_elements)
    {
        DATA_TYPE data = data_in_buffer[global_id];
        int key_2_bit = ((GET_KEY(data)) >> bit_shift) & n_way_1;

        int new_id = group_thread_id + s_Pd[key_2_bit];

        // copy data to the final destination
        data_out_buffer[new_id] = data;
    }
}


#endif /* CS_GPU_RADIX_SORT_HLSL */