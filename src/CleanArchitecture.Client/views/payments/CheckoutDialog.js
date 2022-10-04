import {
    DialogTitle, DialogContent, Grid, Stack, Box, Button,
    Typography, TextField, Link as MuiLink, Dialog,
    Accordion as MuiAccordion,
    AccordionSummary as MuiAccordionSummary,
    AccordionDetails as MuiAccordionDetails, Radio, CircularProgress, Alert
} from '@mui/material';
import { LoadingButton } from '@mui/lab';
import { DialogCloseButton, PhoneTextField, useClient, useView } from '../../components';
import MessageDialog from '../misc/MessageDialog';
import * as Icons from '@mui/icons-material';
import Link from 'next/link';
import Image from 'next/image';
import React, { useEffect, useState, useRef } from 'react';
import { Controller, useForm } from 'react-hook-form';
import { preventDefault, getErrorInfo, isHttpError, sleep } from '../../utils';
import { useSnackbar } from 'notistack';
import { useRouter } from 'next/router';
import { useContextualRouting } from '../routes.views';
import { CLIENT_URL } from '../../client';
import { styled } from '@mui/material/styles';
import { useCallback } from 'react';


const Accordion = styled((props) => {
    const childrenWithProps = React.Children.map(props.children, child => {
        // Checking isValidElement is the safe way and avoids a typescript
        // error too.
        if (React.isValidElement(child)) {
            return React.cloneElement(child, { expanded: +props.expanded });
        }
        return child;
    });
    return (<MuiAccordion disableGutters elevation={0} square {...props}>{childrenWithProps}</MuiAccordion>);
})(({ theme }) => ({
    backgroundColor: 'transparent',
    '&:not(:last-child)': {

    },
    '&:before': {
        display: 'none',
    },
}));

const AccordionSummary = styled(({ expanded, ...props }) => (
    <MuiAccordionSummary expandIcon={<Radio checked={!!expanded} />} {...props} />
))(({ theme, ...props }) => ({
    backgroundColor:
        props.expanded ? theme.palette.mode === 'dark'
            ? 'rgba(255, 255, 255, .05)'
            : 'rgba(0, 0, 0, .03)' : 'transparent',
    flexDirection: 'row-reverse',
    '& .MuiAccordionSummary-content': {
        marginLeft: theme.spacing(1),
    },
}));

const AccordionDetails = styled(MuiAccordionDetails)(({ theme, ...props }) => ({
    backgroundColor:
        props.expanded ? theme.palette.mode === 'dark'
            ? 'rgba(255, 255, 255, .05)'
            : 'rgba(0, 0, 0, .03)' : 'transparent',
}));

const CheckoutDialog = (props) => {
    const view = useView();
    const router = useRouter();
    const client = useClient();

    const form = useForm();
    const formState = form.formState;

    const [payment, setPayment] = useState(null);
    const checkoutId = router.query.checkoutId;
    const returnUrl = router.query.returnUrl || '/';

    const { getPagePath, constructLink } = useContextualRouting();

    const [fetcher, setFetcher] = useState({ state: 'loading' });
    const { enqueueSnackbar } = useSnackbar();

    const paymentMonitorRef = useRef({ active: false });

    const onSubmit = async (inputs) => {

        try {
            setFetcher({ state: 'submitting' });
            let response = await client.post(`/payments/checkout/${checkoutId}`, inputs);
            form.clearErrors();
            setFetcher({ state: 'idle' });

            setPayment(payment => ({ ...payment, status: 'processing' }));
            paymentMonitorRef.current = { active: true };
        }
        catch (error) {
            setFetcher({ state: 'idle', error });
            console.error(error);

            if (isHttpError(error)) {
                const { reason } = error?.response?.data || {};

                form.clearErrors();
                form.handleSubmit(() => {
                    const inputErrors = error?.response?.data?.errors || {};
                    Object.entries(inputErrors).forEach(([name, message]) => form.setError(name, { type: 'server', message: message?.join('\n') }));
                })();
            }

            enqueueSnackbar(getErrorInfo(error).title, { variant: 'error' });
        }
    };

    const onLoad = async () => {

        if (!checkoutId) {
            const link = constructLink(returnUrl);
            router.replace(link.href, link.as);
            return;
        }

        try {
            setFetcher({ state: 'loading' });
            const response = await client.get(`/payments/checkout/${checkoutId}`);
            setPayment(response.data);
            setFetcher({ state: 'idle' });

            if (response.data.status == 'processing') {
                paymentMonitorRef.current = { active: true };
            }

            monitorPayment();
        }
        catch (error) {
            setFetcher(fetcher => ({ ...fetcher, error }));
            console.error(error);
        }
    };

    const onClose = () => {
        router.push(getPagePath());
    };

    const monitorPayment = useCallback(async () => {

        while (paymentMonitorRef.current != null) {

            if (paymentMonitorRef.current.active) {
                try {
                    const response = await client.get(`/payments/checkout/${checkoutId}`);
                    if (response.data.status != 'processing') {
                        paymentMonitorRef.current = { active: false };
                        setPayment(response.data);
                    }
                }
                catch (error) {
                    console.error(error);
                }
            }

            await sleep(1000);
        }

    }, []);

    useEffect(() => { onLoad(); }, []);

    useEffect(() => {


        return () => {
            paymentMonitorRef.current = null;
        };
    }, []);

    const canReload = (fetcher?.error?.response ? (fetcher?.error?.response?.status >= 500 && fetcher?.error?.response?.status <= 599) : true);

    return (
        <>
            {fetcher.state == 'loading' ? <MessageDialog {...props} onClose={() => { }} {...fetcher}
                getErrorTitle={() => 'Failed to Checkout'}
                getErrorDetail={(detail) => detail.replace('resource', 'checkout').replace('Resource', 'Checkout')}

                onCancel={onClose}
                cancelLabel={canReload ? 'Cancel' : 'Ok'}

                onAccept={canReload ? onLoad : undefined}
                acceptLabel={canReload ? 'Try again' : 'Ok'}

            /> :
                <Dialog {...props} onClose={onClose}>
                    <DialogTitle component="div" sx={{ pt: 3, pb: 1, textAlign: "center", }}>
                        <Box>
                            <Typography variant="h5" component="h1" gutterBottom>Payment Method</Typography>
                        </Box>
                        <DialogCloseButton onClose={onClose} />
                    </DialogTitle>
                    <DialogContent sx={{ px: 0, pb: 0 }}>
                        <Box component="form" onSubmit={preventDefault(() => onSubmit({ ...form.watch(), paymentOption: payment.method == 'default' ? payment.gateway : payment.method }))}>


                            {(payment.status == 'pending') && <Box px={2} pb={2}><Alert variant="filled" severity="info">
                                {{
                                    plasticMoney: 'Enter your card details to pay.',
                                    mobileMoney: 'Enter your mobile number to pay.',
                                }[payment.method] || 'Select the payment method you want to use.'}
                            </Alert></Box>}

                            {(payment.status == 'processing') && <Box px={2} pb={2}><Alert variant="filled" severity="info" iconMapping={{ info: <CircularProgress size="1.4rem" color="inherit" /> }}
                                action={<Button color="inherit" size="small" onClick={() => { paymentMonitorRef.current = { active: false }; setPayment(payment => ({ ...payment, status: 'pending' })); }}>Cancel</Button>}>Waiting for payment approval...</Alert></Box>}

                            {(payment.status == 'completed') && <Box px={2} pb={2}><Alert variant="filled" severity="success" onClose={() => { setPayment(payment => ({ ...payment, status: 'pending' })) }}>Your payment was successful.</Alert></Box>}

                            {(payment.status == 'declined' || payment.status == 'expired') && <Box px={2} pb={2}><Alert variant="filled" severity="error" onClose={() => { setPayment(payment => ({ ...payment, status: 'pending' })) }}>Your payment was not successful.</Alert></Box>}

                            <Box mb={3}>
                                <Accordion expanded={payment.method == 'plasticMoney'} onChange={() => setPayment(payment => ({ ...payment, method: 'plasticMoney', gateway: null }))} TransitionProps={{ unmountOnExit: true }} >
                                    <AccordionSummary>
                                        <Typography>Pay with Credit or Debit Card</Typography>
                                    </AccordionSummary>
                                    <AccordionDetails>

                                    </AccordionDetails>
                                </Accordion>

                                <Accordion expanded={payment.method == 'mobileMoney'} onChange={() => setPayment(payment => ({ ...payment, method: 'mobileMoney', gateway: null }))} TransitionProps={{ unmountOnExit: true }}>
                                    <AccordionSummary>
                                        <Typography>Pay with Mobile Money</Typography>
                                    </AccordionSummary>
                                    <AccordionDetails>
                                        <Grid container p={2} spacing={3}>
                                            <Grid item xs={12}>
                                                <Controller
                                                    name="mobileNumber"
                                                    control={form.control}
                                                    render={({ field }) => <PhoneTextField {...field}
                                                        phone={true}
                                                        label="Mobile number"
                                                        variant="standard"
                                                        error={!!formState.errors.mobileNumber}
                                                        helperText={formState.errors.mobileNumber?.message}
                                                        fullWidth
                                                        autoFocus />}
                                                />
                                            </Grid>

                                            <Grid item xs={12}>
                                                <Box>
                                                    <LoadingButton startIcon={<></>} loading={(fetcher.state == 'submitting') && payment.method == 'mobileMoney'} loadingPosition="start" disabled={payment.status == 'processing'} type="submit" fullWidth variant="contained" size="large">
                                                        Pay {payment?.amount}
                                                    </LoadingButton>
                                                </Box>
                                            </Grid>
                                        </Grid>
                                    </AccordionDetails>
                                </Accordion>
                            </Box>

                            <Typography component="div" textAlign="center" variant="overline">WE ACCEPT ALL PAYMENTS THROUGH</Typography>
                            <Box px={3} pb={2}><Box position="relative" height={70}><Image src="/img/payment-footer.png" layout="fill" objectFit="contain" /></Box></Box>
                        </Box>
                    </DialogContent>
                </Dialog>
            }
        </>
    );
};

export default CheckoutDialog;