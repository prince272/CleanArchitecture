import Axios from "axios";
import { withAxiosAuth } from "./axiosAuth";
import QueryString from "qs";

let httpClient = withAxiosAuth(Axios.create({
    baseURL: process.env.REACT_APP_SERVER_URL,
    paramsSerializer: params => {
        return QueryString.stringify(params)
    },
}), 
{
    generateTokenCallback: (request, data, requestConfig) => request.post('account/token/generate', data, requestConfig),
    refreshTokenCallback: (request, data, requestConfig) => request.post('account/token/refresh', data, requestConfig),
    revokeTokenCallback: (request, data, requestConfig) => request.post('account/token/revoke', data, requestConfig)
});

export default httpClient;