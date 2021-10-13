﻿using ILGPU.Runtime;
using System;
using System.Linq;

namespace DataScience
{
    public partial class Vector
    {
        /// <summary>
        /// Takes the absolute value of all values in the Vector.
        /// IMPORTANT : Use this method for Vectors of Length less than 100,000
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector Abs(Vector vector)
        {
            return vector.Copy().Abs_IP();
        }
        /// <summary>
        /// Takes the absolute value of all values in this Vector.
        /// IMPORTANT : Use this method for Vectors of Length less than 100,000
        /// </summary>
        public Vector Abs_IP()
        {
            SyncCPU();

            if (this.Min() > 0f) { return this; }

            for (int i = 0; i < this.Value.Length; i++)
            {
                this.Value[i] = MathF.Abs(this.Value[i]);
            }

            UpdateCache();

            return this;
        }
        /// <summary>
        /// Runs on Accelerator. (GPU : Default)
        /// Takes the absolute value of all the values in the Vector.
        /// IMPORTANT : Use this method for Vectors of Length more than 100,000
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector AbsX(Vector vector)
        {
            return vector.Copy().AbsX_IP();
        }
        /// <summary>
        /// Runs on Accelerator. (GPU : Default)
        /// Takes the absolute value of all the values in this Vector.
        /// IMPORTANT : Use this method for Vectors of Length more than 100,000
        /// </summary>
        public Vector AbsX_IP()
        {
            // Secure data
            this.IncrementLiveCount();

            // Make space for data
            this.gpu.DeCacheLRU(this._memorySize);

            // Get the Memory buffer input/output
            MemoryBuffer<float> buffer = this.GetBuffer(); // IO

            // RUN
            this.gpu.absKernel(this.gpu.accelerator.DefaultStream, buffer.Length, buffer.View);

            // SYNC
            this.gpu.accelerator.Synchronize();

            // Remove Security
            this.DecrementLiveCount();

            // Output
            return this;
        }


    }
}
