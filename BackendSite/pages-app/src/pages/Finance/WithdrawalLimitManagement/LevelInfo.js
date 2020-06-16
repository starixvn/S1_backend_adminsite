import React, { useState } from "react";
import { InputBox, ActionButtons } from './components';
// import {number_format} from '../commJS/common';
import {number_format} from '../../../module/Formatting/NumberFormat';

export const LevelInfo = (props) => {


    let levelList = props.LevelList;

    let lastIndex = levelList.length - 1;
    let editValue = props.EditValue;
    let List = levelList.map((value, idx) => {
        let isEdit = editValue[idx] === true;
        let isDelete = lastIndex === idx;
        let _editValue = editValue[idx] && editValue !== null && idx === editValue.idx ? {
            withdrawalMin: editValue.withdrawalMin,
            withdrawalMax: editValue.withdrawalMax
        } : {
                withdrawalMin: value.withdrawalMin,
                withdrawalMax: value.withdrawalMax
            };

        return (
            <tr key={`levellist-${value.currencyId}-${idx}`}>
                <td>{value.vipLevel}</td>
                <td><InputBox isEdit={isEdit} defaultValue={number_format(_editValue.withdrawalMin)} onChange={props.ChangeData.bind(null, idx, "withdrawalMin")} /></td>
                <td><InputBox isEdit={isEdit} defaultValue={number_format(_editValue.withdrawalMax)} onChange={props.ChangeData.bind(null, idx, "withdrawalMax")} /></td>
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
                    <th scope="col">Vip Level</th>
                    <th scope="col">Withdrawal Min</th>
                    <th scope="col">Withdrawal Max</th>
                    <th scope="col">Action</th>
                </tr>
            </thead>
            <tbody>
                {List}
            </tbody>
        </table>
    )
}