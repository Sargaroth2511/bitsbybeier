const { env } = require('process');

// Use HTTP for testing
const target = 'http://localhost:5000';

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
