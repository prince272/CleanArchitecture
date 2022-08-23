import { useCallback } from 'react';
import { useRouter } from 'next/router';
import { getPath, matchPath } from '../utils';
import QueryString from 'qs';
import ChangeAccountDialog from './account/ChangeAccountDialog';
import ChangePasswordDialog from './account/ChangePasswordDialog';
import VerifyAccountDialog from './account/VerifyAccountDialog';
import ResetPasswordDialog from './account/ResetPasswordDialog';
import SignInDialog from './account/SignInDialog';
import SignOutDialog from './account/SignOutDialog';
import SignUpDialog from './account/SignUpDialog';


const routes = [];
routes.push({ pattern: 'account/change', Component: ChangeAccountDialog });
routes.push({ pattern: 'account/password/change', Component: ChangePasswordDialog });
routes.push({ pattern: 'account/verify', Component: VerifyAccountDialog });
routes.push({ pattern: 'account/password/reset', Component: ResetPasswordDialog });
routes.push({ pattern: 'account/signin', Component: SignInDialog });
routes.push({ pattern: 'account/signout', Component: SignOutDialog });
routes.push({ pattern: 'account/signup', Component: SignUpDialog });

const CLIENT_URL = typeof window != 'undefined' ? window.env.CLIENT_URL : process.env.CLIENT_URL;

const findContextualRoute = (url) => {
    const contextualRoute = routes.map(route => {
        const match = matchPath(route.pattern, new URL(getPath(url), CLIENT_URL).pathname);
        return { ...route, match };
    }).filter(route => route.match != null)[0] || null;
    return contextualRoute;
};

const useContextualRouting = () => {
    const router = useRouter();
    const PAGE_PATH_KEY = '_UCR_PAGE_PATH';

    const getPagePath = () => router.query[PAGE_PATH_KEY] ? router.query[PAGE_PATH_KEY] : router.asPath;

    const constructLink = useCallback((url, params) => {
        url = require('url').format(url);

        params = { ...QueryString.parse(url.substring(url.indexOf('?') >= 0 ? url.indexOf('?') + 1 : url.length)), ...params };

        if (findContextualRoute(url)) {
            const path = {
                as: url,
                href: router.pathname + '?' + QueryString.stringify(
                    Object.assign({}, params, { [PAGE_PATH_KEY]: getPagePath() })
                )
            };
            return path;
        } else {
            return {
                href: url
            };
        }

    }, [router.asPath]);

    return { getPagePath, constructLink }
};


export { routes, useContextualRouting, findContextualRoute };