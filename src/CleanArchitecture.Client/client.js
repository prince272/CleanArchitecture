import Axios from "axios";
import QueryString from "qs";
import { axiosJwt } from "./app/utils/axios";
import * as https from "https";
import * as fs from "fs";

const dev = process.env.NODE_ENV !== "production";

const client = axiosJwt(Axios.create({
    baseURL: 'https://localhost:7194',
    paramsSerializer: params => {
        return QueryString.stringify(params)
    },
    httpsAgent: dev ? new https.Agent({
        rejectUnauthorized: false, // (NOTE: this will disable client verification)
        key: fs.readFileSync("./assets/cert/localhost-key.pem"),
        cert: fs.readFileSync("./assets/cert/localhost-cert.pem"),
    }) : null
}),
    {
        clientURL: 'http://localhost:3000',
        generateTokenCallback: (request, data, requestConfig) => request.post('account/token/generate', data, requestConfig),
        refreshTokenCallback: (request, data, requestConfig) => request.post('account/token/refresh', data, requestConfig),
        revokeTokenCallback: (request, data, requestConfig) => request.post('account/token/revoke', data, requestConfig)
    });

export default client;