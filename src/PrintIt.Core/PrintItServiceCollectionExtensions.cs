using System.Diagnostics.CodeAnalysis;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.Extensions.DependencyInjection;
using PrintIt.Core.DocConverters;

namespace PrintIt.Core
{
    [ExcludeFromCodeCoverage]
    public static class PrintItServiceCollectionExtensions
    {
        public static IServiceCollection AddPrintIt(this IServiceCollection services)
        {
            services.AddSingleton<ICommandService, CommandService>();
            services.AddSingleton<IPdfPrintService, PrintService>();
            services.AddSingleton<IPrinterService, PrinterService>();
            services.AddTransient<IConvertImgToPdf, ConvertImgToPdf>();
            services.AddSingleton<IConvertDocXToHtml, ConvertDocxToHtml>();
            services.AddSingleton<IConvertDocxToPdf, ConvertDocxToPdf>();
            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));
            services.AddTransient<IDocConverterService, DocConverterService>();

            return services;
        }
    }
}
