import React, { useState } from "react";
import ReactDOM from "react-dom";

const SpecialItemType = {
  actionBtn: "actionBtn",
  statusBtn: "statusBtn",
};
const defShowStatusType = {
  1: "No login",
  2: "Login",
  3: "Both",
};
const dateFormat = (createTime) => {
  return moment(createTime).format("l LTS");
};
const showStatusTypeFormat = (type) => {
  return defShowStatusType[type];
};
const lastUpdateUserFormat = (userName) => {
  return userName ?? "-";
};
const lastUpdateTimeFormat = (lastUpdTime) => {
  return lastUpdTime ? moment(lastUpdTime).format("l LTS") : "-";
};
const battleTimeFormat = (battleTime) => {
  return battleTime ? moment(battleTime).format("l LTS") : "-";
};
const ActionButtons = (props) => {
  return (
    <div className="button-action" onClick={props.clickEdit}>
      <button className="btn btn-edit">
        <i className="icon-edit"></i>
      </button>
    </div>
  );
};
const StatusBtn = ({ checked, onChange, mainKey }) => {
  let id = `switch-${mainKey}`;
  return (
    <div className="switch__container">
      <input
        id={id}
        className="switch switch--flat"
        type="checkbox"
        checked={checked}
        onChange={onChange}
      />
      <label htmlFor={id}></label>
    </div>
  );
};
const titleData = [
  { dataColumn: "bannerName", columnTitle: "HotEventName" },
  { dataColumn: "siteName", columnTitle: "SiteName" },
  {
    dataColumn: "showStatus",
    columnTitle: "ShowStatus",
    body: { format: showStatusTypeFormat },
  },
  {
    dataColumn: "sportBattleDateTime",
    columnTitle: "Battle Time",
    body: { format: battleTimeFormat },
  },
  { dataColumn: "home", columnTitle: "Home Team" },
  { dataColumn: "away", columnTitle: "Away Team" },
  {
    dataColumn: "startTime",
    columnTitle: "Start Time",
    body: { format: dateFormat },
  },
  {
    dataColumn: "endTime",
    columnTitle: "End Time",
    body: { format: dateFormat },
  },
  {
    dataColumn: "lastUpdateUser",
    columnTitle: "LastUpdateUser",
    body: { format: lastUpdateUserFormat },
  },
  {
    dataColumn: "lastUpdateTime",
    columnTitle: "LastUpdateTime",
    body: { format: lastUpdateTimeFormat },
  },
  {
    dataColumn: "status",
    columnTitle: "Status",
    body: { SpecialItem: SpecialItemType.statusBtn },
  },
  {
    dataColumn: "edit",
    columnTitle: "Edit",
    body: { SpecialItem: SpecialItemType.actionBtn },
  },
];

const Panel = (props) => {
  let headerRows = [];
  titleData.forEach((e, i) => {
    headerRows.push(<th key={`title-${i}`}>{e.columnTitle}</th>);
  });
  let bodyRows = props.bannerList.map((_data, d_idx) => {
    let arr = titleData.map((element, t_idx) => {
      let body_content = null;
      if (element.body && element.body.SpecialItem) {
        if (element.body.SpecialItem === SpecialItemType.actionBtn) {
          body_content = (
            <ActionButtons clickEdit={props.ClickEdit.bind(null, d_idx)} />
          );
        } else if (element.body.SpecialItem === SpecialItemType.statusBtn) {
          body_content = (
            <StatusBtn
              mainKey={`${_data.siteId}-${_data.bannerName}`}
              checked={_data[element.dataColumn] === 1}
              onChange={props.UpdateBannerStatus.bind(null, d_idx)}
            />
          );
        }
      } else {
        body_content =
          element.body && element.body.format
            ? element.body.format(_data[element.dataColumn])
            : _data[element.dataColumn];
      }
      return <td key={`td-${t_idx}`}>{body_content}</td>;
    });

    return <tr key={`body-${d_idx}`}>{arr}</tr>;
  });
  return (
    <>
      <button
        className="btn btn-add"
        style={{ marginBottom: "20px" }}
        onClick={props.ClickCreateBanner}
      >
        Create Banner
      </button>
      <div className="table">
        <table className="table table-striped">
          <thead>
            <tr>{headerRows}</tr>
          </thead>
          <tbody>{bodyRows}</tbody>
        </table>
      </div>
    </>
  );
};
export default Panel;
