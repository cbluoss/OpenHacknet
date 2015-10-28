using System.Collections.Generic;
using System.Xml;

namespace Hacknet
{
    public class Folder : FileType
    {
        private static readonly string ampersandReplacer = "|##AMP##|";
        private static readonly string backslashReplacer = "|##BS##|";
        private static readonly string rightSBReplacer = "|##RSB##|";
        private static readonly string leftSBReplacer = "|##LSB##|";
        private static readonly string rightABReplacer = "|##RAB##|";
        private static readonly string leftABReplacer = "|##LAB##|";
        private static readonly string quoteReplacer = "|##QOT##|";
        private static readonly string singlequoteReplacer = "|##SIQ##|";
        public List<FileEntry> files = new List<FileEntry>();
        public List<Folder> folders = new List<Folder>();
        public string name;

        public Folder(string foldername)
        {
            name = foldername;
        }

        public string getName()
        {
            return name;
        }

        public bool containsFile(string name, string data)
        {
            for (var index = 0; index < files.Count; ++index)
            {
                if (files[index].name.Equals(name) && files[index].data.Equals(data))
                    return true;
            }
            return false;
        }

        public bool containsFile(string name)
        {
            for (var index = 0; index < files.Count; ++index)
            {
                if (files[index].name.Equals(name))
                    return true;
            }
            return false;
        }

        public Folder searchForFolder(string folderName)
        {
            for (var index = 0; index < folders.Count; ++index)
            {
                if (folders[index].name == folderName)
                    return folders[index];
            }
            return null;
        }

        public FileEntry searchForFile(string fileName)
        {
            for (var index = 0; index < files.Count; ++index)
            {
                if (files[index].name == fileName)
                    return files[index];
            }
            return null;
        }

        public string getSaveString()
        {
            var str = "<folder name=\"" + Filter(name) + "\">\n";
            for (var index = 0; index < folders.Count; ++index)
                str += folders[index].getSaveString();
            for (var index = 0; index < files.Count; ++index)
                str = str + "<file name=\"" + Filter(files[index].name) + "\">" + Filter(files[index].data) +
                      "</file>\n";
            return str + "</folder>\n";
        }

        public static string Filter(string s)
        {
            return
                s.Replace("&", ampersandReplacer)
                    .Replace("\\", backslashReplacer)
                    .Replace("[", leftSBReplacer)
                    .Replace("]", rightSBReplacer)
                    .Replace(">", rightABReplacer)
                    .Replace("<", leftABReplacer)
                    .Replace("\"", quoteReplacer)
                    .Replace("'", singlequoteReplacer);
        }

        public static string deFilter(string s)
        {
            return
                s.Replace(ampersandReplacer, "&")
                    .Replace(backslashReplacer, "\\")
                    .Replace(rightSBReplacer, "]")
                    .Replace(leftSBReplacer, "[")
                    .Replace(rightABReplacer, ">")
                    .Replace(leftABReplacer, "<")
                    .Replace(quoteReplacer, "\"")
                    .Replace(singlequoteReplacer, "'");
        }

        public static Folder load(XmlReader reader)
        {
            while (reader.Name != "folder" || reader.NodeType == XmlNodeType.EndElement)
            {
                reader.Read();
                if (reader.EOF)
                    return null;
            }
            reader.MoveToAttribute("name");
            var folder = new Folder(deFilter(reader.ReadContentAsString()));
            reader.Read();
            while (reader.Name != "folder" && reader.Name != "file")
            {
                reader.Read();
                if (reader.EOF || reader.Name == "folder" && reader.NodeType == XmlNodeType.EndElement)
                    return folder;
            }
            label_14:
            while (reader.Name == "folder")
            {
                if (reader.NodeType == XmlNodeType.EndElement)
                    return folder;
                folder.folders.Add(load(reader));
                reader.Read();
                do
                {
                    if (reader.Name != "folder" && reader.Name != "file")
                        reader.Read();
                    else
                        goto label_14;
                } while (!reader.EOF && !(reader.Name == "computer"));
                return folder;
            }
            while (reader.Name != "folder" && reader.Name != "file")
                reader.Read();
            label_20:
            while (reader.Name == "file" && reader.NodeType != XmlNodeType.EndElement)
            {
                reader.MoveToAttribute("name");
                var nameEntry = deFilter(reader.ReadContentAsString());
                reader.MoveToElement();
                var dataEntry = deFilter(reader.ReadElementContentAsString());
                folder.files.Add(new FileEntry(dataEntry, nameEntry));
                reader.Read();
                while (true)
                {
                    if (reader.Name != "folder" && reader.Name != "file")
                        reader.Read();
                    else
                        goto label_20;
                }
            }
            reader.Read();
            return folder;
        }

        public void load(string data)
        {
        }

        public string TestEqualsFolder(Folder f)
        {
            string str1 = null;
            if (name != f.name)
                str1 = str1 + "Name Mismatch : Expected \"" + name + "\" But got \"" + f.name + "\"\r\n";
            if (f.folders.Count != folders.Count)
                str1 = str1 + "Folder Count Mismatch : Expected \"" + folders.Count + "\" But got \"" +
                       f.folders.Count + "\"\r\n";
            if (f.files.Count != files.Count)
            {
                str1 = str1 + "File Count Mismatch In folder \"" + f.name + "\" : Expected \"" + files.Count +
                       "\" But got \"" + f.files.Count + "\"\r\nFound Files:\r\n";
                for (var index = 0; index < f.files.Count; ++index)
                    str1 = str1 + f.files[index].name + "\r\n--------\r\n" + f.files[index].data +
                           "\r\n#######END FILE#############\r\n\r\n";
            }
            if (str1 != null)
                return str1 + "Previous errors are blocking. Abandoning examination.\r\n";
            for (var index = 0; index < folders.Count; ++index)
            {
                var str2 = folders[index].TestEqualsFolder(f.folders[index]);
                if (str2 != null)
                    str1 = str1 + "\r\n" + str2;
            }
            for (var index = 0; index < files.Count; ++index)
            {
                if (files[index].name != f.files[index].name)
                    str1 = str1 + "Filename Mismatch (" + index + ") expected \"" + files[index].name +
                           "\" but got \"" + f.files[index].name + "\"\r\n";
                if (files[index].data != f.files[index].data &&
                    files[index].data.Replace("\r\n", "\n") != f.files[index].data.Replace("\r\n", "\n"))
                    str1 = str1 + "Data Mismatch (" + index + ") expected ------\r\n" + files[index].data +
                           "\r\n----- but got ------\r\n" + f.files[index].data + "\r\n-----\r\n";
                else if (!Utils.CheckStringIsRenderable(files[index].data))
                    str1 = str1 + "Data in file is not renderable!\r\nFile: " + files[index].name + "\r\n";
            }
            return str1;
        }
    }
}