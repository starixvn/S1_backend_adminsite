import moment from "moment";
//import MonthYearPicker from "react-month-year-picker";
import React, { useState } from "react";
import DateTimePicker from "react-datetime";

const StatusType = {
  1: "Pending",
  2: "Approved",
  3: "Rejected",
};

const Panel = (props) => {
  const EditField = (v, dataColumn) => {
    props.SetFilter({
      ...props.StateFilter,
      [dataColumn]: v,
    });
  };
  let radio = "";
  let _site = [];
  if (props.SiteList.length === 0) {
    return <></>;
  } else {
    radio = props.SiteList.map((value) => {
      _site[value.siteId] = value.siteName;
      return (
        <div
          key={`SiteOpt-${value.siteId}`}
          className="custom-control custom-radio custom-control-inline"
        >
          <input
            type="radio"
            id={`SiteId_${value.siteId}`}
            name="SiteList"
            className="custom-control-input"
            value={value.siteId}
            onChange={(e) => {
              EditField(value.siteId, "optSite");
            }}
            disabled={false}
            checked={props.StateFilter.optSite === value.siteId}
          />
          <label
            className="custom-control-label"
            htmlFor={`SiteId_${value.siteId}`}
          >
            {value.siteName}
          </label>
        </div>
      );
    });
  }
  let StatusTypeList = Object.keys(StatusType).map((key) => {
    return {
      displayText: StatusType[key],
      value: key,
    };
  });
  return (
    <div className="filter">
      <div className="row-flex">
        <div className="form-group" style={{ marginTop: "20px" }}>
          <label className="col-form-label d-block">Site Name</label>
          {radio}
        </div>
        <div className="field" style={{ marginBottom: "20px" }}>
          <span className="label">YearMonth</span>
          <DateTimePicker
            value={props.StateFilter.YearMonth}
            dateFormat="YYYY-MM"
            timeFormat={false}
            onChange={(e) => {
              props.SetFilter({ ...props.StateFilter, YearMonth: e });
            }}
          />
        </div>
        <div
          className="field"
          style={{ marginBottom: "20px", marginLeft: "20px" }}
        >
          <DropDownBox
            columnTitle="Status"
            options={StatusTypeList}
            hasAll={true}
            selectedValue={props.StateFilter.Status}
            dataColumn="Status"
            onChange={props.StatusChange}
          />
        </div>
        <div className="field"
          style={{ marginBottom: "20px", marginLeft: "20px" }}>
          <InputField
            columnTitle="UserName"
            Value={props.StateFilter.UserName}
            dataColumn="UserName"
            onChange={props.UserNameChange}
            defaultValue=""
          />
        </div>
        <div className="field" style={{ marginBottom: "20px" }}>
          <button
            className="btn btn-search"
            type="submit"
            style={{ marginLeft: "20px" }}
            onClick={props.Submit}
          >
            Submit
          </button>
        </div>
      </div>
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

export default Panel;
