import React, { useState, useEffect } from "react";
import ReactDOM from 'react-dom';
import { api } from '../../../module/WebAPI';

const initData = {
    optSite: null,
    file: ''
}

const App = () => {
    let _site = [];
    const [site, setSite] = useState([]);
    const [stateSend, updateSend] = useState(false);
    const [stateData, updateData] = useState({ ...initData });
    const [sendData, updateSendData] = useState([]);
    const EditFiled = (v, datacolumn) => { updateData({ ...stateData, [datacolumn]: v }); };
    const SetMsg = () => {
        let msg = "";
        let check = true;
        if (!stateData.file) { check = false; msg = "Please choose excel file."; }
        if (!stateData.optSite) { check = false; msg = "Please check Site."; }
        if (check && window.confirm("Are you sure to import data to " + _site[stateData.optSite] + "?")) {
            let settings = { headers: { 'content-type': 'multipart/form-data' } }
            let file = document.getElementById("uploadfile").files[0];
            let formData = new FormData();
            formData.append("excelUpload", file, file.name);
            formData.append('optSite', parseInt(stateData.optSite));
            if (!stateSend) {
                updateSend(true);
                api.post('/api/Message/ChangeVipLevelBatch', formData, settings)
                    .then(function (response) {
                        updateSend(false);
                        if (response.data) {
                            updateSendData(response.data);
                            alert('sent successfully');
                        } else {
                            alert("sent error");
                        }
                    }).catch(function (error) {
                        updateSend(false);
                        alert(error);
                    })

                updateData({ ...initData });
            }
        } else {
            alert(msg);
        }
    }

    let radio = "";
    useEffect(() => {
        api.post('/api/Common/AdminUserSiteList')
            .then(function (response) {
                if (response.data.errorCode === 0) {
                    setSite(response.data === null ? [] : response.data);
                } else {
                    // alert(response.message);
                }
            }).catch(function (error) {
                alert(error);
            })
    }, []);

    if (site.length === 0) {
        return <></>
    } else {
        radio = site.data.map((value) => {
            _site[value.siteId] = value.siteName;
            return (
                <div key={`SiteOpt-${value.siteId}`} className="custom-control custom-radio custom-control-inline">
                    <input type="radio" id={`SiteId_${value.siteId}`} name="SiteList" className="custom-control-input" value={value.siteId} onChange={(e) => { EditFiled(value.siteId, 'optSite') }} disabled={stateSend} checked={stateData.optSite === value.siteId} />
                    <label className="custom-control-label" htmlFor={`SiteId_${value.siteId}`}>{value.siteName}</label>
                </div>
            )
        });
    }
    let result = sendData.map((value, key) => { return <p key={`result-${key}`}>{value.message}</p> })

    return (
        <div className="container-fluid">
            <h4 className="page-name">Change Vip Level Batch</h4>
            <div className="form-group">
                <label className="col-form-label d-block">Site Name</label>
                {radio}
            </div>
            <div className="form-group">
                <label className="col-form-label d-block">Download Template File </label>
                <a className="btn btn-danger" role="button" href="../../../resource/examplefile/ChangeVipLevelBatch.xlsx" download="ChangeVipTemplateFile.xlsx">Template File</a>
            </div>
            <div className="form-group width-200">
                <label className="col-form-label">Upload File</label>
                <input id="uploadfile" className="form-control" type="file" accept="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet, application/vnd.ms-excel" onChange={(e) => { EditFiled(e.target.value, 'file') }} disabled={stateSend} value={stateData.file} />
            </div>
            <button className="btn btn-add" style={{ marginTop: '20px', marginBottom: '40px' }} onClick={SetMsg}>Submit</button>
            <div className="form-group">{result}</div>
        </div>
    )
}

export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}