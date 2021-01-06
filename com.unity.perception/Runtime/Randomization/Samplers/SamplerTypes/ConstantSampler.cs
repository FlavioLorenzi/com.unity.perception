﻿using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UnityEngine.Experimental.Perception.Randomization.Samplers
{
    /// <summary>
    /// Returns a constant value when sampled
    /// </summary>
    [Serializable]
    public class ConstantSampler : ISampler
    {
        /// <summary>
        /// The value from which samples will be generated
        /// </summary>
        public float value;

        /// <summary>
        /// A range bounding the values generated by this sampler
        /// </summary>
        public FloatRange range
        {
            get => new FloatRange(value, value);
            set { }
        }

        /// <summary>
        /// Constructs a ConstantSampler
        /// </summary>
        public ConstantSampler()
        {
            value = 0f;
        }

        /// <summary>
        /// Constructs a new ConstantSampler
        /// </summary>
        /// <param name="value">The value from which samples will be generated</param>
        public ConstantSampler(float value)
        {
            this.value = value;
        }

        /// <summary>
        /// Generates one sample
        /// </summary>
        /// <returns>The generated sample</returns>
        public float Sample()
        {
            return value;
        }

        /// <summary>
        /// Schedules a job to generate an array of samples
        /// </summary>
        /// <param name="sampleCount">The number of samples to generate</param>
        /// <param name="jobHandle">The handle of the scheduled job</param>
        /// <returns>A NativeArray of generated samples</returns>
        public NativeArray<float> Samples(int sampleCount, out JobHandle jobHandle)
        {
            var samples = new NativeArray<float>(
                sampleCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            jobHandle = new SampleJob
            {
                value = value,
                samples = samples
            }.Schedule();
            return samples;
        }

        [BurstCompile]
        struct SampleJob : IJob
        {
            public float value;
            public NativeArray<float> samples;

            public void Execute()
            {
                for (var i = 0; i < samples.Length; i++)
                    samples[i] = value;
            }
        }
    }
}
