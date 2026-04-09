using ExamCare.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ExamCare.Services
{
    internal class StudentExamApiService
    {
        private readonly HttpClient _httpClient;

        public StudentExamApiService()
        {
            var baseUrl = Startup.Configuration["ApiSettings:BaseUrl"];

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
        }

        public async Task<StudentExamResponse?> StartExamAsync(int studentId, int examId)
        {
            var requestBody = new
            {
                studentId,
                examId
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("student-exam/start-exam", content);
            var responseText = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<StudentExamResponse>(
                responseText,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            Debug.WriteLine("alo: " + responseText);
            return result;
        }

        public async Task<bool> SendAttemptMapAsync(Dictionary<int, AttemptQuestion> attemptMap)
        {
            var attemptList = attemptMap.Values.ToList();

            var json = System.Text.Json.JsonSerializer.Serialize(attemptList);

            Debug.WriteLine("JSON gửi đi:");
            Debug.WriteLine(json);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                "student-exam/submit-attempt",
                content
            );

            var responseBody = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($"StatusCode: {(int)response.StatusCode}");
            Debug.WriteLine($"Response: {responseBody}");

            return response.IsSuccessStatusCode;
        }

    }
}