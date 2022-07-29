import logo from './logo.svg';
import './App.css';
import { PopupWindow } from './jwt';
import { useEffect } from 'react';
import UrlUtility from './jwt/UrlUtility';


function App() {

  useEffect(() => {
    PopupWindow.notify();
  }, []);

  return (
    <div className="App">
      <header className="App-header">
        <input type="button" value="button name" onClick={() => window.open('http://localhost:9000/')} />
        <img src={logo} className="App-logo" alt="logo" />
        <p>
          Edit <code>src/App.js</code> and save to reload.
        </p>
        <a
          className="App-link"
          onClick={async () => {
            try {
              const popupWindow = new PopupWindow();
              let url = UrlUtility.addQueryParam('http://localhost:3000', 'state', popupWindow.state);
              url = UrlUtility.addQueryParam('https://localhost:7194/account/google/connect', 'returnUrl', url);
              await popupWindow.navigate(url);
              alert('Ended with a success!');
            }
            catch (ex) {
              alert('Ended with an error!');
              throw ex;
            }
          }}
          target="_blank"
          rel="noopener noreferrer"
        >
          Learn React
        </a>
      </header>
    </div>
  );
}

export default App;
