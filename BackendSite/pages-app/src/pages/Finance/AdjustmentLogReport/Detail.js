import React, { useEffect, useRef, useReducer, useState } from "react";
import moment from "moment";

const Panel = (props) => {
  return (
    <div className="filter">
      <div className="row-flex">
        <InputField
          columnTitle="Id"
          Value={props.DetailData.Id}
          dataColumn="Id"
          defaultValue=""
          Disabled={true}
        />
      </div>
      <div className="row-flex">
        <InputField2
          columnTitle="Menu"
          Value={props.DetailData.Menu}
          dataColumn="Menu"
          defaultValue=""
          Disabled={true}
          Size={400}
        />
      </div>
      <div className="row-flex">
        <InputField
          columnTitle="ActionType"
          Value={props.DetailData.ActionType === 1 ? "UPDATE/CREATE" : "DELETE"}
          dataColumn="ActionType"
          defaultValue=""
          Disabled={true}
        />
      </div>
      <div className="row-flex">
        <InputField2
          columnTitle="CreateTime"
          Value={moment(props.DetailData.CreateTime).format("l LTS")}
          dataColumn="CreateTime"
          defaultValue=""
          Disabled={true}
          Size={400}
        />
      </div>
      <div className="row-flex">
        <InputField
          columnTitle="User"
          Value={props.DetailData.Operator}
          dataColumn="User"
          defaultValue=""
          Disabled={true}
        />
      </div>
      <div>
        <TextInputBox
          Title="DetailInfo"
          ClassName="form-group"
          inputClass="form-control"
          dataColumn="DetailInfo"
          value={props.DetailData.DetailInfo}
        />
      </div>
      <button
        className="btn btn-search"
        type="submit"
        onClick={props.Back}
        style={{
          marginTop: "20px",
          marginBottom: "40px",
          marginRight: "10px",
        }}
      >
        Back
      </button>
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
          disabled={props.Disabled === true}
          className={props.inputClass ? props.inputClass : ""}
        />
      </div>
    </div>
  );
};
export default Panel;
