using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
                { nameof(TracksPreviewShellStatus), TracksPreviewShellStatus },
                { nameof(OpensProjectThroughHubAndPersistsRecent), OpensProjectThroughHubAndPersistsRecent },
                { nameof(ProtectsUnsavedProjectReplacement), ProtectsUnsavedProjectReplacement },
                { nameof(PersistsBoundedRecentProjects), PersistsBoundedRecentProjects },
                { nameof(FiltersPreviewTranslationWorkspace), FiltersPreviewTranslationWorkspace },
                { nameof(HandlesPreviewTranslationFailures), HandlesPreviewTranslationFailures },
                { nameof(CancelsPreviewTranslationWork), CancelsPreviewTranslationWork },
                { nameof(FiltersLargePreviewTranslationProject), FiltersLargePreviewTranslationProject },
                { nameof(AnalyzesMixedQualityFindings), AnalyzesMixedQualityFindings },
                { nameof(TracksReviewDecisionsAndBulkUndo), TracksReviewDecisionsAndBulkUndo },
                { nameof(PersistsPrivateReviewMetadata), PersistsPrivateReviewMetadata },
                { nameof(NavigatesFromFindingToTranslationEntry), NavigatesFromFindingToTranslationEntry },
                { nameof(ClassifiesProjectRevisionChanges), ClassifiesProjectRevisionChanges },
                { nameof(PreservesReviewedTargetsAsConflicts), PreservesReviewedTargetsAsConflicts },
                { nameof(PersistsPrivateRevisionHistory), PersistsPrivateRevisionHistory },
                { nameof(AppliesAndUndoesExplicitRevisionReuse), AppliesAndUndoesExplicitRevisionReuse },
                { nameof(StagesAndCancelsUnifiedSettings), StagesAndCancelsUnifiedSettings },
                { nameof(ValidatesAndAppliesUnifiedSettings), ValidatesAndAppliesUnifiedSettings },
                { nameof(SearchesSettingsByLegacyTerminology), SearchesSettingsByLegacyTerminology },
                { nameof(ProtectsSettingsSecrets), ProtectsSettingsSecrets },
                { nameof(TestsProviderWithoutSavingSettings), TestsProviderWithoutSavingSettings },
                { nameof(ClearsStagedCredentialWhenProviderChanges), ClearsStagedCredentialWhenProviderChanges }
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
            XDocument reviewDocument = XDocument.Load(
                Path.Combine(testDataDirectory, "Xml", "mixed-review-quality.xml"));
            AssertEqual(5, reviewDocument.Descendants("String").Count(),
                "The mixed review fixture must retain every deterministic finding scenario.");

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

            foreach (string prefix in new[]
            {
                "Common_", "Shell_", "Workspace_", "Review_", "Quality_", "Settings_", "Accessibility_"
            })
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
            AssertEqual("Error", viewModel.NotificationSeverityText,
                "A notification must expose a non-color semantic severity label.");
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

        private static void OpensProjectThroughHubAndPersistsRecent()
        {
            string projectPath = Path.Combine(Path.GetTempPath(), "PhoenixProjectHubTests", Guid.NewGuid().ToString("N"), "project.xml");
            Directory.CreateDirectory(Path.GetDirectoryName(projectPath));
            File.WriteAllText(projectPath, "fixture");
            try
            {
                var project = new FakePreviewTranslationProject(
                    new[] { new PreviewTranslationEntry("1", "XML", "ENTRY", "Source", string.Empty, 100) },
                    projectPath);
                var shell = new PreviewShellViewModel(() => { });
                var workspace = new PreviewTranslationWorkspaceViewModel(
                    () => projectPath, () => { }, shell, path => project);
                var store = new RecordingRecentProjectStore();
                var hub = new PreviewProjectHubViewModel(
                    workspace, shell, () => projectPath, () => true, store, () => { });

                bool opened = hub.OpenProjectAsync(projectPath).GetAwaiter().GetResult();

                AssertEqual(true, opened,
                    "The Project Hub must report a successful normalized project open.");
                AssertEqual(PreviewShellDestination.Translate, shell.CurrentDestination,
                    "A successful project open must enter the translation workflow.");
                AssertEqual(1, hub.RecentProjects.Count,
                    "A successful project open must create one bounded recent reference.");
                AssertEqual("project.xml", hub.RecentProjects[0].DisplayName,
                    "Recent project UI metadata must expose only the safe file name.");
                AssertEqual(1, store.SaveCalls,
                    "A successful open must persist recent references exactly once.");
                hub.Dispose();
                workspace.Dispose();
            }
            finally
            {
                Directory.Delete(Path.GetDirectoryName(projectPath), true);
            }
        }

        private static void ProtectsUnsavedProjectReplacement()
        {
            string directory = Path.Combine(Path.GetTempPath(), "PhoenixProjectHubTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string firstPath = Path.Combine(directory, "first.xml");
            string secondPath = Path.Combine(directory, "second.xml");
            File.WriteAllText(firstPath, "first");
            File.WriteAllText(secondPath, "second");
            try
            {
                var firstProject = new FakePreviewTranslationProject(
                    new[] { new PreviewTranslationEntry("1", "XML", "ENTRY", "First", string.Empty, 100) },
                    firstPath);
                var secondProject = new FakePreviewTranslationProject(
                    new[] { new PreviewTranslationEntry("2", "XML", "ENTRY", "Second", string.Empty, 100) },
                    secondPath);
                var shell = new PreviewShellViewModel(() => { });
                var workspace = new PreviewTranslationWorkspaceViewModel(
                    () => firstPath,
                    () => { },
                    shell,
                    path => string.Equals(path, firstPath, StringComparison.Ordinal) ? firstProject : secondProject);
                var hub = new PreviewProjectHubViewModel(
                    workspace,
                    shell,
                    () => secondPath,
                    () => false,
                    new RecordingRecentProjectStore(),
                    () => { });

                AssertEqual(true, hub.OpenProjectAsync(firstPath).GetAwaiter().GetResult(),
                    "The initial project must open without an unsaved-state prompt.");
                workspace.SelectedEntry.TargetText = "Unsaved target";
                AssertEqual(false, hub.OpenProjectAsync(secondPath).GetAwaiter().GetResult(),
                    "Cancelling project replacement must reject the new project.");
                AssertEqual(firstPath, workspace.ProjectPath,
                    "Cancelling replacement must preserve the previous usable project.");
                AssertEqual("First", workspace.SelectedEntry.SourceText,
                    "Cancelling replacement must preserve normalized project content.");
                hub.Dispose();
                workspace.Dispose();
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        private static void PersistsBoundedRecentProjects()
        {
            string directory = Path.Combine(Path.GetTempPath(), "PhoenixProjectHubStoreTests", Guid.NewGuid().ToString("N"));
            string storePath = Path.Combine(directory, "recent.xml");
            Directory.CreateDirectory(directory);
            try
            {
                var store = new PreviewRecentProjectStore(storePath);
                var projects = Enumerable.Range(0, 15)
                    .Select(index => new PreviewRecentProject(
                        Path.Combine(directory, "project-" + index + ".xml"),
                        DateTime.UtcNow.AddMinutes(-index)))
                    .ToArray();

                store.Save(projects);
                IReadOnlyList<PreviewRecentProject> loaded = store.Load();

                AssertEqual(12, loaded.Count,
                    "Recent-project persistence must enforce its documented bound.");
                AssertEqual("project-0.xml", loaded[0].DisplayName,
                    "Recent projects must retain newest-first ordering.");
                AssertEqual(false, File.ReadAllText(storePath).Contains("Source"),
                    "Recent-project persistence must not contain translation content.");

                store.Save(loaded);
                AssertEqual(12, store.Load().Count,
                    "Atomic replacement must preserve the bounded recent-project list.");

                File.WriteAllText(storePath, "<!DOCTYPE recentProjects [<!ENTITY probe SYSTEM 'file:///missing'>]><recentProjects>&probe;</recentProjects>");
                AssertEqual(0, store.Load().Count,
                    "Recent-project parsing must reject DTD-backed content without resolving it.");
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        private static void FiltersPreviewTranslationWorkspace()
        {
            var shell = new PreviewShellViewModel(() => { });
            var project = new FakePreviewTranslationProject(new[]
            {
                new PreviewTranslationEntry("1", "MCM", "GREETING", "Hello Dragonborn", "", 100),
                new PreviewTranslationEntry("2", "MCM", "FAREWELL", "Goodbye", "Auf Wiedersehen", 100),
                new PreviewTranslationEntry("3", "XML", "NOTICE", "Read this", "", 100)
            });
            var viewModel = new PreviewTranslationWorkspaceViewModel(
                () => "fixture",
                () => { },
                shell,
                path => project);

            viewModel.OpenProjectAsync("fixture").GetAwaiter().GetResult();

            AssertEqual(3, viewModel.Entries.Count,
                "Opening a project must expose every normalized entry.");
            AssertEqual("fixture.xml", shell.ProjectIdentity,
                "The shell must receive only the safe project display name.");
            AssertEqual(false, viewModel.SaveCommand.CanExecute(null),
                "An unchanged project must not offer a redundant save.");

            viewModel.SearchText = "dragonborn";
            AssertEqual(1, viewModel.Entries.Count,
                "Search must filter source text without changing project data.");

            viewModel.SearchText = string.Empty;
            viewModel.SelectedTypeFilter = "MCM";
            AssertEqual(2, viewModel.Entries.Count,
                "The type filter must retain matching normalized entries.");

            viewModel.SelectedEntry.TargetText = "Willkommen";
            AssertEqual(true, shell.IsModified,
                "Editing a target must update persistent shell unsaved state.");
            AssertEqual(true, viewModel.SaveCommand.CanExecute(null),
                "A staged target edit must enable persistence.");
            viewModel.Dispose();
        }

        private static void HandlesPreviewTranslationFailures()
        {
            var shell = new PreviewShellViewModel(() => { });
            var project = new FakePreviewTranslationProject(new[]
            {
                new PreviewTranslationEntry("1", "MCM", "ONE", "First", "", 100),
                new PreviewTranslationEntry("2", "MCM", "TWO", "Second", "", 100)
            })
            {
                FailedKey = "2"
            };
            var viewModel = new PreviewTranslationWorkspaceViewModel(
                () => "fixture",
                () => { },
                shell,
                path => project);
            viewModel.OpenProjectAsync("fixture").GetAwaiter().GetResult();

            viewModel.TranslateEntriesAsync(viewModel.Entries.ToList()).GetAwaiter().GetResult();

            AssertEqual("Translated First", project.Entries[0].TargetText,
                "A successful provider result must remain staged.");
            AssertEqual(string.Empty, project.Entries[1].TargetText,
                "A failed entry must preserve its previous target.");
            AssertEqual(PreviewShellNotificationSeverity.Warning, shell.NotificationSeverity,
                "Partial provider failure must use a warning rather than discard successful edits.");
            AssertEqual("Translation completed with 1 failed entries.", shell.NotificationMessage,
                "Partial failure must expose a user-safe bounded summary.");
            viewModel.Dispose();
        }

        private static void CancelsPreviewTranslationWork()
        {
            var shell = new PreviewShellViewModel(() => { });
            var project = new FakePreviewTranslationProject(new[]
            {
                new PreviewTranslationEntry("1", "MCM", "ONE", "First", "", 100)
            })
            {
                BlockUntilCancelled = true
            };
            var viewModel = new PreviewTranslationWorkspaceViewModel(
                () => "fixture",
                () => { },
                shell,
                path => project);
            viewModel.OpenProjectAsync("fixture").GetAwaiter().GetResult();

            System.Threading.Tasks.Task operation = viewModel.TranslateEntriesAsync(viewModel.Entries.ToList());
            AssertEqual(true, System.Threading.SpinWait.SpinUntil(() => project.TranslateStarted, 2000),
                "The synthetic provider must start before cancellation is requested.");
            viewModel.CancelOperationCommand.Execute(null);
            operation.GetAwaiter().GetResult();

            AssertEqual(false, viewModel.IsBusy,
                "Cooperative cancellation must return the workspace to its idle state.");
            AssertEqual(string.Empty, project.Entries[0].TargetText,
                "Cancellation must not replace an entry with a partial provider result.");
            AssertEqual("Ready", shell.StatusText,
                "Cooperative cancellation must restore persistent shell status.");
            viewModel.Dispose();
        }

        private static void FiltersLargePreviewTranslationProject()
        {
            var entries = Enumerable.Range(0, 25000)
                .Select(index => new PreviewTranslationEntry(
                    index.ToString(CultureInfo.InvariantCulture),
                    index % 2 == 0 ? "MCM" : "XML",
                    "RECORD_" + index.ToString(CultureInfo.InvariantCulture),
                    "Synthetic source " + index.ToString(CultureInfo.InvariantCulture),
                    index % 3 == 0 ? "Synthetic target" : string.Empty,
                    100))
                .ToList();
            var shell = new PreviewShellViewModel(() => { });
            var project = new FakePreviewTranslationProject(entries);
            var viewModel = new PreviewTranslationWorkspaceViewModel(
                () => "fixture",
                () => { },
                shell,
                path => project);

            viewModel.OpenProjectAsync("fixture").GetAwaiter().GetResult();
            AssertEqual(25000, viewModel.Entries.Count,
                "A large project must load as one complete virtualized list snapshot.");

            viewModel.SearchText = "source 24999";
            AssertEqual(1, viewModel.Entries.Count,
                "Search must isolate one record in a large synthetic project.");
            AssertEqual("24999", viewModel.Entries[0].Key,
                "Large-project filtering must retain stable record identity.");

            viewModel.SearchText = string.Empty;
            viewModel.SelectedTypeFilter = "MCM";
            AssertEqual(12500, viewModel.Entries.Count,
                "Large-project type filtering must retain every matching record.");
            viewModel.Dispose();
        }

        private static void AnalyzesMixedQualityFindings()
        {
            var entries = new[]
            {
                new PreviewTranslationEntry("1", "MCM", "GREETING", "Hello {0}", "Hallo", 100),
                new PreviewTranslationEntry("2", "PEX", "IDENTIFIER", "MENU_FILE", "Menü", 100),
                new PreviewTranslationEntry("3", "XML", "DUPLICATE_A", "Same source", "First", 100),
                new PreviewTranslationEntry("4", "XML", "DUPLICATE_B", "Same source", "Second", 100),
                new PreviewTranslationEntry("5", "ESP", "EMPTY", "Needs translation", "", 25)
            };

            IReadOnlyList<PreviewQualityFinding> findings = new PreviewQualityAnalyzer().Analyze(entries);

            AssertEqual(true, findings.Any(finding => finding.RuleId == "placeholder-mismatch" && finding.IsBlocking),
                "A missing source placeholder must block export.");
            AssertEqual(true, findings.Any(finding => finding.RuleId == "technical-string" &&
                finding.Severity == PreviewFindingSeverity.Warning),
                "Identifier-like text must be exposed as an acknowledgeable warning.");
            AssertEqual(2, findings.Count(finding => finding.RuleId == "duplicate-inconsistent"),
                "Every affected duplicate must identify its exact entry.");
            AssertEqual(true, findings.Any(finding => finding.RuleId == "untranslated" && finding.Entry.Key == "5"),
                "An untranslated entry must remain an explicit blocking finding.");
            AssertEqual(true, findings.Any(finding => finding.RuleId == "low-confidence" && finding.Source == "ESP"),
                "Format confidence findings must retain their diagnostic source.");
        }

        private static void TracksReviewDecisionsAndBulkUndo()
        {
            string stateDirectory = Path.Combine(Path.GetTempPath(), "PhoenixReviewTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(stateDirectory);
            try
            {
                var entries = new[]
                {
                    new PreviewTranslationEntry("1", "MCM", "ONE", "First", "Erste", 100),
                    new PreviewTranslationEntry("2", "MCM", "TWO", "Second", "Zweite", 100)
                };
                var shell = new PreviewShellViewModel(() => { });
                var project = new FakePreviewTranslationProject(entries, Path.Combine(stateDirectory, "project.xml"));
                var workspace = new PreviewTranslationWorkspaceViewModel(() => project.Path, () => { }, shell, path => project);
                workspace.OpenProjectAsync(project.Path).GetAwaiter().GetResult();
                var cancelled = new PreviewReviewQualityViewModel(shell, workspace, new PreviewQualityAnalyzer(),
                    new PreviewReviewStateStore(stateDirectory), count => false, () => { });

                cancelled.ApproveScopeCommand.Execute(null);
                AssertEqual(PreviewReviewState.Unreviewed, entries[0].ReviewState,
                    "Cancelling bulk approval must preserve every review decision.");
                cancelled.Dispose();

                var review = new PreviewReviewQualityViewModel(shell, workspace, new PreviewQualityAnalyzer(),
                    new PreviewReviewStateStore(stateDirectory), count => count == 2, () => { });
                review.ApproveScopeCommand.Execute(null);
                AssertEqual(PreviewReviewState.Approved, entries[0].ReviewState,
                    "Confirmed bulk approval must update the visible eligible scope.");
                AssertEqual(PreviewReviewState.Approved, entries[1].ReviewState,
                    "Confirmed bulk approval must update every visible eligible entry.");

                review.UndoBulkCommand.Execute(null);
                AssertEqual(PreviewReviewState.Unreviewed, entries[0].ReviewState,
                    "Undo must restore the review state captured before bulk approval.");
                AssertEqual(PreviewReviewState.Unreviewed, entries[1].ReviewState,
                    "Undo must restore the complete bulk scope.");
                review.Dispose();
                workspace.Dispose();
            }
            finally
            {
                Directory.Delete(stateDirectory, true);
            }
        }

        private static void PersistsPrivateReviewMetadata()
        {
            string stateDirectory = Path.Combine(Path.GetTempPath(), "PhoenixReviewTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(stateDirectory);
            try
            {
                string projectPath = Path.Combine(stateDirectory, "private-project.xml");
                const string privateTarget = "Private translated content";
                var store = new PreviewReviewStateStore(stateDirectory);
                var entry = new PreviewTranslationEntry("entry-1", "XML", "RECORD", "Source", privateTarget, 100);
                entry.SetReviewState(PreviewReviewState.Approved);
                store.Save(projectPath, new[] { entry }, new[] { "technical-string:entry-1" });

                string persistedText = File.ReadAllText(Directory.GetFiles(stateDirectory, "*.xml").Single());
                AssertEqual(false, persistedText.Contains(projectPath),
                    "Review metadata must not contain an absolute project path.");
                AssertEqual(false, persistedText.Contains(privateTarget),
                    "Review metadata must not contain private translated content.");

                PreviewReviewStateSnapshot snapshot = store.Load(projectPath);
                AssertEqual(PreviewReviewState.Approved, snapshot.Decisions["entry-1"].State,
                    "A matching project and target fingerprint must restore its review decision.");
                AssertEqual(true, snapshot.AcknowledgedFindingIds.Contains("technical-string:entry-1"),
                    "Acknowledged warning identity must persist without warning content.");

                entry.TargetText = "Changed target";
                AssertEqual(PreviewReviewState.Unreviewed, entry.ReviewState,
                    "Editing approved content must invalidate its human review decision.");
            }
            finally
            {
                Directory.Delete(stateDirectory, true);
            }
        }

        private static void NavigatesFromFindingToTranslationEntry()
        {
            string stateDirectory = Path.Combine(Path.GetTempPath(), "PhoenixReviewTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(stateDirectory);
            try
            {
                var entries = new[]
                {
                    new PreviewTranslationEntry("1", "PEX", "MENU_FILE", "MENU_FILE", "Menü", 100),
                    new PreviewTranslationEntry("2", "XML", "OTHER", "Other", "Andere", 100)
                };
                var shell = new PreviewShellViewModel(() => { });
                var project = new FakePreviewTranslationProject(entries, Path.Combine(stateDirectory, "project.xml"));
                var workspace = new PreviewTranslationWorkspaceViewModel(() => project.Path, () => { }, shell, path => project);
                workspace.OpenProjectAsync(project.Path).GetAwaiter().GetResult();
                var review = new PreviewReviewQualityViewModel(shell, workspace, new PreviewQualityAnalyzer(),
                    new PreviewReviewStateStore(stateDirectory), count => true, () => { });
                shell.CurrentDestination = PreviewShellDestination.Quality;
                review.SelectedFinding = review.Findings.Single(finding => finding.RuleId == "technical-string");

                review.AcknowledgeFindingCommand.Execute(null);
                AssertEqual("Ready with acknowledged warnings", review.ExportReadinessText,
                    "Acknowledged non-blocking warnings must be distinguished from open warnings.");
                review.GoToEntryCommand.Execute(null);
                AssertEqual(PreviewShellDestination.Translate, shell.CurrentDestination,
                    "Finding navigation must return to the translation workspace.");
                AssertEqual(entries[0], workspace.SelectedEntry,
                    "Finding navigation must reveal the exact affected entry.");
                review.Dispose();
                workspace.Dispose();
            }
            finally
            {
                Directory.Delete(stateDirectory, true);
            }
        }

        private static void ClassifiesProjectRevisionChanges()
        {
            var current = new[]
            {
                new PreviewTranslationEntry("added", "XML", "ADDED", "New", string.Empty, 100),
                new PreviewTranslationEntry("reusable", "XML", "REUSABLE", "Same", string.Empty, 100),
                new PreviewTranslationEntry("changed", "XML", "CHANGED", "New source", string.Empty, 100),
                new PreviewTranslationEntry("unchanged", "XML", "UNCHANGED", "Stable", "Stabil", 100)
            };
            var previous = new[]
            {
                new PreviewTranslationEntry("unchanged", "XML", "UNCHANGED", "Stable", "Stabil", 100),
                new PreviewTranslationEntry("removed", "XML", "REMOVED", "Old", "Alt", 100),
                new PreviewTranslationEntry("changed", "XML", "CHANGED", "Old source", "Alte Quelle", 100),
                new PreviewTranslationEntry("reusable", "XML", "REUSABLE", "Same", "Gleich", 100)
            };

            IReadOnlyList<PreviewProjectComparisonItem> result =
                new PreviewProjectComparisonService().Compare(current, previous);

            AssertEqual(PreviewRevisionComparisonState.Added,
                result.Single(item => item.Key == "added").State,
                "A current-only stable identity must be classified as added.");
            AssertEqual(PreviewRevisionComparisonState.Removed,
                result.Single(item => item.Key == "removed").State,
                "A previous-only stable identity must be classified as removed.");
            AssertEqual(PreviewRevisionComparisonState.Reusable,
                result.Single(item => item.Key == "reusable").State,
                "An untranslated equal source must expose the prior target as reusable.");
            AssertEqual(PreviewRevisionComparisonState.Changed,
                result.Single(item => item.Key == "changed").State,
                "A changed source without a current target must require fresh translation.");
            AssertEqual(PreviewRevisionComparisonState.Unchanged,
                result.Single(item => item.Key == "unchanged").State,
                "Equal source and target content must remain unchanged regardless of source order.");

            PreviewProjectComparisonItem ambiguous = new PreviewProjectComparisonService().Compare(
                new[]
                {
                    new PreviewTranslationEntry("duplicate", "XML", "ONE", "First", string.Empty, 100),
                    new PreviewTranslationEntry("duplicate", "XML", "TWO", "Second", string.Empty, 100)
                },
                new[] { new PreviewTranslationEntry("duplicate", "XML", "OLD", "First", "Erste", 100) })
                .Single();
            AssertEqual(PreviewRevisionComparisonState.Conflict, ambiguous.State,
                "Ambiguous stable identities must be exposed as conflicts instead of being silently discarded.");
        }

        private static void PreservesReviewedTargetsAsConflicts()
        {
            var current = new PreviewTranslationEntry("entry", "PEX", "ENTRY", "Source", "Current", 100);
            current.SetReviewState(PreviewReviewState.Approved);
            var previous = new PreviewTranslationEntry("entry", "PEX", "ENTRY", "Source", "Previous", 100);

            PreviewProjectComparisonItem result = new PreviewProjectComparisonService()
                .Compare(new[] { current }, new[] { previous }).Single();

            AssertEqual(PreviewRevisionComparisonState.Conflict, result.State,
                "Different populated targets must never be silently reusable.");
            AssertEqual(true, result.CanReuse,
                "A conflicting prior target may remain available for an explicit confirmed decision.");
            AssertEqual("Current", current.TargetText,
                "Classification must not alter a reviewed current target.");

            current.ApplyReusedTarget(previous.TargetText);
            AssertEqual(PreviewReviewState.Unreviewed, current.ReviewState,
                "Explicit target reuse must invalidate the previous review decision.");
            AssertEqual("Previous project revision", current.Provenance,
                "Explicit target reuse must expose revision provenance.");
        }

        private static void PersistsPrivateRevisionHistory()
        {
            string stateDirectory = Path.Combine(Path.GetTempPath(), "PhoenixHistoryTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(stateDirectory);
            try
            {
                string projectPath = Path.Combine(stateDirectory, "private-project.xml");
                const string privateSource = "Private source content";
                const string privateTarget = "Private target content";
                var entry = new PreviewTranslationEntry("entry", "XML", "RECORD", privateSource, privateTarget, 100);
                var store = new PreviewRevisionHistoryStore(stateDirectory);
                store.Append(projectPath, PreviewRevisionHistoryStore.Create("Reused", entry, DateTime.UtcNow));

                string persistedText = File.ReadAllText(Directory.GetFiles(stateDirectory, "*.xml").Single());
                AssertEqual(false, persistedText.Contains(projectPath),
                    "Revision history must not contain an absolute project path.");
                AssertEqual(false, persistedText.Contains(privateSource),
                    "Revision history must not contain private source content.");
                AssertEqual(false, persistedText.Contains(privateTarget),
                    "Revision history must not contain private target content.");
                AssertEqual(1, store.Load(projectPath).Count,
                    "A valid privacy-preserving history event must round-trip.");
            }
            finally
            {
                Directory.Delete(stateDirectory, true);
            }
        }

        private static void AppliesAndUndoesExplicitRevisionReuse()
        {
            string stateDirectory = Path.Combine(Path.GetTempPath(), "PhoenixUpdateTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(stateDirectory);
            try
            {
                string currentPath = Path.Combine(stateDirectory, "current.xml");
                string previousPath = Path.Combine(stateDirectory, "previous.xml");
                var reusable = new PreviewTranslationEntry("reusable", "XML", "REUSABLE", "Same", string.Empty, 100);
                var conflict = new PreviewTranslationEntry("conflict", "XML", "CONFLICT", "Stable", "Current", 100);
                conflict.SetReviewState(PreviewReviewState.Approved);
                var currentProject = new FakePreviewTranslationProject(new[] { reusable, conflict }, currentPath);
                var previousProject = new FakePreviewTranslationProject(new[]
                {
                    new PreviewTranslationEntry("conflict", "XML", "CONFLICT", "Stable", "Previous", 100),
                    new PreviewTranslationEntry("reusable", "XML", "REUSABLE", "Same", "Reusable target", 100)
                }, previousPath);
                var shell = new PreviewShellViewModel(() => { });
                var workspace = new PreviewTranslationWorkspaceViewModel(
                    () => currentPath, () => { }, shell, path => currentProject);
                workspace.OpenProjectAsync(currentPath).GetAwaiter().GetResult();
                var update = new PreviewHistoryUpdateViewModel(
                    shell,
                    workspace,
                    new PreviewProjectComparisonService(),
                    new PreviewRevisionHistoryStore(stateDirectory),
                    () => previousPath,
                    path => previousProject,
                    count => false,
                    count => true,
                    () => { });

                update.CompareRevisionCommand.Execute(null);
                DateTime timeout = DateTime.UtcNow.AddSeconds(5);
                while (!update.HasComparison && DateTime.UtcNow < timeout)
                {
                    System.Threading.Thread.Sleep(10);
                }

                AssertEqual(true, update.HasComparison,
                    "A compatible selected revision must produce comparison results.");
                update.SelectedComparisonItem = update.ComparisonItems.Single(item => item.Key == "conflict");
                update.ReuseSelectedCommand.Execute(null);
                AssertEqual("Current", conflict.TargetText,
                    "Cancelling conflict confirmation must preserve the reviewed current target.");
                AssertEqual(PreviewReviewState.Approved, conflict.ReviewState,
                    "Cancelling conflict confirmation must preserve its review decision.");

                update.SelectedComparisonItem = update.ComparisonItems.Single(item => item.Key == "reusable");
                update.ReuseSelectedCommand.Execute(null);
                AssertEqual("Reusable target", reusable.TargetText,
                    "Explicit safe reuse must stage the selected prior target.");
                AssertEqual(PreviewReviewState.Unreviewed, reusable.ReviewState,
                    "Reused content must enter review as unreviewed.");

                update.UndoCommand.Execute(null);
                AssertEqual(string.Empty, reusable.TargetText,
                    "Undo must restore the target captured before explicit reuse.");
                AssertEqual(true, update.HistoryEntries.Any(entry => entry.ActionId == "Reused"),
                    "Explicit reuse must create a project history event.");
                AssertEqual(true, update.HistoryEntries.Any(entry => entry.ActionId == "Undone"),
                    "Undo must create a project history event.");
                update.Dispose();
                workspace.Dispose();
            }
            finally
            {
                Directory.Delete(stateDirectory, true);
            }
        }

        private static void StagesAndCancelsUnifiedSettings()
        {
            var store = new RecordingPreviewSettingsStore();
            var shell = new PreviewShellViewModel(() => { });
            var settings = new PreviewSettingsViewModel(shell, store, () => true, () => true, () => { });

            settings.Settings.ContextLimitText = "500";
            AssertEqual(true, settings.IsModified,
                "Editing one centralized setting must stage a modified state.");
            AssertEqual(0, store.SaveCalls,
                "Editing staged settings must not persist through legacy immediate-save handlers.");

            settings.CancelCommand.Execute(null);
            AssertEqual("200", settings.Settings.ContextLimitText,
                "Cancel must restore the complete loaded snapshot.");
            AssertEqual(false, settings.IsModified,
                "Cancel must restore the clean state.");
            AssertEqual(0, store.SaveCalls,
                "Cancel must not persist any staged value.");
            settings.Dispose();
        }

        private static void ValidatesAndAppliesUnifiedSettings()
        {
            var store = new RecordingPreviewSettingsStore();
            var shell = new PreviewShellViewModel(() => { });
            var settings = new PreviewSettingsViewModel(shell, store, () => true, () => true, () => { });

            settings.Settings.MaxThreadCountText = "0";
            AssertEqual(true, settings.HasValidationErrors,
                "An invalid worker limit must be explained before persistence.");
            settings.ApplyCommand.Execute(null);
            AssertEqual(0, store.SaveCalls,
                "Invalid settings must never reach persistence.");

            settings.Settings.MaxThreadCountText = "4";
            settings.Settings.EnableGlobalSearch = true;
            settings.ApplyCommand.Execute(null);
            AssertEqual(1, store.SaveCalls,
                "One explicit Apply action must persist the complete valid snapshot exactly once.");
            AssertEqual(true, store.Current.EnableGlobalSearch,
                "Apply must persist staged values through the central store boundary.");
            AssertEqual(false, settings.IsModified,
                "Successful Apply must establish a new clean baseline.");
            settings.Dispose();
        }

        private static void SearchesSettingsByLegacyTerminology()
        {
            var store = new RecordingPreviewSettingsStore();
            var shell = new PreviewShellViewModel(() => { });
            var settings = new PreviewSettingsViewModel(shell, store, () => true, () => true, () => { });

            settings.SearchText = "node";
            AssertEqual(1, settings.VisibleCategories.Count,
                "Legacy provider-node terminology must resolve to one central category.");
            AssertEqual(PreviewSettingsCategory.Providers, settings.VisibleCategories[0].Value,
                "Provider nodes must resolve to Providers instead of another settings window.");

            settings.SearchText = "dictionary";
            AssertEqual(PreviewSettingsCategory.HistoryAndData, settings.VisibleCategories.Single().Value,
                "Legacy dictionary terminology must route to History and data.");
            settings.Dispose();
        }

        private static void ProtectsSettingsSecrets()
        {
            var store = new RecordingPreviewSettingsStore();
            var shell = new PreviewShellViewModel(() => { });
            var settings = new PreviewSettingsViewModel(shell, store, () => true, () => true, () => { });
            const string secret = "private-provider-secret";

            settings.StageProviderCredential(secret);
            AssertEqual(false, settings.Settings.GetType().GetProperties()
                    .Any(property => string.Equals(property.GetValue(settings.Settings) as string, secret, StringComparison.Ordinal)),
                "A staged credential must not be exposed through bindable settings properties.");
            AssertEqual(false, settings.ValidationMessages.Any(message => message.Contains(secret)),
                "Validation output must never contain a staged credential.");

            settings.Settings.EnableGlobalSearch = true;
            settings.ApplyCommand.Execute(null);
            AssertEqual(secret, store.LastProviderCredential,
                "The write-only store boundary must receive the explicitly staged credential.");
            AssertEqual(false, settings.Settings.GetType().GetProperties()
                    .Any(property => string.Equals(property.GetValue(settings.Settings) as string, secret, StringComparison.Ordinal)),
                "Reloaded settings must expose only credential presence, never its value.");
            settings.Dispose();
        }

        private static void TestsProviderWithoutSavingSettings()
        {
            var store = new RecordingPreviewSettingsStore();
            var shell = new PreviewShellViewModel(() => { });
            var settings = new PreviewSettingsViewModel(shell, store, () => true, () => true, () => { });
            const string secret = "staged-test-secret";

            settings.StageProviderCredential(secret);
            settings.TestProviderCommand.Execute(null);

            AssertEqual(1, store.TestCalls,
                "An explicit provider test must cross the connectivity boundary once.");
            AssertEqual(0, store.SaveCalls,
                "Testing staged provider settings must not persist them.");
            AssertEqual(secret, store.LastTestCredential,
                "The connectivity boundary must receive a staged credential without exposing it to binding.");
            AssertEqual(PreviewMessageCatalog.Get("Settings_Providers_Test_Succeeded"), settings.ProviderTestStatusText,
                "A successful provider test must produce a sanitized status.");
            settings.Dispose();
        }

        private static void ClearsStagedCredentialWhenProviderChanges()
        {
            var store = new RecordingPreviewSettingsStore();
            var shell = new PreviewShellViewModel(() => { });
            var settings = new PreviewSettingsViewModel(shell, store, () => true, () => true, () => { });

            settings.StageProviderCredential("credential-for-first-provider");
            settings.SelectedProvider = settings.ProviderOptions[1];
            settings.TestProviderCommand.Execute(null);

            AssertEqual(0, store.TestCalls,
                "A staged credential must not follow the user to another provider configuration.");
            AssertEqual(true, settings.ProviderTestStatusText.Contains(
                    PreviewMessageCatalog.Get("Settings_Validation_CredentialRequired")),
                "The newly selected provider must require its own credential.");
            settings.Dispose();
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

        private sealed class FakePreviewTranslationProject : IPreviewTranslationProject
        {
            internal FakePreviewTranslationProject(IReadOnlyList<PreviewTranslationEntry> entries, string path = "fixture.xml")
            {
                Entries = entries;
                Path = path;
            }

            public string Path { get; private set; }

            public string DisplayName => "fixture.xml";

            public IReadOnlyList<PreviewTranslationEntry> Entries { get; private set; }

            internal string FailedKey { get; set; }

            internal bool BlockUntilCancelled { get; set; }

            internal bool TranslateStarted { get; private set; }

            public string Translate(PreviewTranslationEntry entry, System.Threading.CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                TranslateStarted = true;
                if (BlockUntilCancelled)
                {
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        System.Threading.Thread.Sleep(5);
                    }
                }

                if (string.Equals(entry.Key, FailedKey, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("Synthetic provider failure.");
                }

                return "Translated " + entry.SourceText;
            }

            public void Save()
            {
            }

            public void Dispose()
            {
            }
        }

        private sealed class RecordingRecentProjectStore : IPreviewRecentProjectStore
        {
            private IReadOnlyList<PreviewRecentProject> _projects = new PreviewRecentProject[0];

            internal int SaveCalls { get; private set; }

            public IReadOnlyList<PreviewRecentProject> Load()
            {
                return _projects;
            }

            public void Save(IReadOnlyList<PreviewRecentProject> projects)
            {
                SaveCalls++;
                _projects = projects.ToArray();
            }
        }

        private sealed class RecordingPreviewSettingsStore : IPreviewSettingsStore
        {
            internal RecordingPreviewSettingsStore()
            {
                Current = new PreviewSettingsSnapshot
                {
                    ProviderKey = 1,
                    ProviderModel = "test-model",
                    ProviderEnabled = false,
                    HasStoredCredential = true,
                    LocalPortText = "1234",
                    SourceLanguage = "English",
                    TargetLanguage = "English",
                    EnableLanguageDetection = true,
                    EnableContext = true,
                    ContextLimitText = "200",
                    PlaceholderPattern = "<(.*?)>,",
                    GenerateCSharp = true,
                    UiLanguage = "English",
                    Density = "Compact",
                    MaxThreadCountText = "2",
                    ThrottleRatioText = "0.7",
                    ThrottleDelayText = "200"
                };
            }

            internal PreviewSettingsSnapshot Current { get; private set; }
            internal int SaveCalls { get; private set; }
            internal int TestCalls { get; private set; }
            internal string LastProviderCredential { get; private set; }
            internal string LastTestCredential { get; private set; }

            public IReadOnlyList<PreviewProviderOption> GetProviders()
            {
                return new[]
                {
                    new PreviewProviderOption(1, "Test provider", false, true, true, new[] { "test-model" }),
                    new PreviewProviderOption(2, "Second provider", false, false, true, new[] { "other-model" })
                };
            }

            public IReadOnlyList<string> GetLanguages()
            {
                return new[] { "English", "German" };
            }

            public PreviewSettingsSnapshot Load()
            {
                return Current.Clone();
            }

            public void Save(PreviewSettingsSnapshot settings, string providerCredential, string proxyPassword)
            {
                SaveCalls++;
                Current = settings.Clone();
                if (!string.IsNullOrWhiteSpace(providerCredential))
                {
                    LastProviderCredential = providerCredential;
                    Current.HasStoredCredential = true;
                }

                if (!string.IsNullOrWhiteSpace(proxyPassword))
                {
                    Current.HasStoredProxyPassword = true;
                }
            }

            public Task<PreviewProviderTestResult> TestProviderAsync(
                PreviewSettingsSnapshot settings,
                string providerCredential,
                string proxyPassword,
                CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();
                TestCalls++;
                LastTestCredential = providerCredential;
                return Task.FromResult(new PreviewProviderTestResult(PreviewProviderTestStatus.Succeeded));
            }
        }
    }
}
