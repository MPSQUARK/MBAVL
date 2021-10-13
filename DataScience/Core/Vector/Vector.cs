﻿using ILGPU;
using ILGPU.Runtime;
using ILGPU.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DataScience.Utility;

namespace DataScience
{

    /// <summary>
    /// Class for 1D and 2D Vector support
    /// Float Precision
    /// </summary>
    public partial class Vector : VectorBase<float>
    {

        // CONSTRUCTOR
        /// <summary>
        /// Constructs a Vector class object.
        /// </summary>
        /// <param name="gpu">The device to use when computing this Vector.</param>
        /// <param name="values">The array of data contained in this Vector.</param>
        /// <param name="columns">The number of Columns IF this is a 2D Vector, for 1D Vectors use the default Columns = 1</param>
        public Vector(GPU gpu, float[] values, int columns = 1, bool cache = true) : base(gpu, values, columns, cache)
        {
        }

        // METHODS
        public bool Equals(Vector vector)
        {
            if (this.Value.Length != vector.Value.Length)
            {
                return false;
            }

            for (int i = 0; i < vector.Value.Length; i++)
            {
                if (this.Value[i] != vector.Value[i])
                {
                    return false;
                }
            }

            return true;
        }
        public Vector Copy(bool Cache = true)
        {
            if (this._id == 0)
            {
                return new Vector(this.gpu, this.Value[..], this.Columns, Cache);
            }

            return new Vector(this.gpu, this.Pull(), this.Columns, Cache);
        }


        #region "MATHEMATICAL PROPERTIES "
        public override float Mean()
        {
            return this.Sum() / this.Length;
        }
        public float Std()
        {
            return XMath.Sqrt(this.Var());
        }
        public float Var()
        {
            if (this.Length < 1e4f)
            {
                int vectorSize = System.Numerics.Vector<float>.Count;
                int i = 0;

                float[] array = this.Value;

                float mean = this.Mean();

                System.Numerics.Vector<float> meanvec = new System.Numerics.Vector<float>(mean);

                System.Numerics.Vector<float> sumVector = System.Numerics.Vector<float>.Zero;

                for (; i <= array.Length - vectorSize; i += vectorSize)
                {
                    System.Numerics.Vector<float> input = new System.Numerics.Vector<float>(array, i);

                    System.Numerics.Vector<float> difference = input - meanvec;

                    sumVector += (difference * difference);

                }

                float sum = 0;

                for (int j = 0; j < vectorSize; j++)
                {
                    sum += sumVector[j];
                }

                for (; i < array.Length; i++)
                {
                    sum += XMath.Pow((array[i] - mean), 2f);
                }

                return sum / this.Length;
            }

            //Vector diff = Vector.AbsX(this - this.Mean());

            //return (diff * diff).Sum() / this.Length();
            return Vector.ConsecutiveOP(this, this.Mean(), Operations.squareOfDiffs).Sum() / this.Length;
        }
        public override float Range()
        {
            return this.Value.Max() - this.Value.Min();
        }
        public override float Sum()
        {
            int vectorSize = System.Numerics.Vector<float>.Count;
            int i = 0;
            float[] array = this.Value;

            System.Numerics.Vector<float> sumVector = System.Numerics.Vector<float>.Zero;

            if (array.Length >= 1e4f)
            {
                System.Numerics.Vector<float> c = System.Numerics.Vector<float>.Zero;
                for (; i <= array.Length - vectorSize; i += vectorSize)
                {

                    System.Numerics.Vector<float> input = new System.Numerics.Vector<float>(array, i);

                    System.Numerics.Vector<float> y = input - c;

                    System.Numerics.Vector<float> t = sumVector + y;

                    c = (t - sumVector) - y;

                    sumVector = t;
                }
            }
            else
            {
                for (; i <= array.Length - vectorSize; i += vectorSize)
                {
                    System.Numerics.Vector<float> v = new System.Numerics.Vector<float>(array, i);

                    sumVector = System.Numerics.Vector.Add(sumVector, v);
                }
            }
            
            float result = 0;
            for (int j = 0; j < vectorSize; j++)
            {
                result += sumVector[j];
            }

            for (; i < array.Length; i++)
            {
                result += array[i];
            }
            return result;
        }
        public void Flatten()
        {
            this.Columns = 1;
        }

        #endregion


        #region "CONVERSION"

        public Geometric.Vector3 ToVector3()
        {
            if (this.Length % 3 != 0) { throw new Exception("Vector length must be a multiple of 3"); }
            return new Geometric.Vector3(this.gpu, this.Value);
        }

        #endregion


        #region "OPERATORS"
        public static Vector operator +(Vector vector)
        {
            return Vector.AbsX(vector);
        }



        public static Vector operator +(Vector vectorA, Vector vectorB)
        {
            return Vector.ConsecutiveOP(vectorA, vectorB, Operations.addition);
        }
        public static Vector operator +(Vector vector, float Scalar)
        {
            return Vector.ConsecutiveOP(vector, Scalar, Operations.addition);
        }
        public static Vector operator +(float Scalar, Vector vector)
        {
            return Vector.ConsecutiveOP(vector, Scalar, Operations.addition);
        }



        public static Vector operator -(Vector vector)
        {
            return Vector.ConsecutiveOP(vector, -1, Operations.multiplication);
        }



        public static Vector operator -(Vector vectorA, Vector vectorB)
        {
            return Vector.ConsecutiveOP(vectorA, vectorB, Operations.subtraction);
        }
        public static Vector operator -(Vector vector, float Scalar)
        {
            return Vector.ConsecutiveOP(vector, Scalar, Operations.subtraction);
        }

        public static Vector operator -(float Scalar, Vector vector)
        {
            return Vector.ConsecutiveOP(vector, Scalar, Operations.flipSubtraction);
        }



        public static Vector operator *(Vector vectorA, Vector vectorB)
        {
            return Vector.ConsecutiveOP(vectorA, vectorB, Operations.multiplication);
        }
        public static Vector operator *(Vector vector, float Scalar)
        {
            return Vector.ConsecutiveOP(vector, Scalar, Operations.multiplication);
        }
        public static Vector operator *(float Scalar, Vector Vector)
        {
            return Vector.ConsecutiveOP(Vector, Scalar, Operations.multiplication);
        }



        public static Vector operator /(Vector vectorA, Vector vectorB)
        {
            return Vector.ConsecutiveOP(vectorA, vectorB, Operations.division);
        }
        public static Vector operator /(Vector vector, float Scalar)
        {
            return Vector.ConsecutiveOP(vector, Scalar, Operations.division);
        }
        public static Vector operator /(float Scalar, Vector vector)
        {
            return Vector.ConsecutiveOP(vector, Scalar, Operations.inverseDivision);
        }



        public static Vector operator ^(Vector vectorA, Vector vectorB)
        {
            return Vector.ConsecutiveOP(vectorA, vectorB, Operations.power);
        }
        public static Vector operator ^(Vector vector, float Scalar)
        {
            return Vector.ConsecutiveOP(vector, Scalar, Operations.power);
        }
        public static Vector operator ^(float Scalar, Vector vector)
        {
            return Vector.ConsecutiveOP(vector, Scalar, Operations.powerFlipped);
        }






        #endregion



        // FUNCTIONS
        public static Vector ConsecutiveOP(Vector vectorA, Vector vectorB, Operations operation, bool Warp = false)
        {
            // Check function conditions
            if (vectorA.Value.Length == vectorB.Value.Length)
            {
                return _VectorVectorOP(vectorA, vectorB, operation);
            }

            bool ThisLonger = vectorA.Value.Length > vectorB.Value.Length;


            // If one input is a Vector and other is Matrix
            if ((vectorA.Columns == 1 && vectorB.Columns > 1) || (vectorA.Columns > 1 && vectorB.Columns == 1))
            {
                if (ThisLonger) { return _VectorMatrixOP(vectorB, vectorA, operation); }
                return _VectorMatrixOP(vectorA, vectorB, operation);
            }


            throw new IndexOutOfRangeException("Vector A and Vector B provided MUST be of EQUAL length");
        }
        public void ConsecutiveOP_IP(Vector vectorB, Operations operation)
        {
            // If the lengths are the same and both 1D vectors
            if (this.Value.Length == vectorB.Value.Length && vectorB.Columns == 1 && this.Columns == 1)
            {
                this._VectorVectorOP_IP(vectorB, operation);
                return;
            }

            bool ThisLonger = this.Value.Length > vectorB.Value.Length;

            // If one input is a Vector and other is Matrix
            if ((this.Columns == 1 && vectorB.Columns > 1) || (this.Columns > 1 && vectorB.Columns == 1))
            {
                
                
            }

            throw new IndexOutOfRangeException("Vector A and Vector B provided MUST be of EQUAL length");
        }

        public static Vector ConsecutiveOP(Vector vector, float scalar, Operations operation)
        {
            // Ensure there is enough space for all the data
            long size = vector._memorySize << 1;

            vector.IncrementLiveCount();

            vector.gpu.DeCacheLRU(size);

            // Make the Output Vector
            Vector Output = new Vector(vector.gpu, new float[vector.Value.Length], vector.Columns);

            // Check if the input & output are in Cache
            MemoryBuffer<float> buffer = Output.GetBuffer(); // Output
            MemoryBuffer<float> buffer2 = vector.GetBuffer(); // Input

            vector.gpu.scalarConsecOpKernel(vector.gpu.accelerator.DefaultStream, buffer.Length, buffer.View, buffer2.View, scalar, new SpecializedValue<int>((int)operation));

            vector.gpu.accelerator.Synchronize();

            buffer.CopyTo(Output.Value, 0, 0, Output.Value.Length);

            vector.DecrementLiveCount();

            return Output;
        }

        public void ConsecutiveOP_IP(float scalar, Operations operation)
        {
            // Check if the input & output are in Cache
            MemoryBuffer<float> buffer = this.GetBuffer(); // IO

            gpu.scalarConsecOpKernelIP(this.gpu.accelerator.DefaultStream, buffer.Length, buffer.View, scalar, new SpecializedValue<int>((int)operation));

            gpu.accelerator.Synchronize();

            buffer.CopyTo(this.Value, 0, 0, this.Value.Length);

            return;
        }


        internal static Vector _VectorVectorOP(Vector vectorA, Vector vectorB, Operations operation)
        {
            // Ensure there is enough space for all the data
            long size = vectorA._memorySize * 3;

            vectorA.IncrementLiveCount();
            vectorB.IncrementLiveCount();
            vectorA.gpu.DeCacheLRU(size);

            // Make the Output Vector
            Vector Output = new Vector(vectorA.gpu, new float[vectorA.Value.Length], vectorA.Columns);

            // Check if the input & output are in Cache
            MemoryBuffer<float> buffer = Output.GetBuffer(); // Output
            MemoryBuffer<float> buffer2 = vectorA.GetBuffer(); // Input
            MemoryBuffer<float> buffer3 = vectorB.GetBuffer(); // Input

            // Run the kernel
            vectorA.gpu.consecOpKernel(vectorA.gpu.accelerator.DefaultStream, buffer.Length, buffer.View, buffer2.View, buffer3.View, new SpecializedValue<int>((int)operation));

            // Synchronise the kernel
            vectorA.gpu.accelerator.Synchronize();

            // Copy output
            buffer.CopyTo(Output.Value, 0, 0, Output.Value.Length);

            vectorA.DecrementLiveCount();
            vectorB.DecrementLiveCount();

            // Return the result
            return Output;
        }

        internal void _VectorVectorOP_IP(Vector vectorB, Operations operation)
        {
            // Ensure there is enough space for all the data
            long size = this._memorySize << 1;

            vectorB.IncrementLiveCount();
            this.IncrementLiveCount();
            this.gpu.DeCacheLRU(size);

            // Check if the input & output are in Cache
            MemoryBuffer<float> buffer = this.GetBuffer(); // Input/Output
            MemoryBuffer<float> buffer2 = vectorB.GetBuffer(); // Input

            // Run the kernel
            this.gpu.consecOpKernelIP(this.gpu.accelerator.DefaultStream, buffer.Length, buffer.View, buffer2.View, new SpecializedValue<int>((int)operation));

            // Synchronise the kernel
            this.gpu.accelerator.Synchronize();

            // Copy output
            buffer.CopyTo(this.Value, 0, 0, this.Length);

            vectorB.DecrementLiveCount();
            this.DecrementLiveCount();

            return;
        }

        internal static Vector _VectorMatrixOP(Vector vector, Vector matrix, Operations operation)
        {
            // Ensure there is enough space for all the data
            long size = (vector._memorySize << 2) + matrix._memorySize;

            vector.IncrementLiveCount();
            matrix.IncrementLiveCount();
            vector.gpu.DeCacheLRU(size);

            // Make the Output Vector
            Vector Output = new Vector(vector.gpu, new float[vector.Value.Length], vector.Columns);

            // Check if the input & output are in Cache
            MemoryBuffer<float> buffer = Output.GetBuffer(); // Output
            MemoryBuffer<float> buffer2 = vector.GetBuffer(); // Input
            MemoryBuffer<float> buffer3 = matrix.GetBuffer(); // Input

            // Run the kernel
            vector.gpu.vectormatrixOpKernel(vector.gpu.accelerator.DefaultStream, matrix.RowCount(), buffer.View, buffer2.View, buffer3.View, matrix.Columns, new SpecializedValue<int>((int)operation));

            // Synchronise the kernel
            vector.gpu.accelerator.Synchronize();

            // Copy output
            buffer.CopyTo(Output.Value, 0, 0, Output.Value.Length);

            vector.DecrementLiveCount();
            matrix.DecrementLiveCount();

            // Return the result
            return Output;
        }

        internal void _VectorMatrixOP_IP(Vector matrix, Operations operation)
        {
            // Ensure there is enough space for all the data
            long size = (this._memorySize << 2) + matrix._memorySize;

            this.IncrementLiveCount();
            matrix.IncrementLiveCount();
            this.gpu.DeCacheLRU(size);

            // Make the Output Vector
            Vector Output = new Vector(this.gpu, new float[this.Value.Length], this.Columns);

            // Check if the input & output are in Cache
            MemoryBuffer<float> buffer = Output.GetBuffer(); // Output
            MemoryBuffer<float> buffer2 = this.GetBuffer(); // Input
            MemoryBuffer<float> buffer3 = matrix.GetBuffer(); // Input

            // Run the kernel
            this.gpu.vectormatrixOpKernel(this.gpu.accelerator.DefaultStream, matrix.RowCount(), buffer.View, buffer2.View, buffer3.View, matrix.Columns, new SpecializedValue<int>((int)operation));

            // Synchronise the kernel
            this.gpu.accelerator.Synchronize();

            // Copy output
            buffer.CopyTo(Output.Value, 0, 0, Output.Value.Length);

            this.DecrementLiveCount();
            matrix.DecrementLiveCount();

            return;
        }


       

    }



}