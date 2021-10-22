﻿using ILGPU.Runtime;

namespace DataScience
{
    public partial class Vector
    {
        public static Vector Reciprocal(Vector vector)
        {
            return vector.Copy().Reciprocal_IP();
        }
        public Vector Reciprocal_IP()
        {
            IncrementLiveCount();

            // Check if the input & output are in Cache
            MemoryBuffer<float> buffer = GetBuffer(); // IO

            gpu.reciprocalKernel(gpu.accelerator.DefaultStream, buffer.Length, buffer.View);

            gpu.accelerator.Synchronize();

            DecrementLiveCount();

            return this;
        }

    }
}
