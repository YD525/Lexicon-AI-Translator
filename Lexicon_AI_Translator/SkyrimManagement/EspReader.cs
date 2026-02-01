using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using PhoenixEngine.EngineManagement;
using PhoenixEngine.TranslateManage;
using LexTranslator.TranslateManagement;
using static PhoenixEngine.Bridges.NativeBridge;
using LexTranslator.ConvertManager;
using LexTranslator.TranslateManage;

namespace LexTranslator.SkyrimManagement
{
    public class SubRecordData
    {
        public string Sig { get; set; }
        public byte[] Data { get; set; }
        public bool IsLocalized { get; set; }
        public uint StringID { get; set; }
        public string Content { get; set; }
        public int OccurrenceIndex { get; set; }  
        public int Index { get; set; }
    }

    public class EspRecordInfo
    {
        public IntPtr Handle { get; set; }  // Intptr
        public string Sig { get; set; }
        public uint FormID { get; set; }
        public uint Flags { get; set; }
        public List<SubRecordData> SubRecords { get; set; }
        public int Index { get; set; }

        public string EditorID { get; set; }

        public EspRecordInfo()
        {
            SubRecords = new List<SubRecordData>();
        }

        public string GetUniqueKey()
        {
            return $"{Sig}:{FormID}";
        }

        public string GetFormIDHex()
        {
            return $"{FormID:X8}";
        }

        public string GetEditorID()
        {
            var edid = SubRecords.Find(s => s.Sig == "EDID");
            return edid?.Content ?? "";
        }

        public string GetDisplayName()
        {
            var full = SubRecords.Find(s => s.Sig == "FULL");
            if (full != null && !string.IsNullOrEmpty(full.Content))
                return full.Content;
            return GetEditorID();
        }
    }

    public static class EspInterop
    {
        private const string DllName = "EspReader.dll";

        public static string Version = "";

        #region P/Invoke 

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr C_GetFieldReport();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int C_GetFieldReportLength();



        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr C_GetVersion();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int C_GetVersionLength();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void C_Init();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void C_InitDefaultFilter();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_SetDefaultFilter();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void C_ClearFilter();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_SetFilter(
            [MarshalAs(UnmanagedType.LPStr)] string parentSig,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] childSigs,
            int childCount);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int C_ReadEsp([MarshalAs(UnmanagedType.LPWStr)] string espPath);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr C_SearchBySig(
            [MarshalAs(UnmanagedType.LPStr)] string parentSig,
            [MarshalAs(UnmanagedType.LPStr)] string childSig,
            out int outCount);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void FreeSearchResults(IntPtr arr, int count);

        // SubRecordData Api
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr C_GetSubRecordData_Ptr(IntPtr record, int index);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_SubRecordData_GetOccurrenceIndex(IntPtr subRecord);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_SubRecordData_GetIndex(IntPtr subRecord);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr C_SubRecordData_GetSig(IntPtr subRecord);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr C_SubRecordData_GetString(IntPtr subRecord);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_SubRecordData_GetStringUtf8(IntPtr subRecord, byte[] buffer, int bufferSize);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_SubRecordData_GetSigUtf8(IntPtr subRecord, byte[] buffer, int bufferSize);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool C_SubRecordData_IsLocalized(IntPtr subRecord);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint C_SubRecordData_GetStringID(IntPtr subRecord);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_SubRecordData_GetDataSize(IntPtr subRecord);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool C_SubRecordData_GetData(IntPtr subRecord, byte[] buffer, int bufferSize);

        // Record Api
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int C_GetRecordSig(IntPtr RecordPtr, byte[] buffer, int bufferSize);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint C_GetRecordFormID(IntPtr record);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr C_GetRecordEditorID(IntPtr record);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint C_GetRecordFlags(IntPtr record);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int C_GetRecordIndex(IntPtr record);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int C_GetSubRecordCount(IntPtr record);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool C_ModifySubRecordByOffset(int IsCell, int RecordOffset, int SubOffset, IntPtr NewUtf8Data);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool C_ModifySubRecord(
         uint FormID,
         IntPtr RecordSig,
         IntPtr SubSig,
         int OccurrenceIndex,
         int GlobalIndex,
         IntPtr NewUtf8Data
        );

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool C_SaveEsp(IntPtr utf8Path);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void C_Close();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void C_Clear();

        #endregion

        public static string GetFieldReport()
        {
            int Length = C_GetFieldReportLength();
            if (Length <= 0)
            {
                return "Validator not initialized";
            }

            IntPtr Ptr = C_GetFieldReport();
            if (Ptr == IntPtr.Zero)
            {
                return "Validator not initialized";
            }

            byte[] Buffer = new byte[Length];
            Marshal.Copy(Ptr, Buffer, 0, Length);
            return Encoding.UTF8.GetString(Buffer);
        }


        public static bool SaveEsp(string path)
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = StringToUtf8IntPtr(path);
                return C_SaveEsp(ptr);
            }
            finally
            {
                if (ptr != IntPtr.Zero) Marshal.FreeHGlobal(ptr);
            }
        }

        public static string GetVersion()
        {
            try
            {
                int length = C_GetVersionLength();
                if (length <= 0)
                {
                    return "Unknown";
                }

                IntPtr ptr = C_GetVersion();
                if (ptr == IntPtr.Zero)
                {
                    return "Unknown";
                }

                return Marshal.PtrToStringAnsi(ptr, length);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting version: {ex.Message}");
                return "Error";
            }
        }

        private static IntPtr StringToUtf8IntPtr(string Str)
        {
            if (Str == null)
                Str = "";

            byte[] UTF8 = System.Text.Encoding.UTF8.GetBytes(Str);
            IntPtr Ptr = Marshal.AllocHGlobal(UTF8.Length + 1);

            Marshal.Copy(UTF8, 0, Ptr, UTF8.Length);
            Marshal.WriteByte(Ptr, UTF8.Length, 0);

            return Ptr;
        }

        public static bool ModifySubRecordByOffset(bool IsCell, int ParentIndex, int SubIndex, string NewUtf8Data)
        {
            IntPtr PtrNewData = IntPtr.Zero;
            try
            {
                PtrNewData = StringToUtf8IntPtr(NewUtf8Data ?? "");
                if (IsCell)
                {
                    return C_ModifySubRecordByOffset(1, ParentIndex, SubIndex, PtrNewData);
                }
                else
                {
                    return C_ModifySubRecordByOffset(0, ParentIndex, SubIndex, PtrNewData);
                }
            }
            finally
            {
                if (PtrNewData != IntPtr.Zero) Marshal.FreeHGlobal(PtrNewData);
            }
        }

        public static bool ModifySubRecord(uint formId,string recordSig,string subSig,int occurrenceIndex,int globalIndex,string newUtf8Data)
        {
            IntPtr ptrRecordSig = IntPtr.Zero;
            IntPtr ptrSubSig = IntPtr.Zero;
            IntPtr ptrNewData = IntPtr.Zero;

            try
            {
                ptrRecordSig = StringToUtf8IntPtr(recordSig ?? "");
                ptrSubSig = StringToUtf8IntPtr(subSig ?? "");
                ptrNewData = StringToUtf8IntPtr(newUtf8Data ?? "");

                return C_ModifySubRecord(formId, ptrRecordSig, ptrSubSig, occurrenceIndex, globalIndex, ptrNewData);
            }
            finally
            {
                if (ptrRecordSig != IntPtr.Zero) Marshal.FreeHGlobal(ptrRecordSig);
                if (ptrSubSig != IntPtr.Zero) Marshal.FreeHGlobal(ptrSubSig);
                if (ptrNewData != IntPtr.Zero) Marshal.FreeHGlobal(ptrNewData);
            }
        }

        public static void SetFilter(Dictionary<string, string[]> filterConfig)
        {
            C_ClearFilter();

            foreach (var kvp in filterConfig)
            {
                C_SetFilter(kvp.Key, kvp.Value, kvp.Value.Length);
            }
        }

        static EspInterop()
        {
            C_Init();
            C_InitDefaultFilter();
            C_SetDefaultFilter();

            EspInterop.Version = GetVersion();
        }

        public static int LoadEsp(string Path)
        {
            if (File.Exists(Path))
            {
                return C_ReadEsp(Path);
            }
            return -1;
        }

        public static string GetRecordEditorID(IntPtr RecordPtr)
        {
            if (RecordPtr == IntPtr.Zero)
                return string.Empty;

            IntPtr EditorIDPtr = C_GetRecordEditorID(RecordPtr);

            if (EditorIDPtr == IntPtr.Zero)
                return string.Empty;

            string Result = Marshal.PtrToStringAnsi(EditorIDPtr) ?? string.Empty;

            int NullIndex = Result.IndexOf('\0');
            if (NullIndex >= 0)
                Result = Result.Substring(0, NullIndex);

            return Result;
        }
        public static List<EspRecordInfo> SearchBySig(string ParentSig = "ALL", string ChildSig = "")
        {
            int Count;
            IntPtr ResultsPtr = C_SearchBySig(ParentSig, ChildSig, out Count);

            var Results = new List<EspRecordInfo>();

            if (ResultsPtr == IntPtr.Zero || Count == 0)
            {
                return Results;
            }

            try
            {
                for (int i = 0; i < Count; i++)
                {
                    IntPtr RecordPtr = Marshal.ReadIntPtr(ResultsPtr, i * IntPtr.Size);

                    if (RecordPtr == IntPtr.Zero)
                        continue;

                    var Record = new EspRecordInfo();
                    Record.Handle = RecordPtr;

                    Record.Sig = GetRecordSigUtf8(RecordPtr);                 

                    Record.FormID = C_GetRecordFormID(RecordPtr);
                    Record.EditorID = GetRecordEditorID(RecordPtr);
                    Record.Flags = C_GetRecordFlags(RecordPtr);
                    Record.Index = C_GetRecordIndex(RecordPtr);

                    int SubRecordCount = C_GetSubRecordCount(RecordPtr);

                    for (int j = 0; j < SubRecordCount; j++)
                    {
                        IntPtr SubRecordPtr = C_GetSubRecordData_Ptr(RecordPtr, j);
                        if (SubRecordPtr == IntPtr.Zero)
                            continue;

                        var SubRecord = new SubRecordData();

                        SubRecord.Sig = GetSubRecordSigUtf8(SubRecordPtr);
                        SubRecord.Content = GetSubRecordStringUtf8(SubRecordPtr);
                        SubRecord.IsLocalized = C_SubRecordData_IsLocalized(SubRecordPtr);
                        SubRecord.StringID = C_SubRecordData_GetStringID(SubRecordPtr);

                        SubRecord.OccurrenceIndex = C_SubRecordData_GetOccurrenceIndex(SubRecordPtr);
                        SubRecord.Index = C_SubRecordData_GetIndex(SubRecordPtr);

                        int DataSize = C_SubRecordData_GetDataSize(SubRecordPtr);
                        if (DataSize > 0)
                        {
                            SubRecord.Data = new byte[DataSize];
                            C_SubRecordData_GetData(SubRecordPtr, SubRecord.Data, DataSize);
                        }
                        else
                        {
                            SubRecord.Data = new byte[0];
                        }

                        Record.SubRecords.Add(SubRecord);
                    }

                    Results.Add(Record);
                }

                return Results;
            }
            finally
            {
                FreeSearchResults(ResultsPtr, Count);
            }
        }

        private static string GetSubRecordStringUtf8(IntPtr SubRecordPtr)
        {
            int Len = C_SubRecordData_GetStringUtf8(SubRecordPtr, null, 0);
            if (Len <= 0) return string.Empty;

            byte[] Buffer = new byte[Len + 1]; 
            int ActualLen = C_SubRecordData_GetStringUtf8(SubRecordPtr, Buffer, Buffer.Length);

            int NullIndex = Array.IndexOf(Buffer, (byte)0, 0, ActualLen);
            if (NullIndex >= 0)
                ActualLen = NullIndex;

            string NewStr = Encoding.UTF8.GetString(Buffer, 0, ActualLen);

            return string.Copy(NewStr);
        }

        private static string GetSubRecordSigUtf8(IntPtr SubRecordPtr)
        {
            byte[] Buffer = new byte[8];
            int Len = C_SubRecordData_GetSigUtf8(SubRecordPtr, Buffer, Buffer.Length);
            if (Len <= 0) return string.Empty;

            int NullIndex = Array.IndexOf(Buffer, (byte)0, 0, Len);
            if (NullIndex >= 0)
                Len = NullIndex;

            return Encoding.UTF8.GetString(Buffer, 0, Len);
        }

        private static string GetRecordSigUtf8(IntPtr RecordPtr)
        {
            byte[] Buffer = new byte[8];
            int Len = C_GetRecordSig(RecordPtr, Buffer, Buffer.Length);
            if (Len <= 0) return string.Empty;

            int NullIndex = Array.IndexOf(Buffer, (byte)0, 0, Len);
            if (NullIndex >= 0)
                Len = NullIndex;

            return Encoding.UTF8.GetString(Buffer, 0, Len);
        }
    }

    public static class EspReader
    {
        public static StringsFileReader FromStringsFile = new StringsFileReader();
        public static StringsFileReader ToStringsFile = new StringsFileReader();
        public static string EspPath = "";
        public class RecordItem
        {
            public uint StringID = 0;      // StringsFile id
            public uint RealFormID = 0;
            public string FormID = "";
            public string EditorID = "";
            public string ParentSig = "";
            public string ChildSig = "";
            public string UniqueKey = "";
            public string String = "";
            public int ParentIndex = 0;
            public int SubIndex = 0;
            public int OccurrenceIndex = 0;
            public bool IsModify = false;
        }

        public static Dictionary<string, RecordItem> Records = new Dictionary<string, RecordItem>();
        public static List<string> Types = new List<string>();

        public static void SelectSig(string Sig)
        {
            Records.Clear();

            foreach (var GetRecord in EspInterop.SearchBySig(Sig))
            {
                uint RealFormID = GetRecord.FormID;
                string ParentFormID = GetRecord.GetFormIDHex();
                string ParentSig = GetRecord.Sig;

                string ParentEditorID = GetRecord.EditorID;

                foreach (var Sub in GetRecord.SubRecords)
                {
                    var MergeSig = Phoenix.GetFileUniqueKey() + ":" + ParentFormID + ":" + ParentSig + ":" + Sub.Sig + ":" + Sub.Index + ":" + ParentEditorID;
                    string UniqueKey = "[" + Crc32Helper.ComputeCrc32(MergeSig) + "]" + Sub.Sig;

                    RecordItem NRecordItem = new RecordItem
                    {
                        RealFormID = RealFormID,
                        StringID = Sub.StringID,
                        FormID = ParentFormID,
                        EditorID = ParentEditorID,
                        ParentSig = ParentSig,
                        ChildSig = Sub.Sig,
                        UniqueKey = UniqueKey,
                        String = Sub.Content,
                        ParentIndex = GetRecord.Index,
                        SubIndex = Sub.Index,
                        OccurrenceIndex = Sub.OccurrenceIndex
                    };

                    if (NRecordItem.String.Length > 0)
                    {
                        if (!Records.ContainsKey(NRecordItem.UniqueKey))
                        {
                            Records.Add(NRecordItem.UniqueKey, NRecordItem);
                        }
                        else
                        {
                            throw new Exception("Warning: Duplicate key detected: {NRecordItem.UniqueKey}");
                        }
                    }
                    else
                    {
                        
                    }
                }
            }
        }

        //Fast Load Esp
        public static bool LoadEsp(string Path)
        {
            Records.Clear();
            Types.Clear();
            var State = EspInterop.LoadEsp(Path);

            string Report = EspInterop.GetFieldReport();
            TranslatorExtend.LogCall(Report);

            EspPath = Path;

            if (State >= 0)
            {
                foreach (var GetRecord in EspInterop.SearchBySig("ALL"))
                {
                    string ParentFormID = GetRecord.GetFormIDHex();
                    string ParentSig = GetRecord.Sig;

                    if (!Types.Contains(ParentSig))
                    {
                        Types.Add(ParentSig);
                    }
                }

                return true;
            }

            return false;
        }

        public static void LoadStringsFile()
        {
            FromStringsFile.Close();
            ToStringsFile.Close();

            FromStringsFile.LoadStringsFiles(EspPath, Phoenix.From);
            ToStringsFile.LoadStringsFiles(EspPath, Phoenix.To);
        }

        //public static void TestSaveEsp()
        //{
        //    int ModifyCount = 0;
        //    for (int i = 0; i < Records.Count; i++)
        //    {
        //        var Record = Records[Records.ElementAt(i).Key];

        //        bool IsCell = false;

        //        if (Record.ParentSig == "CELL")
        //        {
        //            IsCell = true;
        //        }

        //        if (EspInterop.ModifySubRecord(Record.RealFormID, Record.ParentSig,Record.ChildSig,Record.OccurrenceIndex,Record.SubIndex,i.ToString()))
        //        {
        //            ModifyCount++;
        //        }
        //    }

        //    EspInterop.SaveEsp(DeFine.GetFullPath(@"\Test.esp"));
        //}

        public static int SaveEsp(string OutPutPath)
        {
            var Get = Translator.TransData;

            int ModifyCount = 0;

            for (int i = 0; i < Records.Count; i++)
            {
                var Record = Records[Records.ElementAt(i).Key];

                var GetTransData = TranslatorBridge.GetTransCache(Record.UniqueKey);
                if (GetTransData != null)
                {
                    if (GetTransData.Length > 0 && GetTransData != Record.String)
                    {
                        bool IsCell = false;

                        if (Record.ParentSig == "CELL")
                        {
                            IsCell = true;
                        }

                        if (ConvertHelper.ObjToLong(GetTransData) > 0)
                        {
                            continue;
                        }

                        if (EspInterop.ModifySubRecordByOffset(IsCell,Record.ParentIndex,Record.SubIndex, GetTransData))
                        {
                            ModifyCount++;
                        }                     
                    }
                }
            }

            EspInterop.SaveEsp(OutPutPath);

            return ModifyCount;
        }

        public static void Close()
        {
            EspPath = String.Empty;
            FromStringsFile.Close();
            ToStringsFile.Close();
            EspInterop.C_Clear();
        }
    }
}