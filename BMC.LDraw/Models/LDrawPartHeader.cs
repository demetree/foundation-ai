using System.Collections.Generic;

namespace BMC.LDraw.Models
{
    /// <summary>
    /// Parsed header metadata from an LDraw .dat part file.
    /// Contains only the metadata — no geometry data.
    /// </summary>
    public class LDrawPartHeader
    {
        /// <summary>File name from the "0 Name:" line (e.g. "3001.dat").</summary>
        public string FileName;

        /// <summary>Part title from the first description line (e.g. "Brick  2 x  4").</summary>
        public string Title;

        /// <summary>Part author from the "0 Author:" line.</summary>
        public string Author;

        /// <summary>LDraw part type from "0 !LDRAW_ORG" line: Part, Subpart, Primitive, Shortcut, Alias.</summary>
        public string PartType;

        /// <summary>
        /// Part category from "0 !CATEGORY" line.
        /// If absent, inferred from the first word of the title.
        /// </summary>
        public string Category;

        /// <summary>License text from the "0 !LICENSE" line.</summary>
        public string License;

        /// <summary>Aggregated keywords from all "0 !KEYWORDS" lines.</summary>
        public List<string> Keywords = new List<string>();

        /// <summary>History entries from "0 !HISTORY" lines.</summary>
        public List<string> History = new List<string>();
    }
}
