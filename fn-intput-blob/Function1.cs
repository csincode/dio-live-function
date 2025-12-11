using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace fn_input_blob
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function(nameof(Function1))]
        [BlobOutput("output/{name}-output.txt", Connection = "live-dio")]
        public async Task<string> RunAsync(
            [BlobTrigger("samples-workitems/{name}", Connection = "live-dio")] Stream blobStream,
            string name)
        {
            _logger.LogInformation("Blob Triggered: {BlobName}", name);

            using var reader = new StreamReader(blobStream, Encoding.UTF8);
            string content = await reader.ReadToEndAsync();

            _logger.LogInformation("Blob Content Length = {Length}", content.Length);

            // Processamento (exemplo)
            string processed = content.ToUpperInvariant();

            return processed;
        }
    }
}
