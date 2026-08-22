using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;
using PhoenixTranslator.ApplicationLayer;

namespace PhoenixTranslator.PresetTests
{
    internal static class Program
    {
        private static int Main()
        {
            var tests = new Dictionary<string, Action>
            {
                { nameof(PreservesCustomSettingsOnLoad), PreservesCustomSettingsOnLoad },
                { nameof(AppliesNamedPresetOnce), AppliesNamedPresetOnce },
                { nameof(MarksManualEditsAsCustom), MarksManualEditsAsCustom },
                { nameof(BoundsAndValidatesPresetValues), BoundsAndValidatesPresetValues },
                { nameof(BuildsProjectIndependentDatabaseQuery), BuildsProjectIndependentDatabaseQuery },
                { nameof(BuildsLanguageFilteredDatabaseQuery), BuildsLanguageFilteredDatabaseQuery },
                { nameof(ValidatesReferenceCorpusManifest), ValidatesReferenceCorpusManifest },
                { nameof(ValidatesMcmFixtures), ValidatesMcmFixtures },
                { nameof(ValidatesXmlFixtures), ValidatesXmlFixtures }
            };
            int failures = 0;
            foreach (KeyValuePair<string, Action> test in tests)
            {
                try
                {
                    test.Value();
                    Console.WriteLine("Passed {0}", test.Key);
                }
                catch (Exception exception)
                {
                    failures++;
                    Console.Error.WriteLine("Failed {0}: {1}", test.Key, exception);
                }
            }

            Console.WriteLine("{0} tests passed; {1} failed.", tests.Count - failures, failures);
            return failures == 0 ? 0 : 1;
        }

        private static void PreservesCustomSettingsOnLoad()
        {
            var store = new RecordingStore(
                TranslationPreset.Custom,
                new TranslationPresetSettings(777, 4321, true, true, false));
            var coordinator = CreateCoordinator(store);

            TranslationPresetSelection selection = coordinator.Load();

            AssertEqual(TranslationPreset.Custom, selection.Preset, "Existing values must remain custom.");
            AssertEqual(777, store.Settings.ContextLimit, "Loading must preserve the context limit.");
            AssertEqual(0, store.SaveCalls, "Loading must not save configuration.");
        }

        private static void AppliesNamedPresetOnce()
        {
            var store = new RecordingStore(
                TranslationPreset.Custom,
                new TranslationPresetSettings(777, 4321, true, true, false));
            var coordinator = CreateCoordinator(store);
            TranslationPresetSelection selection;

            AssertEqual(true, coordinator.TrySelect(TranslationPreset.Balanced, out selection),
                "Balanced must be selectable.");
            AssertEqual(200, store.Settings.ContextLimit, "Balanced must apply the complete context limit.");
            AssertEqual(3900, store.Settings.BucketLengthLimit, "Balanced must apply the complete bucket limit.");
            AssertEqual(1, store.SaveCalls, "One selection must save exactly once.");
        }

        private static void MarksManualEditsAsCustom()
        {
            var store = new RecordingStore(
                TranslationPreset.Balanced,
                new TranslationPresetSettings(200, 3900, false, false, false));
            var coordinator = CreateCoordinator(store);
            TranslationPresetSelection selection;

            AssertEqual(true, coordinator.TryApplyCustomSettings(
                new TranslationPresetSettings(250, 3900, false, false, false),
                false,
                out selection),
                "A valid manual edit must be applied.");
            AssertEqual(TranslationPreset.Custom, store.Preset, "A manual edit must select Custom.");
            AssertEqual(0, store.SaveCalls, "A text edit may use the existing shutdown save.");
        }

        private static void BoundsAndValidatesPresetValues()
        {
            var service = new TranslationPresetService();
            foreach (TranslationPreset preset in new[]
            {
                TranslationPreset.Balanced,
                TranslationPreset.QualityFirst,
                TranslationPreset.SpeedFirst
            })
            {
                TranslationPresetProfile profile;
                AssertEqual(true, service.TryGetProfile(preset, out profile),
                    preset + " must define a complete profile.");
                AssertEqual(true, profile.TranslationQuality >= 0 && profile.TranslationQuality <= 100,
                    preset + " quality must stay within the radar range.");
                AssertEqual(true, profile.TranslationSpeed >= 0 && profile.TranslationSpeed <= 100,
                    preset + " speed must stay within the radar range.");
            }

            AssertEqual(false, service.IsValid(
                new TranslationPresetSettings(0, 3900, false, false, false)),
                "Zero context length must be rejected.");
        }

        private static void BuildsProjectIndependentDatabaseQuery()
        {
            AssertEqual(
                "Select * From AdvancedDictionary Limit 100000",
                AdvancedDictionaryQueryBuilder.Build(null, null),
                "Opening the database without a project must use a bounded query.");
        }

        private static void BuildsLanguageFilteredDatabaseQuery()
        {
            AssertEqual(
                "Select * From AdvancedDictionary Where [From] = 1 And [To] = 2 Limit 100000",
                AdvancedDictionaryQueryBuilder.Build(1, 2),
                "Opening the database with a project must retain its language filter.");
        }

        private static void ValidatesReferenceCorpusManifest()
        {
            string testDataDirectory = GetTestDataDirectory();
            string[] rows = File.ReadAllLines(Path.Combine(testDataDirectory, "manifest.tsv"));
            AssertEqual(5, rows.Length, "The fixture manifest must contain four records and one header.");

            foreach (string row in rows.Skip(1))
            {
                string[] columns = row.Split('\t');
                AssertEqual(4, columns.Length, "Every fixture manifest row must contain four columns.");
                string fixturePath = Path.Combine(
                    testDataDirectory,
                    columns[0].Replace('/', Path.DirectorySeparatorChar));
                AssertEqual(true, File.Exists(fixturePath), "Every manifest entry must resolve to a fixture.");
                AssertEqual(columns[1], ComputeSha256(fixturePath), "Fixture hashes must remain reproducible.");
                AssertEqual("Project-owned", columns[2], "Synthetic fixtures must retain their ownership marker.");
                AssertEqual("Synthetic", columns[3], "External content must not enter the synthetic corpus.");
            }
        }

        private static void ValidatesMcmFixtures()
        {
            string testDataDirectory = GetTestDataDirectory();
            string[] validLines = File.ReadAllLines(Path.Combine(testDataDirectory, "Mcm", "valid-mcm-english.txt"));
            string[] malformedLines = File.ReadAllLines(Path.Combine(testDataDirectory, "Mcm", "malformed-mcm.txt"));

            AssertEqual(true, LooksLikeMcm(validLines), "The valid MCM fixture must retain its expected structure.");
            AssertEqual(false, LooksLikeMcm(malformedLines), "The malformed MCM fixture must remain invalid.");
            AssertEqual(true, validLines[1].Contains("\\n"), "The valid fixture must cover escaped line breaks.");
        }

        private static void ValidatesXmlFixtures()
        {
            string testDataDirectory = GetTestDataDirectory();
            XDocument validDocument = XDocument.Load(Path.Combine(testDataDirectory, "Xml", "valid-translation.xml"));
            AssertEqual(2, validDocument.Descendants("String").Count(),
                "The valid XML fixture must contain two translation records.");

            bool rejected = false;
            try
            {
                XDocument.Load(Path.Combine(testDataDirectory, "Xml", "malformed-translation.xml"));
            }
            catch (XmlException)
            {
                rejected = true;
            }

            AssertEqual(true, rejected, "The malformed XML fixture must fail deterministic parsing.");
        }

        private static string GetTestDataDirectory()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData");
        }

        private static string ComputeSha256(string path)
        {
            using (SHA256 sha256 = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
            {
                return BitConverter.ToString(sha256.ComputeHash(stream)).Replace("-", string.Empty).ToLowerInvariant();
            }
        }

        private static bool LooksLikeMcm(IEnumerable<string> lines)
        {
            int remainingCandidates = 2;
            foreach (string line in lines)
            {
                string value = line.TrimStart('\uFEFF').Trim();
                if (value.Length == 0)
                {
                    continue;
                }

                remainingCandidates--;
                if ((value.StartsWith("$") || value.StartsWith("#")) &&
                    (value.Contains("\t") || value.Contains(" ")))
                {
                    return true;
                }

                if (remainingCandidates == 0)
                {
                    break;
                }
            }

            return false;
        }

        private static TranslationPresetCoordinator CreateCoordinator(RecordingStore store)
        {
            return new TranslationPresetCoordinator(new TranslationPresetService(), store);
        }

        private static void AssertEqual<T>(T expected, T actual, string message)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new InvalidOperationException(string.Format(
                    "{0} Expected <{1}> but received <{2}>.",
                    message,
                    expected,
                    actual));
            }
        }

        private sealed class RecordingStore : ITranslationPresetStore
        {
            internal RecordingStore(TranslationPreset preset, TranslationPresetSettings settings)
            {
                Preset = preset;
                Settings = settings;
            }

            public TranslationPreset Preset { get; set; }

            internal TranslationPresetSettings Settings { get; private set; }

            internal int SaveCalls { get; private set; }

            public TranslationPresetSettings ReadSettings()
            {
                return Settings;
            }

            public void ApplySettings(TranslationPresetSettings settings)
            {
                Settings = settings;
            }

            public void Save()
            {
                SaveCalls++;
            }
        }
    }
}
