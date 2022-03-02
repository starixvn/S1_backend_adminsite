import React from "react";
import WebAPI from "../../../module/WebAPI";
import { api } from "../../../module/WebAPI";
import Pagination from "./Pagination";

const dataColumnDefs = (CurrencyList) => {
  if (CurrencyList != null) {
    return [
      { dataColumn: "gameId", columnTitle: "GameId" },
      { dataColumn: "eName", columnTitle: "Ename" },
      { dataColumn: "cName", columnTitle: "Cname" },
      { dataColumn: "operator", columnTitle: "LastOperator" },
      ...CurrencyList.map((x) => {
        return {
          dataColumn: x,
          columnTitle: window.i18n[`lbl_Currency${x}`],
        };
      }),
      {
        dataColumn: "updateTime",
        columnTitle: "LastUpdateTime",
        body: {
          format: dateFormat,
        },
      },
    ];
  } else {
    return [];
  }
};
const Panel = (props) => {
  let CurrencyList = null;
  props.SiteList.forEach((x) => {
    if (parseInt(x.siteId, 10) == parseInt(props.BackupFilter.Site, 10)) {
      CurrencyList = x.iCurrencyList;
    }
  });

  let headerRows = [];
  let columnDefs = [];
  columnDefs = dataColumnDefs(CurrencyList);
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
      let currencyStatusList = Object.assign(
        {},
        ...data.currencyStatusList.map((x) => ({ [x.currencyId]: x.status }))
      );
      return element.dataColumn === "gameId" ||
        element.dataColumn === "eName" ||
        element.dataColumn === "cName" ||
        element.dataColumn === "operator" ||
        element.dataColumn === "updateTime" ? (
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
      ) : (
        <td key={`td-${element.dataColumn}-${idx}-${props.BackupFilter.Site}`}>
          <div className="switch__container">
            <input
              id={`switch-${element.dataColumn + dataidx}`}
              className="switch switch--flat"
              checked={currencyStatusList[element.dataColumn]}
              type="checkbox"
              onChange={() =>
                handleToggle(
                  element.dataColumn,
                  data,
                  props.BackupFilter.Site,
                  props.UpdateResultStatus,
                  currencyStatusList[element.dataColumn]
                )
              }
              disabled={
                window.subFuncKeys &&
                window.subFuncKeys.includes("cms_sgm_edit")
                  ? false
                  : true
              }
            />
            <label htmlFor={`switch-${element.dataColumn + dataidx}`}></label>
          </div>
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

const handleToggle = (type, value, site, UpdateResultStatus, status) => {
  if (window.confirm(`Are you sure change ${value.eName}-${window.i18n[`lbl_Currency${type}`]} ?`)) {
    let Data = {
      CurrencyId: type,
      SlotGameId: value.id,
      SiteId: site,
      Status: status == 0 ? 1 : 0,
    };
    api
      .post("/api/SlotGamesManagement/SetSlotGame", Data)
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
