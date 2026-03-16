using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LexTranslator.SkyrimModManager
{
    public class DataHelper
    {
        public static string ShowSaveFileDialog(string defaultFileName, string filter = "All files (*.*)|*.*")
        {
            using (var saveFileDialog = new System.Windows.Forms.SaveFileDialog())
            {
                saveFileDialog.FileName = defaultFileName;
                saveFileDialog.Filter = filter;
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    return saveFileDialog.FileName;
                }
                else
                {
                    return null;
                }
            }
        }

        public static string ReadFileByStr(string filepath, Encoding EnCoding)
        {
            try
            {
                StreamReader rd = new StreamReader(filepath, Encoding.UTF8);

                StringBuilder sb = new StringBuilder();
                while (!rd.EndOfStream)
                {
                    string dqstr = rd.ReadLine();
                    sb = sb.Append(dqstr + "\r\n");
                }

                rd.Close();
                rd.Dispose();
                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }
        public static byte[] StrToStream(string Str)
        {
            return Encoding.UTF8.GetBytes(Str);
        }
        public static byte[] ReadFile(string Path)
        {
            byte[] Data = null;

            if (File.Exists(Path))
            {
                using (FileStream FS =
                          new FileStream(Path, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader BR = new BinaryReader(FS))
                    {
                        Data = BR.ReadBytes((int)FS.Length);
                    }
                }
            }
            else
            {
                return new byte[0];
            }

            return Data;
        }
        public static void WriteFile(string TargetPath, byte[] Data)
        {
            FileStream FS = new FileStream(TargetPath, FileMode.Create);
            FS.Write(Data, 0, Data.Length);
            FS.Close();
            FS.Dispose();
        }
        public byte[] StreamToBytes(Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);

            stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }
        public Stream BytesToStream(byte[] bytes)
        {
            Stream stream = new MemoryStream(bytes);
            return stream;
        }


        public static System.Text.Encoding GetFileEncodeType(string FilePath)
        {
            try
            {
                System.IO.FileStream FileStream = new System.IO.FileStream(FilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                System.IO.BinaryReader BinaryReader = new System.IO.BinaryReader(FileStream);
                Byte[] buffer = BinaryReader.ReadBytes(2);
                if (buffer[0] >= 0xEF)
                {
                    if (buffer[0] == 0xEF && buffer[1] == 0xBB)
                    {
                        FileStream.Close();
                        BinaryReader.Close();
                        return System.Text.Encoding.UTF8;
                    }
                    else if (buffer[0] == 0xFE && buffer[1] == 0xFF)
                    {
                        FileStream.Close();
                        BinaryReader.Close();
                        return System.Text.Encoding.BigEndianUnicode;
                    }
                    else if (buffer[0] == 0xFF && buffer[1] == 0xFE)
                    {
                        FileStream.Close();
                        BinaryReader.Close();
                        return System.Text.Encoding.Unicode;
                    }
                    else
                    {
                        FileStream.Close();
                        BinaryReader.Close();
                        return System.Text.Encoding.UTF8;
                    }
                }
                if (buffer[0] == 0x3c)//UTF-8 without BOM
                {
                    FileStream.Close();
                    BinaryReader.Close();
                    return System.Text.Encoding.UTF8;
                }
                else
                {
                    FileStream.Close();
                    BinaryReader.Close();
                    return System.Text.Encoding.UTF8;
                }
            }
            catch { return Encoding.UTF8; }
        }

        public static void CopyDir(string Path, string TargetPath)
        {
            try
            {
                if (TargetPath[TargetPath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
                {
                    TargetPath += System.IO.Path.DirectorySeparatorChar;
                }
                if (!System.IO.Directory.Exists(TargetPath))
                {
                    System.IO.Directory.CreateDirectory(TargetPath);
                }
                string[] FileList = System.IO.Directory.GetFileSystemEntries(Path);
                foreach (string File in FileList)
                {
                    if (System.IO.Directory.Exists(File))
                    {
                        CopyDir(File, TargetPath + System.IO.Path.GetFileName(File));
                    }
                    else
                    {
                        System.IO.File.Copy(File, TargetPath + System.IO.Path.GetFileName(File), true);
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
        public static List<FileInformation> GetAllFile(string filepath, List<string> filetype = null)
        {
            DirectoryAllFiles.FileList.Clear();
            List<FileInformation> list = DirectoryAllFiles.GetAllFiles(new System.IO.DirectoryInfo(filepath));
            List<FileInformation> nlist = new List<FileInformation>();

            if (filetype == null == false)
            {
                nlist.AddRange(list);
                foreach (var autoget in list)
                {
                    if (filetype.Contains(autoget.Filetype) == false)
                    {
                        nlist.Remove(autoget);
                    }
                }

                return nlist;
            }

            return list;
        }
    }

    public class DirectoryAllFiles
    {
        public static List<FileInformation> FileList = new List<FileInformation>();
        public static List<FileInformation> GetAllFiles(DirectoryInfo dir)
        {

            List<FileInfo> allFile = new List<FileInfo>(); ;

            try
            {
                allFile = dir.GetFiles().ToList();
            }
            catch { }

            foreach (FileInfo fi in allFile)
            {
                FileList.Add(new FileInformation { FileName = fi.Name, FilePath = fi.FullName, Filetype = fi.Extension });
            }

            List<DirectoryInfo> allDir = new List<DirectoryInfo>();

            try
            {
                allDir = dir.GetDirectories().ToList();
            }
            catch { }

            foreach (DirectoryInfo d in allDir)
            {
                GetAllFiles(d);
            }
            return FileList;
        }
    }

    public class FileInformation
    {
        public string Filetype = "";
        public string FileName = "";
        public string FilePath = "";
    }
}
