using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectCompScience.Services
{
    public class SemanticPredictionService
    {
        private readonly Kernel _kernel;

        public SemanticPredictionService()
        {
            var builder = Kernel.CreateBuilder();

            builder.AddGoogleAIGeminiChatCompletion(
                modelId: "gemini-3.5-flash", //gemini-3.5-flash //gemini-3-flash-preview
                apiKey: Environment.GetEnvironmentVariable("API_KEY_gemini")
            );

            _kernel = builder.Build();
        }

        public async Task<List<double>> GetFuturePointsAsync(List<double> currentPoints, int daysToPredict)
        {
            try
            {
                string prompt = @"You are a quantitative stock market algorithm. 
                Based on this recent price history: {{$history}}
                Generate a realistic prediction for the next {{$days}} days.
                RESPOND ONLY WITH A COMMA-SEPARATED LIST OF NUMBERS. NO TEXT. NO EXPLANATIONS.";

                var arguments = new KernelArguments
                {
                    { "history", string.Join(", ", currentPoints.TakeLast(15)) },
                    { "days", daysToPredict }
                };


                var result = await _kernel.InvokePromptAsync(prompt, arguments);
                string aiResponse = result.GetValue<string>();

                return ParseAiResponse(aiResponse);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Semantic Kernel Error: {ex.Message}");

                // Instantly fall back to local math generation so the app doesn't freeze
                return GenerateFallbackPredictions(currentPoints.LastOrDefault(), daysToPredict);
            }
        }

        private List<double> ParseAiResponse(string response)
        {
            var points = new List<double>();
            if (string.IsNullOrWhiteSpace(response)) return points;

            var stringNumbers = response.Split(',');
            foreach (var str in stringNumbers)
            {
                if (double.TryParse(str.Trim(), out double number))
                {
                    points.Add(number);
                }
            }
            return points;
        }

        private List<double> GenerateFallbackPredictions(double lastPrice, int days)
        {
            if (lastPrice <= 0) lastPrice = 150.00;
            var random = new Random();
            var fallbackPoints = new List<double>();

            for (int i = 0; i < days; i++)
            {
                // Swings between -5% and +5% with a tiny upward drift
                double changePercent = 2 * 0.05 * random.NextDouble() - 0.05 + 0.002;
                lastPrice += lastPrice * changePercent;

                // Prevent negative stock prices
                lastPrice = Math.Max(0.01, lastPrice);

                fallbackPoints.Add(Math.Round(lastPrice, 2));
            }
            return fallbackPoints;
        }
    }
}