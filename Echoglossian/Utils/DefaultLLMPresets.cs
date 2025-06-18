using System.Collections.Generic;

namespace Echoglossian.Utils
{
    internal class DefaultLLMPresets
    {
        // Recommended models

        public static readonly List<LLMPreset> Default = new() {
            new LLMPreset(
                "Gemini 2.0 Flash (Google AI Studio)",  // cheap and good model
                "https://generativelanguage.googleapis.com/v1beta/openai/chat/completions",
                "gemini-2.0-flash"),
            new LLMPreset(
                "Gemini 2.5 Flash (Google AI Studio)",  // affordable, slightly better than 2.0
                "https://generativelanguage.googleapis.com/v1beta/openai/chat/completions",
                "gemini-2.5-flash"),
            new LLMPreset(
                "GPT 4.1 (OpenAI)",  // expensive openai model
                "https://api.openai.com/v1/chat/completions",
                "gpt-4.1"),
            new LLMPreset(
                "Claude 4.0 Sonnet (OpenRouter)",  // very expensive top tier model
                "https://openrouter.ai/api/v1/chat/completions",
                "anthropic/claude-sonnet-4"),
            new LLMPreset(
                "DeepSeek V3 (DeepSeek)",  // excellent in Chinese
                "https://api.deepseek.com/chat/completions",
                "deepseek-chat"),

            new LLMPreset(
                "Gemini 2.0 Flash (OpenRouter)",
                "https://openrouter.ai/api/v1/chat/completions",
                "google/gemini-2.0-flash-001"),
            new LLMPreset(
                "GPT 4.1 (OpenRouter)",
                "https://openrouter.ai/api/v1/chat/completions",
                "openai/gpt-4.1"),
            // ... other presets
        };
    }
}
