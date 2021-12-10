using System.IO;

namespace PrintIt.Core.DocConverters
{
    public interface IConvertDocxToPdf
    {
        MemoryStream DocxToPdf(MemoryStream docMemoryStream);
    }
}