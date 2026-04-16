using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace LexTranslator.SkyrimManagement
{
    // ============================================================
    //  Data transfer objects  (unchanged from original)
    // ============================================================
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
        public IntPtr Handle { get; set; }
        public string Sig { get; set; }
        public uint FormID { get; set; }
        public uint Flags { get; set; }
        public int Index { get; set; }
        public string EditorID { get; set; }
        public List<SubRecordData> SubRecords { get; set; } = new List<SubRecordData>();

        public string GetUniqueKey() => $"{Sig}:{FormID}";
        public string GetFormIDHex() => $"{FormID:X8}";
        public string GetEditorID() => SubRecords.Find(s => s.Sig == "EDID")?.Content ?? "";
        public string GetDisplayName()
        {
            var full = SubRecords.Find(s => s.Sig == "FULL");
            return (!string.IsNullOrEmpty(full?.Content)) ? full.Content : GetEditorID();
        }
    }

    public class CharacterRecordInfo
    {
        public uint NpcFormID { get; set; }
        public string Name { get; set; } = "";
        public string EditorID { get; set; } = "";
        public string VoiceType { get; set; } = "";
        public int Gender { get; set; }   // 0=Unknown 1=Male 2=Female

        public List<uint> LinkedInfos { get; set; } = new List<uint>();
        public List<uint> LinkedFactions { get; set; } = new List<uint>();
        public List<uint> LinkedRaces { get; set; } = new List<uint>();
        public List<uint> LinkedVoiceTypes { get; set; } = new List<uint>();

        public string GenderString => Gender == 1 ? "Male" : Gender == 2 ? "Female" : "Unknown";
        public string FormIDHex => $"{NpcFormID:X8}";
    }

    // ============================================================
    //  Raw P/Invoke  –  every DLL function now takes handle first
    // ============================================================
    internal static class EspNative
    {
        private const string DllName = "EspReader.dll";

        // Lifecycle
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr C_CreateInstance();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void C_DestroyInstance(IntPtr handle);

        // Version  (no handle – global)
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr C_GetVersion();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_GetVersionLength();

        // Filter
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void C_InitDefaultFilter(IntPtr handle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_SetDefaultFilter(IntPtr handle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void C_ClearFilter(IntPtr handle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_SetFilter(
            IntPtr handle,
            [MarshalAs(UnmanagedType.LPStr)] string parentSig,
            [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] childSigs,
            int childCount);

        // IO
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int C_ReadEsp(IntPtr handle, [MarshalAs(UnmanagedType.LPWStr)] string espPath);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern bool C_SaveEsp(IntPtr handle, IntPtr utf8Path);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void C_Clear(IntPtr handle);

        // Field report
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr C_GetFieldReport(IntPtr handle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_GetFieldReportLength(IntPtr handle);

        // Search
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr C_SearchBySig(
            IntPtr handle,
            [MarshalAs(UnmanagedType.LPStr)] string parentSig,
            [MarshalAs(UnmanagedType.LPStr)] string childSig,
            out int outCount);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void FreeSearchResults(IntPtr arr, int count);

        // Record accessors  (record pointer only – no instance)
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_GetRecordSig(IntPtr record, byte[] buffer, int bufferSize);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint C_GetRecordFormID(IntPtr record);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr C_GetRecordEditorID(IntPtr record);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint C_GetRecordFlags(IntPtr record);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_GetRecordIndex(IntPtr record);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_GetSubRecordCount(IntPtr record);

        // SubRecord accessors
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr C_GetSubRecordData_Ptr(IntPtr record, int index);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_SubRecordData_GetOccurrenceIndex(IntPtr sub);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_SubRecordData_GetIndex(IntPtr sub);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_SubRecordData_GetStringUtf8(IntPtr sub, byte[] buffer, int bufferSize);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_SubRecordData_GetSigUtf8(IntPtr sub, byte[] buffer, int bufferSize);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool C_SubRecordData_IsLocalized(IntPtr sub);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint C_SubRecordData_GetStringID(IntPtr sub);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_SubRecordData_GetDataSize(IntPtr sub);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool C_SubRecordData_GetData(IntPtr sub, byte[] buffer, int bufferSize);

        // Modify
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool C_ModifySubRecordByOffset(IntPtr handle, int isCell, int recordOffset, int subOffset, IntPtr newUtf8Data);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool C_ModifySubRecord(IntPtr handle, uint formID, IntPtr recordSig, IntPtr subSig, int occurrenceIndex, int globalIndex, IntPtr newUtf8Data);

        // Character tracker
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void C_ClearCharacterTracker(IntPtr handle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_GetCharacterCount(IntPtr handle);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint C_GetCharacterFormID(IntPtr handle, int index);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_GetCharacterGender(IntPtr handle, int index);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_GetCharacterName(IntPtr handle, int index, byte[] buffer, int bufferSize);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_GetCharacterEditorID(IntPtr handle, int index, byte[] buffer, int bufferSize);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_GetCharacterVoiceType(IntPtr handle, int index, byte[] buffer, int bufferSize);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_GetCharacterLinkedInfoCount(IntPtr handle, int index);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint C_GetCharacterLinkedInfo(IntPtr handle, int index, int linkIndex);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_GetCharacterLinkedFactionCount(IntPtr handle, int index);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint C_GetCharacterLinkedFaction(IntPtr handle, int index, int linkIndex);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_GetCharacterLinkedRaceCount(IntPtr handle, int index);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint C_GetCharacterLinkedRace(IntPtr handle, int index, int linkIndex);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int C_GetCharacterLinkedVoiceTypeCount(IntPtr handle, int index);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint C_GetCharacterLinkedVoiceType(IntPtr handle, int index, int linkIndex);
    }

    // ============================================================
    //  EspReader  –  managed wrapper, one instance per object
    //  Use pattern identical to PexReader.
    // ============================================================
    public class EspReader : IDisposable
    {
        private IntPtr _Handle;
        private bool _Disposed = false;

        public static string DllVersion { get; } = ReadDllVersion();

        public string EspPath { get; private set; } = "";

        // ── Constructor / destructor ──────────────────────────
        public EspReader()
        {
            _Handle = EspNative.C_CreateInstance();
            if (_Handle == IntPtr.Zero)
                throw new InvalidOperationException("Failed to create EspInstance in native DLL.");

            EspNative.C_InitDefaultFilter(_Handle);
            EspNative.C_SetDefaultFilter(_Handle);
        }

        public void Dispose()
        {
            if (!_Disposed)
            {
                if (_Handle != IntPtr.Zero)
                {
                    EspNative.C_DestroyInstance(_Handle);
                    _Handle = IntPtr.Zero;
                }
                _Disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        ~EspReader() { Dispose(); }

        private void EnsureNotDisposed()
        {
            if (_Disposed || _Handle == IntPtr.Zero)
                throw new ObjectDisposedException(nameof(EspReader));
        }

        // ── Version ──────────────────────────────────────────
        private static string ReadDllVersion()
        {
            try
            {
                int len = EspNative.C_GetVersionLength();
                if (len <= 0) return "Unknown";
                IntPtr ptr = EspNative.C_GetVersion();
                return ptr == IntPtr.Zero ? "Unknown" : Marshal.PtrToStringAnsi(ptr, len);
            }
            catch { return "Error"; }
        }

        // ── Filter helpers ───────────────────────────────────
        public void SetDefaultFilter()
        {
            EnsureNotDisposed();
            EspNative.C_InitDefaultFilter(_Handle);
            EspNative.C_SetDefaultFilter(_Handle);
        }

        public void SetFilter(Dictionary<string, string[]> filterConfig)
        {
            EnsureNotDisposed();
            EspNative.C_ClearFilter(_Handle);
            foreach (var kvp in filterConfig)
                EspNative.C_SetFilter(_Handle, kvp.Key, kvp.Value, kvp.Value.Length);
        }

        public void ClearFilter()
        {
            EnsureNotDisposed();
            EspNative.C_ClearFilter(_Handle);
        }

        // ── IO ───────────────────────────────────────────────
        /// <summary>
        /// Load an ESP/ESM file.  Returns true on success.
        /// </summary>
        public bool LoadEsp(string path)
        {
            EnsureNotDisposed();
            if (!File.Exists(path)) return false;
            int result = EspNative.C_ReadEsp(_Handle, path);
            if (result == 0) EspPath = path;
            return result == 0;
        }

        /// <summary>
        /// Save modified records to <paramref name="outputPath"/>.
        /// </summary>
        public bool SaveEsp(string outputPath)
        {
            EnsureNotDisposed();
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = StringToUtf8Ptr(outputPath);
                return EspNative.C_SaveEsp(_Handle, ptr);
            }
            finally { if (ptr != IntPtr.Zero) Marshal.FreeHGlobal(ptr); }
        }

        /// <summary>
        /// Release parsed data without destroying the instance or its filter.
        /// </summary>
        public void Clear()
        {
            EnsureNotDisposed();
            EspPath = string.Empty;
            EspNative.C_Clear(_Handle);
        }

        // ── Field report ─────────────────────────────────────
        public string GetFieldReport()
        {
            EnsureNotDisposed();
            int len = EspNative.C_GetFieldReportLength(_Handle);
            if (len <= 0) return "Validator not initialized";
            IntPtr ptr = EspNative.C_GetFieldReport(_Handle);
            if (ptr == IntPtr.Zero) return "Validator not initialized";
            byte[] buf = new byte[len];
            Marshal.Copy(ptr, buf, 0, len);
            return Encoding.UTF8.GetString(buf);
        }

        // ── Search ───────────────────────────────────────────
        public List<EspRecordInfo> SearchBySig(string parentSig = "ALL", string childSig = "")
        {
            EnsureNotDisposed();
            var results = new List<EspRecordInfo>();

            int count;
            IntPtr resultsPtr = EspNative.C_SearchBySig(_Handle, parentSig, childSig, out count);
            if (resultsPtr == IntPtr.Zero || count == 0) return results;

            try
            {
                for (int i = 0; i < count; i++)
                {
                    IntPtr recPtr = Marshal.ReadIntPtr(resultsPtr, i * IntPtr.Size);
                    if (recPtr == IntPtr.Zero) continue;

                    var rec = new EspRecordInfo
                    {
                        Handle = recPtr,
                        Sig = GetRecordSigUtf8(recPtr),
                        FormID = EspNative.C_GetRecordFormID(recPtr),
                        Flags = EspNative.C_GetRecordFlags(recPtr),
                        Index = EspNative.C_GetRecordIndex(recPtr),
                        EditorID = GetRecordEditorIDStr(recPtr),
                    };

                    int subCount = EspNative.C_GetSubRecordCount(recPtr);
                    for (int j = 0; j < subCount; j++)
                    {
                        IntPtr subPtr = EspNative.C_GetSubRecordData_Ptr(recPtr, j);
                        if (subPtr == IntPtr.Zero) continue;

                        var sub = new SubRecordData
                        {
                            Sig = GetSubRecordSigUtf8(subPtr),
                            Content = GetSubRecordStringUtf8(subPtr),
                            IsLocalized = EspNative.C_SubRecordData_IsLocalized(subPtr),
                            StringID = EspNative.C_SubRecordData_GetStringID(subPtr),
                            OccurrenceIndex = EspNative.C_SubRecordData_GetOccurrenceIndex(subPtr),
                            Index = EspNative.C_SubRecordData_GetIndex(subPtr),
                        };

                        int dataSize = EspNative.C_SubRecordData_GetDataSize(subPtr);
                        sub.Data = dataSize > 0 ? new byte[dataSize] : new byte[0];
                        if (dataSize > 0) EspNative.C_SubRecordData_GetData(subPtr, sub.Data, dataSize);

                        rec.SubRecords.Add(sub);
                    }

                    results.Add(rec);
                }
            }
            finally { EspNative.FreeSearchResults(resultsPtr, count); }

            return results;
        }

        // ── Modify ───────────────────────────────────────────
        public bool ModifySubRecordByOffset(bool isCell, int parentIndex, int subIndex, string newData)
        {
            EnsureNotDisposed();
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = StringToUtf8Ptr(newData ?? "");
                return EspNative.C_ModifySubRecordByOffset(_Handle, isCell ? 1 : 0, parentIndex, subIndex, ptr);
            }
            finally { if (ptr != IntPtr.Zero) Marshal.FreeHGlobal(ptr); }
        }

        public bool ModifySubRecord(uint formId, string recordSig, string subSig,
            int occurrenceIndex, int globalIndex, string newData)
        {
            EnsureNotDisposed();
            IntPtr pRec = IntPtr.Zero, pSub = IntPtr.Zero, pData = IntPtr.Zero;
            try
            {
                pRec = StringToUtf8Ptr(recordSig ?? "");
                pSub = StringToUtf8Ptr(subSig ?? "");
                pData = StringToUtf8Ptr(newData ?? "");
                return EspNative.C_ModifySubRecord(_Handle, formId, pRec, pSub, occurrenceIndex, globalIndex, pData);
            }
            finally
            {
                if (pRec != IntPtr.Zero) Marshal.FreeHGlobal(pRec);
                if (pSub != IntPtr.Zero) Marshal.FreeHGlobal(pSub);
                if (pData != IntPtr.Zero) Marshal.FreeHGlobal(pData);
            }
        }

        // ── Character tracker ────────────────────────────────
        public void ClearCharacterTracker()
        {
            EnsureNotDisposed();
            EspNative.C_ClearCharacterTracker(_Handle);
        }

        public List<CharacterRecordInfo> GetAllCharacters()
        {
            EnsureNotDisposed();
            int count = EspNative.C_GetCharacterCount(_Handle);
            var list = new List<CharacterRecordInfo>(count);

            for (int i = 0; i < count; i++)
            {
                var ch = new CharacterRecordInfo
                {
                    NpcFormID = EspNative.C_GetCharacterFormID(_Handle, i),
                    Name = GetCharUtf8(i, EspNative.C_GetCharacterName),
                    EditorID = GetCharUtf8(i, EspNative.C_GetCharacterEditorID),
                    VoiceType = GetCharUtf8(i, EspNative.C_GetCharacterVoiceType),
                    Gender = EspNative.C_GetCharacterGender(_Handle, i),
                };

                int n;
                n = EspNative.C_GetCharacterLinkedInfoCount(_Handle, i);
                for (int j = 0; j < n; j++) ch.LinkedInfos.Add(EspNative.C_GetCharacterLinkedInfo(_Handle, i, j));

                n = EspNative.C_GetCharacterLinkedFactionCount(_Handle, i);
                for (int j = 0; j < n; j++) ch.LinkedFactions.Add(EspNative.C_GetCharacterLinkedFaction(_Handle, i, j));

                n = EspNative.C_GetCharacterLinkedRaceCount(_Handle, i);
                for (int j = 0; j < n; j++) ch.LinkedRaces.Add(EspNative.C_GetCharacterLinkedRace(_Handle, i, j));

                n = EspNative.C_GetCharacterLinkedVoiceTypeCount(_Handle, i);
                for (int j = 0; j < n; j++) ch.LinkedVoiceTypes.Add(EspNative.C_GetCharacterLinkedVoiceType(_Handle, i, j));

                list.Add(ch);
            }
            return list;
        }

        // ── Private helpers ──────────────────────────────────
        private string GetCharUtf8(int index, Func<IntPtr, int, byte[], int, int> getter)
        {
            int len = getter(_Handle, index, null, 0);
            if (len <= 0) return string.Empty;
            byte[] buf = new byte[len + 1];
            int actual = getter(_Handle, index, buf, buf.Length);
            int nullIdx = Array.IndexOf(buf, (byte)0, 0, actual);
            if (nullIdx >= 0) actual = nullIdx;
            return Encoding.UTF8.GetString(buf, 0, actual);
        }

        private static string GetSubRecordStringUtf8(IntPtr subPtr)
        {
            int len = EspNative.C_SubRecordData_GetStringUtf8(subPtr, null, 0);
            if (len <= 0) return string.Empty;
            byte[] buf = new byte[len + 1];
            int actual = EspNative.C_SubRecordData_GetStringUtf8(subPtr, buf, buf.Length);
            int nullIdx = Array.IndexOf(buf, (byte)0, 0, actual);
            if (nullIdx >= 0) actual = nullIdx;
            return string.Copy(Encoding.UTF8.GetString(buf, 0, actual));
        }

        private static string GetSubRecordSigUtf8(IntPtr subPtr)
        {
            byte[] buf = new byte[8];
            int len = EspNative.C_SubRecordData_GetSigUtf8(subPtr, buf, buf.Length);
            if (len <= 0) return string.Empty;
            int nullIdx = Array.IndexOf(buf, (byte)0, 0, len);
            if (nullIdx >= 0) len = nullIdx;
            return Encoding.UTF8.GetString(buf, 0, len);
        }

        private static string GetRecordSigUtf8(IntPtr recPtr)
        {
            byte[] buf = new byte[8];
            int len = EspNative.C_GetRecordSig(recPtr, buf, buf.Length);
            if (len <= 0) return string.Empty;
            int nullIdx = Array.IndexOf(buf, (byte)0, 0, len);
            if (nullIdx >= 0) len = nullIdx;
            return Encoding.UTF8.GetString(buf, 0, len);
        }

        private static string GetRecordEditorIDStr(IntPtr recPtr)
        {
            if (recPtr == IntPtr.Zero) return string.Empty;
            IntPtr ptr = EspNative.C_GetRecordEditorID(recPtr);
            if (ptr == IntPtr.Zero) return string.Empty;
            string s = Marshal.PtrToStringAnsi(ptr) ?? string.Empty;
            int idx = s.IndexOf('\0');
            return idx >= 0 ? s.Substring(0, idx) : s;
        }

        private static IntPtr StringToUtf8Ptr(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s ?? "");
            IntPtr ptr = Marshal.AllocHGlobal(bytes.Length + 1);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            Marshal.WriteByte(ptr, bytes.Length, 0);
            return ptr;
        }
    }

    // ============================================================
    //  Usage example – two independent instances
    // ============================================================
    /*
    using (var reader1 = new EspReader())
    using (var reader2 = new EspReader())
    {
        reader1.LoadEsp(@"C:\Skyrim\Data\Mod1.esp");
        reader2.LoadEsp(@"C:\Skyrim\Data\Mod2.esp");

        var records1 = reader1.SearchBySig("NPC_");
        var records2 = reader2.SearchBySig("NPC_");

        reader1.ModifySubRecord(0x12345678, "NPC_", "FULL", 0, 0, "New Name");
        reader1.SaveEsp(@"C:\Skyrim\Data\Mod1_translated.esp");
    }  // both instances destroyed here
    */
}