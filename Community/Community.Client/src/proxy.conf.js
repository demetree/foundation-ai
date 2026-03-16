
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
    target: "https://localhost:52369",
    secure: false
  }
]

module.exports = PROXY_CONFIG;
