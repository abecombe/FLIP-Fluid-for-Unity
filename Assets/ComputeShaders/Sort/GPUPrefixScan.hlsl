﻿#ifndef CS_SORT_GPU_PREFIX_SCAN_HLSL
#define CS_SORT_GPU_PREFIX_SCAN_HLSL

//#pragma kernel PrefixScan
//#pragma kernel AddGroupSum

#pragma warning (disable : 3568)
#pragma multi_compile _ NUM_GROUP_THREADS_128 NUM_GROUP_THREADS_256 NUM_GROUP_THREADS_512
#if !defined(NUM_GROUP_THREADS_128) && !defined(NUM_GROUP_THREADS_256) && !defined(NUM_GROUP_THREADS_512)
#define NUM_GROUP_THREADS_128
#endif

#if defined(NUM_GROUP_THREADS_128)
#define NUM_GROUP_THREADS 128
#elif defined(NUM_GROUP_THREADS_256)
#define NUM_GROUP_THREADS 256
#elif defined(NUM_GROUP_THREADS_512)
#define NUM_GROUP_THREADS 512
#endif

// macro used for computing bank-conflict-free shared memory array indices
#define NUM_BANKS 32
#define LOG_NUM_BANKS 5
#define CONFLICT_FREE_OFFSET(n) ((n) >> LOG_NUM_BANKS)

RWStructuredBuffer<uint> data_buffer;
RWStructuredBuffer<uint> group_sum_buffer;

int num_elements;
int group_offset;
int group_sum_offset;

static const int num_group_threads = NUM_GROUP_THREADS;
static const int num_elements_per_group = 2 * NUM_GROUP_THREADS;
static const int num_elements_per_group_1 = num_elements_per_group - 1;
static const int log_num_elements_per_group = log2(num_elements_per_group);

static const int s_scan_len = num_elements_per_group + (num_elements_per_group >> LOG_NUM_BANKS);

groupshared uint s_scan[s_scan_len];

// scan input data locally and output total sums within groups
[numthreads(NUM_GROUP_THREADS, 1, 1)]
void PrefixScan(int group_thread_id : SV_GroupThreadID, int group_id : SV_GroupID)
{
    // handle two values in one thread
    int ai = group_thread_id;
    int bi = ai + num_group_threads;
    
    int global_ai = group_thread_id + num_elements_per_group * (group_id + group_offset);
    int global_bi = global_ai + num_group_threads;
    
    // copy input data to shared memory
    s_scan[ai + CONFLICT_FREE_OFFSET(ai)] = global_ai < num_elements ? data_buffer[global_ai] : 0;
    s_scan[bi + CONFLICT_FREE_OFFSET(bi)] = global_bi < num_elements ? data_buffer[global_bi] : 0;

    int offset = 1;
    
    // upsweep step
    [unroll(log_num_elements_per_group)]
    for (int du = num_elements_per_group >> 1;; du >>= 1)
    {
        GroupMemoryBarrierWithGroupSync();

        if (group_thread_id < du)
        {
            int ai_u = offset * ((group_thread_id << 1) + 1) - 1;
            int bi_u = offset * ((group_thread_id << 1) + 2) - 1;
            ai_u += CONFLICT_FREE_OFFSET(ai_u);
            bi_u += CONFLICT_FREE_OFFSET(bi_u);

            s_scan[bi_u] += s_scan[ai_u];
        }
        
        offset <<= 1;
    }

    // save the total sum on global memory
    if (group_thread_id == 0)
    {
        group_sum_buffer[group_id + group_offset + group_sum_offset] = s_scan[num_elements_per_group_1 + CONFLICT_FREE_OFFSET(num_elements_per_group_1)];
        s_scan[num_elements_per_group_1 + CONFLICT_FREE_OFFSET(num_elements_per_group_1)] = 0;
    }

    // downsweep step
    [unroll(log_num_elements_per_group)]
    for (int dd = 1;; dd <<= 1)
    {
        offset >>= 1;
        
        GroupMemoryBarrierWithGroupSync();

        if (group_thread_id < dd)
        {
            int ai_d = offset * ((group_thread_id << 1) + 1) - 1;
            int bi_d = offset * ((group_thread_id << 1) + 2) - 1;
            ai_d += CONFLICT_FREE_OFFSET(ai_d);
            bi_d += CONFLICT_FREE_OFFSET(bi_d);

            uint temp = s_scan[ai_d];
            s_scan[ai_d] = s_scan[bi_d];
            s_scan[bi_d] += temp;
        }
    }
    
    GroupMemoryBarrierWithGroupSync();

    // copy scanned data to global memory
    if (global_ai < num_elements)
        data_buffer[global_ai] = s_scan[ai + CONFLICT_FREE_OFFSET(ai)];
    if (global_bi < num_elements)
        data_buffer[global_bi] = s_scan[bi + CONFLICT_FREE_OFFSET(bi)];
}

// add each group's total sum to its scan output
[numthreads(NUM_GROUP_THREADS, 1, 1)]
void AddGroupSum(int group_thread_id : SV_GroupThreadID, int group_id : SV_GroupID)
{
    uint group_sum = group_sum_buffer[group_id + group_offset];

    int global_ai = group_thread_id + num_elements_per_group * (group_id + group_offset);
    int global_bi = global_ai + num_group_threads;
    
    if (global_ai < num_elements)
        data_buffer[global_ai] += group_sum;
    if (global_bi < num_elements)
        data_buffer[global_bi] += group_sum;
}


#endif /* CS_SORT_GPU_PREFIX_SCAN_HLSL */