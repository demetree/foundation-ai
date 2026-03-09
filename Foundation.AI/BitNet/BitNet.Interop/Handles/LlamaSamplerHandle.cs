using System.Runtime.InteropServices;
using BitNet.Interop.Native;

namespace BitNet.Interop.Handles;

/// <summary>
/// SafeHandle wrapper for a llama_sampler pointer (typically a sampler chain).
/// Automatically calls llama_sampler_free on disposal.
/// </summary>
public sealed class LlamaSamplerHandle : SafeHandle
{
    public LlamaSamplerHandle() : base(IntPtr.Zero, ownsHandle: true) { }

    public override bool IsInvalid => handle == IntPtr.Zero;

    protected override bool ReleaseHandle()
    {
        NativeMethods.SamplerFree(handle);
        return true;
    }

    /// <summary>
    /// Initialize a sampler chain with default or specified parameters.
    /// </summary>
    public static LlamaSamplerHandle CreateChain(LlamaSamplerChainParams chainParams)
    {
        var ptr = NativeMethods.SamplerChainInit(chainParams);
        if (ptr == IntPtr.Zero)
            throw new InvalidOperationException("Failed to create sampler chain.");

        var smplHandle = new LlamaSamplerHandle();
        smplHandle.SetHandle(ptr);
        return smplHandle;
    }
}
