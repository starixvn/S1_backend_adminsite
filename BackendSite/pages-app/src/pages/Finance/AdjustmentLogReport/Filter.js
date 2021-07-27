import React from "react";
import moment from "moment";
import DateTimePicker from "react-datetime";

const Panel = (props) => {
  return (
    <div className="filter">
      <div className="row-flex">
        {props.Menu !== null ? (
          <DropDownBox
            columnTitle="Menu"
            options={props.Menu.map((x, index) => {
              return { value: x.menu, displayText: x.menu };
            })}
            hasAll={true}
            selectedValue={props.stateFilter.Menu}
            dataColumn="Menu"
            onChange={props.onChange}
          />
        ) : null}
        <DateTimePickerElem
          columnTitle="From Date"
          dateTimePickerProps={{
            defaultValue: moment(props.stateFilter.StartDate).format("l"),
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
            defaultValue: moment(props.stateFilter.EndDate).format("l"),
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
          columnTitle="User"
          defaultValue=""
          dataColumn="User"
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

export default Panel;
