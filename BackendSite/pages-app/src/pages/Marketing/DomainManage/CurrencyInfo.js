import React, { useState, useEffect } from "react";
import { DomainInfo } from "./DomainInfo";
import { PopBoxContentAddPoint } from "./PopboxContentAddPoint";
import { api } from '../../../module/WebAPI';

if (process.env.NODE_ENV !== 'production') {
    window.i18n = {
        "lbl_Currency1": "SGD", "lbl_Currency2": "MYR", "lbl_Currency3": "USD", "lbl_Currency4": "THB", "lbl_Currency6": "EURO", "lbl_Currency9": "AUD", "lbl_Currency12": "GBP", "lbl_Currency13": "RMB", "lbl_Currency15": "IDR", "lbl_Currency20": "UUS", "lbl_Currency32": "JPY", "lbl_Currency40": "WON", "lbl_Currency41": "CHF", "lbl_Currency45": "WON", "lbl_Currency51": "VND", "lbl_Save": "Save", "lbl_Success": "Success", "lbl_Action": "Action", "lbl_SelectSite": "Select site", "lbl_Point": "Point", "lbl_DepositLeast": "Deposit least", "lbl_BetLeast": "Bet least", "lbl_AddPointLevel": "Add point level", "lbl_AddNewPointLevel": "Add new point level", "lbl_Delete": "Delete", "lbl_AreYouSure": "Are you sure?", "lbl_OK": "OK", "lbl_Cancel": "Cancel", "lbl_Error": "Error"
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
    const [domainList, setDomainList] = useState(null);
    const [statusValue, setstatusValue] = useState({});

    const handleOpen = (_isOpen) => {
        setOpen(_isOpen);
    }


    const handleAddDomainList = (_newList) => {
        setDomainList([...domainList, _newList]);
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

    const handleSetDomainList = (idx, status, Domain, Sort, dstatus) => {
        let tmp = [...domainList];
        if (status === "Delete") {
            tmp.splice(idx, 1);
            setDomainList(tmp);
        } else if (status === "Update") {
            tmp[idx].domain = Domain;
            tmp[idx].sort = Sort;
            tmp[idx].status = dstatus;
            
            setDomainList(tmp);
        }
    }
    

    const handleDelete = (idx) => {
        if (window.confirm('Are you sure to delete'+domainList[idx].domain +'?') === true) {
            api.post('/api/DomainTable/DelDomain', { dId : domainList[idx].did })
                .then(function (response) {
                    if (response.data.errorCode === 0) {
                        handleSetDomainList(idx, "Delete");
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
            let Domain = editValue.domain === undefined ? domainList[idx].domain : editValue.domain;
            let Sort = editValue.sort === undefined ? domainList[idx].sort : editValue.sort;
            let Status = editValue.status === undefined ? domainList[idx].status : (editValue.status) ? 1 : 0;
            let param = { dId : domainList[idx].dId ,  domain: Domain, sort: Sort, status: parseInt(Status) };

            api.post('/api/DomainTable/UpdateDomain', { dId : domainList[idx].dId ,  domain: Domain, sort: parseInt(Sort) , status: parseInt(Status) })
                .then(function (response) {
                    if (response.data.errorCode === 0) {
                        handleSetDomainList(idx, "Update", Domain, Sort, Status);
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
    const handleStatusChange = (idx, _newData) => {
        
        if (statusValue !== null) { 
            setstatusValue(null);
        }
        setstatusValue({ [idx]: _newData });
        let tmp = [...domainList], status = (_newData) ? 1 : 0;
        tmp[idx].status = status;
        setDomainList(tmp);
        let param = { dId : domainList[idx].dId, domain: domainList[idx].domain, sort: domainList[idx].sort, status: status };

        api.post('/api/DomainTable/UpdateDomain', { dId : domainList[idx].dId, domain: domainList[idx].domain, sort: domainList[idx].sort, status: parseInt(status) })
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
    const ChangeData = (idx, _locate, _newData) => {
        
        setEditValue({
            ...editValue,
            idx: idx,
            [_locate]: _newData
        });
    }

    useEffect(() => {
        let mounted = true;
        api.post('/api/DomainTable/GetDomainListbySite', { siteId: props.siteId, currencyId: props.currId })
            .then(function (response) {
                if (response.data.errorCode === 0) {
                    
                    // if(response.data !== null)
                    //    { console.log(response.data);}
                    
                    if (mounted) {
                        setDomainList(response.data === null ? [] : response.data.data);
                    }
                } else {
                    // alert(response.message);
                }
            }).catch(function (error) {
                alert(error);
            })  
        return () => mounted = false;
    }, []);

    if (domainList === null) {
        return <></>
    } else {
        
        return (
            <div className="panel-body" >
                <div className="collapse show" id={`collapseExample-${props.currId}`}>
                    <div className="table">
                        <div className="table-options">
                            <button className="btn btn-add" onClick={handleOpen.bind(null, true)}>Add Domain</button>
                        </div>
                        <DomainInfo
                            domainList={domainList}
                            handleEdit={handleEdit}
                            handleUpdate={handleUpdate}
                            handleDelete={handleDelete}
                            EditValue={editValue}
                            StateEdit={stateEdit}
                            StatusValue={statusValue}
                            ChangeData={ChangeData}
                            handleStatusChange={handleStatusChange}
                        />
                    </div>
                    {isOpen ? 
                    <PopBoxContentAddPoint 
                    ClickCancel={handleOpen.bind(null, false)} 
                    domainList={domainList} 
                    SiteId={props.siteId} 
                    CurrencyId={props.currId} 
                    ClickSave={handleAddDomainList} /> : null}
                </div>
            </div >
        )
    }
}