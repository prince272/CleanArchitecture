import { getPath, matchPath } from '../utils';
import { CLIENT_URL } from '../client';

const routes = [];
routes.push({ key: 'changeAccount', pattern: '/account/change' });
routes.push({ key: 'changePassword', pattern: '/account/password/change' });
routes.push({ key: 'verifyAccount', pattern: '/account/verify' });
routes.push({ key: 'resetPassword', pattern: '/account/password/reset' });
routes.push({ key: 'signIn', pattern: '/account/signin' });
routes.push({ key: 'signOut', pattern: '/account/signout' });
routes.push({ key: 'signUp', pattern: '/account/signup' });

const findContextualRoute = (url) => {
    const contextualRoute = routes.map(route => {
        const match = matchPath(route.pattern, new URL(getPath(url), CLIENT_URL).pathname);
        return { ...route, match };
    }).filter(route => route.match != null)[0] || null;
    return contextualRoute;
};

export { findContextualRoute };