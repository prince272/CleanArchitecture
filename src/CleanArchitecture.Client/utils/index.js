import { AxiosError } from 'axios';

export const isPhoneFormat = (value) => {
    function isNullOrWhitespace(input) {
        return (typeof input === 'undefined' || input == null)
            || input.replace(/\s/g, '').length < 1;
    }

    if (isNullOrWhitespace(value))
        return false;

    const result = new RegExp("^[ 0-9\.\,\+\-]*$").test(value || '');
    return result;
};

export function warning(cond, message) {
    if (!cond) {
        // eslint-disable-next-line no-console
        if (typeof console !== "undefined") console.warn(message);

        try {
            // Welcome to debugging React Router!
            //
            // This error is thrown as a convenience so you can more easily
            // find the source for a warning that appears in the console by
            // enabling "pause on exceptions" in your JavaScript debugger.
            throw new Error(message);
            // eslint-disable-next-line no-empty
        } catch (e) { }
    }
}

/**
 * Performs pattern matching on a URL pathname and returns information about
 * the match.
 *
 * @see https://reactrouter.com/docs/en/v6/utils/match-path
 */
export function matchPath(pattern, pathname) {
    if (typeof pattern === "string") {
        pattern = { path: pattern, caseSensitive: false, end: true };
    }

    let [matcher, paramNames] = compilePath(
        pattern.path,
        pattern.caseSensitive,
        pattern.end
    );

    let match = pathname.match(matcher);
    if (!match) return null;

    let matchedPathname = match[0];
    let pathnameBase = matchedPathname.replace(/(.)\/+$/, "$1");
    let captureGroups = match.slice(1);
    let params = paramNames.reduce(
        (memo, paramName, index) => {
            // We need to compute the pathnameBase here using the raw splat value
            // instead of using params["*"] later because it will be decoded then
            if (paramName === "*") {
                let splatValue = captureGroups[index] || "";
                pathnameBase = matchedPathname
                    .slice(0, matchedPathname.length - splatValue.length)
                    .replace(/(.)\/+$/, "$1");
            }

            memo[paramName] = safelyDecodeURIComponent(
                captureGroups[index] || "",
                paramName
            );
            return memo;
        },
        {}
    );

    return {
        params,
        pathname: matchedPathname,
        pathnameBase,
        pattern,
    };
}

function compilePath(path, caseSensitive = false, end = true) {
    warning(
        path === "*" || !path.endsWith("*") || path.endsWith("/*"),
        `Route path "${path}" will be treated as if it were ` +
        `"${path.replace(/\*$/, "/*")}" because the \`*\` character must ` +
        `always follow a \`/\` in the pattern. To get rid of this warning, ` +
        `please change the route path to "${path.replace(/\*$/, "/*")}".`
    );

    let paramNames = [];
    let regexpSource =
        "^" +
        path
            .replace(/\/*\*?$/, "") // Ignore trailing / and /*, we'll handle it below
            .replace(/^\/*/, "/") // Make sure it has a leading /
            .replace(/[\\.*+^$?{}|()[\]]/g, "\\$&") // Escape special regex chars
            .replace(/:(\w+)/g, (_, paramName) => {
                paramNames.push(paramName);
                return "([^\\/]+)";
            });

    if (path.endsWith("*")) {
        paramNames.push("*");
        regexpSource +=
            path === "*" || path === "/*"
                ? "(.*)$" // Already matched the initial /, just match the rest
                : "(?:\\/(.+)|\\/*)$"; // Don't include the / in params["*"]
    } else {
        regexpSource += end
            ? "\\/*$" // When matching to the end, ignore trailing slashes
            : // Otherwise, match a word boundary or a proceeding /. The word boundary restricts
            // parent routes to matching only their own words and nothing more, e.g. parent
            // route "/home" should not match "/home2".
            // Additionally, allow paths starting with `.`, `-`, `~`, and url-encoded entities,
            // but do not consume the character in the matched path so they can match against
            // nested paths.
            "(?:(?=[.~-]|%[0-9A-F]{2})|\\b|\\/|$)";
    }

    let matcher = new RegExp(regexpSource, caseSensitive ? undefined : "i");

    return [matcher, paramNames];
}

function safelyDecodeURIComponent(value, paramName) {
    try {
        return decodeURIComponent(value);
    } catch (error) {
        warning(
            false,
            `The value for the URL param "${paramName}" will not be decoded because` +
            ` the string "${value}" is a malformed URL segment. This is probably` +
            ` due to a bad percent encoding (${error}).`
        );

        return value;
    }
}

// JavaScript snippet: Remove base URL from link
// source: https://www.wkoorts.com/2012/10/09/javascript-snippet-remove-base-url-from-link/
export function getPath(url) {
    /*
     * Replace base URL in given string, if it exists, and return the result.
     *
     * e.g. "http://localhost:8000/api/v1/blah/" becomes "/api/v1/blah/"
     *      "/api/v1/blah/" stays "/api/v1/blah/"
     */
    let baseUrlPattern = /^(?<Protocol>\w+):\/\/(?<Domain>[\w@][\w.:@]+)*/;
    let result = "";

    let match = baseUrlPattern.exec(url);
    if (match != null) {
        result = match[0];
    }

    if (result.length > 0) {
        url = url.replace(result, "");
    }

    return url;
}

// 
// source: https://github.com/donavon/prevent-default
export const preventDefault = (cb) => {
    return (event, ...others) => {
        event.preventDefault();
        cb(event, ...others);
    }
};


// Axios handling errors
// source: https://stackoverflow.com/questions/49967779/axios-handling-errors

export const getErrorInfo = (error) => {

    let title = null;
    let detail = null;
    let info = null;
    let canRetry = true;

    if (error.response) {

        // The request was made and the server responded with a status code
        // that falls out of the range of 2xx
        info = error.response.data;
        canRetry = (error.response.status >= 500) && (error.response.status <= 599);

        if (error.response.status == 401) {
            title = info.title || 'Authentication Required';
            detail = info.detail || `We were unable to verify your identity. Please sign in again and if the problem persists, contact support.`;
        }
        else if (error.response.status == 404) {
            title = info.title || 'Resource Not Found';
            detail = info.detail || `The resource is not available.`;
        }
        else {
            title = info.title || ``;
            detail = info.detail || `A ${error.response.status} error occurred. Please contact support if the problem persists.`;
        }
    }
    else if (error.request) {

        // The request was made but no response was received
        // `error.request` is an instance of XMLHttpRequest in the browser and an instance of
        // http.ClientRequest in node.js

        if (error.code === AxiosError.ERR_NETWORK) {
            title = 'No internet connection';
            detail = 'There is no internet connection. Please check your internet connection and try again.';
        }
        else {
            title = 'Oops! Something went wrong';
            detail = 'An unexpected error occurred. Please contact support if the problem persists.';
        }
    }
    else {
        title = 'Oops! Something went wrong';
        detail = 'An unexpected error occurred. Please contact support if the problem persists.';
    }

    return { title, detail, canRetry, ...info };
};

export function isHttpError(payload) {
    return payload !== null && typeof payload === 'object' && (payload.isAxiosError === true);
};

// useCombinedRefs - CodeSandbox
// source: https://codesandbox.io/s/uhj08?file=/src/App.js:223-537
export const setRefs = (...refs) => (element) => {
    refs.forEach((ref) => {
        if (!ref) {
            return;
        }

        // Ref can have two types - a function or an object. We treat each case.
        if (typeof ref === "function") {
            return ref(element);
        }

        ref.current = element;
    });
};

// Encrypt and decrypt a string using simple Javascript without using any external library
// source: https://javascript.tutorialink.com/encrypt-and-decrypt-a-string-using-simple-javascript-without-using-any-external-library/
export function compressString(string) {
    string = unescape(encodeURIComponent(string));
    var newString = '',
        char, nextChar, combinedCharCode;
    for (var i = 0; i < string.length; i += 2) {
        char = string.charCodeAt(i);

        if ((i + 1) < string.length) {


            nextChar = string.charCodeAt(i + 1) - 31;


            combinedCharCode = char + "" + nextChar.toLocaleString('en', {
                minimumIntegerDigits: 2
            });

            newString += String.fromCharCode(parseInt(combinedCharCode, 10));

        } else {


            newString += string.charAt(i);
        }
    }
    return btoa(unescape(encodeURIComponent(newString)));
};

export function decompressString(string) {

    var newString = '',
        char, codeStr, firstCharCode, lastCharCode;
    string = decodeURIComponent(escape(atob(string)));
    for (var i = 0; i < string.length; i++) {
        char = string.charCodeAt(i);
        if (char > 132) {
            codeStr = char.toString(10);

            firstCharCode = parseInt(codeStr.substring(0, codeStr.length - 2), 10);

            lastCharCode = parseInt(codeStr.substring(codeStr.length - 2, codeStr.length), 10) + 31;

            newString += String.fromCharCode(firstCharCode) + String.fromCharCode(lastCharCode);
        } else {
            newString += string.charAt(i);
        }
    }
    return newString;
};

export const setQueryParams = (url, queryParams) => {
    // source: 
    const combineQueryString = (a, b, overwrite = false) => {
        a = new URLSearchParams(a);
        const fn = overwrite ? a.set : a.append;
        for (let [key, value] of new URLSearchParams(b)) {
            fn.call(a, key, value);
        }
        return a.toString();
    };

    let path = url.split("?")[0];
    let queryString = url.split("?")[1];

    queryString = combineQueryString(queryParams, queryString, true);
    url = path + (queryString ? `?${queryString}` : '');
    return url;
};

// Remove an empty query string from url
// source: https://stackoverflow.com/questions/36331073/remove-an-empty-query-string-from-url/36331581#36331581
export const removeEmptyQueryParams = (uri) => {
    return uri.replace(/([\?&])([^=]+=($|&))+/g, '$1').replace(/[\?&]$/g, '');
};

// Remove blank attributes from an Object in Javascript
// source: https://stackoverflow.com/questions/286141/remove-blank-attributes-from-an-object-in-javascript#answer-38340730
export const cleanObject = (obj) => {
    return Object.fromEntries(
        Object.entries(obj)
            .filter(([_, v]) => v != null)
            .map(([k, v]) => [k, v === Object(v) ? cleanObject(v) : v])
    );
};

export const generateId = (length = 16) => {
    return parseInt(Math.ceil(Math.random() * Date.now()).toPrecision(length).toString().replace(".", ""))
};
