using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using Microsoft.Extensions.Logging;
using PrintIt.Core.DocConverters;
using PrintIt.Core.Internal;
using PrintIt.Core.Pdfium;

namespace PrintIt.Core {

    [ExcludeFromCodeCoverage]
    internal sealed class PrintService : IPdfPrintService {
        private readonly IConvertImgToPdf _imgToPdf;
        private readonly IDocConverterService _docConverter;
        private readonly ILogger<PrintService> _logger;

        public PrintService(IDocConverterService docConverter, IConvertImgToPdf imgToPdf, ILogger<PrintService> logger) {
            _docConverter = docConverter ?? throw new ArgumentNullException(nameof(docConverter));
            _imgToPdf = imgToPdf ?? throw new ArgumentNullException(nameof(imgToPdf));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Print(Stream stream, string? mimeType, string printerName, string pageRange = null, string printJobName = null, bool duplex = false) {

            if (stream == null) {
                throw new ArgumentNullException(nameof(stream));
            }

            Stream pdf;
            if (mimeType == "application/pdf") {
                pdf = stream;
            } else if (mimeType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document") {
                pdf = _docConverter.ConvertDocument(stream, mimeType);
            } else if (mimeType.StartsWith("image")) {
                pdf = _imgToPdf.ImgToPdf(stream);
            } else {
                throw new Exception($"File type not supported: {mimeType} is not supported for printing");
            }

            PdfDocument document = PdfDocument.Open(pdf);

            _logger.LogInformation($"Printing PDF containing {document.PageCount} page(s) to printer '{printerName}'");

            using var printDocument = new PrintDocument();
            printDocument.PrinterSettings.PrinterName = printerName;

            if (printDocument.PrinterSettings.CanDuplex && duplex) {
                printDocument.PrinterSettings.Duplex = Duplex.Vertical;
            }
            
            printDocument.DocumentName = printJobName ?? string.Empty;
            PrintState state = PrintStateFactory.Create(document, pageRange);
            printDocument.PrintPage += (_, e) => PrintDocumentOnPrintPage(e, state);
            printDocument.Print();
        }

        private void PrintDocumentOnPrintPage(PrintPageEventArgs e, PrintState state) {
            var destinationRect = new RectangleF(
                x: e.Graphics.VisibleClipBounds.X * e.Graphics.DpiX / 100.0f,
                y: e.Graphics.VisibleClipBounds.Y * e.Graphics.DpiY / 100.0f,
                width: e.Graphics.VisibleClipBounds.Width * e.Graphics.DpiX / 100.0f,
                height: e.Graphics.VisibleClipBounds.Height * e.Graphics.DpiY / 100.0f);
            using PdfPage page = state.Document.OpenPage(state.CurrentPageIndex);
            page.RenderTo(e.Graphics, destinationRect);
            e.HasMorePages = state.AdvanceToNextPage();
        }
    }
}
