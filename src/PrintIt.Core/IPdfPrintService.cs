using System.IO;

namespace PrintIt.Core {
    public interface IPdfPrintService {
        void Print(Stream pdfStream, string mimeType, string printerName, string pageRange = null, string printJobName = null);
    }
}