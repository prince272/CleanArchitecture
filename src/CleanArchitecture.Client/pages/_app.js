import '../assets/styles/globals.css';
import { DialogProvider } from '../utils/dialog';
import { DialogRoute } from '../dialogs';
import { SnackbarProvider } from 'notistack';

function MyApp({ Component, pageProps }) {

  return (
    <>
      <SnackbarProvider maxSnack={3} preventDuplicate anchorOrigin={{ vertical: 'top', horizontal: 'center' }} >
        <DialogProvider>
          <Component {...pageProps} />
          <DialogRoute />
        </DialogProvider>
      </SnackbarProvider>
    </>
  );
}

export default MyApp;
