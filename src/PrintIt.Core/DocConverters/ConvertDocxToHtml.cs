using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using DocumentFormat.OpenXml.Packaging;
using OpenXmlPowerTools;

namespace PrintIt.Core.DocConverters
{
    public class ConvertDocxToHtml : IConvertDocXToHtml
    {
        /// <summary>
        /// Converts a .docx file into HTML using WmlToHtmlConverter from OpenXmlPowerTools.
        /// To keep spacing consistent in layout with tables keep a paragraph between tables.
        /// </summary>
        /// <param name="docMemoryStream">MemoryStream containting a WordprocessingDocument object.</param>
        /// <returns>Memory stream</returns>
        public MemoryStream DocxToHtml(MemoryStream docMemoryStream)
        {
            var wDoc = WordprocessingDocument.Open(docMemoryStream, true);
            var settings = new WmlToHtmlConverterSettings
            {
                AdditionalCss = string.Empty,
                FabricateCssClasses = true,
                CssClassPrefix = "pt-",
                RestrictToSupportedLanguages = false,
                RestrictToSupportedNumberingFormats = false,
                ImageHandler = (img) => UpdateImagePath(img),
            };
            XElement htmlElement = WmlToHtmlConverterWithPageBreaks.ConvertToHtml(wDoc, settings);
            var html = new XDocument(new XDocumentType("html", null, null, null), htmlElement);
            var htmlOut = html.ToString(SaveOptions.DisableFormatting);
            return new MemoryStream(Encoding.UTF8.GetBytes(htmlOut));
        }

        /// <summary>
        /// Note: This method fixes broken image paths from word docs when converting https://www.codeproject.com/Articles/1162184/Csharp-Docx-to-HTML-to-Docx
        /// </summary>
        /// <param name="imageInfo"></param>
        /// <returns></returns>
        private XElement UpdateImagePath(ImageInfo imageInfo)
        {
            string extension = imageInfo.ContentType.Split('/')[1].ToLower();
            ImageFormat imageFormat = null;
            if (extension == "png")
            {
                imageFormat = ImageFormat.Png;
            }
            else if (extension == "gif")
            {
                imageFormat = ImageFormat.Gif;
            }
            else if (extension == "bmp")
            {
                imageFormat = ImageFormat.Bmp;
            }
            else if (extension == "jpeg" || extension == "jpg")
            {
                imageFormat = ImageFormat.Jpeg;
            }
            else if (extension == "tiff")
            {
                imageFormat = ImageFormat.Tiff;
            }
            else if (extension == "x-wmf")
            {
                extension = "wmf";
                imageFormat = ImageFormat.Wmf;
            }

            if (imageFormat == null)
                return null;

            string base64 = null;
            try
            {
                using (var ms = new MemoryStream())
                {
                    imageInfo.Bitmap.Save(ms, imageFormat);
                    var ba = ms.ToArray();
                    base64 = Convert.ToBase64String(ba);
                }
            }
            catch (System.Runtime.InteropServices.ExternalException)
            {
                return null;
            }

            ImageFormat format = imageInfo.Bitmap.RawFormat;
            ImageCodecInfo codec = ImageCodecInfo.GetImageDecoders().First(c => c.FormatID == format.Guid);

            string imageSource = string.Format("data:{0};base64,{1}", codec.MimeType, base64);

            var img = new XElement(
                Xhtml.img,
                new XAttribute(NoNamespace.src, imageSource),
                imageInfo.ImgStyleAttribute,
                imageInfo.AltText != null ? new XAttribute(NoNamespace.alt, imageInfo.AltText) : null);
            return img;
        }
    }
}
