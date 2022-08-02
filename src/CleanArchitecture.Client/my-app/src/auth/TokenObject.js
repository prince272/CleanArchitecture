import Cookies from 'js-cookie';

class TokenStoarge {

    constructor() {
        if (this.constructor == TokenStoarge) {
            throw new Error("Cannot create an instance of an abstract class.");
        }
    }

    get(name) {
        throw new Error("The method or operation is not implemented.");
    }

    set(name, value) {
        throw new Error("The method or operation is not implemented.");
    }
}

class LocalTokenStorage extends TokenStoarge {
    constructor() {
        super();
    }

    get(name) {
        return localStorage.getItem(name);
    }

    set(name, value) {
        localStorage.setItem(name, value);
    }
}

class CookieTokenStorage extends TokenStoarge {
    constructor(cookieDomain, cookieSecure) {
        super();

        this.cookieDomain = cookieDomain;
        this.cookieSecure = cookieSecure;
    }

    get(name) {
        return Cookies.get(name);
    }

    set(name, value) {
        Cookies.set(name, value, {
            expires: expiresAt,
            domain: this.cookieDomain,
            secure: this.cookieSecure,
        });
    }
}