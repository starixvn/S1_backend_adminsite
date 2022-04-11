import React from "react";
import { InputBox, ActionButtons } from './components';

export const LevelInfo = (props) => {
    let levelList = props.LevelList;
    let lastIndex = levelList.length - 1;
    let editValue = props.EditValue;
    let List = levelList.map((value, idx) => {
        let isEdit = editValue[idx] === true;
        let isDelete = lastIndex === idx;
        let _editValue = editValue[idx] && editValue !== null && idx === editValue.idx ? {
            withdrawalMin: editValue.withdrawalMin,
            withdrawalMax: editValue.withdrawalMax,
            times: editValue.times
        } : {
                withdrawalMin: value.withdrawalMin,
                withdrawalMax: value.withdrawalMax,
                times: value.times
            };
        return (
            <tr key={`levellist-${value.currencyId}-${idx}`}>
                <td>{value.vipLevel}</td>
                <td>{value.levelName}</td>
                <td><InputBox isEdit={isEdit} defaultValue={_editValue.times} onChange={props.ChangeData.bind(null, idx, "times")} /></td>
                <td><InputBox isEdit={isEdit} defaultValue={_editValue.withdrawalMin} onChange={props.ChangeData.bind(null, idx, "withdrawalMin")} /></td>
                <td><InputBox isEdit={isEdit} defaultValue={_editValue.withdrawalMax} onChange={props.ChangeData.bind(null, idx, "withdrawalMax")} /></td>
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
                    <th scope="col">Vip Level Name</th>
                    <th scope="col">Withdrawal Frequency (0 = Unlimited)</th>
                    <th scope="col">Withdrawal Min</th>
                    <th scope="col">Withdrawal Max (0 = Unlimited)</th>
                    <th scope="col">Action</th>
                </tr>
            </thead>
            <tbody>
                {List}
            </tbody>
        </table>
    )
}