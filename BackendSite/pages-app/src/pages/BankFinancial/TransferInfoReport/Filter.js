import React from "react";
import moment from "moment";
import DateTimePicker from "react-datetime";

const Panel = (props) => {
  let siteListOptions = [];
  let accountListOptions = [];

  siteListOptions.push({
    displayText: "-- Select Site --",
    value: 0,
  });

  if (props.SiteList.length != 0) {
    props.SiteList.forEach((element) => {
      siteListOptions.push({
        displayText: element.siteName,
        value: element.siteId,
      });
    });
  }
  accountListOptions.push({
    displayText: "-- Select Account --",
    value: "",
  });
  if (props.AccountList.length != 0) {
    props.AccountList.forEach((s) => {
      accountListOptions.push({
        displayText: s.displayName,
        value: s.accountNo,
      });
    });
  }

  return (
    <div className="filter">
      <div className="row-flex">
        <DropDownBox
          columnTitle="Site"
          options={siteListOptions}
          hasAll={false}
          selectedValue={props.filterInfo.SiteId}
          dataColumn="SiteId"
          onChange={(e) => props.siteIdOnChange(e, "SiteId")}
        />
        <DropDownBox
          columnTitle="Account"
          options={accountListOptions}
          hasAll={false}
          selectedValue={props.filterInfo.AccountNo}
          dataColumn="AccountNo"
          onChange={(e) => props.onChange(e, "AccountNo")}
        />
        <DateTimePickerElem
          columnTitle="Start Date"
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
          columnTitle="End Date"
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

const DropDownBox = (props) => {
  const onChange = (e) => {
    if (props.onChange) {
      props.onChange(e.target.value, props.dataColumn);
    }
  };
  let optionRows = props.options.map((item) => {
    return (
      <option key={`${props.columnTitle}-${item.value}`} value={item.value}>
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
