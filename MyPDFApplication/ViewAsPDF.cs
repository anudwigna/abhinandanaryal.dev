using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MyPDFApplication
{
    public class PDFAsView : IActionResult
    {
        private readonly string _url;

        public PDFAsView(string url)
        {
            _url = url;
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            try
            {
                string tempFileName = Path.GetTempFileName();

                var exited = false;

                using var process = new Process();
                process.StartInfo.FileName = @"wkhtmltopdf.exe";
                process.StartInfo.Arguments = $"-O Portrait --dpi 600 -L 2mm -R 2mm -T 1mm -B 1mm --page-size A4 {_url} {tempFileName}";
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                process.OutputDataReceived += (sender, data) => Console.WriteLine(data.Data);
                process.ErrorDataReceived += (sender, data) => Console.WriteLine(data.Data);
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                exited = process.WaitForExit(1000000 * 15);
                string filePath = tempFileName;

                if (exited)
                {
                    using var fileStream = new FileStream(filePath, FileMode.Open);
                    fileStream.CopyTo(context.HttpContext.Response.Body);
                    var fileStreamResult = new FileStreamResult(fileStream, "application/pdf");
                    fileStreamResult.ExecuteResult(context);
                }

                return Task.CompletedTask;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}
