using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TNO.BitUtilities;

namespace HusicBasic.Services
{
    public class VersionedFileManager<TVersion, TInstance> : IVersionedFileManager<TVersion, TInstance> where TVersion : unmanaged
    {

        #region Properties
        private TVersion _Latest;
        public TVersion Latest { get => _Latest; 
            set {
                if (HasRegistered(value))
                    _Latest = value;
                else
                    throw new Exception($"No read and write methods have been registered for the version '{value}'.");
            }
        }
        private Dictionary<TVersion, Func<IAdvancedBitReader, TInstance>> ReadMethods = new Dictionary<TVersion, Func<IAdvancedBitReader, TInstance>>();
        private Dictionary<TVersion, Action<IAdvancedBitWriter, TInstance>> WriteMethods = new Dictionary<TVersion, Action<IAdvancedBitWriter, TInstance>>();
        #endregion

        #region Methods
        public bool HasRegistered(TVersion version) => ReadMethods.ContainsKey(version);
        public void Register(TVersion version, Func<IAdvancedBitReader, TInstance> read, Action<IAdvancedBitWriter, TInstance> write)
        {
            if (HasRegistered(version)) throw new ArgumentException($"Read and write methods have already been registered for the version '{version}'.", nameof(version));
            ReadMethods.Add(version, read);
            WriteMethods.Add(version, write);
        }
        public void Unregister(TVersion version)
        {
            if (!HasRegistered(version)) throw new ArgumentException($"Read and write methods have not been registered for the version '{version}'.", nameof(version));
            ReadMethods.Remove(version);
            WriteMethods.Remove(version);
        }
        public TInstance Read(IAdvancedBitReader reader)
        {
            TVersion version = reader.Read<TVersion>();
            if (ReadMethods.TryGetValue(version, out var readMethod))
                return readMethod(reader);
            else throw new Exception($"No read method has been registered for the version {version} for the type {typeof(TInstance)}.");
        }
        public void Write(IAdvancedBitWriter writer, TVersion version, TInstance inst)
        {
            if (WriteMethods.TryGetValue(version, out var writeMethod))
            {
                writer.Write(version);
                writeMethod(writer, inst);
            }
            else throw new Exception($"No write method has been registered for the version {version} for the type {typeof(TInstance)}.");
        }
        #region Read
        public TInstance Read(string path) => Read(File.ReadAllBytes(path));
        public TInstance Read(byte[] data)
        {
            AdvancedBitReader r = new AdvancedBitReader();
            r.FromArray(data);
            return Read(r);
        }
        public TInstance Read(Stream stream) => Read(new AdvancedBitReader(stream));
        #endregion
        #region Write
        public IAdvancedBitWriter Write(TVersion version, TInstance inst)
        {
            AdvancedBitWriter w = new AdvancedBitWriter();
            Write(w, version, inst);
            return w;
        }
        public byte[] WriteToBytes(TVersion version, TInstance inst)
        {
            IAdvancedBitWriter w = Write(version, inst);
            return w.ToArray();
        }
        public void Write(string path, TVersion version, TInstance inst)
        {
            byte[] data = WriteToBytes(version, inst);
            File.WriteAllBytes(path, data);
        }
        #endregion
        #region Write Latest
        public void Write(IAdvancedBitWriter writer, TInstance inst) => Write(writer, Latest, inst);
        public IAdvancedBitWriter Write(TInstance inst) => Write(Latest, inst);
        public byte[] WriteToBytes(TInstance inst) => WriteToBytes(Latest, inst);
        public void Write(string path, TInstance inst) => Write(path, Latest, inst);
        #endregion
        #endregion
    }
}
