using System;
using PhoenixEngine;

namespace PhoenixTranslator.ApplicationLayer
{
    /// <summary>Adapts existing engine and local settings persistence to preset coordination.</summary>
    internal sealed class LegacyTranslationPresetStore : ITranslationPresetStore
    {
        /// <inheritdoc />
        public TranslationPreset Preset
        {
            get => DeFine.GlobalLocalSetting.Preset;
            set => DeFine.GlobalLocalSetting.Preset = value;
        }

        /// <inheritdoc />
        public TranslationPresetSettings ReadSettings()
        {
            return new TranslationPresetSettings(
                Phoenix.Config.ContextLimit,
                Phoenix.Config.BucketLengthLimit,
                Phoenix.Config.PreserveConversationContext,
                Phoenix.Config.ForceContextDeduplication,
                Phoenix.Config.StrictLinkBucketPurity);
        }

        /// <inheritdoc />
        public void ApplySettings(TranslationPresetSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            Phoenix.Config.ContextLimit = settings.ContextLimit;
            Phoenix.Config.BucketLengthLimit = settings.BucketLengthLimit;
            Phoenix.Config.PreserveConversationContext = settings.PreserveConversationContext;
            Phoenix.Config.ForceContextDeduplication = settings.ForceContextDeduplication;
            Phoenix.Config.StrictLinkBucketPurity = settings.StrictLinkBucketPurity;
        }

        /// <inheritdoc />
        public void Save()
        {
            DeFine.GlobalLocalSetting.SaveConfig();
        }
    }
}
