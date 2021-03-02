import React, { useEffect, useRef, useReducer, useState } from "react";
import WebAPI from "../../../module/WebAPI";
import { api } from "../../../module/WebAPI";
import moment from "moment";

const dataColumnDefs = () => {
  return [
    { dataColumn: "hot", columnTitle: "Hot" },
    { dataColumn: "featured", columnTitle: "Featured" },
    { dataColumn: "status", columnTitle: "Status" },
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
  let body = props.DetailSearchData.map((data, idx) => {
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
                        {data.hot === 1 ? "Yes" : "No"}
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
                      </td>
                      <td>
                        {data.featured === 1 ? "Yes" : "No"}
                        <div>
                          <input
                            id={`switch-featured${idx}`}
                            className="switch switch--flat"
                            checked={data.featured === 1}
                            type="checkbox"
                            onChange={() =>
                                handleToggle("featured", data)
                            }
                            disabled={false}
                          />
                          <label htmlFor={`switch-featured${idx}`}></label>
                        </div>
                      </td>
                      <td>
                        {data.status === 1 ? "Yes" : "No"}
                        <div>
                          <input
                            id={`switch-status${idx}`}
                            className="switch switch--flat"
                            checked={data.status === 1}
                            type="checkbox"
                            onChange={() =>
                                handleToggle("status", data)
                            }
                            disabled={false}
                          />
                          <label htmlFor={`switch-status${idx}`}></label>
                        </div>
                      </td>
                    </tr>
                  </tbody>
                </table>
                <div style={{ margin: "5px" }}>
                  <button
                    className="btn btn-search"
                    type="submit"
                    onClick={props.Delete.bind(null, data)}
                  >
                    Delete
                  </button>
                </div>
              </div>
            </div>
          ) : (
            <div>
              <button
                className="btn btn-search"
                type="submit"
                onClick={props.Create.bind(null, data)}
              >
                Create
              </button>
            </div>
          )}
        </div>
      </div>
    );
  });
  return <>{body}</>;
};

export default Panel;
