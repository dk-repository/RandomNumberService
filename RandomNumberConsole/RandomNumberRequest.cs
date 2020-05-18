using System.Text.Json.Serialization;

namespace RandomNumberService.Generator
{
    public class RandomNumberRequest 
    {
        [JsonPropertyName("number")]
        public int number { get; set; }
    }
}
