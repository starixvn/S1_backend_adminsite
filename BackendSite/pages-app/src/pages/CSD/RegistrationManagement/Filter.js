import React from "react";
import moment from "moment-timezone";
import DateTimePicker from "react-datetime";

const defCurrencyType = {
  2: "MYR",
  4: "THB",
  13: "RMB",
  15: "IDR",
  51: "VND",
  61: "INR",
};
const defDepositStatusType = {
  1: "Non Deposit",
  2: "Deposited ",
};
const defStatusType = {
  1: "Pending ",
  2: "Done ",
  3: "Invalid ",
};
const defRegTimeType = {
  1: "Eariler ",
  2: "Later ",
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
          columnTitle="Currency"
          options={Object.keys(defCurrencyType).map((key) => {
            return { value: key, displayText: defCurrencyType[key] };
          })}
          hasAll={true}
          selectedValue={0}
          dataColumn="Currency"
          onChange={props.onChange}
        />
        <DropDownBox
          columnTitle="Deposit Status"
          options={Object.keys(defDepositStatusType).map((key) => {
            return { value: key, displayText: defDepositStatusType[key] };
          })}
          hasAll={true}
          selectedValue={0}
          dataColumn="DepositStatus"
          onChange={props.onChange}
        />
        <DropDownBox
          columnTitle="Status"
          options={Object.keys(defStatusType).map((key) => {
            return { value: key, displayText: defStatusType[key] };
          })}
          hasAll={true}
          selectedValue={0}
          dataColumn="Status"
          onChange={props.onChange}
        />
      </div>
      <div className="row-flex">
        <InputField
          columnTitle="User"
          defaultValue=""
          dataColumn="User"
          onChange={props.onChange}
        />
        <DropDownBox
          columnTitle="Sort User Reg Time"
          options={Object.keys(defRegTimeType).map((key) => {
            return { value: key, displayText: defRegTimeType[key] };
          })}
          hasAll={false}
          selectedValue={1}
          dataColumn="RegTimeType"
          onChange={props.onChange}
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
      <div className="row-flex">
        <DateTimePickerElem
          columnTitle="Export From Date"
          dateTimePickerProps={{
            value: props.exportFilter.StartDate,
            defaultValue: props.exportFilter.StartDate,
            timeFormat: false,
            onChange: (e) => {
              try {
                props.ExportOnChange(e, "StartDate");
              } catch (ex) {}
            },
          }}
        />
        <DateTimePickerElem
          columnTitle="Export To Date"
          dateTimePickerProps={{
            value: props.exportFilter.EndDate,
            defaultValue: props.exportFilter.EndDate,
            timeFormat: false,
            onChange: (e) => {
              try {
                props.ExportOnChange(e, "EndDate");
              } catch (ex) {}
            },
          }}
        />
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

const DateTimePickerElem = (props) => {
  return (
    <div className="input-item">
      <span className="label">{props.columnTitle}</span>
      <DateTimePicker {...props.dateTimePickerProps} />
    </div>
  );
};

export default Panel;
