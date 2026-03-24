
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
    target: "https://localhost:10101",
    secure: false
  },
  {
    context: [
      "/FileManagerSignal",
      "/SchedulerSignal",
      "/hubs"
    ],
    target: "https://localhost:10101",
    secure: false,
    ws: true
  }
]

module.exports = PROXY_CONFIG;
