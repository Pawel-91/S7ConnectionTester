using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S7ConnectionTester
{
    struct PLCstruct  : IModule
    {
        /// <summary>
        /// Module ID
        /// </summary>
        public int ID;

        /// <summary>
        /// Instance of communication class
        /// </summary>
        public ModuleCommunication Module;

        /// <summary>
        /// Module ID
        /// </summary>
        int IModule.ID
        {
            get
            {
                return this.ID;
            }

            set
            {
                this.ID = value;
            }
        }

        /// <summary>
        /// Instance of communication class
        /// </summary>
        ModuleCommunication IModule.Module
        {
            get
            {
                return this.Module;
            }

            set
            {
                this.Module = value;
            }
        }

        public PLCstruct(int id, ModuleCommunication Plc)
        {
            this.ID = id;
            this.Module = Plc;

        }
        ///// <summary>
        ///// For creating empty object
        ///// </summary>
        ///// <param name="nothing"></param>
        //public PLCstruct(bool nothing)
        //{
        //    this.ID = new int();
        //    this.Module = new ModuleCommunication();
        //}

    }
}
