import axios from 'axios';
import Cookies from 'js-cookie';
import QueryString from 'qs';

const getLocalStorage = () => {
    return {
        get: (key) => {
            const itemStr = window.localStorage.getItem(key);
            // if the item doesn't exist, return null
            if (!itemStr) {
                return null;
            }
            const item = JSON.parse(itemStr);
            const now = new Date();
            // compare the expiry time of the item with the current time
            if (now.getTime() > item.expiry.getTime()) {
                // If the item is expired, delete the item from storage
                // and return null
                window.localStorage.removeItem(key);
                return null;
            }
            return item.value;
        },
        set: (key, value, expiry) => {
            // `item` is an object which contains the original value
            // as well as the time when it's supposed to expire
            const item = {
                value: value,
                expiry: expiry,
            };

            window.localStorage.setItem(key, JSON.stringify(item));
        },
        remove: (key) => {
            window.localStorage.removeItem(key);
        }
    };
};

const getCookieStorage = (domain, secure) => {

    return {
        get: (key) => {
            return Cookies.get(key);
        },
        set: (key, value, expiry) => {
            Cookies.set(key, value, {
                expires: expiry,
                domain: domain,
                secure: secure,
            });
        },
        remove: (key) => {
            Cookies.remove(key, {
                domain: domain,
                secure: secure,
            });
        }
    };
};

const createHttpClient = (config) => {

    const defaultConfig = {
        paramsSerializer: params => {
            return QueryString.stringify(params);
        },
        storageType: 'local'
    };

    config = Object.assign({}, defaultConfig, config);
    const { storageType, ...config } = config;

    const httpClient = axios.create(config);
    const tokenStorage = (() => {
        switch (storageType) {
            case 'local': return getLocalStorage();
            case 'cookie': return () => {
                const baseURL = new URL(config.baseURL); 
                return getCookieStorage(baseURL.origin, baseURL.protocol == 'https:');
            };
            default: throw new Error(`StorageType of value '${storageType}' is not supported.`);
        }
    })();

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