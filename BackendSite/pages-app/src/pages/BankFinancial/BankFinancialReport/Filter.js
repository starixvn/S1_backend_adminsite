import React from "react";
import moment from "moment";
import DateTimePicker from "react-datetime";
import MultiSelect from "react-multi-select-component";
console.log("aa", process.env.NODE_ENV);
const defCurrencyType = {
  2: "MYR",
  4: "THB",
  13: "RMB",
  15: "IDR",
  51: "VND",
  61: "INR",
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
            Selected={props.filterInfo.SiteIds}
            OnChange={props.onChangeCheckBox}
            OnChangeAll={props.onChangeAllCheckBox}
            dataColumn="SiteIds"
          />
        ) : null}
        <CurrencyElem
          columnTitle="Currency"
          options={Object.keys(defCurrencyType).map((key) => {
            return { value: key, label: defCurrencyType[key] };
          })}
          dataColumn="CurrencyIds"
          onChange={props.onChange}
          Currencies={props.filterInfo.CurrencyIds}
        />
        <DateTimePickerElem
          columnTitle="From Date"
          dateTimePickerProps={{
            // moment.unix(props.filterInfo.StartDate).format('l') : moment().add(-10, 'days').format('l')
            defaultValue: moment(props.filterInfo.StartDate).format("l"),
            timeFormat: false,
            onBlur: (e) => {
              try {
                props.onChange(moment(e).format("l"), "StartDate");
              } catch (ex) {}
            },
          }}
        />
        <DateTimePickerElem
          columnTitle="To Date"
          dateTimePickerProps={{
            defaultValue: moment(props.filterInfo.EndDate).format("l"),
            timeFormat: false,
            onBlur: (e) => {
              try {
                props.onChange(moment(e).format("l"), "EndDate");
              } catch (ex) {}
            },
          }}
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

const CurrencyElem = ({ columnTitle, options, onChange, Currencies }) => {
  const OnSelect = (value) => {
    onChange(value, "CurrencyIds");
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

  return (
    <div className="input-item" style={{ width: "200px", zIndex: "4" }}>
      <span className="label">{columnTitle}</span>
      <div className="field">
        <MultiSelect
          disableSearch={"true"}
          options={options}
          value={Currencies}
          onChange={OnSelect}
          labelledBy={"Select"}
          ItemRenderer={itemRenderer}
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

export default Panel;
