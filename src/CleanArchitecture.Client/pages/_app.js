import '../assets/styles/globals.css';
import { DialogProvider } from '../utils/dialog';
import { DialogRoute } from '../dialogs';

function MyApp({ Component, pageProps }) {

  return (
    <DialogProvider>
      <Component {...pageProps} />
      <DialogRoute />
    </DialogProvider>
  );
}

export default MyApp;
