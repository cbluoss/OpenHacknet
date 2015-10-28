// Decompiled with JetBrains decompiler
// Type: Hacknet.PlatformAPI.Storage.LocalDocumentsStorageMethod
// Assembly: Hacknet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 48C62A5D-184B-4610-A7EA-84B38D090891
// Assembly location: C:\Program Files (x86)\Steam\SteamApps\common\Hacknet\Hacknet.exe

using System;
using System.IO;

namespace Hacknet.PlatformAPI.Storage
{
    public class LocalDocumentsStorageMethod : BasicStorageMethod
    {
        private bool deserialized;
        private string FolderPath;

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
            FolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "/My Games/Hacknet/Accounts/";
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);
            try
            {
                manifest = SaveFileManifest.Deserialize(this);
            }
            catch (FormatException ex)
            {
                manifest = null;
            }
            catch (NullReferenceException ex)
            {
                manifest = null;
            }
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
            return File.Exists(FolderPath + filename);
        }

        public override Stream GetFileReadStream(string filename)
        {
            return File.OpenRead(FolderPath + filename);
        }

        public override void WriteFileData(string filename, byte[] data)
        {
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);
            Utils.SafeWriteToFile(data, FolderPath + filename);
        }

        public override void WriteFileData(string filename, string data)
        {
            if (!Directory.Exists(FolderPath))
                Directory.CreateDirectory(FolderPath);
            Utils.SafeWriteToFile(data, FolderPath + filename);
        }
    }
}