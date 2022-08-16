import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogTitle from '@mui/material/DialogTitle';
import DialogContent from '@mui/material/DialogContent';
import * as Icons from '@mui/icons-material';
import Typography from '@mui/material/Typography';
import { useState } from 'react';
import { Grid, Link, Stack, TextField, Box } from '@mui/material';
import { DialogCloseButton } from '../../components';

const CreateAccountModal = () => {
    const [open, setOpen] = useState(false);
    const [provider, setProvider] = useState(null);

    return (
        <div>
            <Dialog
                aria-labelledby="signup-dialog-title"
                open={true}
                maxWidth="xs"
            >
                <DialogTitle component="div" sx={{ pt: 3, pb: 3, textAlign: "center", }}>
                    <Typography variant="h5" component="h1">Create an account</Typography>
                    <Typography>Already have an account? <Link underline="hover">Sign in</Link></Typography>
                    <DialogCloseButton onClose={handleClose} />
                </DialogTitle>

                <DialogContent sx={{ p: 4 }}>
                    {provider == 'username' ?
                        <>
                            <Grid container pb={3} spacing={2}>
                                <Grid item xs={12} sm={6}>
                                    <TextField
                                        variant="standard"
                                        autoComplete="given-name"
                                        name="firstName"
                                        required
                                        fullWidth
                                        id="firstName"
                                        label="First Name"
                                        autoFocus
                                    />
                                </Grid>
                                <Grid item xs={12} sm={6}>
                                    <TextField
                                        variant="standard"
                                        required
                                        fullWidth
                                        id="lastName"
                                        label="Last Name"
                                        name="lastName"
                                        autoComplete="family-name"
                                    />
                                </Grid>
                                <Grid item xs={12}>
                                    <TextField
                                        variant="standard"
                                        required
                                        fullWidth
                                        id="email"
                                        label="Email Address"
                                        name="email"
                                        autoComplete="email"
                                    />
                                </Grid>
                                <Grid item xs={12}>
                                    <TextField
                                        variant="standard"
                                        required
                                        fullWidth
                                        name="password"
                                        label="Password"
                                        type="password"
                                        id="password"
                                        autoComplete="new-password"
                                    />
                                </Grid>
                                <Grid item xs={12}>
                                    <Box mt={2}><Button fullWidth variant="contained" size="large">Sign Up</Button></Box>
                                </Grid>
                            </Grid>
                        </> :
                        <>
                            <Stack pb={3} spacing={2}>
                                <Button variant="contained" size="large" onClick={() => setProvider("username")}>Sign up with Email or Phone</Button>
                                <Button variant="outlined" size="large" startIcon={<Icons.Google />}>Sign up with Google</Button>
                            </Stack>
                        </>
                    }
                    <Typography textAlign="center" variant="body2">By continuing you agree to our <Link underline="hover">Terms and Conditions</Link></Typography>
                </DialogContent>
            </Dialog>
        </div >
    );
};

export default CreateAccountModal;