using GemBardPT.Controllers;

public class GeminiApiRequest
{
    public GeminiContent[] Contents { get; set; }
    public GenerationConfig GenerationConfig { get; set; }
}