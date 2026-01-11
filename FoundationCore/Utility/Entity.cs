using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foundation.Entity
{
    /// <summary>
    /// This interface defines a set of methods to convert an Entity object into an Anonymous type
    /// </summary>
    public interface IAnonymousConvertible
    {
        /// <summary>
        /// Anonymous object with top level properties
        /// </summary>
        /// <returns></returns>
        public object ToAnonymous();

        /// <summary>
        /// Anonymous object with top level properties, and first level children anonymous objects
        /// </summary>
        /// <returns></returns>
        public object ToAnonymousWithFirstLevelSubObjects();

        /// <summary>
        /// A very minimal anonymous object.  ID, name and description will be the only properties.
        /// </summary>
        /// <returns></returns>
        public object ToMinimalAnonymous();
    }
}
