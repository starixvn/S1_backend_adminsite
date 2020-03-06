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

    static DownloadData(param){
        let data = param.Data || {};
        let _rootUrl = window.RootUrl;
        const url = _rootUrl + '/' + param.Url;
        
        axios({
            method: "post",
            params: data,
            url: url,
            responseType: "blob"
        }).then((response) => {
            let blob = new Blob([response.data], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' }),
            downloadUrl = window.URL.createObjectURL(blob),
            filename = "",
            disposition = response.headers["content-disposition"];
            
            if (disposition && disposition.indexOf("attachment") !== -1) {
                let filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/,
                    matches = filenameRegex.exec(disposition);
        
                if (matches != null && matches[1]) {
                    filename = matches[1].replace(/['"]/g, "");
                }
            }
        
            let a = document.createElement("a");
            if (typeof a.download === "undefined") {
                window.location.href = downloadUrl;
            } else {
                a.href = downloadUrl;
                a.download = filename;
                document.body.appendChild(a);
                a.click();
            }
        }).catch(error=>{
            console.log(error);
        })
    }
}