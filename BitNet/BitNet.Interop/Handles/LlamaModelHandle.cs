using System.Runtime.InteropServices;
using BitNet.Interop.Native;

namespace BitNet.Interop.Handles;

/// <summary>
/// SafeHandle wrapper for a llama_model pointer.
/// Automatically calls llama_free_model on disposal.
/// </summary>
public sealed class LlamaModelHandle : SafeHandle
{
    public LlamaModelHandle() : base(IntPtr.Zero, ownsHandle: true) { }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        NativeMethods.FreeModel(handle);
        return true;
    }

    /// <summary>
    /// Load a model from a GGUF file.
    /// </summary>
    public static LlamaModelHandle LoadFromFile(string path, LlamaModelParams modelParams)
    {
        var ptr = NativeMethods.LoadModelFromFile(path, modelParams);
        if (ptr == IntPtr.Zero)
            throw new InvalidOperationException($"Failed to load model from '{path}'.");

        var handle = new LlamaModelHandle();
        handle.SetHandle(ptr);
        return handle;
    }
}
