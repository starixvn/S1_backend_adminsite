import React from "react";
import Pagination from "./Pagination";
import WebAPI from "../../../module/WebAPI";
import { api } from "../../../module/WebAPI";

const dataColumnDefs = () => {
  return [
    { dataColumn: "gameId", columnTitle: "GameId" },
    { dataColumn: "ename", columnTitle: "Ename" },
    { dataColumn: "cname", columnTitle: "Cname" },
    { dataColumn: "hot", columnTitle: "Hot" },
    { dataColumn: "featured", columnTitle: "Featured" },
    { dataColumn: "status", columnTitle: "Status" },
    { dataColumn: "detail", columnTitle: "Detail" },
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
      return element.dataColumn === "detail" ? (
        <td key={`td-${element.dataColumn}-${idx}`}>
          <div>
            <button
              className="btn btn-search"
              type="submit"
              onClick={props.Detail.bind(this, data)}
            >
              Detail
            </button>
          </div>
        </td>
      ) : element.dataColumn === "hot" ||
        element.dataColumn === "featured" ||
        element.dataColumn === "status" ? (
        <td key={`td-${element.dataColumn}-${idx}`}>
          {data[element.dataColumn] ? "Yes" : "No"}
          <div className="switch__container">
            <input
              id={`switch-${element.dataColumn + dataidx}`}
              className="switch switch--flat"
              checked={data[element.dataColumn]}
              type="checkbox"
              onChange={() => handleToggle(element.dataColumn, data,props.UpdateResultStatus)}
              disabled={!props.CanChangeGameCasino}
            />
            <label htmlFor={`switch-${element.dataColumn + dataidx}`}></label>
          </div>
        </td>
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

const handleToggle = (type, value,UpdateResultStatus) => {
  if (window.confirm(`Are you sure change ${value.ename}-${type} ?`)) {
    let Data = {
      GameCasinoId: value.gameCasinoId,
      Type: type
    };
    api
      .post("/api/GameCasinoManagement/SetGameCasino", Data)
      .then(function (res) {
        if (res.data.errorCode === 0) {
          UpdateResultStatus()
        } else {
          alert(res.data.message ?? "failed to get data.");
        }
      })
      .catch(function (error) {
        alert(error);
      });
  }
};

export default Panel;
