import { createContext, useContext, useRef, useState } from 'react';
import { Dialog, useMediaQuery, useTheme } from '@mui/material';

// MUI - How to open Dialog imperatively/programmatically
// source: https://stackoverflow.com/questions/63737526/mui-how-to-open-dialog-imperatively-programmatically
const DialogContext = createContext();

const DialogContainer = ({ children, open, onClose, onExited, ...props }) => {

    const theme = useTheme();
    const fullScreen = useMediaQuery(theme.breakpoints.down('sm'));

    return (
        <Dialog maxWidth="xs" fullScreen={fullScreen} {...props} open={open} onClose={onClose} TransitionProps={{ onExited: onExited }}>
            {children}
        </Dialog>
    );
};

const DialogProvider = ({ children }) => {

    const [dialogs, setDialogs] = useState([]);

    const openDialog = (options) => {
        const dialog = { ...options, open: true };
        setDialogs((dialogs) => [...dialogs, dialog]);
    };

    const replaceDialog = (options) => {
        setDialogs((dialogs) => {
            const latestDialog = dialogs.pop();
            if (!latestDialog) {
                const dialog = { ...options, open: true };
                return [...dialogs, dialog];
            }
            else {
                const currentDialog = { ...options, open: true };
                return [...dialogs].concat({ ...latestDialog, ...currentDialog, open: true });
            }
        });
    };

    const closeDialog = () => {
        setDialogs((dialogs) => {
            const latestDialog = dialogs.pop();
            if (!latestDialog) return dialogs;
            return [...dialogs].concat({ ...latestDialog, open: false });
        });
    };

    const disposeDialog = () => {
        setDialogs((dialogs) => dialogs.slice(0, dialogs.length - 1));
    };

    return (
        <DialogContext.Provider value={{ open: openDialog, replace: replaceDialog, close: closeDialog }}>
            {children}
            {dialogs.map((dialogProps, i) => {
                return (
                    <DialogContainer
                        key={i}
                        onClose={closeDialog}
                        onExited={disposeDialog}
                        {...dialogProps}
                    />
                );
            })}
        </DialogContext.Provider>
    )
};

const DialogConsumer = ({ children }) => {
    return (
        <CountContext.Consumer>
            {context => {
                if (context === undefined) {
                    throw new Error('DialogConsumer must be used within a DialogProvider')
                }
                return children(context)
            }}
        </CountContext.Consumer>
    )
};

const useDialog = () => {
    const context = useContext(DialogContext)
    if (context === undefined) {
        throw new Error('useDialog must be used within a DialogProvider')
    }
    return context
};

export { DialogProvider, DialogConsumer, useDialog };