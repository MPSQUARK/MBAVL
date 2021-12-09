﻿using System;
using BenchmarkDotNet.Running;

using BAVCL;
using BAVCL.Core;
using BAVCL.Geometric;
using System.Threading.Tasks;

//using BAVCL.Geometric;
//using ILGPU.Algorithms;
//using BAVCL.Plotting;

GPU gpu = new();

Vector vector = new (gpu, new float[2] { 1f,2f});

vector.UpdateCache(new float[] { 6f,7f});

