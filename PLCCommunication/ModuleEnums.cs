using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using S7.Net;
namespace PerfSoftCommunication.Enums
{
    enum ModuleTypeEnum { S7200, S7300, S7400, S71200, S71500, AdvantechAdam, Other }

    static class ModuleTypeParser
    {
        public static ModuleTypeEnum ToModuleTypeEnum(this string type)
        {
            ModuleTypeEnum moduleType;
            switch (type)
            {
                case "Adam":
                    moduleType = ModuleTypeEnum.AdvantechAdam;
                    break;
                case "S7300":
                    moduleType = ModuleTypeEnum.S7300;
                    break;
                case "S7400":
                    moduleType = ModuleTypeEnum.S7400;
                    break;
                case "S71200":
                    moduleType = ModuleTypeEnum.S71200;
                    break;
                case "S71500":
                    moduleType = ModuleTypeEnum.S71500;
                    break;
                default:
                    moduleType = ModuleTypeEnum.Other;
                    break;
            }
            return moduleType;
        }

        public static CpuType ToS7CpuType(this ModuleTypeEnum type)
        {
            switch (type)
            {
                case ModuleTypeEnum.S7200:
                    return CpuType.S7200;
                case ModuleTypeEnum.S7300:
                    return CpuType.S7300;
                case ModuleTypeEnum.S7400:
                    return CpuType.S7400;
                case ModuleTypeEnum.S71200:
                    return CpuType.S71200;
                case ModuleTypeEnum.S71500:
                    return CpuType.S71500;
                default:
                    throw new InvalidOperationException("Próba rzutowania nie-plcka na plcka!");
                
            }

        }

    }
}
