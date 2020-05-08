import React, { useState, useEffect } from "react";
import { BankInfo } from "./BankInfo";
import { PopBoxContentAddPoint } from "./PopboxContentAddPoint";
import {api} from '../../../module/WebAPI';

if(process.env.NODE_ENV !== 'production'){
    window.i18n = {
        "lbl_Currency1":"SGD","lbl_Currency2":"MYR","lbl_Currency3":"USD","lbl_Currency4":"THB","lbl_Currency6":"EURO","lbl_Currency9":"AUD","lbl_Currency12":"GBP","lbl_Currency13":"RMB","lbl_Currency15":"IDR","lbl_Currency20":"UUS","lbl_Currency32":"JPY","lbl_Currency40":"WON","lbl_Currency41":"CHF","lbl_Currency45":"WON","lbl_Currency51":"VND","lbl_Save":"Save","lbl_Success":"Success","lbl_Action":"Action","lbl_SelectSite":"Select site","lbl_Point":"Point","lbl_DepositLeast":"Deposit least","lbl_BetLeast":"Bet least","lbl_AddPointLevel":"Add point level","lbl_AddNewPointLevel":"Add new point level","lbl_Delete":"Delete","lbl_AreYouSure":"Are you sure?","lbl_OK":"OK","lbl_Cancel":"Cancel","lbl_Error":"Error"
    };
}

export const CurrencyInfo = (props) => {
    let CurrencyList = props.optSite.siteCurrencyList.map((value) => {
        return (
            <div key={`CurrencyInfo-${value}`} className="panel mt-20">
                <a className="panel-title" data-toggle="collapse" href={`#collapseExample-${value}`} role="button" aria-expanded="true" aria-controls="collapseExample">
                    {window.i18n["lbl_Currency" + value]}
                </a>
                <Result siteId={props.optSite.siteId} currId={value} />
            </div>
        )
    })
    return (
        CurrencyList
    )
}

const Result = (props) => {
    const [stateEdit, setEdit] = useState({});
    const [editValue, setEditValue] = useState(null);
    const [isOpen, setOpen] = useState(false);
    const [bankList, setBankList] = useState(null);
    const handleOpen = (_isOpen) => {
        setOpen(_isOpen);
    }
    
    const handleAddBankList = (_newList) => {
        setBankList([...bankList, _newList]);
        setOpen(false);
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

    const handleSetBankList = (idx, status, depositLeast, betLeast) => {
        let tmp = [...bankList];
        if (status === "Delete") {
            tmp.splice(idx, 1);
            setBankList(tmp);
        } else if (status === "Update") {
            tmp[idx].depositLeast = depositLeast;
            tmp[idx].betLeast = betLeast;
            setBankList(tmp);
        }
    }

    const handleDelete = (idx) => {
        if (window.confirm("您真的確定要刪除嗎？\n\n請確認！") === true) {
            api.post('/api/Marketing/DeletePointLevelInfo', { siteId: bankList[idx].siteId, currencyId: bankList[idx].currencyId, pointLevel: bankList[idx].pointLevel })
                .then(function (response) {
                    if (response.data.errorCode === 0) {
                        handleSetBankList(idx, "Delete");
                        alert(response.data.message);
                    } else {
                        alert(response.data.message);
                    }
                }).catch(function (error) {
                    alert(error);
                })
        }
    }

    const handleUpdate = (idx) => {
        if (editValue !== null) {
            let depositLeast = typeof editValue.depositLeast === "undefined" ? bankList[idx].depositLeast : editValue.depositLeast;
            let betLeast = typeof editValue.betLeast === "undefined" ? bankList[idx].betLeast : editValue.betLeast;
            api.post('/api/Marketing/EditPointLevelInfo', { siteId: bankList[idx].siteId, currencyId: bankList[idx].currencyId, pointLevel: bankList[idx].pointLevel, betLeast: parseInt(betLeast), depositLeast: parseInt(depositLeast) })
                .then(function (response) {
                    if (response.data.errorCode === 0) {
                        handleSetBankList(idx, "Update", depositLeast, betLeast);
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

    const ChangeData = (idx, _locate, _newData) => {
        setEditValue({
            ...editValue,
            idx: idx,
            [_locate]: _newData
        });
    }

    useEffect(() => {
        let mounted = true;
        api.post('/api/Marketing/GetPointLevelInfos', { siteId: props.siteId, currencyId: props.currId })
            .then(function (response) {
                if (response.data.errorCode === 0) {
                    if(mounted){
                        setBankList(response.data === null ? [] : response.data.data);
                    }
                } else {
                    // alert(response.message);
                }
            }).catch(function (error) {
                alert(error);
            })
        return ()=> mounted = false;
    }, []);

    if (bankList === null) {
        return <></>
    } else {
        return (
            <div className="panel-body" >
                <div className="collapse show" id={`collapseExample-${props.currId}`}>
                    <div className="table">
                        <div className="table-options">
                            <button className="btn btn-add" onClick={handleOpen.bind(null, true)}>Add point level</button>
                        </div>
                        <BankInfo 
                            BankList={bankList} 
                            handleEdit={handleEdit}
                            handleUpdate={handleUpdate}
                            handleDelete={handleDelete}
                            EditValue={stateEdit}
                            ChangeData={ChangeData}
                        />
                    </div>
                    {isOpen ? <PopBoxContentAddPoint ClickCancel={handleOpen.bind(null, false)} BankList={bankList} SiteId={props.siteId} CurrencyId={props.currId} ClickSave={handleAddBankList} /> : null}
                </div>
            </div >
        )
    }
}