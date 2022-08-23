import { DialogTitle, DialogContent, Grid, Stack, Box, Button, Typography, Link as MuiLink, Dialog, TextField } from '@mui/material';
import { LoadingButton } from '@mui/lab';
import { DialogCloseButton } from '../../components';
import * as Icons from '@mui/icons-material';
import PhoneInput from '../../components/PhoneInput';
import Link from 'next/link';
import { useEffect, useMemo, useState } from 'react';
import { useTimer } from 'use-timer';
import { Controller, useForm } from 'react-hook-form';
import client from '../../client';
import { preventDefault, formatError, isPhoneFormat, isHttpError } from '../../utils';
import { useSnackbar } from 'notistack';
import { useRouter } from 'next/router';
import { useContextualRouting } from '..';
import * as casing from 'change-case';

const VerifyAccountDialog = (props) => {
    const router = useRouter();

    const [sent, setSent] = useState(false);
    const sendingTimer = useTimer({ initialTime: 60, endTime: 0, timerType: 'DECREMENTAL' });

    const returnUrl = router.query.returnUrl;
    if (!returnUrl) throw new Error('\'returnUrl\' query value was not supplied');

    const initialState = JSON.parse(router.query.state || null);
    if (!initialState) throw new Error('\'state\' query value was not supplied');

    const { getPagePath, constructLink } = useContextualRouting();
    const form = useForm();
    const formState = form.formState;
    const [fetcher, setFetcher] = useState({ state: 'idle', data: null });
    const { enqueueSnackbar } = useSnackbar();

    const accountType = isPhoneFormat(initialState.inputs.username) ? 'PhoneNumber' : 'EmailAddress';
    const messageType = isPhoneFormat(initialState.inputs.username) ? 'SMS' : 'Email';

    const onSubmit = async (inputs, resend = false) => {
        try {
            if (!sent || resend) {
                setFetcher(fetcher => ({ ...fetcher, state: 'sending' }));
                sendingTimer.start();

                let response = await client.post('account/verify/send', inputs);
                form.clearErrors();

                setSent(true);
                enqueueSnackbar(`${casing.pascalCase(messageType)} sent!`, { variant: 'success' });
            }
            else {
                setFetcher(fetcher => ({ ...fetcher, state: 'verifying' }));

                let response = await client.post('account/verify', inputs);
                form.clearErrors();

                const link = constructLink(returnUrl, { state: JSON.stringify(initialState) });
                router.push(link.href, link.as);
            }
        }
        catch (error) {
            console.error(error);

            if (isHttpError(error)) {

                form.clearErrors();
                form.handleSubmit(() => {
                    const inputErrors = error?.response?.data?.errors || {};
                    Object.entries(inputErrors).forEach(([name, message]) => form.setError(name, { type: 'server', message: message?.join('\n') }));
                })();
            }

            enqueueSnackbar(formatError(error), { variant: 'error' });
        }
        finally {
            setFetcher(fetcher => ({ ...fetcher, state: 'idle' }));
        }
    };

    const onload = () => {
        form.reset({
            username: initialState.inputs.username
        });
    };

    const onClose = () => {
        router.push(getPagePath());
    };

    useEffect(() => { onload(); }, []);

    return (
        <Dialog {...props} onClose={onClose}>
            <DialogTitle component="div" sx={{ pt: 3, pb: 2, textAlign: "center" }}>
                <Typography variant="h5" component="h1" gutterBottom>Verify the {casing.noCase(accountType)}</Typography>
                <Typography variant="body2" gutterBottom>
                    {!sent ? `We\'d send a security code to this ${casing.noCase(accountType)}`
                        : `We\'ve sent a security code to this ${casing.noCase(accountType)}`}
                </Typography>
                <DialogCloseButton onClose={onClose} />
            </DialogTitle>

            <DialogContent sx={{ px: 4, pb: 0 }}>

                <Box component={"form"} onSubmit={preventDefault(() => onSubmit(form.watch()))}>
                    <Grid container pt={1} pb={4} spacing={4}>
                        <Grid item xs={12}>
                            <Controller
                                name="username"
                                control={form.control}
                                render={({ field }) => <PhoneInput {...field}
                                    label="Email or Phone number"
                                    variant="standard"
                                    error={!!formState.errors.username}
                                    helperText={formState.errors.username?.message}
                                    fullWidth
                                    focused={false}
                                    InputProps={{
                                        readOnly: true,
                                    }} />}
                            />

                        </Grid>
                        {sent &&
                            <Grid item xs={12}>
                                <Controller
                                    name="code"
                                    defaultValue=""
                                    control={form.control}
                                    render={({ field }) => <TextField {...field}
                                        label="Enter security code"
                                        variant="standard"
                                        error={!!formState.errors.code}
                                        helperText={formState.errors.code?.message}
                                        autoFocus
                                        fullWidth focused />}
                                />
                            </Grid>
                        }
                    </Grid>

                    <Box mb={3}>
                        <LoadingButton startIcon={<></>} loading={!sent && fetcher.state == 'sending' || fetcher.state == 'verifying'} loadingPosition="start" type="submit" fullWidth variant="contained" size="large">
                            {!sent ? 'Send code' : `Verify ${casing.noCase(accountType)}`}
                        </LoadingButton>
                    </Box>
                </Box>

                {sent &&
                    <Typography variant="body2" textAlign="center" pb={4}>
                        {sendingTimer.status == 'RUNNING' ?
                            (<>Resending {casing.noCase(messageType)} in {sendingTimer.time} seconds...</>) :
                            (<> Didn't receive {messageType}? <MuiLink href="" underline="hover" onClick={preventDefault(() => onSubmit(form.watch(), true))}>Resend</MuiLink></>)}
                    </Typography>
                }
            </DialogContent>
        </Dialog>
    );
};

export default VerifyAccountDialog;