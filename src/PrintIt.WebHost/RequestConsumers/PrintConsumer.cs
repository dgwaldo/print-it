﻿using MassTransit;
using PrintIt.Core;
using PrintIt.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PrintIt.WebHost.RequestConsumers {
    public class PrintConsumer : IConsumer<ISendDocumentForPrint> {
        private readonly IPdfPrintService _pdfPrintService;

        public PrintConsumer(IPdfPrintService pdfPrintService) {
            _pdfPrintService = pdfPrintService ?? throw new ArgumentNullException(nameof(pdfPrintService));
        }

        public async Task Consume(ConsumeContext<ISendDocumentForPrint> context) {
            try {
                var message = context.Message;
                var fileStream = new MemoryStream(message.File);
                _pdfPrintService.Print(fileStream, message.FileContentType, message.PrinterPath, message.PageRange, message.FileName, duplex: message.Duplex);
                
                await context.RespondAsync<IPrintResult>(new {
                    IsSuccess = true,
                    Message = "Print Successful"
                });
            } catch (Exception e) {
                await context.RespondAsync<IPrintResult>(new {
                    IsSuccess = false,
                    Message = e.Message
                });
            }
        }
    }
}
