import React, { useState, useEffect } from "react";
import ReactDOM from 'react-dom';
import { api } from '../../../module/WebAPI';
import qs from 'qs';

const initData = {
    optSite: null,
    option: null,
    checkwd: false,
    withdrawal: 1,
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
        if (!stateData.option) { check = false; msg = "Please check Option."; }
        if (!stateData.optSite) { check = false; msg = "Please check Site."; }
        if (check && window.confirm("Are you sure to import data to " + _site[stateData.optSite] + "?")) {
            let settings = { headers: { 'content-type': 'multipart/form-data' } }
            let file = document.getElementById("uploadfile").files[0];
            let formData = new FormData();
            formData.append("excelUpload", file, file.name);
            formData.append('option', parseInt(stateData.option));
            formData.append('optSite', parseInt(stateData.optSite));
            formData.append('withdrawalLimitOption', (stateData.checkwd) ? 1 : 0);
            if (!stateSend) {
                updateSend(true);
                api.post('/api/Payment/BalanceAdjustmentBatch', formData, settings)
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
        <>
            <div className="container-fluid">
                <h4 className="page-name">Balance Adjustment Batch(Batch)</h4>
                <div className="form-group">
                    <label className="col-form-label d-block">Site Name</label>
                    {radio}
                </div>
                <div className="form-group">
                    <label className="col-form-label d-block">Download Template File </label>
                    <a className="btn btn-danger" role="button" href="../../../resource/examplefile/BalanceAdjustmentBatch.xlsx" download="BalanceAdjustmentTemplateFile.xlsx">Template File</a>
                </div>
                <div className="form-group">
                    <label className="col-form-label d-block">Option</label>
                    <div className="custom-control custom-radio custom-control-inline">
                        <input type="radio" id="Credit" name="Option" className="custom-control-input" onChange={(e) => { EditFiled(1, 'option') }} disabled={stateSend} checked={stateData.option === 1} />
                        <label className="custom-control-label" htmlFor="Credit">Credit</label>
                    </div>
                    <div className="custom-control custom-radio custom-control-inline">
                        <input type="radio" id="Debit" name="Option" className="custom-control-input" onChange={(e) => { EditFiled(2, 'option') }} disabled={stateSend} checked={stateData.option === 2} />
                        <label className="custom-control-label" htmlFor="Debit">Debit</label>
                    </div>
                </div>
                <div className="form-group">
                    <div className="custom-control custom-checkbox custom-control-inline">
                        <input className="custom-control-input" id="checkwd" type="checkbox" onChange={(e) => { EditFiled(e.target.checked, 'checkwd') }} disabled={stateSend} checked={stateData.checkwd === true} />
                        <label className="custom-control-label" htmlFor="checkwd">Withdraw Condition(Balance Amount X Times):</label>
                    </div>
                    <input className="form-control width-200" type="number" defaultValue={stateData.withdrawal} disabled />
                </div>
                <div className="form-group width-200">
                    <label className="col-form-label">Upload File</label>
                    <input id="uploadfile" className="form-control" type="file" accept="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet, application/vnd.ms-excel" onChange={(e) => { EditFiled(e.target.value, 'file') }} disabled={stateSend} value={stateData.file} />
                </div>
                <button className="btn btn-add" style={{ marginTop: '20px', marginBottom: '40px' }} onClick={SetMsg}>Submit</button>
                <div className="form-group">{result}</div>
            </div>
        </>
    )
}

export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}