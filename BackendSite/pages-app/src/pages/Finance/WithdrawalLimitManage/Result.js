import React, { useEffect, useRef, useReducer, useState } from "react";
import { PopBoxContentEdit } from "./PopBoxContentEdit";
import {
  number_format,
  number_toFixed,
} from "../../../module/Formatting/NumberFormat";

const dataColumnDefs = () => {
  return [
    {
      dataColumn: "withdrawalMin",
      columnTitle: "WithdrawalMin"
    },
    { dataColumn: "action", columnTitle: "Action" },
  ];
};
const Panel = (props) => {
  const columnDefs = dataColumnDefs();
  let headerRows = [];
  const handleToggle = (type, value) => {
    props.Update(type, value);
  };
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
  let body = props.Data.map((data, idx) => {
    let subData = {};
    return (
      <div key={`CurrencyInfo-${data.currencyName}`} className="panel mt-20">
        <a
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
                        <div
                          className="field"
                          //style={{ marginBottom: "20px", marginLeft: "20px" }}
                        >
                          {data.withdrawalMin}
                        </div>
                      </td>
                      <td>
                        <button
                          className="btn btn-search"
                          type="submit"
                          onClick={props.Edit.bind(null, data)}
                          style={{
                            marginRight: "10px",
                          }}
                        >
                          Edit
                        </button>
                        <button
                          className="btn btn-search"
                          type="submit"
                          onClick={props.Deleta.bind(null, data)}
                          style={{
                            marginRight: "10px",
                          }}
                        >
                          Delete
                        </button>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          ) : (
            <button
              className="btn btn-search"
              type="submit"
              onClick={props.Create.bind(null, data)}
            >
              Create
            </button>
          )}
        </div>
      </div>
    );
  });
  return <>{body}</>;
};
const InputField = (props) => {
  const EditField = (e) => {
    if (props.onChange) {
      props.onChange(e.target.value, props.dataColumn);
    } else if (props.onBlur) {
      props.onBlur(e.target.value, props.dataColumn);
    }
  };
  return (
    <div className="input-item">
      <span className="label">{props.columnTitle}</span>
      <input
        className="form-control"
        value={props.Value}
        onChange={EditField}
        onBlur={EditField}
      />
    </div>
  );
};
const numberFormat = (number) => {
  return number_format(number_toFixed(number, 0));
};
export default Panel;
