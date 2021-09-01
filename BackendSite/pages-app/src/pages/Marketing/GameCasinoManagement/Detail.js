import React, { useEffect, useRef, useReducer, useState } from "react";
import WebAPI from "../../../module/WebAPI";
import { api } from "../../../module/WebAPI";
import moment from "moment";

const dataColumnDefs = () => {
  return [
    { dataColumn: "hot", columnTitle: "Hot" },
    { dataColumn: "featured", columnTitle: "Featured" },
    { dataColumn: "status", columnTitle: "Status" },
    { dataColumn: "promotion", columnTitle: "Jackpot" },
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

  let body = props.DetailSearchData.map((siteData) => {
    let currency = siteData.gameCasinoDetails.map((data, idx) => {
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
              <div className="collapse show" id={`collapseExample-${siteData.siteId}-${idx}`}>
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
                                id={`switch-hot${siteData.siteId}-${idx}`}
                                className="switch switch--flat"
                                checked={data.hot === 1}
                                type="checkbox"
                                onChange={() => handleToggle("hot", data)}
                                disabled={false}
                              />
                              <label htmlFor={`switch-hot${siteData.siteId}-${idx}`}></label>
                            </div>
                          ) : null}
                        </td>
                        <td>
                          {data.featured === 1 ? "Yes" : "No"}
                          {window.subFuncKeys.includes("gcm_edit_detail") ? (
                            <div>
                              <input
                                id={`switch-featured${siteData.siteId}-${idx}`}
                                className="switch switch--flat"
                                checked={data.featured === 1}
                                type="checkbox"
                                onChange={() => handleToggle("featured", data)}
                                disabled={false}
                              />
                              <label htmlFor={`switch-featured${siteData.siteId}-${idx}`}></label>
                            </div>
                          ) : null}
                        </td>
                        <td>
                          {data.status === 1 ? "Yes" : "No"}
                          {window.subFuncKeys.includes("gcm_edit_detail") ? (
                            <div>
                              <input
                                id={`switch-status${siteData.siteId}-${idx}`}
                                className="switch switch--flat"
                                checked={data.status === 1}
                                type="checkbox"
                                onChange={() => handleToggle("status", data)}
                                disabled={false}
                              />
                              <label htmlFor={`switch-status${siteData.siteId}-${idx}`}></label>
                            </div>
                          ) : null}
                        </td>
                        <td>
                          {data.promotion === 1 ? "Yes" : "No"}
                          {window.subFuncKeys.includes("gcm_edit_detail") ? (
                            <div>
                              <input
                                id={`switch-promotion${siteData.siteId}-${idx}`}
                                className="switch switch--flat"
                                checked={data.promotion === 1}
                                type="checkbox"
                                onChange={() => handleToggle("promotion", data)}
                                disabled={false}
                              />
                              <label htmlFor={`switch-promotion${siteData.siteId}-${idx}`}></label>
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
          </div>
        </div>
      );
    });
    return (
      <div key={`Site-${siteData.siteId}`} className="panel mt-20">
        <a
          className="panel-title"
          data-toggle="collapse"
          style={{ color: "white" }}
        >
          {siteData.siteName}
        </a>
        <div className="panel-body">{currency}</div>
      </div>
    );
  });
  return <>{body}</>;
};

export default Panel;
