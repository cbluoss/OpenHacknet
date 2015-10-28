// Decompiled with JetBrains decompiler
// Type: Hacknet.PlatformAPI.Storage.OldSystemStorageMethod
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.IO;

namespace Hacknet.PlatformAPI.Storage
{
    public class OldSystemStorageMethod : IStorageMethod
    {
        private readonly SaveFileManifest manifest;

        public OldSystemStorageMethod(SaveFileManifest manifest)
        {
            this.manifest = manifest;
        }

        public bool ShouldWrite
        {
            get { return false; }
        }

        public bool DidDeserialize
        {
            get { return false; }
        }

        public void Load()
        {
        }

        public SaveFileManifest GetSaveManifest()
        {
            return manifest;
        }

        public Stream GetFileReadStream(string filename)
        {
            return File.OpenRead(filename);
        }

        public bool FileExists(string filename)
        {
            return File.Exists(filename);
        }

        public void WriteFileData(string filename, byte[] data)
        {
            Utils.SafeWriteToFile(data, filename);
        }

        public void WriteFileData(string filename, string data)
        {
            Utils.SafeWriteToFile(data, filename);
        }

        public void UpdateDataFromOtherManager(IStorageMethod otherMethod)
        {
            throw new NotImplementedException();
        }

        public void WriteSaveFileData(string filename, string username, string data, DateTime utcSaveFileTime)
        {
            throw new NotImplementedException();
        }
    }
}