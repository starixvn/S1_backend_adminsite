import React, { useState } from "react";
import { InputBox, ActionButtons, SwitchBtn } from './components';

export const BankInfo = (props) => {
    let bankList = props.BankList;
    let lastIndex = bankList.length - 1;
    let stateEdit = props.StateEdit;
    let editValue = props.EditValue;

    let List = bankList.map((value, idx) => {
        let isEdit = stateEdit[idx] === true;
        let isDelete = lastIndex === idx;
        let _editIsKyc = stateEdit[idx] && stateEdit !== null && editValue !== null && typeof editValue.isKyc !== 'undefined' ? editValue.isKyc : value.isKyc === 1;
        // let _editValue = stateEdit[idx] && stateEdit !== null && idx === stateEdit.idx ? {
        //     depositCnt: editValue.depositCnt,
        //     withdrawalCnt: editValue.withdrawalCnt
        // } : {
        //         depositCnt: value.depositCnt,
        //         withdrawalCnt: value.withdrawalCnt
        //     };
        let _editValue = { depositCnt: value.depositCnt, withdrawalCnt: value.withdrawalCnt };


        return (
            <tr key={`banklist-${value.currencyId}-${idx}`}>
                <td>{value.pointLevel}</td>
                <td><InputBox isEdit={isEdit} defaultValue={_editValue.depositCnt} onChange={props.ChangeData.bind(null, idx, "depositCnt")} /></td>
                <td><InputBox isEdit={isEdit} defaultValue={_editValue.withdrawalCnt} onChange={props.ChangeData.bind(null, idx, "withdrawalCnt")} /></td>
                <td><SwitchBtn isEdit={isEdit} SwitchBtnKey={`Btn-${value.currencyId}-${idx}`} checked={_editIsKyc} onChange={props.ChangeData.bind(null, idx, "isKyc")} /></td>
                <td>
                    <div className="button-action">
                        <ActionButtons isEdit={isEdit} isDelete={isDelete} ClickEdit={props.handleEdit.bind(null, idx)} HandleDelete={props.handleDelete.bind(null, idx)} HandleUpdate={props.handleUpdate.bind(null, idx)} />
                    </div>
                </td>
            </tr>
        )
    })
    return (
        <table className="table table-striped">
            <thead>
                <tr>
                    <th scope="col">Point</th>
                    <th scope="col">Deposit Count</th>
                    <th scope="col">Withdrawal Count</th>
                    <th scope="col">IsKyc</th>
                    <th scope="col">Action</th>
                </tr>
            </thead>
            <tbody>
                {List}
            </tbody>
        </table>
    )
}