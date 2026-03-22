# Lexicon AI Translator

**Lexicon AI Translator** is a free, open-source tool for Skyrim mod localization.
It supports multiple mod-related file formats, including PEX, ESM, ESP, and MCM.
The tool focuses on structured string preprocessing, rule-based protection, and customizable translation workflows to reduce errors during batch translation.
With optional integration of local or online translation engines and user-defined dictionaries, Lexicon AI Translator helps translators work more efficiently while maintaining control over the final output.

If you want to give feedback, report issues, or discuss Lex Translator, please feel free to visit any of the following sites:  

- [Nexus Mods (for international users)](https://www.nexusmods.com/skyrimspecialedition/mods/143056)  

You can download it directly from Nexus Mods or build it yourself here. Both versions are kept up to date.

Your support and feedback are greatly appreciated!

---

## 📦 Features

- ✅ Support for `.pex`, `.esm`, `.esp`, and `.mcm` formats  
- 🔁 Batch processing and translation history tracking  
- 🌐 Integration with OpenAI, DeepL, and other translation APIs  
- 🧠 Heuristic filtering to avoid code-related terms being mistranslated  
- 🔧 Designed for extendability and customization

---

### Steps:

1. Clone the repository:  
   [https://github.com/YD525/PhoenixEngine](https://github.com/YD525/PhoenixEngine)

2. Open the solution in Visual Studio and build the project.

3. After building, make sure to **reference the generated DLLs** (e.g., `PhoenixEngine.dll`) in the **Lex Translator** project.  
   You can do this either by adding project references or linking the compiled DLLs directly.

This step is **mandatory** — the LexTranslator project will not build correctly without it.

---

## 🧩 Third-party Components

This project uses the following key open-source libraries/frameworks:

- [AvalonEdit](https://github.com/icsharpcode/AvalonEdit) – WPF text editor component used for code/text display.  

---

### 🙏 Special Thanks

I would like to give special thanks to the developers of 

[Cutleast](https://github.com/Cutleast) It helped me solve some problems with reading PEX,ESP files.

[Noggog](https://github.com/Noggog) for helping me understand how StringsFile reads and writes.

[Cutleast](https://github.com/Cutleast), [SkyHorizon3](https://github.com/SkyHorizon3) for helping me resolve the issue with generating specific JSON fields in the DSD file.

[SSEAT](https://github.com/Cutleast/SSE-Auto-Translator) This is a highly automated program that can automatically download pre-translated content, avoiding repeated translation of a single module. It's ideal for use with SSELex, The functions of both parties may also be integrated in the future.

[Mutagen.Bethesda](https://github.com/Mutagen-Modding/Mutagen) Without this framework, there would be no earliest version of SSELex.

[walkswithwolf](https://www.nexusmods.com/profile/walkswithwolf?gameId=110) Help me understand the structure of Skyrim files.

[Champollion](https://github.com/Orvid/Champollion) This framework was a huge help!

[Kanie17](https://www.nexusmods.com/profile/Kanie17/mods) introduced many meaningful feature improvements.

[50809501](https://www.nexusmods.com/profile/50809501) has been continuously maintaining the Chinese dictionary database for SSELex.

[Neko41](https://www.nexusmods.com/profile/Neko41) has been continuously maintaining the German dictionary database for SSELex.

[撒倫](https://home.gamer.com.tw/profile/index.php?owner=salunt) offered many meaningful suggestions regarding Traditional Chinese.

[zhuabaobao123](https://www.nexusmods.com/profile/zhuabaobao123) Created the Chinese interface for Lex.

Their excellent libraries provide Lex Translator with a stable and solid foundation, allowing us to focus more on developing the translation features.
Acknowledgements: Nexus Mods,9DM,2Game.info,and 泰姆瑞尔MOD组, for their support and encouragement that inspire my creative work.

---
