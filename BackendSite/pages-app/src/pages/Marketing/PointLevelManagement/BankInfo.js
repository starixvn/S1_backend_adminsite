import React, { useState } from "react";
import { InputBox, ActionButtons } from './components';

export const BankInfo = (props) => {
    

    let bankList = props.BankList;

    let lastIndex = bankList.length - 1;
    let editValue = props.EditValue;
    let List = bankList.map((value, idx) => {
        let isEdit = editValue[idx] === true;
        let isDelete = lastIndex === idx;
        let _editValue = editValue[idx] && editValue !== null && idx === editValue.idx ? {
            depositLeast: editValue.depositLeast,
            betLeast: editValue.betLeast
        } : {
                depositLeast: value.depositLeast,
                betLeast: value.betLeast
            };

        return (
            <tr key={`banklist-${value.currencyId}-${idx}`}>
                <td>{value.pointLevel}</td>
                <td><InputBox isEdit={isEdit} defaultValue={_editValue.depositLeast} onChange={props.ChangeData.bind(null, idx, "depositLeast")} /></td>
                <td><InputBox isEdit={isEdit} defaultValue={_editValue.betLeast} onChange={props.ChangeData.bind(null, idx, "betLeast")} /></td>
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
                    <th scope="col">Deposit least</th>
                    <th scope="col">Bet least</th>
                    <th scope="col">Action</th>
                </tr>
            </thead>
            <tbody>
                {List}
            </tbody>
        </table>
    )
}