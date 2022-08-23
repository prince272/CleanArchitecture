import { createContext, useContext, useState } from 'react';
import { Backdrop, Fade, useMediaQuery } from '@mui/material';
import { useTheme } from '@emotion/react';

// MUI - How to open Dialog imperatively/programmatically
// source: https://stackoverflow.com/questions/63737526/mui-how-to-open-dialog-imperatively-programmatically
const DialogContext = createContext();

const DialogContainer = ({ Component, actions, props: { open, disableTransition, ...props } }) => {
    const theme = useTheme();
    const fullScreen = useMediaQuery(theme.breakpoints.down('sm'));

    const TransitionComponent = disableTransition ? Fade : undefined;
    const TransitionProps = disableTransition ? { timeout: 0 } : undefined;

    return (
        <Backdrop open={open} sx={{ color: '#fff', zIndex: (theme) => theme.zIndex.drawer + 1 }}>
            <Component maxWidth="xs" fullScreen={fullScreen} fullWidth={true} hideBackdrop={true} open={open} onClose={actions.close} {...{ ...props, TransitionComponent, TransitionProps: { onExited: actions.dispose, ...TransitionProps } }} />
        </Backdrop>
    );
};

const DialogProvider = ({ children }) => {

    const [dialogs, setDialogs] = useState([]);

    const openDialog = (newDialog) => {
        setDialogs((dialogs) => [...dialogs, { ...newDialog, props: { ...newDialog.props, open: true } }]);
    };

    const closeDialog = () => {
        setDialogs((dialogs) => {
            const currentDialog = dialogs.pop();
            if (!currentDialog) return dialogs;

            const updatedDialogs = [...dialogs].concat({
                ...currentDialog,
                props: {
                    ...currentDialog.props,
                    disableTransition: false,
                    open: false
                }
            });
            return updatedDialogs;
        });
    };

    const replaceDialog = (newDialog) => {
        setDialogs((dialogs) => {
            const currentDialog = dialogs.pop();
            if (!currentDialog) {
                return [...dialogs, { ...newDialog, props: { ...newDialog.props, open: true } }];
            }
            else {

                const updatedDialog = {
                    ...currentDialog,
                    ...newDialog,
                    props: {
                        ...newDialog.props,
                        disableTransition: true,
                        open: true
                    }
                };

                return [...dialogs].concat(updatedDialog);
            }
        });
    };

    const disposeDialog = () => {
        setDialogs((dialogs) => dialogs.slice(0, dialogs.length - 1));
    };

    const actions = { open: openDialog, replace: replaceDialog, close: closeDialog, dispose: disposeDialog };

    return (
        <DialogContext.Provider value={actions}>
            {children}

            {dialogs.map((dialog, i) => {
                return (<DialogContainer key={i} {...{ ...dialog, actions }} />);
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