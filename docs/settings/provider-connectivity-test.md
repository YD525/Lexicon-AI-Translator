# Provider connectivity test

The Settings Center provides one explicit, cancellable availability test for built-in translation providers.
The test validates staged settings without persisting them and never performs a translation request.

## Privacy and security boundary

- No project name, path, source text, target text, prompt, terminology, or translation-memory content is sent.
- Credentials and proxy passwords remain write-only and are never included in bindings, status messages, logs,
  exception text, or response parsing.
- The response body is not read. Only the HTTP status is classified into a small sanitized result set.
- Requests use normal platform TLS validation. No certificate callback or TLS override is installed.
- Each request has a ten-second timeout and accepts cooperative cancellation.
- Staged proxy settings are honored for cloud providers. Local LM Studio tests bypass proxies.

## Built-in endpoints

| Provider | Read-only request | Authentication |
| --- | --- | --- |
| OpenAI | `GET https://api.openai.com/v1/models` | `Authorization: Bearer` |
| Gemini | `GET https://generativelanguage.googleapis.com/v1beta/models` | `x-goog-api-key` |
| DeepSeek | `GET https://api.deepseek.com/models` | `Authorization: Bearer` |
| DeepL API Pro | `GET https://api.deepl.com/v2/usage` | `Authorization: DeepL-Auth-Key` |
| DeepL API Free | `GET https://api-free.deepl.com/v2/usage` | `Authorization: DeepL-Auth-Key` |
| LM Studio | `GET http://localhost:{port}/v1/models` | None |

The endpoints follow the providers' official model-list or usage documentation:

- [OpenAI model listing](https://developers.openai.com/api/reference/resources/models/methods/list)
- [Gemini model listing](https://ai.google.dev/api/models)
- [DeepSeek model listing](https://api-docs.deepseek.com/api/list-models/)
- [DeepL usage and limits](https://developers.deepl.com/api-reference/usage-and-quota/check-usage-and-limits)
- [LM Studio OpenAI-compatible API](https://lmstudio.ai/docs/developer/openai-compat)

Custom and interactive providers are intentionally not probed from the central Settings Center. Their arbitrary
request definitions require the advanced provider manager, where the user can inspect the endpoint and request
shape before testing.
