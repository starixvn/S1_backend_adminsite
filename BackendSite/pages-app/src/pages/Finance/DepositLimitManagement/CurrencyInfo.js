import React, { useState, useEffect } from "react";
import { LevelInfo } from "./LevelInfo";
import { PopBoxContentAddLevel } from "./PopBoxContentAddLevel";
import { api } from '../../../module/WebAPI';

if (process.env.NODE_ENV !== 'production') {
    window.i18n = {
        "lbl_Currency1": "SGD", "lbl_Currency2": "MYR", "lbl_Currency3": "USD", "lbl_Currency4": "THB", "lbl_Currency6": "EURO", "lbl_Currency9": "AUD", "lbl_Currency12": "GBP", "lbl_Currency13": "CNY", "lbl_Currency15": "IDR", "lbl_Currency20": "UUS", "lbl_Currency32": "JPY", "lbl_Currency40": "WON", "lbl_Currency41": "CHF", "lbl_Currency45": "WON", "lbl_Currency51": "VND", "lbl_Currency61": "INR","lbl_Save": "Save", "lbl_Success": "Success", "lbl_Action": "Action", "lbl_SelectSite": "Select site", "lbl_Point": "Point", "lbl_DepositLeast": "Deposit least", "lbl_BetLeast": "Bet least", "lbl_AddPointLevel": "Add point level", "lbl_AddNewPointLevel": "Add new point level", "lbl_Delete": "Delete", "lbl_AreYouSure": "Are you sure?", "lbl_OK": "OK", "lbl_Cancel": "Cancel", "lbl_Error": "Error"
    };
}

export const CurrencyInfo = (props) => {
    let CurrencyList = props.optSite.siteCurrencyList.map((value) => {
        return (
            <div key={`CurrencyInfo-${value}`} className="panel mt-20">
                <a className="panel-title" data-toggle="collapse" href={`#collapseExample-${value}`} role="button" aria-expanded="true" aria-controls="collapseExample">
                    {window.i18n["lbl_Currency" + value]}
                </a>
                <Result siteId={props.optSite.siteId} depositType={props.optSite.depositType} currId={value} />
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
    const [levelList, setLevelList] = useState(null);
    const [statusValue, setstatusValue] = useState({});
    const handleOpen = (_isOpen) => {
        setOpen(_isOpen);
    }

    const handleAddLevelList = (_newList) => {
        setLevelList([...levelList, _newList]);
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

    const handleStatusChange = (idx, _newData) => {
        if (statusValue !== null) {
            setstatusValue(null);
        }
        setstatusValue({ [idx]: _newData });
        let tmp = [...levelList], status = (_newData) ? 1 : 0;
        tmp[idx].depositStatus;
        setLevelList(tmp);
        let data = {
            siteId: levelList[idx].siteId,
            currencyId: levelList[idx].currencyId,
            before: {
                depositType: levelList[idx].depositType,
                vipLevel: levelList[idx].vipLevel,
                depositMin: parseInt(levelList[idx].depositMin),
                depositMax: parseInt(levelList[idx].depositMax),
                depositStatus: levelList[idx].depositStatus
            },
            after:{
                depositType: levelList[idx].depositType,
                vipLevel: levelList[idx].vipLevel,
                depositMin: parseInt(levelList[idx].depositMin),
                depositMax: parseInt(levelList[idx].depositMax),
                depositStatus: status
            }
        };

        api.post('/apiv2/DepositLimit/Update', data)
            .then(function (response) {
                if (response.data.errorCode === 0) {
                    alert(response.data.message);
                } else {
                    alert(response.data.message);
                }
            }).catch(function (error) {
                alert(error);
            })
    }


    const handleSetLevelList = (idx, status, depositMin, depositMax) => {
        let tmp = [...levelList];
        if (status === "Delete") {
            tmp.splice(idx, 1);
            setLevelList(tmp);
        } else if (status === "Update") {
            tmp[idx].depositMin = depositMin;
            tmp[idx].depositMax = depositMax;
            setLevelList(tmp);
        }
    }

    const handleDelete = (idx) => {
        let data = {
            siteId: levelList[idx].siteId,
            currencyId: levelList[idx].currencyId,
            before: {
                depositType: levelList[idx].depositType,
                vipLevel: levelList[idx].vipLevel,
                depositMin: parseInt(levelList[idx].depositMin),
                depositMax: parseInt(levelList[idx].depositMax),
                depositStatus: levelList[idx].depositStatus
            }
        };
        if (window.confirm("您真的確定要刪除嗎？\n\n請確認！") === true) {
            api.post('/apiv2/DepositLimit/Delete', data)
                .then(function (response) {
                    if (response.data.errorCode === 0) {
                        handleSetLevelList(idx, "Delete");
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
            let depositMin = typeof editValue.depositMin === "undefined" ? levelList[idx].depositMin : editValue.depositMin;
            let depositMax = typeof editValue.depositMax === "undefined" ? levelList[idx].depositMax : editValue.depositMax;
            let data = {
                siteId: levelList[idx].siteId,
                currencyId: levelList[idx].currencyId,
                before: {
                    depositType: levelList[idx].depositType,
                    vipLevel: levelList[idx].vipLevel,
                    depositMin: parseInt(levelList[idx].depositMin),
                    depositMax: parseInt(levelList[idx].depositMax),
                    depositStatus: levelList[idx].depositStatus
                },
                after:{
                    depositType: levelList[idx].depositType,
                    vipLevel: levelList[idx].vipLevel,
                    depositMin: parseInt(depositMin),
                    depositMax: parseInt(depositMax),
                    depositStatus: levelList[idx].depositStatus
                }
            };

            api.post('/apiv2/DepositLimit/Update', data)
                .then(function (response) {
                    if (response.data.errorCode === 0) {
                        handleSetLevelList(idx, "Update", depositMin, depositMax);
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
        api.post('/api/Deposit/GetDepositLimitInfos', { siteId: props.siteId, depositType: props.depositType, currencyId: props.currId })
            .then(function (response) {
                if (response.data.errorCode === 0) {
                    if (mounted) {
                        setLevelList(response.data === null ? [] : response.data.data);
                    }
                } else {
                    // alert(response.message);
                }
            }).catch(function (error) {
                alert(error);
            })
        return () => mounted = false;
    }, []);

    if (levelList === null) {
        return <></>
    } else {
        return (
            <div className="panel-body" >
                <div className="collapse show" id={`collapseExample-${props.currId}`}>
                    <div className="table">
                        <div className="table-options">
                            <button className="btn btn-add" onClick={handleOpen.bind(null, true)}>Add point level</button>
                        </div>
                        <LevelInfo
                            LevelList={levelList}
                            handleEdit={handleEdit}
                            handleUpdate={handleUpdate}
                            handleDelete={handleDelete}
                            EditValue={stateEdit}
                            ChangeData={ChangeData}
                            StatusValue={statusValue}
                            handleStatusChange={handleStatusChange}
                        />
                    </div>
                    {isOpen ? <PopBoxContentAddLevel ClickCancel={handleOpen.bind(null, false)} LevelList={levelList} SiteId={props.siteId} CurrencyId={props.currId} DepositType={props.depositType} ClickSave={handleAddLevelList} /> : null}
                </div>
            </div >
        )
    }
}