import ChangeAccountDialog from './account/ChangeAccountDialog';
import ChangePasswordDialog from './account/ChangePasswordDialog';
import VerifyAccountDialog from './account/VerifyAccountDialog';
import ResetPasswordDialog from './account/ResetPasswordDialog';
import SignInDialog from './account/SignInDialog';
import SignOutDialog from './account/SignOutDialog';
import SignUpDialog from './account/SignUpDialog';

import { findContextualRoute as findContextualRouteOnly } from './routes';
import { useCallback } from 'react';
import { useRouter } from 'next/router';
import QueryString from 'qs';

const views = [];
views.push({ key: 'changeAccount', Component: ChangeAccountDialog });
views.push({ key: 'changePassword', Component: ChangePasswordDialog });
views.push({ key: 'verifyAccount', Component: VerifyAccountDialog });
views.push({ key: 'resetPassword', Component: ResetPasswordDialog });
views.push({ key: 'signIn', Component: SignInDialog });
views.push({ key: 'signOut', Component: SignOutDialog });
views.push({ key: 'signUp', Component: SignUpDialog });

const findContextualRoute = (url) => {
    const contextualRoute = findContextualRouteOnly(url);
    if (contextualRoute) {
        const Component = views.find(view => view.key == contextualRoute.key)?.Component;
        if (Component) return { ...contextualRoute, Component };
    }
    else return null;
};

const useContextualRouting = () => {
    const PAGE_HREF_QUERY_PARAM = '_UCR_PAGE_PATH';

    const router = useRouter();
    const pageHrefQueryParam = router.query[PAGE_HREF_QUERY_PARAM];
    const watchedQuery = Object.assign({}, router.query);
    delete watchedQuery[PAGE_HREF_QUERY_PARAM];

    const pageHref = pageHrefQueryParam ?? '/';
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

export { findContextualRoute, useContextualRouting };
