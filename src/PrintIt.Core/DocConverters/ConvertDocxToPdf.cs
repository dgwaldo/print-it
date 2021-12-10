using System;
using DinkToPdf;
using System.IO;
using System.Text;

namespace PrintIt.Core.DocConverters
{
    public class ConvertDocxToPdf : IConvertDocxToPdf
    {

        private IConvertDocXToHtml _docToHtml;
        private SynchronizedConverter _htmlToPdf;

        public ConvertDocxToPdf(IConvertDocXToHtml docToHtml, SynchronizedConverter htmlToPdf)
        {
            _docToHtml = docToHtml ?? throw new ArgumentNullException(nameof(docToHtml));
            _htmlToPdf = htmlToPdf ?? throw new ArgumentNullException(nameof(htmlToPdf));
        }

        public MemoryStream DocxToPdf(MemoryStream docMemoryStream)
        {
            var htmlStream = _docToHtml.DocxToHtml(docMemoryStream);
            var htmlText = Encoding.UTF8.GetString(htmlStream.ToArray());

            var htmlDoc = new HtmlToPdfDocument
            {
                GlobalSettings = {
                    DPI = 120,
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.Letter
                },
                Objects = {
                    new ObjectSettings {
                        HtmlContent = htmlText,
                        WebSettings = { DefaultEncoding = "utf-8" }
                    }
                }
            };

            byte[] pdf = _htmlToPdf.Convert(htmlDoc);

            return new MemoryStream(pdf) { Position = 0 };

        }

    }

}
