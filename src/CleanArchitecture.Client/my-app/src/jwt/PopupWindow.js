import UrlUtility from './UrlUtility.js';

const CheckForPopupClosedInterval = 500;
const DefaultPopupTarget = "_blank";

export class PopupWindow {

    constructor(params) {
        params = params || {};
        this._promise = new Promise((resolve, reject) => {
            this._resolve = resolve;
            this._reject = reject;
        });

        let target = params.popupWindowTarget || DefaultPopupTarget;
        let features = params.popupWindowFeatures || (() => {
            const w = 500;
            const h = 600;
            const y = window.top.outerHeight / 2 + window.top.screenY - (h / 2);
            const x = window.top.outerWidth / 2 + window.top.screenX - (w / 2);
            return `toolbar=no, location=no, directories=no, status=no, menubar=no, scrollbars=no, resizable=no, copyhistory=no, width=${w}, height=${h}, top=${y}, left=${x}`;
        })();

        this._state = random();
        this._popup = window.open('', target, features);

        if (this._popup) {
            console.debug("PopupWindow.ctor: popup successfully created");
            this._checkForPopupClosedTimer = window.setInterval(this._checkForPopupClosed.bind(this), CheckForPopupClosedInterval);
        }
    }

    get state() {
        return this._state;
    }

    get promise() {
        return this._promise;
    }

    navigate(url) {
        if (!this._popup) {
            this._error("PopupWindow.navigate: error opening popup window");
        }
        else if (!url) {
            this._error("PopupWindow.navigate: no url provided");
            this._error("No url provided");
        }
        else {
            console.debug("PopupWindow.navigate: Setting URL in popup");

            window["popupCallback_" + this._state] = this._callback.bind(this);

            this._popup.focus();
            this._popup.window.location = url;
        }

        return this.promise;
    }

    _success(data) {
        console.debug("PopupWindow.callback: Successful response from popup window");

        this._cleanup();
        this._resolve(data);
    }
    _error(message) {
        console.error("PopupWindow.error: ", message);

        this._cleanup();
        this._reject(new Error(message));
    }

    close() {
        this._cleanup(false);
    }

    _cleanup(keepOpen) {
        console.debug("PopupWindow.cleanup");

        window.clearInterval(this._checkForPopupClosedTimer);
        this._checkForPopupClosedTimer = null;

        delete window["popupCallback_" + this._state];

        if (this._popup && !keepOpen) {
            this._popup.close();
        }
        this._popup = null;
    }

    _checkForPopupClosed() {
        if (!this._popup || this._popup.closed) {
            this._error("Popup window closed");
        }
    }

    _callback(url, keepOpen) {
        this._cleanup(keepOpen);

        if (url) {
            console.debug("PopupWindow.callback success");
            this._success({ url: url });
        }
        else {
            console.debug("PopupWindow.callback: Invalid response from popup");
            this._error("Invalid response from popup");
        }
    }

    static notify(url, keepOpen, delimiter) {
        if (window.opener) {
            url = url || window.location.href;
            if (url) {
                var data = UrlUtility.parseUrlFragment(url, delimiter);

                if (data.state) {
                    var name = "popupCallback_" + data.state;
                    var callback = window.opener[name];
                    if (callback) {
                        console.debug("PopupWindow.notifyOpener: passing url message to opener");
                        callback(url, keepOpen);
                    }
                    else {
                        console.warn("PopupWindow.notifyOpener: no matching callback found on opener");
                    }
                }
                else {
                    console.warn("PopupWindow.notifyOpener: no popup id found in response url.");
                }
            }
        }
        else {
            console.warn("PopupWindow.notifyOpener: no window.opener. Can't complete notification.");
        }
    }
}

function random() {
    function _uuidv4() {
        return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
            (c ^ Math.random() * 16 >> c / 4).toString(16)
        )
    }

    function _cryptoUuidv4() {
        return ([1e7] + -1e3 + -4e3 + -8e3 + -1e11).replace(/[018]/g, c =>
            (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
        )
    }

    const crypto = (typeof window !== 'undefined') ? (window.crypto || window.msCrypto) : null;
    var hasCrypto = crypto != 'undefined' && crypto !== null;
    var hasRandomValues = hasCrypto && (typeof (crypto.getRandomValues) != 'undefined');
    var uuid = hasRandomValues ? _cryptoUuidv4 : _uuidv4;
    return uuid().replace(/-/g, '');
}