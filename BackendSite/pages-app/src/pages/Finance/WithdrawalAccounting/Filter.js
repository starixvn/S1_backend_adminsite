import moment from "moment";
import React from "react";
import DateTimePicker from "react-datetime";
import MultiSelect from "react-multi-select-component";

const defCurrencyType = {
  2: "MYR",
  3: "USD",
  4: "THB",
  13: "CNY",
  15: "IDR",
  51: "VND",
  61: "INR",
  74: "BDT",
};

const Panel = (props) => {
  let siteList = props.SiteList.map((x) => {
    return {
      label: x.siteName,
      value: x.siteId,
    };
  });
  let currencyList = Object.keys(defCurrencyType).map((key) => {
    return {
      label: defCurrencyType[key],
      value: key,
    };
  });

  const SiteIdsChange = (x) => {
    props.SetFilter({
      ...props.filterInfo,
      SiteIds: x,
    });
  };
  const CurrencyChange = (x) => {
    props.SetFilter({
      ...props.filterInfo,
      CurrencyIds: x,
    });
  };

  return (
    <div className="filter">
      <div className="row-flex">
        <div className="input-item" style={{ width: "200px", zIndex: "1" }}>
          <span className="label">Site</span>
          <div className="field">
            <MultiSelect
              disableSearch={"true"}
              options={siteList}
              value={props.filterInfo.SiteIds}
              onChange={SiteIdsChange}
              labelledBy={"Select"}
              ItemRenderer={itemRenderer}
            />
          </div>
        </div>
        <div className="input-item" style={{ width: "200px", zIndex: "1" }}>
          <span className="label">Currency</span>
          <div className="field">
            <MultiSelect
              disableSearch={"true"}
              options={currencyList}
              value={props.filterInfo.CurrencyIds}
              onChange={CurrencyChange}
              labelledBy={"Select"}
              ItemRenderer={itemRenderer}
            />
          </div>
        </div>
        <DateTimePickerElem
          columnTitle="From Date"
          dateTimePickerProps={{
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
      </div>
      <div className="row-flex">
        <InputField
          columnTitle="Trans Id"
          defaultValue=""
          dataColumn="TransId"
          onChange={props.onChange}
        />
        <InputField
          columnTitle="User Name"
          defaultValue=""
          dataColumn="UserName"
          onChange={props.onChange}
        />
        <div className="button-group">
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

export default Panel;
