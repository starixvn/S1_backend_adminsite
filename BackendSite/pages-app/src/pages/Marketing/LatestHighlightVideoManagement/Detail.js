import React, { useEffect, useRef, useReducer, useState } from "react";
import WebAPI from "../../../module/WebAPI";
import { api } from "../../../module/WebAPI";
import moment from "moment";
import DateTimePicker from "react-datetime";

const Panel = (props) => {
  return (
    <div className="filter">
      <div className="row-flex">
        <InputField
          columnTitle="SiteName"
          Value={props.AlterDatas.SiteName}
          dataColumn="SiteName"
          defaultValue=""
          Disabled={true}
        />
        <InputField
          columnTitle="LangId"
          Value={props.AlterDatas.LangId}
          dataColumn="LangId"
          defaultValue=""
          Disabled={true}
        />
      </div>
      <div className="row-flex">
        <DateTimePickerElem
          columnTitle="Date"
          dateTimePickerProps={{
            defaultValue: moment(props.AlterDatas.Date),
            timeFormat: false,
            onBlur: (e) => {
              try {
                props.FiterOnChange(moment(e).format("l"), "Date");
              } catch (ex) {}
            },
          }}
        />
      </div>
      <div className="row-flex">
        <InputField2
          columnTitle="Title"
          Value={props.AlterDatas.Title}
          dataColumn="Title"
          defaultValue=""
          onChange={props.FiterOnChange}
          Disabled={false}
          Size={500}
        />
      </div>
      <div className="row-flex">
        <InputField2
          columnTitle="Link"
          Value={props.AlterDatas.Link}
          dataColumn="Link"
          defaultValue=""
          onChange={props.FiterOnChange}
          Disabled={false}
          Size={500}
        />
      </div>
      <div className="row-flex">
        <font color="red">
          Youtube video need use embed url. <br></br>
          Sample:
          https://www.youtube.com/embed/m5_QAn0fHzY?start=2 (start=2 means the
          video will start from 2s.) <br></br>
          Refer:
          https://support.google.com/youtube/answer/171780?hl=en
        </font>
      </div>
      <div className="button-group">
        <button
          className="btn btn-search"
          type="submit"
          style={{ marginTop: "20px", marginBottom: "40px" }}
          onClick={props.Update}
        >
          Update
        </button>
        <button
          className="btn btn-search"
          type="submit"
          style={{ marginTop: "20px", marginBottom: "40px" }}
          onClick={props.Back}
        >
          Back
        </button>
      </div>
    </div>
  );
};

const DateTimePickerElem = (props) => {
  return (
    <div className="input-item">
      <span className="label">{props.columnTitle}</span>
      <DateTimePicker {...props.dateTimePickerProps} />
    </div>
  );
};

const CheckBoxElem = ({
  Title,
  Options,
  Selected = [],
  OnChange,
  OnChangeAll,
  dataColumn,
  mainKey,
}) => {
  let opt = Options.map((o) => {
    return (
      <CheckBox
        key={`${Title}-${o.value}${mainKey === null ? "" : `-${mainKey}`}`}
        Options={null}
        onChange={OnChange}
        Title={o.displayText}
        checked={Selected.includes(o.value)}
        Value={o.value}
        dataColumn={dataColumn}
      />
    );
  });
  return (
    <div className="input-item" style={{ width: "auto" }}>
      <label className="label">{Title}</label>
      <CheckBox
        onChange={OnChangeAll}
        Title="All"
        Options={Options}
        checked={Selected.length === Options.length && Selected.length > 0}
        Value="All"
        dataColumn={dataColumn}
      />
      {opt}
    </div>
  );
};
const CheckBox = (props) => {
  const OnChange = (e) => {
    props.onChange(props.Value, props.dataColumn, props.Options);
  };
  return (
    <label className="checkbox checkbox-checked">
      <input
        type="checkbox"
        style={{ width: "0" }}
        value={props.Value}
        checked={props.checked}
        onChange={OnChange}
      />
      <i className="icon icon-checkbox"></i>
      <span className="text">{props.Title}</span>
    </label>
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
export default Panel;
