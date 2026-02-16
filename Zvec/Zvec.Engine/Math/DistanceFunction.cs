// Copyright 2025-present the zvec project — Pure C# Engine
// SIMD-accelerated distance functions

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Foundation.AI.Zvec.Engine.Math;

/// <summary>
/// Distance metric types matching the public Zvec SDK enum.
/// </summary>
public enum MetricType
{
    Undefined = 0,
    L2 = 1,
    InnerProduct = 2,
    Cosine = 3,
}

/// <summary>
/// Factory for creating distance functions based on metric type.
/// </summary>
public static class DistanceFunction
{
    /// <summary>
    /// Get a distance function for the specified metric type.
    /// Lower return values = more similar for L2.
    /// Higher return values = more similar for IP and Cosine.
    /// </summary>
    public static Func<ReadOnlySpan<float>, ReadOnlySpan<float>, float> Get(MetricType metric)
    {
        return metric switch
        {
            MetricType.L2 => SimdDistance.EuclideanSquared,
            MetricType.InnerProduct => SimdDistance.InnerProduct,
            MetricType.Cosine => SimdDistance.CosineDistance,
            _ => throw new ArgumentException($"Unsupported metric type: {metric}")
        };
    }

    /// <summary>
    /// Returns true if lower distance values mean "more similar" for this metric.
    /// </summary>
    public static bool IsLowerBetter(MetricType metric)
    {
        return metric switch
        {
            MetricType.L2 => true,
            MetricType.InnerProduct => false, // higher IP = more similar
            MetricType.Cosine => true,        // cosine distance: 0 = identical
            _ => true
        };
    }
}

/// <summary>
/// SIMD-accelerated distance computations using System.Runtime.Intrinsics.
/// Automatically selects AVX2, SSE, or scalar fallback.
/// </summary>
public static class SimdDistance
{
    /// <summary>
    /// Squared Euclidean distance: sum((a[i] - b[i])^2).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static float EuclideanSquared(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        int length = System.Math.Min(a.Length, b.Length);
        float sum = 0f;
        int i = 0;

        if (Avx.IsSupported && length >= 8)
        {
            var accumulator = Vector256<float>.Zero;
            ref float refA = ref MemoryMarshal.GetReference(a);
            ref float refB = ref MemoryMarshal.GetReference(b);

            for (; i <= length - 8; i += 8)
            {
                var va = Vector256.LoadUnsafe(ref Unsafe.Add(ref refA, i));
                var vb = Vector256.LoadUnsafe(ref Unsafe.Add(ref refB, i));
                var diff = Avx.Subtract(va, vb);
                accumulator = SimdFma(accumulator, diff, diff);
            }

            sum = Vector256.Sum(accumulator);
        }
        else if (Sse.IsSupported && length >= 4)
        {
            var accumulator = Vector128<float>.Zero;
            ref float refA = ref MemoryMarshal.GetReference(a);
            ref float refB = ref MemoryMarshal.GetReference(b);

            for (; i <= length - 4; i += 4)
            {
                var va = Vector128.LoadUnsafe(ref Unsafe.Add(ref refA, i));
                var vb = Vector128.LoadUnsafe(ref Unsafe.Add(ref refB, i));
                var diff = Sse.Subtract(va, vb);
                accumulator = Sse.Add(accumulator, Sse.Multiply(diff, diff));
            }

            sum = Vector128.Sum(accumulator);
        }

        // Scalar tail
        for (; i < length; i++)
        {
            float d = a[i] - b[i];
            sum += d * d;
        }

        return sum;
    }

    /// <summary>
    /// Inner product (dot product): sum(a[i] * b[i]).
    /// Higher values = more similar.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static float InnerProduct(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        int length = System.Math.Min(a.Length, b.Length);
        float sum = 0f;
        int i = 0;

        if (Avx.IsSupported && length >= 8)
        {
            var accumulator = Vector256<float>.Zero;
            ref float refA = ref MemoryMarshal.GetReference(a);
            ref float refB = ref MemoryMarshal.GetReference(b);

            for (; i <= length - 8; i += 8)
            {
                var va = Vector256.LoadUnsafe(ref Unsafe.Add(ref refA, i));
                var vb = Vector256.LoadUnsafe(ref Unsafe.Add(ref refB, i));
                accumulator = SimdFma(accumulator, va, vb);
            }

            sum = Vector256.Sum(accumulator);
        }
        else if (Sse.IsSupported && length >= 4)
        {
            var accumulator = Vector128<float>.Zero;
            ref float refA = ref MemoryMarshal.GetReference(a);
            ref float refB = ref MemoryMarshal.GetReference(b);

            for (; i <= length - 4; i += 4)
            {
                var va = Vector128.LoadUnsafe(ref Unsafe.Add(ref refA, i));
                var vb = Vector128.LoadUnsafe(ref Unsafe.Add(ref refB, i));
                accumulator = Sse.Add(accumulator, Sse.Multiply(va, vb));
            }

            sum = Vector128.Sum(accumulator);
        }

        // Scalar tail
        for (; i < length; i++)
        {
            sum += a[i] * b[i];
        }

        return sum;
    }

    /// <summary>
    /// Cosine distance: 1 - cosine_similarity(a, b).
    /// Returns 0 for identical vectors, 2 for opposite vectors.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static float CosineDistance(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
    {
        int length = System.Math.Min(a.Length, b.Length);
        float dot = 0f, normA = 0f, normB = 0f;
        int i = 0;

        if (Avx.IsSupported && length >= 8)
        {
            var accDot = Vector256<float>.Zero;
            var accA = Vector256<float>.Zero;
            var accB = Vector256<float>.Zero;
            ref float refA = ref MemoryMarshal.GetReference(a);
            ref float refB = ref MemoryMarshal.GetReference(b);

            for (; i <= length - 8; i += 8)
            {
                var va = Vector256.LoadUnsafe(ref Unsafe.Add(ref refA, i));
                var vb = Vector256.LoadUnsafe(ref Unsafe.Add(ref refB, i));
                accDot = SimdFma(accDot, va, vb);
                accA = SimdFma(accA, va, va);
                accB = SimdFma(accB, vb, vb);
            }

            dot = Vector256.Sum(accDot);
            normA = Vector256.Sum(accA);
            normB = Vector256.Sum(accB);
        }
        else if (Sse.IsSupported && length >= 4)
        {
            var accDot = Vector128<float>.Zero;
            var accA = Vector128<float>.Zero;
            var accB = Vector128<float>.Zero;
            ref float refA = ref MemoryMarshal.GetReference(a);
            ref float refB = ref MemoryMarshal.GetReference(b);

            for (; i <= length - 4; i += 4)
            {
                var va = Vector128.LoadUnsafe(ref Unsafe.Add(ref refA, i));
                var vb = Vector128.LoadUnsafe(ref Unsafe.Add(ref refB, i));
                accDot = Sse.Add(accDot, Sse.Multiply(va, vb));
                accA = Sse.Add(accA, Sse.Multiply(va, va));
                accB = Sse.Add(accB, Sse.Multiply(vb, vb));
            }

            dot = Vector128.Sum(accDot);
            normA = Vector128.Sum(accA);
            normB = Vector128.Sum(accB);
        }

        // Scalar tail
        for (; i < length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        float denom = MathF.Sqrt(normA) * MathF.Sqrt(normB);
        return denom < 1e-10f ? 1f : 1f - (dot / denom);
    }

    /// <summary>
    /// FMA (Fused Multiply–Add) helper. Uses FMA instruction if available, otherwise multiply + add.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<float> SimdFma(
        Vector256<float> acc, Vector256<float> a, Vector256<float> b)
    {
        if (Fma.IsSupported)
            return Fma.MultiplyAdd(a, b, acc);
        return Avx.Add(acc, Avx.Multiply(a, b));
    }
}
