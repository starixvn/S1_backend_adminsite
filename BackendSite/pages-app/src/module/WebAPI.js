import axios from 'axios';

export default class WidgetApi {
    static GetWebAPI(actionUrl){
        let lapRootUrl = window.RootUrl;
        const lapApi = lapRootUrl + '/' + actionUrl
        return lapApi
    }

    static fetchData(param) {
        let data = param.Data || {};
        let lapRootUrl = window.RootUrl;
        const lapApi = lapRootUrl + '/' + param.Url
        return axios.get(lapApi, {
            params: data
        });
    }

    static PostData(param) {
        let data = param.Data || {};
        let _rootUrl = window.RootUrl;
        const url = _rootUrl + '/' + param.Url;
        return axios.post(url, data);
    }

    static MultiPostData(params){
        return axios.all(params.map(param=>{
            return WidgetApi.PostData(param);
        }));
    }
}