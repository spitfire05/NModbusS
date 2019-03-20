using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Modbus.Data
{
    public interface IDataStore
    {
        /// <summary>
        ///     Occurs when the DataStore is written to via a Modbus command.
        /// </summary>
        event EventHandler<DataStoreEventArgs> DataStoreWrittenTo;

        /// <summary>
        ///     Occurs when the DataStore is read from via a Modbus command.
        /// </summary>
        event EventHandler<DataStoreEventArgs> DataStoreReadFrom;

        /// <summary>
        ///     Gets the coil discretes.
        /// </summary>
        IModbusDataCollection<bool> CoilDiscretes { get; }

        /// <summary>
        ///     Gets the input discretes.
        /// </summary>
        IModbusDataCollection<bool> InputDiscretes { get; }

        /// <summary>
        ///     Gets the holding registers.
        /// </summary>
        IModbusDataCollection<ushort> HoldingRegisters { get; }

        /// <summary>
        ///     Gets the input registers.
        /// </summary>
        IModbusDataCollection<ushort> InputRegisters { get; }

        /// <summary>
        ///     An object that can be used to synchronize direct access to the DataStore collections.
        /// </summary>
        object SyncRoot { get; }

        /// <summary>
        ///     Retrieves subset of data from collection.
        /// </summary>
        /// <typeparam name="T">The collection type.</typeparam>
        /// <typeparam name="U">The type of elements in the collection.</typeparam>
        T ReadData<T, U>(IModbusDataCollection<U> dataSource, ushort startAddress,
            ushort count, object syncRoot) where T : Collection<U>, new();

        /// <summary>
        ///     Write data to data store.
        /// </summary>
        /// <typeparam name="TData">The type of the data.</typeparam>
        void WriteData<TData>(IEnumerable<TData> items,
            IModbusDataCollection<TData> destination, ushort startAddress, object syncRoot);
    }
}