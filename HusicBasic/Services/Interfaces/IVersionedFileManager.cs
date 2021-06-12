using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TNO.BitUtilities;

namespace HusicBasic.Services
{
    public interface IVersionedFileManager<TVersion, TInstance> where TVersion : unmanaged
    {
        #region Properties
        public TVersion Latest { get; set; }
        #endregion

        #region Methods
        public void Register(TVersion version, Func<IAdvancedBitReader, TInstance> read, Action<IAdvancedBitWriter, TInstance> write);
        public void Unregister(TVersion version);
        public bool HasRegistered(TVersion version);
        #region Read
        public TInstance Read(string path);
        public TInstance Read(byte[] data);
        public TInstance Read(Stream stream);
        public TInstance Read(IAdvancedBitReader reader);
        #endregion
        #region Write
        public void Write(IAdvancedBitWriter writer, TVersion version, TInstance inst);
        public IAdvancedBitWriter Write(TVersion version, TInstance inst);
        public byte[] WriteToBytes(TVersion version, TInstance inst);
        public void Write(string path, TVersion version, TInstance inst);
        #endregion
        #region Write Latest
        public void Write(IAdvancedBitWriter writer, TInstance inst);
        public IAdvancedBitWriter Write(TInstance inst);
        public byte[] WriteToBytes(TInstance inst);
        public void Write(string path, TInstance inst);
        #endregion
        #endregion
    }
}
