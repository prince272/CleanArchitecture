import '../assets/styles/globals.css';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { DialogProvider } from '../utils/dialog';
import { DialogRoute } from '../dialogs';
import { SnackbarProvider } from 'notistack';

const theme = createTheme({
  palette: {
    mode: 'dark',
  },
});

function MyApp({ Component, pageProps }) {

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <SnackbarProvider maxSnack={3} preventDuplicate anchorOrigin={{ vertical: 'top', horizontal: 'center' }} >
        <DialogProvider>
          <Component {...pageProps} />
          <DialogRoute />
        </DialogProvider>
      </SnackbarProvider>
    </ThemeProvider>
  );
}

export default MyApp;
