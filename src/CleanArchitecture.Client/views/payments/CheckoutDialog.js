import {
    DialogTitle, DialogContent, Grid, Stack, Box, Button,
    Typography, TextField, Link as MuiLink, Dialog,
    Accordion as MuiAccordion,
    AccordionSummary as MuiAccordionSummary,
    AccordionDetails as MuiAccordionDetails, Radio, CircularProgress
} from '@mui/material';
import { LoadingButton } from '@mui/lab';
import { DialogCloseButton, PasswordField, PhoneTextField, useClient } from '../../components';
import ErrorDialog from '../misc/ErrorDialog';
import * as Icons from '@mui/icons-material';
import Link from 'next/link';
import React, { useEffect, useState, useMemo } from 'react';
import { Controller, useForm } from 'react-hook-form';
import { preventDefault, getErrorInfo, isHttpError, getPath } from '../../utils';
import { useSnackbar } from 'notistack';
import { useRouter } from 'next/router';
import { useContextualRouting } from '../routes.views';
import { CLIENT_URL } from '../../client';
import { styled } from '@mui/material/styles';

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
    const router = useRouter();
    const client = useClient();

    const form = useForm();
    const formState = form.formState;

    const [payment, setPayment] = useState(null);
    const paymentId = router.query.paymentId;
    const accessCode = router.query.accessCode;
    const returnUrl = router.query.returnUrl || '/';

    const { getPagePath, constructLink } = useContextualRouting();
    const [paymentMethod, setPaymentMethod] = useState(null);

    const [fetcher, setFetcher] = useState({ state: 'idle' });
    const { enqueueSnackbar } = useSnackbar();

    const onSubmit = async (inputs) => {

        try {
            setFetcher({ state: 'processing' });

            let response = await client.post('/payments/checkout', inputs);
            form.clearErrors();
        }
        catch (error) {
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
        finally {
            setFetcher({ state: 'idle' });
        }
    };

    const onLoad = async () => {

        if (!paymentId || !accessCode) {
            const link = constructLink(returnUrl);
            router.replace(link.href, link.as);
            return;
        }

        try {
            setFetcher({ state: 'loading' });
            const response = await client.get(`/payments/${paymentId}`, { params: { accessCode } });
            setPayment(response.data);
            setFetcher({ state: 'idle' });
        }
        catch (error) {
            console.error(error);
            setFetcher(fetcher => ({ ...fetcher, error }));
        }
    };

    const onClose = () => {
        router.push(getPagePath());
    };

    useEffect(() => { onLoad(); }, []);

    return (
        <>
            {fetcher.state == 'loading' ? <ErrorDialog {...props} {...fetcher} errorTitle="Unable to Checkout" onClose={onClose} onRetry={onLoad} /> :
                <Dialog {...props} onClose={onClose}>
                    <DialogTitle component="div" sx={{ pt: 3, pb: 2, textAlign: "center", }}>
                        <Typography variant="h5" component="h1" gutterBottom>Payment Method</Typography>
                        <Typography variant="body2" gutterBottom>
                            {{
                                mobileMoney: 'Enter your mobile number to pay'
                            }[paymentMethod] || 'Select the payment method you want to use'}
                        </Typography>
                        <DialogCloseButton onClose={onClose} />
                    </DialogTitle>
                    <DialogContent sx={{ px: 0, pb: 0 }}>
                        <Box component="form" sx={{ pb: 3 }} onSubmit={preventDefault(() => onSubmit({ ...form.watch(), method: paymentMethod }))}>
                            <Accordion expanded={paymentMethod == 'plasticMoney'} onChange={() => { setPaymentMethod('plasticMoney'); }} TransitionProps={{ unmountOnExit: true }} >
                                <AccordionSummary>
                                    <Typography>Pay with Credit or Debit Card</Typography>
                                </AccordionSummary>
                                <AccordionDetails>

                                </AccordionDetails>
                            </Accordion>
                            <Accordion expanded={paymentMethod == 'mobileMoney'} onChange={() => { setPaymentMethod('mobileMoney'); }} TransitionProps={{ unmountOnExit: true }}>
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
                                                <LoadingButton startIcon={<></>} loading={fetcher.state == 'submitting' && paymentMethod == 'credential'} loadingPosition="start" type="submit" fullWidth variant="contained" size="large">
                                                    Pay {payment?.amount}
                                                </LoadingButton>
                                            </Box>
                                        </Grid>
                                    </Grid>
                                </AccordionDetails>
                            </Accordion>
                        </Box>
                        <Typography variant="body2" textAlign="center" pb={4}>Don't have an account? <Link {...constructLink({ pathname: '/account/signup', query: { returnUrl } })} passHref><MuiLink underline="hover">Sign up</MuiLink></Link></Typography>
                    </DialogContent>
                </Dialog>
            }
        </>
    );
};

export default CheckoutDialog;