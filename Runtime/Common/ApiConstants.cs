namespace YagizEraslan.DeepSeek.Unity
{
    public static class ApiConstants
    {
        // API Configuration
        public const string API_URL = "https://api.deepseek.com/chat/completions";

        // HTTP Headers
        public const string CONTENT_TYPE_JSON = "application/json";
        public const string ACCEPT_SSE = "text/event-stream";
        public const string HEADER_AUTHORIZATION = "Authorization";
        public const string HEADER_CONTENT_TYPE = "Content-Type";
        public const string HEADER_ACCEPT = "Accept";
        public const string BEARER_PREFIX = "Bearer ";

        // SSE Protocol
        public const string SSE_DATA_PREFIX = "data: ";
        public const string SSE_DONE_MARKER = "[DONE]";

        // Default Request Parameters
        public const float DEFAULT_TEMPERATURE = 0.7f;
        public const int DEFAULT_MAX_TOKENS = 1000;
        public const float DEFAULT_TOP_P = 1f;
        public const float DEFAULT_TIMEOUT_SECONDS = 60f;

        // Memory Management
        public const int DEFAULT_MAX_HISTORY_MESSAGES = 50;
        public const int DEFAULT_HISTORY_TRIM_COUNT = 30;
        public const int DEFAULT_MAX_UI_MESSAGES = 100;
        public const int DEFAULT_UI_TRIM_COUNT = 70;

        // Logging
        public const string LOG_PREFIX_API = "[DeepSeekApi]";
        public const string LOG_PREFIX_STREAMING = "[DeepSeekStreamingApi]";
        public const string LOG_PREFIX_CHAT = "[DeepSeekChat]";
    }
}
