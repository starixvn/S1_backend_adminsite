import moment from "moment";
//import MonthYearPicker from "react-month-year-picker";
import React, { useState } from "react";

const pickerLang = {
  months: [
    "Jan",
    "Feb",
    "Mar",
    "Apr",
    "May",
    "Jun",
    "Jul",
    "Aug",
    "Sep",
    "Oct",
    "Nov",
    "Dec",
  ],
  from: "From",
  to: "To",
};
const makeText = (m) => {
  if (m && m.year && m.month)
    return pickerLang.months[m.month - 1] + ". " + m.year;
  return "?";
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
  return (
    <div className="filter">
      <div className="row-flex">
        <div className="input-item">
          <div className="field">
            <label className="col-form-label d-block">Site Name</label>
            {radio}
          </div>
          <div className="field">
            <button
              className="btn btn-search"
              type="submit"
              onClick={props.Submit}
            >
              Submit
            </button>
          </div>
        </div>
        <div className="input-item">
          <div className="field">
            {/* <MonthYearPicker
              selectedMonth={props.StateFilter.month}
              selectedYear={props.StateFilter.year}
              minYear={2000}
              maxYear={2030}
              onChangeYear={(year) =>
                props.SetFilter({ ...props.StateFilter, year: year })
              }
              onChangeMonth={(month) =>
                props.SetFilter({ ...props.StateFilter, month: month })
              }
            /> */}
          </div>
        </div>
      </div>
    </div>
  );
};

export default Panel;
