// HTTPS on localhost using NextJS + Express
// source: https://stackoverflow.com/questions/55304101/https-on-localhost-using-nextjs-express

const { createServer } = require('https');
const { parse } = require('url');
const fs = require('fs');
const next = require('next');

const port = parseInt(process.env.PORT, 10) || 3000;
const dev = process.env.NODE_ENV !== 'production';
const app = next({ dev, hostname: 'localhost', port: port });
const handle = app.getRequestHandler();

const httpsOptions = {
  key: fs.readFileSync('./assets/certificates/localhost-key.pem'),
  cert: fs.readFileSync('./assets/certificates/localhost.pem'),
};

app.prepare()
  .then(() => {
    createServer(httpsOptions, (req, res) => {
      const parsedUrl = parse(req.url, true);
      handle(req, res, parsedUrl);
    }).listen(port, err => {
      if (err) throw err;
      console.log(`> Ready on https://localhost:${port}`);
    })
  });