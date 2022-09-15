import QueryString from 'qs';
import Axios from 'axios';
import * as https from 'https';
import { axiosAuth } from './utils/axios';
import { addQueryParams, generateId } from './utils';
import { BrowserWindow } from './utils/browser';

export const SERVER_URL = typeof window !== 'undefined' ? window.env.SERVER_URL : process.env.SERVER_URL;
export const CLIENT_URL = typeof window !== 'undefined' ? window.env.CLIENT_URL : process.env.CLIENT_URL;
export const ENV_MODE = typeof window !== 'undefined' ? window.env.ENV_MODE : process.env.NODE_ENV;

export const AUTH_HEADER_NAME = 'Authorization';
export const AUTH_HEADER_PREFIX = 'Bearer ';

const createClient = (settings) => {
    let axios = Axios.create({
        baseURL: SERVER_URL,
        paramsSerializer: params => {
            return QueryString.stringify(params)
        },
        httpsAgent: (() => {

            if (ENV_MODE === 'development') {
                const httpsAgent = new https.Agent({
                    rejectUnauthorized: false,
                });
                // eslint-disable-next-line no-console
                console.log(ENV_MODE, `RejectUnauthorized is disabled.`);
                return httpsAgent;
            }

            return undefined;
        })(),
        withCredentials: true
    });

    axios = axiosAuth(axios, {
        clientURL: CLIENT_URL,
        authHeaderName: AUTH_HEADER_NAME,
        authHeaderPrefix: AUTH_HEADER_PREFIX,
        generateTokenCallback: async (request, { provider, returnUrl, ...data }, requestConfig) => {
            if (provider == 'credential') {
                return await request.post(`/account/token/generate`, data, requestConfig);
            }
            else {
                const browserId = generateId();
                try { await new BrowserWindow(browserId, addQueryParams(`${SERVER_URL}/account/${provider}/connect`, { returnUrl: addQueryParams(returnUrl, { browserId }) })).wait(); }
                catch (ex) { }
                return await request.post(`/account/${provider}/token/generate`, data, requestConfig);
            }
        },
        refreshTokenCallback: (request, data, requestConfig) => request.post('/account/token/refresh', data, requestConfig),
        revokeTokenCallback: (request, data, requestConfig) => request.post('/account/token/revoke', data, requestConfig),
        ...settings
    });

    return axios;
};

export { createClient };