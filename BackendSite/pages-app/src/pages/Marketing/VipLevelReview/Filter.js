import moment from "moment";
import React, { useState } from "react";
import DateTimePicker from "react-datetime";

const VipType = {
  1: "Upgrade",
  2: "Downgrade",
};

const StatusType = {
  1: "Pending",
  2: "Approved",
  3: "Rejected",
  4: "Settled",
};

const Panel = (props) => {
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
              props.FilterOnChange(value.siteId, "SiteId");
            }}
            disabled={false}
            checked={props.StateFilter.SiteId === value.siteId}
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
  let vipRadio = Object.keys(VipType).map((key) => {
    return (
      <div
        key={`VipOpt-${key}`}
        className="custom-control custom-radio custom-control-inline"
      >
        <input
          type="radio"
          id={`VipType_${key}`}
          name="VipTypeList"
          className="custom-control-input"
          value={key}
          onChange={(e) => {
            props.FilterOnChange(parseInt(key), "VipType");
          }}
          disabled={false}
          checked={props.StateFilter.VipType === parseInt(key)}
        />
        <label className="custom-control-label" htmlFor={`VipType_${key}`}>
          {VipType[key]}
        </label>
      </div>
    );
  });

  let StatusTypeList = Object.keys(StatusType).map((key) => {
    return {
      displayText: StatusType[key],
      value: key,
    };
  });
  return (
    <div className="filter">
      <div className="row-flex">
        <div className="form-group">
          <label className="col-form-label d-block">VIP Type</label>
          {vipRadio}
        </div>
      </div>
      <div className="row-flex">
        <div className="form-group" style={{ marginTop: "20px" }}>
          <label className="col-form-label d-block">Site Name</label>
          {radio}
        </div>
        {props.StateFilter.VipType == 1 ? (
          <div className="field" style={{ marginBottom: "20px" }}>
            <span className="label">Year-Month</span>
            <DateTimePicker
              dataColumn="Upgrade_YearMonth"
              value={props.StateFilter.Upgrade_YearMonth}
              dateFormat="YYYY-MM"
              timeFormat={false}
              onChange={(e) => {
                props.FilterOnChange(e, "Upgrade_YearMonth");
              }}
            />
          </div>
        ) : (
          <div className="row-flex">
            <div className="field" style={{ marginBottom: "20px" }}>
              <span className="label">From Year-Month</span>
              <DateTimePicker
                dataColumn="Downgrade_FromYearMonth"
                value={props.StateFilter.Downgrade_FromYearMonth}
                dateFormat="YYYY-MM"
                timeFormat={false}
                onChange={(e) => {
                  props.FilterDowngradeYearMonthChange(
                    e,
                    new moment(e).add(2, "M")
                  );
                }}
              />
            </div>
            <div className="field" style={{ marginBottom: "20px" }}>
              <span className="label">To Year-Month</span>
              <DateTimePicker
                dataColumn="Downgrade_ToYearMonth"
                value={props.StateFilter.Downgrade_ToYearMonth}
                dateFormat="YYYY-MM"
                timeFormat={false}
                onChange={(e) => {
                  props.FilterDowngradeYearMonthChange(
                    new moment(e).add(-2, "M"),
                    e
                  );
                }}
              />
            </div>
          </div>
        )}
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
            onChange={props.FilterOnChange}
          />
        </div>
        <div
          className="field"
          style={{ marginBottom: "20px", marginLeft: "20px" }}
        >
          <InputField
            columnTitle="UserName"
            Value={props.StateFilter.UserName}
            dataColumn="UserName"
            onChange={props.FilterOnChange}
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
      <div className="row-flex">
        {props.StateFilter.VipType == 1 ? (
          <span style={{ color: "red", fontSize: "14px" }}>
            ATTENTION : Upgrade Setting is 1 months calculation.
          </span>
        ) : (
          <span style={{ color: "red", fontSize: "14px" }}>
            ATTENTION : Downgrade Setting is 3 months calculation.When select [
            From/To Year-Month ] ,System will calculate another Year-Month and
            reset.
          </span>
        )}
      </div>
    </div>
  );
};

//Status
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

//UserName
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
