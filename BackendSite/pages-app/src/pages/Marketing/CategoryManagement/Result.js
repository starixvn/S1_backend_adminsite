import React from "react";
import WebAPI from "../../../module/WebAPI";
import { api } from "../../../module/WebAPI";
import Pagination from "./Pagination";

const dataNovaColumnDefs = () => {
  return [
    { dataColumn: "gameId", columnTitle: "GameId" },
    { dataColumn: "eName", columnTitle: "Ename" },
    { dataColumn: "cName", columnTitle: "Cname" },
    { dataColumn: "myr", columnTitle: "MYR" },
    { dataColumn: "thb", columnTitle: "THB" },
    { dataColumn: "rmb", columnTitle: "RMB" },
    { dataColumn: "idr", columnTitle: "IDR" },
    { dataColumn: "inr", columnTitle: "INR" },
    { dataColumn: "operator", columnTitle: "LastOperator" },
    {
      dataColumn: "updateTime",
      columnTitle: "LastUpdateTime",
      body: {
        format: dateFormat,
      },
    },
  ];
};
const dataGoalColumnDefs = () => {
  return [
    { dataColumn: "gameId", columnTitle: "GameId" },
    { dataColumn: "eName", columnTitle: "Ename" },
    { dataColumn: "cName", columnTitle: "Cname" },
    { dataColumn: "vnd", columnTitle: "VND" },
    { dataColumn: "operator", columnTitle: "LastOperator" },
    {
      dataColumn: "updateTime",
      columnTitle: "LastUpdateTime",
      body: {
        format: dateFormat,
      },
    },
  ];
};
const dataBwingColumnDefs = () => {
  return [
    { dataColumn: "gameId", columnTitle: "GameId" },
    { dataColumn: "eName", columnTitle: "Ename" },
    { dataColumn: "cName", columnTitle: "Cname" },
    { dataColumn: "vnd", columnTitle: "VND" },
    { dataColumn: "operator", columnTitle: "LastOperator" },
    {
      dataColumn: "updateTime",
      columnTitle: "LastUpdateTime",
      body: {
        format: dateFormat,
      },
    },
  ];
};
const dataSoonColumnDefs = () => {
  return [
    { dataColumn: "gameId", columnTitle: "GameId" },
    { dataColumn: "eName", columnTitle: "Ename" },
    { dataColumn: "cName", columnTitle: "Cname" },
    { dataColumn: "rmb", columnTitle: "RMB" },
    { dataColumn: "inr", columnTitle: "INR" },
    { dataColumn: "operator", columnTitle: "LastOperator" },
    {
      dataColumn: "updateTime",
      columnTitle: "LastUpdateTime",
      body: {
        format: dateFormat,
      },
    },
  ];
};

const Panel = (props) => {
  let headerRows = [];
  let columnDefs = [];
  switch (props.BackupFilter.Site) {
    case 1:
      columnDefs = dataNovaColumnDefs();
      break;
    case 2:
      columnDefs = dataGoalColumnDefs();
      break;
    case 3:
      columnDefs = dataBwingColumnDefs();
      break;
    case 4:
      columnDefs = dataSoonColumnDefs();
      break;
  }
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
      return element.dataColumn === "myr" ||
        element.dataColumn === "thb" ||
        element.dataColumn === "rmb" ||
        element.dataColumn === "idr" ||
        element.dataColumn === "inr" ||
        element.dataColumn === "vnd" ? (
        <td key={`td-${element.dataColumn}-${idx}-${props.BackupFilter.Site}`}>
          <div className="switch__container">
            <input
              id={`switch-${element.dataColumn + dataidx}`}
              className="switch switch--flat"
              checked={data[element.dataColumn]}
              type="checkbox"
              onChange={() =>
                handleToggle(
                  element.dataColumn,
                  data,
                  props.BackupFilter.Site,
                  props.BackupFilter.Category,
                  props.UpdateResultStatus,
                  data[element.dataColumn]
                )
              }
              disabled={
                props.BackupFilter.Category == 0
                  ? true
                  : window.subFuncKeys &&
                    window.subFuncKeys.includes("cms_cm_edit")
                  ? false
                  : true
              }
            />
            <label htmlFor={`switch-${element.dataColumn + dataidx}`}></label>
          </div>
        </td>
      ) : (
        <td
          key={`td-${element.dataColumn}-${idx}-${props.BackupFilter.Site}`}
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

const handleToggle = (
  type,
  value,
  site,
  category,
  UpdateResultStatus,
  status
) => {
  if (window.confirm(`Are you sure change ${value.eName}-${type} ?`)) {
    let currencyId = 0;
    switch (type) {
      case "myr":
        currencyId = 2;
        break;
      case "thb":
        currencyId = 4;
        break;
      case "rmb":
        currencyId = 13;
        break;
      case "idr":
        currencyId = 15;
        break;
      case "inr":
        currencyId = 61;
        break;
      case "vnd":
        currencyId = 51;
        break;
    }
    let Data = {
      CurrencyId: currencyId,
      SlotGameId: value.id,
      SiteId: site,
      CategoryId: category,
      Status: status == 0 ? 1 : 0,
    };
    api
      .post("/api/SlotGamesManagement/SetSlotGameCategory", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          UpdateResultStatus();
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  }
};
const dateFormat = (createTime) => {
  return moment(createTime).format("l LTS");
};
export default Panel;
