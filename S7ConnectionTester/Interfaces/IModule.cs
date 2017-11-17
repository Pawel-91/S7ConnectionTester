using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S7ConnectionTester
{
    /// <summary>
    /// Communication with external module
    /// </summary>
    interface IModule
    {
        /// <summary>
        /// Module ID
        /// </summary>
        int ID { get; set; }

        /// <summary>
        /// Instance of communication class
        /// </summary>
        ModuleCommunication Module { get; set; }
    }
}
