import ChangeAccountModal from './account/ChangeAccountModal';
import ChangePasswordModal from './account/ChangePasswordModal';
import ConfirmAccountModal from './account/ConfirmAccountModal';
import CreateAccountModal from './account/CreateAccountModal';
import ResetPasswordModal from './account/ResetPasswordModal';
import SignInModal from './account/SignInModal';
import SignOutModal from './account/SignOutModal';

const routes = [];
routes.push({ pattern: 'account/change', component: ChangeAccountModal });
routes.push({ pattern: 'account/password/change', component: ChangePasswordModal });
routes.push({ pattern: 'account/confirm', component: ConfirmAccountModal });
routes.push({ pattern: 'account/register', component: CreateAccountModal });
routes.push({ pattern: 'account/password/reset', component: ResetPasswordModal });
routes.push({ pattern: 'account/signin', component: SignInModal });
routes.push({ pattern: 'account/signup', component: SignOutModal });

export { routes };