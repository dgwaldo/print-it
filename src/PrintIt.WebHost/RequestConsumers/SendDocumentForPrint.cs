﻿namespace PrintIt.Messages {
    public interface SendDocumentForPrint {
        public byte[] File { get; }
        public string FileName { get; }
        public string FileContentType { get; }
        public string PrinterPath { get; }
        public string PageRange { get; }
        public bool Duplex { get; }
    }
}
