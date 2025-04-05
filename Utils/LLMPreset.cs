namespace Echoglossian.Utils
{
    public class LLMPreset(string name, string llmApiEndpoint, string llmModel, float temperature)
    {
        public string Name { get; set; } = name;
        public string LLM_API_Endpoint { get; set; } = llmApiEndpoint;
        public string Model { get; set; } = llmModel;
        public float Temperature { get; set; } = temperature;
    }
}
