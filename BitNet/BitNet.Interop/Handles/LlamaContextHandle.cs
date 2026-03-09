using System.Runtime.InteropServices;
using BitNet.Interop.Native;

namespace BitNet.Interop.Handles;

/// <summary>
/// SafeHandle wrapper for a llama_context pointer.
/// Automatically calls llama_free on disposal.
/// </summary>
public sealed class LlamaContextHandle : SafeHandle
{
    public LlamaContextHandle() : base(IntPtr.Zero, ownsHandle: true) { }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        NativeMethods.Free(handle);
        return true;
    }

    /// <summary>
    /// Create a new context from an existing model handle.
    /// </summary>
    public static LlamaContextHandle Create(LlamaModelHandle modelHandle, LlamaContextParams contextParams)
    {
        var ptr = NativeMethods.NewContextWithModel(modelHandle.DangerousGetHandle(), contextParams);
        if (ptr == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create llama context.");

        var ctxHandle = new LlamaContextHandle();
        ctxHandle.SetHandle(ptr);
        return ctxHandle;
    }
}
