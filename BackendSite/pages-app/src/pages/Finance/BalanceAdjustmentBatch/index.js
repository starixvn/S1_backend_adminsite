import React, { useState } from "react";
import ReactDOM from 'react-dom';
import { api } from '../../../module/WebAPI';
import qs from 'qs';

const App = () => {
    const [stateSend, updateSend] = useState(false);
    const [stateData, updateData] = useState({
        userName: '',
        option: null,
        checkwd: false,
        withdrawal: 1,
        amount: 0,
        withdrawalamount: 0
    });
    const [sendData, updateSendData] = useState([]);

    const EditFiled = (v, datacolumn) => {
        updateData({ ...stateData, [datacolumn]: v });
    };
    const SetMsg = () => {
        let msg = "";
        let check = true;
        if (!stateData.userName) {
            check = false;
            msg = "Please input userName.";
        } else if (!stateData.option) {
            check = false;
            msg = "Please check Option.";
        } else if (stateData.option === 1 && stateData.checkwd && stateData.withdrawal <= 0) {
            check = false;
            msg = "Withdraw must over 0.";
        } else if (stateData.amount <= 0) {
            check = false;
            msg = "Please input Amount.";
        }

        if (check) {
            let param = qs.stringify({
                custList: stateData.userName.trim().split(/\r?\n/).map(u => { return u.trim() }),
                option: parseInt(stateData.option),
                withdrawalLimitOption: (stateData.checkwd) ? 1 : 0,
                withdrawalLimitAmount: isNaN(parseFloat(stateData.amount) * stateData.withdrawal) ? 0 : parseFloat(stateData.amount) * stateData.withdrawal,
                amount: parseFloat(stateData.amount),
            })
            if (!stateSend) {
                updateSend(true);
                api.post('/api/Payment/BalanceAdjustmentBatch', param)
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
            <h4 className="page-name">Balance AdjustmentBatch(Batch)</h4>
            <div className="form-group">
                <label>User Name</label>
                <textarea style={{ 'width': '250px' }} className="form-control" rows="10" value={stateData.userName} onChange={(e) => { EditFiled(e.target.value, 'userName') }} disabled={stateSend}></textarea>
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
            {stateData.option === 1 ?
                <div className="form-group">
                    <div className="custom-control custom-checkbox custom-control-inline">
                        <input className="custom-control-input" id="checkwd" type="checkbox" onChange={(e) => { EditFiled(e.target.checked, 'checkwd') }} disabled={stateSend} />
                        <label className="custom-control-label" htmlFor="checkwd">Withdraw Condition(Balance Amount X Times):</label>
                    </div>
                    <input className="form-control width-200" type="number" defaultValue={stateData.withdrawal} disabled />
                </div>
                : null}
            <div className="form-group width-200">
                <label className="col-form-label">Amount</label>
                <input className="form-control" type="number" onChange={(e) => { EditFiled(e.target.value, 'amount') }} step="1" min="1" disabled={stateSend} />
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