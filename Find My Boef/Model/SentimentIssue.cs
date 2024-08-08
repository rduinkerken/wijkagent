using Microsoft.ML.Data;

namespace Find_My_Boef.Model
{
    public class SentimentIssue
    {
        [LoadColumn(0)]
        public bool Label { get; set; }
        [LoadColumn(1)]
        public string? Text { get; set; }
    }
}
