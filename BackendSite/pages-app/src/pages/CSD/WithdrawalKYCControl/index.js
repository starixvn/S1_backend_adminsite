import React, { useState, useEffect } from "react";
import ReactDOM from 'react-dom';
import { KycInfo } from './KycInfo';
import { api } from '../../../module/WebAPI';

const App = () => {
    return (
        <div className="container-fluid">
            <h4 className="page-name">Withdrawal KYC Control</h4>
            <div>
                <WithdrawalKYCInfo />
            </div>
        </div>
    )
}

const WithdrawalKYCInfo = () => {
    const [site, setSite] = useState([]);
    const [kycList, setKycList] = useState(null);
    const [stateEdit, setEdit] = useState({});
    const [editValue, setEditValue] = useState(null);

    const optSite = (siteId) => {
        let mounted = true;
        setEdit({});
        setEditValue(null);
        api.post('/api/Withdrawal/WithdrawalKYCList', { siteId })
            .then(function (response) {
                if (response.data.errorCode === 0) {
                    if (mounted) {
                        setKycList(response.data === null ? [] : response.data.data);
                    }
                } else {
                    // alert(response.message);
                }
            }).catch(function (error) {
                alert(error);
            })
        return () => mounted = false;
    }

    const handleEdit = (idx) => {
        let isEdit = stateEdit[idx] === true ? false : true;
        if (editValue !== null) {
            setEditValue(null);
        }
        setEdit({
            [idx]: isEdit
        });
    };
    const ChangeData = (idx, _locate, _newData) => {
        setEditValue({
            ...editValue,
            idx: idx,
            [_locate]: _newData
        });
    }

    const handleUpdate = (idx) => {
        if (editValue !== null) {
            let amount = typeof editValue.amount === "undefined" ? kycList[idx].amount : editValue.amount;
            api.post('/api/Withdrawal/EditWithdrawalKYCList', { siteId: kycList[idx].siteId, currencyId: kycList[idx].currencyId, amount: parseInt(amount) })
                .then(function (response) {
                    if (response.data.errorCode === 0) {
                        let tmp = [...kycList];
                        tmp[idx].amount = amount;
                        setKycList(tmp);
                        alert(response.data.message);
                        handleEdit(idx);
                    } else {
                        alert(response.data.message);
                    }
                }).catch(function (error) {
                    alert(error);
                })
        }
    }

    let radio = "";
    useEffect(() => {
        api.get('/api/Marketing/GetAllSites')
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
            return (
                <div key={`SiteOpt-${value.siteId}`} className="custom-control custom-radio custom-control-inline">
                    <input type="radio" id={`SiteId_${value.siteId}`} name="SiteList" className="custom-control-input" value={value.siteId} onChange={optSite.bind(null, value.siteId)} />
                    <label className="custom-control-label" htmlFor={`SiteId_${value.siteId}`}>{value.siteName}</label>
                </div>
            )
        });
    }

    return (
        <>
            <div className="form-group">
                <label className="col-form-label d-block">Site Name</label>
                {radio}
            </div>
            {(kycList) ? <KycInfo kycList={kycList}
                HandleEdit={handleEdit}
                HandleUpdate={handleUpdate}
                StateEdit={stateEdit}
                EditValue={editValue}
                ChangeData={ChangeData}
            /> : null}
        </>
    )
}

export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}