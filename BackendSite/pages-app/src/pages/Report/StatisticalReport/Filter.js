import React, { forwardRef } from "react";
import moment from "moment";
import CalendarIcon from "mdi-react/CalendarIcon";

if (process.env.NODE_ENV !== "production") {
  //     window.gCurrencyList = [2, 4, 13, 15, 51, 61];
  //     window.i18n = { "lbl_Currency2": "MYR",
  //                     "lbl_Currency4": "THB",
  //                     "lbl_Currency13": "RMB",
  //                     "lbl_Currency15": "IDR",
  //                     "lbl_Currency51": "VND",
  //                     "lbl_Currency61": "INR" };
}

const Panel = (props) => {
  let siteListOptions = "";
  if (props.SiteList) {
    siteListOptions = props.SiteList.map((i) => {
      return {
        value: Number(i.siteId),
        displayText: i.siteName,
      };
    });
  }

  let TimeZoneList = Object.keys(props.TimeZoneList).map((key) => {
    return {
      displayText: props.TimeZoneList[key],
      value: key,
    };
  });

  return (
    <div className="filter">
      <div className="row-flex">
        {siteListOptions ? (
          <CheckBoxElem
            Title="Site"
            Options={siteListOptions}
            Selected={props.filterInfo.SiteIds}
            OnChange={props.onChangeCheckBox}
            OnChangeAll={props.onChangeAllCheckBox}
            dataColumn="SiteIds"
          />
        ) : null}
        <DropDownBox
          columnTitle="TimeZone"
          options={TimeZoneList}
          hasAll={false}
          selectedValue={props.filterInfo.TimeZone}
          dataColumn="TimeZone"
          onChange={props.onChange}
        />
        <button
          className="btn btn-search"
          type="submit"
          onClick={props.SubmitBtnDisabled ? null : props.ClickSubmit}
          disabled={props.SubmitBtnDisabled}
        >
          Submit
        </button>
      </div>
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

export default Panel;
