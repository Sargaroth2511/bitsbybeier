const { env } = require('process');

// Default to the HTTPS port from launchSettings (7193) to avoid proxying to the old IIS Express port.
const target = env.ASPNETCORE_HTTPS_PORT
  ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}`
  : env.ASPNETCORE_URLS
    ? env.ASPNETCORE_URLS.split(';')[0]
    : 'https://localhost:7193';

const PROXY_CONFIG = [
  {
    context: [
      "/weatherforecast",
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
