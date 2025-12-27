const { env } = require('process');

// Use HTTPS in development for Google OAuth compatibility
const target = 'https://localhost:5001';

const PROXY_CONFIG = [
  {
    context: [
      "/weatherforecast",
      "/api",
   ],
    proxyTimeout: 10000,
    target: target,
    secure: false,
    headers: {
      Connection: 'Keep-Alive'
    }
  }
]

module.exports = PROXY_CONFIG;
