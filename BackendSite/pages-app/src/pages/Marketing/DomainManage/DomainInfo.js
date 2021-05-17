import React, { useState } from "react";
import { InputBox, ActionButtons, SwitchBtn } from './components';

export const DomainInfo = (props) => {
    let DomainList = props.domainList;
    let lastIndex = DomainList.length - 1;
    let stateEdit = props.StateEdit;
    let editValue = props.EditValue;

    let List = DomainList.map((value, idx) => {
        let isEdit = stateEdit[idx] === true;
        let isDelete = lastIndex === idx;
        let _editStatus = props.StatusValue[idx] && props.StatusValue != null ? props.StatusValue[idx] : value.status === 1;
        let _editValue = { Domain: value.domain, Status: value.status , Sort: value.sort };

        return (
            <tr key={`DomainList-${value.currencyId}-${idx}`}>
                {/* <td>{value.pointLevel}</td> */}
                <td><InputBox isEdit={isEdit} defaultValue={_editValue.Domain} onChange={props.ChangeData.bind(null, idx, "domain")} /></td>
                <td><InputBox isEdit={isEdit} defaultValue={_editValue.Sort} onChange={props.ChangeData.bind(null, idx, "sort")} type="number" /></td>
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
                    <th scope="col">Domain</th>
                    <th scope="col">Sort</th>
                    <th scope="col">Status</th>
                    <th scope="col">Action</th>
                </tr>
            </thead>
            <tbody>
                {List}
            </tbody>
        </table>
    )
}