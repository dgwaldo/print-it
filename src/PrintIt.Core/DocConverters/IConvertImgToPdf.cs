using System.IO;

namespace PrintIt.Core.DocConverters {
    public interface IConvertImgToPdf {
        MemoryStream ImgToPdf(Stream imgMemStream);
    }
}