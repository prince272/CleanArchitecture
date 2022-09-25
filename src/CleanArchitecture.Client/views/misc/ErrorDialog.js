import { Box, Button, CircularProgress, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, Typography } from '@mui/material';
import * as React from 'react';
import { getErrorInfo } from '../../utils';

export default function ErrorDialog({ error, errorTitle, onClose, onRetry, fullScreen, ...props }) {
    const errorInfo = error ? getErrorInfo(error) : null;

    return (
        <>
            {!error ?
                <Box sx={{ display: "flex", alignItems: "center", justifyContent: "center", height: "100%", px: 5, py: 5 }}>
                    <CircularProgress color="inherit" />
                </Box>
                :
                <Dialog {...props} onClose={onClose}>
                    <DialogTitle>{errorTitle}</DialogTitle>
                    <DialogContent>
                        <DialogContentText component="div" variant="body1" style={{ whiteSpace: 'pre-line' }}>
                            {errorInfo.detail}
                        </DialogContentText>
                    </DialogContent>
                    <DialogActions>
                        {errorInfo.canRetry ?
                            <>
                                <Button onClick={onClose}>Cancel</Button>
                                <Button onClick={onRetry} autoFocus>Retry</Button>
                            </>
                            :
                            <>
                                <Button onClick={onClose} autoFocus>Close</Button>
                            </>
                        }

                    </DialogActions>
                </Dialog>
            }
        </>
    );
}