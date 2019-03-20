namespace Modbus.Data
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using Unme.Common;

    /// <summary>
    ///     Object simulation of device memory map.
    ///     The underlying collections are thread safe when using the ModbusMaster API to read/write values.
    ///     You can use the SyncRoot property to synchronize direct access to the DataStore collections.
    /// </summary>
    public class DataStore : IDataStore
    {
        private readonly object _syncRoot = new object();

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataStore" /> class.
        /// </summary>
        public DataStore()
        {
            CoilDiscretes = new ModbusDataCollection<bool> {ModbusDataType = ModbusDataType.Coil};
            InputDiscretes = new ModbusDataCollection<bool> {ModbusDataType = ModbusDataType.Input};
            HoldingRegisters = new ModbusDataCollection<ushort> {ModbusDataType = ModbusDataType.HoldingRegister};
            InputRegisters = new ModbusDataCollection<ushort> {ModbusDataType = ModbusDataType.InputRegister};
        }

        internal DataStore(IList<bool> coilDiscretes, IList<bool> inputDiscretes, IList<ushort> holdingRegisters, IList<ushort> inputRegisters)
        {
            CoilDiscretes = new ModbusDataCollection<bool>(coilDiscretes) { ModbusDataType = ModbusDataType.Coil };
            InputDiscretes = new ModbusDataCollection<bool>(inputDiscretes) { ModbusDataType = ModbusDataType.Input };
            HoldingRegisters = new ModbusDataCollection<ushort>(holdingRegisters) { ModbusDataType = ModbusDataType.HoldingRegister };
            InputRegisters = new ModbusDataCollection<ushort>(inputRegisters) { ModbusDataType = ModbusDataType.InputRegister };
        }

        /// <summary>
        ///     Occurs when the DataStore is written to via a Modbus command.
        /// </summary>
        public event EventHandler<DataStoreEventArgs> DataStoreWrittenTo;

        /// <summary>
        ///     Occurs when the DataStore is read from via a Modbus command.
        /// </summary>
        public event EventHandler<DataStoreEventArgs> DataStoreReadFrom;

        /// <summary>
        ///     Gets the coil discretes.
        /// </summary>
        public ModbusDataCollection<bool> CoilDiscretes { get; private set; }

        IModbusDataCollection<bool> IDataStore.CoilDiscretes
        {
            get { return CoilDiscretes; }
        }

        /// <summary>
        ///     Gets the input discretes.
        /// </summary>
        public ModbusDataCollection<bool> InputDiscretes { get; private set; }

        IModbusDataCollection<bool> IDataStore.InputDiscretes
        {
            get { return InputDiscretes; }
        }

        /// <summary>
        ///     Gets the holding registers.
        /// </summary>
        public ModbusDataCollection<ushort> HoldingRegisters { get; private set; }

        IModbusDataCollection<ushort> IDataStore.HoldingRegisters
        {
            get { return HoldingRegisters; }
        }

        /// <summary>
        ///     Gets the input registers.
        /// </summary>
        public ModbusDataCollection<ushort> InputRegisters { get; private set; }

        IModbusDataCollection<ushort> IDataStore.InputRegisters
        {
            get { return InputRegisters; }
        }

        /// <summary>
        ///     An object that can be used to synchronize direct access to the DataStore collections.
        /// </summary>
        public object SyncRoot
        {
            get { return _syncRoot; }
        }

        /// <summary>
        ///     Retrieves subset of data from collection.
        /// </summary>
        /// <typeparam name="T">The collection type.</typeparam>
        /// <typeparam name="U">The type of elements in the collection.</typeparam>
        public T ReadData<T, U>(IModbusDataCollection<U> dataSource, ushort startAddress,
            ushort count, object syncRoot) where T : Collection<U>, new()
        {
            int startIndex = startAddress + 1;

            if (startIndex < 0 || dataSource.Count < startIndex + count)
                throw new InvalidModbusRequestException(Modbus.IllegalDataAddress);

            U[] dataToRetrieve;
            lock (syncRoot)
                dataToRetrieve = dataSource.Slice(startIndex, count).ToArray();

            T result = new T();
            for (int i = 0; i < count; i++)
                result.Add(dataToRetrieve[i]);

            DataStoreReadFrom.Raise(this,
                DataStoreEventArgs.CreateDataStoreEventArgs(startAddress, dataSource.ModbusDataType, result));

            return result;
        }

        /// <summary>
        ///     Write data to data store.
        /// </summary>
        /// <typeparam name="TData">The type of the data.</typeparam>
        public void WriteData<TData>(IEnumerable<TData> items,
            IModbusDataCollection<TData> destination, ushort startAddress, object syncRoot)
        {
            int startIndex = startAddress + 1;

            if (startIndex < 0 || destination.Count < startIndex + items.Count())
                throw new InvalidModbusRequestException(Modbus.IllegalDataAddress);

            lock (syncRoot)
                Update(items, (IList<TData>)destination, startIndex);

            DataStoreWrittenTo.Raise(this,
                DataStoreEventArgs.CreateDataStoreEventArgs(startAddress, destination.ModbusDataType, items));
        }

        /// <summary>
        ///     Updates subset of values in a collection.
        /// </summary>
        internal static void Update<T>(IEnumerable<T> items, IList<T> destination, int startIndex)
        {
            if (startIndex < 0 || destination.Count < startIndex + items.Count())
                throw new InvalidModbusRequestException(Modbus.IllegalDataAddress);

            int index = startIndex;
            foreach (T item in items)
            {
                destination[index] = item;
                ++index;
            }
        }
    }
}