import Pagination from "./Pagination";
import React, { useEffect, useRef, useReducer, useState } from "react";
import WebAPI from "../../../module/WebAPI";
import { api } from "../../../module/WebAPI";
import moment from "moment";

const Panel = (props) => {
  console.log(props.EditData);
  return (
    <div className="filter">
      <div className="row-flex">
        <InputField
          columnTitle="Id"
          Value={props.EditData.Id}
          dataColumn="Id"
          defaultValue=""
          Disabled={true}
        />
      </div>
      <div className="row-flex">
        <InputField2
          columnTitle="FuncKey"
          Value={props.EditData.FuncKey}
          dataColumn="FuncKey"
          defaultValue=""
          Disabled={true}
          Size={400}
        />
      </div>
      <div className="row-flex">
        <InputField
          columnTitle="ReasonId"
          Value={props.EditData.ReasonId}
          dataColumn="ReasonId"
          defaultValue=""
          Disabled={true}
        />
      </div>
      <div>
        <TextInputBox
          Title="Description"
          ClassName="form-group"
          inputClass="form-control"
          OnChange={props.Change}
          dataColumn="Description"
          value={props.EditData.Description}
        />
      </div>
      <div className="row-flex">
        <InputField
          columnTitle="Creator"
          Value={props.EditData.Creator}
          dataColumn="Creator"
          defaultValue=""
          Disabled={true}
        />
      </div>
      <div className="row-flex">
        <InputField2
          columnTitle={"CreateTime(" + moment().format('Z') + ")"}
          Value={moment(props.EditData.CreateTime).format('l LTS')}
          dataColumn="CreateTime"
          defaultValue=""
          Disabled={true}
          Size={300}
        />
      </div>
      <div className="row-flex">
        <InputField
          columnTitle="Updator"
          Value={props.EditData.Updator}
          dataColumn="Updator"
          defaultValue=""
          Disabled={true}
        />
      </div>
      <div className="row-flex">
        <InputField2
          columnTitle={"UpdateTime(" + moment().format('Z') + ")"}
          Value={moment(props.EditData.UpdateTime).format('l LTS')}
          dataColumn="UpdateTime"
          defaultValue=""
          Disabled={true}
          Size={300}
        />
      </div>
    </div>
  );
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
        disabled={props.Disabled}
      />
    </div>
  );
};
const InputField2 = (props) => {
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
        disabled={props.Disabled}
        style={{ width: `${props.Size}px` }}
      />
    </div>
  );
};

const TextInputBox = (props) => {
  const OnChange = (e) => {
    let value = e.target.value;
    let checked =
      value === "" || props.regex === undefined
        ? true
        : value.match(props.regex);
    if (checked) {
      props.OnChange(value, props.dataColumn);
    }
  };
  return (
    <div className={props.ClassName ? props.ClassName : "input-item"}>
      {props.HideTitle === true ? null : (
        <label className="label">{props.Title}</label>
      )}
      <div
        className={`field${props.iptSubClass ? " " + props.iptSubClass : ""}`}
      >
        {props.SubTitle ? (
          <span className="bule-bold">{props.SubTitle}</span>
        ) : null}
        <textarea
          type={"text"}
          rows="8"
          value={
            props.value === null || props.value === undefined ? "" : props.value
          }
          onChange={OnChange}
          disabled={props.Disabled === true}
          className={props.inputClass ? props.inputClass : ""}
        />
      </div>
    </div>
  );
};
export default Panel;
