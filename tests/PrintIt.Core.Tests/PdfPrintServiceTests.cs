using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using PrintIt.Core.Pdfium;

namespace PrintIt.Core.Tests
{
    public sealed class PdfPrintServiceTests
    {
        //// [Fact]
        public void Print_IntegrationTest()
        {
            // Arrange
            PdfLibrary.EnsureInitialized();
            var service = new PrintService(Mock.Of<ILogger<PrintService>>());
            using Stream stream = GetEmbeddedResourceStream("Pdfium.dummy.pdf");

            // Act
            service.Print(stream, "Some printer name");
        }

        private static Stream GetEmbeddedResourceStream(string name)
        {
            Type type = typeof(PdfPrintServiceTests);
            return type.Assembly.GetManifestResourceStream(type, name);
        }
    }
}
