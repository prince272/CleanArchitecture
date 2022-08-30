import { createContext, useContext, useState } from 'react';
import { Backdrop, Fade, useMediaQuery } from '@mui/material';
import { useTheme } from '@emotion/react';

// MUI - How to open Dialog imperatively/programmatically
// source: https://stackoverflow.com/questions/63737526/mui-how-to-open-dialog-imperatively-programmatically
const ViewContext = createContext();

const ViewContainer = ({ Component, actions, props: { open, disableTransition, ...props } }) => {
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

const ViewProvider = ({ children }) => {

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
        <ViewContext.Provider value={actions}>
            {children}

            {dialogs.map((dialog, i) => {
                return (<ViewContainer key={i} {...{ ...dialog, actions }} />);
            })}

        </ViewContext.Provider>
    )
};

const ViewConsumer = ({ children }) => {
    return (
        <ViewContext.Consumer>
            {context => {
                if (context === undefined) {
                    throw new Error('ViewConsumer must be used within a ViewProvider')
                }
                return children(context)
            }}
        </ViewContext.Consumer>
    )
};

const useView = () => {
    const context = useContext(ViewContext)
    if (context === undefined) {
        throw new Error('useView must be used within a ViewProvider')
    }
    return context
};

export default ViewContext;
export { ViewProvider, ViewConsumer, useView };