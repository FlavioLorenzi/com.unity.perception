﻿using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Experimental.Perception.Randomization.Scenarios;

namespace UnityEngine.Experimental.Perception.Randomization.Samplers
{
    /// <summary>
    /// Returns normally distributed random values bounded within a specified range
    /// https://en.wikipedia.org/wiki/Truncated_normal_distribution
    /// </summary>
    [Serializable]
    public class NormalSampler : ISampler
    {
        /// <summary>
        /// The mean of the normal distribution to sample from
        /// </summary>
        public float mean;

        /// <summary>
        /// The standard deviation of the normal distribution to sample from
        /// </summary>
        public float standardDeviation;

        /// <summary>
        /// A range bounding the values generated by this sampler
        /// </summary>
        [field: SerializeField]
        public FloatRange range { get; set; }

        /// <summary>
        /// Constructs a normal distribution sampler
        /// </summary>
        public NormalSampler()
        {
            range = new FloatRange(-1f, 1f);
            mean = 0;
            standardDeviation = 1;
        }

        /// <summary>
        /// Constructs a normal distribution sampler
        /// </summary>
        /// <param name="min">The smallest value contained within the range</param>
        /// <param name="max">The largest value contained within the range</param>
        /// <param name="mean">The mean of the normal distribution to sample from</param>
        /// <param name="standardDeviation">The standard deviation of the normal distribution to sample from</param>
        public NormalSampler(
            float min, float max, float mean, float standardDeviation)
        {
            range = new FloatRange(min, max);
            this.mean = mean;
            this.standardDeviation = standardDeviation;
        }

        /// <summary>
        /// Generates one sample
        /// </summary>
        /// <returns>The generated sample</returns>
        public float Sample()
        {
            var rng = new Unity.Mathematics.Random(ScenarioBase.activeScenario.NextRandomState());
            return SamplerUtility.TruncatedNormalSample(
                rng.NextFloat(), range.minimum, range.maximum, mean, standardDeviation);
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
                min = range.minimum,
                max = range.maximum,
                mean = mean,
                standardDeviation = standardDeviation,
                seed = ScenarioBase.activeScenario.NextRandomState(),
                samples = samples
            }.ScheduleBatch(sampleCount, SamplerUtility.samplingBatchSize);
            return samples;
        }

        [BurstCompile]
        struct SampleJob : IJobParallelForBatch
        {
            public float min;
            public float max;
            public float mean;
            public float standardDeviation;
            public uint seed;
            public NativeArray<float> samples;

            public void Execute(int startIndex, int count)
            {
                var endIndex = startIndex + count;
                var batchIndex = startIndex / SamplerUtility.samplingBatchSize;
                var rng = new Unity.Mathematics.Random(SamplerUtility.IterateSeed((uint)batchIndex, seed));
                for (var i = startIndex; i < endIndex; i++)
                {
                    samples[i] = SamplerUtility.TruncatedNormalSample(
                        rng.NextFloat(), min, max, mean, standardDeviation);
                }
            }
        }
    }
}
