using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LexTranslator.FileManagement;

namespace LexTranslator.SkyrimManagement
{
    public class BackupManager
    {
        public static int BackupRetentionCount = 10;

        public static string BackupSuffix = "_backup.zip";

        public static string CreateBackupName(string FileName,int ID)
        {
            string Time = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            return $"{Time}_{ID}_{FileName}";
        }

        public static string AddFile(string Path,ref List<ZipFileInfo> InFos)
        {
            int ID = 0;

            string FileName = new FileInfo(Path).Name;
           
            string GetManagePath = FileName + BackupSuffix;

            if (File.Exists(GetManagePath))
            {
                InFos = ZipHelper.GetFileList(GetManagePath);

                if (InFos.Count > 10)
                {
                    ZipFileInfo Oldest = InFos.OrderBy(x => x.Time).FirstOrDefault();

                    if (Oldest != null)
                    {
                        ZipHelper.DeleteFileFromZip(
                            GetManagePath,
                            Oldest.Name
                        );

                        InFos.Remove(Oldest);
                    }
                }

                ID = InFos.Count;

                ZipHelper.AddFileToZip(GetManagePath,Path,CreateBackupName(FileName, ID));

                return GetManagePath;
            }
            else
            {
                ZipHelper.CompressFile(Path,GetManagePath,CreateBackupName(FileName, ID));
            }

            return GetManagePath;
        }
    }
}
