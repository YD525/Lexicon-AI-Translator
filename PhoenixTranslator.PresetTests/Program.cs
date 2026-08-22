using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using PhoenixTranslator.ApplicationLayer;
using PhoenixTranslator.Properties;

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
                { nameof(ValidatesXmlFixtures), ValidatesXmlFixtures },
                { nameof(ValidatesPreviewMessageIdentifiers), ValidatesPreviewMessageIdentifiers },
                { nameof(FormatsPreviewMessages), FormatsPreviewMessages },
                { nameof(NavigatesPreviewShell), NavigatesPreviewShell },
                { nameof(TracksPreviewShellStatus), TracksPreviewShellStatus }
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

        private static void ValidatesPreviewMessageIdentifiers()
        {
            var entries = new Dictionary<string, string>();
            ResourceSet resourceSet = Resources.ResourceManager.GetResourceSet(
                CultureInfo.InvariantCulture,
                true,
                true);
            foreach (DictionaryEntry entry in resourceSet)
            {
                entries.Add((string)entry.Key, (string)entry.Value);
            }

            AssertEqual(true, entries.Count >= 70, "The preview source catalogue must retain its baseline coverage.");
            foreach (KeyValuePair<string, string> entry in entries)
            {
                AssertEqual(true,
                    Regex.IsMatch(entry.Key, "^[A-Z][A-Za-z0-9]*(?:_[A-Z][A-Za-z0-9]*)+$"),
                    entry.Key + " must use stable PascalCase segments separated by underscores.");
                AssertEqual(false, string.IsNullOrWhiteSpace(entry.Value), entry.Key + " must have English source text.");

                MatchCollection placeholders = Regex.Matches(entry.Value, "\\{(\\d+)(?:[^}]*)\\}");
                if (placeholders.Count > 0)
                {
                    int[] indexes = placeholders.Cast<Match>()
                        .Select(match => int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture))
                        .Distinct()
                        .OrderBy(index => index)
                        .ToArray();
                    AssertEqual(
                        string.Join(",", Enumerable.Range(0, indexes[indexes.Length - 1] + 1)),
                        string.Join(",", indexes),
                        entry.Key + " placeholders must be contiguous from zero.");
                }
            }

            foreach (string prefix in new[] { "Common_", "Shell_", "Workspace_", "Settings_", "Accessibility_" })
            {
                AssertEqual(true, entries.Keys.Any(id => id.StartsWith(prefix, StringComparison.Ordinal)),
                    "The source catalogue must cover " + prefix.TrimEnd('_') + ".");
            }
        }

        private static void FormatsPreviewMessages()
        {
            AssertEqual(
                "Opening project: 42%",
                PreviewMessageCatalog.Format("Shell_ProjectOpen_OpeningProgress", 42),
                "Project progress must format with the current UI culture.");
            AssertEqual(
                "Translating 3 of 10 entries",
                PreviewMessageCatalog.Format("Workspace_Operation_TranslatingProgress", 3, 10),
                "Workspace progress must preserve both documented placeholders.");

            bool rejected = false;
            try
            {
                PreviewMessageCatalog.Get("Unknown_Message_Id");
            }
            catch (InvalidOperationException)
            {
                rejected = true;
            }

            AssertEqual(true, rejected, "Unknown preview message identifiers must fail explicitly.");
        }

        private static void NavigatesPreviewShell()
        {
            int legacyOpenCalls = 0;
            var viewModel = new PreviewShellViewModel(() => legacyOpenCalls++);

            AssertEqual(PreviewShellDestination.Projects, viewModel.CurrentDestination,
                "The preview shell must start at Projects.");
            AssertEqual("Projects", viewModel.CurrentTitle,
                "The initial destination must expose its localized title.");

            viewModel.NavigateCommand.Execute("Review");

            AssertEqual(PreviewShellDestination.Review, viewModel.CurrentDestination,
                "A keyboard command parameter must select the matching destination.");
            AssertEqual("Review", viewModel.CurrentTitle,
                "Navigation must update the localized destination title.");

            viewModel.OpenLegacyWorkspaceCommand.Execute(null);
            AssertEqual(1, legacyOpenCalls,
                "The fallback command must delegate to the legacy workspace integration exactly once.");
        }

        private static void TracksPreviewShellStatus()
        {
            var viewModel = new PreviewShellViewModel(() => { });

            AssertEqual("No project open", viewModel.ProjectIdentity,
                "The shell must expose an explicit no-project state.");
            AssertEqual("Ready", viewModel.StatusText,
                "The shell must start with an explicit idle status.");

            viewModel.SetProject("Example.esp", true);
            viewModel.SetWarningCount(3);
            viewModel.SetOperation("Shell_ProjectOpen_OpeningProgress", 42, 42);

            AssertEqual("Example.esp", viewModel.ProjectIdentity,
                "The shell must retain the safe project display name.");
            AssertEqual(true, viewModel.IsModified,
                "The shell must retain the unsaved project state.");
            AssertEqual("3 warnings", viewModel.WarningText,
                "The shell must format the persistent warning count.");
            AssertEqual("Opening project: 42%", viewModel.StatusText,
                "An active operation must replace the idle status.");
            AssertEqual(42d, viewModel.OperationProgress,
                "The shell must retain bounded operation progress.");

            viewModel.ShowNotification(
                PreviewShellNotificationSeverity.Error,
                "Shell_ProjectOpen_Failed");
            AssertEqual(true, viewModel.HasNotification,
                "The shell must expose a persistent error boundary.");
            AssertEqual(PreviewShellNotificationSeverity.Error, viewModel.NotificationSeverity,
                "The shell must retain notification severity independently from its text.");
            AssertEqual("The project could not be opened.", viewModel.NotificationMessage,
                "The error boundary must expose registered user-safe text.");
            viewModel.DismissNotificationCommand.Execute(null);
            AssertEqual(false, viewModel.HasNotification,
                "A non-blocking notification must be dismissible by command.");

            viewModel.CompleteOperation();
            AssertEqual("Ready", viewModel.StatusText,
                "Completing an operation must restore the idle status.");
            AssertEqual(0d, viewModel.OperationProgress,
                "Completing an operation must clear stale progress.");
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
