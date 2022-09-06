import '../assets/styles/globals.css';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { SnackbarProvider } from 'notistack';
import { findContextualRouteWithComponent } from '../views/routes.views';
import App from 'next/app';
import { useRouter } from 'next/router';
import { useEffect, useRef } from 'react';
import { ClientProvider, ViewProvider, useView } from '../components';
import { createClient } from '../client';

const theme = createTheme({
  palette: {
    mode: 'dark',
  },
});

const PageRoute = ({ Component, pageProps, ...appProps }) => {
  const router = useRouter();
  const view = useView();

  useEffect(() => {

    const handleRouteChangeStart = (url, { shallow }) => {
    };

    const handleRouteChangeComplete = (url, { shallow }) => {
      const contextualRoute = findContextualRouteWithComponent(url);
      if (contextualRoute) {
        const Component = contextualRoute.Component;
        view.replace({ Component });
      }
      else {
        view.close();
      }
    };

    const handleRouteChangeError = (err, url) => {
      if (err.cancelled) {
        console.log(`Route to ${url} was cancelled!`)
      }
    };

    handleRouteChangeComplete(router.asPath, {});
    router.events.on('routeChangeStart', handleRouteChangeStart);
    router.events.on('routeChangeComplete', handleRouteChangeComplete);
    router.events.on('routeChangeError', handleRouteChangeError);

    return () => {
      router.events.off('routeChangeStart', handleRouteChangeStart);
      router.events.off('routeChangeComplete', handleRouteChangeComplete);
      router.events.off('routeChangeError', handleRouteChangeError);
    };
    
  }, []);

  return (<Component {...appProps} {...pageProps} />);
};

const MyApp = ({ server, ...props }) => {
  const clientRef = useRef(createClient({ server }));

  return (
    <ClientProvider client={clientRef.current}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <SnackbarProvider maxSnack={3} preventDuplicate anchorOrigin={{ vertical: 'top', horizontal: 'center' }} >
          <ViewProvider>
            <PageRoute {...props} />
          </ViewProvider>
        </SnackbarProvider>
      </ThemeProvider>
    </ClientProvider>
  );
};

MyApp.getInitialProps = async (appContext) => {
  // calls page's `getInitialProps` and fills `appProps.pageProps`
  const appProps = await App.getInitialProps(appContext);

  const server = typeof window === 'undefined' ? {
    req: {
      cookies: appContext?.ctx?.req?.cookies,
      headers: appContext?.ctx?.req?.headers
    }
  } : undefined;

  return { ...appProps, server };
}

export default MyApp;
