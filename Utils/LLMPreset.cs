namespace Echoglossian.Utils
{
    internal class LLMPreset(string name, string llmApiEndpoint, string llmModel, string llmApiKey, float temperature)
    {
        public string Name { get; set; } = name;
        public string LLM_API_Endpoint { get; set; } = llmApiEndpoint;
        public string LLM_Model { get; set; } = llmModel;
        public string LLM_API_Key { get; set; } = llmApiKey;
        private float Temperature { get; set; } = temperature;
    }
}
