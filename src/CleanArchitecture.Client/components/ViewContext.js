import { createContext, useContext, useState } from 'react';
import { Backdrop, Fade, useMediaQuery } from '@mui/material';
import { useTheme } from '@emotion/react';

// MUI - How to open View imperatively/programmatically
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

    const [views, setViews] = useState([]);

    const openView = (newView) => {
        setViews((views) => [...views, { ...newView, props: { ...newView.props, open: true } }]);
    };

    const closeView = () => {
        setViews((views) => {
            const currentView = views.pop();
            if (!currentView) return views;

            const updatedViews = [...views].concat({
                ...currentView,
                props: {
                    ...currentView.props,
                    disableTransition: false,
                    open: false
                }
            });
            return updatedViews;
        });
    };

    const replaceView = (newView) => {
        setViews((views) => {
            const currentView = views.pop();
            if (!currentView) {
                return [...views, { ...newView, props: { ...newView.props, open: true } }];
            }
            else {

                const updatedView = {
                    ...currentView,
                    ...newView,
                    props: {
                        ...newView.props,
                        disableTransition: true,
                        open: true
                    }
                };

                return [...views].concat(updatedView);
            }
        });
    };

    const disposeView = () => {
        setViews((views) => views.slice(0, views.length - 1));
    };

    const actions = { open: openView, replace: replaceView, close: closeView, dispose: disposeView };

    return (
        <ViewContext.Provider value={actions}>
            {children}

            {views.map((view, i) => {
                return (<ViewContainer key={i} {...{ ...view, actions }} />);
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