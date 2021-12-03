import React, { useState, useEffect } from "react";
import ReactDOM from 'react-dom';
import { api } from '../../../module/WebAPI';
import qs from 'qs';
import { PopBoxContent } from "./PopBoxContent";

const initData = {
    optSite: null,
    option: null,
    checkwd: false,
    withdrawal: 1,
    file: '',
    bonusCode:'',
    HasBonusCode:false,
    Name:'',
    CheckedBonusCode:'',
    BonusCodeDetail:null,
    WalletType:0,
    NeedCheckPassword:false,
    HasCheckPassword:false,
}

const App = () => {
    let _site = [];
    const [site, setSite] = useState([]);
    const [stateSend, updateSend] = useState(false);
    const [stateData, updateData] = useState({ ...initData });
    const [sendData, updateSendData] = useState([]);
    const EditFiled = (v, datacolumn) => {
        if(datacolumn == 'option'){
            updateData(
                { ...stateData,
                    bonusCode:'',
                    HasBonusCode:false,
                    Name:'',
                    CheckedBonusCode:'',
                    BonusCodeDetail:null,
                    WalletType:0,
                    NeedCheckPassword:false,
                    HasCheckPassword:false,
                    [datacolumn]: v 
                }); 
        }
        else{
            updateData({ ...stateData, [datacolumn]: v }); 
        }       
    };
    const SetMsg = () => {
        let msg = "";
        let check = true;
        let hasCheckPassword = stateData.HasCheckPassword;
        if (!stateData.file) { check = false; msg = "Please choose excel file."; }
        if (!stateData.option) { check = false; msg = "Please check Option."; }
        if (!stateData.optSite) { check = false; msg = "Please check Site."; }
        if ((stateData.option == 5 || stateData.option == 6) && stateData.CheckedBonusCode == '') { check = false; msg = "Please check BonusCode."; }
        if(stateData.checkwd && (isNaN(parseInt(stateData.withdrawal))? 0:parseInt(stateData.withdrawal)) === 0) { check = false; msg = "Please Enter Withdrawal Limit."; }
        if (check && window.confirm("Are you sure to import data to " + _site[stateData.optSite] + "?")) {
            let settings = { headers: { 'content-type': 'multipart/form-data' } }
            let file = document.getElementById("uploadfile").files[0];
            let formData = new FormData();
            formData.append("excelUpload", file, file.name);
            formData.append('option', parseInt(stateData.option));
            formData.append('optSite', parseInt(stateData.optSite));
            formData.append('withdrawalLimitOption', (stateData.checkwd) ? 1 : 0);
            formData.append('withdrawal',isNaN(parseFloat(stateData.withdrawal)) ? 0 : parseFloat(stateData.withdrawal));
            formData.append('bonuscode',stateData.CheckedBonusCode);
            let Data = {
                BonusCode: stateData.bonusCode,
                SiteId: parseInt(stateData.optSite)
            };
            if (!stateSend) {
                if(!hasCheckPassword && stateData.option == 5){
                    api.post('/api/Payment/CheckRententionBonusAdjustmentLimit', Data)
                    .then(function (response) {
                        if(response.data.errorCode == 0){
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
                        else if(response.data.errorCode == 1000){
                            updateData({
                                ...stateData,
                                NeedCheckPassword: true
                            })
                        }
                        else{
                            alert(response.data.message);
                        }
                    }).catch(function (error) {
                        updateSend(false);
                        alert(error);
                    })
                }
                else{
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
            }
        } else {
            alert(msg);
        }
    }
    const CheckPassWordSuccess = () =>{
        updateData({
            ...stateData,
            NeedCheckPassword: false,
            HasCheckPassword:true
        })
        CheckSetMsg();
    }
    const CheckSetMsg = ()=>{
        let settings = { headers: { 'content-type': 'multipart/form-data' } }
        let file = document.getElementById("uploadfile").files[0];
        let formData = new FormData();
        formData.append("excelUpload", file, file.name);
        formData.append('option', parseInt(stateData.option));
        formData.append('optSite', parseInt(stateData.optSite));
        formData.append('withdrawalLimitOption', (stateData.checkwd) ? 1 : 0);
        formData.append('withdrawal',parseFloat(stateData.withdrawal))
        formData.append('bonuscode',stateData.CheckedBonusCode)
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
    const CheckBonusCode = () =>{
        if (!stateData.optSite) { 
            alert("Please check Site.") 
            return;
        }
        let Data = {
            BonusCode: stateData.bonusCode,
            PromotionType: parseInt(stateData.option) == 5 ? 9 : 8,
            SiteId: parseInt(stateData.optSite)
          };
        api.post('/api/Payment/CheckBalanceAdjustBonusCodeBatch',Data)
            .then(function (response) {
                if (response.data.errorCode === 0) {
                    if(response.data.data.hasBonusCode){
                        alert("BonusCode " + response.data.data.bonusCode + " exist")       
                        updateData({ ...stateData, 
                            HasBonusCode: true,
                            CheckedBonusCode:response.data.data.bonusCode,
                            Name:response.data.data.name,
                            BonusCodeDetail:response.data.data.detail,
                            WalletType:response.data.data.walletType
                        })
                    }
                    else{
                        alert("BonusCode Error! Please Check Again")
                        updateData({ ...stateData, 
                            HasBonusCode: false,
                            CheckedBonusCode:'',
                            Name:'',
                            BonusCodeDetail:null,
                            WalletType:0
                        })
                    }
                } else {
                    alert(response.message);
                    updateData({ ...stateData, 
                        HasBonusCode: false,
                        CheckedBonusCode:'',
                        Name:'',
                        BonusCodeDetail:null,
                        WalletType:0
                    })
                }
            }).catch(function (error) {
                alert(error);
            })
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
    let bnousCodeDetail = null
    if(stateData.BonusCodeDetail !== null){
        bnousCodeDetail = stateData.BonusCodeDetail.map((value) => { 
            return <tr>
                <td>
                    {value.currency}
                </td>
                {stateData.option == 5 ? 
                <>
                    <td>
                        {value.bonus}
                    </td>
                    <td>
                        {value.rollOver}
                    </td>
                </>:null}
            </tr>
        })
    }
    const Cancel =()=>{
        updateData({ ...stateData, 
            NeedCheckPassword: false
        })
    }

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
                        <input type="radio" id="Option_1" name="Option" className="custom-control-input" onChange={(e) => { EditFiled(1, 'option') }} disabled={stateSend} checked={stateData.option === 1} />
                        <label className="custom-control-label" htmlFor="Option_1">Balance(+)</label>
                    </div>
                    <div className="custom-control custom-radio custom-control-inline">
                        <input type="radio" id="Option_2" name="Option" className="custom-control-input" onChange={(e) => { EditFiled(2, 'option') }} disabled={stateSend} checked={stateData.option === 2} />
                        <label className="custom-control-label" htmlFor="Option_2">Balance(-)</label>
                    </div>
                    <div className="custom-control custom-radio custom-control-inline">
                        <input type="radio" id="Option_3" name="Option" className="custom-control-input" onChange={(e) => { EditFiled(3, 'option') }} disabled={stateSend} checked={stateData.option === 3} />
                        <label className="custom-control-label" htmlFor="Option_3">Bonus(+)</label>
                    </div>
                    <div className="custom-control custom-radio custom-control-inline">
                        <input type="radio" id="Option_4" name="Option" className="custom-control-input" onChange={(e) => { EditFiled(4, 'option') }} disabled={stateSend} checked={stateData.option === 4} />
                        <label className="custom-control-label" htmlFor="Option_4">Bonus(-)</label>
                    </div>
                    <div className="custom-control custom-radio custom-control-inline">
                        <input type="radio" id="Option_5" name="Option" className="custom-control-input" onChange={(e) => { EditFiled(5, 'option') }} disabled={stateSend} checked={stateData.option === 5} />
                        <label className="custom-control-label" htmlFor="Option_5">Retention Bonus</label>
                    </div>
                    <div className="custom-control custom-radio custom-control-inline">
                        <input type="radio" id="Option_6" name="Option" className="custom-control-input" onChange={(e) => { EditFiled(6, 'option') }} disabled={stateSend} checked={stateData.option === 6} />
                        <label className="custom-control-label" htmlFor="Option_6">Manual Bonus</label>
                    </div>
                </div>
                {
                    stateData.option == 5 || stateData.option == 6? 
                    <>
                        <div className="form-group">
                            <label className="col-form-label d-block">BonusCode</label>
                            <input className="form-control width-200" value={stateData.bonusCode} onChange={(e) => { EditFiled(e.target.value, 'bonusCode') }} />
                        </div>
                        <button className="btn btn-add" style={{marginBottom: '5px' }} onClick={CheckBonusCode}>CheckBonusCode</button>
                    </>:null
                }
                {/* {
                    stateData.option == 6 ? 
                    <>
                        <div className="form-group">
                            <label className="col-form-label d-block">BonusCode</label>
                            <input className="form-control width-200" defaultValue={stateData.bonusCode} onChange={(e) => { EditFiled(e.target.value, 'bonusCode') }} />
                        </div>
                        <button className="btn btn-add" style={{marginBottom: '5px' }} onClick={CheckBonusCode}>CheckBonusCode</button>
                    </>:null
                } */}
                {
                    stateData.BonusCodeDetail !== null && (stateData.option == 5 || stateData.option == 6) ?
                    <>
                        <h4 className="page-name">{stateData.option == 5 ? "Retention Bonus Info" : "Manual Bonus Info"}</h4>
                        <table className="table table-detail">
                            <tr>
                                <th scope="row">BonusCode</th>
                                <td>{stateData.CheckedBonusCode}</td>
                            </tr>
                            <tr>
                                <th scope="row">WalletType</th>
                                <td>{stateData.WalletType == 0 ? "Transfer" : "Seamless" }</td>
                            </tr>
                            <tr>
                                <th scope="row">ProductCategory</th>
                                <td>{stateData.Name}</td>
                            </tr>
                            <tr>
                                <th scope="row">Detail</th>
                                <td>
                                    <tr>
                                        <th>
                                            Currency
                                        </th>
                                        {stateData.option == 5 ? 
                                        <>
                                            <th>
                                                Amount
                                            </th>
                                            <th>
                                                RollOver
                                            </th>
                                        </>:null}
                                    </tr>
                                    {bnousCodeDetail}
                                </td>
                            </tr>
                        </table>
                    </>:null
                }
                {
                    (stateData.option != 5 && stateData.option != 6) ?
                    <div className="form-group">
                        <div className="custom-control custom-checkbox custom-control-inline">
                            <input className="custom-control-input" id="checkwd" type="checkbox" onChange={(e) => { EditFiled(e.target.checked, 'checkwd') }} disabled={stateSend} checked={stateData.checkwd === true} />
                            <label className="custom-control-label" htmlFor="checkwd">Withdraw Condition(Balance Amount X Times):</label>
                        </div>
                        <input className="form-control width-200" type="number" defaultValue={stateData.withdrawal} onChange={(e) => { EditFiled(e.target.value, 'withdrawal') }} />
                    </div>:null                    
                }           
                <div className="form-group width-200">
                    <label className="col-form-label">Upload File</label>
                    <input id="uploadfile" className="form-control" type="file" accept="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet, application/vnd.ms-excel" onChange={(e) => { EditFiled(e.target.value, 'file') }} disabled={stateSend} value={stateData.file} />
                </div>
                <button className="btn btn-add" style={{ marginTop: '20px', marginBottom: '40px' }} onClick={SetMsg}>Submit</button>
                <div className="form-group">{result}</div>
            </div>
            {stateData.NeedCheckPassword ? (
                <PopBoxContent
                ClickCancel={Cancel}
                SiteId={stateData.optSite}
                CheckPassWordSuccess={CheckPassWordSuccess}
                />
            ) : null}
        </>
    )
}

export default App;

if (process.env.NODE_ENV === 'production') {
    ReactDOM.render(<App />, document.getElementById('content'))
}