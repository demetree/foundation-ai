namespace BMC.LDraw.Models
{
    /// <summary>
    /// A Type 3 line — a triangle defined by 3 vertices.
    /// </summary>
    public class LDrawTriangle
    {
        public int ColourCode;
        public float X1, Y1, Z1;
        public float X2, Y2, Z2;
        public float X3, Y3, Z3;
    }

    /// <summary>
    /// A Type 4 line — a quadrilateral defined by 4 vertices.
    /// During rendering, split into 2 triangles: (1,2,3) and (1,3,4).
    /// </summary>
    public class LDrawQuad
    {
        public int ColourCode;
        public float X1, Y1, Z1;
        public float X2, Y2, Z2;
        public float X3, Y3, Z3;
        public float X4, Y4, Z4;
    }

    /// <summary>
    /// A Type 2 line — a line segment defined by 2 vertices.
    /// Used for edge/wireframe rendering.
    /// </summary>
    public class LDrawLine
    {
        public int ColourCode;
        public float X1, Y1, Z1;
        public float X2, Y2, Z2;
    }

    /// <summary>
    /// A Type 5 line — a conditional line (optional edge).
    /// Drawn only when control points C1 and C2 are on the same side of the line.
    /// </summary>
    public class LDrawConditionalLine
    {
        public int ColourCode;
        public float X1, Y1, Z1;
        public float X2, Y2, Z2;
        /// <summary>Control point 1.</summary>
        public float CX1, CY1, CZ1;
        /// <summary>Control point 2.</summary>
        public float CX2, CY2, CZ2;
    }
}
