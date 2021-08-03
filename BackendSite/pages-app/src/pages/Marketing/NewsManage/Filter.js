import React from "react";
import moment from "moment-timezone";
import DateTimePicker from "react-datetime";

const defStatusType = {
  1: "Active",
  2: "Inactive",
};

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
  return (
    <div className="filter">
      <div className="row-flex">
        {siteListOptions ? (
          <CheckBoxElem
            Title="Site"
            Options={siteListOptions}
            Selected={props.filterInfo.SelectSite}
            OnChange={props.onChangeCheckBox}
            OnChangeAll={props.onChangeAllCheckBox}
            dataColumn="SelectSite"
          />
        ) : null}
        <DropDownBox
          columnTitle="Status"
          options={Object.keys(defStatusType).map((key) => {
            return { value: key, displayText: defStatusType[key] };
          })}
          hasAll={true}
          selectedValue={props.filterInfo.Status}
          dataColumn="Status"
          onChange={props.onChange}
        />
        <DateTimePickerElem
          columnTitle="From Date"
          dateTimePickerProps={{
            value: props.filterInfo.StartDate,
            defaultValue: props.filterInfo.StartDate,
            timeFormat: false,
            onChange: (e) => {
              try {
                props.onChange(e, "StartDate");
              } catch (ex) {}
            },
          }}
        />
        <DateTimePickerElem
          columnTitle="To Date"
          dateTimePickerProps={{
            value: props.filterInfo.EndDate,
            defaultValue: props.filterInfo.EndDate,
            timeFormat: false,
            onChange: (e) => {
              try {
                props.onChange(e, "EndDate");
              } catch (ex) {}
            },
          }}
        />
        <div className="button-group">
          <button
            className="btn btn-search"
            type="submit"
            onClick={props.ClickSubmit}
          >
            Submit
          </button>
        </div>
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
const DateTimePickerElem = (props) => {
  return (
    <div className="input-item">
      <span className="label">{props.columnTitle}</span>
      <DateTimePicker {...props.dateTimePickerProps} />
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
export default Panel;
