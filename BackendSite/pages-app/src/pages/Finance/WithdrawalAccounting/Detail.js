import React, { useEffect, useRef, useReducer, useState } from "react";
import WebAPI from "../../../module/WebAPI";
import { api } from "../../../module/WebAPI";
import moment from "moment";
import Popout from "./PicturePopout";
import {
  number_format,
  number_toFixed,
} from "../../../module/Formatting/NumberFormat";

const dataColumnDefs = () => {
  return [
    { dataColumn: "seq", columnTitle: "Seq" },
    { dataColumn: "bankAccountNo", columnTitle: "Bank AccountNo." },
    {
      dataColumn: "amount",
      columnTitle: "Amount",
      body: { format: numberFormat },
    },
    {
      dataColumn: "chargeFee",
      columnTitle: "ChargeFee",
      body: { format: numberFormat },
    },
    { dataColumn: "memo", columnTitle: "Memo" },
    { dataColumn: "creatorName", columnTitle: "Creator" },
    {
      dataColumn: "createTime",
      columnTitle: "CreateTime" + " (" + moment().format("Z") + ")",
      body: {
        format: dateFormat,
      },
    },
    { dataColumn: "operatorName", columnTitle: "Operator" },
    {
      dataColumn: "operateTime",
      columnTitle: "OperateTime" + " (" + moment().format("Z") + ")",
      body: {
        format: dateFormat,
      },
    },
    { dataColumn: "action", columnTitle: "Action" },
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
  let bodyRows = props.AccountingData.map((data, dataidx) => {
    let row = columnDefs.map((element, idx) => {
      let subData = {};
      if (element.body && element.body.subData) {
        element.body.subData.forEach((s) => {
          subData[s] = data[s];
        });
      }
      return element.dataColumn === "action" ? (
        <td key={`td-${element.dataColumn}-${idx}`}>
          <div>
            <div className="button-action">
              <button
                className="btn btn-edit"
                onClick={props.GetById.bind(this, data.id)}
              >
                <i className="icon-edit"></i>
              </button>
              <button
                className="btn btn-approved"
                type="submit"
                onClick={props.PopOut_Open.bind(this, data)}
              >
                Pic
              </button>
              <button
                className="btn btn-delete"
                onClick={props.Delete.bind(this, data)}
              >
                <i className="icon-delete"></i>
              </button>
            </div>
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

    return <tr key={`accounting-body-${dataidx}`}>{row}</tr>;
  });

  let body = props.AccountingData.map((data, idx) => {
    return (
      <div key={`CurrencyInfo-${data.currencyName}`} className="panel mt-20">
        {/* <a
          className="panel-title"
          data-toggle="collapse"
          style={{ color: "white" }}
        >
          {data.currencyName}
        </a>
        <div className="panel-body">
          {data.hasDetail ? (
            <div className="collapse show" id={`collapseExample-${idx}`}>
              <div className="table">
                <table className="table table-striped">
                  <thead>
                    <tr>{headerRows}</tr>
                  </thead>
                  <tbody>
                    <tr>
                      <td>
                        {data.hot === 1 ? "Yes" : "No"}
                        {window.subFuncKeys.includes("gcm_edit_detail") ? (
                          <div>
                            <input
                              id={`switch-hot${idx}`}
                              className="switch switch--flat"
                              checked={data.hot === 1}
                              type="checkbox"
                              onChange={() => handleToggle("hot", data)}
                              disabled={false}
                            />
                            <label htmlFor={`switch-hot${idx}`}></label>
                          </div>
                        ) : null}
                      </td>
                      <td>
                        {data.featured === 1 ? "Yes" : "No"}
                        {window.subFuncKeys.includes("gcm_edit_detail") ? (
                          <div>
                            <input
                              id={`switch-featured${idx}`}
                              className="switch switch--flat"
                              checked={data.featured === 1}
                              type="checkbox"
                              onChange={() => handleToggle("featured", data)}
                              disabled={false}
                            />
                            <label htmlFor={`switch-featured${idx}`}></label>
                          </div>
                        ) : null}
                      </td>
                      <td>
                        {data.status === 1 ? "Yes" : "No"}
                        {window.subFuncKeys.includes("gcm_edit_detail") ? (
                          <div>
                            <input
                              id={`switch-status${idx}`}
                              className="switch switch--flat"
                              checked={data.status === 1}
                              type="checkbox"
                              onChange={() => handleToggle("status", data)}
                              disabled={false}
                            />
                            <label htmlFor={`switch-status${idx}`}></label>
                          </div>
                        ) : null}
                      </td>
                      <td>
                        {data.promotion === 1 ? "Yes" : "No"}
                        {window.subFuncKeys.includes("gcm_edit_detail") ? (
                          <div>
                            <input
                              id={`switch-promotion${idx}`}
                              className="switch switch--flat"
                              checked={data.promotion === 1}
                              type="checkbox"
                              onChange={() => handleToggle("promotion", data)}
                              disabled={false}
                            />
                            <label htmlFor={`switch-promotion${idx}`}></label>
                          </div>
                        ) : null}
                      </td>
                    </tr>
                  </tbody>
                </table>
                {window.subFuncKeys.includes("gcm_edit_detail") ? (
                  <div style={{ margin: "5px" }}>
                    <button
                      className="btn btn-search"
                      type="submit"
                      onClick={props.Delete.bind(null, data)}
                    >
                      Delete
                    </button>
                  </div>
                ) : null}
              </div>
            </div>
          ) : (
            <div>
              {window.subFuncKeys.includes("gcm_edit_detail") ? (
                <button
                  className="btn btn-search"
                  type="submit"
                  onClick={props.Create.bind(null, data)}
                >
                  Create
                </button>
              ) : null}
            </div>
          )}
        </div> */}
      </div>
    );
  });

  return (
    <div className="table">
      <table className="table table-striped">
        <thead>
          <tr>{headerRows}</tr>
        </thead>
        <tbody>{bodyRows}</tbody>
      </table>
      <Popout
        data={props.PopOutData}
        handleClose={(e) => props.PopOut_Close()}
        show={props.PopOutStatus}
      />
    </div>
  );
};

const numberFormat = (number) => {
  return number_format(number_toFixed(number, 0));
};

const dateFormat = (datetime) => {
  return moment(datetime).format("l LTS");
};

export default Panel;
