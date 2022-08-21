import { useRouter } from 'next/router';
import { useEffect } from 'react';
import { useDialog } from '../utils/dialog';
import { useContextualRouting } from '../utils/hooks';
import { matchPath } from '../utils';

import ChangeAccountDialog from './account/ChangeAccountDialog';
import ChangePasswordDialog from './account/ChangePasswordDialog';
import VerifyAccountDialog from './account/VerifyAccountDialog';
import ResetPasswordDialog from './account/ResetPasswordDialog';
import SignInAccountDialog from './account/SignInAccountDialog';
import SignOutAccountDialog from './account/SignOutAccountDialog';
import SignUpAccountDialog from './account/SignUpAccountDialog';

const dialogRoutes = [];
dialogRoutes.push({ pattern: 'account/change', Component: ChangeAccountDialog });
dialogRoutes.push({ pattern: 'account/password/change', Component: ChangePasswordDialog });
dialogRoutes.push({ pattern: 'account/verify', Component: VerifyAccountDialog });
dialogRoutes.push({ pattern: 'account/password/reset', Component: ResetPasswordDialog });
dialogRoutes.push({ pattern: 'account/signin', Component: SignInAccountDialog });
dialogRoutes.push({ pattern: 'account/signout', Component: SignOutAccountDialog });
dialogRoutes.push({ pattern: 'account/signup', Component: SignUpAccountDialog });

const DialogRoute = () => {
    const dialog = useDialog();
    const { contextualPath } = useContextualRouting();

    useEffect(() => {

        if (contextualPath) {

            const currentRoute = dialogRoutes.map(route => {
                const match = matchPath(route.pattern, new URL(contextualPath, window.location.origin).pathname);
                return { ...route, match };
            }).filter(route => route.match != null)[0];

            if (currentRoute) {
                const Component = currentRoute.Component;
                dialog.replace({ Component });
            }
        }
        else {
            dialog.close();
        }

    }, [contextualPath]);

    return (
        <>
        </>
    );
};

export { DialogRoute, dialogRoutes };