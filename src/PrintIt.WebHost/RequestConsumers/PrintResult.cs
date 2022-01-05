namespace PrintIt.Messages {
    public interface PrintResult {
        public bool IsSuccess { get; }
        public string Message { get; }
    }
}
