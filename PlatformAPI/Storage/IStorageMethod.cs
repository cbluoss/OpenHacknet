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