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
        /// Prints a pdf to a network printer.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("from-pdf")]
        public async Task<IActionResult> PrintFromPdf([FromForm] PrintFromTemplateRequest request)
        {
            await using Stream pdfStream = request.PdfFile.OpenReadStream();
            _pdfPrintService.Print(pdfStream, request.PrinterPath, request.PageRange);
            return Ok();
        }
    }

    public sealed class PrintFromTemplateRequest
    {
        [Required]
        public IFormFile PdfFile { get; set; }

        [Required]
        public string PrinterPath { get; set; }

        public string PrintJobName { get; set; }

        public string PageRange { get; set; }
    }

}
