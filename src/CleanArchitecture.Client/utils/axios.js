import { BehaviorSubject } from 'rxjs';
import { getCookie, setCookie, deleteCookie } from 'cookies-next';

const createCookieStorage = ({ clientURL, server }) => {

    const cookieURL = new URL(clientURL);
    const cookieDomain = cookieURL.hostname;
    const cookieSecure = cookieURL.protocol === 'https';

    return {
        get: (key) => {
            const itemStr = getCookie(key, { ...server, });
            // if the item doesn't exist, return null
            if (!itemStr) {
                return null;
            }

            const item = JSON.parse(itemStr);
            return item.value;
        },
        set: (key, value, ttl) => {
            const now = new Date();

            const item = {
                value
            };

            setCookie(key, JSON.stringify(item), {
                ...server, ... {
                    expires: new Date(now.getTime() + ttl),
                    domain: cookieDomain,
                    secure: cookieSecure,
                }
            });
        },
        remove: (key) => {
            deleteCookie(key, { ...server, });
        }
    };
};

const getUserStore = (settings) => {
    const accessTokenStorageKey = `_jwt_access_token_storage_${process.env.NODE_ENV}`;
    const refreshTokenStorageKey = `_jwt_refresh_token_storage_${process.env.NODE_ENV}`;
    const userStorageKey = `_jwt_user_storage_${process.env.NODE_ENV}`;

    const storage = createCookieStorage(settings);
    const accessToken = storage.get(accessTokenStorageKey);
    const refreshToken = storage.get(refreshTokenStorageKey);
    const user = storage.get(userStorageKey);

    const subject = new BehaviorSubject(user ? { ...user, accessToken, refreshToken } : null);

    const setAccessToken = (value, ttl) => {
        storage.set(accessTokenStorageKey, value, ttl);
    };

    const setRefreshToken = (value, ttl) => {
        storage.set(refreshTokenStorageKey, value, ttl);
    };

    const setUser = (value, ttl) => {
        storage.set(userStorageKey, value, ttl);
        subject.next(value);
    };

    const clear = () => {
        storage.remove(accessTokenStorageKey);
        storage.remove(refreshTokenStorageKey);
        storage.remove(userStorageKey);

        subject.next(null);
    };

    return {
        setAccessToken,
        setRefreshToken,
        setUser,

        subject,
        clear
    };
};

const getErrorState = (config) => {
    const namespace = 'axios-jwt-error';
    const errorState = config[namespace] || {};
    errorState.retry = errorState.retry || false;
    errorState.queued = errorState.queued || false;
    config[namespace] = errorState;
    return errorState;
};

const axiosAuth = (axios, settings) => {

    const defaultSettings = {
        unauthorized: (response) => {
            return response.status === 401;
        }
    };

    settings = Object.assign({}, defaultSettings, settings || {});

    const store = getUserStore(settings);

    let refreshing = false;
    let queue = [];

    // Function that resolves all items in the queue with the provided token
    const resolveQueue = () => {
        queue.forEach((p) => {
            p.resolve();
        });

        queue = [];
    };

    // Function that declines all items in the queue with the provided error
    const declineQueue = () => {
        queue.forEach((p) => {
            p.reject();
        });

        queue = [];
    };

    // Add a request interceptor
    axios.interceptors.request.use((config) => {
        const user = store.subject.getValue();

        if (user && user.accessToken && user.refreshToken) {
            config.headers[settings.authHeaderName] = `${settings.authHeaderPrefix}${accessToken}`;
        }
        else {
            delete config.headers[settings.authHeaderName];
            delete axios.defaults.headers.common[settings.authHeaderName];
        }

        return config;
    }, (error) => Promise.reject(error));

    // Add a response interceptor
    axios.interceptors.response.use((response) => response,
        (error) => {

            if (error.response) {

                if (settings.unauthorized(error.response)) {

                    // If we have no configraution to retry the request
                    const { config } = error;
                    if (!config) return Promise.reject(error);

                    // If we have already retired the request
                    const errorState = getErrorState(config);
                    if (errorState.retry || errorState.queued) return Promise.reject(error);

                    if (refreshing) {
                        return new Promise((resolve, reject) => queue.push({ resolve, reject }))
                            .then(() => {
                                errorState.queued = true;
                                return axios.request(config);
                            }).catch(() => {
                                return Promise.reject(error); // Ignore refresh token request's error and return actual "error" for the original request
                            });
                    }
                    else {
                        const user = store.subject.getValue();

                        if (!user?.refreshToken) {
                            store.clear();
                            return Promise.reject(error);
                        }

                        errorState.retry = true;
                        refreshing = true;

                        const refreshTokenCallback = settings.refreshTokenCallback(axios, { refreshToken })
                            .then((response) => {
                                const { accessToken, accessTokenExpiresIn, refreshToken, refreshTokenExpiresIn, user } = response.data;
                                store.setAccessToken(accessToken, accessTokenExpiresIn);
                                store.setRefreshToken(refreshToken, refreshTokenExpiresIn);
                                store.setUser(user, refreshTokenExpiresIn);
                            })
                            .catch((error) => {

                                if (error.response) {
                                    if (settings.unauthorized(error.response))
                                        store.clear();
                                }

                                return Promise.reject(error);
                            })
                            .finally(() => {
                                refreshing = false;
                            });

                        return refreshTokenCallback.then(() => {
                            resolveQueue();
                            return axios.request(config);
                        }).catch(() => {
                            declineQueue();
                            return Promise.reject(error);
                        });
                    }
                }

                // The request was made and the server responded with a status code
                // that falls out of the range of 2xx
                return Promise.reject(error);
            } else if (error.request) {

                // The request was made but no response was received
                // `error.request` is an instance of XMLHttpRequest in the browser and an instance of
                // http.ClientRequest in node.js
                return Promise.reject(error);
            } else {

                // Something happened in setting up the request that triggered an Error
                return Promise.reject(error);
            }
        });

    return {
        ...axios,
        store,

        signin: (data, requestConfig) => {
            return settings.generateTokenCallback(axios, data, requestConfig)
                .then((response) => {
                    const { accessToken, accessTokenExpiresIn, refreshToken, refreshTokenExpiresIn, user } = response.data;
                    store.setAccessToken(accessToken, accessTokenExpiresIn);
                    store.setRefreshToken(refreshToken, refreshTokenExpiresIn);
                    store.setUser(user, refreshTokenExpiresIn);
                    return response;
                });
        },
        signout: () => {
            const user = store.subject.getValue();

            if (!user?.refreshToken) {
                store.clear();
                return Promise.resolve();
            }

            return settings.revokeTokenCallback(axios, { refreshToken })
                .then((response) => {
                    store.clear();
                    return response;
                })
                .catch((error) => {
                    store.clear();
                    console.log(error);
                });
        }
    }
};

export { axiosAuth };