// Decompiled with JetBrains decompiler
// Type: Hacknet.PlatformAPI.Storage.IStorageMethod
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.IO;

namespace Hacknet.PlatformAPI.Storage
{
    public interface IStorageMethod
    {
        bool ShouldWrite { get; }

        bool DidDeserialize { get; }

        void Load();

        SaveFileManifest GetSaveManifest();

        Stream GetFileReadStream(string filename);

        bool FileExists(string filename);

        void WriteFileData(string filename, string data);

        void WriteFileData(string filename, byte[] data);

        void WriteSaveFileData(string filename, string username, string data, DateTime utcSaveFileTime);

        void UpdateDataFromOtherManager(IStorageMethod otherMethod);
    }
}