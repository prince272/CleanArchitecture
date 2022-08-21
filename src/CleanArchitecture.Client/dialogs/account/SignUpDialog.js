import { DialogTitle, DialogContent, Grid, Stack, Box, Button, Typography, TextField, Link as MuiLink, Dialog } from '@mui/material';
import { LoadingButton } from '@mui/lab';
import { DialogCloseButton } from '../../components';
import * as Icons from '@mui/icons-material';
import PhoneInput from '../../components/PhoneInput';
import PasswordField from '../../components/PasswordField';
import Link from 'next/link';
import { useEffect, useState } from 'react';
import { useContextualRouting } from '../../utils/hooks';
import { Controller, useForm } from 'react-hook-form';
import client from '../../client';
import { preventDefault, formatError } from '../../utils';
import { useSnackbar } from 'notistack';
import { useRouter } from 'next/router';

const SignUpDialog = (props) => {
    const router = useRouter();
    const { returnPath, constructContextualPath } = useContextualRouting();
    const [provider, setProvider] = useState(null);
    const form = useForm();
    const formState = form.formState;
    const [fetcher, setFetcher] = useState({ state: 'idle', data: null });
    const { enqueueSnackbar } = useSnackbar();

    const onSubmit = async () => {

        setFetcher(fetcher => ({ ...fetcher, state: 'submitting' }));

        try {
            const inputs = form.watch();
            let response = await client.post('account/register', inputs);
            form.clearErrors();

            const contextualPathParams = constructContextualPath({ pathname: 'account/signin' }, { provider, inputs: JSON.stringify(inputs) });
            router.replace(contextualPathParams.href, contextualPathParams.as);
        }
        catch (unsafeError) {
            console.error(unsafeError);

            const error = (typeof unsafeError == 'object') ? { ...unsafeError } : {};

            form.clearErrors();
            form.handleSubmit(() => {
                const inputErrors = error?.response?.data?.errors || {};
                Object.entries(inputErrors).forEach(([name, message]) => form.setError(name, { type: 'server', message: message?.join('\n') }));
            })();

            enqueueSnackbar(formatError(error), { variant: 'error' });
        }
        finally {
            setFetcher(fetcher => ({ ...fetcher, state: 'idle' }));
        }
    };

    const onload = () => {
        form.reset({
            firstName: '',
            lastName: '',
            username: '',
            password: ''
        });
    };

    const onClose = () => {
        router.push(returnPath);
    };

    useEffect(() => { onload(); }, []);

    return (
        <Dialog {...props} onClose={onClose}>
            <DialogTitle component="div" sx={{ pt: 3, pb: 2, textAlign: "center", }}>
                <Typography variant="h5" component="h1" gutterBottom>Create your account</Typography>
                <Typography textAlign="center" variant="body2" gutterBottom>
                    {{
                        username: 'Enter your personal information'
                    }[provider] || 'Select the method you want to use'}
                </Typography>
                <DialogCloseButton onClose={onClose} />
            </DialogTitle>

            <DialogContent sx={{ px: 4, pb: 0 }}>
                {provider == 'username' ?
                    <Box component={"form"} onSubmit={preventDefault(onSubmit)}>
                        <Grid container pt={1} pb={4} spacing={3}>
                            <Grid item xs={12} sm={6}>
                                <Controller
                                    name="firstName"
                                    control={form.control}
                                    render={({ field }) => <TextField {...field}
                                        label="First name"
                                        variant="standard"
                                        error={!!formState.errors.firstName}
                                        helperText={formState.errors.firstName?.message}
                                        autoFocus
                                        fullWidth />}
                                />
                            </Grid>
                            <Grid item xs={12} sm={6}>
                                <Controller
                                    name="lastName"
                                    control={form.control}
                                    render={({ field }) => <TextField {...field}
                                        label="Last name"
                                        variant="standard"
                                        error={!!formState.errors.lastName}
                                        helperText={formState.errors.lastName?.message}
                                        fullWidth />}
                                />
                            </Grid>
                            <Grid item xs={12}>
                                <Controller
                                    name="username"
                                    control={form.control}
                                    render={({ field }) => <PhoneInput {...field}
                                        label="Email or Phone number"
                                        variant="standard"
                                        error={!!formState.errors.username}
                                        helperText={formState.errors.username?.message}
                                        fullWidth />}
                                />

                            </Grid>
                            <Grid item xs={12}>
                                <Controller
                                    name="password"
                                    control={form.control}
                                    render={({ field }) => <PasswordField {...field}
                                        label="Password"
                                        variant="standard"
                                        error={!!formState.errors.password}
                                        helperText={formState.errors.password?.message}
                                        fullWidth />}
                                />
                            </Grid>
                        </Grid>
                        <Box mb={3}>
                            <LoadingButton startIcon={<></>} loading={fetcher.state == 'submitting'} loadingPosition="start" type="submit" fullWidth variant="contained" size="large">
                                Sign Up
                            </LoadingButton>
                        </Box>
                    </Box> :
                    <>
                        <Stack pt={1} pb={3} spacing={2}>
                            <Button variant="contained" size="large" startIcon={<Icons.AccountCircle />} onClick={() => setProvider("username")}>Use Email or Phone</Button>
                            <Button variant="outlined" size="large" startIcon={<Icons.Google />}>Continue with Google</Button>
                        </Stack>
                    </>
                }
                <Typography textAlign="center" pb={4}>Already have an account? <Link {...constructContextualPath('account/signin')} passHref><MuiLink underline="hover">Sign in</MuiLink></Link></Typography>
            </DialogContent>
        </Dialog>
    );
};

export default SignUpDialog;