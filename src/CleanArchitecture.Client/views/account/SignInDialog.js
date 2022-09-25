import { DialogTitle, DialogContent, Grid, Stack, Box, Button, Typography, TextField, Link as MuiLink, Dialog } from '@mui/material';
import { LoadingButton } from '@mui/lab';
import { DialogCloseButton, PasswordField, PhoneTextField, useClient } from '../../components/';
import * as Icons from '@mui/icons-material';
import Link from 'next/link';
import { useEffect, useState, useMemo } from 'react';
import { Controller, useForm } from 'react-hook-form';
import { preventDefault, getErrorInfo, isHttpError, getPath } from '../../utils';
import { useSnackbar } from 'notistack';
import { useRouter } from 'next/router';
import { useContextualRouting } from '../routes.views';
import { CLIENT_URL } from '../../client';


const SignInDialog = (props) => {
    const router = useRouter();
    const client = useClient();

    const form = useForm();
    const formState = form.formState;

    const returnUrl = router.query.returnUrl || '/';

    const { getPagePath, constructLink } = useContextualRouting();
    const [provider, setProvider] = useState(router.query?.provider || null);

    const [fetcher, setFetcher] = useState({ state: 'idle' });
    const { enqueueSnackbar } = useSnackbar();

    const onSubmit = async ({ provider, ...inputs }) => {
        setProvider(provider);

        try {
            setFetcher({ state: 'submitting' });

            await client.signin({ ...inputs, provider, returnUrl: new URL(getPath(returnUrl), CLIENT_URL).toString() });
            form.clearErrors();

            const link = constructLink(returnUrl);
            router.push(link);
        }
        catch (error) {
            console.error(error);

            if (isHttpError(error)) {
                const { reason } = error?.response?.data || {};

                if (reason == 'requiresVerification') {

                    const link = constructLink({ pathname: '/account/verify', query: { returnUrl: router.asPath } }, {
                        state: JSON.stringify({ inputs, provider })
                    });

                    router.push(link.href, link.as);
                }
                else {

                    form.clearErrors();
                    form.handleSubmit(() => {
                        const inputErrors = error?.response?.data?.errors || {};
                        Object.entries(inputErrors).forEach(([name, message]) => form.setError(name, { type: 'server', message: message?.join('\n') }));
                    })();

                    enqueueSnackbar(getErrorInfo(error).title, { variant: 'error' });
                }
            }
            else {
                enqueueSnackbar(getErrorInfo(error).title, { variant: 'error' });
            }
        }
        finally {
            setFetcher({ state: 'idle' });
        }
    };

    const onLoad = () => {
        const initialInputs = JSON.parse(router.query.inputs || null);

        if (initialInputs) {
            form.reset({
                username: initialInputs.username,
                password: initialInputs.password
            });

            onSubmit({ ...form.watch(), provider });
        }
        else {
            form.reset({
                username: '',
                password: ''
            });
        }
    };

    const onClose = () => {
        router.push(getPagePath());
    };

    useEffect(() => { onLoad(); }, []);

    return (
        <Dialog {...props} onClose={onClose}>
            <DialogTitle component="div" sx={{ pt: 3, pb: 2, textAlign: "center", }}>
                <Typography variant="h5" component="h1" gutterBottom>Sign in to account</Typography>
                <Typography variant="body2" gutterBottom>
                    {{
                        credential: 'Enter your credentials'
                    }[provider] || 'Select the method you want to use'}
                </Typography>
                <DialogCloseButton onClose={onClose} />
            </DialogTitle>

            <DialogContent sx={{ px: 4, pb: 0 }}>
                {provider == 'credential' ?
                    <Box component={"form"} onSubmit={preventDefault(() => onSubmit({ ...form.watch(), provider: 'credential' }))}>
                        <Grid container pt={1} pb={4} spacing={3}>
                            <Grid item xs={12}>
                                <Controller
                                    name="username"
                                    control={form.control}
                                    render={({ field }) => <PhoneTextField {...field}
                                        label="Email or Phone number"
                                        variant="standard"
                                        error={!!formState.errors.username}
                                        helperText={formState.errors.username?.message}
                                        fullWidth autoFocus />}
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
                            <Grid item xs={12}>
                                <Box mt={-1}><Typography variant="body2" textAlign="right"><Link {...constructLink({ pathname: '/account/password/reset', query: { returnUrl } })} passHref><MuiLink underline="hover">Forgot password?</MuiLink></Link></Typography></Box>
                            </Grid>
                            <Grid item xs={12}>
                                <Box mb={3}>
                                    <LoadingButton startIcon={<></>} loading={fetcher.state == 'submitting' && provider == 'credential'} loadingPosition="start" type="submit" fullWidth variant="contained" size="large">
                                        Sign In
                                    </LoadingButton>
                                </Box>
                            </Grid>
                        </Grid>
                    </Box> :
                    <>
                        <Stack pt={1} pb={3} spacing={2}>
                            <Button onClick={() => {
                                // Show the form without submitting it. 
                                setProvider('credential');
                            }} disabled={fetcher.state == 'submitting'} variant="outlined" size="large" startIcon={<Icons.AccountCircle />}>Use Email or Phone</Button>

                            <LoadingButton onClick={() => {
                                onSubmit({ ...form.watch(), provider: 'google' });
                            }}
                                startIcon={<Icons.Google />}
                                loading={fetcher.state == 'submitting' && provider == 'google'}
                                loadingPosition="start"
                                type="submit"
                                fullWidth
                                variant="contained"
                                size="large">Continue with Google</LoadingButton>
                        </Stack>
                    </>
                }
                <Typography variant="body2" textAlign="center" pb={4}>Don't have an account? <Link {...constructLink({ pathname: '/account/signup', query: { returnUrl } })} passHref><MuiLink underline="hover">Sign up</MuiLink></Link></Typography>
            </DialogContent>
        </Dialog>
    );
};

export default SignInDialog;