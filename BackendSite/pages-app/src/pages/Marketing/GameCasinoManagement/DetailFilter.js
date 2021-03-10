import React, { useEffect, useRef, useReducer, useState } from "react";
import WebAPI from "../../../module/WebAPI";
import { api } from "../../../module/WebAPI";
import moment from "moment";

const Panel = (props) => {
  return (
    <div className="filter">
      <div className="row-flex"></div>
      <div>
        {props.SiteList === null ? null : (
          <RadioBox
            Title="Site"
            Options={Object.values(props.SiteList)}
            OnChange={props.DetailFilterChange}
            dataColumn="Site"
            Selected={props.DetailFilter}
          />
        )}
      </div>
      <div>
        {props.StateTPList === null ? null : (
          <button
            className="btn btn-add"
            onClick={props.Submit}
            style={{
              marginTop: "20px",
              marginBottom: "40px",
              marginRight: "10px",
            }}
          >
            Submit
          </button>
        )}
        <button
          className="btn btn-search"
          type="back"
          onClick={props.Goback}
          style={{
            marginTop: "20px",
            marginBottom: "40px",
            marginRight: "10px",
          }}
        >
          Back
        </button>
      </div>
      <div className="table">
        <table className="table table-striped">
          <thead>
            <tr>
              <th>Ename</th>
              <th>Cname</th>
              <th>Hot</th>
              <th>Featured</th>
              <th>Status</th>
              <th>Promotion</th>
            </tr>
          </thead>
          <tbody>
            <tr>
              <td>{props.DetailData.ename}</td>
              <td>{props.DetailData.cname}</td>
              <td>{props.DetailData.hot ? "Yes" : "No"}</td>
              <td>{props.DetailData.featured ? "Yes" : "No"}</td>
              <td>{props.DetailData.status ? "Yes" : "No"}</td>
              <td>{props.DetailData.promotion ? "Yes" : "No"}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  );
};
const RadioBox = ({
  Title,
  Options,
  Selected,
  OnChange,
  dataColumn,
  mainKey = null,
}) => {
  let opt = Options.map((o, oIdx) => {
    let key = `${Title}-${o.value}${mainKey === null ? "" : `-${mainKey}`}`;
    return (
      <RadioBtn
        key={key}
        Id={key}
        DisplayText={o.displayText}
        Value={`${o.value}${mainKey === null ? "" : `-${mainKey}`}`}
        dataColumn={dataColumn}
        Name={`${Title}${mainKey === null ? "" : `-${mainKey}`}`}
        OnChange={OnChange.bind(this, o.value, dataColumn, mainKey, oIdx)}
        Checked={Selected === o.value}
      />
    );
  });
  return (
    <>
      <div className="row-flex">
        <div className="input-item">
          <label className="label">{Title}</label>
        </div>
      </div>
      <div className="row-flex">{opt}</div>
    </>
  );
};
const RadioBtn = ({
  Id,
  DisplayText,
  Value,
  Name,
  OnChange,
  Checked,
  Disabled,
}) => {
  return (
    <div className="custom-control custom-radio custom-control-inline">
      <input
        type="radio"
        id={Id}
        name={Name}
        className="custom-control-input revert-style"
        value={Value}
        onChange={OnChange}
        checked={Checked}
        disabled={Disabled}
      />
      <label className="custom-control-label" htmlFor={Id}>
        {DisplayText}
      </label>
    </div>
  );
};

export default Panel;
