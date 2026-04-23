using ExamCare.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ExamCare.Services
{
    public class StudentExamApiService
    {
        private readonly HttpClient _httpClient;
        private readonly TokenService _tokenService;
        public StudentExamApiService()
        {
            var baseUrl = Startup.Configuration["ApiSettings:BaseUrl"];

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
            _tokenService = new TokenService();
        }

        public async Task<LoginTokenModel> LoginExamAsync(string examCode, string email, string password)
        {
            var requestBody = new
            {
                examCode,
                email,
                password
            };

            var json = JsonSerializer.Serialize(requestBody);
            Debug.WriteLine("Request JSON: " + json);

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("auth/login-exam", content);
            var responseText = await response.Content.ReadAsStringAsync();
            Debug.WriteLine("Login response: " + responseText);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Login failed: {response.StatusCode} - {responseText}");
            }

            var loginResult = JsonSerializer.Deserialize<LoginTokenModel>(
                responseText,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (loginResult == null || string.IsNullOrWhiteSpace(loginResult.AccessToken))
            {
                throw new Exception("Không đọc được access token từ response.");
            }

            _tokenService.SetToken(loginResult.AccessToken);

            return loginResult;
        }

        public async Task<StudentExamResponse?> StartExamAsync()
        {
            string token = _tokenService.GetToken();
            if (string.IsNullOrWhiteSpace(token))
                throw new Exception("Chưa có token.");

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var studentIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            var examIdClaim = jwt.Claims.FirstOrDefault(c => c.Type == "examId")?.Value;

            if (!int.TryParse(studentIdClaim, out int studentId))
                throw new Exception("Không lấy được studentId từ token.");

            if (!int.TryParse(examIdClaim, out int examId))
                throw new Exception("Không lấy được examId từ token.");

            var requestBody = new
            {
                studentId,
                examId
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, "student-exam/start-exam");
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"StartExam failed: {response.StatusCode} - {responseText}");

            return JsonSerializer.Deserialize<StudentExamResponse>(
                responseText,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        //submit attempt map
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
            _tokenService.ClearToken();

            return response.IsSuccessStatusCode;
        }

    }
}