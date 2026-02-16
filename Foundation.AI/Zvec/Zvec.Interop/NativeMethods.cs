// Raw P/Invoke declarations matching zvec_capi.h
using System.Runtime.InteropServices;

namespace Foundation.AI.Zvec.Interop;

internal static partial class NativeMethods
{
    private const string LibName = "zvec_csharp";

    // =========================================================================
    // Error / String Management
    // =========================================================================

    [LibraryImport(LibName)]
    internal static partial int zvec_last_error_code();

    [LibraryImport(LibName)]
    internal static partial IntPtr zvec_last_error_message();

    [LibraryImport(LibName)]
    internal static partial void zvec_free_string(IntPtr str);

    // =========================================================================
    // Schema
    // =========================================================================

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr zvec_schema_create(string name);

    [LibraryImport(LibName)]
    internal static partial void zvec_schema_destroy(IntPtr handle);

    [LibraryImport(LibName)]
    internal static partial int zvec_schema_add_field(IntPtr schema, IntPtr field);

    [LibraryImport(LibName)]
    internal static partial int zvec_schema_set_max_doc_count_per_segment(
        IntPtr schema, ulong count);

    // =========================================================================
    // Field Schema
    // =========================================================================

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr zvec_field_schema_create(
        string name, int dataType, int nullable);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr zvec_field_schema_create_with_index(
        string name, int dataType, int nullable, IntPtr indexParams);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr zvec_field_schema_create_vector(
        string name, int dataType, uint dimension, int nullable,
        IntPtr indexParams);

    [LibraryImport(LibName)]
    internal static partial void zvec_field_schema_destroy(IntPtr handle);

    // =========================================================================
    // Index Params
    // =========================================================================

    [LibraryImport(LibName)]
    internal static partial IntPtr zvec_index_params_create_hnsw(
        int metricType, int m, int efConstruction, int quantizeType);

    [LibraryImport(LibName)]
    internal static partial IntPtr zvec_index_params_create_flat(
        int metricType, int quantizeType);

    [LibraryImport(LibName)]
    internal static partial IntPtr zvec_index_params_create_ivf(
        int metricType, int nList, int nIters, int quantizeType);

    [LibraryImport(LibName)]
    internal static partial IntPtr zvec_index_params_create_invert(
        int enableRangeOptimization);

    [LibraryImport(LibName)]
    internal static partial void zvec_index_params_destroy(IntPtr handle);

    // =========================================================================
    // Query Params
    // =========================================================================

    [LibraryImport(LibName)]
    internal static partial IntPtr zvec_query_params_create_hnsw(
        int ef, float radius, int useLinearSearch);

    [LibraryImport(LibName)]
    internal static partial IntPtr zvec_query_params_create_flat(float radius);

    [LibraryImport(LibName)]
    internal static partial IntPtr zvec_query_params_create_ivf(
        int nprobe, float radius);

    [LibraryImport(LibName)]
    internal static partial void zvec_query_params_destroy(IntPtr handle);

    // =========================================================================
    // Collection Lifecycle
    // =========================================================================

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int zvec_collection_create_and_open(
        string path, IntPtr schema, int readOnly, int enableMmap,
        uint maxBufferSize, out IntPtr outHandle);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int zvec_collection_open(
        string path, int readOnly, int enableMmap, uint maxBufferSize,
        out IntPtr outHandle);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int zvec_collection_destroy_path(string path);

    [LibraryImport(LibName)]
    internal static partial void zvec_collection_close(IntPtr handle);

    [LibraryImport(LibName)]
    internal static partial int zvec_collection_flush(IntPtr handle);

    // =========================================================================
    // Collection Operations
    // =========================================================================

    [LibraryImport(LibName)]
    internal static partial int zvec_collection_stats_doc_count(
        IntPtr handle, out ulong count);

    [LibraryImport(LibName)]
    internal static partial int zvec_collection_optimize(
        IntPtr handle, int concurrency);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int zvec_collection_create_index(
        IntPtr handle, string columnName, IntPtr indexParams, int concurrency);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int zvec_collection_drop_index(
        IntPtr handle, string columnName);

    [LibraryImport(LibName)]
    internal static partial IntPtr zvec_collection_path(IntPtr handle);

    // =========================================================================
    // Document
    // =========================================================================

    [LibraryImport(LibName)]
    internal static partial IntPtr zvec_doc_create();

    [LibraryImport(LibName)]
    internal static partial void zvec_doc_destroy(IntPtr handle);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void zvec_doc_set_pk(IntPtr handle, string pk);

    [LibraryImport(LibName)]
    internal static partial IntPtr zvec_doc_get_pk(IntPtr handle);

    [LibraryImport(LibName)]
    internal static partial float zvec_doc_get_score(IntPtr handle);

    [LibraryImport(LibName)]
    internal static partial ulong zvec_doc_get_doc_id(IntPtr handle);

    // --- Per-type field setters ---

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void zvec_doc_set_bool(
        IntPtr h, string field, int val);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void zvec_doc_set_int32(
        IntPtr h, string field, int val);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void zvec_doc_set_uint32(
        IntPtr h, string field, uint val);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void zvec_doc_set_int64(
        IntPtr h, string field, long val);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void zvec_doc_set_uint64(
        IntPtr h, string field, ulong val);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void zvec_doc_set_float(
        IntPtr h, string field, float val);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void zvec_doc_set_double(
        IntPtr h, string field, double val);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void zvec_doc_set_string(
        IntPtr h, string field, string val);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static unsafe partial void zvec_doc_set_vector_fp32(
        IntPtr h, string field, float* data, int count);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static unsafe partial void zvec_doc_set_vector_fp64(
        IntPtr h, string field, double* data, int count);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static unsafe partial void zvec_doc_set_vector_int8(
        IntPtr h, string field, sbyte* data, int count);

    // --- Per-type field getters ---

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int zvec_doc_get_bool(
        IntPtr h, string field, out int val);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int zvec_doc_get_int32(
        IntPtr h, string field, out int val);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int zvec_doc_get_int64(
        IntPtr h, string field, out long val);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int zvec_doc_get_float(
        IntPtr h, string field, out float val);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int zvec_doc_get_double(
        IntPtr h, string field, out double val);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int zvec_doc_get_string(
        IntPtr h, string field, out IntPtr val);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static unsafe partial int zvec_doc_get_vector_fp32(
        IntPtr h, string field, out float* data, out int count);

    // =========================================================================
    // CRUD Operations
    // =========================================================================

    [LibraryImport(LibName)]
    internal static partial int zvec_collection_insert(
        IntPtr handle, IntPtr[] docs, int count);

    [LibraryImport(LibName)]
    internal static partial int zvec_collection_upsert(
        IntPtr handle, IntPtr[] docs, int count);

    [LibraryImport(LibName)]
    internal static partial int zvec_collection_update(
        IntPtr handle, IntPtr[] docs, int count);

    [LibraryImport(LibName)]
    internal static partial int zvec_collection_delete_by_pks(
        IntPtr handle, IntPtr[] pks, int count);

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int zvec_collection_delete_by_filter(
        IntPtr handle, string filter);

    // =========================================================================
    // Query
    // =========================================================================

    [LibraryImport(LibName, StringMarshalling = StringMarshalling.Utf8)]
    internal static partial int zvec_collection_query(
        IntPtr handle, string fieldName,
        IntPtr vectorData, int vectorByteSize,
        int topk, string? filter, int includeVector,
        IntPtr queryParams, out IntPtr result);

    [LibraryImport(LibName)]
    internal static partial int zvec_collection_fetch(
        IntPtr handle, IntPtr[] pks, int count, out IntPtr result);

    // =========================================================================
    // Query Result Access
    // =========================================================================

    [LibraryImport(LibName)]
    internal static partial int zvec_query_result_count(IntPtr handle);

    [LibraryImport(LibName)]
    internal static partial IntPtr zvec_query_result_get(IntPtr handle, int index);

    [LibraryImport(LibName)]
    internal static partial void zvec_query_result_destroy(IntPtr handle);

    // =========================================================================
    // Helpers
    // =========================================================================

    /// <summary>
    /// Read the thread-local error and throw ZvecException if non-OK.
    /// </summary>
    internal static void ThrowIfError(int code)
    {
        if (code == 0) return;
        var msgPtr = zvec_last_error_message();
        var msg = Marshal.PtrToStringUTF8(msgPtr) ?? "Unknown error";
        throw new ZvecException(code, msg);
    }

    /// <summary>
    /// Read a native UTF-8 string and free it.
    /// </summary>
    internal static string? ReadAndFreeString(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero) return null;
        var str = Marshal.PtrToStringUTF8(ptr);
        zvec_free_string(ptr);
        return str;
    }
}
