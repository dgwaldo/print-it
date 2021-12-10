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
        private readonly IDocConverterService _docConverter;
        private readonly ILogger<PrintService> _logger;

        public PrintService(IDocConverterService docConverter, ILogger<PrintService> logger) {
            _docConverter = docConverter ?? throw new ArgumentNullException(nameof(docConverter));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Print(Stream stream, string? mimeType, string printerName, string pageRange = null, string printJobName = null) {

            if (stream == null) {
                throw new ArgumentNullException(nameof(stream));
            }

            Stream pdf = mimeType == "application/pdf" ? stream: _docConverter.ConvertDocument(stream, mimeType);

            PdfDocument document = PdfDocument.Open(pdf);

            _logger.LogInformation($"Printing PDF containing {document.PageCount} page(s) to printer '{printerName}'");

            using var printDocument = new PrintDocument();
            printDocument.DocumentName = printJobName ?? string.Empty;
            printDocument.PrinterSettings.PrinterName = printerName;
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
