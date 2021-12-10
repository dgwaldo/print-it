using System;
using DinkToPdf;
using System.IO;
using System.Text;
using DinkToPdf.Contracts;
using System.Drawing;

namespace PrintIt.Core.DocConverters {
    public class ConvertImgToPdf : IConvertImgToPdf {

        private IConverter _htmlToPdf;

        public ConvertImgToPdf(IConverter htmlToPdf) {
            _htmlToPdf = htmlToPdf ?? throw new ArgumentNullException(nameof(htmlToPdf));
        }

        public MemoryStream ImgToPdf(Stream imgMemStream) {

            var imgBase64 = ImageToBase64(imgMemStream);

            var htmlText = @$"<!DOCTYPE html>
                                <html>
                                  <head>
                                    <title>Title of the document</title>
                                  </head>
                                  <body>
                                    <div>
                                      <p>From wikipedia</p>
                                      <img src=""{@imgBase64}"" alt=""Red dot"" />
                                        </div>
                                      </body >
                                    </html> ";

            var htmlDoc = new HtmlToPdfDocument {
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


        private string ImageToBase64(Stream imgStream) {
            var image = Image.FromStream(imgStream);

            using (MemoryStream m = new MemoryStream()) {
                image.Save(m, image.RawFormat);
                byte[] imageBytes = m.ToArray();

                // Convert byte[] to Base64 String
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }

    }
}
