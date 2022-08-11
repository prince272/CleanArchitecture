import axios from "axios";
import { useMemo } from "react";
import Home from "./pages/Home";
import httpClient from "./utils/httpClient";

const App = () => {


  return (
    <div style={{ display: "flex", justifyItems: "center", alignItems: "center" }}>
      <button type="button" onClick={async () => {
        httpClient.auth.signin({ username: 'princeowusu.272@gmail.com', password: 'Owusu#15799' });
      }}>Signin</button>
      <Home></Home>
    </div>
  );
}

export default App;
