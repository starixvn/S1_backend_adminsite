import React, { useState } from "react";
import { InputBox, ActionButtons, SwitchBtn } from "./components";

export const LineChannelInfo = (props) => {
  console.log(props);
  let LineChannelList = props.lineChannelList;
  let lastIndex = LineChannelList.length - 1;
  let stateEdit = props.StateEdit;
  let editValue = props.EditValue;

  let List = LineChannelList.map((value, idx) => {
    if (props.CurrencyId == value.currencyId) {
      let isEdit = stateEdit[idx] === true;
      let isDelete = lastIndex === idx;
      let _editStatus =
        props.StatusValue[idx] && props.StatusValue != null
          ? props.StatusValue[idx]
          : value.status === 1;
      let _editValue = { Link: value.link, Status: value.status };
      console.log(props);
      return (
        <tr key={`LineChannelList-${value.currencyId}-${idx}`}>
          <td>
            <InputBox
              isEdit={isEdit}
              defaultValue={_editValue.Link}
              onChange={props.ChangeData.bind(null, idx, "link")}
            />
          </td>
          <td>
            <SwitchBtn
              SwitchBtnKey={`Btn-${value.currencyId}-${idx}`}
              checked={_editStatus}
              onStatusChange={props.handleStatusChange.bind(null, idx)}
            />
          </td>
          <td>
            <div className="button-action">
              <ActionButtons
                isEdit={isEdit}
                isDelete={isDelete}
                ClickEdit={props.handleEdit.bind(null, idx)}
                HandleUpdate={props.handleUpdate.bind(null, idx)}
              />
            </div>
          </td>
        </tr>
      );
    }
  });
  return (
    <table className="table table-striped">
      <thead>
        <tr>
          <th scope="col">Link</th>
          <th scope="col">Status</th>
          <th scope="col">Action</th>
        </tr>
      </thead>
      <tbody>{List}</tbody>
    </table>
  );
};
