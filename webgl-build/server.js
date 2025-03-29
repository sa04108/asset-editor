const express = require('express');
const https = require('https');
const path = require('path');
const fs = require('fs');

const app = express();
const port = 2002;

const options = {
  key: fs.readFileSync('./certificate/key.pem'),
  cert: fs.readFileSync('./certificate/cert.pem')
};

app.use(express.static(path.join(__dirname), {
  setHeaders: function (res, filePath) {
    if (filePath.endsWith('.br')) {
      res.setHeader('Content-Encoding', 'br');
    }
    if (filePath.endsWith('.wasm.br')) {
      res.setHeader('Content-Type', 'application/wasm');
    }
  }
}));

// HTTPS 서버 생성
https.createServer(options, app).listen(port, () => {
  console.log(`HTTPS server running at https://localhost:${port}`);
});
