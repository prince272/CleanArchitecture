import { Box, Button, CircularProgress, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, Typography } from '@mui/material';
import * as React from 'react';
import { getErrorInfo } from '../../utils';

export default function ErrorDialog({ getErrorTitle, getErrorDetail, onClose, onRetry, fullScreen, ...props }) {
    const errorInfo = props.error ? getErrorInfo(props.error) : null;

    return (
        <>
            {!errorInfo ?
                <Box sx={{ display: "flex", alignItems: "center", justifyContent: "center", height: "100%", px: 5, py: 5 }}>
                    <CircularProgress color="inherit" />
                </Box>
                :
                <Dialog {...props} fullWidth={true} maxWidth="xs" onClose={onClose}>
                    <DialogTitle>{getErrorTitle ? getErrorTitle(errorInfo.title) : errorInfo.title}</DialogTitle>
                    <DialogContent>
                        <DialogContentText component="div" style={{ whiteSpace: 'pre-line' }}>
                            {getErrorDetail ? getErrorDetail(errorInfo.detail) : errorInfo.detail}
                        </DialogContentText>
                    </DialogContent>
                    <DialogActions>
                        {errorInfo.canRetry ?
                            <>
                                <Button onClick={onClose}>Close</Button>
                                <Button onClick={onRetry} autoFocus>Try again</Button>
                            </>
                            :
                            <>
                                <Button onClick={onClose} autoFocus>Ok</Button>
                            </>
                        }

                    </DialogActions>
                </Dialog>
            }
        </>
    );
}