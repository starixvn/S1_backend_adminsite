import React, { useState } from "react";
import ReactDOM from 'react-dom';
import { api } from '../../../module/WebAPI';
import qs from 'qs';

const App = () => {
    const [stateSend, updateSend] = useState(false);
    const [stateData, updateData] = useState({
        option: null,
        checkwd: false,
        withdrawal: 1,
        file: ''
    });
    const [sendData, updateSendData] = useState([]);

    const EditFiled = (v, datacolumn) => {
        updateData({ ...stateData, [datacolumn]: v });
    };
    const SetMsg = () => {
        let msg = "";
        let check = true;

        if (!stateData.option) {
            check = false;
            msg = "Please check Option.";
        }

        if (check) {
            let settings = { headers: { 'content-type': 'multipart/form-data' } }
            let file = document.getElementById("uploadfile").files[0];
            let formData = new FormData();
            formData.append("excelUpload", file, file.name);
            formData.append('option', parseInt(stateData.option));
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
            }
        } else {
            alert(msg);
        }
    }
    let result = sendData.map((value, key) => { return <p key={`result-${key}`}>{value.message}</p> })
    return (
        <div className="container-fluid">
            <h4 className="page-name">Balance Adjustment Batch(Batch)</h4>
            <div className="form-group">
                <label className="col-form-label d-block">Download Template File </label>
                <a className="btn btn-danger" role="button" href="../../../resource/examplefile/BalanceAdjustmentBatch.xlsx" download="TemplateFile.xlsx">Template File</a>
            </div>
            <div className="form-group">
                <label className="col-form-label d-block">Option</label>
                <div className="custom-control custom-radio custom-control-inline">
                    <input type="radio" id="Credit" name="Option" className="custom-control-input" onChange={(e) => { EditFiled(1, 'option') }} disabled={stateSend} />
                    <label className="custom-control-label" htmlFor="Credit">Credit</label>
                </div>
                <div className="custom-control custom-radio custom-control-inline">
                    <input type="radio" id="Debit" name="Option" className="custom-control-input" onChange={(e) => { EditFiled(2, 'option') }} disabled={stateSend} />
                    <label className="custom-control-label" htmlFor="Debit">Debit</label>
                </div>
            </div>
            <div className="form-group">
                <div className="custom-control custom-checkbox custom-control-inline">
                    <input className="custom-control-input" id="checkwd" type="checkbox" onChange={(e) => { EditFiled(e.target.checked, 'checkwd') }} disabled={stateSend} />
                    <label className="custom-control-label" htmlFor="checkwd">Withdraw Condition(Balance Amount X Times):</label>
                </div>
                <input className="form-control width-200" type="number" defaultValue={stateData.withdrawal} disabled />
            </div>
            <div className="form-group width-200">
                <label className="col-form-label">Upload File</label>
                <input id="uploadfile" className="form-control" type="file" accept="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet, application/vnd.ms-excel" onChange={(e) => { EditFiled(e.target.value, 'file') }} disabled={stateSend} />
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