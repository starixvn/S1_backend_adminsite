import React, { useState } from "react";
import moment from "moment";
import DateTimePicker from "react-datetime";

const filterdateformat = "YYYY-MM-DD";

const Panel = (props) => {

	let siteListOptions = [];
	if (props.SiteList) {	
		siteListOptions = Object.keys(props.SiteList)
		.map(siteId => {	
			return {
				value: Number(siteId),
				displayText: props.SiteList[siteId],
			};	
		})
		.filter(item => {
			return item.value != 0;
		});
  	}

  return (
    <div className="filter">
      <div className="row-flex">
        {siteListOptions.length > 0 ? (
        <CheckBoxElem
		  Title="Site"
		  Options={siteListOptions}
		  Selected={props.filterInfo.SelectSite}
		  OnChange={props.onChangeCheckBox}
		  OnChangeAll={props.onChangeAllCheckBox}
		  dataColumn="SelectSite"
		/>
        ) : null}

        <DateTimePickerElem
          columnTitle="From Date"
          dateTimePickerProps={{
            defaultValue: moment(props.filterInfo.StartDate).format(filterdateformat),
            timeFormat: false,
            onBlur: (e) => {
              try {
                props.onChange(moment(e).format(filterdateformat), "StartDate");
              } catch (ex) {}
            },
          }}
        />

        <DateTimePickerElem
          columnTitle="To Date"
          dateTimePickerProps={{
            defaultValue: moment(props.filterInfo.EndDate).format(filterdateformat),
            timeFormat: false,
            onBlur: (e) => {
              try {
                props.onChange(moment(e).format(filterdateformat), "EndDate");
              } catch (ex) {}
            },
          }}
        />
        <div className="button-group">
          <button
            className="btn btn-search"
            type="submit"
            onClick={props.ClickSearch}
          >
            Search
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
		  Title="ALL"
		  Options={Options}
		  checked={Selected.length === Options.length && Selected.length > 0}
		  Value="ALL"
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
