import { useRouter } from 'next/router';
import '../assets/styles/globals.css';

const MyApp = ({ Component, pageProps }) => {

  const router = useRouter();

  useEffect(() => {

    const handleStart = (url, { shallow }) => {
      console.log(url);
    };

    const handleComplete = () => {

    };

    const handleError = (error, url) => {
      if (error.cancelled) {
        console.log(`Route to ${url} was cancelled!`)
      }
    };

    router.events.on('routeChangeStart', handleStart);
    router.events.on('routeChangeComplete', handleComplete);
    router.events.on('routeChangeError', handleError);

    return () => {
      router.events.off('routeChangeStart', handleStart);
      router.events.off('routeChangeComplete', handleComplete);
      router.events.off('routeChangeError', handleError);
    };

  }, [router]);

  return <Component {...pageProps} />
};

export default MyApp
