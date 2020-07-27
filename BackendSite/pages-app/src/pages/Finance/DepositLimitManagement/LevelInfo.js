import React, { useState } from "react";
import { InputBox, ActionButtons, SwitchBtn } from './components';
// import {number_format} from '../commJS/common';
import { number_format } from '../../../module/Formatting/NumberFormat';

export const LevelInfo = (props) => {
    let levelList = props.LevelList;
    let lastIndex = levelList.length - 1;
    let editValue = props.EditValue;
    let List = levelList.map((value, idx) => {
        let isEdit = editValue[idx] === true;
        let isDelete = lastIndex === idx;
        let _editStatus = props.StatusValue[idx] && props.StatusValue != null ? props.StatusValue[idx] : value.depositStatus === 1;
        let _editValue = editValue[idx] && editValue !== null && idx === editValue.idx ? {
            depositMin: editValue.depositMin,
            depositMax: editValue.depositMax
        } : {
                depositMin: value.depositMin,
                depositMax: value.depositMax
            };

        return (
            <tr key={`levellist-${value.currencyId}-${idx}`}>
                <td>{value.vipLevel}</td>
                <td><InputBox isEdit={isEdit} defaultValue={number_format(_editValue.depositMin)} onChange={props.ChangeData.bind(null, idx, "depositMin")} /></td>
                <td><InputBox isEdit={isEdit} defaultValue={number_format(_editValue.depositMax)} onChange={props.ChangeData.bind(null, idx, "depositMax")} /></td>
                <td><SwitchBtn SwitchBtnKey={`Btn-${value.currencyId}-${idx}`} checked={_editStatus} onStatusChange={props.handleStatusChange.bind(null, idx)} /></td>
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
                    <th scope="col">Deposit Min</th>
                    <th scope="col">Deposit Max</th>
                    <th scope="col">Deposit Status</th>
                    <th scope="col">Action</th>
                </tr>
            </thead>
            <tbody>
                {List}
            </tbody>
        </table>
    )
}