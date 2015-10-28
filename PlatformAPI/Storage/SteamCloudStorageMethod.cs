using System;
using System.IO;
using System.Text;
using Steamworks;

namespace Hacknet.PlatformAPI.Storage
{
    public class SteamCloudStorageMethod : BasicStorageMethod
    {
        private bool deserialized;

        public override bool ShouldWrite
        {
            get { return true; }
        }

        public override bool DidDeserialize
        {
            get { return deserialized; }
        }

        public override void Load()
        {
            if (!PlatformAPISettings.Running)
                return;
            manifest = SaveFileManifest.Deserialize(this);
            if (manifest == null)
            {
                manifest = new SaveFileManifest();
                manifest.Save(this);
            }
            else
                deserialized = true;
        }

        public override bool FileExists(string filename)
        {
            if (!PlatformAPISettings.Running)
                return false;
            return SteamRemoteStorage.FileExists(filename);
        }

        public override Stream GetFileReadStream(string filename)
        {
            if (!PlatformAPISettings.Running)
                return null;
            var cubDataToRead = 2000000;
            var numArray = new byte[cubDataToRead];
            if (!SteamRemoteStorage.FileExists(filename))
                return null;
            var count = SteamRemoteStorage.FileRead(filename, numArray, cubDataToRead);
            Encoding.UTF8.GetString(numArray);
            return new MemoryStream(numArray, 0, count);
        }

        public override void WriteFileData(string filename, byte[] data)
        {
            if (!PlatformAPISettings.Running || SteamRemoteStorage.FileWrite(filename, data, data.Length))
                return;
            Console.WriteLine("Failed to write to steam");
        }

        public override void WriteFileData(string filename, string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            WriteFileData(filename, bytes);
        }
    }
}