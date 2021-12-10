using System.IO;

namespace PrintIt.Core.DocConverters
{
    public interface IConvertDocXToHtml
    {
        MemoryStream DocxToHtml(MemoryStream docMemoryStream);
    }
}