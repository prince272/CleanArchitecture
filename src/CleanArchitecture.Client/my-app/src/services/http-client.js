import axios from 'axios';
import Cookies from 'js-cookie';
import QueryString from 'qs';

const createHttpClient = (config) => {

    const defaultConfig = {
        paramsSerializer: params => {
            return QueryString.stringify(params);
        },
    };
    config = Object.assign({}, defaultConfig, config);

    const httpClient = axios.create(config);

    httpClient.interceptors.request.use(function (requestConfig) {

        return requestConfig;
    }, function (error) {
        return Promise.reject(error);
    });

    httpClient.interceptors.response.use(function (response) {
        return response;
    }, function (error) {
        return Promise.reject(error);
    });

    return httpClient;
};

export { createHttpClient };