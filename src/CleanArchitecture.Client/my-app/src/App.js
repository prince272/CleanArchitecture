import logo from './logo.svg';
import './App.css';
import { useEffect, useMemo, useState } from 'react';
import { createHttpClient } from './utils/http-client';

const httpClient = createHttpClient({
  baseURL: 'https://localhost:7194',

  signinCallback: (request, data, requestConfig) => {
    if (data.provider) return request.post(`account/${data.provider}/token/generate`, data, requestConfig);
    return request.post(`account/token/generate`, data, requestConfig);
  },

  refreshCallback: (request, data, requestConfig) => {
    return request.post('account/token/refresh', data, requestConfig);
  },

  signoutCallback: (request, data, requestConfig) => {
    return request.post('account/token/revoke', data, requestConfig);
  },

  userCallback: (request, requestConfig) => {
    return request.get('account/profile', requestConfig);
  }
});

function App() {
  const [profile, setProfile] = useState(null);

  useEffect(() => {
    httpClient.auth.userSubject.subscribe((value) => {
      setProfile(value);
    });
  })

  return (
    <div className="App">
      <header className="App-header">
        <input type="button" value="button name" onClick={() => window.open('http://localhost:9000/')} />
        <img src={logo} className="App-logo" alt="logo" />
        <p>
          {profile ? JSON.stringify(profile) : <>No profile</>}
        </p>
        <button
          className="App-link"
          onClick={async () => {
            console.log("Loging in");
            await httpClient.signin({ username: 'princeowusu.272@gmail.com', password: 'Owusu#15799' });
            console.log("Login complete");
          }}
        >
          Login to Account
        </button>
        <button
          className="App-link"
          onClick={async () => {
            httpClient.auth.removeAccessToken();
          }}
        >
          Clear authentication
        </button>
        <button
          className="App-link"
          onClick={async () => {
            console.log((await httpClient.get('authorize')).data);
          }}
        >
          Access a protected resource
        </button>
        <button
          className="App-link"
          onClick={async () => {
            await httpClient.signout();
          }}
        >
          Log out
        </button>
      </header>
    </div>
  );
}

export default App;
