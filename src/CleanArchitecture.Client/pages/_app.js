import '../assets/styles/globals.css';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { SnackbarProvider } from 'notistack';
import { findContextualRouteWithComponent } from '../views/routes.views';
import App from 'next/app';
import { useRouter } from 'next/router';
import { useEffect, useMemo, useRef, useState } from 'react';
import { ClientProvider, ViewProvider, useView, PageProgress } from '../components';
import { createClient } from '../client';

const theme = createTheme({
  palette: {
    mode: 'dark',
  },
});

const PageRoute = ({ Component, pageProps, ...appProps }) => {
  const router = useRouter();
  const view = useView();

  const [progress, setProgress] = useState({
    animating: false,
    key: 0,
  });

  const startProgress = () => {
    setProgress((prevProgress) => ({
      animating: true,
      key: prevProgress.animating ? prevProgress.key : prevProgress.key ^ 1,
    }));
  };

  const endProgress = () => {
    setProgress((prevProgress) => ({
      ...prevProgress,
      animating: false
    }));
  };

  useEffect(() => {

    const handleRouteChangeStart = (url, { shallow }) => {
      startProgress();
    };

    const handleRouteChangeComplete = (url, { shallow }) => {
      endProgress();

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
      endProgress();

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

  return (
    <>
      <PageProgress {...progress}/>
      <Component {...appProps} {...pageProps} />
    </>
  );
};

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
