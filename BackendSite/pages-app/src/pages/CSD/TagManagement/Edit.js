import React, { useEffect, useRef, useReducer, useState } from "react";
import moment from "moment";
import { SketchPicker } from "react-color";

const Panel = (props) => {
  console.log(props);
  return (
    <div className="filter">
      <div>
        <InputField
          columnTitle="TagName"
          defaultValue={props.EditData.TagName}
          dataColumn="TagName"
          value={props.EditData.TagName}
          onChange={props.onChange}
        />
      </div>
      <div>
        <span className="label">TagColor</span>
        <SketchPicker
          color={props.EditData.TagColour}
          onChangeComplete={props.ChangeColor}
        />
      </div>
      <div>
        <TextInputBox
          Title="Description"
          ClassName="form-group"
          inputClass="form-control"
          OnChange={props.onChange}
          dataColumn="Description"
          value={props.EditData.Description}
        />
      </div>
      <div>
        <button
          className="btn btn-search"
          type="submit"
          onClick={props.Submit}
          style={{
            marginTop: "20px",
            marginBottom: "40px",
            marginRight: "10px",
          }}
        >
          Submit
        </button>
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
const InputField = (props) => {
  const onChange = (e) => {
    if (props.onChange) {
      props.onChange(e.target.value, props.dataColumn);
    }
  };
  return (
    <div className="input-item">
      <span className="label">{props.columnTitle}</span>
      <div className="field">
        <input
          type="text"
          defaultValue={props.defaultValue}
          onChange={onChange}
        />
      </div>
    </div>
  );
};

export default Panel;
