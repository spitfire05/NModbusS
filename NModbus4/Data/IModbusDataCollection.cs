using System.Collections.Generic;

namespace Modbus.Data
{
    public interface IModbusDataCollection<TData> : ICollection<TData>
    {
        ModbusDataType ModbusDataType { get; set; }
    }
}