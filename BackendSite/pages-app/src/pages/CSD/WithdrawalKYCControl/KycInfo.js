import React from "react";
import { InputBox, ActionButtons } from './components';

if (process.env.NODE_ENV !== 'production') {
    window.i18n = {
        "lbl_Currency1": "SGD", "lbl_Currency2": "MYR", "lbl_Currency3": "USD", "lbl_Currency4": "THB", "lbl_Currency6": "EURO", "lbl_Currency9": "AUD", "lbl_Currency12": "GBP", "lbl_Currency13": "RMB", "lbl_Currency15": "IDR", "lbl_Currency20": "UUS", "lbl_Currency32": "JPY", "lbl_Currency40": "WON", "lbl_Currency41": "CHF", "lbl_Currency45": "WON", "lbl_Currency51": "VND", "lbl_Save": "Save", "lbl_Success": "Success", "lbl_Action": "Action", "lbl_SelectSite": "Select site", "lbl_Point": "Point", "lbl_DepositLeast": "Deposit least", "lbl_BetLeast": "Bet least", "lbl_AddPointLevel": "Add point level", "lbl_AddNewPointLevel": "Add new point level", "lbl_Delete": "Delete", "lbl_AreYouSure": "Are you sure?", "lbl_OK": "OK", "lbl_Cancel": "Cancel", "lbl_Error": "Error"
    };
}

export const KycInfo = (props) => {
    if (props.kycList === null) {
        return <></>
    } else {
        let editValue = props.EditValue;
        let Kyc = props.kycList.map((value, idx) => {
            let isEdit = props.StateEdit[idx] === true;
            let _editValue = editValue !== null && idx === editValue.idx ? editValue.amount : value.amount;
            return (
                <div key={`CurrencyInfo-${value.currencyId}`} className="panel mt-20">
                    <a className="panel-title" data-toggle="collapse" href={`#collapseExample-${value.currencyId}`} role="button" aria-expanded="true" aria-controls="collapseExample">
                        {window.i18n["lbl_Currency" + value.currencyId]}
                    </a>
                    <div className="panel-body" >
                        <div className="collapse show" id={`collapseExample-${value.currencyId}`}>
                            <div className="table">
                                <table className="table table-striped">
                                    <thead>
                                        <tr>
                                            <th scope="col">Amount</th>
                                            <th scope="col">Action</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <tr>
                                            <td><InputBox isEdit={isEdit} defaultValue={_editValue} onChange={props.ChangeData.bind(null, idx, "amount")} /></td>
                                            <td>
                                                <div className="button-action">
                                                    <ActionButtons isEdit={isEdit} ClickEdit={props.HandleEdit.bind(null, idx)} HandleUpdate={props.HandleUpdate.bind(null, idx)} />
                                                </div>
                                            </td>
                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </div >
                </div>
            )
        })
        return (
            Kyc
        )
    }
}