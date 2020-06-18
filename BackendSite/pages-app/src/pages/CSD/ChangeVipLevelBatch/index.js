import React, { useState, useEffect } from "react";
import ReactDOM from 'react-dom';
import { api } from '../../../module/WebAPI';
import qs from 'qs';

const App = () => {
    const [stateData, updateData] = useState({
        userName: '',
        viplevel: ''
    });

    const EditFiled = (v, datacolumn) => {
        updateData({
            ...stateData,
            [datacolumn]: v
        });
    };
    const SetMsg = () => {
        let msg = "";
        let check = true;
        if (!stateData.userName) {
            check = false;
            msg = "Please input userName.";
        } else if (!stateData.viplevel) {
            check = false;
            msg = "Please input viplevel.";
        }
        if (check) {
            let param = qs.stringify({
                custList: stateData.userName.trim().split(/\r?\n/).map(u => { return u.trim() }),
                vipLevel: parseInt(stateData.viplevel)
            })

            api.post('/api/Message/ChangeVipLevelBatch', param)
                .then(function (response) {
                    if (response.data.errorCode === 0) {
                        updateData({
                            userName: '',
                            viplevel: ''
                        });
                        alert('sent successfully');
                    } else {
                        alert(response.data.message);
                    }
                }).catch(function (error) {
                    alert(error);
                })
        } else {
            alert(msg);
        }
    }
    return (
        <div className="container-fluid">
            <h4 className="page-name">Change Vip Level Batch</h4>
            <div class="form-group">
                <label>User Name</label>
                <textarea style={{ 'width': '250px' }} className="form-control" rows="10" value={stateData.userName} onChange={(e) => { EditFiled(e.target.value, 'userName') }}></textarea>
            </div>
            <div class="form-group">
                <label>Vip Level</label>
                <input type="number" style={{ 'width': '100px' }} className="form-control" value={stateData.viplevel} onChange={(e) => { EditFiled(e.target.value, 'viplevel') }} />
            </div>
            <button className="btn btn-add" style={{ marginTop: '20px', marginBottom: '40px', float: 'left' }} onClick={SetMsg}>Submit</button>
        </div>
    )
}

export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}