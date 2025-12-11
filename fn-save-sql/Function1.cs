using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;
using Microsoft.Extensions.Logging;
using fn_save_sql.Model;

namespace AzureSQL.ToDo
{
    public class PostToDo
    {
        private readonly ILogger<PostToDo> _logger;
        private readonly string _baseUrl;

        public PostToDo(ILogger<PostToDo> logger)
        {
            _logger = logger;
            _baseUrl = Environment.GetEnvironmentVariable("ToDoUri") ?? "https://default-url/";
        }

        [Function(nameof(PostToDo))]
        public async Task<OutputType> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "PostFunction")] HttpRequestData req)
        {
            _logger.LogInformation("Processing POST request for ToDo item...");

            // Read body safely
            string body = await req.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body))
                return Error(req, "Request body is empty.");

            ToDoItem item;

            try
            {
                item = JsonSerializer.Deserialize<ToDoItem>(body);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalid JSON received.");
                return Error(req, "Invalid JSON payload.");
            }

            // Validation
            if (item == null)
                return Error(req, "Invalid or empty object.");

            if (string.IsNullOrEmpty(item.Title))
                return Error(req, "The 'title' field is required.");

            // Business logic
            item.Id = Guid.NewGuid();
            item.Completed = item.Completed ?? false; 
            item.Url = $"{_baseUrl}?id={item.Id}";

            var response = req.CreateResponse(System.Net.HttpStatusCode.Created);
            await response.WriteAsJsonAsync(item);

            return new OutputType
            {
                ToDoItem = item,
                HttpResponse = response
            };
        }

        private OutputType Error(HttpRequestData req, string message)
        {
            var response = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
            response.WriteString(message);

            return new OutputType
            {
                HttpResponse = response,
                ToDoItem = null
            };
        }
    }

    public class OutputType
    {
        [SqlOutput("dbo.ToDo", connectionStringSetting: "SqlConnectionString")]
        public ToDoItem ToDoItem { get; set; }

        public HttpResponseData HttpResponse { get; set; }
    }
}
