using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uno.UITest.Helpers.Queries
{
    public enum Platform
    {
        /// <summary>
        /// Not supported. Exists only for compatibility purposes.
        /// </summary>
        iOS,
        /// <summary>
        /// Not supported. Exists only for compatibility purposes.
        /// </summary>
        Android,
        /// <summary>
        /// For compatibility, this is equivalent to <see cref="Chrome"/>. If you wish to use a different browser for WASM, 
        /// select the value that corresponds to the browser's name.
        /// </summary>
        Browser,
        /// <summary>
        /// Uses Chrome with WASM via Chrome Driver.
        /// </summary>
        Chrome,
        /// <summary>
        /// Uses Edge with WASM via Edge Driver.
        /// </summary>
        Edge,
        /// <summary>
        /// Not yet implemented.
        /// </summary>
        Firefox
    }
}
