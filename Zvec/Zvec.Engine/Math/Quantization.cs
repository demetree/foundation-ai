// Copyright 2025-present the zvec project — Pure C# Engine
// Vector quantization: FP16, INT8, INT4

namespace Foundation.AI.Zvec.Engine.Math;

/// <summary>
/// Vector quantization utilities for reducing memory footprint.
/// Supports FP16, INT8, and INT4 with automatic min/max calibration.
/// </summary>
public static class Quantization
{
    // ═══════════════════════════════════════════════════════════════════
    // FP16 (Half-precision): lossless within Half range
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>Compress FP32 → FP16 (2 bytes per component).</summary>
    public static Half[] ToFP16(ReadOnlySpan<float> vector)
    {
        var result = new Half[vector.Length];
        for (int i = 0; i < vector.Length; i++)
            result[i] = (Half)vector[i];
        return result;
    }

    /// <summary>Decompress FP16 → FP32.</summary>
    public static float[] FromFP16(ReadOnlySpan<Half> vector)
    {
        var result = new float[vector.Length];
        for (int i = 0; i < vector.Length; i++)
            result[i] = (float)vector[i];
        return result;
    }

    /// <summary>Compute distance between FP16 vectors by decompressing on the fly.</summary>
    public static float DistanceFP16(
        ReadOnlySpan<Half> a, ReadOnlySpan<Half> b,
        Func<ReadOnlySpan<float>, ReadOnlySpan<float>, float> distFunc)
    {
        // Decompress to temporary buffers
        Span<float> fa = stackalloc float[a.Length];
        Span<float> fb = stackalloc float[b.Length];
        for (int i = 0; i < a.Length; i++) fa[i] = (float)a[i];
        for (int i = 0; i < b.Length; i++) fb[i] = (float)b[i];
        return distFunc(fa, fb);
    }

    // ═══════════════════════════════════════════════════════════════════
    // INT8: uniform scalar quantization (1 byte per component)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calibration parameters for INT8 quantization.
    /// Maps [minVal, maxVal] → [0, 255].
    /// </summary>
    public readonly record struct Int8Calibration(float MinVal, float MaxVal)
    {
        public float Scale => (MaxVal - MinVal) / 255f;
    }

    /// <summary>Calibrate INT8 by scanning the vector for min/max.</summary>
    public static Int8Calibration CalibrateInt8(ReadOnlySpan<float> vector)
    {
        float min = float.MaxValue, max = float.MinValue;
        for (int i = 0; i < vector.Length; i++)
        {
            if (vector[i] < min) min = vector[i];
            if (vector[i] > max) max = vector[i];
        }
        if (System.Math.Abs(max - min) < 1e-12f) max = min + 1e-6f;
        return new Int8Calibration(min, max);
    }

    /// <summary>Calibrate INT8 across a batch of vectors (global calibration).</summary>
    public static Int8Calibration CalibrateInt8Batch(IEnumerable<float[]> vectors)
    {
        float min = float.MaxValue, max = float.MinValue;
        foreach (var vec in vectors)
        {
            for (int i = 0; i < vec.Length; i++)
            {
                if (vec[i] < min) min = vec[i];
                if (vec[i] > max) max = vec[i];
            }
        }
        if (System.Math.Abs(max - min) < 1e-12f) max = min + 1e-6f;
        return new Int8Calibration(min, max);
    }

    /// <summary>Compress FP32 → INT8 using given calibration.</summary>
    public static byte[] ToInt8(ReadOnlySpan<float> vector, Int8Calibration cal)
    {
        var result = new byte[vector.Length];
        float invScale = 255f / (cal.MaxVal - cal.MinVal);
        for (int i = 0; i < vector.Length; i++)
        {
            float normalized = (vector[i] - cal.MinVal) * invScale;
            result[i] = (byte)System.Math.Clamp(normalized + 0.5f, 0, 255);
        }
        return result;
    }

    /// <summary>Decompress INT8 → FP32.</summary>
    public static float[] FromInt8(ReadOnlySpan<byte> quantized, Int8Calibration cal)
    {
        var result = new float[quantized.Length];
        float scale = (cal.MaxVal - cal.MinVal) / 255f;
        for (int i = 0; i < quantized.Length; i++)
            result[i] = quantized[i] * scale + cal.MinVal;
        return result;
    }

    // ═══════════════════════════════════════════════════════════════════
    // INT4: packed nibble quantization (0.5 bytes per component)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calibration parameters for INT4 quantization.
    /// Maps [minVal, maxVal] → [0, 15].
    /// </summary>
    public readonly record struct Int4Calibration(float MinVal, float MaxVal)
    {
        public float Scale => (MaxVal - MinVal) / 15f;
    }

    /// <summary>Calibrate INT4 by scanning the vector for min/max.</summary>
    public static Int4Calibration CalibrateInt4(ReadOnlySpan<float> vector)
    {
        float min = float.MaxValue, max = float.MinValue;
        for (int i = 0; i < vector.Length; i++)
        {
            if (vector[i] < min) min = vector[i];
            if (vector[i] > max) max = vector[i];
        }
        if (System.Math.Abs(max - min) < 1e-12f) max = min + 1e-6f;
        return new Int4Calibration(min, max);
    }

    /// <summary>Calibrate INT4 across a batch of vectors.</summary>
    public static Int4Calibration CalibrateInt4Batch(IEnumerable<float[]> vectors)
    {
        float min = float.MaxValue, max = float.MinValue;
        foreach (var vec in vectors)
        {
            for (int i = 0; i < vec.Length; i++)
            {
                if (vec[i] < min) min = vec[i];
                if (vec[i] > max) max = vec[i];
            }
        }
        if (System.Math.Abs(max - min) < 1e-12f) max = min + 1e-6f;
        return new Int4Calibration(min, max);
    }

    /// <summary>
    /// Compress FP32 → INT4 (packed nibbles, 2 values per byte).
    /// Output length = ceil(vector.Length / 2).
    /// </summary>
    public static byte[] ToInt4(ReadOnlySpan<float> vector, Int4Calibration cal)
    {
        int packedLen = (vector.Length + 1) / 2;
        var result = new byte[packedLen];
        float invScale = 15f / (cal.MaxVal - cal.MinVal);

        for (int i = 0; i < vector.Length; i++)
        {
            float normalized = (vector[i] - cal.MinVal) * invScale;
            byte nibble = (byte)System.Math.Clamp(normalized + 0.5f, 0, 15);

            int byteIdx = i / 2;
            if (i % 2 == 0)
                result[byteIdx] = (byte)(nibble << 4); // high nibble
            else
                result[byteIdx] |= nibble;             // low nibble
        }
        return result;
    }

    /// <summary>
    /// Decompress INT4 → FP32.
    /// </summary>
    /// <param name="packedLen">Number of original float components (not byte length).</param>
    public static float[] FromInt4(ReadOnlySpan<byte> packed, int originalLength, Int4Calibration cal)
    {
        var result = new float[originalLength];
        float scale = (cal.MaxVal - cal.MinVal) / 15f;

        for (int i = 0; i < originalLength; i++)
        {
            int byteIdx = i / 2;
            byte nibble = (i % 2 == 0)
                ? (byte)(packed[byteIdx] >> 4)     // high nibble
                : (byte)(packed[byteIdx] & 0x0F);  // low nibble

            result[i] = nibble * scale + cal.MinVal;
        }
        return result;
    }

    // ═══════════════════════════════════════════════════════════════════
    // Utility: Memory savings calculator
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculate memory usage for a given quantization type and dimension.
    /// </summary>
    public static (long fp32Bytes, long quantizedBytes, double ratio) MemoryStats(
        int dimension, long vectorCount, Core.QuantizeType quantize)
    {
        long fp32 = (long)dimension * vectorCount * 4; // 4 bytes per float

        long quantized = quantize switch
        {
            Core.QuantizeType.FP16 => (long)dimension * vectorCount * 2,
            Core.QuantizeType.Int8 => (long)dimension * vectorCount * 1,
            Core.QuantizeType.Int4 => (long)((dimension + 1) / 2) * vectorCount,
            _ => fp32
        };

        double ratio = fp32 > 0 ? (double)quantized / fp32 : 1.0;
        return (fp32, quantized, ratio);
    }
}
