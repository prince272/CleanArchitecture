class BrowserError extends Error {
    constructor(message) {
        super(message);
    }
}

export class BrowserWindow {

    constructor(browserId, url, params = {}) {
        if (!browserId) {
            throw new Error('browserId was not supplied');
        }

        if (!url) {
            throw new Error('url was not supplied');
        }

        this._promise = new Promise((resolve, reject) => {
            this._resolve = resolve;
            this._reject = reject;
        });

        this._browser = (() => {
            const w = params?.width || 500;
            const h = params?.height || 600;
            const y = window.top.outerHeight / 2 + window.top.screenY - (h / 2);
            const x = window.top.outerWidth / 2 + window.top.screenX - (w / 2);

            let features = `width=${w}, height=${h}, top=${y}, left=${x}`;
            features = `toolbar=no, location=no, directories=no, status=no, menubar=no, scrollbars=no, resizable=no, copyhistory=no, ${features}`;
            const browser = window.open(url, '_blank', features);

            if (!browser) {
                throw new Error('Unable to open browser window');
            }

            window["browserCallback_" + browserId] = this._callback.bind(this);

            const checkForBrowserClosedInterval = 500;
            this._checkForBrowserClosedTimer = window.setInterval(this._checkForBrowserClosed.bind(this), checkForBrowserClosedInterval);

            browser.focus();
            return browser;
        })();
        this._browserId = browserId;
    }

    get browserId() {
        return this._browserId;
    }

    wait() {
        return this._promise;
    }

    _success(data) {
        console.debug("BrowserWindow.callback: Successful response from browser window");

        this._cleanup();
        this._resolve(data);
    }

    _error(message) {
        console.error("BrowserWindow.error: ", message);

        this._cleanup();
        this._reject(new BrowserError(message));
    }

    close() {
        this._cleanup(false);
    }

    _cleanup(keepOpen) {
        console.debug("BrowserWindow.cleanup");

        window.clearInterval(this._checkForBrowserClosedTimer);
        this._checkForBrowserClosedTimer = null;

        delete window["browserCallback_" + this._id];

        if (this._browser && !keepOpen) {
            this._browser.close();
        }
        this._browser = null;
    }

    _checkForBrowserClosed() {
        if (!this._browser || this._browser.closed) {
            this._error("Browser window closed");
        }
    }

    _callback(url, keepOpen) {
        this._cleanup(keepOpen);

        if (url) {
            console.debug("BrowserWindow.callback success");
            this._success({ url: url });
        }
        else {
            console.debug("BrowserWindow.callback: Invalid response from browser");
            this._error("Invalid response from browser");
        }
    }

    static notify(keepOpen) {
        const url = window.location;

        if (window.opener) {

            if (url) {
                const browserId = new URLSearchParams(url.search).get('browserId');

                if (browserId) {
                    const name = "browserCallback_" + browserId;
                    const callback = window.opener[name];

                    if (callback) {
                        console.debug("BrowserWindow.notifyOpener: passing url message to opener");
                        callback(url, keepOpen);
                    }
                    else {
                        console.warn("BrowserWindow.notifyOpener: no matching callback found on opener");
                    }
                }
                else {
                    console.warn("BrowserWindow.notifyOpener: no state found in response url");
                }
            }
        }
    }
}

export const isBrowserError = (error) => {
    return error instanceof BrowserError;
};