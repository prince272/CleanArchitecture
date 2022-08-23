import '../assets/styles/globals.css';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { DialogProvider, useDialog } from '../utils/dialog';
import { SnackbarProvider } from 'notistack';
import { findContextualRoute, useContextualRouting } from '../dialogs';
import { useRouter } from 'next/router';
import { useEffect } from 'react';

const theme = createTheme({
  palette: {
    mode: 'dark',
  },
});

const PageRoute = ({ Component, pageProps }) => {
  const router = useRouter();
  const dialog = useDialog();


  useEffect(() => {

    const handleRouteChangeStart = (url, { shallow }) => {
      const contextualRoute = findContextualRoute(router.asPath);

      if (contextualRoute) {
        const Component = contextualRoute.Component;
        dialog.replace({ Component });
      }
      else {
        dialog.close();
      }
    };

    const handleRouteChangeComplete = (url, { shallow }) => {

    };

    const handleRouteChangeError = (err, url) => {
      if (err.cancelled) {
        console.log(`Route to ${url} was cancelled!`)
      }
    };

    router.events.on('routeChangeStart', handleRouteChangeStart);
    router.events.on('routeChangeComplete', handleRouteChangeComplete);
    router.events.on('routeChangeError', handleRouteChangeError);

    return () => {
      router.events.off('routeChangeStart', handleRouteChangeStart);
      router.events.off('routeChangeComplete', handleRouteChangeComplete);
      router.events.off('routeChangeError', handleRouteChangeError);
    }
  }, []);

  useEffect(() => {

    const contextualRoute = findContextualRoute(router.asPath);

    if (contextualRoute) {
      const Component = contextualRoute.Component;
      dialog.replace({ Component });
    }
    else {
      dialog.close();
    }

  }, [router.asPath]);

  return (<Component {...pageProps} />);
};

function MyApp(props) {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <SnackbarProvider maxSnack={3} preventDuplicate anchorOrigin={{ vertical: 'top', horizontal: 'center' }} >
        <DialogProvider>
          <PageRoute {...props} />
        </DialogProvider>
      </SnackbarProvider>
    </ThemeProvider>
  );
}

export default MyApp;
