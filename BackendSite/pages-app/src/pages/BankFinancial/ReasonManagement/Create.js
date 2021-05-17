import React, { useEffect, useRef, useReducer, useState } from "react";
import moment from "moment";

const defFuncKeyType = {
  0: {
    value: 0,
    displayText: "Old FuncKey",
  },
  1: {
    value: 1,
    displayText: "Create FuncKey",
  },
};

const initFuncKeyType = () => Object.values(defFuncKeyType);

const Panel = (props) => {
  return (
    <div className="filter">
      <div>
        <RadioBox
          Title="FuncKey Type"
          Options={initFuncKeyType()}
          OnChange={props.onChange}
          dataColumn="FuncKeyType"
          Selected={props.insertData.FuncKeyType}
        />
      </div>
      {props.insertData.FuncKeyType === 0 && props.Funckey !== null ? (
        <div>
          <DropDownBox
            columnTitle="Funckey"
            options={GetFunckeyoptions(props.Funckey)}
            hasAll={false}
            selectedValue={props.insertData.OldFuncKey}
            dataColumn="OldFuncKey"
            onChange={props.onChange}
          />
        </div>
      ) : null}
      {props.insertData.FuncKeyType === 1 ? (
        <div>
          <InputField2
            columnTitle="FuncKey"
            Value={props.insertData.NewFuncKey}
            dataColumn="NewFuncKey"
            defaultValue=""
            Disabled={false}
            Size={400}
            onChange={props.onChange}
          />
        </div>
      ) : null}
      <div>
        <TextInputBox
          Title="Description"
          ClassName="form-group"
          inputClass="form-control"
          OnChange={props.onChange}
          dataColumn="Description"
          value={props.insertData.Description}
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
const GetFunckeyoptions = (props) => {
  let result = [];
  result.push({ value: "0", displayText: "" });
  if (props !== null) {
    for (let i = 0; i <= props.length - 1; i++) {
      result.push({ value: props[i].funcKey, displayText: props[i].funcKey });
    }
  }
  return result;
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
const DropDownBox = (props) => {
  const onChange = (e) => {
    if (props.onChange) {
      props.onChange(e.target.value, props.dataColumn);
    }
  };
  let optionRows = props.options.map((item) => {
    return (
      <option key={`${props.ColumnTitle}-${item.value}`} value={item.value}>
        {item.displayText}
      </option>
    );
  });

  return (
    <div className="input-item">
      <span className="label">{props.columnTitle}</span>
      <div className="field">
        <select defaultValue={props.selectedValue} onChange={onChange}>
          {props.hasAll ? <option value="0">All</option> : null}
          {optionRows}
        </select>
      </div>
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
