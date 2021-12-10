using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrintIt.Core;

namespace PrintIt.WebHost.Controllerrs
{
    [ApiController]
    [Route("api/print")]
    public class PrintController : ControllerBase
    {
        private readonly IPdfPrintService _pdfPrintService;

        public PrintController(IPdfPrintService pdfPrintService)
        {
            _pdfPrintService = pdfPrintService;
        }

        /// <summary>
        /// Prints a file to a network printer.
        /// Supported file types include (.pdf, .docx)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> PrintFile([FromForm] PrintRequest request)
        {
            await using Stream pdfStream = request.File.OpenReadStream();
            _pdfPrintService.Print(pdfStream, request.File.ContentType, request.PrinterPath, request.PageRange, request.File.FileName);
            return Ok();
        }
    }

    public class PrintRequest
    {
        [Required]
        public IFormFile File { get; set; }

        [Required]
        public string PrinterPath { get; set; }

        public string PageRange { get; set; }
    }

}
