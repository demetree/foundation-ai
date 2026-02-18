
const PROXY_CONFIG = [
    {
        context: [
            "/api",
            "/swagger",
            "/Swagger",
            "/connect",
            "/oauth",
            "/.well-known"
        ],
        target: "https://localhost:12101",
        secure: false
    },
    {
        context: ["/AiChatSignal"],
        target: "https://localhost:12101",
        secure: false,
        ws: true
    }
]

module.exports = PROXY_CONFIG;
