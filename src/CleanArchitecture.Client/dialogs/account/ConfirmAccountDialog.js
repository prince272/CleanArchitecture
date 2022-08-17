import { DialogTitle, DialogContent, DialogActions, Button } from '@mui/material';
import React from 'react';

const ConfirmAccountDialog = ({ closeDialog }) => {
    return (
        <div>
            <>
                <DialogTitle>ConfirmAccountDialog</DialogTitle>
                <DialogContent>
                establish the truth or correctness of (something previously believed or suspected to be the case):
                </DialogContent>
                <DialogActions>
                    <Button color="primary" onClick={() => closeDialog()}>
                        Close
                    </Button>
                </DialogActions>
            </>
        </div>
    );
};

export default ConfirmAccountDialog;