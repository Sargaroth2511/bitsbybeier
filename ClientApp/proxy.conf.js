const { env } = require('process');

// Use HTTPS for API calls (matches backend default)
const target = 'https://localhost:5001';

const PROXY_CONFIG = [
  {
    context: [
      "/weatherforecast",
      "/api",
   ],
    proxyTimeout: 10000,
    target: target,
    secure: false, // Allow self-signed certificates in development
    changeOrigin: true,
    logLevel: 'debug',
    headers: {
      Connection: 'Keep-Alive'
    }
  }
]

module.exports = PROXY_CONFIG;
