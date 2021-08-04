import DateTimePicker from "react-datetime";
import moment from "moment-timezone";

const defDateType = {
  0: "",
  1: "Today",
  2: "Yesterday",
  3: "This Week",
  4: "Last Week",
  5: "This Month",
  6: "Last Month",
};

const Filter = (props) => {
  return (
    <form className="filter" style={{ display: "block" }}>
      <div className="row-flex">
        <DateTimePickerElem
          columnTitle="StartTime"
          dateTimePickerProps={{
            value: props.FilterData.StartTime,
            defaultValue: props.FilterData.StartTime,
            onChange: (e) => {
              try {
                props.EditfilterData(moment(e).format("l LTS"), "StartTime");
              } catch (ex) {
                console.log(ex);
              }
            },
          }}
        />
        <DateTimePickerElem
          columnTitle="EndTime"
          dateTimePickerProps={{
            value: props.FilterData.EndTime,
            defaultValue: props.FilterData.EndTime,
            onChange: (e) => {
              try {
                props.EditfilterData(moment(e).format("l LTS"), "EndTime");
              } catch (ex) {
                console.log(ex);
              }
            },
          }}
        />
        <DropDownBox
          columnTitle="Date Quick Picker"
          options={Object.keys(defDateType).map((key) => {
            return { value: key, displayText: defDateType[key] };
          })}
          hasAll={false}
          value={props.FilterData.DateQuickPicker}
          dataColumn="DateQuickPicker"
          onChange={(e) => props.EditDate(e, "DateQuickPicker")}
        />
        <div className="button-group">
          <button
            className="btn btn-search"
            type="button"
            onClick={props.ClickSubmit}
          >
            Submit
          </button>
        </div>
      </div>
    </form>
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
        <select value={props.value} onChange={onChange}>
          {props.hasAll ? <option value="0">All</option> : null}
          {optionRows}
        </select>
      </div>
    </div>
  );
};
export default Filter;
