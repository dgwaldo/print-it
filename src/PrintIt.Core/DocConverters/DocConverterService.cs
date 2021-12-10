using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrintIt.Core.DocConverters {
    public class DocConverterService : IDocConverterService {

        private readonly IConvertDocxToPdf _docxToPdf;

        public DocConverterService(IConvertDocxToPdf docxToPdf) {
            _docxToPdf = docxToPdf ?? throw new ArgumentNullException(nameof(docxToPdf));
        }

        public Stream ConvertDocument(Stream data, string mimeType) {

            if (data == null) {
                throw new ArgumentNullException(nameof(data));
            }

            if (mimeType == null) {
                throw new ArgumentNullException(nameof(mimeType));
            }

            MemoryStream pdf = new MemoryStream();

            if (mimeType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document") {
                var memStream = new MemoryStream();
                data.CopyTo(memStream);
                pdf = _docxToPdf.DocxToPdf(memStream);
            }

            return pdf;

        }
    }
}
