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
routes.push({ pattern: '/account/change', Component: ChangeAccountDialog });
routes.push({ pattern: '/account/password/change', Component: ChangePasswordDialog });
routes.push({ pattern: '/account/verify', Component: VerifyAccountDialog });
routes.push({ pattern: '/account/password/reset', Component: ResetPasswordDialog });
routes.push({ pattern: '/account/signin', Component: SignInDialog });
routes.push({ pattern: '/account/signout', Component: SignOutDialog });
routes.push({ pattern: '/account/signup', Component: SignUpDialog });

const CLIENT_URL = typeof window != 'undefined' ? window.env.CLIENT_URL : process.env.CLIENT_URL;

const findContextualRoute = (url) => {
    const contextualRoute = routes.map(route => {
        const match = matchPath(route.pattern, new URL(getPath(url), CLIENT_URL).pathname);
        return { ...route, match };
    }).filter(route => route.match != null)[0] || null;
    return contextualRoute;
};

const useContextualRouting = () => {
    const PAGE_HREF_QUERY_PARAM = '_UCR_PAGE_PATH';

    const router = useRouter();
    const pageHrefQueryParam = router.query[PAGE_HREF_QUERY_PARAM];
    const watchedQuery = Object.assign({}, router.query);
    delete watchedQuery[PAGE_HREF_QUERY_PARAM];

    const pageHref = pageHrefQueryParam ?? router.asPath;
    // @NOTE JSON.stringify might be replaced with any hashing solution
    const queryHash = JSON.stringify(watchedQuery);
    const constructLink = useCallback((urlString, hiddenParams) => {
        urlString = require('url').format(urlString);
        const urlParams = Object.fromEntries(new URLSearchParams(urlString.split('?')[1]).entries());
        const hiddenUrlString = `${router.pathname}?${QueryString.stringify({
            ...urlParams,
            ...hiddenParams,
            [PAGE_HREF_QUERY_PARAM]: pageHref
        })}`;


        if (findContextualRoute(urlString)) {
            return {
                as: urlString,
                href: hiddenUrlString
            };
        }
        else {
            return { href: urlString };
        }
    },
        [queryHash, pageHref]
    );

    return { getPagePath: () => pageHref, constructLink };
};

export { routes, useContextualRouting, findContextualRoute };