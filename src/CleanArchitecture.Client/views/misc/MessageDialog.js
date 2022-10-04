import { Box, Button, CircularProgress, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, Typography } from '@mui/material';
import * as React from 'react';
import { getErrorInfo } from '../../utils';

export default function ErrorDialog({ getErrorTitle, getErrorDetail, onClose, onCancel, cancelLabel = 'Cancel', onAccept, acceptLabel = 'Ok', fullScreen, ...props }) {
    const info = props.error ? getErrorInfo(props.error) : props.info;

    return (
        <>
            <Dialog {...props} fullWidth={true} maxWidth="xs" onClose={() => { onCancel(); onClose(); }}>
                {props.state == 'loading' && !info ?
                    <DialogContent>
                        <Box sx={{ display: "flex", alignItems: "center", justifyContent: "center", height: "100%", px: 1, py: 1 }}>
                            <CircularProgress color="primary" />
                        </Box>
                    </DialogContent>
                    :
                    <>
                        <DialogTitle>{getErrorTitle ? getErrorTitle(info.title) : info.title}</DialogTitle>
                        <DialogContent>
                            <Typography variant="body1" style={{ whiteSpace: 'pre-line' }}>
                                {getErrorDetail ? getErrorDetail(info.detail) : info.detail}
                            </Typography>
                        </DialogContent>
                        <DialogActions>
                            {onCancel && <Button onClick={() => { onCancel(); onClose(); }}>{cancelLabel}</Button>}
                            {onAccept && <Button onClick={() => { onAccept(); onClose(); }} autoFocus>{acceptLabel}</Button>}
                        </DialogActions>
                    </>
                }
            </Dialog>

        </>
    );
}