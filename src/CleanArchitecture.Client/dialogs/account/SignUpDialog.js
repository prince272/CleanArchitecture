import { DialogTitle, DialogContent, Grid, Stack, Box, Button, Typography, TextField, Link as MuiLink } from '@mui/material';
import { LoadingButton } from '@mui/lab';
import { DialogCloseButton } from '../../components';
import * as Icons from '@mui/icons-material';
import Link from 'next/link';
import React, { useEffect, useState } from 'react';
import { useContextualRouting } from '../../utils/hooks';
import { Controller, useForm } from 'react-hook-form';
import client from '../../client';
import { preventDefault, formatError } from '../../utils';
import { useSnackbar } from 'notistack';

const SignUpDialog = ({ closeDialog }) => {
    const { constructContextualPath } = useContextualRouting();
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
        }
        catch (unsafeError) {
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

    useEffect(() => onload(), []);

    return (
        <>
            <DialogTitle component="div" sx={{ pt: 3, pb: 3, textAlign: "center", }}>
                <Typography variant="h5" component="h1" gutterBottom>Create an account</Typography>
                <Typography>Already have an account? <Link {...constructContextualPath('account/signin')} passHref><MuiLink underline="hover">Sign in</MuiLink></Link></Typography>
                <DialogCloseButton onClose={closeDialog} />
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
                                    render={({ field }) => <TextField {...field}
                                        label="Email"
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
                                    render={({ field }) => <TextField {...field}
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
                        <Stack pb={3} spacing={2}>
                            <Button variant="contained" size="large" onClick={() => setProvider("username")}>Sign up with Email or Phone</Button>
                            <Button variant="outlined" size="large" startIcon={<Icons.Google />}>Sign up with Google</Button>
                        </Stack>
                    </>
                }
                <Typography textAlign="center" variant="body2" pb={4}>By signing up, you agree to our <MuiLink underline="hover">Terms and Conditions</MuiLink></Typography>
            </DialogContent>
        </>
    );
};

export default SignUpDialog;