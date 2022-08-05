import axios from 'axios';
import Cookies from 'js-cookie';
import QueryString from 'qs';
import { async, BehaviorSubject } from 'rxjs';

const withAuthHeader = (requestConfig, accessToken) => {
    if (accessToken) {
        requestConfig.headers = requestConfig.headers || {}; // Ensure geaders are defined.
        requestConfig.headers['Authorization'] = `Bearer ${accessToken}`;
    }

    return requestConfig;
};

const createLocalStorage = () => {
    return {
        get: (key) => {
            const itemStr = localStorage.getItem(key);

            // if the item doesn't exist, return null
            if (!itemStr) {
                return null;
            }

            const item = JSON.parse(itemStr);
            const now = new Date();

            // compare the expiry time of the item with the current time
            if (now.getTime() > item.expiry) {
                // If the item is expired, delete the item from storage
                // and return null
                localStorage.removeItem(key);
                return null;
            }

            return item.value;
        },
        set: (key, value, ttl) => {

            if (ttl == null) throw new Error(`Value '${ttl}' cannot be a null or undefined.`);

            const now = new Date();

            // `item` is an object which contains the original value
            // as well as the time when it's supposed to expire
            const item = {
                value: value,
                expiry: now.getTime() + ttl,
            };

            localStorage.setItem(key, JSON.stringify(item));
        },
        remove: (key) => {
            localStorage.removeItem(key);
        }
    };
};

const createCookieStorage = (domain, secure) => {

    return {
        get: (key) => {
            return Cookies.get(key);
        },
        set: (key, value, ttl) => {
            if (ttl == null) throw new Error(`Value '${ttl}' cannot be a null or undefined.`);

            const now = new Date();

            Cookies.set(key, value, {
                expires: new Date(now.getTime() + ttl),
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

const createAuthStore = ((baseLocation) => {
    const userStorage = createLocalStorage();
    const tokenStorage = createLocalStorage(baseLocation.orgin, baseLocation.protocol === 'https:');

    const userStorageKey = `_auth_user_storage_${process.env.NODE_ENV}`
    const accessTokenStorageKey = `_auth_access_token_storage_${process.env.NODE_ENV}`;
    const refreshTokenStorageKey = `_auth_refresh_token_storage_${process.env.NODE_ENV}`;

    const userSubject = new BehaviorSubject(userStorage.get(userStorageKey));

    const getAccessToken = () => {
        return tokenStorage.get(accessTokenStorageKey);
    };

    const getRefreshToken = () => {
        return tokenStorage.get(refreshTokenStorageKey);
    };

    const removeAccessToken = () => {
        return tokenStorage.remove(accessTokenStorageKey);
    };

    const removeRefreshToken = () => {
        return tokenStorage.remove(refreshTokenStorageKey);
    };

    const setTokens = (value) => {
        tokenStorage.set(accessTokenStorageKey, value.accessToken, value.accessTokenExpiresIn);
        tokenStorage.set(refreshTokenStorageKey, value.refreshToken, value.refreshTokenExpiresIn);
    };

    const setUser = (value, ttl) => {
        userStorage.set(userStorageKey, value, ttl);
        userSubject.next(value);
    };

    const getUser = () => {
        return userStorage.get(userStorageKey);
    };

    const clear = () => {
        tokenStorage.remove(accessTokenStorageKey);
        tokenStorage.remove(refreshTokenStorageKey);

        userStorage.remove(userStorageKey);
        userSubject.next(null);
    };

    return {
        getAccessToken,
        getRefreshToken,
        setTokens,

        removeAccessToken,
        removeRefreshToken,

        setUser,
        getUser,

        userSubject,

        clear
    };
});

const createAuthInterceptor = ({ authStore, refreshCallback }) => {

    let isRefreshing = false;
    let queue = [];

    // Function that resolves all items in the queue with the provided token.
    const resolveQueue = (token) => {
        queue.forEach((p) => {
            p.resolve(token);
        });
        queue = [];
    };

    // Function that declines all items in the queue with the provided error.
    const declineQueue = (error) => {
        queue.forEach((p) => {
            p.reject(error);
        });
        queue = [];
    };

    const refreshAccessToken = async () => {
        const accessToken = authStore.getAccessToken();
        if (accessToken) return accessToken;

        try {
            isRefreshing = true;

            // Refresh and store access token using the supplied refresh function.
            const response = await refreshCallback();
            return response.data.accessToken;
        } catch (error) {

            // Failed to refresh token.
            const status = error?.response?.status;

            if (status === 401 || status === 422) {

                // The refresh token is invalid so remove the stored tokens
                authStore.clear();

                throw new Error(`Got ${status} on token refresh; clearing both auth tokens`);
            } else {

                // A different error, probably network error
                throw new Error(`Failed to refresh auth token: ${error.message}`);
            }
        } finally {
            isRefreshing = false;
        }
    };

    return async (requestConfig) => {
        if (!authStore.getRefreshToken()) return requestConfig;

        // Queue the request if another refresh request is currently happening.
        if (isRefreshing) {
            return new Promise((resolve, reject) => {
                queue.push({ resolve, reject });
            })
                .then((accessToken) => {
                    return withAuthHeader(requestConfig, accessToken);
                })
                .catch(Promise.reject);
        }

        // Do refresh if needed.
        let accessToken = null;
        try {
            accessToken = await refreshAccessToken();
            resolveQueue(accessToken);
        } catch (error) {
            if (error instanceof Error) {
                declineQueue(error);
                throw new Error(`Unable to refresh access token for request due to token refresh error: ${error.message}`);
            }
        }

        // add token to headers.
        return withAuthHeader(requestConfig, accessToken);
    }
};

const createHttpClient = (config) => {
    const defaultConfig = {
        paramsSerializer: params => {
            return QueryString.stringify(params);
        }
    };

    config = Object.assign({}, defaultConfig, config);

    const request = axios.create({ baseURL: config.baseURL });
    const authStore = createAuthStore(new URL(config.baseURL));

    const signinCallback = (data, requestConfig = {}) => {
        return config.signinCallback(request, data, requestConfig)
            .then((response) => {
                authStore.setTokens(response.data);

                return config.userCallback(request, withAuthHeader({}, response.data.accessToken))
                    .then(userResponse => {
                        authStore.setUser(userResponse.data, response.data.accessTokenExpiresIn);
                        return response;
                    });
            });
    };

    const refreshCallback = (data, requestConfig = {}) => {

        const refreshToken = authStore.getRefreshToken();
        if (!refreshToken) throw new Error('No refresh token available.');

        return config.refreshCallback(request, { ...data, refreshToken }, requestConfig)
            .then((response) => {
                authStore.setTokens(response.data);
                
                return config.userCallback(request, withAuthHeader({}, response.data.accessToken))
                    .then(userResponse => {
                        authStore.setUser(userResponse.data, response.data.accessTokenExpiresIn);
                        return response;
                    });
            });
    };

    const signoutCallback = (data, requestConfig = {}) => {

        const refreshToken = authStore.getRefreshToken();
        if (!refreshToken) throw new Error('No refresh token available.');

        return config.signoutCallback(request, { ...data, refreshToken }, requestConfig)
        .then((response) => {
            authStore.clear();
            return response;
        });
    };

    const httpClient = axios.create(config);
    httpClient.interceptors.request.use(createAuthInterceptor({ authStore, refreshCallback }));
    return { ...httpClient, auth: authStore, signin: signinCallback, signout: signoutCallback };
};

export { createHttpClient };