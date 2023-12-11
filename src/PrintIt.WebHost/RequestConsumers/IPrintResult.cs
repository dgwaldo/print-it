namespace PrintIt.Messages {
    public interface IPrintResult {
        public bool IsSuccess { get; }
        public string Message { get; }
    }
}
