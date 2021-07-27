import React from "react";
import Pagination from "./Pagination";
import moment from "moment-timezone";
import { api } from "../../../module/WebAPI";
const SpecialItemType = {
  statusBtn: "statusBtn",
};
const dataColumnDefs = () => {
  return [
    { dataColumn: "tagName", columnTitle: "TagName" },
    { dataColumn: "tagColour", columnTitle: "TagColour" },
    { dataColumn: "description", columnTitle: "Description" },
    { dataColumn: "updateTime", columnTitle: "UpdateTime (Adjusted to GMT+8)" },
    { dataColumn: "updator", columnTitle: "Updator" },
    { dataColumn: "status", columnTitle: "Status" },
    {
      dataColumn: "edit",
      columnTitle: "Edit",
      body: { SpecialItem: SpecialItemType.actionBtn },
    },
  ];
};

const Panel = (props) => {
  const columnDefs = dataColumnDefs();
  let headerRows = [];

  columnDefs.forEach((element, idx) => {
    headerRows.push(
      <th
        key={`th-${element.dataColumn}-${idx}`}
        scope="col"
        className={
          element.header && element.header.attr && element.header.attr.class
            ? element.header.attr.class
            : ""
        }
      >
        {element.columnTitle}
      </th>
    );
  });
  let bodyRows = props.Datas.map((data, dataidx) => {
    let row = columnDefs.map((element, idx) => {
      let subData = {};
      if (element.body && element.body.subData) {
        element.body.subData.forEach((s) => {
          subData[s] = data[s];
        });
      }
      return element.dataColumn === "status" ? (
        <td key={`td-${element.dataColumn}-${idx}`}>
          {data[element.dataColumn] ? "Yes" : "No"}
          <div className="switch__container">
            <input
              id={`switch-${element.dataColumn + dataidx}`}
              className="switch switch--flat"
              checked={data[element.dataColumn]}
              type="checkbox"
              // onChange={() => handleToggle(data, props.Resubmit)}
              onChange = {props.StatusChange.bind(null,data)}
              disabled={false}
            />
            <label htmlFor={`switch-${element.dataColumn + dataidx}`}></label>
          </div>
        </td>
      ) : element.dataColumn === "edit" ? (
        <td key={`td-${element.dataColumn}-${idx}`}>
          <ActionButtons clickEdit={props.Detail.bind(null, data)} />
        </td>
      ) : element.dataColumn === "tagColour" ? (
        <td bgcolor={data[element.dataColumn]}></td>
      ) : (
        <td
          key={`td-${element.dataColumn}-${idx}`}
          className={
            element.body && element.body.attr && element.body.attr.class
              ? element.body.attr.class
              : ""
          }
        >
          {element.body && element.body.format
            ? element.body.format(data[element.dataColumn], subData)
            : data[element.dataColumn]}
        </td>
      );
    });
    return <tr key={`result-body-${dataidx}`}>{row}</tr>;
  });

  return (
    <div className="table">
      <table className="table table-striped">
        <thead>
          <tr>{headerRows}</tr>
        </thead>
        <tbody>{bodyRows}</tbody>
      </table>
      <Pagination
        Data={{
          totalPages: props.PageInfo.totalPages,
          pageNumber: props.PageInfo.pageNumber,
          rowCount: props.PageInfo.rowCount,
        }}
        ChangePage={props.ChangePage}
      />
    </div>
  );
};
const handleToggle = (value, Resubmit) => {
  if (window.confirm(`Are you sure change ${value.tagName} ?`)) {
    let Data = {
      Id: value.id,
    };
    api
      .post("/api/CustManage/SetTagStatus", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          Resubmit();
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  }
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

const dateFormat = (createTime) => {
  return moment(createTime).format("l LTS");
};
export default Panel;
