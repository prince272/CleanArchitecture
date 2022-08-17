import { useRouter } from 'next/router';
import { useEffect } from 'react';
import { useDialog } from '../utils/dialog';
import { useContextualRouting } from '../utils/hooks';
import { matchPath } from '../utils';

import ChangeAccountDialog from './account/ChangeAccountDialog';
import ChangePasswordDialog from './account/ChangePasswordDialog';
import ConfirmAccountDialog from './account/ConfirmAccountDialog';
import CreateAccountDialog from './account/CreateAccountDialog';
import ResetPasswordDialog from './account/ResetPasswordDialog';
import SignInDialog from './account/SignInDialog';
import SignOutDialog from './account/SignOutDialog';

const dialogRoutes = [];
dialogRoutes.push({ pattern: 'account/change', Component: ChangeAccountDialog });
dialogRoutes.push({ pattern: 'account/password/change', Component: ChangePasswordDialog });
dialogRoutes.push({ pattern: 'account/confirm', Component: ConfirmAccountDialog });
dialogRoutes.push({ pattern: 'account/create', Component: CreateAccountDialog });
dialogRoutes.push({ pattern: 'account/password/reset', Component: ResetPasswordDialog });
dialogRoutes.push({ pattern: 'account/signin', Component: SignInDialog });
dialogRoutes.push({ pattern: 'account/signout', Component: SignOutDialog });

const DialogRoute = () => {
    const router = useRouter();
    const dialog = useDialog();
    const { returnPath, contextualPath } = useContextualRouting();

    useEffect(() => {

        if (contextualPath) {

            const currentRoute = dialogRoutes.map(route => {
                const match = matchPath(route.pattern, new URL(contextualPath, window.location.origin).pathname);
                return { ...route, match };
            }).filter(route => route.match != null)[0];

            if (currentRoute) {
                const Component = currentRoute.Component;
                const closeDialog = () => router.push(returnPath);
                dialog.replace({ onClose: closeDialog, children: <Component {...{ closeDialog }} /> });
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