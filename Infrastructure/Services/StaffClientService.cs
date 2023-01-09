using AuthServer.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.Infrastructure.Services
{
    public class StaffClientService : IStaffClientService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public StaffClientService(HttpClient httpClient, IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            _httpClient.BaseAddress = new Uri(_configuration.GetValue<string>("AppSettings:weeditApiUrl"));
        }

        public async Task Create(CreateStaffVM createStaffVM, string token)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var data = new StringContent(JsonConvert.SerializeObject(createStaffVM), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}Staffs", data);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Create Staff error");
                }

                var result = await response.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException taskCanceledException)
            {
                throw taskCanceledException;
            }
        }
    }
}
