using System;
using System.IO;
using System.Text;
using BMC.LDraw.Models;

namespace BMC.LDraw.Render
{
    /// <summary>
    /// Exports an LDraw mesh as an STL (stereolithography) file.
    ///
    /// Supports two output formats:
    ///   - Binary STL (compact, 84 + 50×N bytes) — the default for 3D printing / CAD import.
    ///   - ASCII STL (human-readable solid/facet/vertex text).
    ///
    /// The LDrawMesh already contains world-space triangles with face normals, so the
    /// conversion is a direct 1:1 mapping — no projection or lighting needed.
    ///
    /// AI-generated — Mar 2026.
    /// </summary>
    public static class StlExporter
    {
        private const string HEADER_TEXT = "Exported from BMC - LDraw part geometry";


        /// <summary>
        /// Export an LDraw mesh as a binary STL byte array.
        ///
        /// Binary STL format:
        ///   80-byte header (padded with zeroes)
        ///   4-byte uint32 triangle count
        ///   For each triangle (50 bytes):
        ///     12 bytes — face normal (3 × float32)
        ///     36 bytes — 3 vertices (9 × float32)
        ///      2 bytes — attribute byte count (0)
        /// </summary>
        /// <param name="mesh">Pre-resolved LDraw mesh with world-space triangles.</param>
        /// <returns>Complete binary STL file as a byte array.</returns>
        public static byte[] ExportBinary(LDrawMesh mesh)
        {
            if (mesh == null)
            {
                throw new ArgumentNullException(nameof(mesh));
            }

            int triangleCount = mesh.Triangles.Count;
            int fileSize = 84 + (50 * triangleCount);

            byte[] result = new byte[fileSize];

            //
            // Header — 80 bytes (ASCII text padded with zeroes)
            //
            byte[] headerBytes = Encoding.ASCII.GetBytes(HEADER_TEXT);
            int headerLen = Math.Min(headerBytes.Length, 80);
            Buffer.BlockCopy(headerBytes, 0, result, 0, headerLen);

            //
            // Triangle count — uint32 at offset 80
            //
            BitConverter.TryWriteBytes(new Span<byte>(result, 80, 4), (uint)triangleCount);

            //
            // Facets — 50 bytes each starting at offset 84
            //
            int offset = 84;
            for (int i = 0; i < triangleCount; i++)
            {
                MeshTriangle tri = mesh.Triangles[i];

                // Face normal (3 × float32 = 12 bytes)
                BitConverter.TryWriteBytes(new Span<byte>(result, offset, 4), tri.NX);
                BitConverter.TryWriteBytes(new Span<byte>(result, offset + 4, 4), tri.NY);
                BitConverter.TryWriteBytes(new Span<byte>(result, offset + 8, 4), tri.NZ);

                // Vertex 1 (3 × float32 = 12 bytes)
                BitConverter.TryWriteBytes(new Span<byte>(result, offset + 12, 4), tri.X1);
                BitConverter.TryWriteBytes(new Span<byte>(result, offset + 16, 4), tri.Y1);
                BitConverter.TryWriteBytes(new Span<byte>(result, offset + 20, 4), tri.Z1);

                // Vertex 2 (3 × float32 = 12 bytes)
                BitConverter.TryWriteBytes(new Span<byte>(result, offset + 24, 4), tri.X2);
                BitConverter.TryWriteBytes(new Span<byte>(result, offset + 28, 4), tri.Y2);
                BitConverter.TryWriteBytes(new Span<byte>(result, offset + 32, 4), tri.Z2);

                // Vertex 3 (3 × float32 = 12 bytes)
                BitConverter.TryWriteBytes(new Span<byte>(result, offset + 36, 4), tri.X3);
                BitConverter.TryWriteBytes(new Span<byte>(result, offset + 40, 4), tri.Y3);
                BitConverter.TryWriteBytes(new Span<byte>(result, offset + 44, 4), tri.Z3);

                // Attribute byte count — 2 bytes (always 0)
                // Already zeroed from array initialisation

                offset += 50;
            }

            return result;
        }


        /// <summary>
        /// Export an LDraw mesh as an ASCII STL string.
        ///
        /// ASCII STL format:
        ///   solid [name]
        ///     facet normal nx ny nz
        ///       outer loop
        ///         vertex x1 y1 z1
        ///         vertex x2 y2 z2
        ///         vertex x3 y3 z3
        ///       endloop
        ///     endfacet
        ///   endsolid [name]
        /// </summary>
        /// <param name="mesh">Pre-resolved LDraw mesh with world-space triangles.</param>
        /// <param name="solidName">Optional name for the solid block.  Defaults to "LDrawPart".</param>
        /// <returns>Complete ASCII STL document as a string.</returns>
        public static string ExportAscii(LDrawMesh mesh, string solidName = "LDrawPart")
        {
            if (mesh == null)
            {
                throw new ArgumentNullException(nameof(mesh));
            }

            // Rough estimate: ~180 chars per triangle
            StringBuilder sb = new StringBuilder(mesh.Triangles.Count * 180 + 64);

            sb.Append("solid ");
            sb.AppendLine(solidName);

            for (int i = 0; i < mesh.Triangles.Count; i++)
            {
                MeshTriangle tri = mesh.Triangles[i];

                sb.AppendFormat("  facet normal {0:G9} {1:G9} {2:G9}\n", tri.NX, tri.NY, tri.NZ);
                sb.AppendLine("    outer loop");
                sb.AppendFormat("      vertex {0:G9} {1:G9} {2:G9}\n", tri.X1, tri.Y1, tri.Z1);
                sb.AppendFormat("      vertex {0:G9} {1:G9} {2:G9}\n", tri.X2, tri.Y2, tri.Z2);
                sb.AppendFormat("      vertex {0:G9} {1:G9} {2:G9}\n", tri.X3, tri.Y3, tri.Z3);
                sb.AppendLine("    endloop");
                sb.AppendLine("  endfacet");
            }

            sb.Append("endsolid ");
            sb.AppendLine(solidName);

            return sb.ToString();
        }
    }
}
