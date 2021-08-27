import React from "react";
import moment from "moment-timezone";
import DateTimePicker from "react-datetime";
import MultiSelect from "react-multi-select-component";

const defStatusType = {
  1: "Active",
  2: "Inactive",
};
const defDateType = {
  0: "",
  1: "Today",
  2: "Yesterday",
  3: "This Week",
  4: "Last Week",
  5: "This Month",
  6: "Last Month",
};
const Panel = (props) => {
  let siteList = props.SiteList.map((x) => {
    return {
      label: x.siteName,
      value: x.siteId,
    };
  });
  const SiteIdsChange = (x) => {
    props.onChange(x, "SelectSite");
  };
  return (
    <div className="filter">
      <div className="row-flex">
        <div className="input-item" style={{ width: "200px", zIndex: "999" }}>
          <span className="label">Site</span>
          <div className="field">
            <MultiSelect
              disableSearch={"true"}
              options={siteList}
              value={props.filterInfo.SelectSite}
              onChange={SiteIdsChange}
              labelledBy={"Select"}
              ItemRenderer={itemRenderer}
            />
          </div>
        </div>
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
        <DropDownBox2
          columnTitle="Date Quick Picker"
          options={Object.keys(defDateType).map((key) => {
            return { value: key, displayText: defDateType[key] };
          })}
          hasAll={false}
          selectedValue={0}
          value={props.filterInfo.DateQuickPicker}
          dataColumn="DateQuickPicker"
          onChange={props.datePickeronChange}
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
const itemRenderer = ({ checked, option, onClick }) => {
  return (
    <div>
      <input
        style={{ height: "15px", width: "20px" }}
        id={option.label}
        type="checkbox"
        onChange={onClick}
        checked={checked}
      />
      <label htmlFor={option.label}>{option.label}</label>
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
const DateTimePickerElem = (props) => {
  return (
    <div className="input-item">
      <span className="label">{props.columnTitle}</span>
      <DateTimePicker {...props.dateTimePickerProps} />
    </div>
  );
};
const DropDownBox2 = (props) => {
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
        <select value={props.value} onChange={onChange}>
          {props.hasAll ? <option value="0">All</option> : null}
          {optionRows}
        </select>
      </div>
    </div>
  );
};
export default Panel;
