using System;
using System.IO;

namespace Hacknet.PlatformAPI.Storage
{
    public class BasicStorageMethod : IStorageMethod
    {
        protected SaveFileManifest manifest;

        public virtual bool ShouldWrite
        {
            get { throw new NotImplementedException(); }
        }

        public virtual bool DidDeserialize
        {
            get { throw new NotImplementedException(); }
        }

        public virtual void Load()
        {
            throw new NotImplementedException();
        }

        public virtual SaveFileManifest GetSaveManifest()
        {
            return manifest;
        }

        public virtual Stream GetFileReadStream(string filename)
        {
            throw new NotImplementedException();
        }

        public virtual bool FileExists(string filename)
        {
            throw new NotImplementedException();
        }

        public virtual void WriteFileData(string filename, string data)
        {
            throw new NotImplementedException();
        }

        public virtual void WriteFileData(string filename, byte[] data)
        {
            throw new NotImplementedException();
        }

        public virtual void WriteSaveFileData(string filename, string username, string data, DateTime utcSaveFileTime)
        {
            WriteFileData(filename, data);
            for (var index = 0; index < manifest.Accounts.Count; ++index)
            {
                var saveAccountData = manifest.Accounts[index];
                if (saveAccountData.Username == username)
                {
                    saveAccountData.LastWriteTime = utcSaveFileTime;
                    manifest.LastLoggedInUser = saveAccountData;
                    break;
                }
            }
            manifest.Save(this);
        }

        public virtual void UpdateDataFromOtherManager(IStorageMethod otherMethod)
        {
            var str = manifest.LastLoggedInUser.Username;
            var saveManifest = otherMethod.GetSaveManifest();
            for (var index1 = 0; index1 < saveManifest.Accounts.Count; ++index1)
            {
                var saveAccountData1 = saveManifest.Accounts[index1];
                var flag = false;
                for (var index2 = 0; index2 < manifest.Accounts.Count; ++index2)
                {
                    var saveAccountData2 = manifest.Accounts[index2];
                    if (saveAccountData2.Username == saveAccountData1.Username)
                    {
                        flag = true;
                        var timeSpan = saveAccountData1.LastWriteTime - saveAccountData2.LastWriteTime;
                        if (saveAccountData1.LastWriteTime > saveAccountData2.LastWriteTime &&
                            timeSpan.TotalSeconds > 5.0)
                        {
                            var fileReadStream = otherMethod.GetFileReadStream(saveAccountData1.FileUsername);
                            if (fileReadStream != null)
                            {
                                var data = Utils.ReadEntireContentsOfStream(fileReadStream);
                                if (data.Length > 100)
                                {
                                    WriteFileData(saveAccountData2.FileUsername, data);
                                }
                            }
                        }
                        break;
                    }
                }
                if (!flag)
                {
                    var fileReadStream = otherMethod.GetFileReadStream(saveAccountData1.FileUsername);
                    if (fileReadStream != null)
                    {
                        var fileNameForUsername = SaveFileManager.GetSaveFileNameForUsername(saveAccountData1.Username);
                        manifest.AddUser(saveAccountData1.Username, saveAccountData1.Password, DateTime.UtcNow,
                            fileNameForUsername);
                        var data = Utils.ReadEntireContentsOfStream(fileReadStream);
                        WriteFileData(fileNameForUsername, data);
                    }
                }
            }
            for (var index = 0; index < manifest.Accounts.Count; ++index)
            {
                if (manifest.Accounts[index].Username == str)
                    manifest.LastLoggedInUser = manifest.Accounts[index];
            }
            if (manifest.LastLoggedInUser.Username == null && manifest.Accounts.Count > 0)
                manifest.LastLoggedInUser = manifest.Accounts[0];
            manifest.Save(this);
        }
    }
}