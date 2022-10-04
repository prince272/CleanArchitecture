import '../assets/styles/globals.css';
import { ThemeProvider, createTheme, useTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { SnackbarProvider } from 'notistack';
import { findContextualRouteWithComponent, useContextualRouting } from '../views/routes.views';
import App from 'next/app';
import { useRouter } from 'next/router';
import { useEffect, useMemo, useRef, useState } from 'react';
import { ClientProvider, ViewProvider, useView } from '../components';
import { createClient } from '../client';
import PageProgressBar from 'react-top-loading-bar';
import { removeEmptyQueryParams } from '../utils';

const PageRoute = ({ Component, pageProps, ...appProps }) => {
  const router = useRouter();
  const view = useView();
  const theme = useTheme();
  const { constructLink } = useContextualRouting();

  const pageProgressBar = useRef(null);

  useEffect(() => {

    const handleRouteChangeStart = (url, { shallow }) => {
      pageProgressBar.current.continuousStart();
    };

    const handleRouteChangeComplete = (url, { shallow }) => {
      pageProgressBar.current.complete();

      const contextualRoute = findContextualRouteWithComponent(url);
      if (contextualRoute) {
        const Component = contextualRoute.Component;
        view.replace({ Component });
      }
      else {
        view.clear();
      }
    };

    const handleRouteChangeError = (err, url) => {
      pageProgressBar.current.complete();

      if (err.cancelled) {
        console.log(`Route to ${url} was cancelled!`)
      }
    };

    const link = constructLink(router.asPath);
    if (link.as) router.replace(link.href, link.as);

    router.events.on('routeChangeStart', handleRouteChangeStart);
    router.events.on('routeChangeComplete', handleRouteChangeComplete);
    router.events.on('routeChangeError', handleRouteChangeError);

    return () => {
      router.events.off('routeChangeStart', handleRouteChangeStart);
      router.events.off('routeChangeComplete', handleRouteChangeComplete);
      router.events.off('routeChangeError', handleRouteChangeError);
    };

  }, []);

  return (
    <>
      <PageProgressBar color={theme.palette.primary.main} ref={pageProgressBar} />
      <Component {...appProps} {...pageProps} />
    </>
  );
};

const theme = createTheme({
  palette: {
    mode: 'dark',
  },
});


const MyApp = ({ server, ...props }) => {
  const client = useMemo(() => createClient({ server }), []);

  return (
    <ClientProvider client={client}>
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
