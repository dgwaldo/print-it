using System.IO;

namespace PrintIt.Core.DocConverters {
    public interface IDocConverterService {
        Stream ConvertDocument(Stream data, string mimeType);
    }
}