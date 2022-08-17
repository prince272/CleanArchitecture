import { DialogTitle, DialogContent, DialogActions, Button } from '@mui/material';
import Link from 'next/link';
import React from 'react';
import { useContextualRouting } from '../../utils/hooks';

const CreateAccountDialog = ({ closeDialog }) => {
    const { constructContextualPath } = useContextualRouting();

    return (
        <div>
            <>
                <DialogTitle>CreateAccountDialog</DialogTitle>
                <DialogContent>
                (of an actor) originate (a role) by playing a character for the first time:
                </DialogContent>
                <DialogActions>
                    <Button color="primary" onClick={() => closeDialog()}>
                        Close
                    </Button>
                    <Link {...constructContextualPath('account/confirm')}>
                        <Button color="primary" component="a">
                            Confirm
                        </Button>
                    </Link>
                </DialogActions>
            </>
        </div>
    );
};

export default CreateAccountDialog;