import ChangeAccountDialog from './account/ChangeAccountDialog';
import ChangePasswordDialog from './account/ChangePasswordDialog';
import VerifyAccountDialog from './account/VerifyAccountDialog';
import ResetPasswordDialog from './account/ResetPasswordDialog';
import SignInDialog from './account/SignInDialog';
import SignOutDialog from './account/SignOutDialog';
import SignUpDialog from './account/SignUpDialog';

import { findContextualRoute, PAGE_PATH_QUERY_PARAM } from './routes';
import { useCallback } from 'react';
import Router, { useRouter } from 'next/router';
import { resolveHref } from 'next/dist/shared/lib/router/router';
import { addQueryParams } from '../utils';

const views = [];
views.push({ key: 'ChangeAccountDialog', Component: ChangeAccountDialog });
views.push({ key: 'ChangePasswordDialog', Component: ChangePasswordDialog });
views.push({ key: 'VerifyAccountDialog', Component: VerifyAccountDialog });
views.push({ key: 'ResetPasswordDialog', Component: ResetPasswordDialog });
views.push({ key: 'SignInDialog', Component: SignInDialog });
views.push({ key: 'SignOutDialog', Component: SignOutDialog });
views.push({ key: 'SignUpDialog', Component: SignUpDialog });

const findContextualRouteWithComponent = (url) => {
    const contextualRoute = findContextualRoute(url);
    if (contextualRoute) {
        const Component = views.find(view => view.key == contextualRoute.key)?.Component;
        if (Component) return { ...contextualRoute, Component };
    }
    else return null;
};

const useContextualRouting = () => {
    const router = useRouter();

    const getPagePath = function () {
        return new URLSearchParams(router.asPath.split('?')[1]).get(PAGE_PATH_QUERY_PARAM) || '/';
    };

    const constructLink = useCallback((urlString, hiddenParams) => {
        urlString = require('url').format(urlString);

        if (findContextualRoute(urlString)) {

            const [, pagePath] = resolveHref(Router, router.route, true);

            urlString = addQueryParams(urlString, { [PAGE_PATH_QUERY_PARAM]: pagePath });
            const hiddenUrlString = addQueryParams(urlString, { ...hiddenParams, [PAGE_PATH_QUERY_PARAM]: pagePath })

            return {
                href: hiddenUrlString,
                as: urlString
            }
        }
        else {
            return { href: urlString };
        }
    },
        [router.asPath, JSON.stringify(router.query)]
    );

    return { getPagePath, constructLink };
};

export { findContextualRouteWithComponent, useContextualRouting };
